using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class P8HumanitarianHighriseCandidateAuditTests
{
    [Test]
    public void HumanitarianCandidateAuditJsonLoadsAndIsExpandedBeyondSamples()
    {
        P8HumanitarianHighriseCandidateAuditData audit = LoadAudit();

        Assert.AreEqual("p8_humanitarian_highrise_candidate_audit_v1", audit.datasetId);
        Assert.IsNotNull(audit.records);
        Assert.Greater(audit.records.Length, 5);
        Assert.Greater(audit.screeningSummary.plateauIncludedCount, 0);
        Assert.AreEqual(5, audit.screeningSummary.existingSampleIncludedCount);
        Assert.AreEqual(audit.records.Length, audit.screeningSummary.plateauIncludedCount + audit.screeningSummary.existingSampleIncludedCount);
    }

    [Test]
    public void EveryCandidateRemainsNonOfficialAndRequiresWarning()
    {
        P8HumanitarianHighriseCandidateAuditData audit = LoadAudit();

        Assert.IsFalse(audit.isOfficialShelterDataset);
        Assert.IsTrue(audit.nonOfficialWarningRequired);
        Assert.IsTrue(audit.dataOnly);

        foreach (P8HumanitarianHighriseCandidateRecord candidate in audit.records)
        {
            Assert.AreEqual("humanitarian_candidate", candidate.candidateLayer, candidate.candidateId);
            Assert.IsFalse(candidate.isOfficialShelter, candidate.candidateId);
            Assert.IsTrue(candidate.nonOfficialWarningRequired, candidate.candidateId);
            Assert.IsTrue(candidate.manualReviewNeeded, candidate.candidateId);
            Assert.AreEqual("not_official_candidate_layer", candidate.officialDesignationStatus, candidate.candidateId);
            Assert.IsTrue(candidate.hazardStatusEligible, candidate.candidateId);
            Assert.IsTrue(candidate.p8cProxyEligible, candidate.candidateId);
            Assert.IsTrue(candidate.p8dDamageStatusEligible, candidate.candidateId);
            Assert.IsTrue(candidate.p8ePersistentVisibilityReviewNeeded, candidate.candidateId);
            Assert.IsTrue(candidate.p9LifeFirstSelectableReviewNeeded, candidate.candidateId);
            Assert.IsFalse(candidate.affectsGameplaySuccessFailure, candidate.candidateId);
            Assert.IsFalse(candidate.implementsP8D, candidate.candidateId);
            Assert.IsFalse(candidate.implementsP9SelectableGameplay, candidate.candidateId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(candidate.sourceFile), candidate.candidateId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(candidate.evidenceSourceType), candidate.candidateId);

            bool hasName = !string.IsNullOrWhiteSpace(candidate.buildingName);
            bool hasFallbackId = !string.IsNullOrWhiteSpace(candidate.fallbackBuildingId);
            Assert.IsTrue(hasName || hasFallbackId, candidate.candidateId);
        }
    }

    [Test]
    public void AuditContainsPlateauAndSampleEvidenceWithoutOfficialShelterClaim()
    {
        P8HumanitarianHighriseCandidateAuditData audit = LoadAudit();

        Assert.IsTrue(audit.records.Any(record => record.evidenceStatus == "plateau_geometry_attribute_only"));
        Assert.IsTrue(audit.records.Any(record => record.evidenceStatus == "existing_controlled_sample_only"));
        Assert.IsTrue(audit.records.Any(record => string.IsNullOrWhiteSpace(record.buildingName) && !string.IsNullOrWhiteSpace(record.fallbackBuildingId)));
        Assert.IsFalse(audit.records.Any(record => record.officialDesignationStatus == "official_designated"));
        Assert.IsFalse(audit.records.Any(record => record.officialDesignationStatus == "official_designated_with_review"));
    }

    [Test]
    public void CandidateAuditDoesNotCoupleToGameplaySuccessFailureOrP9()
    {
        P8HumanitarianHighriseCandidateAuditData audit = LoadAudit();

        Assert.IsFalse(audit.affectsGameplaySuccessFailure);
        Assert.IsFalse(audit.implementsP8D);
        Assert.IsFalse(audit.implementsP9SelectableGameplay);
        Assert.IsFalse(P8InfrastructureHazardEvaluator.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P8InfrastructureHazardEvaluator.ImplementsCollapseProxy);
        Assert.IsFalse(P8InfrastructureHazardEvaluator.RequiresP9Systems);
    }

    [Test]
    public void HumanitarianCandidateProxyAndHighriseMarkerCanReceiveHazardState()
    {
        P8HazardLayerData layer = new P8HazardLayerData
        {
            scenarioId = "p8_humanitarian_candidate_audit_test_layer",
            sourceMode = "official_tsunami_metropolitan",
            hazardLayerVersion = "p8_test",
            features = new[]
            {
                new P8HazardFeature
                {
                    featureId = "p8_candidate_hazard_test_feature",
                    sourceMode = "official_tsunami_metropolitan",
                    sourceCategory = "official_tsunami_metropolitan",
                    geometryType = "grid",
                    extractionStatus = "extracted",
                    spatialExtractionStatus = "extracted",
                    arrivalTimeSeconds = 100f,
                    inundationDepthMeters = 0.8f,
                    maxInundationDepthMeters = 0.8f,
                    waterLevelMeters = 0.8f,
                    tsunamiHeightMeters = 2f,
                    maxTsunamiHeightMeters = 2f,
                    inundationDepthStatus = "test_depth",
                    boundaryStatus = "test_boundary_not_official_contour",
                    inundationBoundary = new[]
                    {
                        new P8BoundaryPoint { x = -1f, y = -1f },
                        new P8BoundaryPoint { x = 1f, y = -1f },
                        new P8BoundaryPoint { x = 1f, y = 1f },
                        new P8BoundaryPoint { x = -1f, y = 1f }
                    },
                    spatialSamples = new[]
                    {
                        new P8HazardSpatialSample
                        {
                            longitude = 0f,
                            latitude = 0f,
                            inundationDepthMeters = 0.8f,
                            tsunamiHeightMeters = 2f
                        }
                    },
                    spatialSampleCount = 1,
                    hazardIntensity = 0.5f,
                    confidence = 0.9f,
                    evidenceSourceId = "tokyo_damage_estimation_map_tsunami",
                    visualHeightMeters = 100f,
                    visualHeightIsCinematicOnly = true,
                    boundaryIsEvidenceBasedOrPrototype = "evidence_based",
                    affectedInfrastructureTypes = new P8AffectedInfrastructureTypes
                    {
                        humanitarian_candidate_proxy = true,
                        highrise_candidate_marker = true
                    }
                }
            }
        };

        P8InfrastructureHazardEvaluation candidate = P8InfrastructureHazardEvaluator.Evaluate(
            P8InfrastructureHazardEvaluationInput.FromHazardCoordinates("candidate_proxy", P8InfrastructureCategory.HumanitarianCandidateProxy, 0f, 0f, true),
            layer,
            150f);
        P8InfrastructureHazardEvaluation marker = P8InfrastructureHazardEvaluator.Evaluate(
            P8InfrastructureHazardEvaluationInput.FromHazardCoordinates("highrise_marker", P8InfrastructureCategory.HighriseCandidateMarker, 0f, 0f, true),
            layer,
            150f);

        Assert.IsTrue(candidate.success, candidate.summary);
        Assert.IsTrue(marker.success, marker.summary);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, candidate.state);
        Assert.AreEqual(P8InfrastructureHazardState.RestrictedProxy, marker.state);
    }

    private static P8HumanitarianHighriseCandidateAuditData LoadAudit()
    {
        string path = Path.Combine(Application.dataPath, "Data", "P8", "humanitarian_highrise_candidate_audit_v1.json");
        Assert.IsTrue(File.Exists(path), path);
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<P8HumanitarianHighriseCandidateAuditData>(json);
    }

    [Serializable]
    private class P8HumanitarianHighriseCandidateAuditData
    {
        public string datasetId;
        public bool isOfficialShelterDataset;
        public bool nonOfficialWarningRequired;
        public bool dataOnly;
        public bool affectsGameplaySuccessFailure;
        public bool implementsP8D;
        public bool implementsP9SelectableGameplay;
        public P8HumanitarianHighriseCandidateScreeningSummary screeningSummary;
        public P8HumanitarianHighriseCandidateRecord[] records;
    }

    [Serializable]
    private class P8HumanitarianHighriseCandidateScreeningSummary
    {
        public int plateauIncludedCount;
        public int existingSampleIncludedCount;
    }

    [Serializable]
    private class P8HumanitarianHighriseCandidateRecord
    {
        public string candidateId;
        public string candidateLayer;
        public string buildingName;
        public string fallbackBuildingId;
        public string sourceFile;
        public string evidenceSourceType;
        public string evidenceStatus;
        public string officialDesignationStatus;
        public bool manualReviewNeeded;
        public bool isOfficialShelter;
        public bool nonOfficialWarningRequired;
        public bool hazardStatusEligible;
        public bool p8cProxyEligible;
        public bool p8dDamageStatusEligible;
        public bool p8ePersistentVisibilityReviewNeeded;
        public bool p9LifeFirstSelectableReviewNeeded;
        public bool affectsGameplaySuccessFailure;
        public bool implementsP8D;
        public bool implementsP9SelectableGameplay;
    }
}
