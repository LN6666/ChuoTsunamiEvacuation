using System;
using System.Collections.Generic;
using UnityEngine;

public class P9EntranceSafeFloorMarkerRuntime : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool ImplementsFinalFailureGameplay = P9RuntimePolicy.ImplementsFinalFailureGameplay;
    public const bool ImplementsIndoorSceneGameplay = P9RuntimePolicy.ImplementsIndoorSceneGameplay;

    [SerializeField] private string proxyId = string.Empty;
    [SerializeField] private string relatedShelterId = string.Empty;
    [SerializeField] private string relatedCandidateId = string.Empty;
    [SerializeField] private string safeFloorLabel = "unknown";
    [SerializeField] private string verticalEvacuationStatus = "unknown";
    [SerializeField] private string evacuationCompleteProxyState = "not_evaluated_p9b";
    [SerializeField] private bool isOfficialShelter;
    [SerializeField] private bool humanitarianCandidateFlag;
    [SerializeField] private bool nonOfficialWarningRequired;
    [SerializeField] private bool finalOutcomeMutationEnabled;

    public string ProxyId => proxyId;
    public string RelatedShelterId => relatedShelterId;
    public string RelatedCandidateId => relatedCandidateId;
    public string SafeFloorLabel => safeFloorLabel;
    public string VerticalEvacuationStatus => verticalEvacuationStatus;
    public string EvacuationCompleteProxyState => evacuationCompleteProxyState;
    public bool IsOfficialShelter => isOfficialShelter;
    public bool HumanitarianCandidateFlag => humanitarianCandidateFlag;
    public bool NonOfficialWarningRequired => nonOfficialWarningRequired;
    public bool FinalOutcomeMutationEnabled => finalOutcomeMutationEnabled;
    public bool RemainsWarningRequiredWhenHumanitarian => !humanitarianCandidateFlag || (!isOfficialShelter && nonOfficialWarningRequired);

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
        evacuationCompleteProxyState = "not_evaluated_p9b";
        isOfficialShelter = record.isOfficialShelter;
        humanitarianCandidateFlag = record.humanitarianCandidateFlag;
        nonOfficialWarningRequired = record.nonOfficialWarningRequired;
        finalOutcomeMutationEnabled = false;
        transform.position = record.entrancePosition == null ? transform.position : record.entrancePosition.ToVector3();
    }

    public static P9BEntranceMarkerGenerationResult GenerateMarkers(
        P9EntranceSafeFloorProxyCollection collection,
        Transform parent)
    {
        var result = new P9BEntranceMarkerGenerationResult
        {
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure,
            implementsIndoorSceneGameplay = ImplementsIndoorSceneGameplay
        };

        if (collection == null || collection.proxies == null)
        {
            result.summary = "P9-B entrance marker data missing. No entrance/safe-floor proxy markers generated.";
            return result;
        }

        var snapshots = new List<P9BEntranceMarkerSnapshot>();
        for (int i = 0; i < collection.proxies.Length; i++)
        {
            P9EntranceSafeFloorProxyRecord record = collection.proxies[i];
            if (record == null)
            {
                continue;
            }

            var markerObject = new GameObject("P9B_EntranceSafeFloor_" + SafeObjectName(record.proxyId));
            if (parent != null)
            {
                markerObject.transform.SetParent(parent, false);
            }

            var existingProxy = markerObject.AddComponent<P9EntranceSafeFloorProxy>();
            existingProxy.ApplyRecord(record);

            var runtime = markerObject.AddComponent<P9EntranceSafeFloorMarkerRuntime>();
            runtime.ApplyRecord(record);

            var marker = markerObject.AddComponent<P9RuntimeMarker>();
            marker.Configure(
                record.proxyId,
                "entrance_safe_floor_proxy",
                string.IsNullOrWhiteSpace(record.relatedCandidateId) ? record.relatedShelterId : record.relatedCandidateId,
                record.safeFloorLabel,
                record.isOfficialShelter,
                record.humanitarianCandidateFlag,
                record.nonOfficialWarningRequired,
                record.RequiresNonOfficialWarning() ? P9HumanitarianCandidateMarkerRuntime.RequiredWarningLabel : string.Empty,
                record.verticalEvacuationStatus,
                "P9-B entrance/safe-floor marker only. No final outcome mutation.");

            snapshots.Add(runtime.CreateSnapshot());
        }

        result.success = true;
        result.generatedMarkerCount = snapshots.Count;
        result.snapshots = snapshots.ToArray();
        result.summary = "P9-B generated " + snapshots.Count + " entrance/safe-floor proxy markers.";
        return result;
    }

    public P9BEntranceMarkerSnapshot CreateSnapshot()
    {
        return new P9BEntranceMarkerSnapshot
        {
            proxyId = proxyId ?? string.Empty,
            relatedShelterId = relatedShelterId ?? string.Empty,
            relatedCandidateId = relatedCandidateId ?? string.Empty,
            position = transform.position,
            safeFloorLabel = safeFloorLabel ?? string.Empty,
            verticalEvacuationStatus = verticalEvacuationStatus ?? string.Empty,
            evacuationCompleteProxyState = evacuationCompleteProxyState ?? string.Empty,
            isOfficialShelter = isOfficialShelter,
            humanitarianCandidateFlag = humanitarianCandidateFlag,
            nonOfficialWarningRequired = nonOfficialWarningRequired,
            finalOutcomeMutationEnabled = finalOutcomeMutationEnabled
        };
    }

    private static string SafeObjectName(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Replace("/", "_").Replace("\\", "_");
    }
}

[Serializable]
public class P9BEntranceMarkerGenerationResult
{
    public bool success;
    public int generatedMarkerCount;
    public bool affectsGameplaySuccessFailure;
    public bool implementsIndoorSceneGameplay;
    public P9BEntranceMarkerSnapshot[] snapshots = Array.Empty<P9BEntranceMarkerSnapshot>();
    public string summary = string.Empty;
}

[Serializable]
public class P9BEntranceMarkerSnapshot
{
    public string proxyId = string.Empty;
    public string relatedShelterId = string.Empty;
    public string relatedCandidateId = string.Empty;
    public Vector3 position;
    public string safeFloorLabel = string.Empty;
    public string verticalEvacuationStatus = string.Empty;
    public string evacuationCompleteProxyState = string.Empty;
    public bool isOfficialShelter;
    public bool humanitarianCandidateFlag;
    public bool nonOfficialWarningRequired;
    public bool finalOutcomeMutationEnabled;
}
