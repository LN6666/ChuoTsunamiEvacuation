using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class NewMapRuntimeBootstrap : MonoBehaviour
{
    private const bool SuppressSceneMeshCollidersForManualTest = true;

    private static readonly string[] RequiredRoots =
    {
        "MapRoot",
        "RuntimeSystemsRoot",
        "PlayerSpawnRoot",
        "ShelterMarkerRoot",
        "CandidateMarkerRoot",
        "HazardVisualRoot",
        "NavigationRoot",
        "CrowdRoot",
        "CollapseDebrisRoot",
        "GreenFrameRoot",
        "UIAnchorRoot",
        "DebugDiagnosticsRoot",
        "PerformanceMetricsRoot"
    };

    public Bounds LastMapBounds { get; private set; }
    public bool LastMapBoundsValid { get; private set; }
    public bool LastUsedGroundSupportProxy { get; private set; }
    public bool LastRuntimeCollisionSupportProxyActive { get; private set; }
    public int LastDisabledSceneMeshColliderCount { get; private set; }
    public int LastRendererCount { get; private set; }
    public int LastColliderCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        Scene scene = SceneManager.GetActiveScene();
        bool isChuoBaseMap =
            string.Equals(scene.name, NewMapRuntimeConstants.SceneName, System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(scene.path, NewMapRuntimeConstants.ScenePath, System.StringComparison.OrdinalIgnoreCase);

        if (!isChuoBaseMap || FindObjectOfType<NewMapRuntimeBootstrap>() != null)
        {
            return;
        }

        CreateForCurrentScene();
    }

    public static NewMapRuntimeBootstrap CreateForCurrentScene()
    {
        Dictionary<string, Transform> roots = EnsureRoots();
        GameObject bootstrapObject = new GameObject("NewMap_RuntimeBootstrap");
        bootstrapObject.transform.SetParent(roots["RuntimeSystemsRoot"], false);
        NewMapRuntimeBootstrap bootstrap = bootstrapObject.AddComponent<NewMapRuntimeBootstrap>();
        bootstrap.Build(roots);
        return bootstrap;
    }

    public static Dictionary<string, Transform> EnsureRoots()
    {
        var roots = new Dictionary<string, Transform>();
        foreach (string rootName in RequiredRoots)
        {
            GameObject root = GameObject.Find(rootName);
            if (root == null)
            {
                root = new GameObject(rootName);
            }

            roots[rootName] = root.transform;
        }

        return roots;
    }

    private void Build(Dictionary<string, Transform> roots)
    {
        Physics.SyncTransforms();
        LastMapBoundsValid = TryCalculateMapBounds(out Bounds mapBounds, out int rendererCount, out int colliderCount);
        LastMapBounds = mapBounds;
        LastRendererCount = rendererCount;
        LastColliderCount = colliderCount;

        Vector3 spawn = ResolveSpawnPosition(mapBounds, LastMapBoundsValid, roots["DebugDiagnosticsRoot"]);
        if (SuppressSceneMeshCollidersForManualTest)
        {
            EnsureRuntimeCollisionSupportProxy(roots["DebugDiagnosticsRoot"], spawn - Vector3.up * 1.15f);
            LastDisabledSceneMeshColliderCount = DisableSceneMeshColliders();
            Physics.SyncTransforms();
        }

        NewMapPlayerController player = NewMapPlayerController.Create(roots["PlayerSpawnRoot"], spawn);
        NewMapRuntimeUI.EnsureRuntimeEventSystem();
        NewMapRuntimeUI ui = NewMapRuntimeUI.Create(roots["UIAnchorRoot"]);
        NewMapHazardController hazard = NewMapHazardController.Create(roots["HazardVisualRoot"], roots["CollapseDebrisRoot"], spawn);
        NewMapNpcCrowdPrototype crowd = NewMapNpcCrowdPrototype.Create(roots["CrowdRoot"], spawn);
        NewMapPerformanceProbe.Create(roots["PerformanceMetricsRoot"]);
        List<NewMapRuntimeTarget> targets = CreateLocalRuntimeTargets(roots, spawn);

        NewMapGameController controller = gameObject.AddComponent<NewMapGameController>();
        controller.Configure(player, ui, hazard, crowd, targets, BuildDiagnosticText());
        Debug.Log(
            $"NewMap runtime bootstrap completed. renderers={LastRendererCount} colliders={LastColliderCount} " +
            $"groundSupportProxy={LastUsedGroundSupportProxy} collisionSupportProxy={LastRuntimeCollisionSupportProxyActive} " +
            $"disabledSceneMeshColliders={LastDisabledSceneMeshColliderCount} activeRuntimeTargets={targets.Count}");
    }

    private string BuildDiagnosticText()
    {
        string ground = LastUsedGroundSupportProxy
            ? "Ground: runtime support proxy active"
            : "Ground: scene collider raycast spawn active";
        string collisionProxy = LastRuntimeCollisionSupportProxyActive
            ? $" | Runtime collision support proxy active; scene MeshColliders disabled={LastDisabledSceneMeshColliderCount}"
            : string.Empty;
        return $"{ground}{collisionProxy} | Old P3/P5 targets disabled unless remapped.";
    }

    private bool TryCalculateMapBounds(out Bounds bounds, out int rendererCount, out int colliderCount)
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        Collider[] colliders = FindObjectsOfType<Collider>();
        rendererCount = 0;
        colliderCount = 0;
        bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer.GetComponentInParent<Canvas>() != null)
            {
                continue;
            }

            rendererCount++;
            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        foreach (Collider collider in colliders)
        {
            if (collider != null && collider.GetComponentInParent<Canvas>() == null)
            {
                colliderCount++;
            }
        }

        return hasBounds && bounds.size.sqrMagnitude > 1f;
    }

    private Vector3 ResolveSpawnPosition(Bounds mapBounds, bool hasBounds, Transform diagnosticsRoot)
    {
        Vector3 basePosition = hasBounds ? mapBounds.center : Vector3.zero;
        float rayStartY = hasBounds ? mapBounds.max.y + 250f : 250f;
        Vector3[] offsets =
        {
            Vector3.zero,
            new Vector3(20f, 0f, 20f),
            new Vector3(-20f, 0f, 20f),
            new Vector3(20f, 0f, -20f),
            new Vector3(-20f, 0f, -20f)
        };

        foreach (Vector3 offset in offsets)
        {
            Vector3 origin = new Vector3(basePosition.x + offset.x, rayStartY, basePosition.z + offset.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
            {
                LastUsedGroundSupportProxy = false;
                return hit.point + Vector3.up * 1.15f;
            }
        }

        LastUsedGroundSupportProxy = true;
        Vector3 supportCenter = new Vector3(basePosition.x, hasBounds ? mapBounds.min.y : 0f, basePosition.z);
        EnsureRuntimeCollisionSupportProxy(diagnosticsRoot, supportCenter);
        return supportCenter + Vector3.up * 1.5f;
    }

    private void EnsureRuntimeCollisionSupportProxy(Transform parent, Vector3 center)
    {
        if (LastRuntimeCollisionSupportProxyActive)
        {
            return;
        }

        CreateGroundSupportProxy(parent, center);
        LastRuntimeCollisionSupportProxyActive = true;
    }

    private static void CreateGroundSupportProxy(Transform parent, Vector3 center)
    {
        Material material = NewMapVisualFactory.CreateMaterial("NewMap_RuntimeGroundSupport_Material", new Color(0.18f, 0.2f, 0.18f, 0.55f), true);
        GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
        support.name = "NewMap_RuntimeGroundSupport_DocumentedProxy";
        support.transform.SetParent(parent, true);
        support.transform.position = center - Vector3.up * 0.25f;
        support.transform.localScale = new Vector3(700f, 0.5f, 700f);
        Renderer renderer = support.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static int DisableSceneMeshColliders()
    {
        int disabled = 0;
        MeshCollider[] meshColliders = FindObjectsOfType<MeshCollider>();
        foreach (MeshCollider meshCollider in meshColliders)
        {
            if (meshCollider == null || !meshCollider.enabled)
            {
                continue;
            }

            meshCollider.enabled = false;
            disabled++;
        }

        return disabled;
    }

    private static List<NewMapRuntimeTarget> CreateLocalRuntimeTargets(Dictionary<string, Transform> roots, Vector3 spawn)
    {
        var targets = new List<NewMapRuntimeTarget>
        {
            CreateTarget(
                roots,
                "newmap_proxy_safe_floor",
                "Local Training Proxy - Safe Floor",
                spawn,
                spawn + new Vector3(12f, -1.05f, 10f),
                false,
                false,
                true,
                "Runtime safe-floor proxy: E starts vertical evacuation and can succeed."),
            CreateTarget(
                roots,
                "newmap_proxy_blocked_entrance",
                "Local Training Proxy - Blocked Entrance",
                spawn,
                spawn + new Vector3(18f, -1.05f, -7f),
                false,
                true,
                true,
                "Runtime entrance-blocked proxy: E triggers entrance_blocked result."),
            CreateTarget(
                roots,
                "newmap_proxy_no_safe_floor",
                "Local Training Proxy - No Safe Floor",
                spawn,
                spawn + new Vector3(-13f, -1.05f, 11f),
                false,
                false,
                false,
                "Runtime safe-floor failure proxy: E triggers safe_floor_unavailable result.")
        };

        return targets;
    }

    private static NewMapRuntimeTarget CreateTarget(
        Dictionary<string, Transform> roots,
        string id,
        string displayName,
        Vector3 spawn,
        Vector3 position,
        bool isOfficial,
        bool entranceBlocked,
        bool safeFloorAvailable,
        string finalBehavior)
    {
        GameObject anchor = new GameObject(id);
        anchor.transform.SetParent(roots["CandidateMarkerRoot"], true);
        anchor.transform.position = position;

        Material markerMaterial = NewMapVisualFactory.CreateMaterial(
            id + "_MarkerMaterial",
            entranceBlocked ? new Color(1f, 0.2f, 0.12f, 0.9f) : new Color(0.1f, 0.8f, 0.38f, 0.9f),
            false);
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = id + "_marker_non_official";
        marker.transform.SetParent(anchor.transform, false);
        marker.transform.localPosition = Vector3.up * 0.08f;
        marker.transform.localScale = new Vector3(2.4f, 0.08f, 2.4f);
        Renderer markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null && markerMaterial != null)
        {
            markerRenderer.sharedMaterial = markerMaterial;
        }
        NewMapVisualFactory.RemoveCollider(marker);

        GameObject frame = CreateGreenFrame(roots["GreenFrameRoot"], id + "_green_frame", position);
        frame.SetActive(false);
        GameObject routeGuide = CreateEstimatedRouteGuide(roots["NavigationRoot"], id + "_estimated_route_proxy", spawn, position);
        routeGuide.SetActive(false);
        GameObject label = new GameObject(id + "_label");
        label.transform.SetParent(anchor.transform, false);
        label.transform.localPosition = new Vector3(0f, 2.1f, 0f);
        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = displayName + "\nNon-official training proxy";
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.22f;
        textMesh.fontSize = 22;
        textMesh.color = Color.white;

        return new NewMapRuntimeTarget
        {
            Id = id,
            DisplayName = displayName,
            Category = "runtime_proxy_training_target",
            IsOfficialShelter = isOfficial,
            NonOfficialWarningRequired = !isOfficial,
            SafeApprovedByDefault = false,
            EntranceBlocked = entranceBlocked,
            SafeFloorAvailable = safeFloorAvailable,
            InteractionDistance = 4f,
            ClimbSeconds = 5f,
            Anchor = anchor.transform,
            Marker = marker,
            GreenFrame = frame,
            RouteGuide = routeGuide,
            FinalBehavior = finalBehavior
        };
    }

    private static GameObject CreateGreenFrame(Transform parent, string name, Vector3 position)
    {
        GameObject frame = new GameObject(name);
        frame.transform.SetParent(parent, true);
        frame.transform.position = position + Vector3.up * 0.06f;

        LineRenderer line = frame.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.widthMultiplier = 0.08f;
        line.positionCount = 4;
        line.sharedMaterial = NewMapVisualFactory.CreateMaterial(name + "_Material", new Color(0.2f, 1f, 0.25f, 0.82f), true);
        float half = 2.2f;
        line.SetPosition(0, new Vector3(-half, 0f, -half));
        line.SetPosition(1, new Vector3(half, 0f, -half));
        line.SetPosition(2, new Vector3(half, 0f, half));
        line.SetPosition(3, new Vector3(-half, 0f, half));
        return frame;
    }

    private static GameObject CreateEstimatedRouteGuide(Transform parent, string name, Vector3 spawn, Vector3 target)
    {
        GameObject route = new GameObject(name);
        route.transform.SetParent(parent, true);

        LineRenderer line = route.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.widthMultiplier = 0.12f;
        line.positionCount = 3;
        line.sharedMaterial = NewMapVisualFactory.CreateMaterial(name + "_Material", new Color(1f, 0.86f, 0.16f, 0.78f), true);
        Vector3 start = new Vector3(spawn.x, target.y + 0.16f, spawn.z);
        Vector3 middle = Vector3.Lerp(start, target + Vector3.up * 0.16f, 0.5f) + Vector3.right * 1.8f;
        Vector3 end = target + Vector3.up * 0.16f;
        line.SetPosition(0, start);
        line.SetPosition(1, middle);
        line.SetPosition(2, end);
        return route;
    }
}
