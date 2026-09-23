using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NpcEvacuationPlayModeTests
{
    private readonly List<GameObject> objectsToDestroy = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject objectToDestroy in objectsToDestroy)
        {
            if (objectToDestroy != null)
            {
                Object.Destroy(objectToDestroy);
            }
        }

        objectsToDestroy.Clear();
        yield return null;
    }

    [UnityTest]
    public IEnumerator SmallNpcGroupCanSpawnInTestScene()
    {
        NpcEvacuationSpawner spawner = CreateSpawner("P6B_PlayMode_Spawner");
        spawner.ConfigureForTests(5, Vector3.zero, new Vector2(4f, 4f));

        int spawnedCount = spawner.Spawn(CreateTargets(new Vector3(8f, 0f, 0f)));
        yield return null;

        Assert.AreEqual(5, spawnedCount);
        Assert.AreEqual(5, spawner.SpawnedAgents.Length);
        foreach (NpcEvacuationAgent agent in spawner.SpawnedAgents)
        {
            Assert.NotNull(agent);
            Assert.AreEqual(NpcEvacuationState.MovingToTarget, agent.CurrentState);
            AssertNoEnabledColliders(agent.gameObject);
        }
    }

    [UnityTest]
    public IEnumerator NpcMovesTowardTargetOverFrames()
    {
        GameObject npcObject = CreateTrackedObject("P6B_PlayMode_MovingNpc");
        NpcEvacuationAgent agent = npcObject.AddComponent<NpcEvacuationAgent>();
        agent.ConfigureMovement(2f, 0.1f);
        agent.ConfigureTargets(CreateTargets(new Vector3(6f, 0f, 0f)), true);
        float startX = npcObject.transform.position.x;

        yield return null;
        agent.Tick(0.5f);

        Assert.Greater(npcObject.transform.position.x, startX);
        Assert.AreEqual(NpcEvacuationState.MovingToTarget, agent.CurrentState);
    }

    [UnityTest]
    public IEnumerator NpcReachesArrivedNearTarget()
    {
        GameObject npcObject = CreateTrackedObject("P6B_PlayMode_ArrivingNpc");
        NpcEvacuationAgent agent = npcObject.AddComponent<NpcEvacuationAgent>();
        agent.ConfigureMovement(4f, 0.1f);
        agent.ConfigureTargets(CreateTargets(new Vector3(1f, 0f, 0f)), true);

        agent.Tick(0.5f);
        yield return null;

        Assert.AreEqual(NpcEvacuationState.Arrived, agent.CurrentState);
        Assert.AreEqual(1f, npcObject.transform.position.x, 0.001f);
    }

    [UnityTest]
    public IEnumerator SpawnedNpcHasNoBlockingCollider()
    {
        NpcEvacuationSpawner spawner = CreateSpawner("P6B_PlayMode_ColliderSpawner");
        spawner.ConfigureForTests(1, Vector3.zero, Vector2.zero);

        spawner.Spawn(CreateTargets(new Vector3(3f, 0f, 0f)));
        yield return null;

        Assert.AreEqual(1, spawner.SpawnedAgents.Length);
        AssertNoEnabledColliders(spawner.SpawnedAgents[0].gameObject);
    }

    [UnityTest]
    public IEnumerator ZeroTargetsDoNotThrowAndAgentsFailSafely()
    {
        NpcEvacuationSpawner spawner = CreateSpawner("P6B_PlayMode_ZeroTargetSpawner");
        spawner.ConfigureForTests(3, Vector3.zero, new Vector2(2f, 2f));

        int spawnedCount = spawner.Spawn(new List<NpcEvacuationTargetInfo>());
        yield return null;

        Assert.AreEqual(3, spawnedCount);
        foreach (NpcEvacuationAgent agent in spawner.SpawnedAgents)
        {
            Assert.AreEqual(NpcEvacuationState.FailedNoTarget, agent.CurrentState);
        }
    }

    private NpcEvacuationSpawner CreateSpawner(string name)
    {
        GameObject spawnerObject = CreateTrackedObject(name);
        return spawnerObject.AddComponent<NpcEvacuationSpawner>();
    }

    private GameObject CreateTrackedObject(string name)
    {
        GameObject gameObject = new GameObject(name);
        objectsToDestroy.Add(gameObject);
        return gameObject;
    }

    private static List<NpcEvacuationTargetInfo> CreateTargets(Vector3 targetPosition)
    {
        return new List<NpcEvacuationTargetInfo>
        {
            new NpcEvacuationTargetInfo
            {
                targetId = "playmode_target",
                displayName = "PlayMode Target",
                position = targetPosition,
                canEnter = true,
                isSelectable = true,
                postEarthquakeStatus = "usable",
                isOfficialShelter = true
            }
        };
    }

    private static void AssertNoEnabledColliders(GameObject gameObject)
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            Assert.IsFalse(collider.enabled, $"NPC collider should be disabled: {collider.name}");
        }
    }
}
