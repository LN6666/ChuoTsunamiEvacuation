using NUnit.Framework;

public class P10CPrePerformanceGateEditModeTests
{
    [Test]
    public void PerformanceGateConfigIsPreReleaseAndSafe()
    {
        P10CPrePerformanceGateConfig config = P10CPreDataLoader.LoadPerformanceGateConfig().data;

        Assert.IsNotNull(config);
        Assert.IsTrue(config.IsSafePreReleaseGateConfig());
        Assert.IsTrue(config.finalReleaseBuildDeferredToP10C);
        Assert.IsTrue(config.finalReleasePackageDeferredToP10C);
        Assert.IsTrue(config.buildArtifactsMustStayUncommitted);
        Assert.IsFalse(config.trueChunkStreamingImplemented);
    }

    [Test]
    public void PerformanceGateClassifiesMeasuredLowProfilePass()
    {
        P10CPrePerformanceGateConfig config = P10CPreDataLoader.LoadPerformanceGateConfig().data;
        var metrics = new P10CPreRuntimeMetrics
        {
            measured = true,
            averageFps = 34f,
            onePercentLowFps = 24f,
            maxFrameTimeMs = 90f,
            frameSpikeCountOverThreshold = 2
        };

        string classification = config.Classify(metrics, 0, true, true);

        Assert.AreEqual("pass_for_p10c", classification);
    }

    [Test]
    public void QualityProfilesAreBoundedAndProduceSafeRuntimeOptimizerConfig()
    {
        P10CPreQualityProfileCollection profiles = P10CPreDataLoader.LoadQualityProfiles().data;

        Assert.IsNotNull(profiles);
        Assert.IsTrue(profiles.IsSafeProfileSet());
        Assert.IsNotNull(P10CPreQualityProfileApplier.FindProfile(profiles, "Low"));
        Assert.IsNotNull(P10CPreQualityProfileApplier.FindProfile(profiles, "Medium"));
        Assert.IsNotNull(P10CPreQualityProfileApplier.FindProfile(profiles, "High"));

        P10CPreQualityProfile low = P10CPreQualityProfileApplier.FindProfile(profiles, "Low");
        P10BPlusPlusOptimizationConfig optimizerConfig = P10CPreQualityProfileApplier.BuildRuntimeOptimizerConfig(low);

        Assert.IsTrue(optimizerConfig.IsSafeFinalOptimizationConfig());
        Assert.AreEqual("Low", optimizerConfig.defaultQualityPreset);
        Assert.IsFalse(optimizerConfig.lightCurtainEnabledByDefault);
        Assert.AreEqual(40, optimizerConfig.maxNpcCount);
        Assert.AreEqual(90, optimizerConfig.maxGreenFrameCount);
    }

    [Test]
    public void StagedActivationConfigCapsBatches()
    {
        var config = new P10CPreStagedActivationConfig
        {
            maxActivationsPerFrame = 24,
            prewarmBeforeHazardStartCount = 64
        };

        var cursor = new P10CPreStagedActivationCursor(50, config);

        Assert.IsTrue(config.IsSafeStagingConfig());
        Assert.AreEqual(3, config.EstimateFrameCount(50));
        Assert.AreEqual(24, cursor.TakeNextBatchSize());
        Assert.AreEqual(24, cursor.TakeNextBatchSize());
        Assert.AreEqual(2, cursor.TakeNextBatchSize());
        Assert.IsTrue(cursor.IsComplete);
    }

    [Test]
    public void DecisionAndChecklistDoNotClaimFinalReleaseWork()
    {
        P10CPreBuiltPlayerProfileSummary summary = P10CPreDataLoader.LoadBuiltPlayerProfileSummary().data;
        P10CPreOptimizationDecision decision = P10CPreDataLoader.LoadOptimizationDecision().data;
        P10CPreManualPlaytestChecklist checklist = P10CPreDataLoader.LoadManualPlaytestChecklist().data;

        Assert.IsNotNull(summary);
        Assert.IsTrue(summary.IsTemporaryBuildOnly());
        Assert.IsFalse(summary.finalReleasePackageCreated);
        Assert.AreEqual("needs_quick_fix", summary.readinessClassification);

        Assert.IsNotNull(decision);
        Assert.IsTrue(decision.IsHonestPreReleaseDecision());
        Assert.AreEqual("needs_quick_fix", decision.readinessDecision);
        Assert.IsFalse(decision.readyForP10CReleaseBuild);
        Assert.IsFalse(decision.trueChunkStreamingImplemented);

        Assert.IsNotNull(checklist);
        Assert.IsTrue(checklist.IsPreReleaseChecklist());
        Assert.IsFalse(checklist.finalReleasePackageCreated);
    }
}
