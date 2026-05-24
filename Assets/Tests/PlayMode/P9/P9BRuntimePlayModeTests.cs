using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P9BRuntimePlayModeTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [UnityTest]
    public IEnumerator WeightedSpawnRuntimeGeneratorCreatesTemporaryMarkers()
    {
        GameObject root = CreateObject("P9B_PlayMode_SpawnRoot");
        P9SpawnRuntimeGenerator generator = root.AddComponent<P9SpawnRuntimeGenerator>();
        P9BWeightedSpawnGenerationResult result = generator.Generate(
            P9BDataLoader.LoadSpawnZones().data,
            P9BDataLoader.LoadWeightedSpawnConfig().data,
            8,
            root.transform);

        yield return null;

        Assert.IsTrue(result.success, result.summary);
        Assert.AreEqual(8, result.generatedMarkerCount);
        Assert.IsFalse(result.affectsGameplaySuccessFailure);
        Assert.GreaterOrEqual(root.transform.childCount, 8);
    }

    [UnityTest]
    public IEnumerator HumanitarianRuntimeGeneratesAllAvailableP8ECandidateMarkers()
    {
        GameObject root = CreateObject("P9B_PlayMode_HumanitarianRoot");
        P9BHumanitarianCandidateMarkerCollection collection = P9BDataLoader.LoadP8HumanitarianCandidateMarkers().data;
        P9BHumanitarianMarkerGenerationResult result = P9HumanitarianCandidateMarkerRuntime.GenerateMarkers(
            collection,
            root.transform,
            0);

        yield return null;

        Assert.IsTrue(result.success, result.summary);
        Assert.AreEqual(110, result.generatedMarkerCount);
        Assert.AreEqual(28, result.namedMarkerCount);
        Assert.AreEqual(82, result.idOnlyMarkerCount);
        Assert.IsTrue(result.allCandidatesNonOfficial);
        Assert.IsTrue(result.allCandidatesRequireNonOfficialWarning);
        Assert.IsFalse(result.selectableGameplayEnabled);
        Assert.IsFalse(result.affectsGameplaySuccessFailure);
    }

    [UnityTest]
    public IEnumerator EntranceMarkersAndCrowdSpawnerRespectPrototypeBoundary()
    {
        GameObject root = CreateObject("P9B_PlayMode_CrowdRoot");
        P9EntranceSafeFloorProxyCollection targets = P9BDataLoader.LoadEntranceMarkerAssignments().data;
        P9BEntranceMarkerGenerationResult entranceResult = P9EntranceSafeFloorMarkerRuntime.GenerateMarkers(targets, root.transform);

        P9BWeightedSpawnSelectionResult selection = P9WeightedSpawnSelector.Select(
            P9BDataLoader.LoadSpawnZones().data,
            P9BDataLoader.LoadWeightedSpawnConfig().data,
            20);
        P9CrowdRuntimeSpawner spawner = root.AddComponent<P9CrowdRuntimeSpawner>();
        P9CrowdRuntimeSpawnResult crowdResult = spawner.Spawn(
            selection,
            targets,
            P9BDataLoader.LoadCrowdScenario().data,
            root.transform);

        yield return null;

        Assert.IsTrue(entranceResult.success, entranceResult.summary);
        Assert.AreEqual(4, entranceResult.generatedMarkerCount);
        Assert.IsTrue(crowdResult.success, crowdResult.summary);
        Assert.AreEqual(16, crowdResult.spawnedCount);
        Assert.AreEqual(16, crowdResult.metrics.spawnedCount);
        Assert.IsFalse(crowdResult.affectsGameplaySuccessFailure);
        Assert.IsFalse(crowdResult.metrics.affectsGameplaySuccessFailure);
        Assert.IsFalse(P9CrowdRuntimeSpawner.AffectsPlayerSuccessFailure);
        Assert.IsFalse(P9CrowdRuntimeSpawner.ClaimsRealCrowdModel);
    }

    [UnityTest]
    public IEnumerator CollapseDebrisRuntimeMarkersDoNotCauseOutcomeMutation()
    {
        GameObject root = CreateObject("P9B_PlayMode_CollapseRoot");
        P9BCollapseDebrisRiskZoneGenerationResult result = P9CollapseDebrisRiskZone.GenerateMarkers(
            P9BDataLoader.LoadCollapseDebrisRiskZones().data,
            root.transform);

        yield return null;

        Assert.IsTrue(result.success, result.summary);
        Assert.AreEqual(3, result.generatedZoneCount);
        Assert.IsFalse(result.affectsGameplaySuccessFailure);
        Assert.IsFalse(result.canKillPlayer);

        P9CollapseDebrisRiskZone zone = root.GetComponentInChildren<P9CollapseDebrisRiskZone>();
        P9CollapseDebrisExposureResult exposure = zone.CalculateExposure(zone.transform.position, 42);
        Assert.IsTrue(exposure.inZone);
        Assert.IsFalse(exposure.canKillPlayer);
        Assert.IsFalse(exposure.playerOutcomeMutationApplied);
    }

    [UnityTest]
    public IEnumerator SceneRuntimeBootstrapCreatesRuntimeRootWithoutProtectedSceneDependency()
    {
        GameObject root = CreateObject("P9B_PlayMode_Bootstrap");
        P9SceneRuntimeBootstrap bootstrap = root.AddComponent<P9SceneRuntimeBootstrap>();
        P9BScenarioRuntimeSummaryResult summary = bootstrap.GenerateRuntimePrototype();

        yield return null;

        Assert.IsNotNull(bootstrap.RuntimeRoot);
        Assert.IsTrue(summary.p8HandoffExpectedFilesPresent);
        Assert.Greater(summary.spawnMarkerCount, 0);
        Assert.AreEqual(4, summary.entranceMarkerCount);
        Assert.AreEqual(110, summary.humanitarianMarkerCount);
        Assert.AreEqual(3, summary.collapseDebrisMarkerCount);
        Assert.IsFalse(summary.routesAreOfficial);
        Assert.IsFalse(summary.affectsGameplaySuccessFailure);
        Assert.IsFalse(summary.implementsFinalFailureGameplay);
        Assert.IsFalse(P9SceneRuntimeBootstrap.RequiresChuoBaseMap);
        Assert.IsFalse(P9SceneRuntimeBootstrap.RequiresP7HighDetailScene);
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
