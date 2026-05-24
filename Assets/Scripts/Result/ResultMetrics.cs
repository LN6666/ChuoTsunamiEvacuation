using System;
using System.Text;

[Serializable]
public class ResultMetrics
{
    public bool success;
    public string failureReason;
    public string selectedShelterId;
    public string selectedShelterName;
    public string selectedShelterSourceType;
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
    public string runId;
    public string timestamp;
    public string advice;
    public string p5cDecisionFeedback;
    public string p5dDecisionFeedback;
    public string p5gHumanitarianCandidateFeedback;
    public string p9cOutcomeFeedback;
    public string p9cFinalReasonCode;

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

        if (!string.IsNullOrWhiteSpace(p5cDecisionFeedback))
        {
            builder.AppendLine("P5-C evidence");
            builder.AppendLine(p5cDecisionFeedback.Trim());
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(p5dDecisionFeedback))
        {
            builder.AppendLine("P5-D real qualified feedback");
            builder.AppendLine(p5dDecisionFeedback.Trim());
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(p5gHumanitarianCandidateFeedback))
        {
            builder.AppendLine("P5-GH humanitarian candidate feedback");
            builder.AppendLine(p5gHumanitarianCandidateFeedback.Trim());
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(p9cOutcomeFeedback))
        {
            builder.AppendLine("P9-C evacuation proxy feedback");
            builder.AppendLine(p9cOutcomeFeedback.Trim());
            if (!string.IsNullOrWhiteSpace(p9cFinalReasonCode))
            {
                builder.AppendLine($"P9-C final reason code: {p9cFinalReasonCode}");
            }

            builder.AppendLine();
        }

        builder.AppendLine($"Next: {GetAdviceText()}");
        return builder.ToString().TrimEnd();
    }

    public string GetOutcomeReason()
    {
        if (!string.IsNullOrWhiteSpace(failureReason))
        {
            return failureReason;
        }

        return success
            ? "Reached a safe floor before the risk boundary arrived."
            : "Evacuation failed before reaching a safe floor.";
    }

    public string GetAdviceText()
    {
        return string.IsNullOrWhiteSpace(advice) ? GenerateAdviceText() : advice;
    }

    private string GenerateAdviceText()
    {
        if (wasShelterBlockedByCampingRule)
        {
            return "Avoid waiting at the same shelter before warning.";
        }

        string reason = string.IsNullOrWhiteSpace(failureReason) ? string.Empty : failureReason.ToLowerInvariant();

        if (reason.Contains("blocked") || reason.Contains("unavailable") || reason.Contains("not available"))
        {
            return "Try another shelter when the nearest one is unavailable.";
        }

        if (!success && (IsRiskRelatedFailure(reason) || tsunamiArrivalTime > 0f))
        {
            return "Choose a faster or closer shelter under this scenario.";
        }

        if (crowdingDelaySeconds > 0f)
        {
            return success
                ? "Success, but compare less crowded shelters to reduce evacuation time."
                : "Compare less crowded shelters when crowding delay makes evacuation slower.";
        }

        if (success)
        {
            float totalDelay = entryDelaySeconds + climbTimeSeconds + crowdingDelaySeconds;
            if (totalDelay > 0f && totalDelay <= 8f)
            {
                return "Fast shelter timing worked; compare it with closer options in other scenarios.";
            }

            return "Compare this shelter choice with the other scenario options.";
        }

        return "Review shelter usability and entry timing before rerunning.";
    }

    private static bool IsRiskRelatedFailure(string normalizedReason)
    {
        if (string.IsNullOrWhiteSpace(normalizedReason))
        {
            return false;
        }

        return normalizedReason.Contains("risk reached the shelter entrance") ||
            normalizedReason.Contains("tsunami risk reached") ||
            normalizedReason.Contains("risk front") ||
            normalizedReason.Contains("risk boundary") ||
            normalizedReason.Contains("before you reached a safe floor");
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
