using NUnit.Framework;
using UnityEngine;

public class TsunamiHazardFixtureLoaderTests
{
    [Test]
    public void HazardFixtureAssetLoadsP3ReleaseShape()
    {
        TsunamiHazardFixtureLoader.HazardFixtureLoadResult result =
            TsunamiHazardFixtureLoader.LoadHazardFixture();

        Assert.IsTrue(result.success);
        Assert.AreEqual("sample_tsunami_hazard_zones", result.datasetId);
        Assert.AreEqual("EPSG:4326", result.coordinateSystem);
        Assert.AreEqual("P3-02 Synthetic Tsunami Hazard Fixture", result.sourceName);
        Assert.AreEqual("synthetic_sample", result.officialStatus);
        Assert.AreEqual(3, result.zones.Length);

        TsunamiHazardFixtureLoader.HazardZoneRecord lowZone = result.zones[0];
        Assert.AreEqual("sample_hazard_harumi_low", lowZone.zoneId);
        Assert.AreEqual("Sample Harumi Low Hazard Zone", lowZone.zoneName);
        Assert.AreEqual("tsunami", lowZone.hazardFamily);
        Assert.AreEqual(1, lowZone.hazardLevel);
        Assert.AreEqual("polygon", lowZone.geometryType);
        Assert.AreEqual("Polygon", lowZone.geometryDataType);
        Assert.IsTrue(lowZone.hasInundationDepth);
        Assert.AreEqual(0.5f, lowZone.inundationDepthMeters, 0.0001f);
        Assert.IsTrue(lowZone.hasTsunamiHeight);
        Assert.AreEqual(1.2f, lowZone.tsunamiHeightMeters, 0.0001f);
        Assert.IsFalse(lowZone.isOfficialPrimary);

        TsunamiHazardFixtureLoader.HazardZoneRecord unknownDepthZone = result.zones[2];
        Assert.AreEqual("sample_hazard_tsukishima_unknown_depth", unknownDepthZone.zoneId);
        Assert.IsFalse(unknownDepthZone.hasInundationDepth);
        Assert.IsFalse(unknownDepthZone.hasTsunamiHeight);
    }

    [Test]
    public void HazardFixtureDebugShapesMapWithoutSceneDependency()
    {
        TsunamiHazardFixtureLoader.HazardFixtureLoadResult result =
            TsunamiHazardFixtureLoader.LoadHazardFixture();

        TsunamiHazardDebugLayout.HazardDebugShape[] shapes =
            TsunamiHazardDebugLayout.CreateShapes(result.zones);

        Assert.AreEqual(3, shapes.Length);
        Assert.AreEqual("sample_hazard_harumi_low", shapes[0].zoneId);
        Assert.AreEqual(new Vector3(-22f, 0.08f, 20f), shapes[0].localPosition);
        Assert.Greater(shapes[0].localScale.x, 0f);
        Assert.Greater(shapes[0].localScale.z, 0f);
        StringAssert.Contains("Sample Harumi Low Hazard Zone", shapes[0].label);
        StringAssert.Contains("Level: 1", shapes[0].label);
        StringAssert.Contains("Source: P3-02 Synthetic Tsunami Hazard Fixture", shapes[0].label);
        StringAssert.Contains("Status: synthetic_sample", shapes[0].label);
    }

    [Test]
    public void HazardDebugLayerHasNoGameplayRuleEffect()
    {
        TsunamiHazardFixtureLoader.HazardFixtureLoadResult result =
            TsunamiHazardFixtureLoader.LoadHazardFixture();
        TsunamiHazardDebugLayout.HazardDebugShape[] shapes =
            TsunamiHazardDebugLayout.CreateShapes(result.zones);

        Assert.IsFalse(TsunamiHazardDebugVisualizer.AffectsGameplayRules);

        foreach (TsunamiHazardDebugLayout.HazardDebugShape shape in shapes)
        {
            Assert.IsFalse(shape.affectsGameplayRules, shape.zoneId);
        }
    }
}
