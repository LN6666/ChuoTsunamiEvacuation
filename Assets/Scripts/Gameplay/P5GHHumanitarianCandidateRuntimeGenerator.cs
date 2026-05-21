using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class P5GHHumanitarianCandidateRuntimeGenerator : MonoBehaviour
{
    private const string RuntimeRootName = "P5GH_HumanitarianCandidates_Runtime";
    private const string ControllerObjectName = "P5GH_HumanitarianCandidate_Controller";
    private const string GameplayRootName = "GameplayTestRoot";
    private const string TestGroundName = "TestGround";
    private static readonly Vector3 DefaultDebugOrigin = new Vector3(-80f, 80f, -250f);

    private static readonly BindingFlags PrivateInstanceFlags =
        BindingFlags.Instance | BindingFlags.NonPublic;

    [SerializeField] private Vector3 displayMarkerSize = new Vector3(2.4f, 0.42f, 2.4f);
    [SerializeField] private Vector3 selectableMarkerSize = new Vector3(3.1f, 0.55f, 3.1f);
    [SerializeField] private Vector3 entranceSize = new Vector3(2f, 2f, 2f);
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 3.4f, 0f);
    [SerializeField] private KeyCode metadataToggleKey = KeyCode.N;
    [SerializeField] private bool showDetailedMetadata;

    private Material displayOnlyMaterial;
    private Material selectableMaterial;
    private Material reviewMaterial;
    private Material entranceMaterial;
    private GameObject runtimeRoot;
    private readonly List<GeneratedCandidateLabel> generatedLabels = new List<GeneratedCandidateLabel>();

    public int LastGeneratedMarkerCount { get; private set; }
    public int LastGeneratedDisplayOnlyCount { get; private set; }
    public int LastGeneratedSelectableCount { get; private set; }
    public static bool AffectsGameplayRules => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapHumanitarianCandidates()
    {
        if (!IsDebugContext())
        {
            return;
        }

        if (GameObject.Find(ControllerObjectName) != null || GameObject.Find(RuntimeRootName) != null)
        {
            return;
        }

        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.Load();
        if (config == null || !config.enableHumanitarianCandidates)
        {
            return;
        }

        EvacuationGameManager gameManager = UnityEngine.Object.FindObjectOfType<EvacuationGameManager>();
        if (gameManager == null)
        {
            return;
        }

        GameObject generatorObject = new GameObject(ControllerObjectName);
        P5GHHumanitarianCandidateRuntimeGenerator generator =
            generatorObject.AddComponent<P5GHHumanitarianCandidateRuntimeGenerator>();
        generator.GenerateFromConfig(config, gameManager, UnityEngine.Object.FindObjectOfType<GameUIManager>());
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
            $"P5-GH humanitarian candidate metadata switched to {(showDetailedMetadata ? "detailed" : "compact")} mode.");
    }

    public int GenerateFromConfig(
        ShelterSourceConfigLoader.ShelterSourceConfig config,
        EvacuationGameManager gameManager,
        GameUIManager gameUIManager)
    {
        if (config == null || !config.enableHumanitarianCandidates)
        {
            ResetCounters();
            return 0;
        }

        HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult loadResult =
            HumanitarianCandidateDataLoader.LoadFromAssetsData();
        return Generate(loadResult, config.enableLifeFirstCandidateSelection, gameManager, gameUIManager);
    }

    public int Generate(
        HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult loadResult,
        bool enableLifeFirstCandidateSelection,
        EvacuationGameManager gameManager,
        GameUIManager gameUIManager)
    {
        ResetCounters();

        if (loadResult == null || !loadResult.success || loadResult.records == null || loadResult.records.Length == 0)
        {
            Debug.LogWarning("P5-GH humanitarian candidate layer did not generate because candidate data was unavailable.");
            return 0;
        }

        InitializeMaterials();
        Transform parent = ResolveRuntimeParent();
        Vector3 debugOrigin = ResolveDebugOrigin();

        runtimeRoot = new GameObject(RuntimeRootName);
        runtimeRoot.transform.SetParent(parent, true);

        for (int i = 0; i < loadResult.records.Length; i++)
        {
            HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record = loadResult.records[i];
            if (record == null || !record.isDisplayable)
            {
                continue;
            }

            bool selectable = enableLifeFirstCandidateSelection && record.isSelectableInLifeFirstMode;
            CreateCandidateMarker(record, i, selectable, runtimeRoot.transform, debugOrigin, gameManager, gameUIManager);
            LastGeneratedMarkerCount++;
            if (selectable)
            {
                LastGeneratedSelectableCount++;
            }
            else
            {
                LastGeneratedDisplayOnlyCount++;
            }
        }

        Debug.Log(
            $"Displayed {LastGeneratedMarkerCount} P5-GH humanitarian candidate marker(s) from Assets/Data. " +
            $"Selectable life-first proxies: {LastGeneratedSelectableCount}. " +
            "Candidates are non-official and do not change success/failure rules.");
        return LastGeneratedMarkerCount;
    }

    private void CreateCandidateMarker(
        HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record,
        int index,
        bool selectable,
        Transform parent,
        Vector3 debugOrigin,
        EvacuationGameManager gameManager,
        GameUIManager gameUIManager)
    {
        Vector3 localPosition = CreateSchematicPosition(index);
        Vector3 worldPosition = debugOrigin + localPosition;
        string safeId = MakeSafeObjectName(record.candidateId);

        GameObject candidateObject = new GameObject($"P5GH_HumanitarianCandidate_{safeId}");
        candidateObject.transform.SetParent(parent, true);
        candidateObject.transform.position = worldPosition;

        HumanitarianCandidateMetadata metadata = candidateObject.AddComponent<HumanitarianCandidateMetadata>();
        metadata.ApplyRecord(record);

        if (selectable)
        {
            BuildingShelter shelter = candidateObject.AddComponent<BuildingShelter>();
            shelter.ApplyShelterData(MapToLifeFirstShelterData(record, localPosition));
        }

        GameObject markerObject = CreatePrimitive(
            selectable ? $"P5GH_LifeFirstCandidateMarker_{safeId}" : $"P5GH_DisplayCandidateMarker_{safeId}",
            PrimitiveType.Cylinder,
            candidateObject.transform,
            worldPosition + new Vector3(0f, 0.25f, 0f),
            selectable ? selectableMarkerSize : displayMarkerSize);
        SetMaterial(markerObject, ResolveMarkerMaterial(record, selectable));
        RemoveCollider(markerObject);

        if (selectable)
        {
            BuildingShelter shelter = candidateObject.GetComponent<BuildingShelter>();
            GameObject entranceObject = CreatePrimitive(
                $"P5GH_LifeFirstCandidateEntrance_{safeId}",
                PrimitiveType.Cube,
                candidateObject.transform,
                worldPosition + new Vector3(0f, 1f, 0f),
                entranceSize);
            SetMaterial(entranceObject, entranceMaterial);
            SetColliderTrigger(entranceObject, true);

            ShelterEntranceTrigger entrance = entranceObject.AddComponent<ShelterEntranceTrigger>();
            ConfigureEntrance(entrance, shelter, gameManager, gameUIManager);
        }

        CreateLabel($"P5GH_HumanitarianCandidateLabel_{safeId}", candidateObject.transform, worldPosition + labelOffset, metadata);
    }

    private static ShelterDataLoader.ShelterData MapToLifeFirstShelterData(
        HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record,
        Vector3 localPosition)
    {
        int capacity = Mathf.Max(0, record.estimatedCapacityProxy);
        return new ShelterDataLoader.ShelterData
        {
            shelterId = record.candidateId,
            shelterName = string.IsNullOrWhiteSpace(record.buildingName)
                ? record.candidateId
                : record.buildingName,
            shelterRank = "Life-first",
            isOfficialShelter = false,
            canEnter = true,
            entryDelaySeconds = 0f,
            climbTimeSeconds = 10f,
            crowdingDelaySeconds = 0f,
            failureReason = "This humanitarian candidate is not available in the current life-first scenario.",
            sourceType = HumanitarianCandidateDataLoader.SourceType,
            facilityType = "humanitarian_emergency_candidate_highrise",
            layoutPosition = new ShelterDataLoader.LayoutPosition
            {
                x = localPosition.x,
                y = localPosition.y,
                z = localPosition.z
            },
            realFacilityName = record.buildingName,
            address = record.address,
            latitude = record.latitude >= 0f ? record.latitude : 0f,
            longitude = record.longitude >= 0f ? record.longitude : 0f,
            coordinateSystem = "prototype_debug_layout_not_geospatial",
            plateauBuildingId = record.plateauBuildingId,
            safeFloor = Mathf.Max(0, record.floorsAboveGround),
            capacity = capacity,
            dataSource = "P5-GH Assets/Data controlled humanitarian candidate sample",
            sourceUrl = FirstSourceUrl(record.sourceRefs),
            sourceUpdatedAt = FirstSourceUpdatedAt(record.sourceRefs),
            notes = HumanitarianCandidateDataLoader.ControlledSampleNotice
        };
    }

    private Material ResolveMarkerMaterial(
        HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record,
        bool selectable)
    {
        if (record != null && record.manualReviewNeeded)
        {
            return reviewMaterial;
        }

        return selectable ? selectableMaterial : displayOnlyMaterial;
    }

    private Vector3 CreateSchematicPosition(int index)
    {
        int safeIndex = Mathf.Max(0, index);
        int column = safeIndex % 3;
        int row = safeIndex / 3;
        return new Vector3(column * 7f, 0f, row * 8f);
    }

    private void CreateLabel(
        string name,
        Transform parent,
        Vector3 position,
        HumanitarianCandidateMetadata metadata)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, true);
        labelObject.transform.position = position;
        labelObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = FormatLabel(metadata);
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.23f;
        textMesh.fontSize = 21;
        textMesh.color = Color.white;

        generatedLabels.Add(new GeneratedCandidateLabel
        {
            Label = textMesh,
            Metadata = metadata
        });
    }

    private void RefreshGeneratedLabels()
    {
        foreach (GeneratedCandidateLabel label in generatedLabels)
        {
            if (label == null || label.Label == null)
            {
                continue;
            }

            label.Label.text = FormatLabel(label.Metadata);
        }
    }

    private string FormatLabel(HumanitarianCandidateMetadata metadata)
    {
        if (metadata == null)
        {
            return string.Empty;
        }

        return showDetailedMetadata ? metadata.BuildDetailedLabel() : metadata.BuildCompactLabel();
    }

    private void InitializeMaterials()
    {
        displayOnlyMaterial = CreateMaterial("P5-GH Humanitarian Display Marker Material", new Color(0.82f, 0.32f, 0.9f, 0.82f), true);
        selectableMaterial = CreateMaterial("P5-GH Life First Selectable Material", new Color(0.92f, 0.22f, 0.48f, 0.92f), false);
        reviewMaterial = CreateMaterial("P5-GH Manual Review Candidate Material", new Color(1f, 0.72f, 0.2f, 0.9f), true);
        entranceMaterial = CreateMaterial("P5-GH Candidate Entrance Material", new Color(1f, 0.16f, 0.42f, 0.72f), true);
    }

    private void ResetCounters()
    {
        LastGeneratedMarkerCount = 0;
        LastGeneratedDisplayOnlyCount = 0;
        LastGeneratedSelectableCount = 0;
        generatedLabels.Clear();
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
        return testGround != null ? testGround.transform.position + new Vector3(-80f, 0f, 0f) : DefaultDebugOrigin;
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

    private static void SetColliderTrigger(GameObject gameObject, bool isTrigger)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = isTrigger;
        }
    }

    private static void RemoveCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(collider);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
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

    private static string FirstSourceUrl(HumanitarianCandidateDataLoader.SourceRefRecord[] sourceRefs)
    {
        if (sourceRefs == null)
        {
            return string.Empty;
        }

        foreach (HumanitarianCandidateDataLoader.SourceRefRecord sourceRef in sourceRefs)
        {
            if (sourceRef != null && !string.IsNullOrWhiteSpace(sourceRef.sourceUrl))
            {
                return sourceRef.sourceUrl.Trim();
            }
        }

        return string.Empty;
    }

    private static string FirstSourceUpdatedAt(HumanitarianCandidateDataLoader.SourceRefRecord[] sourceRefs)
    {
        if (sourceRefs == null)
        {
            return string.Empty;
        }

        foreach (HumanitarianCandidateDataLoader.SourceRefRecord sourceRef in sourceRefs)
        {
            if (sourceRef != null && !string.IsNullOrWhiteSpace(sourceRef.sourceUpdatedAt))
            {
                return sourceRef.sourceUpdatedAt.Trim();
            }
        }

        return string.Empty;
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

    private class GeneratedCandidateLabel
    {
        public TextMesh Label;
        public HumanitarianCandidateMetadata Metadata;
    }
}
