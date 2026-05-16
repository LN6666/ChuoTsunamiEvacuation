using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
    public void HazardFixtureLoaderUsesAssetsDataCopy()
    {
        TsunamiHazardFixtureLoader.HazardFixtureLoadResult result =
            TsunamiHazardFixtureLoader.LoadHazardFixture();
        string normalizedSourcePath = result.sourcePath.Replace('\\', '/');

        Assert.IsTrue(result.success);
        StringAssert.Contains("Assets/Data", normalizedSourcePath);
        Assert.IsFalse(normalizedSourcePath.Contains("data_pipeline/"));
    }

    [Test]
    public void HazardFixtureRejectsDataPipelineRuntimePath()
    {
        string dataPipelinePath = Path.Combine(
            "data_pipeline",
            "processed",
            "release",
            "sample_tsunami_hazard_zones.json");

        LogAssert.Expect(LogType.Warning, new Regex("must use Assets/Data copies"));

        TsunamiHazardFixtureLoader.HazardFixtureLoadResult result =
            TsunamiHazardFixtureLoader.LoadFromPath(dataPipelinePath);

        Assert.IsFalse(result.success);
        Assert.AreEqual(0, result.zones.Length);
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
        StringAssert.Contains("Depth: 0.5m", shapes[0].label);
        Assert.IsFalse(shapes[0].label.Contains("Source:"));
        Assert.IsFalse(shapes[0].label.Contains("Status:"));
        Assert.IsFalse(shapes[0].label.Contains("Notes:"));
    }

    [Test]
    public void HazardDebugShapesAreSchematicUniqueAndMetadataRich()
    {
        TsunamiHazardFixtureLoader.HazardFixtureLoadResult result =
            TsunamiHazardFixtureLoader.LoadHazardFixture();
        TsunamiHazardDebugLayout.HazardDebugShape[] shapes =
            TsunamiHazardDebugLayout.CreateShapes(result.zones);

        Assert.AreEqual(result.zones.Length, shapes.Length);

        for (int i = 0; i < shapes.Length; i++)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(shapes[i].zoneId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(shapes[i].label));
            Assert.Greater(shapes[i].localScale.x, 0f);
            Assert.Greater(shapes[i].localScale.z, 0f);
            Assert.GreaterOrEqual(shapes[i].color.a, 0.3f);
            StringAssert.Contains(result.zones[i].zoneName, shapes[i].label);
            StringAssert.Contains($"Level: {result.zones[i].hazardLevel}", shapes[i].label);
            if (!string.IsNullOrWhiteSpace(result.zones[i].notes))
            {
                Assert.IsFalse(shapes[i].label.Contains(result.zones[i].notes));
            }

            for (int j = i + 1; j < shapes.Length; j++)
            {
                Assert.GreaterOrEqual(
                    Vector3.Distance(shapes[i].localPosition, shapes[j].localPosition),
                    10f,
                    $"{shapes[i].zoneId} vs {shapes[j].zoneId}");
            }
        }
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
