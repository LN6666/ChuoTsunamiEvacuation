using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class P8InfrastructureDamageEvaluatorTests
{
    [Test]
    public void DamageConfigLoadsFromP8Data()
    {
        string path = Path.Combine(Application.dataPath, "Data", "P8", "infrastructure_damage_proxy_config.json");
        Assert.IsTrue(File.Exists(path), path);

        P8InfrastructureDamageConfig config = P8InfrastructureDamageConfig.LoadFromFile(path);

        Assert.AreEqual("p8_infrastructure_damage_proxy_config_v1", config.configId);
        Assert.Greater(config.lowFloorInundationDepthMeters, 0f);
        Assert.Greater(config.buildingDamageDepthMeters, config.lowFloorInundationDepthMeters);
        Assert.LessOrEqual(config.collapseProxyProbability, 0.1f);
        Assert.IsTrue(config.enableCollapseProxyVisual);
        Assert.IsTrue(config.noPhysicsCollapse);
        Assert.IsTrue(config.noGameplaySuccessFailureChange);
    }

    [Test]
    public void DamageEvaluatorUsesDepthAndIntensityForProxyStates()
    {
        P8InfrastructureDamageConfig config = P8InfrastructureDamageConfig.Default();

        P8InfrastructureDamageEvaluation lowFloor = P8InfrastructureDamageEvaluator.Evaluate(
            CreateHazard(P8InfrastructureCategory.Building, 0.4f, 0.2f, P8InfrastructureHazardState.Warning),
            config,
            CreateInput("building_low_floor", P8InfrastructureCategory.Building, false, false, false, false, 0));
        P8InfrastructureDamageEvaluation damaged = P8InfrastructureDamageEvaluator.Evaluate(
            CreateHazard(P8InfrastructureCategory.Building, 1.2f, 0.7f, P8InfrastructureHazardState.InundatedProxy),
            config,
            CreateInput("building_damaged", P8InfrastructureCategory.Building, false, false, false, false, 99));

        Assert.AreEqual(P8InfrastructureDamageState.LowFloorInundationWarning, lowFloor.state, lowFloor.summary);
        Assert.AreEqual(P8InfrastructureDamageState.BuildingDamagedProxy, damaged.state, damaged.summary);
        StringAssert.Contains("inundationDepthMeters", damaged.summary);
        StringAssert.Contains("hazardIntensity", damaged.summary);
    }

    [Test]
    public void MaxTsunamiHeightAndVisualHeightAreNotPhysicalDamageDepth()
    {
        P8InfrastructureHazardEvaluation hazard = CreateHazard(
            P8InfrastructureCategory.Building,
            0f,
            0f,
            P8InfrastructureHazardState.Safe);
        hazard.maxTsunamiHeightMeters = 99f;
        hazard.visualHeightMeters = 999f;

        P8InfrastructureDamageEvaluation result = P8InfrastructureDamageEvaluator.Evaluate(
            hazard,
            P8InfrastructureDamageConfig.Default(),
            CreateInput("metadata_only_height", P8InfrastructureCategory.Building, false, false, false, false, 0));

        Assert.AreEqual(P8InfrastructureDamageState.NoDamage, result.state, result.summary);
        Assert.IsFalse(result.usesMaxTsunamiHeightAsDepth);
        Assert.IsFalse(result.usesVisualHeightAsPhysicalDepth);
        StringAssert.Contains("maxTsunamiHeightMeters is metadata only", result.summary);
        StringAssert.Contains("visualHeightMeters is cinematic only", result.summary);
    }

    [Test]
    public void DeterministicCollapseProxyIsStableAndSampleLimited()
    {
        P8InfrastructureDamageConfig config = P8InfrastructureDamageConfig.Default();
        config.collapseProxyProbability = 1f;
        config.maxCollapseProxySampleCount = 1;
        config.collapseProxyDepthMeters = 0.1f;
        config.collapseProxyIntensity = 0.1f;
        config.collapseProxyRandomSeed = 1234;
        P8InfrastructureHazardEvaluation hazard = CreateHazard(
            P8InfrastructureCategory.Building,
            2f,
            0.9f,
            P8InfrastructureHazardState.InundatedProxy);

        P8InfrastructureDamageEvaluation first = P8InfrastructureDamageEvaluator.Evaluate(
            hazard,
            config,
            CreateInput("stable_collapse_id", P8InfrastructureCategory.Building, false, false, false, false, 0));
        P8InfrastructureDamageEvaluation second = P8InfrastructureDamageEvaluator.Evaluate(
            hazard,
            config,
            CreateInput("stable_collapse_id", P8InfrastructureCategory.Building, false, false, false, false, 0));
        P8InfrastructureDamageEvaluation overLimit = P8InfrastructureDamageEvaluator.Evaluate(
            hazard,
            config,
            CreateInput("stable_collapse_id", P8InfrastructureCategory.Building, false, false, false, false, 1));

        Assert.AreEqual(P8InfrastructureDamageState.CollapsedProxyVisual, first.state, first.summary);
        Assert.AreEqual(first.collapseProxyState, second.collapseProxyState);
        Assert.AreEqual(P8CollapseProxyState.SuppressedBySampleLimit, overLimit.collapseProxyState, overLimit.summary);
        Assert.AreNotEqual(P8InfrastructureDamageState.CollapsedProxyVisual, overLimit.state);
    }

    [Test]
    public void CollapseProxyProbabilityCanSuppressVisualCollapse()
    {
        P8InfrastructureDamageConfig config = P8InfrastructureDamageConfig.Default();
        config.collapseProxyProbability = 0f;
        config.maxCollapseProxySampleCount = 10;
        config.collapseProxyDepthMeters = 0.1f;
        config.collapseProxyIntensity = 0.1f;

        P8InfrastructureDamageEvaluation result = P8InfrastructureDamageEvaluator.Evaluate(
            CreateHazard(P8InfrastructureCategory.Building, 2f, 0.9f, P8InfrastructureHazardState.InundatedProxy),
            config,
            CreateInput("probability_suppressed", P8InfrastructureCategory.Building, false, false, false, false, 0));

        Assert.AreNotEqual(P8InfrastructureDamageState.CollapsedProxyVisual, result.state);
        Assert.AreEqual(P8CollapseProxyState.SuppressedByProbability, result.collapseProxyState, result.summary);
    }

    [Test]
    public void EntranceBlockedAndUndergroundAvoidRulesWork()
    {
        P8InfrastructureDamageConfig config = P8InfrastructureDamageConfig.Default();
        P8InfrastructureDamageEvaluation entrance = P8InfrastructureDamageEvaluator.Evaluate(
            CreateHazard(P8InfrastructureCategory.Entrance, 0.35f, 0.5f, P8InfrastructureHazardState.RestrictedProxy),
            config,
            CreateInput("entrance", P8InfrastructureCategory.Entrance, false, false, false, false, 0));
        P8InfrastructureDamageEvaluation underground = P8InfrastructureDamageEvaluator.Evaluate(
            CreateHazard(P8InfrastructureCategory.Underground, 0.1f, 0.2f, P8InfrastructureHazardState.AvoidProxy),
            config,
            CreateInput("underground", P8InfrastructureCategory.Underground, false, false, false, false, 0));

        Assert.AreEqual(P8InfrastructureDamageState.EntranceBlockedProxy, entrance.state, entrance.summary);
        Assert.AreEqual(P8InfrastructureBlockageState.EntranceBlockedProxy, entrance.blockageState);
        Assert.AreEqual(P8InfrastructureDamageState.UndergroundAvoidProxy, underground.state, underground.summary);
        Assert.AreEqual(P8InfrastructureBlockageState.UndergroundAvoidProxy, underground.blockageState);
    }

    [Test]
    public void HumanitarianCandidatesRemainNonOfficialAndWarningRequired()
    {
        P8HumanitarianCandidateAuditDataset audit = LoadCandidateAudit();
        P8HumanitarianCandidateAuditRecord named = audit.records.First(record => !string.IsNullOrWhiteSpace(record.buildingName));
        P8HumanitarianCandidateAuditRecord unknown = audit.records.First(record => string.IsNullOrWhiteSpace(record.buildingName));
        P8InfrastructureHazardEvaluation hazard = CreateHazard(
            P8InfrastructureCategory.HumanitarianCandidateProxy,
            0.4f,
            0.5f,
            P8InfrastructureHazardState.RestrictedProxy);

        P8HumanitarianCandidateHazardStatus namedStatus =
            P8HumanitarianCandidateHazardStatus.Evaluate(named, hazard, P8InfrastructureDamageConfig.Default(), 99);
        P8HumanitarianCandidateHazardStatus unknownStatus =
            P8HumanitarianCandidateHazardStatus.Evaluate(unknown, hazard, P8InfrastructureDamageConfig.Default(), 99);

        Assert.IsFalse(namedStatus.isOfficialShelter);
        Assert.IsFalse(unknownStatus.isOfficialShelter);
        Assert.IsTrue(namedStatus.nonOfficialWarningRequired);
        Assert.IsTrue(unknownStatus.nonOfficialWarningRequired);
        Assert.IsTrue(unknownStatus.manualReviewNeeded);
        Assert.IsFalse(namedStatus.selectableGameplayEnabled);
        Assert.AreEqual("low_floor_inundation_warning", namedStatus.damageStatusToken);
    }

    [Test]
    public void DamageProxyHasNoGameplaySuccessFailureCoupling()
    {
        Assert.IsTrue(P8InfrastructureDamageEvaluator.IsGameplayNeutral());
        Assert.IsFalse(P8InfrastructureDamageEvaluator.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P8InfrastructureDamageEvaluator.RequiresP9Systems);
        Assert.IsFalse(P8InfrastructureDamageEvaluator.ImplementsTrueStructuralCollapse);
        Assert.IsFalse(P8InfrastructureDamageEvaluator.ImplementsPhysicsCollapse);
        Assert.IsFalse(P8InfrastructureDamageEvaluator.UsesMaxTsunamiHeightAsDepth);
        Assert.IsFalse(P8InfrastructureDamageEvaluator.UsesVisualHeightAsPhysicalDepth);
    }

    private static P8HumanitarianCandidateAuditDataset LoadCandidateAudit()
    {
        string path = Path.Combine(Application.dataPath, "Data", "P8", "humanitarian_highrise_candidate_audit_v1.json");
        Assert.IsTrue(File.Exists(path), path);
        return JsonUtility.FromJson<P8HumanitarianCandidateAuditDataset>(File.ReadAllText(path));
    }

    private static P8InfrastructureDamageEvaluationInput CreateInput(
        string targetId,
        P8InfrastructureCategory category,
        bool isHumanitarianCandidate,
        bool isOfficialShelter,
        bool nonOfficialWarningRequired,
        bool manualReviewNeeded,
        int collapseSelectionRank)
    {
        return new P8InfrastructureDamageEvaluationInput
        {
            targetId = targetId,
            category = category,
            isHumanitarianCandidate = isHumanitarianCandidate,
            isOfficialShelter = isOfficialShelter,
            nonOfficialWarningRequired = nonOfficialWarningRequired,
            manualReviewNeeded = manualReviewNeeded,
            collapseSelectionRank = collapseSelectionRank
        };
    }

    private static P8InfrastructureHazardEvaluation CreateHazard(
        P8InfrastructureCategory category,
        float depthMeters,
        float intensity,
        P8InfrastructureHazardState state)
    {
        return new P8InfrastructureHazardEvaluation
        {
            success = true,
            failSafe = false,
            targetId = "hazard_" + P8InfrastructureCategoryUtility.ToToken(category),
            category = category,
            state = state,
            arrivalTimeSeconds = 100f,
            inundationDepthMeters = depthMeters,
            maxTsunamiHeightMeters = 3f,
            visualHeightMeters = 1000f,
            hazardIntensity = intensity,
            confidence = 0.9f,
            sourceMode = "official_tsunami_metropolitan",
            evidenceSourceId = "tokyo_damage_estimation_map_tsunami",
            sourceBasis = P8InfrastructureHazardEvaluator.DriverSource
        };
    }
}
