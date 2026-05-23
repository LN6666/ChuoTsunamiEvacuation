using System.IO;
using System.Linq;
using NUnit.Framework;

public class P8HazardLayerFoundationTests
{
    [Test]
    public void SampleHazardJsonLoadsAndValidates()
    {
        P8HazardLayerLoadResult result = P8HazardLayerLoader.LoadSampleHazardLayer();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        Assert.IsFalse(result.failSafe);
        Assert.AreEqual("p8b_chuo_tokyo_tsunami_official_spatial_v1", result.data.scenarioId);
        Assert.AreEqual("official_tsunami_metropolitan", result.data.sourceMode);
        Assert.AreEqual("PASS", result.data.p8cGateDecision);
        Assert.IsTrue(result.data.officialMetropolitanEvidenceIdentified);
        Assert.IsTrue(result.data.completeOfficialSpatialLayerExtracted);
        Assert.AreEqual("extracted", result.data.spatialExtractionStatus);
        Assert.AreEqual(2, result.data.features.Length);
        Assert.IsTrue(result.summary.Contains("features=2"));
    }

    [Test]
    public void EvidenceLayerPrioritizesTokyoMetropolitanSources()
    {
        P8HazardLayerLoadResult result = P8HazardLayerLoader.LoadSampleHazardLayer();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        Assert.IsTrue(result.data.evidenceSources.Any(source => source.evidenceSourceId == "tokyo_damage_estimation_map_tsunami"));
        Assert.IsTrue(result.data.evidenceSources.Any(source => source.evidenceSourceId == "tokyo_damage_estimation_report_tsunami"));
        Assert.IsTrue(result.data.evidenceSources.Any(source => source.evidenceSourceId == "chuo_city_tsunami_liquefaction_page"));
        Assert.IsTrue(result.data.evidenceSources.Any(source => source.evidenceSourceId == "supplementary_pdf_tokyo_bay_tsunami_height_chuo"));
        Assert.IsTrue(result.data.evidenceSources.Any(source => source.evidenceSourceId == "flood_proxy_chuo_hazard_map"));
        Assert.IsTrue(result.data.evidenceSources.Any(source => source.evidenceSourceId == "mlit_n03_chuo_admin_boundary"));
        Assert.IsTrue(result.data.evidenceSources.Any(source => source.sourceCategory == "official_tsunami_metropolitan"));
        Assert.IsTrue(result.data.evidenceSources.Any(source => source.sourceCategory == "official_tsunami_report_reference"));
        Assert.IsTrue(result.data.evidenceSources.Any(source => source.sourceCategory == "official_flood_proxy"));
        Assert.IsTrue(result.data.evidenceSources.Any(source => source.sourceCategory == "official_admin_boundary"));
    }

    [Test]
    public void MaxTsunamiHeightReferenceIsSeparateFromSpatialDepth()
    {
        P8HazardLayerLoadResult result = P8HazardLayerLoader.LoadSampleHazardLayer();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        Assert.IsTrue(result.data.features.Any(feature => feature.maxTsunamiHeightMeters >= 2.4f));
        Assert.IsTrue(result.data.features.All(feature => feature.extractionStatus == "extracted"));
        Assert.IsTrue(result.data.features.All(feature => feature.spatialSampleCount > 1000));
        Assert.IsTrue(result.data.features.All(feature => feature.spatialSamples.Length == feature.spatialSampleCount));
        Assert.IsTrue(result.data.features.All(feature => feature.maxInundationDepthMeters > 0f));
        Assert.IsTrue(result.data.features.All(feature => feature.maxTsunamiHeightMeters > 0f));
        Assert.IsTrue(result.data.features.All(feature => feature.inundationDepthStatus.Contains("extracted_spatial")));
        Assert.IsTrue(result.data.features.All(feature => feature.notes.Contains("not used as the inundation-depth grid")));
        Assert.IsTrue(result.data.features.All(feature => feature.boundaryIsEvidenceBasedOrPrototype == "evidence_based"));
    }

    [Test]
    public void P8CGateBlocksManualExtractionWithoutUserOverride()
    {
        P8HazardLayerLoadResult loaded = P8HazardLayerLoader.LoadSampleHazardLayer();
        Assert.IsTrue(loaded.success, string.Join("\n", loaded.validation.errorsArray));

        P8HazardValidationResult passGate = P8HazardDataValidator.ValidateP8CSpatialGate(loaded.data, false);
        Assert.IsTrue(passGate.isValid, string.Join("\n", passGate.errorsArray));

        P8HazardLayerData manualLayer = CreateMinimalValidLayer("evidence_planned");
        manualLayer.p8cGateDecision = "CONDITIONAL PASS";
        manualLayer.extractionStatus = "manual_extraction_required";
        manualLayer.spatialExtractionStatus = "manual_extraction_required";
        manualLayer.completeOfficialSpatialLayerExtracted = false;
        manualLayer.features[0].spatialExtractionStatus = "manual_extraction_required";

        P8HazardValidationResult blocked = P8HazardDataValidator.ValidateP8CSpatialGate(manualLayer, false);

        Assert.IsFalse(blocked.isValid);
        Assert.IsTrue(blocked.isFailSafe);
        StringAssert.Contains("explicit user override", string.Join("\n", blocked.errorsArray));
    }

    [Test]
    public void HazardLayerValidatesExtractionStatus()
    {
        P8HazardLayerData data = CreateMinimalValidLayer();
        data.features[0].extractionStatus = "not_a_real_status";

        P8HazardValidationResult result = P8HazardDataValidator.ValidateHazardLayer(data);

        Assert.IsFalse(result.isValid);
        Assert.IsTrue(result.isFailSafe);
        StringAssert.Contains("extractionStatus", string.Join("\n", result.errorsArray));
    }

    [Test]
    public void ValidatorCatchesMissingRequiredFields()
    {
        var data = new P8HazardLayerData
        {
            sourceMode = "manual_sample",
            hazardLayerVersion = "test",
            features = new P8HazardFeature[0]
        };

        P8HazardValidationResult result = P8HazardDataValidator.ValidateHazardLayer(data);

        Assert.IsFalse(result.isValid);
        Assert.IsTrue(result.isFailSafe);
        Assert.GreaterOrEqual(result.errorsArray.Length, 2);
    }

    [Test]
    public void ManualSampleFeatureRequiresEvidenceSourceId()
    {
        var data = CreateMinimalValidLayer("manual_sample");
        data.features[0].evidenceSourceId = string.Empty;

        P8HazardValidationResult result = P8HazardDataValidator.ValidateHazardLayer(data);

        Assert.IsFalse(result.isValid);
        Assert.IsTrue(result.isFailSafe);
        StringAssert.Contains("evidenceSourceId is required", string.Join("\n", result.errorsArray));
    }

    [Test]
    public void CinematicVisualHeightRequiresCinematicOnlyFlag()
    {
        var data = CreateMinimalValidLayer();
        data.features[0].visualHeightMeters = 120f;
        data.features[0].tsunamiHeightMeters = 1f;
        data.features[0].visualHeightIsCinematicOnly = false;

        P8HazardValidationResult result = P8HazardDataValidator.ValidateHazardLayer(data);

        Assert.IsFalse(result.isValid);
        Assert.IsTrue(result.isFailSafe);
        StringAssert.Contains("visualHeightIsCinematicOnly=true", string.Join("\n", result.errorsArray));
    }

    [Test]
    public void UnknownSourceModeFailsSafe()
    {
        var data = CreateMinimalValidLayer();
        data.sourceMode = "official_claim_without_review";
        data.features[0].sourceMode = "official_claim_without_review";

        P8HazardValidationResult result = P8HazardDataValidator.ValidateHazardLayer(data);

        Assert.IsFalse(result.isValid);
        Assert.IsTrue(result.isFailSafe);
        StringAssert.Contains("sourceMode", string.Join("\n", result.errorsArray));
    }

    [Test]
    public void CollapseProxyFieldsParseButDoNotTriggerGameplayEffects()
    {
        P8HazardLayerLoadResult layerResult = P8HazardLayerLoader.LoadSampleHazardLayer();
        P8InfrastructureConfigLoadResult infrastructureResult = P8HazardLayerLoader.LoadInfrastructureConfig();

        Assert.IsTrue(layerResult.success, string.Join("\n", layerResult.validation.errorsArray));
        Assert.IsTrue(infrastructureResult.success, string.Join("\n", infrastructureResult.validation.errorsArray));
        Assert.AreEqual("data_only", layerResult.data.features[0].collapseProxyState);
        Assert.AreEqual(0f, layerResult.data.features[0].collapseProbability, 0.0001f);
        Assert.IsFalse(layerResult.data.features[0].hazardDrivenCollapse);
        Assert.IsFalse(infrastructureResult.config.collapseProxyEnabledInP8A);
        Assert.IsFalse(infrastructureResult.config.collapseGameplayEnabledInP8A);
        Assert.IsFalse(P8HazardLayerLoader.AppliesHazardInteractionsInP8A);
        Assert.IsFalse(P8HazardLayerLoader.AffectsGameplaySuccessFailure);
    }

    [Test]
    public void RiskFrontConfigSeparatesScienceAndVisualLayers()
    {
        P8RiskFrontConfigLoadResult result = P8HazardLayerLoader.LoadRiskFrontConfig();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        Assert.IsFalse(result.config.riskFrontEnabledInP8A);
        Assert.Greater(result.config.visualHeightMeters, P8HazardDataValidator.CinematicHeightThresholdMeters);
        Assert.IsTrue(result.config.visualHeightIsCinematicOnly);
        CollectionAssert.Contains(result.config.scienceLayerFields, "arrivalTimeSeconds");
        CollectionAssert.Contains(result.config.scienceLayerFields, "inundationDepthMeters");
        CollectionAssert.Contains(result.config.visualLayerFields, "visualHeightMeters");
        CollectionAssert.Contains(result.config.visualLayerFields, "visualHeightIsCinematicOnly");
        StringAssert.Contains("cinematic", result.config.notes);
    }

    [Test]
    public void ScienceAndVisualFieldListsRejectCrossLayerFields()
    {
        var data = CreateMinimalValidLayer();
        data.scienceLayerFields = new[]
        {
            "arrivalTimeSeconds",
            "inundationDepthMeters",
            "waterLevelMeters",
            "tsunamiHeightMeters",
            "inundationBoundary",
            "hazardIntensity",
            "confidence",
            "evidenceSourceId",
            "visualHeightMeters"
        };

        P8HazardValidationResult result = P8HazardDataValidator.ValidateHazardLayer(data);

        Assert.IsFalse(result.isValid);
        Assert.IsTrue(result.isFailSafe);
        StringAssert.Contains("scienceLayerFields must not contain visual field", string.Join("\n", result.errorsArray));
    }

    [Test]
    public void LoaderDoesNotReferenceGameplaySuccessFailureControllers()
    {
        string loaderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Assets",
            "Scripts",
            "P8",
            "P8HazardLayerLoader.cs");

        string loaderSource = File.ReadAllText(loaderPath);

        Assert.IsFalse(P8HazardLayerLoader.AffectsGameplaySuccessFailure);
        Assert.IsFalse(loaderSource.Contains("EvacuationGameManager"));
        Assert.IsFalse(loaderSource.Contains("ResultPanelController"));
        Assert.IsFalse(loaderSource.Contains("BuildingShelter"));
        Assert.IsFalse(loaderSource.Contains("NavigationGuidanceController"));
    }

    [Test]
    public void P2P6CompatibilityInspectorIsReadOnlyTool()
    {
        string toolPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "tools",
            "p8",
            "inspect_p8a_p2_p6_compatibility.ps1");

        Assert.IsTrue(File.Exists(toolPath), toolPath);

        string toolSource = File.ReadAllText(toolPath);
        Assert.IsFalse(toolSource.Contains("Set-Content"));
        Assert.IsFalse(toolSource.Contains("Remove-Item"));
        Assert.IsFalse(toolSource.Contains("git checkout"));
        Assert.IsFalse(toolSource.Contains("git reset"));
    }

    private static P8HazardLayerData CreateMinimalValidLayer()
    {
        return CreateMinimalValidLayer("test");
    }

    private static P8HazardLayerData CreateMinimalValidLayer(string sourceMode)
    {
        return new P8HazardLayerData
        {
            scenarioId = "test_scenario",
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
            features = new[]
            {
                new P8HazardFeature
                {
                    featureId = "test_feature",
                    sourceMode = sourceMode,
                    geometryType = "synthetic",
                    arrivalTimeSeconds = 1f,
                    inundationDepthMeters = 0.1f,
                    waterLevelMeters = 0.2f,
                    tsunamiHeightMeters = 0.2f,
                    inundationBoundary = new[]
                    {
                        new P8BoundaryPoint { x = 0f, y = 0f }
                    },
                    hazardIntensity = 0.2f,
                    confidence = 0.1f,
                    evidenceSourceId = "test_source",
                    visualHeightMeters = 1f,
                    visualHeightIsCinematicOnly = false,
                    boundaryIsEvidenceBasedOrPrototype = "prototype",
                    affectedInfrastructureTypes = new P8AffectedInfrastructureTypes { roads = true },
                    buildingDamageState = "none",
                    collapseProxyState = "disabled",
                    collapseProbability = 0f,
                    collapseRandomSeed = 1,
                    hazardDrivenCollapse = false
                }
            },
            evidenceSources = new[]
            {
                new P8EvidenceSource
                {
                    evidenceSourceId = "test_source",
                    sourceMode = sourceMode,
                    sourceCategory = "manual_sample",
                    title = "Test source",
                    reviewedStatus = "reviewed_for_planning",
                    notes = "Test-only source."
                }
            }
        };
    }
}
