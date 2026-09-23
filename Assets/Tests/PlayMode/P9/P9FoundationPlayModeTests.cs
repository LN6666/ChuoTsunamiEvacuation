using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class P9FoundationPlayModeTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [UnityTest]
    public IEnumerator SpawnPointComponentCanExistInTemporaryScene()
    {
        GameObject spawnObject = CreateObject("P9A_TemporarySpawnPoint");
        P9SpawnPointData spawnPoint = spawnObject.AddComponent<P9SpawnPointData>();
        spawnPoint.ConfigureForTests("p9_playmode_spawn", "test_origin", "test", 3, 1f, 9010);

        yield return null;

        P9SpawnPointSnapshot snapshot = spawnPoint.CreateSnapshot();
        Assert.AreEqual("p9_playmode_spawn", snapshot.spawnPointId);
        Assert.AreEqual("test_origin", snapshot.spawnType);
        Assert.AreEqual(3, snapshot.capacity);
        Assert.IsFalse(P9SpawnPointData.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P9SpawnPointData.RequiresChuoBaseMap);
    }

    [UnityTest]
    public IEnumerator EntranceProxyCanExistInTemporaryScene()
    {
        GameObject proxyObject = CreateObject("P9A_TemporaryEntranceProxy");
        P9EntranceSafeFloorProxy proxy = proxyObject.AddComponent<P9EntranceSafeFloorProxy>();
        proxy.ConfigureForTests("p9_playmode_humanitarian_proxy", false, true, true, "warning");

        yield return null;

        P9EvacuationProxyState state = proxy.CreateNoEffectState(2, 4);
        Assert.AreEqual(P9VerticalEvacuationStatus.Warning, proxy.VerticalEvacuationStatus);
        Assert.IsTrue(proxy.RequiresNonOfficialWarning);
        Assert.IsTrue(proxy.HumanitarianCandidateRemainsNonOfficial);
        Assert.AreEqual(P9EntranceCongestionState.Queueing, state.congestionState);
        Assert.IsFalse(state.affectsGameplaySuccessFailure);
        Assert.IsFalse(P9EntranceSafeFloorProxy.ImplementsIndoorSceneGameplay);
    }

    [UnityTest]
    public IEnumerator SimpleCrowdAgentProxyCanBeInstantiatedSafely()
    {
        GameObject agentObject = CreateObject("P9A_TemporaryCrowdAgentProxy");
        P9CrowdAgentProfile profile = agentObject.AddComponent<P9CrowdAgentProfile>();
        profile.ConfigureForTests(
            "p9_playmode_agent",
            "test_agent",
            1.1f,
            "nearest_available_proxy_entrance",
            "direct_to_entrance_proxy_no_failure_effect",
            0.35f,
            0.1f);

        yield return null;

        Assert.AreEqual("p9_playmode_agent", profile.AgentProfileId);
        Assert.AreEqual("test_agent", profile.AgentType);
        Assert.AreEqual(1.1f, profile.SpeedMetersPerSecond, 0.0001f);
        Assert.IsFalse(P9CrowdAgentProfile.AffectsPlayerSuccessFailure);
        Assert.IsFalse(P9CrowdAgentProfile.ClaimsRealCrowdModel);
    }

    [UnityTest]
    public IEnumerator FoundationHasNoDependencyOnChuoBaseMapOrP7HighDetailScene()
    {
        GameObject configObject = CreateObject("P9A_TemporaryCrowdConfig");
        P9CrowdSimulationConfig config = configObject.AddComponent<P9CrowdSimulationConfig>();

        yield return null;

        P9CrowdSimulationConfigSnapshot snapshot = config.CreateSnapshot();
        string activeScenePath = SceneManager.GetActiveScene().path.Replace("\\", "/");

        Assert.IsFalse(activeScenePath.Contains("Chuo_BaseMap"));
        Assert.IsFalse(activeScenePath.Contains("P7_HighDetail_Chuo"));
        Assert.IsFalse(P9RuntimePolicy.RequiresChuoBaseMap);
        Assert.IsFalse(P9RuntimePolicy.RequiresP7HighDetailScene);
        Assert.IsFalse(snapshot.finalFailureEnabledInP9A);
        Assert.AreEqual(0, Object.FindObjectsByType<EvacuationGameManager>(FindObjectsSortMode.None).Length);
        Assert.AreEqual(0, Object.FindObjectsByType<ResultPanelController>(FindObjectsSortMode.None).Length);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            if (createdObjects[i] != null)
            {
                Object.Destroy(createdObjects[i]);
            }
        }

        createdObjects.Clear();
        yield return null;
    }

    private GameObject CreateObject(string name)
    {
        var gameObject = new GameObject(name);
        createdObjects.Add(gameObject);
        return gameObject;
    }
}
