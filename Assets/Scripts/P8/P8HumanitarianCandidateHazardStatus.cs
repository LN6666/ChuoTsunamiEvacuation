using System;

[Serializable]
public class P8HumanitarianCandidateAuditDataset
{
    public string datasetId = string.Empty;
    public bool isOfficialShelterDataset;
    public bool nonOfficialWarningRequired;
    public bool dataOnly;
    public bool affectsGameplaySuccessFailure;
    public bool implementsP8D;
    public bool implementsP9SelectableGameplay;
    public P8HumanitarianCandidateAuditRecord[] records = new P8HumanitarianCandidateAuditRecord[0];
}

[Serializable]
public class P8HumanitarianCandidateAuditRecord
{
    public string candidateId = string.Empty;
    public string buildingName = string.Empty;
    public string fallbackBuildingId = string.Empty;
    public string candidateLayer = string.Empty;
    public string publicAccessStatus = string.Empty;
    public string evidenceStatus = string.Empty;
    public bool manualReviewNeeded;
    public bool isOfficialShelter;
    public bool nonOfficialWarningRequired;
    public bool hazardStatusEligible;
    public bool p8cProxyEligible;
    public bool p8dDamageStatusEligible;
    public bool p8ePersistentVisibilityReviewNeeded;
    public bool p9LifeFirstSelectableReviewNeeded;
    public bool affectsGameplaySuccessFailure;
    public bool implementsP8D;
    public bool implementsP9SelectableGameplay;
}

[Serializable]
public class P8HumanitarianCandidateHazardStatus
{
    public string candidateId = string.Empty;
    public string displayName = string.Empty;
    public string hazardStatusToken = "safe";
    public string damageStatusToken = "no_damage";
    public string publicAccessStatus = string.Empty;
    public bool manualReviewNeeded;
    public bool isOfficialShelter;
    public bool nonOfficialWarningRequired;
    public bool selectableGameplayEnabled;
    public bool affectsGameplaySuccessFailure;
    public bool p8dDamageStatusEligible;
    public bool p9LifeFirstSelectableReviewNeeded;
    public P8InfrastructureDamageEvaluation damageEvaluation;
    public string warning = "Non-official humanitarian high-rise candidate. Not an official evacuation shelter.";

    public static P8HumanitarianCandidateHazardStatus Evaluate(
        P8HumanitarianCandidateAuditRecord candidate,
        P8InfrastructureHazardEvaluation hazard,
        P8InfrastructureDamageConfig config,
        int collapseSelectionRank)
    {
        candidate = candidate ?? new P8HumanitarianCandidateAuditRecord();
        var damageInput = new P8InfrastructureDamageEvaluationInput
        {
            targetId = candidate.candidateId,
            category = P8InfrastructureCategory.HumanitarianCandidateProxy,
            isHumanitarianCandidate = true,
            isOfficialShelter = false,
            nonOfficialWarningRequired = true,
            manualReviewNeeded = candidate.manualReviewNeeded,
            collapseSelectionRank = collapseSelectionRank
        };

        P8InfrastructureDamageEvaluation damage =
            P8InfrastructureDamageEvaluator.Evaluate(hazard, config, damageInput);

        return new P8HumanitarianCandidateHazardStatus
        {
            candidateId = candidate.candidateId ?? string.Empty,
            displayName = SelectDisplayName(candidate),
            hazardStatusToken = HazardStateToToken(hazard != null ? hazard.state : P8InfrastructureHazardState.Safe),
            damageStatusToken = damage.damageStateToken,
            publicAccessStatus = candidate.publicAccessStatus ?? string.Empty,
            manualReviewNeeded = candidate.manualReviewNeeded || string.IsNullOrWhiteSpace(candidate.buildingName),
            isOfficialShelter = false,
            nonOfficialWarningRequired = true,
            selectableGameplayEnabled = false,
            affectsGameplaySuccessFailure = false,
            p8dDamageStatusEligible = candidate.p8dDamageStatusEligible,
            p9LifeFirstSelectableReviewNeeded = candidate.p9LifeFirstSelectableReviewNeeded,
            damageEvaluation = damage
        };
    }

    private static string SelectDisplayName(P8HumanitarianCandidateAuditRecord candidate)
    {
        if (!string.IsNullOrWhiteSpace(candidate.buildingName))
        {
            return candidate.buildingName;
        }

        if (!string.IsNullOrWhiteSpace(candidate.fallbackBuildingId))
        {
            return "unknown (" + candidate.fallbackBuildingId + ")";
        }

        return "unknown";
    }

    private static string HazardStateToToken(P8InfrastructureHazardState state)
    {
        switch (state)
        {
            case P8InfrastructureHazardState.Watch:
                return "watch";
            case P8InfrastructureHazardState.Warning:
                return "warning";
            case P8InfrastructureHazardState.InundatedProxy:
                return "inundated_proxy";
            case P8InfrastructureHazardState.RestrictedProxy:
                return "restricted_proxy";
            case P8InfrastructureHazardState.AvoidProxy:
                return "avoid_proxy";
            default:
                return "safe";
        }
    }
}
