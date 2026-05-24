using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class P8InfrastructureHazardEvaluatorTests
{
    [Test]
    public void EvaluatorUsesOfficialEvidenceHazardLayerValues()
    {
        P8HazardLayerLoadResult loaded = P8HazardLayerLoader.LoadSampleHazardLayer();
        Assert.IsTrue(loaded.success, string.Join("\n", loaded.validation.errorsArray));

        P8HazardFeature feature = loaded.data.features[0];
        P8HazardSpatialSample sample = feature.spatialSamples[feature.spatialSamples.Length / 2];
        P8InfrastructureHazardEvaluationInput input =
            P8InfrastructureHazardEvaluationInput.FromHazardCoordinates(
                "official_road_proxy",
                P8InfrastructureCategory.Road,
                sample.longitude,
                sample.latitude,
                true);

        P8InfrastructureHazardEvaluation result =
            P8InfrastructureHazardEvaluator.Evaluate(input, loaded.data, feature.arrivalTimeSeconds);

        Assert.IsTrue(result.success, result.summary);
        Assert.AreEqual("official_tsunami_metropolitan", result.sourceMode);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.evidenceSourceId));
        Assert.AreEqual(feature.featureId, result.selectedFeatureId);
        Assert.AreEqual(sample.inundationDepthMeters, result.inundationDepthMeters, 0.0001f);
        Assert.AreEqual(feature.maxTsunamiHeightMeters, result.maxTsunamiHeightMeters, 0.0001f);
        Assert.AreEqual(P8RiskFrontContactPhase.AtRiskFrontContact, result.contactPhase);
        Assert.AreNotEqual(P8InfrastructureHazardState.Safe, result.state);
        StringAssert.Contains("inundationDepthMeters", result.summary);
        StringAssert.Contains("visualHeightMeters is cinematic only", result.summary);
    }

    [Test]
    public void RoadBuildingBridgeUndergroundAndEntranceTransitionAfterArrival()
    {
        P8HazardLayerData layer = CreateLayer(600f, 1.5f, 0.8f);

        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, EvaluateCategory(layer, P8InfrastructureCategory.Road).state);
        Assert.AreEqual(P8InfrastructureHazardState.InundatedProxy, EvaluateCategory(layer, P8InfrastructureCategory.Building).state);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, EvaluateCategory(layer, P8InfrastructureCategory.Bridge).state);
        Assert.AreEqual(P8InfrastructureHazardState.AvoidProxy, EvaluateCategory(layer, P8InfrastructureCategory.Underground).state);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, EvaluateCategory(layer, P8InfrastructureCategory.Entrance).state);
    }

    [Test]
    public void HumanitarianCandidateProxyReceivesHazardStateWithoutOfficialShelterClaim()
    {
        P8HazardLayerData layer = CreateLayer(100f, 0.8f, 0.5f);
        P8InfrastructureHazardEvaluation candidate =
            P8InfrastructureHazardEvaluator.Evaluate(
                P8InfrastructureHazardEvaluationInput.FromHazardCoordinates(
                    "p5g_non_official_candidate",
                    P8InfrastructureCategory.HumanitarianCandidateProxy,
                    0f,
                    0f,
                    true),
                layer,
                300f);
        P8InfrastructureHazardEvaluation marker =
            P8InfrastructureHazardEvaluator.Evaluate(
                P8InfrastructureHazardEvaluationInput.FromHazardCoordinates(
                    "p5g_highrise_marker",
                    P8InfrastructureCategory.HighriseCandidateMarker,
                    0f,
                    0f,
                    true),
                layer,
                300f);

        Assert.IsTrue(candidate.success, candidate.summary);
        Assert.IsTrue(marker.success, marker.summary);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, candidate.state);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, marker.state);
        Assert.IsTrue(candidate.proxyBased);
        Assert.IsTrue(marker.proxyBased);
        StringAssert.Contains("humanitarian_candidate_proxy", candidate.summary);
        StringAssert.Contains("highrise_candidate_marker", marker.summary);
        StringAssert.Contains("no gameplay success/failure rule changes", candidate.summary);
        Assert.IsFalse(P8InfrastructureHazardEvaluator.AffectsGameplaySuccessFailure);
    }

    [Test]
    public void ArrivalTimeSecondsAffectsContactPhaseAndState()
    {
        P8HazardLayerData layer = CreateLayer(600f, 0.6f, 0.4f);
        P8InfrastructureHazardEvaluationInput input = CreateInput(P8InfrastructureCategory.Road);

        P8InfrastructureHazardEvaluation before = P8InfrastructureHazardEvaluator.Evaluate(input, layer, 100f);
        P8InfrastructureHazardEvaluation contact = P8InfrastructureHazardEvaluator.Evaluate(input, layer, 590f);
        P8InfrastructureHazardEvaluation after = P8InfrastructureHazardEvaluator.Evaluate(input, layer, 900f);

        Assert.AreEqual(P8RiskFrontContactPhase.BeforeFrontArrival, before.contactPhase);
        Assert.AreEqual(P8RiskFrontContactPhase.AtRiskFrontContact, contact.contactPhase);
        Assert.AreEqual(P8RiskFrontContactPhase.AfterFrontArrival, after.contactPhase);
        Assert.AreEqual(P8InfrastructureHazardState.Watch, before.state);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, contact.state);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, after.state);
    }

    [Test]
    public void InundationDepthMetersAffectsState()
    {
        P8HazardLayerData dryLayer = CreateLayer(100f, 0f, 0f);
        P8HazardLayerData wetLayer = CreateLayer(100f, 0.7f, 0f);
        P8InfrastructureHazardEvaluationInput input = CreateInput(P8InfrastructureCategory.Building);

        P8InfrastructureHazardEvaluation dry = P8InfrastructureHazardEvaluator.Evaluate(input, dryLayer, 300f);
        P8InfrastructureHazardEvaluation wet = P8InfrastructureHazardEvaluator.Evaluate(input, wetLayer, 300f);

        Assert.AreEqual(P8InfrastructureHazardState.Safe, dry.state);
        Assert.AreEqual(P8InfrastructureHazardState.Warning, wet.state);
    }

    [Test]
    public void HazardIntensityAffectsState()
    {
        P8HazardLayerData low = CreateLayer(100f, 0f, 0.05f);
        P8HazardLayerData high = CreateLayer(100f, 0f, 0.8f);
        P8InfrastructureHazardEvaluationInput input = CreateInput(P8InfrastructureCategory.Road);

        P8InfrastructureHazardEvaluation lowResult = P8InfrastructureHazardEvaluator.Evaluate(input, low, 300f);
        P8InfrastructureHazardEvaluation highResult = P8InfrastructureHazardEvaluator.Evaluate(input, high, 300f);

        Assert.AreEqual(P8InfrastructureHazardState.Safe, lowResult.state);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, highResult.state);
    }

    [Test]
    public void MaxTsunamiHeightDoesNotReplaceInundationDepth()
    {
        P8HazardLayerData layer = CreateLayer(100f, 0f, 0f, maxTsunamiHeightMeters: 99f, visualHeightMeters: 1f);
        P8InfrastructureHazardEvaluation result =
            P8InfrastructureHazardEvaluator.Evaluate(CreateInput(P8InfrastructureCategory.Road), layer, 300f);

        Assert.AreEqual(0f, result.inundationDepthMeters, 0.0001f);
        Assert.AreEqual(99f, result.maxTsunamiHeightMeters, 0.0001f);
        Assert.AreEqual(P8InfrastructureHazardState.Safe, result.state);
    }

    [Test]
    public void VisualHeightMetersDoesNotAffectPhysicalHazardState()
    {
        P8HazardLayerData lowVisual = CreateLayer(100f, 0f, 0f, maxTsunamiHeightMeters: 1f, visualHeightMeters: 1f);
        P8HazardLayerData highVisual = CreateLayer(100f, 0f, 0f, maxTsunamiHeightMeters: 1f, visualHeightMeters: 1000f);
        P8InfrastructureHazardEvaluationInput input = CreateInput(P8InfrastructureCategory.Entrance);

        P8InfrastructureHazardEvaluation low = P8InfrastructureHazardEvaluator.Evaluate(input, lowVisual, 300f);
        P8InfrastructureHazardEvaluation high = P8InfrastructureHazardEvaluator.Evaluate(input, highVisual, 300f);

        Assert.AreEqual(low.state, high.state);
        Assert.AreEqual(P8InfrastructureHazardState.Safe, high.state);
    }

    [Test]
    public void MissingDataFailsSafeWithoutGameplayCoupling()
    {
        P8InfrastructureHazardEvaluation result =
            P8InfrastructureHazardEvaluator.Evaluate(CreateInput(P8InfrastructureCategory.Underground), null, 10f);

        Assert.IsFalse(result.success);
        Assert.IsTrue(result.failSafe);
        Assert.AreEqual(P8InfrastructureHazardState.Safe, result.state);
        Assert.IsTrue(P8InfrastructureHazardEvaluator.IsGameplayNeutral());
        Assert.IsFalse(P8InfrastructureHazardEvaluator.AffectsGameplaySuccessFailure);
    }

    [Test]
    public void P8CScriptsDoNotReferenceGameplaySuccessFailureControllers()
    {
        string scriptRoot = Path.Combine(Application.dataPath, "Scripts", "P8");
        string[] p8cScripts =
        {
            "P8InfrastructureHazardEvaluator.cs",
            "P8InfrastructureHazardTarget.cs",
            "P8InfrastructureHazardProxy.cs",
            "P8InfrastructureHazardMarker.cs",
            "P8InfrastructureHazardDebugSummary.cs"
        };

        foreach (string script in p8cScripts)
        {
            string source = File.ReadAllText(Path.Combine(scriptRoot, script));
            Assert.IsFalse(source.Contains("EvacuationGameManager"), script);
            Assert.IsFalse(source.Contains("ResultPanelController"), script);
            Assert.IsFalse(source.Contains("ShelterEntranceTrigger"), script);
            Assert.IsFalse(source.Contains("BuildingShelter"), script);
        }
    }

    [Test]
    public void P2P6CompatibilityInspectorReturnsStructuredResults()
    {
        P8RuntimeCompatibilityResult[] results = P8P2P6RuntimeCompatibilityInspector.Inspect();

        CollectionAssert.Contains(results.Select(result => result.phase).Distinct().ToArray(), "P2");
        CollectionAssert.Contains(results.Select(result => result.phase).Distinct().ToArray(), "P6");
        Assert.IsTrue(results.Any(result => result.status == P8RuntimeAdaptationStatus.Passed));
        Assert.IsTrue(results.Any(result => result.status == P8RuntimeAdaptationStatus.ProxyBased));
        Assert.IsFalse(results.Any(result => result.status == P8RuntimeAdaptationStatus.Blocked));
        Assert.IsFalse(P8P2P6RuntimeCompatibilityInspector.AffectsGameplaySuccessFailure);
        StringAssert.Contains("No gameplay success/failure rule changes", P8P2P6RuntimeCompatibilityInspector.CreateSummary());
    }

    private static P8InfrastructureHazardEvaluation EvaluateCategory(
        P8HazardLayerData layer,
        P8InfrastructureCategory category)
    {
        return P8InfrastructureHazardEvaluator.Evaluate(CreateInput(category), layer, 900f);
    }

    private static P8InfrastructureHazardEvaluationInput CreateInput(P8InfrastructureCategory category)
    {
        return P8InfrastructureHazardEvaluationInput.FromHazardCoordinates("target_" + category, category, 0f, 0f, true);
    }

    private static P8HazardLayerData CreateLayer(
        float arrivalTimeSeconds,
        float depthMeters,
        float hazardIntensity,
        float maxTsunamiHeightMeters = 2f,
        float visualHeightMeters = 1000f)
    {
        return new P8HazardLayerData
        {
            scenarioId = "p8c_test_layer",
            sourceMode = "official_tsunami_metropolitan",
            hazardLayerVersion = "p8c_test.1",
            timeOriginSeconds = 0f,
            features = new[]
            {
                new P8HazardFeature
                {
                    featureId = "p8c_test_feature",
                    sourceMode = "official_tsunami_metropolitan",
                    sourceCategory = "official_tsunami_metropolitan",
                    geometryType = "grid",
                    extractionStatus = "extracted",
                    spatialExtractionStatus = "extracted",
                    arrivalTimeSeconds = arrivalTimeSeconds,
                    inundationDepthMeters = depthMeters,
                    maxInundationDepthMeters = depthMeters,
                    waterLevelMeters = depthMeters,
                    tsunamiHeightMeters = maxTsunamiHeightMeters,
                    maxTsunamiHeightMeters = maxTsunamiHeightMeters,
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
                            tsunamiHeightMeters = maxTsunamiHeightMeters
                        }
                    },
                    spatialSampleCount = 1,
                    hazardIntensity = hazardIntensity,
                    confidence = 0.9f,
                    evidenceSourceId = "tokyo_damage_estimation_map_tsunami",
                    visualHeightMeters = visualHeightMeters,
                    visualHeightIsCinematicOnly = visualHeightMeters > P8HazardDataValidator.CinematicHeightThresholdMeters,
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
                        navigation_target_proxy = true,
                        humanitarian_candidate_proxy = true,
                        highrise_candidate_marker = true
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
