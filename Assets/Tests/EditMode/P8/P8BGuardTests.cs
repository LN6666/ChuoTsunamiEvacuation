using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

public class P8BGuardTests
{
    [Test]
    public void RiskFrontConfigCinematicHeightRequiresCinematicOnlyFlag()
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
    public void RiskFrontConfigRejectsInvalidGeometryType()
    {
        P8RiskFrontConfig config = CreateValidRiskFrontConfig();
        config.geometryType = "fluid_mesh";

        P8HazardValidationResult result = P8HazardDataValidator.ValidateRiskFrontConfig(config);

        Assert.IsFalse(result.isValid);
        Assert.IsTrue(result.isFailSafe);
        StringAssert.Contains("geometryType", string.Join("\n", result.errorsArray));
    }

    [Test]
    public void RiskFrontConfigKeepsScientificFieldsSeparateFromVisualFields()
    {
        P8RiskFrontConfigLoadResult result = P8HazardLayerLoader.LoadRiskFrontConfig();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        CollectionAssert.Contains(result.config.scienceLayerFields, "tsunamiHeightMeters");
        CollectionAssert.Contains(result.config.scienceLayerFields, "waterLevelMeters");
        CollectionAssert.Contains(result.config.visualLayerFields, "visualHeightMeters");
        CollectionAssert.DoesNotContain(result.config.visualLayerFields, "tsunamiHeightMeters");
        CollectionAssert.DoesNotContain(result.config.visualLayerFields, "waterLevelMeters");
        CollectionAssert.DoesNotContain(result.config.scienceLayerFields, "visualHeightMeters");
        Assert.IsTrue(result.config.visualHeightIsCinematicOnly);
        StringAssert.Contains("not physical tsunami height", result.config.notes);
    }

    [Test]
    public void P8BCodeDoesNotReferenceP9NamespacesOrClasses()
    {
        string p8ScriptRoot = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Scripts", "P8");
        string allP8Source = string.Join(
            "\n",
            Directory.GetFiles(p8ScriptRoot, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        Assert.IsFalse(allP8Source.Contains("using P9"));
        Assert.IsFalse(allP8Source.Contains("P9."));
        Assert.IsFalse(allP8Source.Contains("P9CrowdController"));
        Assert.IsFalse(allP8Source.Contains("IndoorShelterGameplay"));
    }

    [Test]
    public void P8BConfigDoesNotEnableCollapseGameplayOrP8CInteractions()
    {
        P8RiskFrontConfigLoadResult riskFront = P8HazardLayerLoader.LoadRiskFrontConfig();
        P8InfrastructureConfigLoadResult infrastructure = P8HazardLayerLoader.LoadInfrastructureConfig();

        Assert.IsTrue(riskFront.success, string.Join("\n", riskFront.validation.errorsArray));
        Assert.IsTrue(infrastructure.success, string.Join("\n", infrastructure.validation.errorsArray));
        Assert.IsFalse(riskFront.config.p8cInfrastructureInteractionImplemented);
        Assert.IsFalse(riskFront.config.p8dCollapseProxyGameplayImplemented);
        Assert.IsFalse(infrastructure.config.collapseGameplayEnabledInP8A);
        Assert.IsFalse(infrastructure.config.hazardDrivenCollapse);
        Assert.AreEqual("data_only", infrastructure.config.roadInteractionMode);
        Assert.AreEqual("data_only", infrastructure.config.buildingInteractionMode);
        Assert.AreEqual("data_only", infrastructure.config.bridgeInteractionMode);
        Assert.AreEqual("data_only", infrastructure.config.undergroundInteractionMode);
        Assert.AreEqual("data_only", infrastructure.config.collapseProxyState);
    }

    [Test]
    public void PerformanceGuardReturnsWarningsForUnsafeSettings()
    {
        var settings = new P8RiskFrontPerformanceSettings
        {
            segmentCount = P8RiskFrontPerformanceGuard.HardMaxSegmentCount + 1,
            rebuildsMeshEveryFrame = true,
            transparentLayerCount = P8RiskFrontPerformanceGuard.RecommendedMaxTransparentLayerCount + 1,
            lightCurtainObjectCount = P8RiskFrontPerformanceGuard.RecommendedMaxLightCurtainObjectCount + 1,
            maxParticleCount = P8RiskFrontPerformanceGuard.RecommendedMaxParticleCount + 1,
            particlesAreBounded = false,
            materialMode = "multi_pass_expensive_transparent",
            shaderRisk = "high",
            hasCullingStrategy = false,
            hasEnableDisableStrategy = false,
            usesSharedMeshOrInstancePool = false
        };

        P8RiskFrontPerformanceReport report = P8RiskFrontPerformanceGuard.Inspect(settings);
        string warnings = string.Join("\n", report.warningsArray);

        Assert.IsTrue(report.hasWarnings);
        Assert.IsTrue(report.hasBlockingRisk);
        StringAssert.Contains("segmentCount", warnings);
        StringAssert.Contains("rebuildsMeshEveryFrame", warnings);
        StringAssert.Contains("transparentLayerCount", warnings);
        StringAssert.Contains("lightCurtainObjectCount", warnings);
        StringAssert.Contains("particlesAreBounded", warnings);
        StringAssert.Contains("materialMode", warnings);
        StringAssert.Contains("hasCullingStrategy", warnings);
        StringAssert.Contains("hasEnableDisableStrategy", warnings);
    }

    [Test]
    public void PerformanceRiskInspectorToolWarnsForUnsafeSettings()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), "p8b_unsafe_performance_config.json");
        string unsafeJson =
            "{\n" +
            "  \"performanceSettings\": {\n" +
            "    \"segmentCount\": 2048,\n" +
            "    \"rebuildsMeshEveryFrame\": true,\n" +
            "    \"transparentLayerCount\": 4,\n" +
            "    \"lightCurtainObjectCount\": 64,\n" +
            "    \"maxParticleCount\": 5000,\n" +
            "    \"particlesAreBounded\": false,\n" +
            "    \"materialMode\": \"multi_pass_expensive_transparent\",\n" +
            "    \"shaderRisk\": \"high\",\n" +
            "    \"hasCullingStrategy\": false,\n" +
            "    \"hasEnableDisableStrategy\": false,\n" +
            "    \"usesSharedMeshOrInstancePool\": false\n" +
            "  }\n" +
            "}\n";

        try
        {
            File.WriteAllText(tempPath, unsafeJson);

            string output = RunPowerShellTool(
                "tools/p8/inspect_p8b_visual_performance_risk.ps1",
                "-ConfigPath \"" + tempPath + "\"");

            StringAssert.Contains("WARN:", output);
            StringAssert.Contains("excessive segment counts", output);
            StringAssert.Contains("per-frame mesh rebuild", output);
            StringAssert.Contains("transparency overdraw", output);
            StringAssert.Contains("light curtain objects", output);
            StringAssert.Contains("unbounded particle usage", output);
            StringAssert.Contains("material/shader risk", output);
            StringAssert.Contains("culling strategy", output);
            StringAssert.Contains("enable/disable strategy", output);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Test]
    public void PerformanceGuardAcceptsCurrentDataConfig()
    {
        P8RiskFrontConfigLoadResult riskFront = P8HazardLayerLoader.LoadRiskFrontConfig();

        Assert.IsTrue(riskFront.success, string.Join("\n", riskFront.validation.errorsArray));
        P8RiskFrontPerformanceReport report = P8RiskFrontPerformanceGuard.Inspect(riskFront.config.performanceSettings);

        Assert.IsFalse(report.hasWarnings, string.Join("\n", report.warningsArray));
        Assert.IsFalse(report.hasBlockingRisk);
    }

    private static P8RiskFrontConfig CreateValidRiskFrontConfig()
    {
        return new P8RiskFrontConfig
        {
            scenarioId = "test_scenario",
            sourceMode = "test",
            evidenceSourceId = string.Empty,
            configVersion = "test.1",
            hazardLayerVersion = "test.1",
            geometryType = "synthetic",
            timeOriginSeconds = 0f,
            riskFrontEnabledInP8A = false,
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
            p8bVisualSceneObjectsImplemented = false,
            p8cInfrastructureInteractionImplemented = false,
            p8dCollapseProxyGameplayImplemented = false,
            manualSampleIsOfficial = false,
            notes = "Cinematic-only test config; not official and not physical tsunami height."
        };
    }

    private static string RunPowerShellTool(string relativePath, string arguments)
    {
        string projectRoot = Directory.GetCurrentDirectory();
        string scriptPath = Path.Combine(projectRoot, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        Assert.IsTrue(File.Exists(scriptPath), scriptPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = "-ExecutionPolicy Bypass -File \"" + scriptPath + "\" " + arguments,
            WorkingDirectory = projectRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            Assert.IsNotNull(process);
            bool exited = process.WaitForExit(30000);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!exited)
            {
                process.Kill();
                Assert.Fail("Timed out running " + relativePath);
            }

            Assert.AreEqual(0, process.ExitCode, output + "\n" + error);
            return output + "\n" + error;
        }
    }
}
