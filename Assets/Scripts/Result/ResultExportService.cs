using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public static class ResultExportService
{
    public const string OutputDirectoryName = "run_logs";
    public const string CsvFileName = "run_results.csv";
    public const bool DefaultExportEnabled = true;

    public static bool ExportEnabled = DefaultExportEnabled;

    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    [Serializable]
    public class ResultExportRecord
    {
        public string runId;
        public string timestamp;
        public string scenarioId;
        public string scenarioName;
        public bool success;
        public string outcome;
        public string outcomeReason;
        public string failureReason;
        public string successReason;
        public string selectedShelterId;
        public string selectedShelterName;
        public string shelterRank;
        public bool isOfficialShelter;
        public float entryDelaySeconds;
        public float climbTimeSeconds;
        public float crowdingDelaySeconds;
        public float evacuationCountdownSeconds;
        public float warningStartTime;
        public float shelterEntryTime;
        public float climbStartTime;
        public float climbCompleteTime;
        public float tsunamiArrivalTime;
        public float resultTime;
        public bool wasCampingDetected;
        public bool wasShelterBlockedByCampingRule;
        public string advice;
    }

    public static void Export(ResultMetrics metrics)
    {
        Export(metrics, GetDefaultOutputDirectory());
    }

    public static void Export(ResultMetrics metrics, string outputDirectory)
    {
        if (!ExportEnabled)
        {
            return;
        }

        try
        {
            ResultExportRecord record = CreateRecord(metrics);
            string directory = string.IsNullOrWhiteSpace(outputDirectory)
                ? GetDefaultOutputDirectory()
                : outputDirectory;

            Directory.CreateDirectory(directory);
            AppendCsv(record, Path.Combine(directory, CsvFileName));
            WriteJson(record, Path.Combine(directory, $"run_{record.runId}.json"));
            Debug.Log($"Exported evacuation result run log to {directory}.");
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not export evacuation result log. {exception.Message}");
        }
    }

    public static ResultExportRecord CreateRecord(ResultMetrics metrics)
    {
        if (metrics == null)
        {
            metrics = new ResultMetrics();
        }

        if (string.IsNullOrWhiteSpace(metrics.runId))
        {
            metrics.runId = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrWhiteSpace(metrics.timestamp))
        {
            metrics.timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        }

        if (string.IsNullOrWhiteSpace(metrics.advice))
        {
            metrics.advice = metrics.GetAdviceText();
        }

        string outcomeReason = metrics.GetOutcomeReason();

        return new ResultExportRecord
        {
            runId = SafeString(metrics.runId),
            timestamp = SafeString(metrics.timestamp),
            scenarioId = SafeString(metrics.activeScenarioId, ScenarioPresetLoader.DefaultScenarioId),
            scenarioName = SafeString(metrics.activeScenarioName, "Default"),
            success = metrics.success,
            outcome = metrics.success ? "success" : "failure",
            outcomeReason = SafeString(outcomeReason),
            failureReason = metrics.success ? string.Empty : SafeString(outcomeReason),
            successReason = metrics.success ? SafeString(outcomeReason) : string.Empty,
            selectedShelterId = SafeString(metrics.selectedShelterId),
            selectedShelterName = SafeString(metrics.selectedShelterName),
            shelterRank = SafeString(metrics.shelterRank),
            isOfficialShelter = metrics.isOfficialShelter,
            entryDelaySeconds = SafeFloat(metrics.entryDelaySeconds),
            climbTimeSeconds = SafeFloat(metrics.climbTimeSeconds),
            crowdingDelaySeconds = SafeFloat(metrics.crowdingDelaySeconds),
            evacuationCountdownSeconds = SafeFloat(metrics.evacuationCountdownSeconds),
            warningStartTime = SafeFloat(metrics.warningStartTime),
            shelterEntryTime = SafeFloat(metrics.shelterEntryTime),
            climbStartTime = SafeFloat(metrics.climbStartTime),
            climbCompleteTime = SafeFloat(metrics.climbCompleteTime),
            tsunamiArrivalTime = SafeFloat(metrics.tsunamiArrivalTime),
            resultTime = SafeFloat(metrics.resultTime),
            wasCampingDetected = metrics.wasCampingDetected,
            wasShelterBlockedByCampingRule = metrics.wasShelterBlockedByCampingRule,
            advice = SafeString(metrics.advice)
        };
    }

    public static string ToJson(ResultExportRecord record)
    {
        return JsonUtility.ToJson(record ?? CreateRecord(null), true);
    }

    public static string ToCsvHeader()
    {
        return string.Join(",",
            "runId",
            "timestamp",
            "scenarioId",
            "scenarioName",
            "outcome",
            "outcomeReason",
            "failureReason",
            "successReason",
            "selectedShelterId",
            "selectedShelterName",
            "shelterRank",
            "isOfficialShelter",
            "entryDelaySeconds",
            "climbTimeSeconds",
            "crowdingDelaySeconds",
            "evacuationCountdownSeconds",
            "warningStartTime",
            "shelterEntryTime",
            "climbStartTime",
            "climbCompleteTime",
            "tsunamiArrivalTime",
            "resultTime",
            "wasCampingDetected",
            "wasShelterBlockedByCampingRule",
            "advice");
    }

    public static string ToCsvRow(ResultExportRecord record)
    {
        if (record == null)
        {
            record = CreateRecord(null);
        }

        return string.Join(",",
            Csv(record.runId),
            Csv(record.timestamp),
            Csv(record.scenarioId),
            Csv(record.scenarioName),
            Csv(record.outcome),
            Csv(record.outcomeReason),
            Csv(record.failureReason),
            Csv(record.successReason),
            Csv(record.selectedShelterId),
            Csv(record.selectedShelterName),
            Csv(record.shelterRank),
            Csv(record.isOfficialShelter),
            Csv(record.entryDelaySeconds),
            Csv(record.climbTimeSeconds),
            Csv(record.crowdingDelaySeconds),
            Csv(record.evacuationCountdownSeconds),
            Csv(record.warningStartTime),
            Csv(record.shelterEntryTime),
            Csv(record.climbStartTime),
            Csv(record.climbCompleteTime),
            Csv(record.tsunamiArrivalTime),
            Csv(record.resultTime),
            Csv(record.wasCampingDetected),
            Csv(record.wasShelterBlockedByCampingRule),
            Csv(record.advice));
    }

    public static string GetDefaultOutputDirectory()
    {
        string projectRoot = Application.dataPath;
        DirectoryInfo parent = Directory.GetParent(Application.dataPath);
        if (parent != null)
        {
            projectRoot = parent.FullName;
        }

        return Path.Combine(projectRoot, OutputDirectoryName);
    }

    private static void AppendCsv(ResultExportRecord record, string csvPath)
    {
        bool writeHeader = !File.Exists(csvPath) || new FileInfo(csvPath).Length == 0;
        var builder = new StringBuilder();

        if (writeHeader)
        {
            builder.AppendLine(ToCsvHeader());
        }

        builder.AppendLine(ToCsvRow(record));
        File.AppendAllText(csvPath, builder.ToString(), Utf8NoBom);
    }

    private static void WriteJson(ResultExportRecord record, string jsonPath)
    {
        File.WriteAllText(jsonPath, ToJson(record), Utf8NoBom);
    }

    private static string Csv(string value)
    {
        value = SafeString(value);
        bool mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");
        value = value.Replace("\"", "\"\"");
        return mustQuote ? $"\"{value}\"" : value;
    }

    private static string Csv(bool value)
    {
        return value ? "true" : "false";
    }

    private static string Csv(float value)
    {
        return SafeFloat(value).ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string SafeString(string value, string fallback = "")
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static float SafeFloat(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
    }
}
