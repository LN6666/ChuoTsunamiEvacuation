using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class P10CPrePlayableStartupHotfixPlayModeTests
{
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Time.timeScale = 1f;
        DestroyNamed("P10CPre_PlayableStartupBootstrap_Test");
        DestroyNamed("P10CPre_PlayableStartupCanvas");
        DestroyNamed("GameplayTestRoot");
        DestroyNamed("P10CPre_P8_RiskFrontVerification");
        DestroyNamed("EventSystem");
        yield return null;
    }

    [UnityTest]
    public IEnumerator StartupMenuShowsLanguageSelectorAndRules()
    {
        P10CPrePlayableStartupBootstrap bootstrap = P10CPrePlayableStartupBootstrap.InstallForTest(new P10CPrePlayableStartupConfig());

        yield return null;

        Assert.IsTrue(bootstrap.StartMenuVisible);
        Text[] texts = Resources.FindObjectsOfTypeAll<Text>();
        Assert.IsTrue(texts.Any(text => text.text == "Start Game"));
        Assert.IsTrue(texts.Any(text => text.text == "English"));
        Assert.IsTrue(texts.Any(text => text.text == "Japanese"));

        bootstrap.ShowRules();
        yield return null;

        Assert.IsTrue(bootstrap.RulesPanelVisible);
        Assert.IsNotNull(Object.FindObjectOfType<ScrollRect>());
    }

    [UnityTest]
    public IEnumerator StartGameBuildsPlayableRuntimeAndDiagnostics()
    {
        var config = new P10CPrePlayableStartupConfig
        {
            startGameLoadsHighDetailScene = false
        };
        P10CPrePlayableStartupBootstrap bootstrap = P10CPrePlayableStartupBootstrap.InstallForTest(config);

        bootstrap.StartGameForTest(false);
        yield return null;
        yield return null;

        P10CPreStartupDiagnosticsSnapshot diagnostics = bootstrap.CaptureDiagnostics();
        Assert.GreaterOrEqual(diagnostics.playerCount, 1);
        Assert.GreaterOrEqual(diagnostics.cameraCount, 1);
        Assert.GreaterOrEqual(diagnostics.canvasCount, 1);
        Assert.GreaterOrEqual(diagnostics.eventSystemCount, 1);
        Assert.GreaterOrEqual(diagnostics.gameManagerCount, 1);
        Assert.GreaterOrEqual(diagnostics.resultPanelCount, 1);
        Assert.GreaterOrEqual(diagnostics.shelterEntranceCount, 1);
        Assert.GreaterOrEqual(diagnostics.p4RealShelterMarkerCount, 1);
        Assert.GreaterOrEqual(diagnostics.p5RealQualifiedShelterCount, 1);
        Assert.GreaterOrEqual(diagnostics.p5HumanitarianCandidateCount, 1);
        Assert.GreaterOrEqual(diagnostics.p6NpcCount, 1);
        Assert.IsTrue(diagnostics.p8RiskFrontLoaded);
        Assert.GreaterOrEqual(diagnostics.p9SpawnMarkerCount, 1);
        Assert.GreaterOrEqual(diagnostics.p9CrowdAgentCount, 1);
        Assert.GreaterOrEqual(diagnostics.p9EntranceSafeFloorMarkerCount, 1);
        Assert.GreaterOrEqual(diagnostics.p9CollapseDebrisZoneCount, 1);
        Assert.GreaterOrEqual(diagnostics.p10GreenFrameRuntimeCount, 1);
        Assert.IsTrue(diagnostics.p10PauseRulesUiAvailable);
        Assert.IsTrue(diagnostics.p10WeatherStaminaAttached);
        Assert.IsFalse(diagnostics.profilingAutoQuitEnabled);
        Assert.AreEqual(config.targetHighDetailScenePath, bootstrap.LastStartGameRequestedScenePath);

        foreach (P10BGreenGroundFrameRuntime runtime in Object.FindObjectsOfType<P10BGreenGroundFrameRuntime>())
        {
            runtime.SetTsunamiStarted(true);
        }

        yield return null;
        diagnostics = bootstrap.CaptureDiagnostics();
        Assert.GreaterOrEqual(diagnostics.p10GreenFrameCount, 1);
    }

    private static void DestroyNamed(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        if (target != null)
        {
            Object.Destroy(target);
        }
    }
}
