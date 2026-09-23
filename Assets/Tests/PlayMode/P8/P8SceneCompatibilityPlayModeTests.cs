using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class P8SceneCompatibilityPlayModeTests
{
    [UnityTest]
    public IEnumerator BaselineReferenceCanBeRepresentedWithoutLoadingChuoBaseMap()
    {
        yield return null;

        string activeScenePath = SceneManager.GetActiveScene().path.Replace("\\", "/");
        Assert.AreNotEqual(P8SceneCompatibilityReport.GetLegacyFallbackScenePath(), activeScenePath);
        Assert.IsTrue(P8SceneCompatibilityReport.IsHighDetailBaselinePath(P8SceneCompatibilityReport.BaselineScenePath));
        Assert.IsFalse(P8SceneCompatibilityReport.UsesChuoBaseMapAsBaseline);
    }

    [UnityTest]
    public IEnumerator HazardLoaderRemainsGameplayNeutralInPlayMode()
    {
        yield return null;

        P8HazardLayerLoadResult result = P8HazardLayerLoader.LoadSampleHazardLayer();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        Assert.IsFalse(P8HazardLayerLoader.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P8HazardLayerLoader.AppliesHazardInteractionsInP8A);
        Assert.AreEqual(0, Object.FindObjectsOfType<EvacuationGameManager>().Length);
        Assert.AreEqual(0, Object.FindObjectsOfType<ResultPanelController>().Length);
    }

    [UnityTest]
    public IEnumerator P8AReportDoesNotImplementLaterStageBehavior()
    {
        yield return null;

        Assert.IsTrue(P8SceneCompatibilityReport.HasExactlyFiveP8Stages());
        Assert.IsTrue(P8SceneCompatibilityReport.IsP8StageAllowed("P8-E"));
        Assert.IsFalse(P8SceneCompatibilityReport.IsP8StageAllowed("P8-0"));
        Assert.IsTrue(P8SceneCompatibilityReport.IsForbiddenP8Stage("P8-F"));
        Assert.IsFalse(P8SceneCompatibilityReport.IsP8StageAllowed("P8-Z"));
        Assert.IsFalse(P8SceneCompatibilityReport.ImplementsP8BRiskFrontVisualization);
        Assert.IsFalse(P8SceneCompatibilityReport.ImplementsP8CHazardInteractions);
        Assert.IsFalse(P8SceneCompatibilityReport.ImplementsP8DCollapseProxy);
        Assert.IsFalse(P8SceneCompatibilityReport.ImplementsP9CrowdSpawnOrIndoorGameplay);
        Assert.IsFalse(P8SceneCompatibilityReport.ImplementsP10Packaging);
    }
}
