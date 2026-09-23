using System.Linq;
using NUnit.Framework;

public class P9CLifeFirstEntranceSafeFloorEditModeTests
{
    [Test]
    public void LifeFirstSelectorCanSelectEligibleNonOfficialCandidateWithWarning()
    {
        P9CVerticalEvacuationTargetDecision decision = P9CLifeFirstTargetSelector.Select(
            P9CDataLoader.LoadVerticalEvacuationTargetRules().data);

        Assert.IsTrue(decision.selected);
        Assert.AreEqual(P9COutcomeReasonCode.SelectedLifeFirstVerticalCandidate, decision.reasonCode);
        Assert.IsFalse(decision.isOfficialShelter);
        Assert.IsTrue(decision.isHumanitarianCandidate);
        Assert.IsTrue(decision.nonOfficialWarningRequired);
        Assert.IsTrue(decision.remainsNonOfficialAndWarningRequired);
        Assert.IsTrue(decision.IsEthicallyWarningSafe());
        Assert.IsFalse(decision.routeIsOfficial);
        Assert.IsTrue(decision.routeIsEstimatedPrototypeGuidance);
    }

    [Test]
    public void SelectorRejectsBlockedLowFloorUnsafeAndMissingSafeFloorCandidates()
    {
        P9CVerticalEvacuationTargetDecision decision = P9CLifeFirstTargetSelector.Select(
            P9CDataLoader.LoadVerticalEvacuationTargetRules().data);
        string[] rejectedCodes = decision.rejectedTargets.Select(record => record.reasonCode).ToArray();

        Assert.Contains(P9COutcomeReasonCode.RejectedNonOfficialCandidateBlocked, rejectedCodes);
        Assert.Contains(P9COutcomeReasonCode.RejectedNonOfficialCandidateLowFloorWarning, rejectedCodes);
        Assert.Contains(P9COutcomeReasonCode.RejectedCandidateMissingSafeFloorProxy, rejectedCodes);
    }

    [Test]
    public void OfficialShelterSelectionStillWorksWhenOfficialAccessIsUsable()
    {
        P9CVerticalEvacuationTargetRules rules = P9CDataLoader.LoadVerticalEvacuationTargetRules().data;
        rules.officialShelterAccessUnsafeOrUnavailable = false;

        P9CVerticalEvacuationTargetDecision decision = P9CLifeFirstTargetSelector.Select(rules);

        Assert.IsTrue(decision.selected);
        Assert.IsTrue(decision.isOfficialShelter);
        Assert.IsFalse(decision.nonOfficialWarningRequired);
        Assert.AreEqual(P9COutcomeReasonCode.SelectedOfficialShelter, decision.reasonCode);
    }

    [Test]
    public void EntranceOpenLeadsToDeterministicNonFailureDelayEvaluation()
    {
        P9CEntranceCongestionRules rules = P9CDataLoader.LoadEntranceCongestionRules().data;
        var state = new P9CEntranceInteractionState
        {
            entranceProxyId = "p9c_open_no_queue",
            entranceStatus = "open",
            queueLength = 0,
            crowdDensity01 = 0f,
            routeCongestionScore01 = 0f,
            baseInteractionDelaySeconds = 0f
        };

        P9CEntranceCongestionResult result = P9CEntranceCongestionEvaluator.Evaluate(state, rules, 300f);

        Assert.IsTrue(result.success, result.summary);
        Assert.IsFalse(result.failure);
        Assert.AreEqual(0f, result.totalDelaySeconds, 0.001f);
        Assert.AreEqual(P9COutcomeReasonCode.EntranceOpen, result.reasonCode);
    }

    [Test]
    public void EntranceCrowdedAppliesDeterministicBoundedDelay()
    {
        P9CEntranceCongestionRules rules = P9CDataLoader.LoadEntranceCongestionRules().data;
        P9CEntranceInteractionState crowded = rules.entrances.Single(record => record.entranceStatus == "crowded");

        P9CEntranceCongestionResult first = P9CEntranceCongestionEvaluator.Evaluate(crowded, rules, 300f);
        P9CEntranceCongestionResult second = P9CEntranceCongestionEvaluator.Evaluate(crowded, rules, 300f);

        Assert.IsTrue(first.success, first.summary);
        Assert.AreEqual(first.totalDelaySeconds, second.totalDelaySeconds, 0.001f);
        Assert.Greater(first.queueDelaySeconds, 0f);
        Assert.Greater(first.congestionDelaySeconds, 0f);
        Assert.LessOrEqual(first.totalDelaySeconds, rules.maxDelayCapSeconds);
        Assert.Contains(P9COutcomeReasonCode.QueueDelayApplied, first.reasonCodes);
    }

    [Test]
    public void EntranceBlockedCanFailOnlyWhenConfigured()
    {
        P9CEntranceCongestionRules rules = P9CDataLoader.LoadEntranceCongestionRules().data;
        P9CEntranceInteractionState blocked = rules.entrances.Single(record => record.entranceStatus == "blocked");

        P9CEntranceCongestionResult failing = P9CEntranceCongestionEvaluator.Evaluate(blocked, rules, 300f);
        rules.failWhenBlocked = false;
        P9CEntranceCongestionResult nonFailing = P9CEntranceCongestionEvaluator.Evaluate(blocked, rules, 300f);

        Assert.IsTrue(failing.failure);
        Assert.AreEqual(P9COutcomeReasonCode.EntranceBlockedFailure, failing.reasonCode);
        Assert.IsFalse(nonFailing.failure);
    }

    [Test]
    public void SafeFloorStatusesResolveToExpectedProxyOutcomes()
    {
        P9CSafeFloorProxyRules rules = P9CDataLoader.LoadSafeFloorProxyRules().data;

        P9CSafeFloorEvaluationResult available = P9CSafeFloorEvaluator.Evaluate("available", rules);
        P9CSafeFloorEvaluationResult unavailable = P9CSafeFloorEvaluator.Evaluate("unavailable", rules);
        P9CSafeFloorEvaluationResult belowRequired = P9CSafeFloorEvaluator.Evaluate("below_required_height", rules);
        P9CSafeFloorEvaluationResult unknown = P9CSafeFloorEvaluator.Evaluate("unknown", rules);

        Assert.IsTrue(available.success);
        Assert.IsTrue(available.verticalEvacuationProxyCompleted);
        Assert.AreEqual(P9COutcomeReasonCode.SafeFloorAvailableSuccess, available.reasonCode);
        Assert.IsTrue(unavailable.failure);
        Assert.AreEqual(P9COutcomeReasonCode.SafeFloorUnavailableFailure, unavailable.reasonCode);
        Assert.IsTrue(belowRequired.failure);
        Assert.AreEqual(P9COutcomeReasonCode.SafeFloorBelowRequiredHeightFailure, belowRequired.reasonCode);
        Assert.IsTrue(unknown.warning);
        Assert.AreEqual(P9COutcomeReasonCode.SafeFloorUnknownWarning, unknown.reasonCode);
    }
}
