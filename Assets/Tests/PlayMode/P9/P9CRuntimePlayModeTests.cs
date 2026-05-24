using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P9CRuntimePlayModeTests
{
    [UnityTest]
    public IEnumerator ResolverCompletesLifeFirstVerticalEvacuationWithoutSceneMutation()
    {
        GameObject root = new GameObject("P9C_PlayMode_ResolverRoot");
        try
        {
            P9COutcomeRulesConfig config = P9CDataLoader.LoadOutcomeRulesConfig().data;
            P9CVerticalEvacuationTargetRules targetRules = P9CDataLoader.LoadVerticalEvacuationTargetRules().data;
            P9CEntranceCongestionRules entranceRules = P9CDataLoader.LoadEntranceCongestionRules().data;
            P9CEntranceInteractionState entrance = entranceRules.entrances.First(record => record.entranceStatus == "open");
            P9CCollapseDebrisFatalityConfig collapseConfig = P9CDataLoader.LoadCollapseDebrisFatalityConfig().data;
            collapseConfig.enableCollapseDebrisFatalityProxy = false;

            P9COutcomeResult result = P9CVerticalEvacuationProxyResolver.Resolve(
                config,
                targetRules,
                entranceRules,
                entrance,
                P9CDataLoader.LoadSafeFloorProxyRules().data,
                collapseConfig,
                null);

            yield return null;

            Assert.IsTrue(result.success, result.summary);
            Assert.IsTrue(result.verticalEvacuationProxyCompleted);
            Assert.AreEqual(P9COutcomeReasonCode.DelayedSuccessBeforeHazardArrival, result.finalReasonCode);
            Assert.IsFalse(result.isOfficialShelter);
            Assert.IsTrue(result.nonOfficialWarningRequired);
            Assert.IsTrue(P9CResultPanelFeedbackFormatter.Format(result).Contains("Not an official evacuation shelter"));
            Assert.AreEqual("P9C_PlayMode_ResolverRoot", root.name);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator ResolverAllowsCrowdDelayToFailWhenSafetyWindowIsExceeded()
    {
        P9COutcomeRulesConfig config = P9CDataLoader.LoadOutcomeRulesConfig().data;
        config.defaultHazardArrivalTimeSeconds = 170f;
        P9CEntranceCongestionRules entranceRules = P9CDataLoader.LoadEntranceCongestionRules().data;
        P9CEntranceInteractionState crowded = entranceRules.entrances.First(record => record.entranceStatus == "crowded");
        P9CCollapseDebrisFatalityConfig collapseConfig = P9CDataLoader.LoadCollapseDebrisFatalityConfig().data;
        collapseConfig.enableCollapseDebrisFatalityProxy = false;

        P9COutcomeResult result = P9CVerticalEvacuationProxyResolver.Resolve(
            config,
            P9CDataLoader.LoadVerticalEvacuationTargetRules().data,
            entranceRules,
            crowded,
            P9CDataLoader.LoadSafeFloorProxyRules().data,
            collapseConfig,
            null);

        yield return null;

        Assert.IsFalse(result.success);
        Assert.AreEqual(P9COutcomeReasonCode.FailedDueToCrowdDelay, result.finalReasonCode);
        Assert.Greater(result.totalDelaySeconds, 0f);
        Assert.Contains(P9COutcomeReasonCode.QueueDelayApplied, result.reasonCodes);
    }

    [UnityTest]
    public IEnumerator ResolverAppliesCollapseDebrisFatalityOnlyForExposureEvent()
    {
        P9COutcomeRulesConfig config = P9CDataLoader.LoadOutcomeRulesConfig().data;
        P9CCollapseDebrisFatalityConfig collapseConfig = P9CDataLoader.LoadCollapseDebrisFatalityConfig().data;
        P9CCollapseDebrisExposureEvent exposureEvent = CreateExposureEvent();
        collapseConfig.deterministicSeed = FindFatalSeed(collapseConfig, exposureEvent);

        P9COutcomeResult result = P9CVerticalEvacuationProxyResolver.Resolve(
            config,
            P9CDataLoader.LoadVerticalEvacuationTargetRules().data,
            P9CDataLoader.LoadEntranceCongestionRules().data,
            P9CDataLoader.LoadEntranceCongestionRules().data.entrances.First(record => record.entranceStatus == "open"),
            P9CDataLoader.LoadSafeFloorProxyRules().data,
            collapseConfig,
            exposureEvent);

        yield return null;

        Assert.IsFalse(result.success);
        Assert.IsTrue(result.collapseDebrisExposureTriggered);
        Assert.IsTrue(result.collapseDebrisFatality);
        Assert.AreEqual(P9COutcomeReasonCode.KilledByBuildingCollapseProxy, result.finalReasonCode);
        Assert.AreEqual(0.35f, result.collapseDebrisFatalityProbability, 0.001f);
    }

    private static int FindFatalSeed(
        P9CCollapseDebrisFatalityConfig config,
        P9CCollapseDebrisExposureEvent exposureEvent)
    {
        for (int seed = 1; seed <= 200; seed++)
        {
            config.deterministicSeed = seed;
            if (P9CCollapseDebrisFatalityEvaluator.Evaluate(config, exposureEvent, 0).fatality)
            {
                return seed;
            }
        }

        Assert.Fail("No fatal deterministic seed found for collapse/debris fixture.");
        return config.deterministicSeed;
    }

    private static P9CCollapseDebrisExposureEvent CreateExposureEvent()
    {
        return new P9CCollapseDebrisExposureEvent
        {
            eventId = "p9c_playmode_exposure_event",
            zoneId = "p9b_collapse_proxy_tower_edge_001",
            riskZoneCategory = "collapse_debris_proxy",
            routeSegmentId = "p9c_playmode_route_segment",
            exposurePosition = new Vector3(30f, 0f, 40f),
            exposureTriggered = true,
            damagedBuildingState = true,
            exposureDistanceMeters = 3f
        };
    }
}
