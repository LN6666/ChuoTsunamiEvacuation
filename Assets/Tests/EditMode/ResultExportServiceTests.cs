using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class ResultExportServiceTests
{
    [TearDown]
    public void ResetExportSetting()
    {
        ResultExportService.ExportEnabled = ResultExportService.DefaultExportEnabled;
    }

    [Test]
    public void ExportRecordContainsRequiredResultFields()
    {
        ResultMetrics metrics = CreateSuccessMetrics();

        ResultExportService.ResultExportRecord record = ResultExportService.CreateRecord(metrics);

        Assert.IsFalse(string.IsNullOrWhiteSpace(record.runId));
        Assert.IsFalse(string.IsNullOrWhiteSpace(record.timestamp));
        Assert.AreEqual("default", record.scenarioId);
        Assert.AreEqual("Default", record.scenarioName);
        Assert.IsTrue(record.success);
        Assert.AreEqual("success", record.outcome);
        Assert.AreEqual("test_shelter_far_fast", record.selectedShelterId);
        Assert.AreEqual("Far Fast Shelter", record.selectedShelterName);
        Assert.AreEqual("A", record.shelterRank);
        Assert.IsTrue(record.isOfficialShelter);
        Assert.AreEqual(0f, record.entryDelaySeconds);
        Assert.AreEqual(5f, record.climbTimeSeconds);
        Assert.AreEqual(0f, record.crowdingDelaySeconds);
        Assert.AreEqual(60f, record.evacuationCountdownSeconds);
        Assert.IsFalse(string.IsNullOrWhiteSpace(record.advice));
    }

    [Test]
    public void CsvExportCreatesHeaderAndOneRow()
    {
        string outputDirectory = CreateTempDirectoryPath();

        try
        {
            ResultExportService.Export(CreateSuccessMetrics(), outputDirectory);

            string csvPath = Path.Combine(outputDirectory, ResultExportService.CsvFileName);
            Assert.IsTrue(File.Exists(csvPath));

            string[] lines = File.ReadAllLines(csvPath);
            Assert.AreEqual(2, lines.Length);
            StringAssert.Contains("runId", lines[0]);
            StringAssert.Contains("scenarioId", lines[0]);
            StringAssert.Contains("advice", lines[0]);
            StringAssert.Contains("test_shelter_far_fast", lines[1]);
        }
        finally
        {
            DeleteDirectory(outputDirectory);
        }
    }

    [Test]
    public void JsonExportCreatesValidJsonRecord()
    {
        string outputDirectory = CreateTempDirectoryPath();

        try
        {
            ResultMetrics metrics = CreateSuccessMetrics();
            ResultExportService.Export(metrics, outputDirectory);

            string[] jsonFiles = Directory.GetFiles(outputDirectory, "run_*.json");
            Assert.AreEqual(1, jsonFiles.Length);

            string json = File.ReadAllText(jsonFiles[0]);
            ResultExportService.ResultExportRecord record =
                JsonUtility.FromJson<ResultExportService.ResultExportRecord>(json);

            Assert.NotNull(record);
            Assert.AreEqual(metrics.runId, record.runId);
            Assert.AreEqual("success", record.outcome);
            Assert.AreEqual("test_shelter_far_fast", record.selectedShelterId);
        }
        finally
        {
            DeleteDirectory(outputDirectory);
        }
    }

    [Test]
    public void ExportHandlesMissingOptionalFieldsSafely()
    {
        ResultExportService.ResultExportRecord record =
            ResultExportService.CreateRecord(new ResultMetrics());

        Assert.IsFalse(string.IsNullOrWhiteSpace(record.runId));
        Assert.IsFalse(string.IsNullOrWhiteSpace(record.timestamp));
        Assert.AreEqual("default", record.scenarioId);
        Assert.AreEqual("failure", record.outcome);
        Assert.IsNotNull(record.selectedShelterId);
        Assert.IsNotNull(record.selectedShelterName);
        Assert.IsNotNull(record.advice);
    }

    [Test]
    public void ExportCreatesRunLogsDirectoryIfMissing()
    {
        string outputDirectory = CreateTempDirectoryPath();
        Assert.IsFalse(Directory.Exists(outputDirectory));

        try
        {
            ResultExportService.Export(CreateSuccessMetrics(), outputDirectory);

            Assert.IsTrue(Directory.Exists(outputDirectory));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, ResultExportService.CsvFileName)));
        }
        finally
        {
            DeleteDirectory(outputDirectory);
        }
    }

    [Test]
    public void AdviceCoversBlockedShelter()
    {
        ResultMetrics metrics = new ResultMetrics
        {
            success = false,
            failureReason = "The nearest shelter is unavailable after the earthquake."
        };

        StringAssert.Contains("Try another shelter", metrics.GetAdviceText());
    }

    [Test]
    public void AdviceCoversCampingBlockedShelter()
    {
        ResultMetrics metrics = new ResultMetrics
        {
            success = false,
            wasShelterBlockedByCampingRule = true
        };

        StringAssert.Contains("Avoid waiting", metrics.GetAdviceText());
    }

    [Test]
    public void AdviceCoversLateRiskFailure()
    {
        ResultMetrics metrics = new ResultMetrics
        {
            success = false,
            failureReason = "The tsunami risk reached the shelter entrance before you reached a safe floor."
        };

        StringAssert.Contains("faster or closer", metrics.GetAdviceText());
    }

    [Test]
    public void ExportRecordIncludesGeneratedAdviceForLateRiskFailure()
    {
        ResultMetrics metrics = new ResultMetrics
        {
            success = false,
            failureReason = "The tsunami risk front passed the player.",
            selectedShelterId = "test_shelter_slow_safe",
            selectedShelterName = "Slow Safe Shelter",
            activeScenarioId = "late_failure",
            activeScenarioName = "Late Failure"
        };

        ResultExportService.ResultExportRecord record = ResultExportService.CreateRecord(metrics);

        Assert.AreEqual(metrics.GetAdviceText(), record.advice);
        StringAssert.Contains("faster or closer", record.advice);
    }

    [Test]
    public void AdviceCoversCrowdingDelay()
    {
        ResultMetrics metrics = new ResultMetrics
        {
            success = false,
            crowdingDelaySeconds = 8f
        };

        StringAssert.Contains("less crowded", metrics.GetAdviceText());
    }

    [Test]
    public void AdviceCoversSuccessfulFastChoice()
    {
        ResultMetrics metrics = new ResultMetrics
        {
            success = true,
            climbTimeSeconds = 5f
        };

        StringAssert.Contains("Fast shelter", metrics.GetAdviceText());
    }

    private static ResultMetrics CreateSuccessMetrics()
    {
        return new ResultMetrics
        {
            success = true,
            failureReason = "Reached a safe floor before the risk boundary arrived.",
            selectedShelterId = "test_shelter_far_fast",
            selectedShelterName = "Far Fast Shelter",
            shelterRank = "A",
            isOfficialShelter = true,
            entryDelaySeconds = 0f,
            climbTimeSeconds = 5f,
            crowdingDelaySeconds = 0f,
            evacuationCountdownSeconds = 60f,
            warningStartTime = 10f,
            shelterEntryTime = 18f,
            climbStartTime = 18f,
            climbCompleteTime = 23f,
            resultTime = 23f,
            activeScenarioId = "default",
            activeScenarioName = "Default"
        };
    }

    private static string CreateTempDirectoryPath()
    {
        return Path.Combine(Path.GetTempPath(), $"chuo_run_logger_test_{Guid.NewGuid():N}");
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
