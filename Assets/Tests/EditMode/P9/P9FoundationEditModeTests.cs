using System.IO;
using System.Linq;
using NUnit.Framework;

public class P9FoundationEditModeTests
{
    [Test]
    public void SpawnSampleLoads()
    {
        P9SpawnPointLoadResult result = P9SpawnPointLoader.LoadSampleSpawnPoints();

        Assert.IsTrue(result.success, string.Join("\n", result.errors));
        Assert.IsFalse(result.failSafe);
        Assert.AreEqual(3, result.data.spawnPoints.Length);
        Assert.AreEqual("player_start", result.data.spawnPoints[0].spawnType);
        Assert.IsTrue(result.summary.Contains("No gameplay failure effect"));
    }

    [Test]
    public void CrowdAgentSampleLoads()
    {
        P9CrowdAgentProfileLoadResult result = P9SpawnPointLoader.LoadSampleCrowdAgentProfiles();

        Assert.IsTrue(result.success, string.Join("\n", result.errors));
        Assert.IsFalse(result.failSafe);
        Assert.AreEqual(3, result.data.agentProfiles.Length);
        Assert.IsTrue(result.data.agentProfiles.Any(profile => profile.agentType == "elderly_proxy"));
        Assert.IsFalse(P9CrowdAgentProfile.ClaimsRealCrowdModel);
    }

    [Test]
    public void EntranceSafeFloorProxySampleLoads()
    {
        P9EntranceSafeFloorProxyLoadResult result = P9SpawnPointLoader.LoadSampleEntranceSafeFloorProxies();

        Assert.IsTrue(result.success, string.Join("\n", result.errors));
        Assert.IsFalse(result.failSafe);
        Assert.AreEqual(3, result.data.proxies.Length);
        Assert.IsTrue(result.data.proxies.Any(proxy => proxy.verticalEvacuationStatus == "available"));
        Assert.IsTrue(result.summary.Contains("No indoor scene gameplay"));
    }

    [Test]
    public void NonOfficialHumanitarianCandidateFlagRequiresWarning()
    {
        P9EntranceSafeFloorProxyLoadResult result = P9SpawnPointLoader.LoadSampleEntranceSafeFloorProxies();

        Assert.IsTrue(result.success, string.Join("\n", result.errors));
        P9EntranceSafeFloorProxyRecord humanitarian = result.data.proxies.Single(proxy => proxy.humanitarianCandidateFlag);

        Assert.IsFalse(humanitarian.isOfficialShelter);
        Assert.IsTrue(humanitarian.nonOfficialWarningRequired);
        Assert.IsTrue(humanitarian.RequiresNonOfficialWarning());
        Assert.IsTrue(humanitarian.HumanitarianCandidateRemainsNonOfficial());
    }

    [Test]
    public void P8HandoffAdapterReturnsSafeDefaultWhenP8DDataMissing()
    {
        P9P8HandoffState state = P9P8HandoffAdapter.Adapt(null);

        Assert.IsTrue(state.success);
        Assert.IsFalse(state.failSafe);
        Assert.IsTrue(state.isMissingOrFallback);
        Assert.AreEqual("unknown_no_effect", state.infrastructureHazardState);
        Assert.AreEqual("unknown_no_effect", state.roadBridgeUndergroundState);
        Assert.AreEqual("unknown_no_effect", state.buildingWarningState);
        Assert.IsFalse(state.entranceBlockedState);
        Assert.IsFalse(state.lowFloorInundationWarning);
        Assert.AreEqual("unknown_no_effect", state.collapseDamageProxyState);
        Assert.AreEqual("unknown", state.depthStatus);
        Assert.AreEqual("unknown", state.intensityStatus);
        Assert.AreEqual("unknown", state.arrivalTimingStatus);
        Assert.IsFalse(P9P8HandoffAdapter.RequiresP8DEFinalHandoff);
        Assert.IsFalse(P9P8HandoffAdapter.AffectsGameplaySuccessFailure);
    }

    [Test]
    public void NoSuccessFailureRuleMutation()
    {
        Assert.IsFalse(P9RuntimePolicy.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P9RuntimePolicy.CanCausePlayerFailureInP9A);
        Assert.IsFalse(P9RuntimePolicy.ImplementsFinalFailureGameplay);
        Assert.IsFalse(P9SpawnPointLoader.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P9CrowdAgentProfile.AffectsPlayerSuccessFailure);
        Assert.IsFalse(P9EntranceSafeFloorProxy.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P9ScenarioDebugSummary.AffectsGameplaySuccessFailure);

        string p9SourceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Scripts", "P9");
        string combinedSource = string.Join(
            "\n",
            Directory.GetFiles(p9SourceDirectory, "*.cs").Select(File.ReadAllText));

        Assert.IsFalse(combinedSource.Contains("EvacuationGameManager"));
        Assert.IsFalse(combinedSource.Contains("ResultPanelController"));
        Assert.IsFalse(combinedSource.Contains("ShelterEntranceTrigger"));
        Assert.IsFalse(combinedSource.Contains("BuildingShelter"));
    }

    [Test]
    public void ScenarioDebugSummaryHandlesMissingP8Handoff()
    {
        P9ScenarioDebugSummaryResult summary = P9ScenarioDebugSummary.Create(
            P9SpawnPointLoader.LoadSampleSpawnPoints().data,
            P9SpawnPointLoader.LoadSampleCrowdAgentProfiles().data,
            P9SpawnPointLoader.LoadSampleEntranceSafeFloorProxies().data,
            P9P8HandoffAdapter.CreateMissingHandoffDefault());

        Assert.AreEqual(3, summary.spawnPointCount);
        Assert.AreEqual(3, summary.crowdAgentProfileCount);
        Assert.AreEqual(3, summary.entranceProxyCount);
        Assert.IsTrue(summary.p8HandoffMissingOrFallback);
        Assert.IsFalse(summary.affectsGameplaySuccessFailure);
        StringAssert.Contains("No final failure gameplay", summary.summary);
    }
}
