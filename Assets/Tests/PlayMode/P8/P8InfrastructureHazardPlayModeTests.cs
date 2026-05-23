using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class P8InfrastructureHazardPlayModeTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [UnityTest]
    public IEnumerator InfrastructureTargetProxyCanExistInTemporaryScene()
    {
        GameObject proxyObject = CreateObject("P8C_RoadHazardProxy_PlayMode");
        P8InfrastructureHazardProxy proxy = proxyObject.AddComponent<P8InfrastructureHazardProxy>();
        proxy.ConfigureForTests("road_proxy", P8InfrastructureCategory.Road, true, true, 0f, 0f);

        yield return null;

        P8InfrastructureHazardEvaluation evaluation = proxy.Evaluate(CreateLayer(100f, 0.8f, 0.7f), 300f);

        Assert.IsTrue(evaluation.success, evaluation.summary);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, proxy.CurrentState);
        Assert.IsTrue(proxy.IsProxyBased);
        Assert.IsFalse(P8InfrastructureHazardTarget.AffectsGameplaySuccessFailure);
    }

    [UnityTest]
    public IEnumerator HazardEvaluatorUpdatesTargetStateOverTime()
    {
        GameObject targetObject = CreateObject("P8C_TimeDrivenHazardTarget_PlayMode");
        P8InfrastructureHazardTarget target = targetObject.AddComponent<P8InfrastructureHazardTarget>();
        target.ConfigureForTests("entrance_proxy", P8InfrastructureCategory.Entrance, true, true, 0f, 0f);
        P8HazardLayerData layer = CreateLayer(500f, 0.6f, 0.2f);

        yield return null;

        P8InfrastructureHazardEvaluation before = target.Evaluate(layer, 100f);
        P8InfrastructureHazardEvaluation after = target.Evaluate(layer, 700f);

        Assert.AreEqual(P8RiskFrontContactPhase.BeforeFrontArrival, before.contactPhase);
        Assert.AreEqual(P8RiskFrontContactPhase.AfterFrontArrival, after.contactPhase);
        Assert.AreEqual(P8InfrastructureHazardState.Watch, before.state);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, after.state);
    }

    [UnityTest]
    public IEnumerator RiskFrontControllerAndInfrastructureEvaluatorCanCoexist()
    {
        GameObject frontObject = CreateObject("P8C_RiskFront_Coexistence_PlayMode");
        P8RiskFrontController frontController = frontObject.AddComponent<P8RiskFrontController>();
        GameObject targetObject = CreateObject("P8C_Infrastructure_Coexistence_PlayMode");
        P8InfrastructureHazardTarget target = targetObject.AddComponent<P8InfrastructureHazardTarget>();
        target.ConfigureForTests("waterfront_proxy", P8InfrastructureCategory.Waterfront, true, true, 0f, 0f);

        yield return null;

        P8HazardLayerLoadResult hazard = P8HazardLayerLoader.LoadSampleHazardLayer();
        P8RiskFrontConfigLoadResult config = P8HazardLayerLoader.LoadRiskFrontConfig();
        Assert.IsTrue(frontController.Initialize(hazard.data, config.config), frontController.LastStatus);

        P8InfrastructureHazardEvaluation evaluation = target.Evaluate(CreateLayer(100f, 1.2f, 0.8f), 300f);

        Assert.IsTrue(frontController.IsVisualVisible);
        Assert.IsTrue(evaluation.success, evaluation.summary);
        Assert.AreEqual(P8InfrastructureHazardState.AvoidProxy, evaluation.state);
        Assert.AreEqual(0, Object.FindObjectsOfType<EvacuationGameManager>().Length);
        Assert.AreEqual(0, Object.FindObjectsOfType<ResultPanelController>().Length);
    }

    [UnityTest]
    public IEnumerator NavigationTargetProxyWorksWithoutChuoBaseMap()
    {
        GameObject player = CreateObject("P8C_Navigation_Player");
        player.transform.position = Vector3.zero;
        GameObject targetObject = CreateObject("P8C_Navigation_TargetProxy");
        targetObject.transform.position = new Vector3(10f, 0f, 0f);
        P8InfrastructureHazardProxy proxy = targetObject.AddComponent<P8InfrastructureHazardProxy>();
        proxy.ConfigureForTests("navigation_target_proxy", P8InfrastructureCategory.NavigationTargetProxy, true, true, 0f, 0f);
        NavigationGuidanceController guidance = player.AddComponent<NavigationGuidanceController>();
        guidance.SetPlayerTransform(player.transform);
        guidance.SetTargetTransform(targetObject.transform);

        yield return null;

        NavigationGuidanceResult result = guidance.RefreshGuidance();
        string activeScenePath = SceneManager.GetActiveScene().path.Replace("\\", "/");

        Assert.AreNotEqual(P8SceneCompatibilityReport.GetLegacyFallbackScenePath(), activeScenePath);
        Assert.IsTrue(result.hasTarget);
        Assert.AreEqual("P8C_Navigation_TargetProxy", result.targetId);
        StringAssert.Contains(NavigationGuidanceCalculator.NotOfficialNavigationWarning, result.statusText);
        Assert.IsFalse(NavigationGuidanceController.AffectsGameplayRules);
    }

    [UnityTest]
    public IEnumerator ShelterInteractionProxyExistsWithoutChangingResultRules()
    {
        GameObject shelterProxyObject = CreateObject("P8C_ShelterInteractionProxy_PlayMode");
        P8InfrastructureHazardProxy proxy = shelterProxyObject.AddComponent<P8InfrastructureHazardProxy>();
        proxy.ConfigureForTests("shelter_proxy", P8InfrastructureCategory.ShelterProxy, true, true, 0f, 0f);

        yield return null;

        P8InfrastructureHazardEvaluation evaluation = proxy.Evaluate(CreateLayer(100f, 0.5f, 0.3f), 300f);

        Assert.IsTrue(evaluation.success, evaluation.summary);
        Assert.AreEqual(P8InfrastructureHazardState.Warning, evaluation.state);
        Assert.IsFalse(P8InfrastructureHazardProxy.AffectsGameplaySuccessFailure);
        Assert.AreEqual(0, Object.FindObjectsOfType<EvacuationGameManager>().Length);
        Assert.AreEqual(0, Object.FindObjectsOfType<ResultPanelController>().Length);
    }

    [UnityTest]
    public IEnumerator NpcPrototypeCanStageAgainstP8CProxyTargetWithoutP9Systems()
    {
        GameObject npcObject = CreateObject("P8C_NpcPrototype_Agent");
        GameObject targetObject = CreateObject("P8C_NpcPrototype_TargetProxy");
        targetObject.transform.position = new Vector3(5f, 0f, 0f);
        P8InfrastructureHazardProxy proxy = targetObject.AddComponent<P8InfrastructureHazardProxy>();
        proxy.ConfigureForTests("npc_navigation_proxy", P8InfrastructureCategory.NavigationTargetProxy, true, true, 0f, 0f);
        NpcEvacuationAgent agent = npcObject.AddComponent<NpcEvacuationAgent>();

        yield return null;

        agent.ConfigureTargets(
            new[]
            {
                new NpcEvacuationTargetInfo
                {
                    targetId = "npc_navigation_proxy",
                    displayName = "NPC Navigation Proxy",
                    position = targetObject.transform.position,
                    targetTransform = targetObject.transform,
                    canEnter = true,
                    isSelectable = true,
                    postEarthquakeStatus = "usable",
                    sourceType = "p8c_proxy"
                }
            },
            true);

        Assert.AreEqual(NpcEvacuationState.MovingToTarget, agent.CurrentState);
        Assert.IsFalse(NpcEvacuationAgent.AffectsPlayerSuccessFailure);
        Assert.IsFalse(P8InfrastructureHazardEvaluator.RequiresP9Systems);
    }

    [UnityTest]
    public IEnumerator P8CInfrastructureRequiresNoP9Systems()
    {
        yield return null;

        Assert.IsFalse(P8InfrastructureHazardEvaluator.RequiresP9Systems);
        Assert.IsFalse(P8InfrastructureHazardTarget.RequiresP9Systems);
        Assert.IsFalse(P8InfrastructureHazardEvaluator.ImplementsCollapseProxy);
        Assert.IsFalse(P8P2P6RuntimeCompatibilityInspector.RequiresP9Systems);
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

    private static P8HazardLayerData CreateLayer(float arrivalTimeSeconds, float depthMeters, float hazardIntensity)
    {
        return new P8HazardLayerData
        {
            scenarioId = "p8c_playmode_test_layer",
            sourceMode = "official_tsunami_metropolitan",
            hazardLayerVersion = "p8c_playmode.1",
            timeOriginSeconds = 0f,
            features = new[]
            {
                new P8HazardFeature
                {
                    featureId = "p8c_playmode_test_feature",
                    sourceMode = "official_tsunami_metropolitan",
                    sourceCategory = "official_tsunami_metropolitan",
                    geometryType = "grid",
                    extractionStatus = "extracted",
                    spatialExtractionStatus = "extracted",
                    arrivalTimeSeconds = arrivalTimeSeconds,
                    inundationDepthMeters = depthMeters,
                    maxInundationDepthMeters = depthMeters,
                    waterLevelMeters = depthMeters,
                    tsunamiHeightMeters = 2f,
                    maxTsunamiHeightMeters = 2f,
                    inundationDepthStatus = "extracted_spatial_test",
                    boundaryStatus = "prototype_test_boundary",
                    inundationBoundary = new[]
                    {
                        new P8BoundaryPoint { x = -1f, y = -1f },
                        new P8BoundaryPoint { x = 1f, y = -1f },
                        new P8BoundaryPoint { x = 1f, y = 1f },
                        new P8BoundaryPoint { x = -1f, y = 1f }
                    },
                    spatialSamples = new[]
                    {
                        new P8HazardSpatialSample
                        {
                            xMeters = 0f,
                            yMeters = 0f,
                            longitude = 0f,
                            latitude = 0f,
                            inundationDepthMeters = depthMeters,
                            tsunamiHeightMeters = 2f
                        }
                    },
                    spatialSampleCount = 1,
                    hazardIntensity = hazardIntensity,
                    confidence = 0.9f,
                    evidenceSourceId = "tokyo_damage_estimation_map_tsunami",
                    visualHeightMeters = 1000f,
                    visualHeightIsCinematicOnly = true,
                    boundaryIsEvidenceBasedOrPrototype = "evidence_based",
                    affectedInfrastructureTypes = new P8AffectedInfrastructureTypes
                    {
                        roads = true,
                        buildings = true,
                        bridges = true,
                        underground = true,
                        entrances = true,
                        waterfront = true,
                        open_space = true,
                        shelter_proxy = true,
                        navigation_target_proxy = true
                    },
                    buildingDamageState = "none",
                    collapseProxyState = "data_only",
                    collapseProbability = 0f,
                    collapseRandomSeed = 1,
                    hazardDrivenCollapse = false
                }
            }
        };
    }
}
