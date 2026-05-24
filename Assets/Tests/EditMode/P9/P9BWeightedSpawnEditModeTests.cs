using System.Linq;
using NUnit.Framework;

public class P9BWeightedSpawnEditModeTests
{
    [Test]
    public void WeightedSpawnSampleLoads()
    {
        P9BLoadResult<P9BWeightedSpawnConfig> config = P9BDataLoader.LoadWeightedSpawnConfig();
        P9BLoadResult<P9BWeightedSpawnZoneCollection> zones = P9BDataLoader.LoadSpawnZones();

        Assert.IsTrue(config.success, config.summary);
        Assert.IsTrue(zones.success, zones.summary);
        Assert.AreEqual("p9b_new_map_runtime_prototype", config.data.scenarioPresetId);
        Assert.GreaterOrEqual(zones.data.zones.Length, 6);
        Assert.IsFalse(P9WeightedSpawnSelector.AffectsGameplaySuccessFailure);
    }

    [Test]
    public void WeightedSpawnSelectionIsDeterministicWithSeed()
    {
        P9BWeightedSpawnConfig config = P9BDataLoader.LoadWeightedSpawnConfig().data;
        P9BWeightedSpawnZoneCollection zones = P9BDataLoader.LoadSpawnZones().data;

        P9BWeightedSpawnSelectionResult first = P9WeightedSpawnSelector.Select(zones, config, 12);
        P9BWeightedSpawnSelectionResult second = P9WeightedSpawnSelector.Select(zones, config, 12);

        Assert.AreEqual(first.selectedSpawnCount, second.selectedSpawnCount);
        for (int i = 0; i < first.selectedSpawns.Length; i++)
        {
            Assert.AreEqual(first.selectedSpawns[i].zoneId, second.selectedSpawns[i].zoneId);
            Assert.AreEqual(first.selectedSpawns[i].position.x, second.selectedSpawns[i].position.x, 0.0001f);
            Assert.AreEqual(first.selectedSpawns[i].position.z, second.selectedSpawns[i].position.z, 0.0001f);
        }
    }

    [Test]
    public void CoastalLowElevationHighRiskZoneReceivesHigherWeight()
    {
        P9BWeightedSpawnConfig config = P9BDataLoader.LoadWeightedSpawnConfig().data;
        P9BWeightedSpawnZoneCollection zones = P9BDataLoader.LoadSpawnZones().data;
        P9BWeightedSpawnZoneRecord waterfront = zones.zones.Single(zone => zone.zoneId == "p9b_harumi_waterfront_high_risk");
        P9BWeightedSpawnZoneRecord office = zones.zones.Single(zone => zone.zoneId == "p9b_ginza_office_dense");

        float waterfrontWeight = P9WeightedSpawnSelector.ComputeWeight(waterfront, config);
        float officeWeight = P9WeightedSpawnSelector.ComputeWeight(office, config);

        Assert.Greater(waterfrontWeight, officeWeight);
    }

    [Test]
    public void SpawnCapAndExclusionRulesAreRespected()
    {
        P9BWeightedSpawnConfig config = P9BDataLoader.LoadWeightedSpawnConfig().data;
        P9BWeightedSpawnZoneCollection zones = P9BDataLoader.LoadSpawnZones().data;

        P9BWeightedSpawnSelectionResult result = P9WeightedSpawnSelector.Select(zones, config, 100);

        Assert.LessOrEqual(result.selectedSpawnCount, config.spawnCountCap);
        Assert.IsFalse(result.selectedSpawns.Any(spawn => spawn.zoneId == "p9b_blocked_construction_proxy"));
        Assert.IsFalse(result.selectedSpawns.Any(spawn => spawn.zoneId == "p9b_already_flooded_pier_proxy"));
        Assert.IsFalse(result.selectedSpawns.Any(spawn => spawn.zoneId == "p9b_zero_position_unsafe_proxy"));

        P9BWeightedSpawnCandidateExplanation zero = result.candidateExplanations
            .Single(explanation => explanation.zoneId == "p9b_zero_position_unsafe_proxy");
        Assert.IsFalse(zero.included);
        Assert.AreEqual("excluded_invalid_position", zero.weightExplanation);
    }
}
