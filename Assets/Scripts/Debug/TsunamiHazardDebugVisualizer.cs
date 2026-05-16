using UnityEngine;

public class TsunamiHazardDebugVisualizer : MonoBehaviour
{
    private const string VisualizerObjectName = "P4B_HazardDebugVisualizer";
    private const string VisualizationRootName = "P4B_HazardDebugVisualization_Runtime";
    private const string GameplayRootName = "GameplayTestRoot";
    private const string TestGroundName = "TestGround";
    private static readonly Vector3 DefaultDebugOrigin = new Vector3(0f, 80f, -250f);

    [SerializeField] private bool showOnStart;
    [SerializeField] private KeyCode toggleKey = KeyCode.H;
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 1.4f, 0f);

    private GameObject visualizationRoot;
    private Material hazardMaterial;

    public static bool AffectsGameplayRules => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapHazardDebugVisualizer()
    {
        if (!IsDebugContext())
        {
            return;
        }

        if (GameObject.Find(VisualizerObjectName) != null)
        {
            return;
        }

        if (UnityEngine.Object.FindObjectOfType<EvacuationGameManager>() == null)
        {
            return;
        }

        GameObject visualizerObject = new GameObject(VisualizerObjectName);
        visualizerObject.AddComponent<TsunamiHazardDebugVisualizer>();
        UnityEngine.Debug.Log("P4-B hazard debug visualizer is available. Press H in Play Mode to toggle the schematic fixture layer.");
    }

    private void Start()
    {
        if (showOnStart)
        {
            Show();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (visualizationRoot != null && visualizationRoot.activeSelf)
        {
            Hide();
            return;
        }

        Show();
    }

    public void Show()
    {
        if (visualizationRoot != null)
        {
            visualizationRoot.SetActive(true);
            return;
        }

        TsunamiHazardFixtureLoader.HazardFixtureLoadResult loadResult =
            TsunamiHazardFixtureLoader.LoadHazardFixture();

        if (!loadResult.success)
        {
            return;
        }

        TsunamiHazardDebugLayout.HazardDebugShape[] shapes =
            TsunamiHazardDebugLayout.CreateShapes(loadResult.zones);

        if (shapes.Length == 0)
        {
            return;
        }

        Transform parent = ResolveRuntimeParent();
        Vector3 debugOrigin = ResolveDebugOrigin();
        visualizationRoot = new GameObject(VisualizationRootName);
        visualizationRoot.transform.SetParent(parent, true);

        foreach (TsunamiHazardDebugLayout.HazardDebugShape shape in shapes)
        {
            CreateHazardShape(shape, visualizationRoot.transform, debugOrigin);
        }

        UnityEngine.Debug.Log(
            $"Displayed {shapes.Length} debug tsunami hazard fixture zone(s). " +
            "This layer is schematic, has no colliders, and does not affect gameplay success or failure.");
    }

    public void Hide()
    {
        if (visualizationRoot != null)
        {
            visualizationRoot.SetActive(false);
        }
    }

    private void CreateHazardShape(
        TsunamiHazardDebugLayout.HazardDebugShape shape,
        Transform parent,
        Vector3 debugOrigin)
    {
        if (shape == null)
        {
            return;
        }

        GameObject zoneObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zoneObject.name = $"HazardDebugZone_{MakeSafeObjectName(shape.zoneId)}";
        zoneObject.transform.SetParent(parent, true);
        zoneObject.transform.position = debugOrigin + shape.localPosition;
        zoneObject.transform.localScale = shape.localScale;
        RemoveCollider(zoneObject);
        SetMaterial(zoneObject, CreateHazardMaterial(shape.color));

        GameObject labelObject = new GameObject($"HazardDebugLabel_{MakeSafeObjectName(shape.zoneId)}");
        labelObject.transform.SetParent(parent, true);
        labelObject.transform.position = debugOrigin + shape.localPosition + labelOffset;
        labelObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = shape.label;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.26f;
        textMesh.fontSize = 24;
        textMesh.color = Color.white;
    }

    private Material CreateHazardMaterial(Color color)
    {
        if (hazardMaterial == null)
        {
            hazardMaterial = CreateMaterial("Hazard Debug Fixture Material", color, true);
            return hazardMaterial;
        }

        Material material = new Material(hazardMaterial)
        {
            color = color
        };
        return material;
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
}
