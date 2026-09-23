using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class P10BGreenGroundFrameEditModeTests
{
    [Test]
    public void GreenGroundFrameConfigLoadsAsRuntimeSafeTsunamiStartFeature()
    {
        P10BGreenGroundFrameConfig config = P10BDataLoader.LoadGreenGroundFrameConfig().data;

        Assert.IsNotNull(config);
        Assert.IsTrue(config.IsRuntimeSafe());
        Assert.IsTrue(config.tsunamiStartTriggered);
        Assert.IsTrue(config.runtimeGenerated);
        Assert.IsFalse(config.debugPreviewBeforeTsunamiStart);
        Assert.IsFalse(config.mutatesPlateauAssets);
        Assert.IsFalse(config.mutatesHighDetailScene);
        Assert.IsFalse(config.claimsExactBuildingFootprint);
        Assert.IsFalse(config.claimsOfficialApprovalForHumanitarianCandidates);
    }

    [Test]
    public void BuildsGreenFrameTargetsFromP9DAnchoringReport()
    {
        P10BGreenGroundFrameConfig config = P10BDataLoader.LoadGreenGroundFrameConfig().data;
        P9DAnchoringReport report = P9DNearestAnchorMatcher.BuildFinalAnchoringReport(
            P9DDataLoader.LoadCoordinateAnchoringConfig().data,
            P9BDataLoader.LoadP8HumanitarianCandidateMarkers().data);

        P10BGreenGroundFrameTarget[] targets = P10BGreenGroundFrameGenerator.BuildTargetsFromAnchoringReport(report, config);

        Assert.AreEqual(111, targets.Length);
        Assert.AreEqual(110, targets.Count(target => target.isHumanitarianCandidate));
        Assert.AreEqual(1, targets.Count(target => target.isOfficialShelter));
        Assert.IsTrue(targets.Where(target => target.isHumanitarianCandidate).All(target => target.PreservesHumanitarianSemantics));
        Assert.IsTrue(targets.All(target => target.UsesProxyRectangle));
    }

    [Test]
    public void NonOfficialHumanitarianTargetCreatesFrameTargetWithoutOfficialClaim()
    {
        P10BGreenGroundFrameTargetCollection sample = P10BDataLoader.LoadGreenGroundFrameTargetsSample().data;
        P10BGreenGroundFrameTarget candidate = sample.targets.First(target => target.isHumanitarianCandidate);

        Assert.IsFalse(candidate.isOfficialShelter);
        Assert.IsTrue(candidate.nonOfficialWarningRequired);
        Assert.IsFalse(candidate.safeApprovedByDefault);
        Assert.IsTrue(candidate.PreservesHumanitarianSemantics);
        Assert.IsTrue(candidate.warningText.Contains("Not an official evacuation shelter"));
    }

    [Test]
    public void InvalidOrUnsafeProxyTargetIsRejected()
    {
        var target = new P10BGreenGroundFrameTarget
        {
            targetId = "p10b_invalid",
            targetType = "non_official_humanitarian_vertical_candidate",
            isHumanitarianCandidate = true,
            nonOfficialWarningRequired = true,
            hasCoordinate = true,
            latitude = float.NaN,
            longitude = 139.77f,
            proxyCenter = Vector3.zero,
            footprintWidthMeters = 20f,
            footprintDepthMeters = 20f
        };

        Assert.IsFalse(target.IsValidForFrame(new P10BGreenGroundFrameConfig()));
    }

    [Test]
    public void PerformanceSummaryCalculatesAverageAndOnePercentLowFps()
    {
        P10BPerformanceSampleSummary summary = P10BPerformanceMetricsRecorder.CreateSummaryFromFrameTimes(
            "p10b_unit_metrics",
            new[] { 0.016f, 0.016f, 0.020f, 0.040f },
            24,
            111,
            111,
            true,
            1,
            0,
            2.5f);

        Assert.AreEqual("p10b_unit_metrics", summary.scenarioId);
        Assert.AreEqual(4, summary.sampleCount);
        Assert.Greater(summary.averageFps, 40f);
        Assert.Less(summary.onePercentLowFps, summary.averageFps);
        Assert.AreEqual(111, summary.greenGroundFrameCount);
        Assert.IsTrue(summary.summary.Contains("avgFPS"));
    }

    [Test]
    public void StressScenariosAreBoundedAndCoverRequiredSmokeCases()
    {
        P10BStressScenarioCollection scenarios = P10BDataLoader.LoadStressScenarios().data;
        string[] ids = scenarios.scenarios.Select(scenario => scenario.scenarioId).ToArray();

        Assert.IsTrue(scenarios.finalWindowsExeBuildDeferredToP10C);
        Assert.IsTrue(scenarios.noThousandsOfNpcs);
        Assert.IsTrue(scenarios.scenarios.All(scenario => scenario.IsBounded()));
        CollectionAssert.Contains(ids, "baseline_low_marker_count");
        CollectionAssert.Contains(ids, "all_candidates_green_frames_after_tsunami_start");
        CollectionAssert.Contains(ids, "crowd_congestion_high_bounded");
        CollectionAssert.Contains(ids, "light_curtain_green_frames_combined");
        CollectionAssert.Contains(ids, "result_panel_long_warning");
        CollectionAssert.Contains(ids, "debug_labels_disabled_performance");
    }
}
