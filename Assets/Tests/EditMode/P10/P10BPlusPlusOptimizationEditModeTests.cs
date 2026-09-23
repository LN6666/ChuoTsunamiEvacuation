using NUnit.Framework;

public class P10BPlusPlusOptimizationEditModeTests
{
    [Test]
    public void OptimizationConfigIsBoundedAndDoesNotClaimReleaseWork()
    {
        P10BPlusPlusOptimizationConfig config = P10BPlusPlusDataLoader.LoadOptimizationConfig().data;

        Assert.IsNotNull(config);
        Assert.IsTrue(config.IsSafeFinalOptimizationConfig());
        Assert.IsTrue(config.finalWindowsExeBuildDeferredToP10C);
        Assert.IsTrue(config.noAddressablesOrPackagesAdded);
        Assert.IsFalse(config.projectSettingsChangeRequired);
        Assert.IsFalse(config.mutatesPlateauAssets);
        Assert.IsFalse(config.mutatesHighDetailScene);
        Assert.IsFalse(config.createsReleaseOrArchiveArtifacts);
    }

    [Test]
    public void MetricsRingBufferCapsSamplesAndReportsSpikes()
    {
        var ringBuffer = new P10BPlusPlusMetricsRingBuffer(3);

        ringBuffer.AddFrameTime(0.016f);
        ringBuffer.AddFrameTime(0.020f);
        ringBuffer.AddFrameTime(0.080f);
        ringBuffer.AddFrameTime(0.040f);

        P10BPlusPlusFrameTimeSummary summary = ringBuffer.CreateSummary(50f);

        Assert.AreEqual(3, summary.capacity);
        Assert.AreEqual(3, summary.sampleCount);
        Assert.AreEqual(1, summary.frameSpikeCountOverThreshold);
        Assert.Greater(summary.averageFps, 0f);
        Assert.Less(summary.onePercentLowFps, summary.averageFps);
    }

    [Test]
    public void P10BMetricsSummaryHonorsConfiguredSpikeThreshold()
    {
        P10BPerformanceSampleSummary summary = P10BPerformanceMetricsRecorder.CreateSummaryFromFrameTimes(
            "p10b_plus_plus_threshold",
            new[] { 0.016f, 0.060f, 0.080f },
            0,
            0,
            0,
            false,
            0,
            0,
            1f,
            70f);

        Assert.AreEqual(70f, summary.frameSpikeThresholdMs, 0.001f);
        Assert.AreEqual(1, summary.frameSpikeCountOverThreshold);
    }

    [Test]
    public void QualityRecommendationsAreBoundedAndDoNotClaimAaMode()
    {
        P10BPlusPlusQualityRecommendationCollection recommendations = P10BPlusPlusDataLoader.LoadQualityRecommendations().data;

        Assert.IsNotNull(recommendations);
        Assert.IsFalse(recommendations.projectSettingsChangeRequired);
        Assert.IsFalse(recommendations.antiAliasingModeClaimed);
        Assert.IsNotNull(P10BPlusPlusQualityPresetAdvisor.FindPreset(recommendations, "Low"));
        Assert.IsNotNull(P10BPlusPlusQualityPresetAdvisor.FindPreset(recommendations, "Medium"));
        Assert.IsNotNull(P10BPlusPlusQualityPresetAdvisor.FindPreset(recommendations, "High"));
        for (int i = 0; i < recommendations.presets.Length; i++)
        {
            Assert.IsTrue(recommendations.presets[i].IsBounded());
            Assert.IsFalse(recommendations.presets[i].debugLabelsEnabled);
        }
    }

    [Test]
    public void QualityRuntimeAuditDoesNotOverclaimAntiAliasingWhenOff()
    {
        string status = P10BPlusPlusQualityPresetAdvisor.DescribeAntiAliasing(0);

        Assert.That(status, Does.Contain("no MSAA is confirmed"));
    }

    [Test]
    public void PerformanceAuditSampleCarriesManualCpuAndPagingPolicy()
    {
        P10BPlusPlusPerformanceAuditSample sample = P10BPlusPlusDataLoader.LoadPerformanceAuditSample().data;

        Assert.IsTrue(sample.finalWindowsExeBuildDeferredToP10C);
        Assert.That(sample.cpuMeasurementPolicy, Does.Contain("OS-level CPU"));
        Assert.That(sample.diskPagingMeasurementPolicy, Does.Contain("Disk paging"));
    }
}
