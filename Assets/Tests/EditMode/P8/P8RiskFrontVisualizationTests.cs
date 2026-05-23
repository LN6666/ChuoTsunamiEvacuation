using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class P8RiskFrontVisualizationTests
{
    [Test]
    public void RiskFrontConfigLoadsForP8BVisualization()
    {
        P8RiskFrontConfigLoadResult result = P8HazardLayerLoader.LoadRiskFrontConfig();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        Assert.IsFalse(result.config.riskFrontEnabledInP8A);
        Assert.IsTrue(result.config.riskFrontEnabledInP8B);
        Assert.Greater(result.config.visualHeightMeters, P8HazardDataValidator.CinematicHeightThresholdMeters);
        Assert.IsTrue(result.config.visualHeightIsCinematicOnly);

        P8RiskFrontVisualConfig visualConfig = P8RiskFrontVisualConfig.FromRiskFrontConfig(result.config);
        Assert.IsTrue(visualConfig.isValid, visualConfig.validationMessage);
        Assert.IsTrue(visualConfig.visualEnabled);
        StringAssert.Contains("not physical tsunami height", visualConfig.disclaimer);
    }

    [Test]
    public void LargeVisualHeightWithoutCinematicFlagFailsValidation()
    {
        P8RiskFrontConfig config = CreateValidRiskFrontConfig();
        config.visualHeightMeters = 1000f;
        config.visualHeightIsCinematicOnly = false;

        P8HazardValidationResult result = P8HazardDataValidator.ValidateRiskFrontConfig(config);

        Assert.IsFalse(result.isValid);
        Assert.IsTrue(result.isFailSafe);
        StringAssert.Contains("visualHeightIsCinematicOnly=true", string.Join("\n", result.errorsArray));
    }

    [Test]
    public void CurveGeneratorProducesNonEmptyNonLinearVisualBoundary()
    {
        P8HazardLayerData data = P8HazardLayerLoader.LoadSampleHazardLayer().data;
        P8RiskFrontConfig config = P8HazardLayerLoader.LoadRiskFrontConfig().config;
        P8RiskFrontVisualConfig visualConfig = P8RiskFrontVisualConfig.FromRiskFrontConfig(config);

        P8RiskFrontCurveResult result = P8RiskFrontCurveGenerator.Generate(data, visualConfig, 900f);

        Assert.IsTrue(result.success, result.summary);
        Assert.GreaterOrEqual(result.visualBoundary.Length, 3);
        Assert.Greater(result.dataBoundary.Length, 0);

        float firstZ = result.visualBoundary[0].z;
        bool hasDifferentZ = result.visualBoundary.Any(point => Mathf.Abs(point.z - firstZ) > 0.001f);
        Assert.IsTrue(hasDifferentZ, "Visual boundary should be non-linear/wavy.");
    }

    [Test]
    public void HazardLayerV1ParsesMultipleArrivalBoundaryRecords()
    {
        P8HazardLayerLoadResult result = P8HazardLayerLoader.LoadSampleHazardLayer();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        Assert.AreEqual("p8b.1.0", result.data.hazardLayerVersion);
        Assert.GreaterOrEqual(result.data.features.Length, 3);
        Assert.IsTrue(result.data.features.All(feature => feature.arrivalTimeSeconds > 0f));
        Assert.IsTrue(result.data.features.All(feature => feature.inundationBoundary.Length >= 1));
        Assert.IsTrue(result.data.features.All(feature => !string.IsNullOrWhiteSpace(feature.evidenceSourceId)));
        Assert.IsTrue(result.data.features.Any(feature => feature.geometryType == "polyline"));
        Assert.IsTrue(result.data.features.Any(feature => feature.geometryType == "polygon"));
    }

    [Test]
    public void RiskFrontSelectsHazardFeatureByArrivalTime()
    {
        P8HazardLayerData data = P8HazardLayerLoader.LoadSampleHazardLayer().data;
        P8RiskFrontVisualConfig visualConfig = P8RiskFrontVisualConfig.FromRiskFrontConfig(P8HazardLayerLoader.LoadRiskFrontConfig().config);

        P8RiskFrontCurveResult early = P8RiskFrontCurveGenerator.Generate(data, visualConfig, 1100f);
        P8RiskFrontCurveResult middle = P8RiskFrontCurveGenerator.Generate(data, visualConfig, 1300f);
        P8RiskFrontCurveResult late = P8RiskFrontCurveGenerator.Generate(data, visualConfig, 2500f);

        Assert.IsTrue(early.success, early.summary);
        Assert.IsTrue(middle.success, middle.summary);
        Assert.IsTrue(late.success, late.summary);
        Assert.AreEqual("p8b_sumida_riverfront_front_v1_manual_sample", early.selectedFeatureId);
        Assert.AreEqual("p8b_harumi_waterfront_front_v1_manual_sample", middle.selectedFeatureId);
        Assert.AreEqual("p8b_tsukishima_inland_front_v1_manual_sample", late.selectedFeatureId);
        Assert.AreEqual(P8RiskFrontCurveGenerator.FrontDriverSource, early.frontDriverSource);
    }

    [Test]
    public void RiskFrontFallsBackSafelyWhenBoundaryIsMissing()
    {
        P8HazardFeature feature = CreateHazardFeature("missing_boundary", 600f, 0.4f, 0.25f);
        feature.inundationBoundary = new P8BoundaryPoint[0];
        P8HazardLayerData data = CreateLayerWithFeature(feature);
        P8RiskFrontVisualConfig visualConfig = P8RiskFrontVisualConfig.FromRiskFrontConfig(CreateValidRiskFrontConfig());

        P8RiskFrontCurveResult result = P8RiskFrontCurveGenerator.Generate(data, visualConfig, 300f);

        Assert.IsTrue(result.success, result.summary);
        Assert.IsTrue(result.usedFallbackBoundary);
        Assert.GreaterOrEqual(result.visualBoundary.Length, 2);
        StringAssert.Contains("fallback_procedural_boundary", result.summary);
    }

    [Test]
    public void DepthAndIntensityAffectVisualWarningLevel()
    {
        P8RiskFrontVisualConfig visualConfig = P8RiskFrontVisualConfig.FromRiskFrontConfig(CreateValidRiskFrontConfig());
        P8RiskFrontCurveResult low = P8RiskFrontCurveGenerator.Generate(
            CreateLayerWithFeature(CreateHazardFeature("low", 600f, 0.2f, 0.15f)),
            visualConfig,
            100f);
        P8RiskFrontCurveResult high = P8RiskFrontCurveGenerator.Generate(
            CreateLayerWithFeature(CreateHazardFeature("high", 600f, 2.2f, 0.9f)),
            visualConfig,
            100f);

        Assert.IsTrue(low.success, low.summary);
        Assert.IsTrue(high.success, high.summary);
        Assert.Less(low.visualIntensity01, high.visualIntensity01);
        Assert.AreEqual("low", low.warningLevel);
        Assert.AreEqual("high", high.warningLevel);
        StringAssert.Contains("inundationDepthMeters", high.summary);
        StringAssert.Contains("hazardIntensity", high.summary);
    }

    [Test]
    public void MissingHazardDataFailsSafe()
    {
        P8RiskFrontVisualConfig visualConfig = P8RiskFrontVisualConfig.FromRiskFrontConfig(CreateValidRiskFrontConfig());

        P8RiskFrontCurveResult result = P8RiskFrontCurveGenerator.Generate(null, visualConfig, 10f);

        Assert.IsFalse(result.success);
        Assert.IsTrue(result.failSafe);
        StringAssert.Contains("Missing hazard data", result.summary);
    }

    [Test]
    public void VisualAndDataLayerSeparationIsEnforced()
    {
        P8RiskFrontConfig config = P8HazardLayerLoader.LoadRiskFrontConfig().config;

        CollectionAssert.Contains(config.scienceLayerFields, "arrivalTimeSeconds");
        CollectionAssert.Contains(config.scienceLayerFields, "inundationDepthMeters");
        CollectionAssert.Contains(config.visualLayerFields, "visualHeightMeters");
        CollectionAssert.DoesNotContain(config.scienceLayerFields, "visualHeightMeters");
        CollectionAssert.DoesNotContain(config.visualLayerFields, "arrivalTimeSeconds");

        P8RiskFrontVisualConfig visualConfig = P8RiskFrontVisualConfig.FromRiskFrontConfig(config);
        Assert.IsTrue(visualConfig.visualHeightIsCinematicOnly);
        Assert.AreEqual("prototype", visualConfig.boundaryIsEvidenceBasedOrPrototype);
        Assert.IsFalse(config.scienceLayerFields.Contains("visualHeightMeters"));
        Assert.IsFalse(config.visualLayerFields.Contains("tsunamiHeightMeters"));
        Assert.IsFalse(config.visualLayerFields.Contains("waterLevelMeters"));
    }

    [Test]
    public void OfficialClaimRequiresReviewedMetadataAndAllowedSourceMode()
    {
        P8HazardLayerData data = CreateLayerWithFeature(CreateHazardFeature("official_like", 600f, 0.4f, 0.25f), "evidence_planned");
        data.features[0].evidenceSourceId = string.Empty;

        P8HazardValidationResult missingEvidence = P8HazardDataValidator.ValidateHazardLayer(data);

        Assert.IsFalse(missingEvidence.isValid);
        Assert.IsTrue(missingEvidence.isFailSafe);
        StringAssert.Contains("evidenceSourceId is required", string.Join("\n", missingEvidence.errorsArray));

        data = CreateLayerWithFeature(CreateHazardFeature("official_like", 600f, 0.4f, 0.25f), "official_claim_without_review");
        data.features[0].sourceMode = "official_claim_without_review";
        data.evidenceSources[0].sourceMode = "official_claim_without_review";

        P8HazardValidationResult officialMode = P8HazardDataValidator.ValidateHazardLayer(data);

        Assert.IsFalse(officialMode.isValid);
        Assert.IsTrue(officialMode.isFailSafe);
        StringAssert.Contains("sourceMode", string.Join("\n", officialMode.errorsArray));
    }

    [Test]
    public void P8BRuntimeScriptsDoNotReferenceGameplaySuccessFailureControllers()
    {
        string scriptRoot = Path.Combine(Application.dataPath, "Scripts", "P8");
        string[] p8bScripts =
        {
            "P8RiskFrontController.cs",
            "P8RiskFrontCurveGenerator.cs",
            "P8RiskFrontLightCurtainRenderer.cs",
            "P8RiskFrontTimeDriver.cs",
            "P8RiskFrontDebugStatus.cs"
        };

        foreach (string script in p8bScripts)
        {
            string source = File.ReadAllText(Path.Combine(scriptRoot, script));
            Assert.IsFalse(source.Contains("EvacuationGameManager"), script);
            Assert.IsFalse(source.Contains("ResultPanelController"), script);
            Assert.IsFalse(source.Contains("BuildingShelter"), script);
            Assert.IsFalse(source.Contains("NpcEvacuation"), script);
        }

        Assert.IsFalse(P8RiskFrontController.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P8RiskFrontController.AppliesInfrastructureHazardInteraction);
        Assert.IsFalse(P8RiskFrontController.AppliesCollapseProxy);
        Assert.IsFalse(P8RiskFrontController.RequiresP9Systems);
    }

    private static P8RiskFrontConfig CreateValidRiskFrontConfig()
    {
        return new P8RiskFrontConfig
        {
            scenarioId = "test_scenario",
            sourceMode = "test",
            evidenceSourceId = "test_source",
            configVersion = "test.1",
            hazardLayerVersion = "test.1",
            geometryType = "synthetic",
            timeOriginSeconds = 0f,
            riskFrontEnabledInP8A = false,
            riskFrontEnabledInP8B = true,
            visualHeightMeters = 100f,
            visualHeightIsCinematicOnly = true,
            boundaryIsEvidenceBasedOrPrototype = "prototype",
            visualLayerPurpose = "cinematic risk-front readability only",
            scienceLayerFields = new[]
            {
                "arrivalTimeSeconds",
                "inundationDepthMeters",
                "waterLevelMeters",
                "tsunamiHeightMeters",
                "inundationBoundary",
                "hazardIntensity",
                "confidence",
                "evidenceSourceId"
            },
            visualLayerFields = new[]
            {
                "visualHeightMeters",
                "visualHeightIsCinematicOnly"
            },
            segmentCount = 16,
            p8bDisclaimer = P8RiskFrontVisualConfig.CinematicDisclaimer,
            manualSampleIsOfficial = false,
            notes = "Test config."
        };
    }

    private static P8HazardLayerData CreateLayerWithFeature(P8HazardFeature feature)
    {
        return CreateLayerWithFeature(feature, "test");
    }

    private static P8HazardLayerData CreateLayerWithFeature(P8HazardFeature feature, string sourceMode)
    {
        feature.sourceMode = sourceMode;
        feature.evidenceSourceId = string.IsNullOrWhiteSpace(feature.evidenceSourceId) ? "test_source" : feature.evidenceSourceId;

        return new P8HazardLayerData
        {
            scenarioId = "test_hazard_layer",
            sourceMode = sourceMode,
            hazardLayerVersion = "test.1",
            timeOriginSeconds = 0f,
            scienceLayerFields = new[]
            {
                "arrivalTimeSeconds",
                "inundationDepthMeters",
                "waterLevelMeters",
                "tsunamiHeightMeters",
                "inundationBoundary",
                "hazardIntensity",
                "confidence",
                "evidenceSourceId",
                "geometryType",
                "sourceMode",
                "boundaryIsEvidenceBasedOrPrototype"
            },
            visualLayerFields = new[]
            {
                "visualHeightMeters",
                "visualHeightIsCinematicOnly"
            },
            features = new[] { feature },
            evidenceSources = new[]
            {
                new P8EvidenceSource
                {
                    evidenceSourceId = "test_source",
                    sourceMode = sourceMode,
                    sourceCategory = "manual_sample",
                    title = "Test source",
                    reviewedStatus = "reviewed_for_planning",
                    notes = "Test source metadata."
                }
            }
        };
    }

    private static P8HazardFeature CreateHazardFeature(string featureId, float arrivalTimeSeconds, float depthMeters, float hazardIntensity)
    {
        return new P8HazardFeature
        {
            featureId = featureId,
            sourceMode = "test",
            geometryType = "polyline",
            arrivalTimeSeconds = arrivalTimeSeconds,
            inundationDepthMeters = depthMeters,
            waterLevelMeters = depthMeters + 0.4f,
            tsunamiHeightMeters = Mathf.Max(0.1f, depthMeters),
            inundationBoundary = new[]
            {
                new P8BoundaryPoint { x = 139.7f, y = 35.6f },
                new P8BoundaryPoint { x = 139.71f, y = 35.61f }
            },
            hazardIntensity = hazardIntensity,
            confidence = 0.25f,
            evidenceSourceId = "test_source",
            visualHeightMeters = 100f,
            visualHeightIsCinematicOnly = true,
            boundaryIsEvidenceBasedOrPrototype = "prototype",
            affectedInfrastructureTypes = new P8AffectedInfrastructureTypes { roads = true },
            buildingDamageState = "none",
            collapseProxyState = "data_only",
            collapseProbability = 0f,
            collapseRandomSeed = 10,
            hazardDrivenCollapse = false,
            notes = "Test hazard feature."
        };
    }
}
