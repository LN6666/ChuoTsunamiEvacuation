using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RealShelterDataLoaderTests
{
    [Test]
    public void SourceModeConfigDefaultRemainsTest()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.CreateDefaultConfig();

        Assert.NotNull(config);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, config.sourceMode);
        Assert.AreEqual("test_shelters.json", config.testSheltersPath);
        Assert.AreEqual("real_chuo_shelters_sample.json", config.realSamplePath);
        Assert.IsTrue(config.fallbackToTestOnError);
    }

    [Test]
    public void TestSourceModeLoadsExistingTestShelters()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.CreateDefaultConfig();

        ShelterDataSourceResolver.ShelterDataSourceResult result =
            ShelterDataSourceResolver.LoadFromConfig(config);

        Assert.IsTrue(result.success);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, result.sourceMode);
        Assert.GreaterOrEqual(result.testShelters.Length, 5);
        Assert.AreEqual(0, result.realShelters.Length);
        Assert.IsTrue(ShelterDataLoader.TryGetShelter("test_shelter_001", out ShelterDataLoader.ShelterData shelter));
        Assert.AreEqual("test", shelter.sourceType);
    }

    [Test]
    public void RealSampleFixtureParsesP3ReleaseShape()
    {
        string json =
            "{" +
            "\"dataset_id\":\"fixture_dataset\"," +
            "\"generated_at\":\"2026-05-17T00:00:00Z\"," +
            "\"source_manifest\":\"fixture_manifest.json\"," +
            "\"coordinate_reference_system\":\"EPSG:4326\"," +
            "\"records\":[{" +
            "\"id\":\"fixture_real_001\"," +
            "\"name\":\"Fixture Real Shelter\"," +
            "\"type\":\"tsunami_evacuation_building\"," +
            "\"latitude\":35.66," +
            "\"longitude\":139.78," +
            "\"address\":\"Fixture address\"," +
            "\"capacity\":null," +
            "\"floors_available\":4," +
            "\"elevation_m\":null," +
            "\"source\":\"Fixture source\"," +
            "\"source_url\":null," +
            "\"source_updated_at\":\"2026-05-17\"," +
            "\"notes\":\"Fixture notes\"," +
            "\"disasterTypes\":[\"tsunami\",\"earthquake\"]," +
            "\"unityPosition\":{\"x\":1,\"y\":2,\"z\":3}," +
            "\"unity\":{\"prefab_hint\":\"ShelterMarker\",\"is_entry_enabled\":true,\"estimated_stair_floors\":4}" +
            "}]}";

        RealShelterDataLoader.RealShelterLoadResult result =
            RealShelterDataLoader.LoadFromJson(json, "inline P3 fixture");

        Assert.IsTrue(result.success);
        Assert.AreEqual("fixture_dataset", result.datasetId);
        Assert.AreEqual("EPSG:4326", result.coordinateSystem);
        Assert.AreEqual(1, result.shelters.Length);

        RealShelterDataLoader.RealShelterRecord shelter = result.shelters[0];
        Assert.AreEqual("fixture_real_001", shelter.shelterId);
        Assert.AreEqual("Fixture Real Shelter", shelter.shelterName);
        Assert.AreEqual("real_sample", shelter.sourceType);
        Assert.AreEqual("tsunami_evacuation_building", shelter.facilityType);
        Assert.AreEqual("Fixture address", shelter.address);
        Assert.AreEqual(35.66f, shelter.latitude, 0.0001f);
        Assert.AreEqual(139.78f, shelter.longitude, 0.0001f);
        Assert.AreEqual("EPSG:4326", shelter.coordinateSystem);
        Assert.IsTrue(shelter.HasUnityPosition);
        Assert.AreEqual(1f, shelter.unityPosition.x);
        Assert.AreEqual(2f, shelter.unityPosition.y);
        Assert.AreEqual(3f, shelter.unityPosition.z);
        CollectionAssert.Contains(shelter.disasterTypes, "tsunami");
        CollectionAssert.Contains(shelter.disasterTypes, "earthquake");
        Assert.AreEqual(4, shelter.safeFloor);
        Assert.AreEqual(0, shelter.capacity);
        Assert.IsFalse(shelter.hasCapacity);
        Assert.IsTrue(shelter.canEnter);
        Assert.AreEqual(12f, shelter.climbTimeSeconds);
        Assert.AreEqual("Fixture source", shelter.dataSource);
        Assert.AreEqual(string.Empty, shelter.sourceUrl);
        Assert.AreEqual("2026-05-17", shelter.sourceUpdatedAt);
        Assert.AreEqual("Fixture notes", shelter.notes);
        Assert.AreEqual("ShelterMarker", shelter.prefabHint);
    }

    [Test]
    public void RealSampleAssetLoadsCopiedP3ReleaseData()
    {
        RealShelterDataLoader.RealShelterLoadResult result = RealShelterDataLoader.LoadRealSample();

        Assert.IsTrue(result.success);
        Assert.AreEqual("real_chuo_shelters_sample", result.datasetId);
        Assert.AreEqual("EPSG:4326", result.coordinateSystem);
        Assert.AreEqual(5, result.shelters.Length);

        RealShelterDataLoader.RealShelterRecord first = FindShelter(result.shelters, "sample_chuo_harumi_001");
        Assert.NotNull(first);
        Assert.AreEqual("Harumi Sample Evacuation Building", first.shelterName);
        Assert.AreEqual("tsunami_evacuation_building", first.facilityType);
        Assert.AreEqual("real_sample", first.sourceType);
        Assert.AreEqual("Harumi area, Chuo City, Tokyo (synthetic sample)", first.address);
        Assert.AreEqual(35.6565f, first.latitude, 0.0001f);
        Assert.AreEqual(139.7805f, first.longitude, 0.0001f);
        Assert.AreEqual(150, first.capacity);
        Assert.IsTrue(first.hasCapacity);
        Assert.AreEqual(5, first.safeFloor);
        Assert.IsTrue(first.canEnter);
        Assert.AreEqual("P3-00 synthetic sample", first.dataSource);
        Assert.AreEqual("2026-05-15", first.sourceUpdatedAt);
        Assert.AreEqual("ShelterMarker", first.prefabHint);
    }

    [Test]
    public void NullableP3FieldsDoNotCrashRealSampleLoader()
    {
        RealShelterDataLoader.RealShelterLoadResult result = RealShelterDataLoader.LoadRealSample();

        Assert.IsTrue(result.success);
        RealShelterDataLoader.RealShelterRecord shelter = FindShelter(result.shelters, "sample_chuo_ginza_005");

        Assert.NotNull(shelter);
        Assert.AreEqual(0, shelter.capacity);
        Assert.IsFalse(shelter.hasCapacity);
        Assert.AreEqual(string.Empty, shelter.sourceUrl);
        Assert.IsFalse(shelter.canEnter);
        Assert.AreEqual(2, shelter.safeFloor);
        Assert.IsNotNull(shelter.disasterTypes);
    }

    [Test]
    public void RealSampleSourceModeLoadsCopiedAssetsDataFile()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.CreateDefaultConfig();
        config.sourceMode = ShelterSourceConfigLoader.RealSampleSourceMode;
        config.realSamplePath = "real_chuo_shelters_sample.json";

        ShelterDataSourceResolver.ShelterDataSourceResult result =
            ShelterDataSourceResolver.LoadFromConfig(config);

        Assert.IsTrue(result.success);
        Assert.AreEqual(ShelterSourceConfigLoader.RealSampleSourceMode, result.sourceMode);
        Assert.IsFalse(result.usedFallbackToTest);
        Assert.AreEqual(0, result.testShelters.Length);
        Assert.AreEqual(5, result.realShelters.Length);
    }

    [Test]
    public void MissingRealSampleFileFallsBackToTestShelters()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.CreateDefaultConfig();
        config.sourceMode = ShelterSourceConfigLoader.RealSampleSourceMode;
        config.realSamplePath = Path.Combine(Path.GetTempPath(), $"missing_real_sample_{Guid.NewGuid():N}.json");

        LogAssert.Expect(LogType.Warning, new Regex("Missing real shelter data"));
        LogAssert.Expect(LogType.Warning, new Regex("Falling back to test shelter data"));

        ShelterDataSourceResolver.ShelterDataSourceResult result =
            ShelterDataSourceResolver.LoadFromConfig(config);

        Assert.IsTrue(result.success);
        Assert.IsTrue(result.usedFallbackToTest);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, result.sourceMode);
        Assert.GreaterOrEqual(result.testShelters.Length, 5);
    }

    [Test]
    public void DataPipelinePathIsRejectedForUnityRuntimeLoading()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.CreateDefaultConfig();
        config.sourceMode = ShelterSourceConfigLoader.RealSampleSourceMode;
        config.realSamplePath = "data_pipeline/processed/release/real_chuo_shelters_sample.json";

        LogAssert.Expect(LogType.Warning, new Regex("must use Assets/Data copies"));
        LogAssert.Expect(LogType.Warning, new Regex("Real shelter sample path was invalid"));
        LogAssert.Expect(LogType.Warning, new Regex("Falling back to test shelter data"));

        ShelterDataSourceResolver.ShelterDataSourceResult result =
            ShelterDataSourceResolver.LoadFromConfig(config);

        Assert.IsTrue(result.success);
        Assert.IsTrue(result.usedFallbackToTest);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, result.sourceMode);
        Assert.GreaterOrEqual(result.testShelters.Length, 5);
    }

    private static RealShelterDataLoader.RealShelterRecord FindShelter(
        RealShelterDataLoader.RealShelterRecord[] shelters,
        string shelterId)
    {
        foreach (RealShelterDataLoader.RealShelterRecord shelter in shelters)
        {
            if (shelter != null && shelter.shelterId == shelterId)
            {
                return shelter;
            }
        }

        return null;
    }
}
