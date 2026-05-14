using NUnit.Framework;

public class AntiCampingConfigTests
{
    [Test]
    public void AntiCampingConfigFileCanBeLoaded()
    {
        GameConfigLoader.AntiCampingConfig config = GameConfigLoader.LoadAntiCampingConfig();

        Assert.NotNull(config);
    }

    [Test]
    public void AntiCampingConfigIsDisabledByDefault()
    {
        GameConfigLoader.AntiCampingConfig config = GameConfigLoader.LoadAntiCampingConfig();

        Assert.NotNull(config);
        Assert.IsFalse(config.antiCampingEnabled);
    }

    [Test]
    public void AntiCampingConfigUsesSafeDefaultTimingAndBlocking()
    {
        GameConfigLoader.AntiCampingConfig config = GameConfigLoader.LoadAntiCampingConfig();

        Assert.NotNull(config);
        Assert.Greater(config.preWarningCampingThresholdSeconds, 0f);
        Assert.IsTrue(config.blockCampedShelterForRound);
    }
}
