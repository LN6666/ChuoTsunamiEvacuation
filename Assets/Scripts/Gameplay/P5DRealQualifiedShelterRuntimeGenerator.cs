using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class P5DRealQualifiedShelterRuntimeGenerator : MonoBehaviour
{
    private const string RuntimeRootName = "P5D_RealQualifiedShelters_Runtime";
    private const string ControllerObjectName = "P5D_RealQualifiedShelter_Controller";
    private const string GameplayRootName = "GameplayTestRoot";
    private const string TestGroundName = "TestGround";
    private const int DefaultRoutePreviewLimit = 1;
    private static readonly Vector3 DefaultDebugOrigin = new Vector3(0f, 80f, -250f);

    private static readonly BindingFlags PrivateInstanceFlags =
        BindingFlags.Instance | BindingFlags.NonPublic;

    [SerializeField] private Vector3 entranceSize = new Vector3(2f, 2f, 2f);
    [SerializeField] private Vector3 markerSize = new Vector3(3f, 0.5f, 3f);
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 3.8f, 0f);
    [SerializeField] private KeyCode metadataToggleKey = KeyCode.M;
    [SerializeField] private bool showDetailedMetadata;
    [SerializeField] private bool enableSelectedRoutePreview = true;
    [SerializeField] private int routePreviewLimit = DefaultRoutePreviewLimit;

    private Material shelterMaterial;
    private Material manualReviewMaterial;
    private Material entranceMaterial;
    private Material routePreviewMaterial;
    private GameObject runtimeRoot;
    private GameObject routePreviewRoot;
    private readonly List<GeneratedShelterLabel> generatedLabels = new List<GeneratedShelterLabel>();

    public int LastGeneratedRouteLineCount { get; private set; }
    public string LastRoutePreviewValidationReason { get; private set; } = string.Empty;
    public static bool AffectsGameplayRules => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapRealQualifiedShelters()
    {
        if (!IsDebugContext())
        {
            return;
        }

        if (GameObject.Find(ControllerObjectName) != null || GameObject.Find(RuntimeRootName) != null)
        {
            return;
        }

        EvacuationGameManager gameManager = UnityEngine.Object.FindObjectOfType<EvacuationGameManager>();
        if (gameManager == null)
        {
            return;
        }

        ShelterDataSourceResolver.ShelterDataSourceResult sourceResult =
            ShelterDataSourceResolver.LoadConfiguredSource();

        if (!sourceResult.IsRealQualified || !sourceResult.success || sourceResult.realQualifiedShelters.Length == 0)
        {
            return;
        }

        GameObject generatorObject = new GameObject(ControllerObjectName);
        P5DRealQualifiedShelterRuntimeGenerator generator =
            generatorObject.AddComponent<P5DRealQualifiedShelterRuntimeGenerator>();
        generator.Generate(sourceResult, gameManager, UnityEngine.Object.FindObjectOfType<GameUIManager>());
    }

    private void Update()
    {
        if (Input.GetKeyDown(metadataToggleKey))
        {
            ToggleDetailedMetadata();
        }
    }

    public void ToggleDetailedMetadata()
    {
        showDetailedMetadata = !showDetailedMetadata;
        RefreshGeneratedLabels();
        Debug.Log(
            $"P5-D real_qualified shelter metadata switched to {(showDetailedMetadata ? "detailed" : "compact")} mode.");
    }

    public int Generate(
        ShelterDataSourceResolver.ShelterDataSourceResult sourceResult,
        EvacuationGameManager gameManager,
        GameUIManager gameUIManager)
    {
        LastGeneratedRouteLineCount = 0;
        LastRoutePreviewValidationReason = string.Empty;
        generatedLabels.Clear();

        if (sourceResult == null || !sourceResult.IsRealQualified || sourceResult.realQualifiedShelters == null)
        {
            return 0;
        }

        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord[] selectableRecords =
            RealQualifiedShelterDataLoader.GetSelectableRecords(sourceResult.realQualifiedShelters);
        ShelterDataLoader.ShelterData[] gameplayShelters =
            ShelterGameplayDataMapper.MapRealQualifiedSheltersToGameplayData(selectableRecords);

        if (gameplayShelters.Length == 0)
        {
            Debug.LogWarning("P5-D real_qualified mode had no selectable records to generate.");
            return 0;
        }

        ShelterDataLoader.ClearRuntimeShelters();
        ShelterDataLoader.RegisterRuntimeShelters(gameplayShelters, "P5-D real_qualified runtime generator");

        DisableExistingTestShelters();
        InitializeMaterials();

        Transform parent = ResolveRuntimeParent();
        Vector3 debugOrigin = ResolveDebugOrigin();

        runtimeRoot = new GameObject(RuntimeRootName);
        runtimeRoot.transform.SetParent(parent, true);

        routePreviewRoot = new GameObject("P5D_SelectedEstimatedRoutePreview_Runtime");
        routePreviewRoot.transform.SetParent(runtimeRoot.transform, true);

        int generatedCount = 0;
        for (int i = 0; i < gameplayShelters.Length && i < selectableRecords.Length; i++)
        {
            if (CreateShelterMarker(selectableRecords[i], gameplayShelters[i], i, runtimeRoot.transform, debugOrigin, gameManager, gameUIManager))
            {
                generatedCount++;
            }
        }

        GenerateSelectedRoutePreview(selectableRecords, gameplayShelters, debugOrigin);
        Debug.Log(
            $"Generated {generatedCount} P5-D real_qualified playable shelter target(s) from Assets/Data. " +
            "Runtime proxies use a prototype/debug layout and do not save scene changes.");
        return generatedCount;
    }

    private bool CreateShelterMarker(
        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record,
        ShelterDataLoader.ShelterData shelterData,
        int index,
        Transform parent,
        Vector3 debugOrigin,
        EvacuationGameManager gameManager,
        GameUIManager gameUIManager)
    {
        if (record == null || shelterData == null)
        {
            return false;
        }

        string safeId = MakeSafeObjectName(shelterData.shelterId);
        Vector3 localPosition = shelterData.layoutPosition != null
            ? shelterData.layoutPosition.ToVector3()
            : ShelterGameplayDataMapper.CreateDeterministicFallbackLayout(index).ToVector3();
        Vector3 worldPosition = debugOrigin + localPosition;

        GameObject shelterObject = new GameObject($"P5D_RealQualifiedShelter_{safeId}");
        shelterObject.transform.SetParent(parent, true);
        shelterObject.transform.position = worldPosition;

        BuildingShelter shelter = shelterObject.AddComponent<BuildingShelter>();
        shelter.ApplyShelterData(shelterData);

        RealQualifiedShelterMetadata metadata = shelterObject.AddComponent<RealQualifiedShelterMetadata>();
        metadata.ApplyRecord(record);

        GameObject markerObject = CreatePrimitive(
            $"P5D_RealQualifiedMarker_{safeId}",
            PrimitiveType.Cube,
            shelterObject.transform,
            worldPosition + new Vector3(0f, 0.25f, 0f),
            markerSize);
        SetMaterial(markerObject, record.manualReviewNeeded ? manualReviewMaterial : shelterMaterial);
        RemoveCollider(markerObject);

        GameObject entranceObject = CreatePrimitive(
            $"P5D_RealQualifiedEntrance_{safeId}",
            PrimitiveType.Cube,
            shelterObject.transform,
            worldPosition + new Vector3(0f, 1f, 0f),
            entranceSize);
        SetMaterial(entranceObject, entranceMaterial);
        SetColliderTrigger(entranceObject, true);

        ShelterEntranceTrigger entrance = entranceObject.AddComponent<ShelterEntranceTrigger>();
        ConfigureEntrance(entrance, shelter, gameManager, gameUIManager);
        CreateLabel($"P5D_RealQualifiedLabel_{safeId}", shelterObject.transform, worldPosition + labelOffset, metadata);
        return true;
    }

    private void GenerateSelectedRoutePreview(
        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord[] selectableRecords,
        ShelterDataLoader.ShelterData[] gameplayShelters,
        Vector3 debugOrigin)
    {
        LastGeneratedRouteLineCount = 0;

        if (!enableSelectedRoutePreview || routePreviewRoot == null || selectableRecords == null || gameplayShelters == null)
        {
            return;
        }

        int maxCount = Mathf.Min(Mathf.Max(0, routePreviewLimit), selectableRecords.Length, gameplayShelters.Length);
        for (int i = 0; i < maxCount; i++)
        {
            RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record = selectableRecords[i];
            ShelterDataLoader.ShelterData shelterData = gameplayShelters[i];
            if (record == null || shelterData == null || record.routeSample == null || shelterData.layoutPosition == null)
            {
                continue;
            }

            Vector3 targetLocal = shelterData.layoutPosition.ToVector3();
            P5DRoutePreviewTransformValidator.ValidationResult validation =
                P5DRoutePreviewTransformValidator.ValidateForSelectedRoutePreview(
                    record.routeSample,
                    Vector3.zero,
                    targetLocal);
            LastRoutePreviewValidationReason = validation.reason;

            if (!validation.canRender)
            {
                continue;
            }

            CreateRouteLine(record, validation, debugOrigin, routePreviewRoot.transform);
            LastGeneratedRouteLineCount++;
        }
    }

    private void CreateRouteLine(
        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record,
        P5DRoutePreviewTransformValidator.ValidationResult validation,
        Vector3 debugOrigin,
        Transform parent)
    {
        GameObject lineObject = new GameObject($"P5D_EstimatedPrototypeRoute_{MakeSafeObjectName(record.routeTargetId)}");
        lineObject.transform.SetParent(parent, true);

        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.sharedMaterial = routePreviewMaterial;
        lineRenderer.widthMultiplier = 0.2f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = validation.localPositions.Length;

        for (int i = 0; i < validation.localPositions.Length; i++)
        {
            lineRenderer.SetPosition(i, debugOrigin + validation.localPositions[i]);
        }

        P5DRoutePreviewMetadata metadata = lineObject.AddComponent<P5DRoutePreviewMetadata>();
        metadata.Apply(record.routeSample, record.gameplayShelterId, record.attribution, validation);
    }

    private static void DisableExistingTestShelters()
    {
        BuildingShelter[] existingShelters = UnityEngine.Object.FindObjectsOfType<BuildingShelter>();

        foreach (BuildingShelter existingShelter in existingShelters)
        {
            if (existingShelter == null ||
                string.Equals(existingShelter.SourceType, RealQualifiedShelterDataLoader.SourceType, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.Equals(existingShelter.SourceType, ShelterSourceConfigLoader.TestSourceMode, StringComparison.OrdinalIgnoreCase) ||
                existingShelter.ShelterId.StartsWith("test_", StringComparison.OrdinalIgnoreCase))
            {
                existingShelter.gameObject.SetActive(false);
            }
        }
    }

    private static void ConfigureEntrance(
        ShelterEntranceTrigger entrance,
        BuildingShelter shelter,
        EvacuationGameManager gameManager,
        GameUIManager gameUIManager)
    {
        SetPrivateField(entrance, "shelter", shelter);
        SetPrivateField(entrance, "gameManager", gameManager);
        SetPrivateField(entrance, "gameUIManager", gameUIManager);
        SetPrivateField(entrance, "playerTag", "Player");
        SetPrivateField(entrance, "interactKey", KeyCode.E);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null)
        {
            return;
        }

        FieldInfo field = target.GetType().GetField(fieldName, PrivateInstanceFlags);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    private static Transform ResolveRuntimeParent()
    {
        GameObject gameplayRoot = GameObject.Find(GameplayRootName);
        return gameplayRoot != null ? gameplayRoot.transform : null;
    }

    private static Vector3 ResolveDebugOrigin()
    {
        GameObject testGround = GameObject.Find(TestGroundName);
        return testGround != null ? testGround.transform.position : DefaultDebugOrigin;
    }

    private void InitializeMaterials()
    {
        shelterMaterial = CreateMaterial("P5-D Real Qualified Shelter Material", new Color(0.05f, 0.72f, 0.55f, 0.92f), false);
        manualReviewMaterial = CreateMaterial("P5-D Manual Review Shelter Material", new Color(1f, 0.72f, 0.18f, 0.92f), false);
        entranceMaterial = CreateMaterial("P5-D Real Qualified Entrance Material", new Color(0.1f, 1f, 0.25f, 0.75f), true);
        routePreviewMaterial = CreateMaterial("P5-D Estimated Prototype Route Material", new Color(0.12f, 0.85f, 1f, 0.82f), true);
    }

    private static GameObject CreatePrimitive(
        string name,
        PrimitiveType primitiveType,
        Transform parent,
        Vector3 position,
        Vector3 scale)
    {
        GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
        gameObject.name = name;
        gameObject.transform.SetParent(parent, true);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        return gameObject;
    }

    private void CreateLabel(
        string name,
        Transform parent,
        Vector3 position,
        RealQualifiedShelterMetadata metadata)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, true);
        labelObject.transform.position = position;
        labelObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = FormatLabel(metadata);
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.24f;
        textMesh.fontSize = 22;
        textMesh.color = Color.white;

        generatedLabels.Add(new GeneratedShelterLabel
        {
            Label = textMesh,
            Metadata = metadata
        });
    }

    private void RefreshGeneratedLabels()
    {
        foreach (GeneratedShelterLabel generatedLabel in generatedLabels)
        {
            if (generatedLabel == null || generatedLabel.Label == null)
            {
                continue;
            }

            generatedLabel.Label.text = FormatLabel(generatedLabel.Metadata);
        }
    }

    private string FormatLabel(RealQualifiedShelterMetadata metadata)
    {
        if (metadata == null)
        {
            return string.Empty;
        }

        return showDetailedMetadata ? metadata.BuildDetailedLabel() : metadata.BuildCompactLabel();
    }

    private static void SetColliderTrigger(GameObject gameObject, bool isTrigger)
    {
        Collider triggerCollider = gameObject.GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = isTrigger;
        }
    }

    private static void RemoveCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.Destroy(collider);
        }
    }

    private static void SetMaterial(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static Material CreateMaterial(string name, Color color, bool transparent)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader)
        {
            name = name,
            color = color
        };

        if (transparent)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        return material;
    }

    private static string MakeSafeObjectName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unnamed";
        }

        return value.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
    }

    private static bool IsDebugContext()
    {
        return Application.isEditor || Debug.isDebugBuild;
    }

    private class GeneratedShelterLabel
    {
        public TextMesh Label;
        public RealQualifiedShelterMetadata Metadata;
    }
}
