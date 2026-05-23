using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class P8RiskFrontPlayModeTests
{
    private GameObject testObject;

    [UnityTest]
    public IEnumerator ControllerCanRenderInTemporarySceneWithoutGameplayCoupling()
    {
        testObject = new GameObject("P8B_RiskFrontController_PlayModeTest");
        P8RiskFrontController controller = testObject.AddComponent<P8RiskFrontController>();

        yield return null;

        bool initialized = controller.Initialize(
            P8HazardLayerLoader.LoadSampleHazardLayer().data,
            P8HazardLayerLoader.LoadRiskFrontConfig().config);

        Assert.IsTrue(initialized, controller.LastStatus);
        Assert.IsFalse(controller.IsFailSafeHidden);
        Assert.IsTrue(controller.IsVisualVisible);
        Assert.IsFalse(P8RiskFrontController.AffectsGameplaySuccessFailure);
        Assert.AreEqual(0, Object.FindObjectsOfType<EvacuationGameManager>().Length);
        Assert.AreEqual(0, Object.FindObjectsOfType<ResultPanelController>().Length);
    }

    [UnityTest]
    public IEnumerator TimeDriverAdvancesWithoutExceptions()
    {
        testObject = new GameObject("P8B_TimeDriver_PlayModeTest");
        P8RiskFrontTimeDriver driver = testObject.AddComponent<P8RiskFrontTimeDriver>();
        driver.SetPlaybackSpeed(2f);
        driver.SetTime(10f);

        yield return null;

        driver.Advance(1.5f);
        Assert.AreEqual(13f, driver.CurrentTimeSeconds, 0.001f);
        Assert.IsFalse(P8RiskFrontTimeDriver.AffectsGameplaySuccessFailure);
    }

    [UnityTest]
    public IEnumerator LightCurtainRendererCanBeEnabledAndDisabled()
    {
        testObject = new GameObject("P8B_LightCurtainRenderer_PlayModeTest");
        P8RiskFrontLightCurtainRenderer renderer = testObject.AddComponent<P8RiskFrontLightCurtainRenderer>();
        P8RiskFrontVisualConfig config = P8RiskFrontVisualConfig.FromRiskFrontConfig(P8HazardLayerLoader.LoadRiskFrontConfig().config);
        var points = new[]
        {
            new Vector3(-1f, 0f, 0f),
            new Vector3(0f, 0f, 1f),
            new Vector3(1f, 0f, 0f)
        };

        yield return null;

        renderer.UpdateCurtain(points, config);
        Assert.IsTrue(renderer.IsVisible);
        Assert.Greater(renderer.LastVertexCount, 0);

        renderer.SetVisible(false);
        Assert.IsFalse(renderer.IsVisible);
    }

    [UnityTest]
    public IEnumerator RiskFrontHasNoChuoBaseMapOrP9Dependency()
    {
        yield return null;

        string activeScenePath = SceneManager.GetActiveScene().path.Replace("\\", "/");
        Assert.AreNotEqual(P8SceneCompatibilityReport.GetLegacyFallbackScenePath(), activeScenePath);
        Assert.IsFalse(P8RiskFrontController.RequiresChuoBaseMap);
        Assert.IsFalse(P8RiskFrontController.RequiresP9Systems);
        Assert.IsFalse(P8RiskFrontVisualConfig.RequiresChuoBaseMap);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (testObject != null)
        {
            Object.Destroy(testObject);
            testObject = null;
        }

        yield return null;
    }
}
