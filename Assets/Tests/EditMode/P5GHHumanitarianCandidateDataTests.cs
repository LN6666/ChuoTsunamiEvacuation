using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P5GHHumanitarianCandidateDataTests
{
    private GameObject gameplayRoot;
    private GameObject testGround;
    private GameObject generatorObject;

    [TearDown]
    public void TearDown()
    {
        DestroyImmediateIfPresent(generatorObject);
        DestroyImmediateIfPresent(GameObject.Find("P5GH_HumanitarianCandidates_Runtime"));
        DestroyImmediateIfPresent(gameplayRoot);
        DestroyImmediateIfPresent(testGround);
    }

    [Test]
    public void SourceConfigDefaultsKeepHumanitarianFlagsOff()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config =
            ShelterSourceConfigLoader.CreateDefaultConfig();

        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, config.sourceMode);
        Assert.IsFalse(config.enableHumanitarianCandidates);
        Assert.IsFalse(config.enableLifeFirstCandidateSelection);

        ShelterSourceConfigLoader.ShelterSourceConfig assetConfig = ShelterSourceConfigLoader.Load();
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, assetConfig.sourceMode);
        Assert.IsFalse(assetConfig.enableHumanitarianCandidates);
        Assert.IsFalse(assetConfig.enableLifeFirstCandidateSelection);
    }

    [Test]
    public void HumanitarianCandidateSampleAssetLoadsFromAssetsDataOnly()
    {
        string path = AssetsDataPath(HumanitarianCandidateDataLoader.FileName);
        Assert.IsTrue(File.Exists(path), path);

        HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult result =
            HumanitarianCandidateDataLoader.LoadFromAssetsData();

        Assert.IsTrue(result.success);
        Assert.IsTrue(result.usesAssetsDataOnly);
        Assert.That(Normalize(result.sourcePath), Does.Contain("/Assets/Data/"));
        Assert.AreEqual("p5_f_highrise_humanitarian_candidates_sample", result.datasetId);
        Assert.AreEqual(7, result.rawRecordCount);
        Assert.AreEqual(5, result.recordCount);
        Assert.AreEqual(2, result.skippedNonHumanitarianLayerCount);
        Assert.AreEqual(2, result.selectableCount);

        foreach (HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record in result.records)
        {
            Assert.AreEqual(HumanitarianCandidateDataLoader.CandidateLayer, record.candidateLayer);
            Assert.AreNotEqual("official_confirmed", record.humanitarianCandidateStatus);
            Assert.AreNotEqual("official_confirmed_with_review", record.humanitarianCandidateStatus);
            Assert.That(record.warnings, Does.Contain("not an official shelter; humanitarian emergency candidate only"));
            Assert.That(record.reviewRisks, Does.Contain("non_official_status"));
        }
    }

    [TestCase("../data_pipeline/raw/test.json")]
    [TestCase("../data_pipeline/downloads/test.json")]
    [TestCase("../data_pipeline/cache/test.json")]
    [TestCase("../tmp/test.json")]
    [TestCase("../.venv/test.json")]
    public void HumanitarianCandidateLoaderRejectsForbiddenRuntimePaths(string relativeForbiddenPath)
    {
        string forbidden = Path.GetFullPath(Path.Combine(Application.dataPath, relativeForbiddenPath));

        LogAssert.Expect(LogType.Warning, new Regex("must use copied static JSON under Assets/Data only"));
        HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult result =
            HumanitarianCandidateDataLoader.LoadFromPath(forbidden);

        Assert.IsFalse(result.success);
        Assert.IsFalse(result.usesAssetsDataOnly);
        Assert.AreEqual(0, result.recordCount);
    }

    [Test]
    public void CandidateLayerSeparationSkipsOfficialLayerRecords()
    {
        HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult result =
            HumanitarianCandidateDataLoader.LoadFromJson(
                BuildInlineDataset(
                    BuildRecord("official_fixture", "official", "official_confirmed", "official_designated", "confirmed_public", "official_agreement_confirmed", "confirmed", false),
                    BuildRecord("human_fixture", "humanitarian_candidate", "humanitarian_strong_candidate", "not_official", "likely_public_or_lobby_access", "unknown", "estimated", true)),
                "inline candidate layer fixture");

        Assert.IsTrue(result.success);
        Assert.AreEqual(2, result.rawRecordCount);
        Assert.AreEqual(1, result.recordCount);
        Assert.AreEqual(1, result.skippedNonHumanitarianLayerCount);
        Assert.AreEqual("human_fixture", result.records[0].candidateId);
        Assert.AreEqual(HumanitarianCandidateDataLoader.CandidateLayer, result.records[0].candidateLayer);
        StringAssert.Contains("candidateLayer 'official'", result.diagnostics);
        Assert.AreNotEqual("official_fixture", result.records[0].candidateId);
    }

    [Test]
    public void UnknownPublicAccessTriggersManualReviewAndReviewRisk()
    {
        HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult result =
            HumanitarianCandidateDataLoader.LoadFromJson(
                BuildInlineDataset(BuildRecord(
                    "unknown_access",
                    "humanitarian_candidate",
                    "humanitarian_candidate_with_review",
                    "unknown",
                    "unknown",
                    "unknown",
                    "unknown",
                    false)),
                "inline unknown access fixture");

        Assert.IsTrue(result.success);
        HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record = result.records[0];
        Assert.IsTrue(record.manualReviewNeeded);
        Assert.That(record.warnings, Does.Contain(HumanitarianCandidateDataLoader.AccessUncertainLabel));
        Assert.That(record.warnings, Does.Contain(HumanitarianCandidateDataLoader.ManagementUncertainLabel));
        Assert.That(record.warnings, Does.Contain(HumanitarianCandidateDataLoader.SeismicUncertainLabel));
        Assert.That(record.reviewRisks, Does.Contain("public_access_unknown"));
        Assert.That(record.reviewRisks, Does.Contain("management_agreement_unknown"));
        Assert.That(record.reviewRisks, Does.Contain("seismic_evidence_unknown"));
    }

    [Test]
    public void MetadataLabelsPreserveNonOfficialLifeFirstWarningsAndReviewRisks()
    {
        HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record =
            HumanitarianCandidateDataLoader.LoadFromAssetsData().records[1];

        GameObject metadataObject = new GameObject("P5GH_Metadata_EditMode_Test");
        try
        {
            HumanitarianCandidateMetadata metadata =
                metadataObject.AddComponent<HumanitarianCandidateMetadata>();
            metadata.ApplyRecord(record);

            string compact = metadata.BuildCompactLabel();
            string detailed = metadata.BuildDetailedLabel();
            string prompt = metadata.BuildPromptText();

            StringAssert.Contains(HumanitarianCandidateDataLoader.HumanitarianCandidateLabel, compact);
            StringAssert.Contains(HumanitarianCandidateDataLoader.NotOfficiallyDesignatedLabel, compact);
            StringAssert.Contains(HumanitarianCandidateDataLoader.LifeFirstAssumptionLabel, detailed);
            StringAssert.Contains(HumanitarianCandidateDataLoader.NotLegalAccessGuaranteeLabel, detailed);
            StringAssert.Contains(HumanitarianCandidateDataLoader.ControlledSampleNotice, detailed);
            StringAssert.Contains("Review risk:", detailed);
            StringAssert.Contains(HumanitarianCandidateDataLoader.ManualReviewNeededLabel, prompt);
        }
        finally
        {
            DestroyImmediateIfPresent(metadataObject);
        }
    }

    [Test]
    public void FeedbackFormatterAlwaysStartsWithProminentNonOfficialWarning()
    {
        var record = new HumanitarianCandidateDataLoader.HumanitarianCandidateRecord
        {
            candidateId = "minimal_candidate"
        };

        string feedback = HumanitarianCandidateFeedbackFormatter.BuildFromRecord(record);
        string firstLine = feedback.Split('\n')[0].Trim();

        StringAssert.Contains(HumanitarianCandidateDataLoader.NotOfficiallyDesignatedWarning, firstLine);
        Assert.That(feedback.ToLowerInvariant(), Does.Contain("not officially designated"));
    }

    [Test]
    public void LifeFirstSelectabilityOnlyAllowsStrongAndReviewCandidates()
    {
        HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult result =
            HumanitarianCandidateDataLoader.LoadFromAssetsData();

        int selectable = 0;
        foreach (HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record in result.records)
        {
            if (record.isSelectableInLifeFirstMode)
            {
                selectable++;
                Assert.IsTrue(
                    HumanitarianCandidateDataLoader.IsDefaultLifeFirstSelectableStatus(record.humanitarianCandidateStatus),
                    record.humanitarianCandidateStatus);
                Assert.AreEqual(HumanitarianCandidateDataLoader.CandidateLayer, record.candidateLayer);
                Assert.AreNotEqual("official_designated", record.officialDesignationStatus);
            }
            else
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(record.nonSelectableReason));
                Assert.IsFalse(record.humanitarianCandidateStatus == "humanitarian_strong_candidate");
                Assert.IsFalse(record.humanitarianCandidateStatus == "humanitarian_candidate_with_review");
            }
        }

        Assert.AreEqual(2, selectable);
    }

    [Test]
    public void DisplayOnlyGenerationCreatesNonPlayableMarkers()
    {
        gameplayRoot = new GameObject("GameplayTestRoot");
        testGround = new GameObject("TestGround");
        testGround.transform.position = new Vector3(0f, 80f, -250f);

        generatorObject = new GameObject("P5GH_EditMode_Generator");
        P5GHHumanitarianCandidateRuntimeGenerator generator =
            generatorObject.AddComponent<P5GHHumanitarianCandidateRuntimeGenerator>();

        int generated = generator.Generate(
            HumanitarianCandidateDataLoader.LoadFromAssetsData(),
            false,
            null,
            null);

        Assert.AreEqual(5, generated);
        Assert.AreEqual(5, generator.LastGeneratedDisplayOnlyCount);
        Assert.AreEqual(0, generator.LastGeneratedSelectableCount);

        Transform root = gameplayRoot.transform.Find("P5GH_HumanitarianCandidates_Runtime");
        Assert.NotNull(root);
        Assert.AreEqual(5, root.GetComponentsInChildren<HumanitarianCandidateMetadata>().Length);
        Assert.AreEqual(0, root.GetComponentsInChildren<BuildingShelter>().Length);
        Assert.AreEqual(0, root.GetComponentsInChildren<ShelterEntranceTrigger>().Length);
        Assert.AreEqual(0, root.GetComponentsInChildren<Collider>().Length);
    }

    private static string BuildInlineDataset(params string[] records)
    {
        return
            "{" +
            "\"datasetId\":\"inline_p5gh_fixture\"," +
            "\"generatedAt\":\"2026-05-21T00:00:00Z\"," +
            "\"coordinateReferenceSystem\":\"EPSG:4326\"," +
            "\"rulebookVersion\":\"p5-f-highrise-humanitarian-candidates-v1\"," +
            "\"notes\":\"inline test fixture\"," +
            "\"records\":[" + string.Join(",", records) + "]}";
    }

    private static string BuildRecord(
        string candidateId,
        string candidateLayer,
        string status,
        string officialDesignationStatus,
        string publicAccessStatus,
        string managementAgreementStatus,
        string seismicEvidenceLevel,
        bool manualReviewNeeded)
    {
        return
            "{" +
            $"\"candidateId\":\"{candidateId}\"," +
            $"\"buildingId\":\"building_{candidateId}\"," +
            $"\"plateauBuildingId\":\"plateau_{candidateId}\"," +
            $"\"buildingName\":\"Building {candidateId}\"," +
            "\"address\":\"Inline address\"," +
            "\"latitude\":35.66," +
            "\"longitude\":139.77," +
            "\"heightMeters\":60.0," +
            "\"floorsAboveGround\":18," +
            "\"usageType\":\"office\"," +
            "\"buildingUse\":\"private_office_tower\"," +
            "\"estimatedCapacityProxy\":500," +
            "\"tsunamiRiskContext\":null," +
            "\"distanceToOfficialShelter\":500.0," +
            "\"routeDistanceMeters\":400.0," +
            "\"routeTimeSeconds\":320.0," +
            $"\"seismicEvidenceLevel\":\"{seismicEvidenceLevel}\"," +
            "\"seismicEvidenceSource\":null," +
            $"\"publicAccessStatus\":\"{publicAccessStatus}\"," +
            $"\"managementAgreementStatus\":\"{managementAgreementStatus}\"," +
            $"\"officialDesignationStatus\":\"{officialDesignationStatus}\"," +
            $"\"candidateLayer\":\"{candidateLayer}\"," +
            $"\"humanitarianCandidateStatus\":\"{status}\"," +
            "\"confidence\":\"medium\"," +
            $"\"manualReviewNeeded\":{manualReviewNeeded.ToString().ToLowerInvariant()}," +
            "\"warnings\":[]," +
            "\"reviewTriggers\":[]," +
            "\"reviewRisks\":[]," +
            "\"sourceRefs\":[]," +
            "\"physicalSuitabilityScore\":80," +
            "\"operationalUncertaintyScore\":45," +
            "\"classificationReason\":\"Inline test record.\"" +
            "}";
    }

    private static string AssetsDataPath(string fileName)
    {
        return Path.Combine(Application.dataPath, "Data", fileName);
    }

    private static string Normalize(string path)
    {
        return path.Replace('\\', '/');
    }

    private static void DestroyImmediateIfPresent(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}
