using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P9DFinalRuntimePlayModeTests
{
    [UnityTest]
    public IEnumerator CoordinateAnchorsCanGenerateRuntimeMarkersWithoutProtectedSceneDependency()
    {
        GameObject root = new GameObject("P9D_PlayMode_AnchorRoot");
        try
        {
            P9DCoordinateAnchoringConfig config = P9DDataLoader.LoadCoordinateAnchoringConfig().data;
            P9DAnchoringReport report = P9DNearestAnchorMatcher.BuildFinalAnchoringReport(
                config,
                P9BDataLoader.LoadP8HumanitarianCandidateMarkers().data);

            for (int i = 0; i < 6; i++)
            {
                GameObject marker = new GameObject("P9D_Anchor_" + i);
                marker.transform.SetParent(root.transform, false);
                P9DCoordinateAnchor anchor = marker.AddComponent<P9DCoordinateAnchor>();
                anchor.ApplyResult(report.results[i]);
            }

            yield return null;

            Assert.AreEqual(6, root.transform.childCount);
            P9DCoordinateAnchor first = root.GetComponentInChildren<P9DCoordinateAnchor>();
            Assert.IsNotNull(first);
            Assert.IsTrue(first.CoordinateBasedProxy);
            Assert.IsFalse(first.ExactPlateauObjectIdentityProven);
            Assert.IsFalse(first.RouteIsOfficial);
            Assert.IsTrue(report.allHumanitarianCandidatesRequireWarning);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator FullP9FinalGameplayFlowRunsSceneSafeRuntimeValidation()
    {
        GameObject root = new GameObject("P9D_PlayMode_FinalFlowRoot");
        try
        {
            P9DAnchoringReport anchoringReport = P9DNearestAnchorMatcher.BuildFinalAnchoringReport(
                P9DDataLoader.LoadCoordinateAnchoringConfig().data,
                P9BDataLoader.LoadP8HumanitarianCandidateMarkers().data);
            P9DFinalGameplayFlowSummary summary = P9DFinalGameplayFlowValidator.Validate(
                P9DDataLoader.LoadFinalGameplayScenarioSample().data,
                anchoringReport);

            yield return null;

            Assert.IsTrue(summary.success, summary.summary);
            Assert.IsTrue(summary.scenarioResults.Any(result => result.finalReasonCode == P9COutcomeReasonCode.EntranceBlockedFailure));
            Assert.IsTrue(summary.scenarioResults.Any(result => result.finalReasonCode == P9COutcomeReasonCode.FailedDueToCrowdDelay));
            Assert.IsTrue(summary.scenarioResults.Any(result => result.finalReasonCode == P9COutcomeReasonCode.KilledByBuildingCollapseProxy));
            Assert.IsFalse(summary.exactPlateauObjectIdentityClaimed);
            Assert.IsTrue(summary.routesRemainEstimatedPrototypeGuidance);
            Assert.AreEqual("P9D_PlayMode_FinalFlowRoot", root.name);
        }
        finally
        {
            Object.Destroy(root);
        }
    }
}
