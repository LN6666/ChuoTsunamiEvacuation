using System;
using System.Collections.Generic;
using UnityEngine;

public class P5CQualificationOverlayRuntimeGenerator : MonoBehaviour
{
    private const string RuntimeRootName = "P5C_QualificationOverlay_Runtime";
    private const string ControllerObjectName = "P5C_QualificationOverlay_Controller";
    private const string GameplayRootName = "GameplayTestRoot";
    private const string TestGroundName = "TestGround";
    private const int DefaultMarkerLimit = 12;
    private static readonly Vector3 DefaultDebugOrigin = new Vector3(0f, 80f, -250f);

    [SerializeField] private int markerLimit = DefaultMarkerLimit;
    [SerializeField] private Vector3 markerSize = new Vector3(2.2f, 0.35f, 2.2f);
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 2.7f, 0f);
    [SerializeField] private KeyCode metadataToggleKey = KeyCode.M;
    [SerializeField] private bool showDetailedMetadata;

    private Material officialMaterial;
    private Material candidateMaterial;
    private Material reviewMaterial;
    private Material unknownMaterial;
    private Material routeMaterial;
    private GameObject visualizationRoot;
    private GameObject routeLineRoot;
    private P5CStaticDataLoader.P5CDataBundle currentBundle;
    private readonly List<GeneratedQualificationLabel> generatedLabels = new List<GeneratedQualificationLabel>();

    public static bool AffectsGameplayRules => false;
    public int LastGeneratedRouteLineCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapP5CQualificationOverlay()
    {
        if (!IsDebugContext())
        {
            return;
        }

        if (GameObject.Find(RuntimeRootName) != null)
        {
            return;
        }

        if (UnityEngine.Object.FindObjectOfType<EvacuationGameManager>() == null)
        {
            return;
        }

        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.Load();
        if (!string.Equals(config.sourceMode, ShelterSourceConfigLoader.RealSampleSourceMode, StringComparison.OrdinalIgnoreCase) ||
            !config.enableP5COverlay)
        {
            return;
        }

        P5CStaticDataLoader.P5CDataBundle bundle = P5CStaticDataLoader.LoadBundleFromAssetsData();
        if (!bundle.HasCoreQualificationData)
        {
            Debug.LogWarning("P5-C overlay was enabled, but static qualification data was unavailable. Overlay disabled.");
            return;
        }

        GameObject generatorObject = new GameObject(ControllerObjectName);
        P5CQualificationOverlayRuntimeGenerator generator =
            generatorObject.AddComponent<P5CQualificationOverlayRuntimeGenerator>();
        generator.Generate(bundle);
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
        RefreshRouteLines();
        Debug.Log(
            $"P5-C qualification overlay metadata switched to {(showDetailedMetadata ? "detailed" : "compact")} mode.");
    }

    public int Generate(P5CStaticDataLoader.P5CDataBundle bundle)
    {
        currentBundle = bundle;
        LastGeneratedRouteLineCount = 0;
        generatedLabels.Clear();

        if (bundle == null || !bundle.HasCoreQualificationData)
        {
            Debug.LogWarning("P5-C qualification overlay did not generate because qualification data was unavailable.");
            return 0;
        }

        InitializeMaterials();
        Transform parent = ResolveRuntimeParent();
        Vector3 debugOrigin = ResolveDebugOrigin();

        visualizationRoot = new GameObject(RuntimeRootName);
        visualizationRoot.transform.SetParent(parent, true);

        routeLineRoot = new GameObject("P5C_EstimatedRouteLines_Runtime");
        routeLineRoot.transform.SetParent(visualizationRoot.transform, true);
        routeLineRoot.SetActive(showDetailedMetadata);

        P5CStaticDataLoader.IntegratedRouteQualificationRecord[] records =
            bundle.integratedRouteQualifications.records;
        int count = Mathf.Min(Mathf.Max(0, markerLimit), records.Length);

        for (int i = 0; i < count; i++)
        {
            CreateQualificationMarker(records[i], i, visualizationRoot.transform, debugOrigin, bundle);
        }

        RefreshRouteLines();
        Debug.Log(
            $"Displayed {count} P5-C qualification marker(s) from Assets/Data. " +
            "Markers are debug-only, collider-free, and do not affect gameplay success or failure.");
        return count;
    }

    private void CreateQualificationMarker(
        P5CStaticDataLoader.IntegratedRouteQualificationRecord record,
        int index,
        Transform parent,
        Vector3 debugOrigin,
        P5CStaticDataLoader.P5CDataBundle bundle)
    {
        if (record == null)
        {
            return;
        }

        Vector3 worldPosition = debugOrigin + CreateSchematicPosition(index);
        string safeId = MakeSafeObjectName(record.shelterId);

        GameObject markerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        markerObject.name = $"P5C_QualificationMarker_{safeId}";
        markerObject.transform.SetParent(parent, true);
        markerObject.transform.position = worldPosition + new Vector3(0f, 0.22f, 0f);
        markerObject.transform.localScale = markerSize;
        RemoveCollider(markerObject);
        SetMaterial(markerObject, ResolveMarkerMaterial(record));

        GameObject labelObject = new GameObject($"P5C_QualificationLabel_{safeId}");
        labelObject.transform.SetParent(parent, true);
        labelObject.transform.position = worldPosition + labelOffset;
        labelObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = FormatLabel(record, bundle);
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.24f;
        textMesh.fontSize = 22;
        textMesh.color = Color.white;

        generatedLabels.Add(new GeneratedQualificationLabel
        {
            Label = textMesh,
            Record = record
        });
    }

    private void RefreshGeneratedLabels()
    {
        foreach (GeneratedQualificationLabel generatedLabel in generatedLabels)
        {
            if (generatedLabel == null || generatedLabel.Label == null)
            {
                continue;
            }

            generatedLabel.Label.text = FormatLabel(generatedLabel.Record, currentBundle);
        }
    }

    private void RefreshRouteLines()
    {
        LastGeneratedRouteLineCount = 0;

        if (routeLineRoot == null)
        {
            return;
        }

        ClearChildren(routeLineRoot.transform);
        routeLineRoot.SetActive(showDetailedMetadata);

        if (!showDetailedMetadata || currentBundle == null || generatedLabels.Count == 0)
        {
            return;
        }

        foreach (GeneratedQualificationLabel generatedLabel in generatedLabels)
        {
            if (generatedLabel == null ||
                generatedLabel.Record == null ||
                !currentBundle.TryGetEvidenceForShelter(generatedLabel.Record.shelterId, out P5CStaticDataLoader.ShelterEvidence evidence) ||
                !P5CStaticDataLoader.CanRenderRouteGeometryInCurrentUnityLayout(evidence.routeSample))
            {
                continue;
            }

            CreateRouteLine(evidence.routeSample, routeLineRoot.transform);
            LastGeneratedRouteLineCount++;
        }
    }

    private void CreateRouteLine(P5CStaticDataLoader.RouteSampleRecord route, Transform parent)
    {
        GameObject lineObject = new GameObject($"P5C_EstimatedRouteLine_{MakeSafeObjectName(route.routeId)}");
        lineObject.transform.SetParent(parent, true);

        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.sharedMaterial = routeMaterial;
        lineRenderer.widthMultiplier = 0.18f;
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = route.geometry.coordinates.Length;

        for (int i = 0; i < route.geometry.coordinates.Length; i++)
        {
            Vector2 coordinate = route.geometry.coordinates[i];
            lineRenderer.SetPosition(i, new Vector3(coordinate.x, 0.1f, coordinate.y));
        }
    }

    private string FormatLabel(
        P5CStaticDataLoader.IntegratedRouteQualificationRecord record,
        P5CStaticDataLoader.P5CDataBundle bundle)
    {
        if (record == null)
        {
            return string.Empty;
        }

        if (!showDetailedMetadata)
        {
            return
                $"{FirstNonEmpty(record.shelterName, record.shelterId)}\n" +
                $"{record.qualificationStatus} / {record.confidence}\n" +
                $"Review: {(record.manualReviewNeeded ? "yes" : "no")}\n" +
                FormatRouteSummary(record);
        }

        string evidenceText = P5CDecisionFeedbackFormatter.BuildFromBundle(bundle, record.shelterId, true);
        return $"{FirstNonEmpty(record.shelterName, record.shelterId)}\n{evidenceText}\nRoute geometry: WGS84 loaded; Unity line rendering disabled until a verified coordinate transform exists.";
    }

    private static string FormatRouteSummary(P5CStaticDataLoader.IntegratedRouteQualificationRecord record)
    {
        if (record == null || !record.HasNearestRouteDistanceMeters || !record.HasEstimatedTravelTimeSeconds)
        {
            return "Route: unavailable";
        }

        return $"Route: {record.nearestRouteDistanceMeters:0.#}m / {record.estimatedTravelTimeSeconds:0.#}s";
    }

    private Vector3 CreateSchematicPosition(int index)
    {
        int safeIndex = Mathf.Max(0, index);
        int column = safeIndex % 4;
        int row = safeIndex / 4;
        return new Vector3(6f + column * 7f, 0f, -18f + row * 7f);
    }

    private Material ResolveMarkerMaterial(P5CStaticDataLoader.IntegratedRouteQualificationRecord record)
    {
        if (record != null && record.manualReviewNeeded)
        {
            return reviewMaterial;
        }

        string classification = P5CStaticDataLoader.ClassifyQualificationStatus(
            record != null ? record.qualificationStatus : string.Empty);

        if (string.Equals(classification, "official", StringComparison.OrdinalIgnoreCase))
        {
            return officialMaterial;
        }

        if (string.Equals(classification, "candidate", StringComparison.OrdinalIgnoreCase))
        {
            return candidateMaterial;
        }

        return unknownMaterial;
    }

    private void InitializeMaterials()
    {
        officialMaterial = CreateMaterial("P5-C Official Qualification Material", new Color(0.1f, 0.65f, 0.95f, 0.88f), true);
        candidateMaterial = CreateMaterial("P5-C Candidate Qualification Material", new Color(0.2f, 0.8f, 0.35f, 0.88f), true);
        reviewMaterial = CreateMaterial("P5-C Manual Review Material", new Color(1f, 0.75f, 0.18f, 0.88f), true);
        unknownMaterial = CreateMaterial("P5-C Unknown Qualification Material", new Color(0.7f, 0.7f, 0.75f, 0.76f), true);
        routeMaterial = CreateMaterial("P5-C Estimated Route Material", new Color(0.25f, 0.95f, 1f, 0.76f), true);
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

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
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

    private static string FirstNonEmpty(string first, string second)
    {
        return string.IsNullOrWhiteSpace(first) ? second : first.Trim();
    }

    private static bool IsDebugContext()
    {
        return Application.isEditor || Debug.isDebugBuild;
    }

    private class GeneratedQualificationLabel
    {
        public TextMesh Label;
        public P5CStaticDataLoader.IntegratedRouteQualificationRecord Record;
    }
}
