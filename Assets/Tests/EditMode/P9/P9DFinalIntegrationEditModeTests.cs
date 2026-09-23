using System.Linq;
using NUnit.Framework;

public class P9DFinalIntegrationEditModeTests
{
    [Test]
    public void FinalGameplayFlowCoversSuccessDelayAndFailurePaths()
    {
        P9DAnchoringReport anchoringReport = BuildReport();
        P9DFinalGameplayScenarioCollection scenarios = P9DDataLoader.LoadFinalGameplayScenarioSample().data;

        P9DFinalGameplayFlowSummary summary = P9DFinalGameplayFlowValidator.Validate(scenarios, anchoringReport);

        Assert.IsTrue(summary.success, summary.summary);
        Assert.AreEqual(7, summary.scenarioCount);
        Assert.AreEqual(7, summary.passedScenarioCount);
        Assert.IsTrue(summary.p8HandoffConsumed);
        Assert.IsTrue(summary.anchoringReportPresent);
        Assert.AreEqual(110, summary.humanitarianCandidateTotal);
        Assert.IsTrue(summary.nonOfficialWarningsPreserved);
        Assert.IsTrue(summary.routesRemainEstimatedPrototypeGuidance);
        Assert.IsFalse(summary.exactPlateauObjectIdentityClaimed);
        Assert.IsTrue(summary.resultPanelFeedbackAvailable);
    }

    [Test]
    public void FinalGameplayFlowProducesExpectedReasonCodes()
    {
        P9DAnchoringReport anchoringReport = BuildReport();
        P9DFinalGameplayFlowSummary summary = P9DFinalGameplayFlowValidator.Validate(
            P9DDataLoader.LoadFinalGameplayScenarioSample().data,
            anchoringReport);

        AssertScenario(summary, "p9d_flow_success", true, P9COutcomeReasonCode.DelayedSuccessBeforeHazardArrival);
        AssertScenario(summary, "p9d_flow_congestion_delay", true, P9COutcomeReasonCode.DelayedSuccessBeforeHazardArrival);
        AssertScenario(summary, "p9d_flow_entrance_blocked_failure", false, P9COutcomeReasonCode.EntranceBlockedFailure);
        AssertScenario(summary, "p9d_flow_safe_floor_failure", false, P9COutcomeReasonCode.SafeFloorUnavailableFailure);
        AssertScenario(summary, "p9d_flow_crowd_delay_hazard_failure", false, P9COutcomeReasonCode.FailedDueToCrowdDelay);
        AssertScenario(summary, "p9d_flow_collapse_debris_fatality", false, P9COutcomeReasonCode.KilledByBuildingCollapseProxy);
        AssertScenario(summary, "p9d_flow_collapse_disabled_success", true, P9COutcomeReasonCode.DelayedSuccessBeforeHazardArrival);
    }

    [Test]
    public void ResultPanelFeedbackAndRunLogCarryFinalReasonCodes()
    {
        P9DFinalGameplayScenarioRecord scenario = P9DDataLoader.LoadFinalGameplayScenarioSample().data.scenarios
            .First(record => record.scenarioId == "p9d_flow_collapse_debris_fatality");

        P9DFinalGameplayScenarioResult result = P9DFinalGameplayFlowValidator.RunScenario(scenario);

        Assert.IsTrue(result.success, result.summary);
        Assert.IsTrue(result.resultPanelFeedback.Contains(P9COutcomeReasonCode.KilledByBuildingCollapseProxy));
        Assert.IsTrue(result.resultPanelFeedback.Contains("Gameplay-level collapse/debris proxy"));
        Assert.IsTrue(result.runLogJson.Contains(P9COutcomeReasonCode.KilledByBuildingCollapseProxy));
        Assert.IsTrue(result.collapseDebrisExposureTriggered);
        Assert.IsTrue(result.collapseDebrisFatality);
    }

    [Test]
    public void CollapseDebrisDisabledScenarioNeverKills()
    {
        P9DFinalGameplayScenarioRecord scenario = P9DDataLoader.LoadFinalGameplayScenarioSample().data.scenarios
            .First(record => record.scenarioId == "p9d_flow_collapse_disabled_success");

        P9DFinalGameplayScenarioResult result = P9DFinalGameplayFlowValidator.RunScenario(scenario);

        Assert.IsTrue(result.success, result.summary);
        Assert.IsTrue(result.outcomeSuccess);
        Assert.IsTrue(result.collapseDebrisExposureTriggered);
        Assert.IsFalse(result.collapseDebrisFatality);
    }

    [Test]
    public void P8HandoffAndRouteGuardsRemainFinalP9Boundaries()
    {
        P9BP8HandoffAvailability availability = P9BDataLoader.VerifyP8HandoffAvailability();
        P9BRouteCandidateGeometryHandoff route = P9BDataLoader.LoadP8RouteCandidateGeometryHandoff().data;

        Assert.IsTrue(availability.allExpectedFilesPresent);
        Assert.IsTrue(route.routesAreEstimatedPrototypeGuidance);
        Assert.IsFalse(route.routesAreOfficial);
        Assert.IsFalse(route.routeRoadGeometryValidated);
        Assert.IsFalse(P9RouteGuidanceProxyMarker.RoutesAreOfficial);
    }

    [Test]
    public void P10HandoffStatusIsReadyWithoutStartingP10Packaging()
    {
        P9DAnchoringReport anchoringReport = BuildReport();
        P9DFinalGameplayFlowSummary flow = P9DFinalGameplayFlowValidator.Validate(
            P9DDataLoader.LoadFinalGameplayScenarioSample().data,
            anchoringReport);
        P9DP10HandoffStatus handoff = P9DDataLoader.LoadP10HandoffStatus().data;

        P9DP10HandoffSummaryResult summary = P9DP10HandoffSummary.Create(anchoringReport, flow, handoff);

        Assert.IsTrue(summary.readyForP10, summary.summary);
        Assert.IsTrue(summary.p9Complete);
        Assert.IsFalse(summary.p9EfgCreated);
        Assert.IsFalse(summary.p10ReleasePackagingStarted);
        Assert.IsTrue(summary.windowsExeProfilingRequiredInP10);
        Assert.IsTrue(summary.highDetailSceneArchiveRequiredInP10);
        Assert.IsTrue(summary.officialRouteValidationDeferred);
        Assert.IsTrue(summary.fullPlateauObjectSemanticCoverageDeferred);
    }

    private static P9DAnchoringReport BuildReport()
    {
        return P9DNearestAnchorMatcher.BuildFinalAnchoringReport(
            P9DDataLoader.LoadCoordinateAnchoringConfig().data,
            P9BDataLoader.LoadP8HumanitarianCandidateMarkers().data);
    }

    private static void AssertScenario(
        P9DFinalGameplayFlowSummary summary,
        string scenarioId,
        bool expectedOutcomeSuccess,
        string expectedReasonCode)
    {
        P9DFinalGameplayScenarioResult result = summary.scenarioResults.First(record => record.scenarioId == scenarioId);

        Assert.IsTrue(result.success, result.summary);
        Assert.AreEqual(expectedOutcomeSuccess, result.outcomeSuccess);
        Assert.AreEqual(expectedReasonCode, result.finalReasonCode);
    }
}
