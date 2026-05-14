using System;
using System.Text;

[Serializable]
public class ResultMetrics
{
    public bool success;
    public string failureReason;
    public string selectedShelterId;
    public string selectedShelterName;
    public string shelterRank;
    public bool isOfficialShelter;
    public float warningStartTime;
    public float evacuationCountdownSeconds;
    public float shelterEntryTime;
    public float climbStartTime;
    public float climbCompleteTime;
    public float tsunamiArrivalTime;
    public float resultTime;
    public float entryDelaySeconds;
    public float climbTimeSeconds;
    public float crowdingDelaySeconds;
    public bool wasCampingDetected;
    public bool wasShelterBlockedByCampingRule;
    public string activeScenarioId;
    public string activeScenarioName;

    public string GetShelterLabel()
    {
        string id = string.IsNullOrWhiteSpace(selectedShelterId) ? "none" : selectedShelterId;
        string name = string.IsNullOrWhiteSpace(selectedShelterName) ? "No shelter selected" : selectedShelterName;
        string rank = string.IsNullOrWhiteSpace(shelterRank) ? "Unknown" : shelterRank;
        string official = isOfficialShelter ? "official" : "candidate";
        return $"{name} ({id}) | Rank {rank} | {official}";
    }

    public string GetElapsedTimeLabel()
    {
        float elapsedSeconds = 0f;

        if (success && climbCompleteTime > 0f && warningStartTime > 0f)
        {
            elapsedSeconds = climbCompleteTime - warningStartTime;
        }
        else if (tsunamiArrivalTime > 0f && warningStartTime > 0f)
        {
            elapsedSeconds = tsunamiArrivalTime - warningStartTime;
        }
        else if (resultTime > 0f && warningStartTime > 0f)
        {
            elapsedSeconds = resultTime - warningStartTime;
        }
        else if (shelterEntryTime > 0f && warningStartTime > 0f)
        {
            elapsedSeconds = shelterEntryTime - warningStartTime;
        }

        return FormatSeconds(elapsedSeconds);
    }

    public string GetDetailText()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Outcome");
        builder.AppendLine($"- Result: {(success ? "Success" : "Failure")}");
        builder.AppendLine($"- Reason: {GetOutcomeReason()}");
        builder.AppendLine();

        builder.AppendLine("Shelter");
        builder.AppendLine($"- ID/name: {GetShelterIdLabel()} / {GetShelterNameLabel()}");
        builder.AppendLine($"- Rank/official: {GetShelterRankLabel()} / {FormatBool(isOfficialShelter)}");
        builder.AppendLine();

        builder.AppendLine("Timing");
        builder.AppendLine($"- Entry {entryDelaySeconds:0.#}s | climb {climbTimeSeconds:0.#}s | crowd {crowdingDelaySeconds:0.#}s");
        builder.AppendLine($"- Countdown setting: {evacuationCountdownSeconds:0.#}s");

        if (tsunamiArrivalTime > 0f && warningStartTime > 0f)
        {
            builder.AppendLine($"- Risk after warning: {tsunamiArrivalTime - warningStartTime:0.#}s");
        }
        else
        {
            builder.AppendLine("- Risk after warning: none recorded");
        }

        builder.AppendLine();
        builder.AppendLine("Rules");
        builder.AppendLine($"- Camping detected: {FormatBool(wasCampingDetected)} | blocked: {FormatBool(wasShelterBlockedByCampingRule)}");
        builder.AppendLine($"- Scenario: {GetScenarioLabel()}");
        builder.AppendLine();
        builder.AppendLine($"Next: {GetNextStepAdvice()}");
        return builder.ToString().TrimEnd();
    }

    private string GetOutcomeReason()
    {
        if (!string.IsNullOrWhiteSpace(failureReason))
        {
            return failureReason;
        }

        return success
            ? "Reached a safe floor before the risk boundary arrived."
            : "Evacuation failed before reaching a safe floor.";
    }

    private string GetNextStepAdvice()
    {
        if (success)
        {
            return "Compare shelter timing and route risk in another scenario.";
        }

        if (wasShelterBlockedByCampingRule)
        {
            return "Start farther from known shelters or test a different evacuation choice.";
        }

        if (tsunamiArrivalTime > 0f)
        {
            return "Reduce delay, choose an earlier shelter, or test a slower tsunami scenario.";
        }

        return "Review shelter usability and entry timing before rerunning.";
    }

    private string GetShelterIdLabel()
    {
        return string.IsNullOrWhiteSpace(selectedShelterId) ? "none" : selectedShelterId;
    }

    private string GetShelterNameLabel()
    {
        return string.IsNullOrWhiteSpace(selectedShelterName) ? "No shelter selected" : selectedShelterName;
    }

    private string GetShelterRankLabel()
    {
        return string.IsNullOrWhiteSpace(shelterRank) ? "Unknown" : shelterRank;
    }

    private string GetScenarioLabel()
    {
        string id = string.IsNullOrWhiteSpace(activeScenarioId) ? "default" : activeScenarioId;
        string name = string.IsNullOrWhiteSpace(activeScenarioName) ? "Default" : activeScenarioName;
        return $"{name} ({id})";
    }

    private static string FormatSeconds(float seconds)
    {
        int totalSeconds = Math.Max(0, (int)Math.Round(seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }

    private static string FormatBool(bool value)
    {
        return value ? "yes" : "no";
    }
}
