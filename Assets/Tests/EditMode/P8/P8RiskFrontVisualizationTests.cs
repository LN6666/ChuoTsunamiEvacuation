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
}
