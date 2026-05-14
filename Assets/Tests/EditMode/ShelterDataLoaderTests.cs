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
        Assert.AreEqual("Test Shelter", shelter.shelterName);
    }

    [Test]
    public void TestShelter001IsEnterableByDefault()
    {
        bool found = ShelterDataLoader.TryGetShelter("test_shelter_001", out ShelterDataLoader.ShelterData shelter);

        Assert.IsTrue(found);
        Assert.NotNull(shelter);
        Assert.IsTrue(shelter.canEnter);
        Assert.AreEqual(10f, shelter.climbTimeSeconds);
        Assert.AreEqual(0f, shelter.crowdingDelaySeconds);
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
}
