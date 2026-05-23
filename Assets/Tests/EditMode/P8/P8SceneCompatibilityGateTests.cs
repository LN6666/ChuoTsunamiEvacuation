using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

public class P8SceneCompatibilityGateTests
{
    [Test]
    public void CompatibilityInspectorToolRuns()
    {
        string output = RunPowerShellTool("tools/p8/inspect_p8a_scene_compatibility.ps1");

        StringAssert.Contains("P8-A scene compatibility inspection: PASS", output);
        StringAssert.Contains(P8SceneCompatibilityReport.BaselineScenePath, output);
    }

    [Test]
    public void CompatibilityReportDefinesHighDetailBaselineWithoutLoadingBaseMap()
    {
        Assert.AreEqual("P8-A", P8SceneCompatibilityReport.Stage);
        Assert.IsTrue(P8SceneCompatibilityReport.HasExactlyFourP8Stages());
        Assert.IsTrue(P8SceneCompatibilityReport.IsHighDetailBaselinePath(P8SceneCompatibilityReport.BaselineScenePath));
        Assert.IsFalse(P8SceneCompatibilityReport.UsesChuoBaseMapAsBaseline);
        Assert.IsFalse(P8SceneCompatibilityReport.BaselineScenePath.Contains("Chuo_BaseMap"));
        Assert.AreEqual("Assets/Scenes/Chuo_BaseMap.unity", P8SceneCompatibilityReport.GetLegacyFallbackScenePath());
    }

    [Test]
    public void CompatibilityAssumptionsAreReportableWithPendingRuntimeChecks()
    {
        P8CompatibilityCheck[] checks = P8SceneCompatibilityReport.GetP2P6CompatibilityChecks();

        CollectionAssert.Contains(checks.Select(check => check.phase).Distinct().ToArray(), "P2");
        CollectionAssert.Contains(checks.Select(check => check.phase).Distinct().ToArray(), "P6");
        Assert.IsTrue(checks.Any(check => check.runtimeSmokePending));
        Assert.IsTrue(checks.Any(check => check.status.Contains("success/failure rules unchanged")));
        Assert.GreaterOrEqual(P8SceneCompatibilityReport.GetPendingRuntimeChecks().Length, 5);
        StringAssert.Contains("gameplay success/failure rules unchanged", P8SceneCompatibilityReport.CreateSummary());
    }

    [Test]
    public void HazardLoaderDoesNotAffectGameplaySuccessFailure()
    {
        P8HazardLayerLoadResult result = P8HazardLayerLoader.LoadSampleHazardLayer();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        Assert.IsFalse(P8HazardLayerLoader.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P8SceneCompatibilityReport.AffectsGameplaySuccessFailure);
    }

    [Test]
    public void P8ACodeDoesNotEnableLaterStageBehavior()
    {
        Assert.IsFalse(P8SceneCompatibilityReport.ImplementsP8BRiskFrontVisualization);
        Assert.IsFalse(P8SceneCompatibilityReport.ImplementsP8CHazardInteractions);
        Assert.IsFalse(P8SceneCompatibilityReport.ImplementsP8DCollapseProxy);
        Assert.IsFalse(P8SceneCompatibilityReport.ImplementsP9CrowdSpawnOrIndoorGameplay);
        Assert.IsFalse(P8SceneCompatibilityReport.ImplementsP10Packaging);
        Assert.IsFalse(P8HazardLayerLoader.AppliesHazardInteractionsInP8A);
    }

    private static string RunPowerShellTool(string relativePath)
    {
        string projectRoot = Directory.GetCurrentDirectory();
        string scriptPath = Path.Combine(projectRoot, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        Assert.IsTrue(File.Exists(scriptPath), scriptPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = "-ExecutionPolicy Bypass -File \"" + scriptPath + "\"",
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
