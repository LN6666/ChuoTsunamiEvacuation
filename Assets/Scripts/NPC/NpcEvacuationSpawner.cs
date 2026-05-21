using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NpcEvacuationSpawner : MonoBehaviour
{
    public static bool AffectsPlayerSuccessFailure => false;

    private const int HardMaxNpcCount = 30;

    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int npcCount = 12;
    [SerializeField] private int maxNpcCount = HardMaxNpcCount;
    [SerializeField] private Vector3 spawnOrigin;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(8f, 8f);
    [SerializeField] private float npcMoveSpeed = 2f;
    [SerializeField] private float arrivalDistance = 0.6f;
    [SerializeField] private bool spawnOnStart;
    [SerializeField] private bool autoFindShelterTargets = true;
    [SerializeField] private bool addStateLabels = true;
    [SerializeField] private Transform npcParent;
    [SerializeField] private List<BuildingShelter> explicitShelterTargets = new List<BuildingShelter>();

    private readonly List<NpcEvacuationAgent> spawnedAgents = new List<NpcEvacuationAgent>();

    public NpcEvacuationAgent[] SpawnedAgents => spawnedAgents.ToArray();

    private void Start()
    {
        if (spawnOnStart)
        {
            Spawn();
        }
    }

    private void OnValidate()
    {
        npcCount = Mathf.Clamp(npcCount, 0, HardMaxNpcCount);
        maxNpcCount = Mathf.Clamp(maxNpcCount, 0, HardMaxNpcCount);
        spawnAreaSize.x = Mathf.Max(0f, spawnAreaSize.x);
        spawnAreaSize.y = Mathf.Max(0f, spawnAreaSize.y);
        npcMoveSpeed = Mathf.Max(0f, npcMoveSpeed);
        arrivalDistance = Mathf.Max(0.01f, arrivalDistance);
    }

    public void ConfigureForTests(int requestedNpcCount, Vector3 origin, Vector2 areaSize)
    {
        npcCount = Mathf.Clamp(requestedNpcCount, 0, HardMaxNpcCount);
        maxNpcCount = HardMaxNpcCount;
        spawnOrigin = origin;
        spawnAreaSize = new Vector2(Mathf.Max(0f, areaSize.x), Mathf.Max(0f, areaSize.y));
    }

    public void ConfigureMovementForTests(float speed, float arriveDistance)
    {
        npcMoveSpeed = Mathf.Max(0f, speed);
        arrivalDistance = Mathf.Max(0.01f, arriveDistance);
    }

    public int Spawn()
    {
        return Spawn(CollectTargetsFromScene());
    }

    public int Spawn(IList<NpcEvacuationTargetInfo> targets)
    {
        ClearSpawnedNpcs();

        int count = ResolveSpawnCount();
        Transform parent = npcParent != null ? npcParent : transform;
        for (int i = 0; i < count; i++)
        {
            GameObject npcObject = CreateNpcObject(i, parent, CreateSpawnPosition(i, count));
            NpcEvacuationAgent agent = npcObject.GetComponent<NpcEvacuationAgent>();
            if (agent == null)
            {
                agent = npcObject.AddComponent<NpcEvacuationAgent>();
            }

            agent.ConfigureMovement(npcMoveSpeed, arrivalDistance);
            agent.EnsureNonBlockingPhysics();
            agent.ConfigureTargets(targets, true);

            if (addStateLabels && npcObject.GetComponent<NpcStateLabel>() == null)
            {
                NpcStateLabel label = npcObject.AddComponent<NpcStateLabel>();
                label.Bind(agent);
            }

            spawnedAgents.Add(agent);
        }

        return spawnedAgents.Count;
    }

    public void ClearSpawnedNpcs()
    {
        foreach (NpcEvacuationAgent agent in spawnedAgents)
        {
            if (agent != null)
            {
                DestroyObject(agent.gameObject);
            }
        }

        spawnedAgents.Clear();
    }

    public NpcEvacuationTargetInfo[] CollectTargetsFromScene()
    {
        if (explicitShelterTargets != null && explicitShelterTargets.Count > 0)
        {
            return CollectTargets(explicitShelterTargets);
        }

        if (!autoFindShelterTargets)
        {
            return new NpcEvacuationTargetInfo[0];
        }

        return CollectTargets(Object.FindObjectsOfType<BuildingShelter>());
    }

    public static NpcEvacuationTargetInfo[] CollectTargets(IList<BuildingShelter> shelters)
    {
        if (shelters == null || shelters.Count == 0)
        {
            return new NpcEvacuationTargetInfo[0];
        }

        var targets = new List<NpcEvacuationTargetInfo>();
        for (int i = 0; i < shelters.Count; i++)
        {
            NpcEvacuationTargetInfo target = NpcEvacuationTargetInfo.FromBuildingShelter(shelters[i]);
            if (target != null)
            {
                targets.Add(target);
            }
        }

        return targets.ToArray();
    }

    private int ResolveSpawnCount()
    {
        int safeMax = Mathf.Clamp(maxNpcCount, 0, HardMaxNpcCount);
        return Mathf.Clamp(npcCount, 0, safeMax);
    }

    private Vector3 CreateSpawnPosition(int index, int totalCount)
    {
        if (totalCount <= 1)
        {
            return transform.position + spawnOrigin;
        }

        int columns = Mathf.CeilToInt(Mathf.Sqrt(totalCount));
        int rows = Mathf.CeilToInt(totalCount / (float)columns);
        int column = index % columns;
        int row = index / columns;
        float x = columns <= 1 ? 0f : Mathf.Lerp(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f, column / (float)(columns - 1));
        float z = rows <= 1 ? 0f : Mathf.Lerp(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f, row / (float)(rows - 1));
        return transform.position + spawnOrigin + new Vector3(x, 0f, z);
    }

    private GameObject CreateNpcObject(int index, Transform parent, Vector3 position)
    {
        GameObject npcObject;
        if (npcPrefab != null)
        {
            npcObject = Instantiate(npcPrefab, position, Quaternion.identity, parent);
            npcObject.name = $"P6B_NPC_{index + 1:00}";
            return npcObject;
        }

        npcObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npcObject.name = $"P6B_NPC_{index + 1:00}";
        npcObject.transform.SetParent(parent, true);
        npcObject.transform.position = position;
        npcObject.transform.localScale = new Vector3(0.6f, 1.2f, 0.6f);
        RemoveCollider(npcObject);
        return npcObject;
    }

    private static void RemoveCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyObject(collider);
        }
    }

    private static void DestroyObject(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(target);
        }
        else
        {
            Object.DestroyImmediate(target);
        }
    }
}
