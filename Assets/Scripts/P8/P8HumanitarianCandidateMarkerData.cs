using System;
using System.IO;
using UnityEngine;

[Serializable]
public class P8HumanitarianCandidateMarkerDataSet
{
    public string datasetId = string.Empty;
    public string sourceAuditFile = string.Empty;
    public int totalCandidates;
    public int namedMarkerCount;
    public int idOnlyMarkerCount;
    public int clusterMarkerCount;
    public int dataOnlyPendingCount;
    public bool persistentSceneObjectsCreated;
    public bool selectableGameplayEnabled;
    public bool affectsGameplaySuccessFailure;
    public bool allCandidatesNonOfficial;
    public bool allCandidatesRequireNonOfficialWarning;
    public P8HumanitarianCandidateMarkerRecord[] records = new P8HumanitarianCandidateMarkerRecord[0];

    public static P8HumanitarianCandidateMarkerDataSet FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new P8HumanitarianCandidateMarkerDataSet();
        }

        P8HumanitarianCandidateMarkerDataSet data = JsonUtility.FromJson<P8HumanitarianCandidateMarkerDataSet>(json);
        return data ?? new P8HumanitarianCandidateMarkerDataSet();
    }

    public static P8HumanitarianCandidateMarkerDataSet LoadFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return new P8HumanitarianCandidateMarkerDataSet();
        }

        return FromJson(File.ReadAllText(path));
    }
}

[Serializable]
public class P8HumanitarianCandidateMarkerRecord
{
    public string candidateId = string.Empty;
    public string buildingName = string.Empty;
    public string fallbackBuildingId = string.Empty;
    public string locationText = string.Empty;
    public float latitude;
    public float longitude;
    public bool hasCoordinates;
    public string sourceEvidence = string.Empty;
    public string markerMode = "data_only_pending";
    public bool hazardStatusEligible;
    public bool nonOfficialWarningRequired = true;
    public bool isOfficialShelter;
    public bool manualReviewNeeded = true;
    public bool p8dDamageStatusEligible;
    public bool p8ePersistentVisibilityReady;
    public bool p9LifeFirstSelectableReviewNeeded = true;
    public bool selectableGameplayEnabled;
    public bool affectsGameplaySuccessFailure;
    public string notes = string.Empty;
}
