using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class NewMapBuildingEntryTrigger : MonoBehaviour
{
    private NewMapRuntimeTarget target;
    private NewMapGameController controller;
    private Collider triggerCollider;

    public NewMapRuntimeTarget Target => target;
    public bool IsTriggerCollider => triggerCollider != null && triggerCollider.isTrigger;
    public Bounds Bounds => triggerCollider != null ? triggerCollider.bounds : new Bounds(transform.position, Vector3.zero);

    public static NewMapBuildingEntryTrigger Create(
        Transform parent,
        NewMapRuntimeTarget runtimeTarget,
        NewMapGameController gameController,
        float radiusMeters,
        float heightMeters)
    {
        if (runtimeTarget == null || runtimeTarget.Anchor == null)
        {
            return null;
        }

        GameObject triggerObject = new GameObject("P10_BuildingEntryTrigger_" + SanitizeName(runtimeTarget.Id));
        triggerObject.transform.SetParent(parent != null ? parent : runtimeTarget.Anchor, true);
        triggerObject.transform.position = runtimeTarget.Anchor.position + Vector3.up * Mathf.Max(0.5f, heightMeters * 0.5f);

        BoxCollider box = triggerObject.AddComponent<BoxCollider>();
        float radius = Mathf.Clamp(radiusMeters, 1f, 60f);
        box.size = new Vector3(radius * 2f, Mathf.Clamp(heightMeters, 1f, 80f), radius * 2f);
        box.isTrigger = true;

        NewMapBuildingEntryTrigger trigger = triggerObject.AddComponent<NewMapBuildingEntryTrigger>();
        trigger.Initialize(runtimeTarget, gameController, box);
        runtimeTarget.EntryTrigger = trigger;
        return trigger;
    }

    public static NewMapBuildingEntryTrigger CreateFromBounds(
        Transform parent,
        NewMapRuntimeTarget runtimeTarget,
        NewMapGameController gameController,
        Bounds buildingBounds,
        float horizontalMarginMeters,
        float minimumHeightMeters)
    {
        if (runtimeTarget == null || runtimeTarget.Anchor == null || buildingBounds.size.sqrMagnitude <= 0.01f)
        {
            return null;
        }

        float margin = Mathf.Clamp(horizontalMarginMeters, 0.25f, 8f);
        float bottomY = Mathf.Min(buildingBounds.min.y, runtimeTarget.Anchor.position.y - 0.5f);
        float topY = Mathf.Max(buildingBounds.max.y, runtimeTarget.Anchor.position.y + Mathf.Max(2f, minimumHeightMeters));
        float sizeY = Mathf.Clamp(topY - bottomY, 2f, 140f);
        Vector3 center = new Vector3(buildingBounds.center.x, bottomY + sizeY * 0.5f, buildingBounds.center.z);
        Vector3 size = new Vector3(
            Mathf.Clamp(buildingBounds.size.x + margin * 2f, 2f, 240f),
            sizeY,
            Mathf.Clamp(buildingBounds.size.z + margin * 2f, 2f, 240f));

        GameObject triggerObject = new GameObject("P10_BuildingEntryTrigger_" + SanitizeName(runtimeTarget.Id));
        triggerObject.transform.SetParent(parent != null ? parent : runtimeTarget.Anchor, true);
        triggerObject.transform.position = center;

        BoxCollider box = triggerObject.AddComponent<BoxCollider>();
        box.size = size;
        box.isTrigger = true;

        NewMapBuildingEntryTrigger trigger = triggerObject.AddComponent<NewMapBuildingEntryTrigger>();
        trigger.Initialize(runtimeTarget, gameController, box);
        runtimeTarget.EntryTrigger = trigger;
        return trigger;
    }

    public bool OverlapsPlayer(Bounds playerBounds, Vector3 playerPosition)
    {
        if (triggerCollider == null)
        {
            return false;
        }

        Bounds bounds = triggerCollider.bounds;
        if (bounds.Intersects(playerBounds))
        {
            return true;
        }

        return playerPosition.x >= bounds.min.x &&
            playerPosition.x <= bounds.max.x &&
            playerPosition.z >= bounds.min.z &&
            playerPosition.z <= bounds.max.z &&
            playerPosition.y >= bounds.min.y - 1f &&
            playerPosition.y <= bounds.max.y + 1f;
    }

    public void Initialize(NewMapRuntimeTarget runtimeTarget, NewMapGameController gameController, Collider collider)
    {
        target = runtimeTarget;
        controller = gameController;
        triggerCollider = collider != null ? collider : GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        controller?.NotifyBuildingEntryTouch(target, true);
        Debug.Log($"NewMap building touched id={target?.Id} name={target?.DisplayName} official={target?.IsOfficialShelter}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        controller?.NotifyBuildingEntryTouch(target, false);
        Debug.Log($"NewMap building touch left id={target?.Id} name={target?.DisplayName}");
    }

    private static bool IsPlayer(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        return other.GetComponentInParent<NewMapPlayerController>() != null;
    }

    private static string SanitizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
            {
                chars[i] = '_';
            }
        }

        return new string(chars);
    }
}
