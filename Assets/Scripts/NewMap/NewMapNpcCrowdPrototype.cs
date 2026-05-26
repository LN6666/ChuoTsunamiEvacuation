using System.Collections.Generic;
using UnityEngine;

public sealed class NewMapNpcCrowdPrototype : MonoBehaviour
{
    [SerializeField] private int npcCap = 8;
    [SerializeField] private float wanderRadius = 9f;
    [SerializeField] private float wanderSpeed = 0.85f;

    private readonly List<Transform> npcs = new List<Transform>();
    private Vector3 center;
    private bool crowdFailuresEnabled;

    public int NpcCap => npcCap;
    public int ActiveNpcCount => npcs.Count;
    public float CurrentCongestionDelaySeconds { get; private set; }

    public static NewMapNpcCrowdPrototype Create(Transform parent, Vector3 centerPosition)
    {
        GameObject crowdObject = new GameObject("NewMap_NPC_CrowdPrototype");
        crowdObject.transform.SetParent(parent, false);
        NewMapNpcCrowdPrototype crowd = crowdObject.AddComponent<NewMapNpcCrowdPrototype>();
        crowd.Build(centerPosition);
        return crowd;
    }

    public void SetCrowdFailuresEnabled(bool enabled)
    {
        crowdFailuresEnabled = enabled;
        CurrentCongestionDelaySeconds = enabled ? Mathf.Min(6f, npcs.Count * 0.4f) : 0f;
    }

    public float GetDelayForTarget(NewMapRuntimeTarget target)
    {
        if (!crowdFailuresEnabled || target == null || target.Anchor == null)
        {
            return 0f;
        }

        int nearby = 0;
        foreach (Transform npc in npcs)
        {
            if (npc != null && Vector3.Distance(npc.position, target.Anchor.position) <= 8f)
            {
                nearby++;
            }
        }

        CurrentCongestionDelaySeconds = Mathf.Min(8f, nearby * 0.75f);
        return CurrentCongestionDelaySeconds;
    }

    private void Update()
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            Transform npc = npcs[i];
            if (npc == null)
            {
                continue;
            }

            float phase = Time.time * 0.3f + i * 1.7f;
            Vector3 target = center + new Vector3(Mathf.Sin(phase), 0f, Mathf.Cos(phase * 0.8f)) * wanderRadius;
            Vector3 delta = target - npc.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > 0.01f)
            {
                npc.position += delta.normalized * wanderSpeed * Time.deltaTime;
                npc.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            }
        }
    }

    private void Build(Vector3 centerPosition)
    {
        center = centerPosition + new Vector3(8f, 0f, 4f);
        int count = Mathf.Clamp(npcCap, 0, 24);
        for (int i = 0; i < count; i++)
        {
            GameObject npc = new GameObject($"NewMap_NPC_{i + 1:00}");
            npc.transform.SetParent(transform, true);
            float angle = i * Mathf.PI * 2f / Mathf.Max(1, count);
            npc.transform.position = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (wanderRadius * 0.55f);
            NewMapVisualFactory.CreateHumanoid(npc.transform, "NPCVisual", new Color(1f, 0.62f, 0.12f, 1f));
            npcs.Add(npc.transform);
        }
    }
}
