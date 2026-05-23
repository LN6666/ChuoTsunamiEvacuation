using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class P8RiskFrontLightCurtainRenderer : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool RequiresCustomShaderOrPackage = false;

    [SerializeField] private Color curtainColor = new Color(0.05f, 0.6f, 1f, 0.42f);
    [SerializeField] private bool visibleOnStart;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh generatedMesh;
    private Material generatedMaterial;

    public int LastVertexCount { get; private set; }
    public bool IsVisible => meshRenderer != null && meshRenderer.enabled;

    private void Awake()
    {
        EnsureComponents();
        SetVisible(visibleOnStart);
    }

    public void UpdateCurtain(Vector3[] visualBoundary, P8RiskFrontVisualConfig config)
    {
        EnsureComponents();

        if (visualBoundary == null || visualBoundary.Length < 2 || config == null || !config.Validate() || !config.visualEnabled)
        {
            ClearMesh();
            SetVisible(false);
            return;
        }

        EnsureMesh();
        float height = Mathf.Max(0.1f, config.visualHeightMeters);
        int pointCount = visualBoundary.Length;
        var vertices = new Vector3[pointCount * 2];
        var colors = new Color[vertices.Length];
        var triangles = new int[(pointCount - 1) * 6];
        Color color = curtainColor;
        color.a = Mathf.Clamp01(config.materialAlpha);

        for (int i = 0; i < pointCount; i++)
        {
            vertices[i * 2] = visualBoundary[i];
            vertices[i * 2 + 1] = visualBoundary[i] + Vector3.up * height;
            colors[i * 2] = color;
            colors[i * 2 + 1] = color;
        }

        int triangleIndex = 0;
        for (int i = 0; i < pointCount - 1; i++)
        {
            int bottomA = i * 2;
            int topA = bottomA + 1;
            int bottomB = bottomA + 2;
            int topB = bottomA + 3;

            triangles[triangleIndex++] = bottomA;
            triangles[triangleIndex++] = topA;
            triangles[triangleIndex++] = topB;
            triangles[triangleIndex++] = bottomA;
            triangles[triangleIndex++] = topB;
            triangles[triangleIndex++] = bottomB;
        }

        generatedMesh.Clear();
        generatedMesh.vertices = vertices;
        generatedMesh.colors = colors;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateBounds();
        LastVertexCount = vertices.Length;
        SetVisible(true);
    }

    public void SetVisible(bool visible)
    {
        EnsureComponents();
        meshRenderer.enabled = visible;
    }

    public void ClearMesh()
    {
        if (generatedMesh != null)
        {
            generatedMesh.Clear();
        }

        LastVertexCount = 0;
    }

    private void EnsureComponents()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Unlit/Color");
                if (shader == null)
                {
                    shader = Shader.Find("Sprites/Default");
                }

                var material = new Material(shader);
                material.name = "P8B_CinematicRiskFront_Fallback";
                material.color = curtainColor;
                material.hideFlags = HideFlags.DontSave;
                meshRenderer.sharedMaterial = material;
                generatedMaterial = material;
            }
        }
    }

    private void EnsureMesh()
    {
        if (generatedMesh != null)
        {
            return;
        }

        generatedMesh = new Mesh
        {
            name = "P8B_CinematicRiskFrontMesh",
            hideFlags = HideFlags.DontSave
        };
        generatedMesh.MarkDynamic();
        meshFilter.sharedMesh = generatedMesh;
    }

    private void OnDestroy()
    {
        if (generatedMesh != null)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(generatedMesh);
            }
        }

        if (generatedMaterial != null)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedMaterial);
            }
            else
            {
                DestroyImmediate(generatedMaterial);
            }
        }
    }
}
