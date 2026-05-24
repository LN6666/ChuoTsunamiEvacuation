using System.IO;
using System.Linq;
using NUnit.Framework;

public class P9CDataAndBoundaryEditModeTests
{
    [Test]
    public void P9CJsonConfigsLoadAndStayWithinProxyBoundaries()
    {
        P9COutcomeRulesConfig config = P9CDataLoader.LoadOutcomeRulesConfig().data;
        P9CVerticalEvacuationTargetRules targetRules = P9CDataLoader.LoadVerticalEvacuationTargetRules().data;
        P9CEntranceCongestionRules entranceRules = P9CDataLoader.LoadEntranceCongestionRules().data;
        P9CSafeFloorProxyRules safeFloorRules = P9CDataLoader.LoadSafeFloorProxyRules().data;
        P9CCollapseDebrisFatalityConfig collapseConfig = P9CDataLoader.LoadCollapseDebrisFatalityConfig().data;
        P9CScenarioFailurePresetCollection presets = P9CDataLoader.LoadScenarioFailurePresets().data;

        Assert.IsTrue(config.IsGameplayProxySafe());
        Assert.IsTrue(config.enableOutcomeMutationProxy);
        Assert.IsFalse(config.claimsOfficialRouteStatus);
        Assert.IsFalse(config.importsExternalCrowdPackage);
        Assert.IsFalse(config.reimplementsP8HazardModel);
        Assert.GreaterOrEqual(targetRules.targets.Length, 5);
        Assert.GreaterOrEqual(entranceRules.entrances.Length, 4);
        Assert.Contains("below_required_height", safeFloorRules.supportedStatuses);
        Assert.AreEqual(0.35f, collapseConfig.collapseDebrisExposureFatalityProbability, 0.001f);
        Assert.IsFalse(collapseConfig.frameLevelRandomDeath);
        Assert.GreaterOrEqual(presets.presets.Length, 4);
    }

    [Test]
    public void ReasonCodeCatalogContainsAllRequiredRuntimeCodes()
    {
        P9CReasonCodeCatalog catalog = P9CDataLoader.LoadReasonCodeCatalog().data;
        string[] catalogCodes = catalog.reasonCodes.Select(record => record.reasonCode).ToArray();

        foreach (string reasonCode in P9COutcomeReasonCode.All)
        {
            Assert.Contains(reasonCode, catalogCodes, reasonCode);
            Assert.IsTrue(P9COutcomeReasonCode.IsKnown(reasonCode), reasonCode);
        }
    }

    [Test]
    public void P8HumanitarianCandidatesRemainNonOfficialAndWarningRequired()
    {
        P9BHumanitarianCandidateMarkerCollection collection = P9BDataLoader.LoadP8HumanitarianCandidateMarkers().data;

        Assert.AreEqual(110, collection.records.Length);
        Assert.IsTrue(collection.records.All(record => !record.isOfficialShelter));
        Assert.IsTrue(collection.records.All(record => record.nonOfficialWarningRequired));
        Assert.IsTrue(collection.records.All(record => !record.selectableGameplayEnabled));
        Assert.IsTrue(collection.records.All(record => !record.affectsGameplaySuccessFailure));
    }

    [Test]
    public void P9CReferenceReviewUsesReferenceOnlyPolicy()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "docs", "P9C_REFERENCE_MODEL_REVIEW.md");
        Assert.IsTrue(File.Exists(path), "P9-C reference review doc is required.");

        string text = File.ReadAllText(path);
        Assert.IsTrue(text.Contains("reference_only"));
        Assert.IsTrue(text.Contains("JR-Morgan/Crowd-Evacuation-Simulation"));
        Assert.IsTrue(text.Contains("TUNAMI-EVAC"));
        Assert.IsTrue(text.Contains("armostafizi/EvacuationModel"));
        Assert.IsTrue(text.Contains("fabhiansan/tsunami_simulation"));
        Assert.IsTrue(text.Contains("Project-PLATEAU/evacuation-simulation-tools"));
        Assert.IsTrue(text.Contains("Social Force Model"));
        Assert.IsTrue(text.Contains("RVO/ORCA"));
        Assert.IsFalse(text.Contains("imported package"));
    }

    [Test]
    public void P9CPolicyDoesNotClaimHeavySimulationOrOfficialRoutes()
    {
        Assert.IsTrue(P9RuntimePolicy.CanCausePlayerFailureInP9C);
        Assert.IsTrue(P9RuntimePolicy.P9CDeterministicRuleBasedProxy);
        Assert.IsTrue(P9RuntimePolicy.P9COutcomeMutationProxyEnabled);
        Assert.IsFalse(P9RuntimePolicy.ClaimsOfficialRoutes);
        Assert.IsFalse(P9RuntimePolicy.ClaimsOfficialHumanitarianCandidateShelters);
        Assert.IsFalse(P9RuntimePolicy.P9CImportsExternalCrowdPackage);
        Assert.IsFalse(P9RuntimePolicy.P9CImplementsFullSocialSimulation);
        Assert.IsFalse(P9RuntimePolicy.P9CImplementsCalibratedSocialForceModel);
        Assert.IsFalse(P9RuntimePolicy.P9CImplementsOrcaRvoNavigation);
    }
}
