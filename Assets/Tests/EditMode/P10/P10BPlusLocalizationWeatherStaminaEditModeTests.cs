using System.Linq;
using NUnit.Framework;

public class P10BPlusLocalizationWeatherStaminaEditModeTests
{
    [Test]
    public void LocalizationLookupSwitchAndFallbackAreSafe()
    {
        P10BPlusLocalizationService service = P10BPlusDataLoader.CreateLocalizationService(P10BPlusLanguage.English);

        Assert.AreEqual("Start Game", service.Translate("menu.start"));
        service.SetLanguage(P10BPlusLanguage.Japanese);
        Assert.AreEqual("ゲーム開始", service.Translate("menu.start"));

        string missing = service.Translate("missing.key");
        Assert.AreEqual("[missing.key]", missing);
    }

    [Test]
    public void LocalizationFallsBackToEnglishWhenJapaneseKeyMissing()
    {
        var english = new P10BPlusLocalizationTable
        {
            entries = new[]
            {
                new P10BPlusLocalizationEntry { key = "only.en", value = "English fallback" }
            }
        };
        var japanese = new P10BPlusLocalizationTable();
        var service = new P10BPlusLocalizationService(english, japanese, P10BPlusLanguage.Japanese);

        Assert.AreEqual("English fallback", service.Translate("only.en"));
    }

    [Test]
    public void UiConfigUsesSafeBackgroundPolicy()
    {
        P10BPlusUiConfig config = P10BPlusDataLoader.LoadUiConfig().data;

        Assert.IsTrue(config.startMenuEnabled);
        Assert.IsTrue(config.pauseMenuEnabled);
        Assert.IsTrue(config.rulesPanelScrollable);
        Assert.IsTrue(config.wrapDynamicText);
        Assert.AreEqual(0.35f, config.backgroundOpacity, 0.001f);
        Assert.IsFalse(config.unlicensedInternetImageCommitted);
        Assert.IsTrue(config.IsBackgroundPolicySafe());
    }

    [Test]
    public void WeatherModeModifiersMatchRequirements()
    {
        P10BPlusWeatherConfig config = P10BPlusDataLoader.LoadWeatherConfig().data;

        Assert.AreEqual(1.0f, P10BPlusWeatherMovementModifier.GetMultiplier(config, P10BPlusWeatherMode.ClearDay), 0.001f);
        Assert.AreEqual(0.75f, P10BPlusWeatherMovementModifier.GetMultiplier(config, P10BPlusWeatherMode.RainyDay), 0.001f);
        Assert.AreEqual(0.85f, P10BPlusWeatherMovementModifier.GetMultiplier(config, P10BPlusWeatherMode.NightClear), 0.001f);
        Assert.AreEqual(0.65f, P10BPlusWeatherMovementModifier.GetMultiplier(config, P10BPlusWeatherMode.NightRain), 0.001f);
        Assert.IsTrue(P10BPlusWeatherMovementModifier.IsNightMode(config, P10BPlusWeatherMode.NightClear));
        Assert.IsTrue(P10BPlusWeatherMovementModifier.IsNightMode(config, P10BPlusWeatherMode.NightRain));
    }

    [Test]
    public void MovementStaminaConfigUsesRequestedSpeedTargets()
    {
        P10BPlusMovementConfig config = P10BPlusDataLoader.LoadMovementStaminaConfig().data;

        Assert.IsTrue(config.IsSpeedConfigValid());
        Assert.AreEqual(0.5f, config.normalWalkSpeedMetersPerSecond, 0.001f);
        Assert.GreaterOrEqual(config.sprintSpeedMetersPerSecond, 2.0f);
        Assert.LessOrEqual(config.sprintSpeedMetersPerSecond, 3.0f);
    }

    [Test]
    public void StaminaDrainIsStagedAndRecoveryMilestonesAreDeterministic()
    {
        P10BPlusMovementConfig config = P10BPlusDataLoader.LoadMovementStaminaConfig().data;
        var controller = new P10BPlusStaminaController(config);

        float slow = controller.GetDrainRateForTesting(1f);
        float sustained = controller.GetDrainRateForTesting(5f);
        float exhaustion = controller.GetDrainRateForTesting(12f);
        Assert.Less(slow, sustained);
        Assert.Less(sustained, exhaustion);

        for (int i = 0; i < 30; i++)
        {
            controller.Tick(1f, true);
        }

        Assert.IsTrue(controller.State.sprintLocked);
        Assert.AreEqual(0f, controller.State.staminaFraction, 0.001f);

        controller.Tick(15f, false);
        Assert.IsFalse(controller.State.sprintLocked);
        Assert.GreaterOrEqual(controller.State.staminaFraction, 0.3f);

        controller.Tick(30f, false);
        Assert.GreaterOrEqual(controller.State.staminaFraction, 0.5f);

        controller.Tick(45f, false);
        Assert.AreEqual(1.0f, controller.State.staminaFraction, 0.001f);
    }

    [Test]
    public void WeatherAndStaminaComposeIntoMovementSpeed()
    {
        P10BPlusMovementConfig movement = P10BPlusDataLoader.LoadMovementStaminaConfig().data;
        P10BPlusWeatherConfig weather = P10BPlusDataLoader.LoadWeatherConfig().data;
        P10BPlusAvatarMobilityConfig avatar = P10BPlusDataLoader.LoadAvatarMobilityConfig().data;

        float clearSprint = P10BPlusMovementSpeedResolver.ResolveSpeed(
            movement,
            weather,
            avatar,
            P10BPlusAvatarPresentation.Female,
            "standard",
            P10BPlusWeatherMode.ClearDay,
            true);
        float nightRainSprint = P10BPlusMovementSpeedResolver.ResolveSpeed(
            movement,
            weather,
            avatar,
            P10BPlusAvatarPresentation.Female,
            "standard",
            P10BPlusWeatherMode.NightRain,
            true);

        Assert.AreEqual(2.5f, clearSprint, 0.001f);
        Assert.AreEqual(2.5f * 0.65f, nightRainSprint, 0.001f);
    }

    [Test]
    public void AvatarPresentationIsBalancedAndMobilityIsSeparateByDefault()
    {
        P10BPlusAvatarMobilityConfig config = P10BPlusDataLoader.LoadAvatarMobilityConfig().data;
        P10BPlusAvatarPresentation[] sample = Enumerable.Range(0, 100)
            .Select(index => P10BPlusPlayerProfileResolver.ResolveBalancedPresentation(42, index))
            .ToArray();

        Assert.IsTrue(config.IsEthicallySafeDefault());
        Assert.AreEqual(50, sample.Count(value => value == P10BPlusAvatarPresentation.Male));
        Assert.AreEqual(50, sample.Count(value => value == P10BPlusAvatarPresentation.Female));
        Assert.AreEqual(1.0f, P10BPlusPlayerProfileResolver.ResolveSpeedMultiplier(config, P10BPlusAvatarPresentation.Female, "standard"), 0.001f);

        config.enableGenderSpeedModifier = true;
        Assert.AreEqual(0.85f, P10BPlusPlayerProfileResolver.ResolveSpeedMultiplier(config, P10BPlusAvatarPresentation.Female, "standard"), 0.001f);
    }
}
