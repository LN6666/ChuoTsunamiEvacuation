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
        return $"{ShelterName}\nID: {shelterId}\nRank: {shelterRank}\n{officialText}\n{enterText}\nClimb: {ClimbTimeSeconds:0.#}s, Crowd: {CrowdingDelaySeconds:0.#}s";
    }
}
