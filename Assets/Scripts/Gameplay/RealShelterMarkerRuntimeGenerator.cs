using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class RealShelterMarkerRuntimeGenerator : MonoBehaviour
{
    private const string RuntimeRootName = "P4B_RealShelterMarkers_Runtime";
    private const string GameplayRootName = "GameplayTestRoot";
    private const string TestGroundName = "TestGround";
    private static readonly Vector3 DefaultDebugOrigin = new Vector3(0f, 80f, -250f);

    private static readonly BindingFlags PrivateInstanceFlags =
        BindingFlags.Instance | BindingFlags.NonPublic;

    [SerializeField] private Vector3 entranceSize = new Vector3(2f, 2f, 2f);
    [SerializeField] private Vector3 markerSize = new Vector3(3f, 0.5f, 3f);
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 3.8f, 0f);
    [SerializeField] private KeyCode metadataToggleKey = KeyCode.M;
    [SerializeField] private bool showDetailedMetadata;

    private Material enterableMaterial;
    private Material blockedMaterial;
    private Material entranceMaterial;
    private readonly List<GeneratedShelterLabel> generatedLabels = new List<GeneratedShelterLabel>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapRealShelterMarkers()
    {
        if (!IsDebugContext())
        {
            return;
        }

        if (GameObject.Find(RuntimeRootName) != null)
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

        if (!sourceResult.IsRealSample || !sourceResult.success || sourceResult.realShelters.Length == 0)
        {
            return;
        }

        GameObject generatorObject = new GameObject(RuntimeRootName);
        RealShelterMarkerRuntimeGenerator generator =
            generatorObject.AddComponent<RealShelterMarkerRuntimeGenerator>();
        generator.Generate(sourceResult, gameManager, UnityEngine.Object.FindObjectOfType<GameUIManager>());
    }

    private void Update()
    {
        if (Input.GetKeyDown(metadataToggleKey))
        {
            showDetailedMetadata = !showDetailedMetadata;
            RefreshGeneratedLabels();
            UnityEngine.Debug.Log(
                $"Real shelter metadata labels switched to {(showDetailedMetadata ? "detailed" : "compact")} mode.");
        }
    }

    public int Generate(
        ShelterDataSourceResolver.ShelterDataSourceResult sourceResult,
        EvacuationGameManager gameManager,
        GameUIManager gameUIManager)
    {
        if (sourceResult == null || !sourceResult.IsRealSample || sourceResult.realShelters == null)
        {
            return 0;
        }

        ShelterDataLoader.ShelterData[] markerShelters =
            ShelterGameplayDataMapper.MapRealSheltersToGameplayData(sourceResult.realShelters);

        if (markerShelters.Length == 0)
        {
            return 0;
        }

        ShelterDataLoader.ClearRuntimeShelters();
        ShelterDataLoader.RegisterRuntimeShelters(markerShelters, "P4-B real_sample marker generator");

        DisableExistingTestShelters();
        InitializeMaterials();
        generatedLabels.Clear();

        Transform parent = ResolveRuntimeParent();
        Vector3 debugOrigin = ResolveDebugOrigin();

        for (int i = 0; i < markerShelters.Length; i++)
        {
            CreateShelterMarker(markerShelters[i], i, parent, debugOrigin, gameManager, gameUIManager);
        }

        UnityEngine.Debug.Log(
            $"Generated {markerShelters.Length} real shelter marker(s) from {sourceResult.sourcePath}. " +
            "This P4-B layer is runtime-only and does not save scene changes.");
        return markerShelters.Length;
    }

    private void CreateShelterMarker(
        ShelterDataLoader.ShelterData shelterData,
        int index,
        Transform parent,
        Vector3 debugOrigin,
        EvacuationGameManager gameManager,
        GameUIManager gameUIManager)
    {
        if (shelterData == null)
        {
            return;
        }

        string safeId = MakeSafeObjectName(shelterData.shelterId);
        Vector3 localPosition = shelterData.layoutPosition != null
            ? shelterData.layoutPosition.ToVector3()
            : ShelterGameplayDataMapper.CreateDeterministicFallbackLayout(index).ToVector3();
        Vector3 worldPosition = debugOrigin + localPosition;

        GameObject shelterObject = new GameObject($"RealShelter_{safeId}");
        shelterObject.transform.SetParent(parent, true);
        shelterObject.transform.position = worldPosition;
        BuildingShelter shelter = shelterObject.AddComponent<BuildingShelter>();
        shelter.ApplyShelterData(shelterData);

        GameObject markerObject = CreatePrimitive(
            $"RealShelterMarker_{safeId}",
            PrimitiveType.Cube,
            shelterObject.transform,
            worldPosition + new Vector3(0f, 0.25f, 0f),
            markerSize);
        SetMaterial(markerObject, shelterData.canEnter ? enterableMaterial : blockedMaterial);
        RemoveCollider(markerObject);

        GameObject entranceObject = CreatePrimitive(
            $"RealShelterEntrance_{safeId}",
            PrimitiveType.Cube,
            shelterObject.transform,
            worldPosition + new Vector3(0f, 1f, 0f),
            entranceSize);
        SetMaterial(entranceObject, entranceMaterial);
        SetColliderTrigger(entranceObject, true);

        ShelterEntranceTrigger entrance = entranceObject.AddComponent<ShelterEntranceTrigger>();
        ConfigureEntrance(entrance, shelter, gameManager, gameUIManager);
        CreateLabel($"RealShelterLabel_{safeId}", shelterObject.transform, worldPosition + labelOffset, shelterData);
    }

    private static void DisableExistingTestShelters()
    {
        BuildingShelter[] existingShelters = UnityEngine.Object.FindObjectsOfType<BuildingShelter>();

        foreach (BuildingShelter existingShelter in existingShelters)
        {
            if (existingShelter == null ||
                string.Equals(existingShelter.SourceType, ShelterSourceConfigLoader.RealSampleSourceMode, StringComparison.OrdinalIgnoreCase))
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
        enterableMaterial = CreateMaterial("Real Shelter Marker Material", new Color(0.05f, 0.8f, 0.45f, 0.92f), false);
        blockedMaterial = CreateMaterial("Real Shelter Blocked Material", new Color(1f, 0.18f, 0.12f, 0.92f), false);
        entranceMaterial = CreateMaterial("Real Shelter Entrance Material", new Color(0.1f, 1f, 0.25f, 0.75f), true);
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
        ShelterDataLoader.ShelterData shelterData)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, true);
        labelObject.transform.position = position;
        labelObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = FormatLabel(shelterData);
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.28f;
        textMesh.fontSize = 26;
        textMesh.color = Color.white;

        generatedLabels.Add(new GeneratedShelterLabel
        {
            Label = textMesh,
            ShelterData = shelterData
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

            generatedLabel.Label.text = FormatLabel(generatedLabel.ShelterData);
        }
    }

    private string FormatLabel(ShelterDataLoader.ShelterData shelterData)
    {
        return showDetailedMetadata
            ? ShelterDebugMetadataFormatter.BuildDetailedMarkerLabel(shelterData)
            : ShelterDebugMetadataFormatter.BuildCompactMarkerLabel(shelterData);
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
        return Application.isEditor || UnityEngine.Debug.isDebugBuild;
    }

    private class GeneratedShelterLabel
    {
        public TextMesh Label;
        public ShelterDataLoader.ShelterData ShelterData;
    }
}
