using UnityEngine;

public class BuildingShelter : MonoBehaviour
{
    [Header("Shelter Data")]
    [SerializeField] private string shelterId = "test-shelter";
    [SerializeField] private string shelterName = "Test Shelter";
    [SerializeField] private string shelterRank = "A";
    [SerializeField] private bool isOfficialShelter;
    [SerializeField] private bool canEnter = true;
    [SerializeField] private string postEarthquakeStatus = "usable";
    [SerializeField] private float entryDelaySeconds;
    [SerializeField] private float climbTimeSeconds = 12f;
    [SerializeField] private float crowdingDelaySeconds;
    [SerializeField] private string failureReason = "This building cannot be used as a shelter.";
    [SerializeField] private string sourceType = "test";
    [SerializeField] private string facilityType = "debug_shelter";
    [SerializeField] private string realFacilityName;
    [SerializeField] private string address;
    [SerializeField] private float latitude;
    [SerializeField] private float longitude;
    [SerializeField] private string coordinateSystem = "debug_platform";
    [SerializeField] private string plateauBuildingId;
    [SerializeField] private int safeFloor;
    [SerializeField] private int capacity;
    [SerializeField] private string dataSource;
    [SerializeField] private string sourceUrl;
    [SerializeField] private string sourceUpdatedAt;
    [SerializeField] private string notes;

    private bool warnedMissingRealQualifiedMetadata;
    private bool warnedMissingHumanitarianCandidateMetadata;

    public string ShelterId => shelterId;
    public string ShelterName => string.IsNullOrWhiteSpace(shelterName) ? gameObject.name : shelterName;
    public string ShelterRank => shelterRank;
    public bool IsOfficialShelter => isOfficialShelter;
    public bool CanEnter => canEnter;
    public string PostEarthquakeStatus => postEarthquakeStatus;
    public float EntryDelaySeconds => Mathf.Max(0f, entryDelaySeconds);
    public float ClimbTimeSeconds => Mathf.Max(0.1f, climbTimeSeconds);
    public float CrowdingDelaySeconds => Mathf.Max(0f, crowdingDelaySeconds);
    public float TotalEvacuationDelaySeconds => Mathf.Max(0.1f, EntryDelaySeconds + ClimbTimeSeconds + CrowdingDelaySeconds);
    public string FailureReason => failureReason;
    public string SourceType => sourceType;
    public string FacilityType => facilityType;
    public int SafeFloor => Mathf.Max(0, safeFloor);
    public int Capacity => Mathf.Max(0, capacity);

    public void ApplyShelterData(ShelterDataLoader.ShelterData shelterData)
    {
        if (shelterData == null)
        {
            return;
        }

        shelterId = shelterData.shelterId;
        shelterName = shelterData.shelterName;
        shelterRank = shelterData.shelterRank;
        isOfficialShelter = shelterData.isOfficialShelter;
        canEnter = shelterData.canEnter;
        entryDelaySeconds = Mathf.Max(0f, shelterData.entryDelaySeconds);
        climbTimeSeconds = Mathf.Max(0.1f, shelterData.climbTimeSeconds);
        crowdingDelaySeconds = Mathf.Max(0f, shelterData.crowdingDelaySeconds);
        failureReason = string.IsNullOrWhiteSpace(shelterData.failureReason)
            ? "This shelter is not available."
            : shelterData.failureReason;
        sourceType = shelterData.sourceType;
        facilityType = shelterData.facilityType;
        realFacilityName = shelterData.realFacilityName;
        address = shelterData.address;
        latitude = shelterData.latitude;
        longitude = shelterData.longitude;
        coordinateSystem = shelterData.coordinateSystem;
        plateauBuildingId = shelterData.plateauBuildingId;
        safeFloor = Mathf.Max(0, shelterData.safeFloor);
        capacity = Mathf.Max(0, shelterData.capacity);
        dataSource = shelterData.dataSource;
        sourceUrl = shelterData.sourceUrl;
        sourceUpdatedAt = shelterData.sourceUpdatedAt;
        notes = shelterData.notes;
    }

    public bool CanUse(out string reason)
    {
        if (!canEnter)
        {
            reason = string.IsNullOrWhiteSpace(failureReason) ? "This shelter entrance is blocked." : failureReason;
            return false;
        }

        if (!string.Equals(postEarthquakeStatus, "usable", System.StringComparison.OrdinalIgnoreCase))
        {
            reason = $"Shelter is not usable after the earthquake: {postEarthquakeStatus}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public string GetDisplayText()
    {
        string officialText = isOfficialShelter ? "Official shelter" : "Candidate shelter";
        string enterText = canEnter ? "Enterable" : "Not enterable";
        string blockedText = canEnter ? string.Empty : $"\nReason: {FailureReason}";
        RealQualifiedShelterMetadata realQualifiedMetadata = GetComponent<RealQualifiedShelterMetadata>();
        HumanitarianCandidateMetadata humanitarianCandidateMetadata = GetComponent<HumanitarianCandidateMetadata>();
        string realQualifiedText = string.Empty;
        string humanitarianCandidateText = string.Empty;
        if (realQualifiedMetadata != null)
        {
            realQualifiedText = $"\n{realQualifiedMetadata.BuildPromptText()}";
        }
        else if (string.Equals(sourceType, RealQualifiedShelterDataLoader.SourceType, System.StringComparison.OrdinalIgnoreCase) &&
            !warnedMissingRealQualifiedMetadata)
        {
            Debug.LogWarning(
                $"real_qualified shelter '{ShelterId}' is missing RealQualifiedShelterMetadata. Continuing with base shelter prompt.",
                this);
            warnedMissingRealQualifiedMetadata = true;
        }

        if (humanitarianCandidateMetadata != null)
        {
            humanitarianCandidateText = $"\n{humanitarianCandidateMetadata.BuildPromptText()}";
        }
        else if (string.Equals(sourceType, HumanitarianCandidateDataLoader.SourceType, System.StringComparison.OrdinalIgnoreCase) &&
            !warnedMissingHumanitarianCandidateMetadata)
        {
            Debug.LogWarning(
                $"P5-GH humanitarian candidate '{ShelterId}' is missing HumanitarianCandidateMetadata. Continuing with base shelter prompt.",
                this);
            warnedMissingHumanitarianCandidateMetadata = true;
        }

        return
            $"{ShelterName}\n" +
            $"ID: {shelterId}\n" +
            $"Rank: {shelterRank} | {officialText}\n" +
            $"{enterText}\n" +
            $"Entry: {EntryDelaySeconds:0.#}s | Climb: {ClimbTimeSeconds:0.#}s | Crowd: {CrowdingDelaySeconds:0.#}s" +
            blockedText +
            realQualifiedText +
            humanitarianCandidateText;
    }
}
