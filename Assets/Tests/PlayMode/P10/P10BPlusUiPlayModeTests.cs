using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class P10BPlusUiPlayModeTests
{
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Time.timeScale = 1f;
        yield return null;
    }

    [UnityTest]
    public IEnumerator RuntimeMenuBuildsStartPauseAndRulesPanelsSceneSafe()
    {
        GameObject root = new GameObject("P10BPlus_MenuRuntime_Test");
        try
        {
            P10BPlusMenuRuntime runtime = root.AddComponent<P10BPlusMenuRuntime>();
            runtime.Build(
                P10BPlusDataLoader.LoadUiConfig().data,
                P10BPlusDataLoader.CreateLocalizationService(P10BPlusLanguage.English));

            yield return null;

            Assert.IsTrue(runtime.StartMenuBuilt);
            Assert.IsTrue(runtime.PauseMenuBuilt);
            Assert.IsTrue(runtime.RulesPanelBuilt);
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            Assert.IsTrue(texts.Any(text => text.text == "Start Game"));
            Assert.IsTrue(texts.Any(text => text.text == "Rules / How to Play"));
            Assert.IsNotNull(root.GetComponentInChildren<ScrollRect>(true));
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator LanguageSwitchRefreshesRuntimeMenuText()
    {
        GameObject root = new GameObject("P10BPlus_MenuRuntime_Language_Test");
        try
        {
            P10BPlusMenuRuntime runtime = root.AddComponent<P10BPlusMenuRuntime>();
            runtime.Build(
                P10BPlusDataLoader.LoadUiConfig().data,
                P10BPlusDataLoader.CreateLocalizationService(P10BPlusLanguage.English));
            runtime.SetLanguage(P10BPlusLanguage.Japanese);

            yield return null;

            Text[] texts = root.GetComponentsInChildren<Text>(true);
            Assert.IsTrue(texts.Any(text => text.text == "ゲーム開始"));
            Assert.IsTrue(texts.Any(text => text.text == "ルール / 操作説明"));
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator PauseAndResumeToggleOverlayAndTimeScale()
    {
        GameObject root = new GameObject("P10BPlus_MenuRuntime_Pause_Test");
        try
        {
            P10BPlusMenuRuntime runtime = root.AddComponent<P10BPlusMenuRuntime>();
            runtime.Build(
                P10BPlusDataLoader.LoadUiConfig().data,
                P10BPlusDataLoader.CreateLocalizationService(P10BPlusLanguage.English));
            runtime.Pause();

            yield return null;

            Assert.IsTrue(runtime.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.That(runtime.GetForceQuitExplanation(), Does.Contain("without saving"));

            runtime.Resume();
            yield return null;

            Assert.IsFalse(runtime.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }
        finally
        {
            Object.Destroy(root);
            Time.timeScale = 1f;
        }
    }

    [UnityTest]
    public IEnumerator NightOverlayEnablesOnlyForNightModes()
    {
        GameObject root = new GameObject("P10BPlus_NightOverlay_Test");
        try
        {
            P10BPlusNightOverlay overlay = root.AddComponent<P10BPlusNightOverlay>();
            P10BPlusWeatherConfig config = P10BPlusDataLoader.LoadWeatherConfig().data;

            overlay.Apply(config, P10BPlusWeatherMode.ClearDay);
            yield return null;
            Assert.IsFalse(overlay.OverlayEnabled);

            overlay.Apply(config, P10BPlusWeatherMode.NightRain);
            yield return null;
            Assert.IsTrue(overlay.OverlayEnabled);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator MovementRuntimeAdapterResolvesWeatherAndStaminaSpeed()
    {
        GameObject root = new GameObject("P10BPlus_MovementAdapter_Test");
        try
        {
            P10BPlusMovementRuntimeAdapter adapter = root.AddComponent<P10BPlusMovementRuntimeAdapter>();
            adapter.Configure(
                P10BPlusDataLoader.LoadMovementStaminaConfig().data,
                P10BPlusDataLoader.LoadWeatherConfig().data,
                P10BPlusDataLoader.LoadAvatarMobilityConfig().data,
                P10BPlusWeatherMode.RainyDay,
                P10BPlusAvatarPresentation.Female,
                "standard");

            float speed = adapter.ResolveMovementSpeed(6f, 14f, true, 1f, out bool sprintActive);
            yield return null;

            Assert.IsTrue(sprintActive);
            Assert.AreEqual(2.5f * 0.75f, speed, 0.001f);
            Assert.Less(adapter.StaminaState.staminaFraction, 1f);
        }
        finally
        {
            Object.Destroy(root);
        }
    }
}
