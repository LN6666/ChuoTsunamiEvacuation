using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P5CStaticDataLoaderTests
{
    [Test]
    public void SourceModeDefaultRemainsTest()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.CreateDefaultConfig();

        Assert.NotNull(config);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, config.sourceMode);
        Assert.IsFalse(config.enableRealSampleLoading);
        Assert.IsFalse(config.enableP5COverlay);
    }

    [Test]
    public void RequiredP5CAssetsDataJsonFilesExist()
    {
        AssertAssetDataFileExists(P5CStaticDataLoader.IntegratedRouteQualificationFileName);
        AssertAssetDataFileExists(P5CStaticDataLoader.RouteSampleFileName);
        AssertAssetDataFileExists(P5CStaticDataLoader.BuildingQualificationFileName);
        AssertAssetDataFileExists(P5CStaticDataLoader.ShelterBuildingMatchesFileName);
    }

    [Test]
    public void IntegratedQualificationAssetParsesRepresentativeRecords()
    {
        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.IntegratedRouteQualificationRecord> result =
            P5CStaticDataLoader.LoadIntegratedRouteQualificationsFromPath(
                AssetsDataPath(P5CStaticDataLoader.IntegratedRouteQualificationFileName));

        Assert.IsTrue(result.success);
        Assert.AreEqual("p5_b5_real_chuo_integrated_route_qualification", result.datasetId);
        Assert.AreEqual("EPSG:4326", result.coordinateReferenceSystem);
        Assert.GreaterOrEqual(result.records.Length, 31);
        Assert.That(result.osmAttribution, Does.Contain("OpenStreetMap"));
        Assert.That(result.osmAttribution, Does.Contain("Open Database License"));

        P5CStaticDataLoader.IntegratedRouteQualificationRecord first = result.records[0];
        Assert.IsFalse(string.IsNullOrWhiteSpace(first.shelterId));
        Assert.AreEqual("official_confirmed", first.qualificationStatus);
        Assert.AreEqual("high", first.confidence);
        Assert.IsFalse(first.manualReviewNeeded);
        Assert.Greater(first.warnings.Length, 0);
        Assert.IsTrue(first.HasNearestRouteDistanceMeters);
        Assert.IsTrue(first.HasEstimatedTravelTimeSeconds);
        Assert.AreEqual("estimated_pedestrian_route", first.routeType);
        Assert.IsTrue(first.IsEstimatedPrototypeRoute);
        Assert.IsFalse(first.isOfficialEvacuationRoute);
        Assert.IsFalse(string.IsNullOrWhiteSpace(first.nearestRouteId));
    }

    [Test]
    public void RouteSampleAssetParsesEstimatedPrototypeRouteRecords()
    {
        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.RouteSampleRecord> result =
            P5CStaticDataLoader.LoadRouteSamplesFromPath(AssetsDataPath(P5CStaticDataLoader.RouteSampleFileName));

        Assert.IsTrue(result.success);
        Assert.AreEqual("p5_b5_real_chuo_osm_routes_sample", result.datasetId);
        Assert.AreEqual("EPSG:4326", result.coordinateReferenceSystem);
        Assert.AreEqual("EPSG:6677", result.metricCoordinateReferenceSystem);
        Assert.GreaterOrEqual(result.records.Length, 135);
        Assert.That(result.osmAttribution, Does.Contain("OpenStreetMap"));
        Assert.That(result.osmAttribution, Does.Contain("Open Database License"));

        P5CStaticDataLoader.RouteSampleRecord first = result.records[0];
        Assert.IsFalse(string.IsNullOrWhiteSpace(first.routeId));
        Assert.AreEqual("available", first.routeAvailability);
        Assert.IsTrue(first.HasRouteDistanceMeters);
        Assert.IsTrue(first.HasEstimatedTravelTimeSeconds);
        Assert.Greater(first.routeDistanceMeters, 0f);
        Assert.Greater(first.estimatedTravelTimeSeconds, 0f);
        Assert.AreEqual("estimated_pedestrian_route", first.routeType);
        Assert.IsTrue(first.IsEstimatedPrototypeRoute);
        Assert.IsFalse(first.isOfficialEvacuationRoute);
        Assert.IsTrue(first.HasGeometry);
        Assert.Greater(first.geometry.coordinates.Length, 2);
        Assert.AreEqual("LineString", first.geometry.type);
        Assert.AreEqual(P5CStaticDataLoader.RouteGeometryCoordinateOrder, "longitude, latitude");
        Assert.IsFalse(P5CStaticDataLoader.CanRenderRouteGeometryInCurrentUnityLayout(first));
    }

    [Test]
    public void RouteGeometryParserFailsSafelyForMissingMalformedOrUnsupportedGeometry()
    {
        string json =
            "{" +
            "\"datasetId\":\"malformed_route_fixture\"," +
            "\"coordinateReferenceSystem\":\"EPSG:4326\"," +
            "\"records\":[" +
            "{" +
            "\"routeId\":\"missing_geometry\"," +
            "\"routeAvailability\":\"available\"," +
            "\"routeType\":\"estimated_pedestrian_route\"," +
            "\"isOfficialEvacuationRoute\":false" +
            "}," +
            "{" +
            "\"routeId\":\"unsupported_point_geometry\"," +
            "\"routeAvailability\":\"available\"," +
            "\"routeType\":\"estimated_pedestrian_route\"," +
            "\"isOfficialEvacuationRoute\":false," +
            "\"geometry\":{\"type\":\"Point\",\"coordinates\":[139.76,35.67]}" +
            "}," +
            "{" +
            "\"routeId\":\"malformed_coordinates\"," +
            "\"routeAvailability\":\"available\"," +
            "\"routeType\":\"estimated_pedestrian_route\"," +
            "\"isOfficialEvacuationRoute\":false," +
            "\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[139.76],[139.77,35.68]]}" +
            "}," +
            "{" +
            "\"routeId\":\"swapped_wgs84_order\"," +
            "\"routeAvailability\":\"available\"," +
            "\"routeType\":\"estimated_pedestrian_route\"," +
            "\"isOfficialEvacuationRoute\":false," +
            "\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[35.67,139.76],[35.68,139.77]]}" +
            "}" +
            "]}";

        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.RouteSampleRecord> result =
            P5CStaticDataLoader.LoadRouteSamplesFromJson(json, "inline malformed route fixture");

        Assert.IsTrue(result.success);
        Assert.AreEqual(4, result.records.Length);
        foreach (P5CStaticDataLoader.RouteSampleRecord record in result.records)
        {
            Assert.IsFalse(record.HasGeometry, record.routeId);
            Assert.IsFalse(P5CStaticDataLoader.CanRenderRouteGeometryInCurrentUnityLayout(record));
        }
    }

    [Test]
    public void BuildingQualificationAssetParsesRepresentativeRecords()
    {
        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.BuildingQualificationRecord> result =
            P5CStaticDataLoader.LoadBuildingQualificationsFromPath(
                AssetsDataPath(P5CStaticDataLoader.BuildingQualificationFileName));

        Assert.IsTrue(result.success);
        Assert.AreEqual("p5_b4_real_chuo_building_qualification", result.datasetId);
        Assert.GreaterOrEqual(result.records.Length, 31);

        P5CStaticDataLoader.BuildingQualificationRecord first = result.records[0];
        Assert.IsFalse(string.IsNullOrWhiteSpace(first.shelterId));
        Assert.IsFalse(string.IsNullOrWhiteSpace(first.plateauBuildingId));
        Assert.AreEqual("official_confirmed", first.qualificationStatus);
        Assert.AreEqual("high", first.confidence);
        Assert.IsFalse(first.manualReviewNeeded);
        Assert.Greater(first.warnings.Length, 0);
    }

    [Test]
    public void ShelterBuildingMatchAssetParsesRepresentativeRecords()
    {
        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.ShelterBuildingMatchRecord> result =
            P5CStaticDataLoader.LoadShelterBuildingMatchesFromPath(
                AssetsDataPath(P5CStaticDataLoader.ShelterBuildingMatchesFileName));

        Assert.IsTrue(result.success);
        Assert.AreEqual("p5_b4_real_chuo_shelter_building_matches", result.datasetId);
        Assert.AreEqual("EPSG:6677", result.metricCoordinateReferenceSystem);
        Assert.GreaterOrEqual(result.records.Length, 31);

        P5CStaticDataLoader.ShelterBuildingMatchRecord first = result.records[0];
        Assert.IsFalse(string.IsNullOrWhiteSpace(first.shelterId));
        Assert.IsFalse(string.IsNullOrWhiteSpace(first.plateauBuildingId));
        Assert.AreEqual("contains", first.matchMethod);
        Assert.AreEqual("high", first.confidence);
        Assert.IsFalse(first.manualReviewNeeded);
        Assert.NotNull(first.warnings);
    }

    [Test]
    public void QualificationStatusesParseIncludingFutureCandidateAndNotQualifiedValues()
    {
        string[] statuses =
        {
            "official_confirmed",
            "official_confirmed_with_review",
            "strong_candidate",
            "weak_candidate",
            "unknown",
            "not_qualified"
        };

        string json = BuildIntegratedStatusFixture(statuses);
        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.IntegratedRouteQualificationRecord> result =
            P5CStaticDataLoader.LoadIntegratedRouteQualificationsFromJson(json, "inline P5-C status fixture");

        Assert.IsTrue(result.success);
        Assert.AreEqual(statuses.Length, result.records.Length);

        foreach (string status in statuses)
        {
            Assert.IsTrue(ContainsQualificationStatus(result.records, status), status);
        }

        Assert.AreEqual("official", P5CStaticDataLoader.ClassifyQualificationStatus("official_confirmed"));
        Assert.AreEqual("official", P5CStaticDataLoader.ClassifyQualificationStatus("official_confirmed_with_review"));
        Assert.AreEqual("candidate", P5CStaticDataLoader.ClassifyQualificationStatus("strong_candidate"));
        Assert.AreEqual("candidate", P5CStaticDataLoader.ClassifyQualificationStatus("weak_candidate"));
        Assert.AreEqual("unknown", P5CStaticDataLoader.ClassifyQualificationStatus("unknown"));
        Assert.AreEqual("not qualified", P5CStaticDataLoader.ClassifyQualificationStatus("not_qualified"));
    }

    [Test]
    public void ManualReviewWarningsConfidenceDistanceAndEstimatedTimeAreParsed()
    {
        string json =
            "{" +
            "\"datasetId\":\"manual_review_fixture\"," +
            "\"coordinateReferenceSystem\":\"EPSG:4326\"," +
            "\"osmAttribution\":\"Route network data from OpenStreetMap contributors; use under the Open Database License.\"," +
            "\"records\":[{" +
            "\"qualificationId\":\"q_manual\"," +
            "\"shelterId\":\"p5c_manual_review_shelter\"," +
            "\"shelterName\":\"Manual Review Shelter\"," +
            "\"qualificationStatus\":\"official_confirmed_with_review\"," +
            "\"confidence\":\"medium\"," +
            "\"manualReviewNeeded\":true," +
            "\"warnings\":[\"needs review\",\"route estimate only\"]," +
            "\"routeAvailability\":\"available\"," +
            "\"nearestRouteDistanceMeters\":123.4," +
            "\"estimatedTravelTimeSeconds\":56.7," +
            "\"routeType\":\"estimated_pedestrian_route\"," +
            "\"isOfficialEvacuationRoute\":false" +
            "}]}";

        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.IntegratedRouteQualificationRecord> result =
            P5CStaticDataLoader.LoadIntegratedRouteQualificationsFromJson(json, "inline manual review fixture");

        Assert.IsTrue(result.success);
        P5CStaticDataLoader.IntegratedRouteQualificationRecord record = result.records[0];
        Assert.AreEqual("medium", record.confidence);
        Assert.IsTrue(record.manualReviewNeeded);
        CollectionAssert.Contains(record.warnings, "needs review");
        CollectionAssert.Contains(record.warnings, "route estimate only");
        Assert.AreEqual(123.4f, record.nearestRouteDistanceMeters, 0.001f);
        Assert.AreEqual(56.7f, record.estimatedTravelTimeSeconds, 0.001f);
        Assert.IsTrue(record.IsEstimatedPrototypeRoute);
    }

    [Test]
    public void MissingOrEmptyOptionalP5CFieldsFailSafely()
    {
        string json =
            "{" +
            "\"datasetId\":\"optional_missing_fixture\"," +
            "\"records\":[{" +
            "\"qualificationId\":\"q_optional\"," +
            "\"shelterId\":\"p5c_optional_missing\"," +
            "\"shelterName\":\"Optional Missing\"," +
            "\"qualificationStatus\":\"unknown\"," +
            "\"confidence\":\"low\"," +
            "\"manualReviewNeeded\":false," +
            "\"warnings\":null," +
            "\"nearestRouteDistanceMeters\":null," +
            "\"estimatedTravelTimeSeconds\":null" +
            "}]}";

        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.IntegratedRouteQualificationRecord> result =
            P5CStaticDataLoader.LoadIntegratedRouteQualificationsFromJson(json, "inline optional missing fixture");

        Assert.IsTrue(result.success);
        P5CStaticDataLoader.IntegratedRouteQualificationRecord record = result.records[0];
        Assert.AreEqual("unknown", record.qualificationStatus);
        Assert.AreEqual("low", record.confidence);
        Assert.IsNotNull(record.warnings);
        Assert.AreEqual(0, record.warnings.Length);
        Assert.IsFalse(record.HasNearestRouteDistanceMeters);
        Assert.IsFalse(record.HasEstimatedTravelTimeSeconds);
    }

    [Test]
    public void P5CBundleResolvesOnlySafeShelterAndNearestRouteIds()
    {
        P5CStaticDataLoader.P5CDataBundle bundle = P5CStaticDataLoader.LoadBundleFromAssetsData();

        Assert.IsTrue(bundle.HasAllStaticData);
        P5CStaticDataLoader.IntegratedRouteQualificationRecord first =
            bundle.integratedRouteQualifications.records[0];

        Assert.IsTrue(bundle.TryGetEvidenceForShelter(first.shelterId, out P5CStaticDataLoader.ShelterEvidence evidence));
        Assert.NotNull(evidence.integratedQualification);
        Assert.NotNull(evidence.buildingQualification);
        Assert.NotNull(evidence.shelterBuildingMatch);
        Assert.NotNull(evidence.routeSample);
        Assert.AreEqual(first.nearestRouteId, evidence.routeSample.routeId);
        Assert.AreEqual(first.shelterId, evidence.routeSample.shelterId);

        Assert.IsFalse(bundle.TryGetEvidenceForShelter("test_shelter_001", out P5CStaticDataLoader.ShelterEvidence missingEvidence));
        Assert.IsNull(missingEvidence);
    }

    [Test]
    public void DecisionFeedbackPreservesPrototypeRouteAndAttributionLabels()
    {
        P5CStaticDataLoader.P5CDataBundle bundle = P5CStaticDataLoader.LoadBundleFromAssetsData();
        string shelterId = bundle.integratedRouteQualifications.records[0].shelterId;

        string feedback = P5CDecisionFeedbackFormatter.BuildFromBundle(bundle, shelterId, true);

        StringAssert.Contains("Evidence qualification:", feedback);
        StringAssert.Contains("Manual review:", feedback);
        StringAssert.Contains("Estimated prototype route:", feedback);
        StringAssert.Contains(P5CStaticDataLoader.EstimatedPrototypeRouteLabel, feedback);
        StringAssert.Contains("Not an official evacuation route.", feedback);
        StringAssert.Contains("OSM/ODbL attribution applies.", feedback);
        Assert.LessOrEqual(Regex.Matches(feedback, "Warning:").Count, 2);
    }

    [Test]
    public void DecisionFeedbackUsesUnavailableFallbackWhenShelterMappingIsAbsent()
    {
        P5CStaticDataLoader.P5CDataBundle bundle = P5CStaticDataLoader.LoadBundleFromAssetsData();

        string feedback = P5CDecisionFeedbackFormatter.BuildFromBundle(bundle, "not_a_p5c_shelter", true);

        Assert.AreEqual(P5CStaticDataLoader.UnavailableMessage, feedback);
    }

    [Test]
    public void DataPipelinePathsAreRejectedForUnityRuntimeLoading()
    {
        string processedPath = Path.Combine(
            Application.dataPath,
            "../data_pipeline/processed/qualification/real_chuo_integrated_route_qualification.json");

        LogAssert.Expect(LogType.Warning, new Regex("must use copied Assets/Data JSON"));
        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.IntegratedRouteQualificationRecord> result =
            P5CStaticDataLoader.LoadIntegratedRouteQualificationsFromPath(processedPath);

        Assert.IsFalse(result.success);
        Assert.AreEqual(0, result.records.Length);
    }

    [Test]
    public void P5CLoaderDoesNotBreakDefaultTestMode()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.CreateDefaultConfig();

        ShelterDataSourceResolver.ShelterDataSourceResult result =
            ShelterDataSourceResolver.LoadFromConfig(config);

        Assert.IsTrue(result.success);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, result.sourceMode);
        Assert.GreaterOrEqual(result.testShelters.Length, 5);
        Assert.AreEqual(0, result.realShelters.Length);
        Assert.IsFalse(P5CDecisionFeedbackFormatter.ShouldShowForSourceType("test"));
    }

    private static string BuildIntegratedStatusFixture(string[] statuses)
    {
        string json =
            "{" +
            "\"datasetId\":\"status_fixture\"," +
            "\"coordinateReferenceSystem\":\"EPSG:4326\"," +
            "\"records\":[";

        for (int i = 0; i < statuses.Length; i++)
        {
            if (i > 0)
            {
                json += ",";
            }

            json +=
                "{" +
                $"\"qualificationId\":\"q_{i}\"," +
                $"\"shelterId\":\"status_shelter_{i}\"," +
                $"\"qualificationStatus\":\"{statuses[i]}\"," +
                "\"confidence\":\"high\"," +
                "\"manualReviewNeeded\":false," +
                "\"warnings\":[]," +
                "\"routeType\":\"estimated_pedestrian_route\"," +
                "\"isOfficialEvacuationRoute\":false" +
                "}";
        }

        return json + "]}";
    }

    private static bool ContainsQualificationStatus(
        P5CStaticDataLoader.IntegratedRouteQualificationRecord[] records,
        string status)
    {
        foreach (P5CStaticDataLoader.IntegratedRouteQualificationRecord record in records)
        {
            if (record != null && record.qualificationStatus == status)
            {
                return true;
            }
        }

        return false;
    }

    private static void AssertAssetDataFileExists(string fileName)
    {
        string path = AssetsDataPath(fileName);
        Assert.IsTrue(File.Exists(path), path);
    }

    private static string AssetsDataPath(string fileName)
    {
        return Path.Combine(Application.dataPath, "Data", fileName);
    }
}
