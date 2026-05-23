using System.IO;
using NUnit.Framework;

public class P8HazardLayerFoundationTests
{
    [Test]
    public void SampleHazardJsonLoadsAndValidates()
    {
        P8HazardLayerLoadResult result = P8HazardLayerLoader.LoadSampleHazardLayer();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        Assert.IsFalse(result.failSafe);
        Assert.AreEqual("p8a_chuo_manual_sample", result.data.scenarioId);
        Assert.AreEqual("manual_sample", result.data.sourceMode);
        Assert.AreEqual(2, result.data.features.Length);
        Assert.IsTrue(result.summary.Contains("features=2"));
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
        StringAssert.Contains("cinematic", result.config.notes);
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
        return new P8HazardLayerData
        {
            scenarioId = "test_scenario",
            sourceMode = "test",
            hazardLayerVersion = "test.1",
            timeOriginSeconds = 0f,
            features = new[]
            {
                new P8HazardFeature
                {
                    featureId = "test_feature",
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
            }
        };
    }
}
