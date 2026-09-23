using System;
using UnityEngine;

public class P9RuntimeMarker : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool ImplementsIndoorSceneGameplay = P9RuntimePolicy.ImplementsIndoorSceneGameplay;
    public const bool ClaimsOfficialRoutes = P9RuntimePolicy.ClaimsOfficialRoutes;

    [SerializeField] private string markerId = "p9b_runtime_marker";
    [SerializeField] private string markerType = "runtime_proxy";
    [SerializeField] private string sourceDataId = string.Empty;
    [SerializeField] private string displayLabel = "P9-B runtime proxy";
    [SerializeField] private bool isOfficialShelter;
    [SerializeField] private bool humanitarianCandidateFlag;
    [SerializeField] private bool nonOfficialWarningRequired;
    [SerializeField] private bool selectableAsFinalEvacuationTarget;
    [SerializeField] private bool routesAreOfficial;
    [SerializeField] private bool routesAreEstimatedPrototypeGuidance = true;
    [SerializeField] private string warningLabel = string.Empty;
    [SerializeField] private string statusToken = "prototype_marker_only";
    [SerializeField] private string notes = string.Empty;

    public string MarkerId => markerId;
    public string MarkerType => markerType;
    public string SourceDataId => sourceDataId;
    public string DisplayLabel => displayLabel;
    public bool IsOfficialShelter => isOfficialShelter;
    public bool HumanitarianCandidateFlag => humanitarianCandidateFlag;
    public bool NonOfficialWarningRequired => nonOfficialWarningRequired;
    public bool SelectableAsFinalEvacuationTarget => selectableAsFinalEvacuationTarget;
    public bool RoutesAreOfficial => routesAreOfficial;
    public bool RoutesAreEstimatedPrototypeGuidance => routesAreEstimatedPrototypeGuidance;
    public string WarningLabel => warningLabel;
    public string StatusToken => statusToken;
    public string Notes => notes;
    public bool IsNonOfficialHumanitarianWarningValid => !humanitarianCandidateFlag || (!isOfficialShelter && nonOfficialWarningRequired);
    public bool HasNoGameplayOutcomeEffect => !AffectsGameplaySuccessFailure && !selectableAsFinalEvacuationTarget;

    public void Configure(
        string id,
        string type,
        string sourceId,
        string label,
        bool officialShelter,
        bool humanitarianCandidate,
        bool warningRequired,
        string warning,
        string status,
        string markerNotes)
    {
        markerId = id ?? string.Empty;
        markerType = type ?? string.Empty;
        sourceDataId = sourceId ?? string.Empty;
        displayLabel = label ?? string.Empty;
        isOfficialShelter = officialShelter;
        humanitarianCandidateFlag = humanitarianCandidate;
        nonOfficialWarningRequired = warningRequired;
        selectableAsFinalEvacuationTarget = false;
        routesAreOfficial = false;
        routesAreEstimatedPrototypeGuidance = true;
        warningLabel = warning ?? string.Empty;
        statusToken = status ?? string.Empty;
        notes = markerNotes ?? string.Empty;
    }

    public P9RuntimeMarkerSnapshot CreateSnapshot()
    {
        return new P9RuntimeMarkerSnapshot
        {
            markerId = markerId ?? string.Empty,
            markerType = markerType ?? string.Empty,
            sourceDataId = sourceDataId ?? string.Empty,
            position = transform.position,
            isOfficialShelter = isOfficialShelter,
            humanitarianCandidateFlag = humanitarianCandidateFlag,
            nonOfficialWarningRequired = nonOfficialWarningRequired,
            selectableAsFinalEvacuationTarget = selectableAsFinalEvacuationTarget,
            routesAreOfficial = routesAreOfficial,
            routesAreEstimatedPrototypeGuidance = routesAreEstimatedPrototypeGuidance,
            statusToken = statusToken ?? string.Empty,
            warningLabel = warningLabel ?? string.Empty
        };
    }
}

[Serializable]
public class P9RuntimeMarkerSnapshot
{
    public string markerId = string.Empty;
    public string markerType = string.Empty;
    public string sourceDataId = string.Empty;
    public Vector3 position;
    public bool isOfficialShelter;
    public bool humanitarianCandidateFlag;
    public bool nonOfficialWarningRequired;
    public bool selectableAsFinalEvacuationTarget;
    public bool routesAreOfficial;
    public bool routesAreEstimatedPrototypeGuidance;
    public string statusToken = string.Empty;
    public string warningLabel = string.Empty;
}
