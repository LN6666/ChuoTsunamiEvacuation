using NUnit.Framework;

public class P10CMMFpsStutterExporterEditModeTests
{
    [Test]
    public void FrameSamplerCalculatesOnePercentLowAndSpikeBuckets()
    {
        var sampler = new P10CMMFrameSampler(10);
        sampler.Add(0.016f);
        sampler.Add(0.020f);
        sampler.Add(0.034f);
        sampler.Add(0.052f);
        sampler.Add(0.120f);

        P10CMMFpsStutterSummary summary = sampler.CreateSummary(
            "editmode_sample",
            1f,
            1f,
            50f);

        Assert.AreEqual("captured", summary.status);
        Assert.AreEqual(5, summary.sampleCount);
        Assert.AreEqual(3, summary.frameSpikeCountOver33Ms);
        Assert.AreEqual(2, summary.frameSpikeCountOver50Ms);
        Assert.AreEqual(1, summary.frameSpikeCountOver100Ms);
        Assert.AreEqual(2, summary.stutterEventCount);
        Assert.Greater(summary.averageFps, 0f);
        Assert.Less(summary.onePercentLowFps, summary.averageFps);
    }

    [Test]
    public void FrameSamplerCapsSamplesWithoutGrowingMemory()
    {
        var sampler = new P10CMMFrameSampler(3);
        sampler.Add(0.016f);
        sampler.Add(0.017f);
        sampler.Add(0.018f);
        sampler.Add(0.019f);

        P10CMMFpsStutterSummary summary = sampler.CreateSummary(
            "capped_sample",
            1f,
            1f,
            50f);

        Assert.AreEqual(3, summary.sampleCapacity);
        Assert.AreEqual(3, summary.sampleCount);
        Assert.AreEqual(0, summary.frameSpikeCountOver50Ms);
    }

    [Test]
    public void ExporterIsExplicitOptInAndAutoQuitDefaultsOff()
    {
        Assert.IsFalse(P10CMMFpsStutterExporter.ShouldInstallForCommandLine(new[] { "player.exe" }));
        Assert.IsTrue(P10CMMFpsStutterExporter.ShouldInstallForCommandLine(new[] { "player.exe", P10CMMFpsStutterExporter.EnableArg }));
        Assert.IsFalse(P10CMMFpsStutterExporter.ShouldInstallForCommandLine(new[] { "player.exe", P10CMMFpsStutterExporter.EnableArg, "-p10cMmDisableFpsExporter" }));
        Assert.IsFalse(P10CMMFpsStutterExporter.ShouldAutoQuitForCommandLine(new[] { "player.exe", P10CMMFpsStutterExporter.EnableArg }));
        Assert.IsTrue(P10CMMFpsStutterExporter.ShouldAutoQuitForCommandLine(new[] { "player.exe", P10CMMFpsStutterExporter.EnableArg, P10CMMFpsStutterExporter.AutoQuitArg }));
    }
}
