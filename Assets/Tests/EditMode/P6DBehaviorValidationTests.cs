using System.IO;
using NUnit.Framework;
using UnityEngine;

public class P6DBehaviorValidationTests
{
    [Test]
    public void RunnerSummarizesGuidanceTrendWarningsAndNpcArrival()
    {
        GameObject npcObject = new GameObject("P6D_EditMode_Npc");
        try
        {
            NpcEvacuationAgent agent = npcObject.AddComponent<NpcEvacuationAgent>();
            agent.ConfigureMovement(4f, 0.1f);
            agent.EnsureNonBlockingPhysics();
            agent.ConfigureTargets(new[]
            {
                new NpcEvacuationTargetInfo
                {
                    targetId = "p6d_target",
                    displayName = "P6-D Target",
                    position = new Vector3(1f, 0f, 0f),
                    canEnter = true,
                    isSelectable = true,
                    isOfficialShelter = true
                }
            }, true);
            agent.Tick(1f);

            var navigationTarget = new NavigationTargetInfo
            {
                targetId = "p6d_guidance_target",
                displayName = "P6-D Guidance Target",
                worldPosition = new Vector3(5f, 0f, 0f),
                hasWorldPosition = true,
                isOfficialShelter = true
            };

            NavigationGuidanceResult initialGuidance = NavigationGuidanceCalculator.BuildGuidance(
                Vector3.zero,
                navigationTarget,
                NavigationGuidanceCalculator.DefaultWalkingSpeedMetersPerSecond);
            NavigationGuidanceResult finalGuidance = NavigationGuidanceCalculator.BuildGuidance(
                new Vector3(2f, 0f, 0f),
                navigationTarget,
                NavigationGuidanceCalculator.DefaultWalkingSpeedMetersPerSecond);

            P6DBehaviorValidationResult result = P6DBehaviorValidationRunner.Evaluate(
                initialGuidance,
                finalGuidance,
                new[] { agent },
                true);

            Assert.IsTrue(result.Passed, result.ToSummaryText());
            Assert.AreEqual(1, result.selectedTargetCount);
            Assert.AreEqual(1, result.npcArrivedCount);
            Assert.AreEqual(5f, result.initialGuidanceDistanceMeters, 0.001f);
            Assert.AreEqual(3f, result.finalGuidanceDistanceMeters, 0.001f);
            Assert.IsTrue(result.warningTextIncludesNotOfficialNavigation);
            Assert.IsTrue(result.warningTextIncludesNotOfficialEvacuationGuidance);
            Assert.IsTrue(result.warningTextAvoidsUnsafeOfficialNavigationClaim);
        }
        finally
        {
            Object.DestroyImmediate(npcObject);
        }
    }

    [Test]
    public void P6DSourceDoesNotReferenceForbiddenRuntimeIntegrationSurfaces()
    {
        string simulationSource = ReadSourceDirectory(Path.Combine(Application.dataPath, "Scripts", "Simulation"));

        string[] forbiddenReferences =
        {
            "EvacuationGameManager",
            "ResultMetrics",
            "ResultPanelController",
            "ResultExportService",
            "TryEnterShelter",
            "HandleClimbCompleted",
            "ReportPlayerReachedByRisk",
            "ReportShelterEntranceReachedByRisk",
            "TriggerFailure",
            "ShowSuccess",
            "ShowFailure",
            "Chuo_BaseMap",
            "ProjectSettings",
            "Packages",
            "data_pipeline/raw",
            "data_pipeline/download",
            "data_pipeline/downloads",
            "data_pipeline/cache",
            "data_pipeline/tmp",
            ".venv",
            "UnityWebRequest",
            "HttpClient",
            "WWW"
        };

        foreach (string forbiddenReference in forbiddenReferences)
        {
            Assert.IsFalse(
                simulationSource.Contains(forbiddenReference),
                $"P6-D runtime validation scripts must not reference: {forbiddenReference}");
        }
    }

    [Test]
    public void InheritedSourceModeDefaultsRemainProtectedForP6D()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.Load();

        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, config.sourceMode);
        Assert.IsFalse(config.enableHumanitarianCandidates);
        Assert.IsFalse(config.enableLifeFirstCandidateSelection);
    }

    private static string ReadSourceDirectory(string directory)
    {
        Assert.IsTrue(Directory.Exists(directory), directory);

        string combinedSource = string.Empty;
        string[] sourceFiles = Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly);
        foreach (string sourceFile in sourceFiles)
        {
            combinedSource += File.ReadAllText(sourceFile);
        }

        return combinedSource;
    }
}
