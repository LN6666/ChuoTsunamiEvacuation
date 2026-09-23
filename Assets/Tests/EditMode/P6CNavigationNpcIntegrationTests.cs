using System.IO;
using NUnit.Framework;
using UnityEngine;

public class P6CNavigationNpcIntegrationTests
{
    [Test]
    public void NavigationAndNpcSourcesDoNotReferencePlayerResultApis()
    {
        string combinedSource = ReadSourceDirectory(Path.Combine(Application.dataPath, "Scripts", "Navigation")) +
            ReadSourceDirectory(Path.Combine(Application.dataPath, "Scripts", "NPC"));

        string[] forbiddenApiReferences =
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
            "ShowFailure"
        };

        foreach (string forbiddenApiReference in forbiddenApiReferences)
        {
            Assert.IsFalse(
                combinedSource.Contains(forbiddenApiReference),
                $"P6-A/P6-B prototype sources must not reference player result API: {forbiddenApiReference}");
        }

        Assert.IsFalse(NavigationGuidanceController.AffectsGameplayRules);
        Assert.IsFalse(NpcEvacuationAgent.AffectsPlayerSuccessFailure);
        Assert.IsFalse(NpcEvacuationSpawner.AffectsPlayerSuccessFailure);
    }

    [Test]
    public void InheritedSourceModeDefaultsRemainProtected()
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
