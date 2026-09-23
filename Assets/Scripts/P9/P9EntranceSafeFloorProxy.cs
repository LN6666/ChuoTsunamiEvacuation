using System;
using UnityEngine;

public class P9EntranceSafeFloorProxy : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool ImplementsFinalFailureGameplay = P9RuntimePolicy.ImplementsFinalFailureGameplay;
    public const bool ImplementsIndoorSceneGameplay = P9RuntimePolicy.ImplementsIndoorSceneGameplay;
    public const bool RequiresChuoBaseMap = P9RuntimePolicy.RequiresChuoBaseMap;

    [SerializeField] private string proxyId = "p9_entrance_proxy";
    [SerializeField] private string relatedShelterId = string.Empty;
    [SerializeField] private string relatedCandidateId = string.Empty;
    [SerializeField] private string safeFloorLabel = "unknown";
    [SerializeField] private string verticalEvacuationStatus = "unknown";
    [SerializeField] private bool isOfficialShelter;
    [SerializeField] private bool humanitarianCandidateFlag;
    [SerializeField] private bool nonOfficialWarningRequired = true;
    [SerializeField] private string interactionMode = "external_proxy";
    [SerializeField] private string notes = string.Empty;

    public string ProxyId => proxyId;
    public string RelatedShelterId => relatedShelterId;
    public string RelatedCandidateId => relatedCandidateId;
    public string SafeFloorLabel => safeFloorLabel;
    public string VerticalEvacuationStatusToken => verticalEvacuationStatus;
    public P9VerticalEvacuationStatus VerticalEvacuationStatus => P9EvacuationProxyState.ParseVerticalStatus(verticalEvacuationStatus);
    public bool IsOfficialShelter => isOfficialShelter;
    public bool HumanitarianCandidateFlag => humanitarianCandidateFlag;
    public bool NonOfficialWarningRequired => nonOfficialWarningRequired;
    public string InteractionMode => interactionMode;
    public string Notes => notes;
    public bool RequiresNonOfficialWarning => !isOfficialShelter && (humanitarianCandidateFlag || nonOfficialWarningRequired);
    public bool HumanitarianCandidateRemainsNonOfficial => !humanitarianCandidateFlag || !isOfficialShelter;

    public void ConfigureForTests(
        string id,
        bool officialShelter,
        bool humanitarianCandidate,
        bool warningRequired,
        string statusToken)
    {
        proxyId = id ?? string.Empty;
        isOfficialShelter = officialShelter;
        humanitarianCandidateFlag = humanitarianCandidate;
        nonOfficialWarningRequired = warningRequired;
        verticalEvacuationStatus = string.IsNullOrWhiteSpace(statusToken) ? "unknown" : statusToken;
    }

    public void ApplyRecord(P9EntranceSafeFloorProxyRecord record)
    {
        if (record == null)
        {
            return;
        }

        proxyId = record.proxyId ?? string.Empty;
        relatedShelterId = record.relatedShelterId ?? string.Empty;
        relatedCandidateId = record.relatedCandidateId ?? string.Empty;
        safeFloorLabel = record.safeFloorLabel ?? string.Empty;
        verticalEvacuationStatus = record.verticalEvacuationStatus ?? "unknown";
        isOfficialShelter = record.isOfficialShelter;
        humanitarianCandidateFlag = record.humanitarianCandidateFlag;
        nonOfficialWarningRequired = record.nonOfficialWarningRequired;
        interactionMode = record.interactionMode ?? string.Empty;
        notes = record.notes ?? string.Empty;
        transform.position = record.entrancePosition == null ? transform.position : record.entrancePosition.ToVector3();
    }

    public P9EvacuationProxyState CreateNoEffectState(int queueLength, int activeCrowdCount)
    {
        return P9EvacuationProxyState.FromQueueSnapshot(proxyId, VerticalEvacuationStatus, queueLength, activeCrowdCount);
    }
}

[Serializable]
public class P9EntranceSafeFloorProxyCollection
{
    public string schemaVersion = string.Empty;
    public string sourceMode = string.Empty;
    public string notes = string.Empty;
    public P9EntranceSafeFloorProxyRecord[] proxies = Array.Empty<P9EntranceSafeFloorProxyRecord>();
}

[Serializable]
public class P9EntranceSafeFloorProxyRecord
{
    public string proxyId = string.Empty;
    public string relatedShelterId = string.Empty;
    public string relatedCandidateId = string.Empty;
    public P9Vector3Data entrancePosition = new P9Vector3Data();
    public string safeFloorLabel = string.Empty;
    public string verticalEvacuationStatus = "unknown";
    public bool isOfficialShelter;
    public bool humanitarianCandidateFlag;
    public bool nonOfficialWarningRequired;
    public string interactionMode = string.Empty;
    public string notes = string.Empty;

    public bool RequiresNonOfficialWarning()
    {
        return !isOfficialShelter && (humanitarianCandidateFlag || nonOfficialWarningRequired);
    }

    public bool HumanitarianCandidateRemainsNonOfficial()
    {
        return !humanitarianCandidateFlag || !isOfficialShelter;
    }
}
