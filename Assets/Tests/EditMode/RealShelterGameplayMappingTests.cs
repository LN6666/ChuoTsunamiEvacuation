using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class RealShelterGameplayMappingTests
{
    [SetUp]
    public void ResetShelterLoader()
    {
        ShelterDataLoader.Reload();
        ShelterDataLoader.ClearRuntimeShelters();
    }

    [TearDown]
    public void CleanupShelterLoader()
    {
        ShelterDataLoader.Reload();
    }

    [Test]
    public void DefaultTestSourceModeStillUsesExistingTestShelters()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.CreateDefaultConfig();

        ShelterDataSourceResolver.ShelterDataSourceResult result =
            ShelterDataSourceResolver.LoadFromConfig(config);

        Assert.IsTrue(result.success);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, result.sourceMode);
        Assert.GreaterOrEqual(result.testShelters.Length, 5);
        Assert.AreEqual(0, result.realShelters.Length);
    }

    [Test]
    public void CommittedShelterSourceConfigAssetRemainsTestMode()
    {
        string configPath = Path.Combine(Application.dataPath, "Data/shelter_source_config.json");

        Assert.IsTrue(File.Exists(configPath), configPath);
        string json = File.ReadAllText(configPath);
        ShelterSourceConfigLoader.ShelterSourceConfig config =
            ShelterSourceConfigLoader.LoadFromPath(configPath);

        StringAssert.Contains("\"sourceMode\": \"test\"", json);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, config.sourceMode);
        Assert.AreEqual("real_chuo_shelters_sample.json", config.realSamplePath);
    }

    [Test]
    public void RealSampleSourceModeProvidesMarkerReadyShelters()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.CreateDefaultConfig();
        config.sourceMode = ShelterSourceConfigLoader.RealSampleSourceMode;

        ShelterDataSourceResolver.ShelterDataSourceResult result =
            ShelterDataSourceResolver.LoadFromConfig(config);
        ShelterDataLoader.ShelterData[] mappedShelters =
            ShelterGameplayDataMapper.MapRealSheltersToGameplayData(result.realShelters);

        Assert.IsTrue(result.success);
        Assert.AreEqual(ShelterSourceConfigLoader.RealSampleSourceMode, result.sourceMode);
        Assert.AreEqual(5, mappedShelters.Length);

        bool hasEnterable = false;
        bool hasBlocked = false;

        foreach (ShelterDataLoader.ShelterData shelter in mappedShelters)
        {
            Assert.NotNull(shelter.layoutPosition, shelter.shelterId);
            Assert.AreEqual(ShelterSourceConfigLoader.RealSampleSourceMode, shelter.sourceType);
            Assert.IsFalse(string.IsNullOrWhiteSpace(shelter.shelterName));
            Assert.IsFalse(string.IsNullOrWhiteSpace(shelter.facilityType));
            Assert.IsFalse(string.IsNullOrWhiteSpace(shelter.address));
            Assert.GreaterOrEqual(shelter.safeFloor, 0);
            Assert.GreaterOrEqual(shelter.capacity, 0);
            hasEnterable |= shelter.canEnter;
            hasBlocked |= !shelter.canEnter;
        }

        Assert.IsTrue(hasEnterable);
        Assert.IsTrue(hasBlocked);
    }

    [Test]
    public void RealSampleAssetMapsToMultipleMarkerReadySheltersWithMetadata()
    {
        RealShelterDataLoader.RealShelterLoadResult loadResult = RealShelterDataLoader.LoadRealSample();
        ShelterDataLoader.ShelterData[] mappedShelters =
            ShelterGameplayDataMapper.MapRealSheltersToGameplayData(loadResult.shelters);

        Assert.IsTrue(loadResult.success);
        Assert.Greater(mappedShelters.Length, 1);

        bool hasName = false;
        bool hasAddress = false;
        bool hasSource = false;
        bool hasCapacity = false;
        bool hasSafeFloor = false;

        foreach (ShelterDataLoader.ShelterData shelter in mappedShelters)
        {
            hasName |= !string.IsNullOrWhiteSpace(shelter.shelterName);
            hasAddress |= !string.IsNullOrWhiteSpace(shelter.address);
            hasSource |= !string.IsNullOrWhiteSpace(shelter.dataSource);
            hasCapacity |= shelter.capacity > 0;
            hasSafeFloor |= shelter.safeFloor > 0;
        }

        Assert.IsTrue(hasName);
        Assert.IsTrue(hasAddress);
        Assert.IsTrue(hasSource);
        Assert.IsTrue(hasCapacity);
        Assert.IsTrue(hasSafeFloor);
    }

    [Test]
    public void DeterministicLayoutFallbackWorksWhenUnityPositionIsAbsent()
    {
        var realShelter = new RealShelterDataLoader.RealShelterRecord
        {
            shelterId = "fallback_layout_real",
            shelterName = "Fallback Layout Real Shelter",
            sourceType = ShelterSourceConfigLoader.RealSampleSourceMode
        };

        ShelterDataLoader.LayoutPosition first =
            ShelterGameplayDataMapper.ResolveLayoutPosition(realShelter, 4);
        ShelterDataLoader.LayoutPosition second =
            ShelterGameplayDataMapper.ResolveLayoutPosition(realShelter, 4);

        Assert.AreEqual(first.x, second.x);
        Assert.AreEqual(first.y, second.y);
        Assert.AreEqual(first.z, second.z);
        Assert.AreEqual(16f, first.x);
        Assert.AreEqual(0f, first.y);
        Assert.AreEqual(-4f, first.z);
    }

    [Test]
    public void RealSampleFallbackPositionsAreDeterministicAndSeparated()
    {
        RealShelterDataLoader.RealShelterLoadResult loadResult = RealShelterDataLoader.LoadRealSample();
        ShelterDataLoader.ShelterData[] firstPass =
            ShelterGameplayDataMapper.MapRealSheltersToGameplayData(loadResult.shelters);
        ShelterDataLoader.ShelterData[] secondPass =
            ShelterGameplayDataMapper.MapRealSheltersToGameplayData(loadResult.shelters);
        var seenPositions = new HashSet<string>();

        Assert.AreEqual(firstPass.Length, secondPass.Length);

        for (int i = 0; i < firstPass.Length; i++)
        {
            Vector3 first = firstPass[i].layoutPosition.ToVector3();
            Vector3 second = secondPass[i].layoutPosition.ToVector3();

            Assert.AreEqual(first, second, firstPass[i].shelterId);
            Assert.IsTrue(seenPositions.Add($"{first.x:0.###},{first.y:0.###},{first.z:0.###}"), firstPass[i].shelterId);

            for (int j = i + 1; j < firstPass.Length; j++)
            {
                Vector3 other = firstPass[j].layoutPosition.ToVector3();
                Assert.GreaterOrEqual(Vector3.Distance(first, other), 8f, $"{firstPass[i].shelterId} vs {firstPass[j].shelterId}");
            }
        }
    }

    [Test]
    public void UnityPositionOverridesFallbackLayout()
    {
        var realShelter = new RealShelterDataLoader.RealShelterRecord
        {
            shelterId = "unity_position_real",
            shelterName = "Unity Position Real Shelter",
            unityPosition = new RealShelterDataLoader.UnityPosition
            {
                x = 1.5f,
                y = 2.5f,
                z = 3.5f
            }
        };

        ShelterDataLoader.LayoutPosition layout =
            ShelterGameplayDataMapper.ResolveLayoutPosition(realShelter, 9);

        Assert.AreEqual(1.5f, layout.x);
        Assert.AreEqual(2.5f, layout.y);
        Assert.AreEqual(3.5f, layout.z);
    }

    [Test]
    public void MetadataFieldsArePreservedForDebugMarkerDisplay()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.CreateDefaultConfig();
        config.sourceMode = ShelterSourceConfigLoader.RealSampleSourceMode;

        ShelterDataSourceResolver.ShelterDataSourceResult result =
            ShelterDataSourceResolver.LoadFromConfig(config);
        ShelterDataLoader.ShelterData[] mappedShelters =
            ShelterGameplayDataMapper.MapRealSheltersToGameplayData(result.realShelters);

        ShelterDataLoader.ShelterData first = mappedShelters[0];
        string label = ShelterDebugMetadataFormatter.BuildMarkerLabel(first);

        StringAssert.Contains("Harumi Sample Evacuation Building", label);
        StringAssert.Contains("tsunami_evacuation_building", label);
        StringAssert.Contains("Harumi area, Chuo City, Tokyo", label);
        StringAssert.Contains("Capacity: 150", label);
        StringAssert.Contains("Safe floor estimate: 5", label);
        StringAssert.Contains("Updated: 2026-05-15", label);
    }

    [Test]
    public void MetadataFormatterHandlesMissingOptionalFields()
    {
        var shelter = new ShelterDataLoader.ShelterData
        {
            shelterId = "metadata_optional_missing",
            shelterName = "Metadata Optional Missing",
            sourceType = ShelterSourceConfigLoader.RealSampleSourceMode,
            facilityType = string.Empty,
            address = string.Empty,
            capacity = 0,
            safeFloor = 0,
            notes = string.Empty
        };

        string label = ShelterDebugMetadataFormatter.BuildMarkerLabel(shelter);

        StringAssert.Contains("Metadata Optional Missing", label);
        StringAssert.Contains("Source: real_sample", label);
        StringAssert.Contains("Capacity: unknown", label);
        Assert.IsFalse(label.Contains("Address:"));
        Assert.IsFalse(label.Contains("Safe floor estimate:"));
        Assert.AreEqual(string.Empty, ShelterDebugMetadataFormatter.BuildMarkerLabel(null));
    }

    [Test]
    public void RuntimeRegisteredRealShelterCanUseExistingShelterLookup()
    {
        var realShelter = new RealShelterDataLoader.RealShelterRecord
        {
            shelterId = "runtime_real_lookup",
            shelterName = "Runtime Real Lookup Shelter",
            sourceType = ShelterSourceConfigLoader.RealSampleSourceMode,
            facilityType = "tsunami_evacuation_building",
            address = "Runtime address",
            canEnter = true,
            safeFloor = 4,
            capacity = 60
        };

        ShelterDataLoader.ShelterData[] mappedShelters =
            ShelterGameplayDataMapper.MapRealSheltersToGameplayData(new[] { realShelter });

        ShelterDataLoader.RegisterRuntimeShelters(mappedShelters, "test runtime real shelter");

        Assert.IsTrue(ShelterDataLoader.TryGetShelter("runtime_real_lookup", out ShelterDataLoader.ShelterData loaded));
        Assert.AreEqual("Runtime Real Lookup Shelter", loaded.shelterName);
        Assert.AreEqual(ShelterSourceConfigLoader.RealSampleSourceMode, loaded.sourceType);
        Assert.AreEqual("Runtime address", loaded.address);
        Assert.AreEqual(4, loaded.safeFloor);
    }
}
