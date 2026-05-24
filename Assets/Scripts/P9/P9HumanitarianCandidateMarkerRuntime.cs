using System;
using System.Collections.Generic;
using UnityEngine;

public class P9HumanitarianCandidateMarkerRuntime : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool ClaimsOfficialHumanitarianCandidateShelters = P9RuntimePolicy.ClaimsOfficialHumanitarianCandidateShelters;

    public const string RequiredWarningLabel = "Non-official humanitarian high-rise candidate. Not an official evacuation shelter.";

    [SerializeField] private string candidateId = string.Empty;
    [SerializeField] private string buildingName = string.Empty;
    [SerializeField] private string fallbackBuildingId = string.Empty;
    [SerializeField] private string markerMode = string.Empty;
    [SerializeField] private bool hasCoordinates;
    [SerializeField] private float latitude;
    [SerializeField] private float longitude;
    [SerializeField] private bool isOfficialShelter;
    [SerializeField] private bool nonOfficialWarningRequired = true;
    [SerializeField] private bool manualReviewNeeded = true;
    [SerializeField] private bool selectableGameplayEnabled;
    [SerializeField] private bool hazardStatusEligible;
    [SerializeField] private string warningLabel = RequiredWarningLabel;

    public string CandidateId => candidateId;
    public string BuildingName => buildingName;
    public string FallbackBuildingId => fallbackBuildingId;
    public string MarkerMode => markerMode;
    public bool HasCoordinates => hasCoordinates;
    public float Latitude => latitude;
    public float Longitude => longitude;
    public bool IsOfficialShelter => isOfficialShelter;
    public bool NonOfficialWarningRequired => nonOfficialWarningRequired;
    public bool ManualReviewNeeded => manualReviewNeeded;
    public bool SelectableGameplayEnabled => selectableGameplayEnabled;
    public bool HazardStatusEligible => hazardStatusEligible;
    public string WarningLabel => warningLabel;
    public bool RemainsNonOfficialWarningRequired => !isOfficialShelter && nonOfficialWarningRequired;
    public bool IsSelectableFinalTargetInP9B => false;

    public void ApplyRecord(P9BHumanitarianCandidateMarkerRecord record)
    {
        if (record == null)
        {
            return;
        }

        candidateId = record.candidateId ?? string.Empty;
        buildingName = record.buildingName ?? string.Empty;
        fallbackBuildingId = record.fallbackBuildingId ?? string.Empty;
        markerMode = record.markerMode ?? string.Empty;
        hasCoordinates = record.hasCoordinates;
        latitude = record.latitude;
        longitude = record.longitude;
        isOfficialShelter = false;
        nonOfficialWarningRequired = true;
        manualReviewNeeded = record.manualReviewNeeded;
        selectableGameplayEnabled = false;
        hazardStatusEligible = record.hazardStatusEligible;
        warningLabel = RequiredWarningLabel;
        transform.position = ProjectToProxyPosition(record, transform.GetSiblingIndex());
    }

    public static P9BHumanitarianMarkerGenerationResult GenerateMarkers(
        P9BHumanitarianCandidateMarkerCollection collection,
        Transform parent,
        int maxMarkers)
    {
        var result = new P9BHumanitarianMarkerGenerationResult
        {
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure
        };

        if (collection == null || collection.records == null)
        {
            result.summary = "P8-E humanitarian marker handoff missing. P9-B generated no candidate markers.";
            return result;
        }

        int cap = maxMarkers <= 0 ? collection.records.Length : Mathf.Min(maxMarkers, collection.records.Length);
        var snapshots = new List<P9BHumanitarianMarkerSnapshot>();
        for (int i = 0; i < cap; i++)
        {
            P9BHumanitarianCandidateMarkerRecord record = collection.records[i];
            if (record == null)
            {
                continue;
            }

            var markerObject = new GameObject("P9B_HumanitarianCandidate_" + SafeObjectName(record.candidateId));
            if (parent != null)
            {
                markerObject.transform.SetParent(parent, false);
            }

            var runtime = markerObject.AddComponent<P9HumanitarianCandidateMarkerRuntime>();
            runtime.ApplyRecord(record);

            var marker = markerObject.AddComponent<P9RuntimeMarker>();
            marker.Configure(
                record.candidateId,
                "humanitarian_highrise_candidate",
                record.fallbackBuildingId,
                string.IsNullOrWhiteSpace(record.buildingName) ? record.candidateId : record.buildingName,
                false,
                true,
                true,
                RequiredWarningLabel,
                "non_official_warning_required",
                "P9-B persistent runtime marker. Not official and not selectable as a final target.");

            snapshots.Add(runtime.CreateSnapshot());
        }

        result.success = true;
        result.generatedMarkerCount = snapshots.Count;
        result.expectedTotalCandidates = collection.totalCandidates;
        result.namedMarkerCount = collection.namedMarkerCount;
        result.idOnlyMarkerCount = collection.idOnlyMarkerCount;
        result.allCandidatesNonOfficial = collection.allCandidatesNonOfficial && AllSnapshotsRemainNonOfficial(snapshots);
        result.allCandidatesRequireNonOfficialWarning = collection.allCandidatesRequireNonOfficialWarning && AllSnapshotsRequireWarning(snapshots);
        result.selectableGameplayEnabled = false;
        result.snapshots = snapshots.ToArray();
        result.summary = "P9-B generated " + result.generatedMarkerCount +
                         " non-official humanitarian candidate markers from P8-E handoff.";
        return result;
    }

    public P9BHumanitarianMarkerSnapshot CreateSnapshot()
    {
        return new P9BHumanitarianMarkerSnapshot
        {
            candidateId = candidateId ?? string.Empty,
            buildingName = buildingName ?? string.Empty,
            fallbackBuildingId = fallbackBuildingId ?? string.Empty,
            position = transform.position,
            isOfficialShelter = isOfficialShelter,
            nonOfficialWarningRequired = nonOfficialWarningRequired,
            selectableGameplayEnabled = false,
            manualReviewNeeded = manualReviewNeeded,
            warningLabel = warningLabel ?? string.Empty
        };
    }

    private static Vector3 ProjectToProxyPosition(P9BHumanitarianCandidateMarkerRecord record, int fallbackIndex)
    {
        if (record != null && record.hasCoordinates)
        {
            const float originLatitude = 35.67f;
            const float originLongitude = 139.77f;
            const float scale = 10000f;
            return new Vector3(
                (record.longitude - originLongitude) * scale,
                0f,
                (record.latitude - originLatitude) * scale);
        }

        int row = fallbackIndex / 12;
        int column = fallbackIndex % 12;
        return new Vector3(column * 3f, 0f, -row * 3f);
    }

    private static bool AllSnapshotsRemainNonOfficial(List<P9BHumanitarianMarkerSnapshot> snapshots)
    {
        for (int i = 0; i < snapshots.Count; i++)
        {
            if (snapshots[i].isOfficialShelter)
            {
                return false;
            }
        }

        return true;
    }

    private static bool AllSnapshotsRequireWarning(List<P9BHumanitarianMarkerSnapshot> snapshots)
    {
        for (int i = 0; i < snapshots.Count; i++)
        {
            if (!snapshots[i].nonOfficialWarningRequired)
            {
                return false;
            }
        }

        return true;
    }

    private static string SafeObjectName(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Replace("/", "_").Replace("\\", "_");
    }
}

[Serializable]
public class P9BHumanitarianMarkerRuntimeConfig
{
    public string schemaVersion = string.Empty;
    public string p8PersistentMarkerPath = string.Empty;
    public string p8VisibilityHandoffPath = string.Empty;
    public int expectedTotalCandidates;
    public int expectedNamedCandidateCount;
    public int expectedIdOnlyCandidateCount;
    public bool generateMarkersForAllAvailableCandidates;
    public string markerCoordinateMode = string.Empty;
    public bool isOfficialShelter;
    public bool nonOfficialWarningRequired;
    public bool selectableGameplayEnabled;
    public bool affectsGameplaySuccessFailure;
    public string requiredWarningLabel = string.Empty;
    public string notes = string.Empty;
}

[Serializable]
public class P9BHumanitarianCandidateMarkerCollection
{
    public string datasetId = string.Empty;
    public int totalCandidates;
    public int namedMarkerCount;
    public int idOnlyMarkerCount;
    public bool persistentSceneObjectsCreated;
    public bool selectableGameplayEnabled;
    public bool affectsGameplaySuccessFailure;
    public bool allCandidatesNonOfficial;
    public bool allCandidatesRequireNonOfficialWarning;
    public string notes = string.Empty;
    public P9BHumanitarianCandidateMarkerRecord[] records = Array.Empty<P9BHumanitarianCandidateMarkerRecord>();
}

[Serializable]
public class P9BHumanitarianCandidateMarkerRecord
{
    public string candidateId = string.Empty;
    public string buildingName = string.Empty;
    public string fallbackBuildingId = string.Empty;
    public string locationText = string.Empty;
    public float latitude;
    public float longitude;
    public bool hasCoordinates;
    public string sourceEvidence = string.Empty;
    public string markerMode = string.Empty;
    public bool hazardStatusEligible;
    public bool nonOfficialWarningRequired;
    public bool isOfficialShelter;
    public bool manualReviewNeeded;
    public bool p8dDamageStatusEligible;
    public bool p8ePersistentVisibilityReady;
    public bool p9LifeFirstSelectableReviewNeeded;
    public bool selectableGameplayEnabled;
    public bool affectsGameplaySuccessFailure;
    public string notes = string.Empty;
}

[Serializable]
public class P9BHumanitarianMarkerGenerationResult
{
    public bool success;
    public int expectedTotalCandidates;
    public int generatedMarkerCount;
    public int namedMarkerCount;
    public int idOnlyMarkerCount;
    public bool allCandidatesNonOfficial;
    public bool allCandidatesRequireNonOfficialWarning;
    public bool selectableGameplayEnabled;
    public bool affectsGameplaySuccessFailure;
    public P9BHumanitarianMarkerSnapshot[] snapshots = Array.Empty<P9BHumanitarianMarkerSnapshot>();
    public string summary = string.Empty;
}

[Serializable]
public class P9BHumanitarianMarkerSnapshot
{
    public string candidateId = string.Empty;
    public string buildingName = string.Empty;
    public string fallbackBuildingId = string.Empty;
    public Vector3 position;
    public bool isOfficialShelter;
    public bool nonOfficialWarningRequired;
    public bool selectableGameplayEnabled;
    public bool manualReviewNeeded;
    public string warningLabel = string.Empty;
}
