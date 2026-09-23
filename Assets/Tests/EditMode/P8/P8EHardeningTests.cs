using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class P8EHardeningTests
{
    [Test]
    public void SemanticBindingDataLoadsWithRequiredCategories()
    {
        P8ESemanticBindingData data = LoadJson<P8ESemanticBindingData>("p8e_semantic_binding_v1.json");

        Assert.AreEqual("p8e_semantic_binding_v1", data.datasetId);
        Assert.IsFalse(data.completeSceneSemanticBinding);
        Assert.IsFalse(data.sceneMutationPerformed);
        Assert.AreEqual(10, data.bindings.Length);

        foreach (string category in new[]
        {
            "road",
            "building",
            "bridge",
            "underground",
            "entrance",
            "humanitarian_candidate_proxy",
            "highrise_candidate_marker"
        })
        {
            Assert.IsTrue(data.bindings.Any(binding => binding.category == category), category);
        }

        Assert.IsTrue(data.bindings.All(binding => IsAllowedBindingMode(binding.bindingMode)));
        Assert.IsTrue(data.bindings.All(binding => !string.IsNullOrWhiteSpace(binding.p9Limitations)));
        Assert.IsTrue(data.bindings.Where(binding => binding.category == "underground" || binding.category == "entrance")
            .All(binding => !string.IsNullOrWhiteSpace(binding.blocker)));
    }

    [Test]
    public void HumanitarianPersistentMarkerDataKeepsCandidatesNonOfficial()
    {
        P8HumanitarianCandidateMarkerDataSet data = P8HumanitarianCandidateMarkerDataSet.LoadFromFile(
            P8DataPath("humanitarian_candidate_persistent_marker_v1.json"));

        Assert.AreEqual("p8e_humanitarian_candidate_persistent_marker_v1", data.datasetId);
        Assert.AreEqual(110, data.totalCandidates);
        Assert.AreEqual(28, data.namedMarkerCount);
        Assert.AreEqual(82, data.idOnlyMarkerCount);
        Assert.IsFalse(data.persistentSceneObjectsCreated);
        Assert.IsFalse(data.selectableGameplayEnabled);
        Assert.IsFalse(data.affectsGameplaySuccessFailure);
        Assert.IsTrue(data.allCandidatesNonOfficial);
        Assert.IsTrue(data.allCandidatesRequireNonOfficialWarning);
        Assert.AreEqual(110, data.records.Length);

        Assert.IsTrue(data.records.All(P8HumanitarianCandidateMarkerStatus.IsNonOfficialAndWarningRequired));
        Assert.IsTrue(data.records.All(record => !record.selectableGameplayEnabled));
        Assert.IsTrue(data.records.All(record => !record.affectsGameplaySuccessFailure));
        Assert.IsTrue(data.records.All(record => P8HumanitarianCandidateMarkerStatus.IsAllowedMarkerMode(record.markerMode)));
        Assert.IsTrue(data.records.All(record => record.manualReviewNeeded));
    }

    [Test]
    public void RiskFrontProgressionUsesArrivalTimeBeforeFallbackSpeed()
    {
        var config = P8RiskFrontProgressionConfig.Default();
        config.baseOnshoreSpeedMetersPerSecond = 100f;
        var input = new P8RiskFrontProgressionInput
        {
            simulationTimeSeconds = 50f,
            timeOriginSeconds = 0f,
            arrivalTimeSeconds = 100f,
            hasArrivalTime = true,
            fallbackDistanceMeters = 10f,
            inundationDepthMeters = 1f,
            hazardIntensity = 0.5f,
            visualHeightMeters = 1000f,
            maxTsunamiHeightMeters = 99f
        };

        P8RiskFrontProgressionResult result = P8RiskFrontProgressionModel.Evaluate(input, config);

        Assert.IsTrue(result.success, result.summary);
        Assert.IsTrue(result.usedArrivalTime);
        Assert.IsFalse(result.usedFallbackSpeed);
        Assert.AreEqual(0.5f, result.progress01, 0.001f);
        Assert.IsFalse(result.usesVisualHeightAsPhysicalDepth);
        Assert.IsFalse(result.usesMaxTsunamiHeightAsDepth);
    }

    [Test]
    public void ShallowDepthReducesFallbackSpeedWithoutUsingVisualHeight()
    {
        var config = P8RiskFrontProgressionConfig.Default();
        var shallow = new P8RiskFrontProgressionInput
        {
            simulationTimeSeconds = 1f,
            hasArrivalTime = false,
            fallbackDistanceMeters = 100f,
            inundationDepthMeters = 0.05f,
            hazardIntensity = 0.1f,
            visualHeightMeters = 2000f,
            maxTsunamiHeightMeters = 50f
        };
        var deeper = new P8RiskFrontProgressionInput
        {
            simulationTimeSeconds = 1f,
            hasArrivalTime = false,
            fallbackDistanceMeters = 100f,
            inundationDepthMeters = 1f,
            hazardIntensity = 0.1f,
            visualHeightMeters = 0f,
            maxTsunamiHeightMeters = 0f
        };

        P8RiskFrontProgressionResult shallowResult = P8RiskFrontProgressionModel.Evaluate(shallow, config);
        P8RiskFrontProgressionResult deeperResult = P8RiskFrontProgressionModel.Evaluate(deeper, config);

        Assert.IsTrue(shallowResult.usedFallbackSpeed);
        Assert.IsTrue(shallowResult.shallowSlowdownApplied);
        Assert.Less(shallowResult.selectedSpeedMetersPerSecond, deeperResult.selectedSpeedMetersPerSecond);
        Assert.IsFalse(shallowResult.usesVisualHeightAsPhysicalDepth);
        Assert.IsFalse(shallowResult.usesMaxTsunamiHeightAsDepth);
    }

    [Test]
    public void VisualHeightDoesNotChangePhysicalProgression()
    {
        var config = P8RiskFrontProgressionConfig.Default();
        var lowVisual = new P8RiskFrontProgressionInput
        {
            simulationTimeSeconds = 20f,
            fallbackDistanceMeters = 100f,
            inundationDepthMeters = 0.2f,
            hazardIntensity = 0.4f,
            visualHeightMeters = 10f,
            maxTsunamiHeightMeters = 1f
        };
        var highVisual = new P8RiskFrontProgressionInput
        {
            simulationTimeSeconds = lowVisual.simulationTimeSeconds,
            fallbackDistanceMeters = lowVisual.fallbackDistanceMeters,
            inundationDepthMeters = lowVisual.inundationDepthMeters,
            hazardIntensity = lowVisual.hazardIntensity,
            visualHeightMeters = 10000f,
            maxTsunamiHeightMeters = 200f
        };

        P8RiskFrontProgressionResult low = P8RiskFrontProgressionModel.Evaluate(lowVisual, config);
        P8RiskFrontProgressionResult high = P8RiskFrontProgressionModel.Evaluate(highVisual, config);

        Assert.AreEqual(low.progress01, high.progress01, 0.001f);
        Assert.AreEqual(low.selectedSpeedMetersPerSecond, high.selectedSpeedMetersPerSecond, 0.001f);
        Assert.AreEqual(low.visualRiskStrength01, high.visualRiskStrength01, 0.001f);
    }

    [Test]
    public void P2P6MatrixUsesAllowedStatusesOnly()
    {
        P8EP2P6Matrix matrix = LoadJson<P8EP2P6Matrix>("p8e_p2_p6_new_map_adaptation_matrix.json");

        Assert.AreEqual("p8e_p2_p6_new_map_adaptation_matrix_v1", matrix.datasetId);
        Assert.IsFalse(matrix.successFailureRulesChangedInP8);
        Assert.IsFalse(matrix.implementsP9Gameplay);
        Assert.GreaterOrEqual(matrix.items.Length, 12);
        Assert.IsTrue(matrix.items.All(item => IsAllowedMatrixStatus(item.status)));
        Assert.IsTrue(matrix.items.All(item => !string.IsNullOrWhiteSpace(item.evidence)));
        Assert.IsTrue(matrix.items.All(item => !string.IsNullOrWhiteSpace(item.p9Boundary)));
    }

    [Test]
    public void RouteHandoffForbidsOfficialRouteClaimWhenNotValidated()
    {
        P8ERouteCandidateGeometryHandoff handoff = LoadJson<P8ERouteCandidateGeometryHandoff>(
            "p8e_route_candidate_geometry_handoff.json");

        Assert.AreEqual("p8e_route_candidate_geometry_handoff_v1", handoff.datasetId);
        Assert.IsFalse(handoff.routeCoordinateTransformGeometryValidated);
        Assert.IsTrue(handoff.wgs84PlateauTransformLimitationRemains);
        Assert.IsFalse(handoff.routesAreOfficial);
        Assert.IsTrue(handoff.routesAreEstimatedPrototypeGuidance);
        Assert.IsFalse(handoff.routesAlignedToNewMapRoads);
        Assert.IsFalse(handoff.routeRoadGeometryValidated);
        StringAssert.Contains("Official evacuation route", handoff.p9MustNotClaim);
    }

    private static T LoadJson<T>(string fileName)
    {
        string path = P8DataPath(fileName);
        Assert.IsTrue(File.Exists(path), path);
        return JsonUtility.FromJson<T>(File.ReadAllText(path));
    }

    private static string P8DataPath(string fileName)
    {
        return Path.Combine(Application.dataPath, "Data", "P8", fileName);
    }

    private static bool IsAllowedBindingMode(string mode)
    {
        return mode == "actual_scene_object" ||
               mode == "plateau_metadata" ||
               mode == "proxy_marker" ||
               mode == "data_only" ||
               mode == "blocked";
    }

    private static bool IsAllowedMatrixStatus(string status)
    {
        return status == "passed" ||
               status == "proxy_ready" ||
               status == "blocked" ||
               status == "p9_final_gameplay_required";
    }

    [Serializable]
    private class P8ESemanticBindingData
    {
        public string datasetId;
        public bool completeSceneSemanticBinding;
        public bool sceneMutationPerformed;
        public P8ESemanticBinding[] bindings;
    }

    [Serializable]
    private class P8ESemanticBinding
    {
        public string category;
        public string bindingMode;
        public string confidence;
        public bool isProxy;
        public bool p9Usable;
        public string p9Limitations;
        public string notes;
        public string blocker;
    }

    [Serializable]
    private class P8EP2P6Matrix
    {
        public string datasetId;
        public bool successFailureRulesChangedInP8;
        public bool implementsP9Gameplay;
        public P8EP2P6MatrixItem[] items;
    }

    [Serializable]
    private class P8EP2P6MatrixItem
    {
        public string system;
        public string item;
        public string status;
        public string evidence;
        public string p9Boundary;
    }

    [Serializable]
    private class P8ERouteCandidateGeometryHandoff
    {
        public string datasetId;
        public bool routeCoordinateTransformGeometryValidated;
        public bool wgs84PlateauTransformLimitationRemains;
        public bool routesAreOfficial;
        public bool routesAreEstimatedPrototypeGuidance;
        public bool routesAlignedToNewMapRoads;
        public bool routeRoadGeometryValidated;
        public string p9MustNotClaim;
    }
}
