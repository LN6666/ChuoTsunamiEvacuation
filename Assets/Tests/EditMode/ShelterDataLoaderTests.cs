using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ShelterDataLoaderTests
{
    [SetUp]
    public void ReloadShelterData()
    {
        ShelterDataLoader.Reload();
    }

    [Test]
    public void TestShelterDataCanBeLoaded()
    {
        bool found = ShelterDataLoader.TryGetShelter("test_shelter_001", out ShelterDataLoader.ShelterData shelter);

        Assert.IsTrue(found);
        Assert.NotNull(shelter);
        Assert.AreEqual("test_shelter_001", shelter.shelterId);
        Assert.AreEqual("Near Official Shelter", shelter.shelterName);
    }

    [Test]
    public void TestShelter001IsEnterableByDefault()
    {
        bool found = ShelterDataLoader.TryGetShelter("test_shelter_001", out ShelterDataLoader.ShelterData shelter);

        Assert.IsTrue(found);
        Assert.NotNull(shelter);
        Assert.IsTrue(shelter.canEnter);
        Assert.AreEqual(11f, shelter.climbTimeSeconds);
        Assert.AreEqual(1f, shelter.crowdingDelaySeconds);
    }

    [Test]
    public void TestShelterDataContainsMultipleDecisionOptions()
    {
        ShelterDataLoader.ShelterData[] shelters = ShelterDataLoader.GetAllShelters();

        Assert.GreaterOrEqual(shelters.Length, 5);
        Assert.IsTrue(ContainsShelter("test_shelter_001", shelters));
        Assert.IsTrue(ContainsShelter("test_shelter_far_fast", shelters));
        Assert.IsTrue(ContainsShelter("test_shelter_crowded_candidate", shelters));
        Assert.IsTrue(ContainsShelter("test_shelter_slow_safe", shelters));
        Assert.IsTrue(ContainsShelter("test_shelter_blocked", shelters));
    }

    [Test]
    public void ShelterIdsAreUnique()
    {
        ShelterDataLoader.ShelterData[] shelters = ShelterDataLoader.GetAllShelters();
        var seenIds = new HashSet<string>();

        foreach (ShelterDataLoader.ShelterData shelter in shelters)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(shelter.shelterId));
            Assert.IsTrue(seenIds.Add(shelter.shelterId), $"Duplicate shelterId: {shelter.shelterId}");
        }
    }

    [Test]
    public void ShelterDataIncludesRequiredDecisionVariety()
    {
        ShelterDataLoader.ShelterData[] shelters = ShelterDataLoader.GetAllShelters();

        bool hasEnterable = false;
        bool hasBlocked = false;
        bool hasOfficial = false;
        bool hasCrowding = false;

        foreach (ShelterDataLoader.ShelterData shelter in shelters)
        {
            hasEnterable |= shelter.canEnter;
            hasBlocked |= !shelter.canEnter;
            hasOfficial |= shelter.isOfficialShelter;
            hasCrowding |= shelter.crowdingDelaySeconds > 0f;
        }

        Assert.IsTrue(hasEnterable);
        Assert.IsTrue(hasBlocked);
        Assert.IsTrue(hasOfficial);
        Assert.IsTrue(hasCrowding);
    }

    [Test]
    public void ShelterLayoutPositionsExistForDebugPlatformGeneration()
    {
        ShelterDataLoader.ShelterData[] shelters = ShelterDataLoader.GetAllShelters();

        foreach (ShelterDataLoader.ShelterData shelter in shelters)
        {
            Assert.NotNull(shelter.layoutPosition, shelter.shelterId);
            Assert.IsFalse(float.IsNaN(shelter.layoutPosition.x), shelter.shelterId);
            Assert.IsFalse(float.IsNaN(shelter.layoutPosition.y), shelter.shelterId);
            Assert.IsFalse(float.IsNaN(shelter.layoutPosition.z), shelter.shelterId);
        }
    }

    [Test]
    public void RealDataReadyOptionalFieldsDoNotBreakLoader()
    {
        Assert.IsTrue(ShelterDataLoader.TryGetShelter("test_shelter_001", out ShelterDataLoader.ShelterData shelter));

        Assert.AreEqual("test", shelter.sourceType);
        Assert.AreEqual("debug_shelter", shelter.facilityType);
        Assert.AreEqual("debug_platform", shelter.coordinateSystem);
        Assert.IsNotNull(shelter.realFacilityName);
        Assert.IsNotNull(shelter.address);
        Assert.IsNotNull(shelter.plateauBuildingId);
        Assert.IsNotNull(shelter.dataSource);
        Assert.GreaterOrEqual(shelter.safeFloor, 0);
        Assert.GreaterOrEqual(shelter.capacity, 0);
    }

    [Test]
    public void UnknownShelterIdReturnsNullAndPreservesSceneValues()
    {
        LogAssert.Expect(LogType.Warning, new Regex("Shelter ID 'unknown_shelter'.*Existing scene/Inspector shelter values will be preserved"));

        bool found = ShelterDataLoader.TryGetShelter("unknown_shelter", out ShelterDataLoader.ShelterData shelter);

        Assert.IsFalse(found);
        Assert.IsNull(shelter);
    }

    [Test]
    public void GetShelterOrDefaultDoesNotCreateDestructiveFallbackForUnknownId()
    {
        LogAssert.Expect(LogType.Warning, new Regex("Shelter ID 'missing_for_default'.*Existing scene/Inspector shelter values will be preserved"));

        ShelterDataLoader.ShelterData shelter = ShelterDataLoader.GetShelterOrDefault("missing_for_default");

        Assert.IsNull(shelter);
    }

    private static bool ContainsShelter(string shelterId, ShelterDataLoader.ShelterData[] shelters)
    {
        foreach (ShelterDataLoader.ShelterData shelter in shelters)
        {
            if (shelter != null && shelter.shelterId == shelterId)
            {
                return true;
            }
        }

        return false;
    }
}
