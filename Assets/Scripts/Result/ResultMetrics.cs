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

        if (!string.IsNullOrWhiteSpace(failureReason))
        {
            builder.AppendLine($"Reason: {failureReason}");
        }

        builder.AppendLine($"Timing: entry {entryDelaySeconds:0.#}s | climb {climbTimeSeconds:0.#}s | crowd {crowdingDelaySeconds:0.#}s");
        builder.Append($"Countdown: {evacuationCountdownSeconds:0.#}s | ");

        if (tsunamiArrivalTime > 0f && warningStartTime > 0f)
        {
            builder.AppendLine($"Risk after warning: {tsunamiArrivalTime - warningStartTime:0.#}s");
        }
        else
        {
            builder.AppendLine("Risk after warning: none recorded");
        }

        builder.AppendLine($"Camping: detected {FormatBool(wasCampingDetected)} | blocked {FormatBool(wasShelterBlockedByCampingRule)}");
        return builder.ToString().TrimEnd();
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
