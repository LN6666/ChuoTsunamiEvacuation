using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class P9CCollapseResultLogEditModeTests
{
    [Test]
    public void CollapseDebrisExposureEventIsDeterministicBySeed()
    {
        P9CCollapseDebrisFatalityConfig config = P9CDataLoader.LoadCollapseDebrisFatalityConfig().data;
        P9CCollapseDebrisExposureEvent exposureEvent = CreateExposureEvent();

        P9CCollapseDebrisFatalityResult first = P9CCollapseDebrisFatalityEvaluator.Evaluate(config, exposureEvent, 0);
        P9CCollapseDebrisFatalityResult second = P9CCollapseDebrisFatalityEvaluator.Evaluate(config, exposureEvent, 0);

        Assert.AreEqual(first.deterministicRoll, second.deterministicRoll, 0.000001f);
        Assert.AreEqual(first.fatality, second.fatality);
        Assert.IsTrue(first.exposureEventLevelProbability);
        Assert.IsFalse(first.frameLevelRandomDeath);
    }

    [Test]
    public void DifferentSeedsCanChangeCollapseDebrisExposureOutcome()
    {
        P9CCollapseDebrisFatalityConfig config = P9CDataLoader.LoadCollapseDebrisFatalityConfig().data;
        P9CCollapseDebrisExposureEvent exposureEvent = CreateExposureEvent();
        bool foundFatal = false;
        bool foundSurvivor = false;

        for (int seed = 1; seed <= 200; seed++)
        {
            config.deterministicSeed = seed;
            P9CCollapseDebrisFatalityResult result = P9CCollapseDebrisFatalityEvaluator.Evaluate(config, exposureEvent, 0);
            foundFatal |= result.fatality;
            foundSurvivor |= !result.fatality;
        }

        Assert.IsTrue(foundFatal);
        Assert.IsTrue(foundSurvivor);
    }

    [Test]
    public void CollapseDebrisFatalityUsesConfiguredExposureEventProbability()
    {
        P9CCollapseDebrisFatalityConfig config = P9CDataLoader.LoadCollapseDebrisFatalityConfig().data;

        Assert.AreEqual(0.35f, config.collapseDebrisExposureFatalityProbability, 0.001f);
        Assert.IsTrue(config.scenarioConfigurable);
        Assert.IsFalse(config.frameLevelRandomDeath);
        Assert.IsTrue(P9CCollapseDebrisFatalityEvaluator.ExposureEventLevelProbability);
        Assert.IsFalse(P9CCollapseDebrisFatalityEvaluator.FrameLevelRandomDeath);
    }

    [Test]
    public void DisabledCollapseDebrisProxyNeverKillsPlayer()
    {
        P9CCollapseDebrisFatalityConfig config = P9CDataLoader.LoadCollapseDebrisFatalityConfig().data;
        config.enableCollapseDebrisFatalityProxy = false;
        P9CCollapseDebrisFatalityResult result = P9CCollapseDebrisFatalityEvaluator.Evaluate(config, CreateExposureEvent(), 0);

        Assert.IsFalse(result.fatality);
        Assert.IsFalse(result.playerOutcomeMutationApplied);
        Assert.AreEqual(P9COutcomeReasonCode.CollapseDebrisProxyDisabled, result.reasonCode);
    }

    [Test]
    public void ResultPanelFeedbackAndRunLogIncludeWarningsAndReasonCode()
    {
        P9COutcomeResult result = CreateLifeFirstOutcome();
        string feedback = P9CResultPanelFeedbackFormatter.Format(result);
        P9CRunLogRecord log = P9CRunLogRecord.FromOutcome(result);
        string json = log.ToJson();
        var metrics = new ResultMetrics
        {
            success = result.success,
            failureReason = result.finalReasonCode,
            p9cOutcomeFeedback = feedback,
            p9cFinalReasonCode = result.finalReasonCode
        };

        Assert.IsTrue(feedback.Contains(P9COutcomeReasonCode.SelectedLifeFirstVerticalCandidate) ||
            result.reasonCodes.Contains(P9COutcomeReasonCode.SelectedLifeFirstVerticalCandidate));
        Assert.IsTrue(feedback.Contains("Not an official evacuation shelter"));
        Assert.IsTrue(feedback.Contains(result.finalReasonCode));
        Assert.AreEqual(result.finalReasonCode, log.finalReasonCode);
        Assert.IsTrue(json.Contains(result.finalReasonCode));
        Assert.IsTrue(metrics.GetDetailText().Contains("P9-C final reason code"));
    }

    [Test]
    public void P5RouteGuardStillPreventsOfficialRouteClaim()
    {
        P9BRouteCandidateGeometryHandoff handoff = P9BDataLoader.LoadP8RouteCandidateGeometryHandoff().data;

        Assert.IsFalse(handoff.routesAreOfficial);
        Assert.IsTrue(handoff.routesAreEstimatedPrototypeGuidance);
        Assert.IsFalse(handoff.routeRoadGeometryValidated);
        Assert.IsFalse(P9RouteGuidanceProxyMarker.RoutesAreOfficial);
    }

    private static P9CCollapseDebrisExposureEvent CreateExposureEvent()
    {
        return new P9CCollapseDebrisExposureEvent
        {
            eventId = "p9c_test_exposure_event",
            zoneId = "p9b_collapse_proxy_tower_edge_001",
            riskZoneCategory = "collapse_debris_proxy",
            routeSegmentId = "p9c_route_segment_fixture",
            exposurePosition = new Vector3(30f, 0f, 40f),
            exposureTriggered = true,
            damagedBuildingState = true,
            exposureDistanceMeters = 3f
        };
    }

    private static P9COutcomeResult CreateLifeFirstOutcome()
    {
        P9CVerticalEvacuationTargetDecision decision = P9CLifeFirstTargetSelector.Select(
            P9CDataLoader.LoadVerticalEvacuationTargetRules().data);

        return new P9COutcomeResult
        {
            success = true,
            selectedTargetId = decision.selectedTargetId,
            selectedTargetType = decision.selectedTargetType,
            isOfficialShelter = false,
            nonOfficialWarningRequired = true,
            nonOfficialWarningText = decision.warningText,
            entranceStatus = "open",
            safeFloorStatus = "available",
            finalReasonCode = P9COutcomeReasonCode.VerticalEvacuationCompleteProxy,
            reasonCodes = new[]
            {
                P9COutcomeReasonCode.SelectedLifeFirstVerticalCandidate,
                P9COutcomeReasonCode.VerticalEvacuationCompleteProxy
            }
        };
    }
}
