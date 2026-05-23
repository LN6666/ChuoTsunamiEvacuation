using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P8BRiskFrontGuardPlayModeTests
{
    [UnityTest]
    public IEnumerator RiskFrontGuardLoadsWithoutCreatingVisualSceneObjects()
    {
        yield return null;

        P8RiskFrontConfigLoadResult result = P8HazardLayerLoader.LoadRiskFrontConfig();

        Assert.IsTrue(result.success, string.Join("\n", result.validation.errorsArray));
        Assert.IsFalse(result.config.p8bVisualSceneObjectsImplemented);
        Assert.IsFalse(result.config.p8cInfrastructureInteractionImplemented);
        Assert.IsFalse(result.config.p8dCollapseProxyGameplayImplemented);
        Assert.AreEqual(0, Object.FindObjectsOfType<ParticleSystem>().Length);
        Assert.AreEqual(0, Object.FindObjectsOfType<MeshRenderer>().Count(renderer => renderer.name.Contains("P8RiskFront")));
    }

    [UnityTest]
    public IEnumerator RiskFrontGuardRemainsGameplayNeutralInPlayMode()
    {
        yield return null;

        P8HazardLayerLoadResult hazardLayer = P8HazardLayerLoader.LoadSampleHazardLayer();
        P8RiskFrontConfigLoadResult riskFront = P8HazardLayerLoader.LoadRiskFrontConfig();
        P8RiskFrontPerformanceReport performance = P8RiskFrontPerformanceGuard.Inspect(riskFront.config.performanceSettings);

        Assert.IsTrue(hazardLayer.success, string.Join("\n", hazardLayer.validation.errorsArray));
        Assert.IsTrue(riskFront.success, string.Join("\n", riskFront.validation.errorsArray));
        Assert.IsFalse(performance.hasWarnings, string.Join("\n", performance.warningsArray));
        Assert.IsFalse(P8HazardLayerLoader.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P8HazardLayerLoader.AppliesHazardInteractionsInP8A);
    }
}
