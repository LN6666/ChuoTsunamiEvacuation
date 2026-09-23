using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P8DamageProxyMarkerPlayModeTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [UnityTest]
    public IEnumerator DamageProxyMarkerCanChangeStatusInTemporaryScene()
    {
        P8DamageProxyMarker marker = CreateMarker("P8D_WarningMarker", P8InfrastructureCategory.Building);

        yield return null;

        marker.ApplyDamageState(P8InfrastructureDamageState.Warning);

        Assert.AreEqual(P8InfrastructureDamageState.Warning, marker.CurrentState);
        Assert.IsFalse(P8DamageProxyMarker.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P8DamageProxyMarker.RequiresChuoBaseMap);
        Assert.IsFalse(P8DamageProxyMarker.RequiresP9Systems);
    }

    [UnityTest]
    public IEnumerator CollapseProxyMarkerTiltsWithoutPhysics()
    {
        P8DamageProxyMarker marker = CreateMarker("P8D_CollapseProxyVisualMarker", P8InfrastructureCategory.Building);
        Quaternion before = marker.transform.localRotation;

        yield return null;

        marker.ApplyDamageState(P8InfrastructureDamageState.CollapsedProxyVisual);

        Assert.AreEqual(P8InfrastructureDamageState.CollapsedProxyVisual, marker.CurrentState);
        Assert.AreNotEqual(before, marker.transform.localRotation);
        Assert.IsNull(marker.GetComponent<Rigidbody>());
        Assert.IsFalse(P8DamageProxyMarker.UsesPhysicsCollapse);
        Assert.IsFalse(P8DamageProxyMarker.UsesDebrisSimulation);
    }

    [UnityTest]
    public IEnumerator HumanitarianCandidateMarkerShowsNonOfficialStatus()
    {
        P8DamageProxyMarker marker = CreateMarker("P8D_HumanitarianCandidateMarker", P8InfrastructureCategory.HumanitarianCandidateProxy);
        marker.ConfigureForTests("candidate_marker", P8InfrastructureCategory.HumanitarianCandidateProxy, true, false);

        yield return null;

        marker.ApplyDamageState(P8InfrastructureDamageState.LowFloorInundationWarning);

        Assert.AreEqual(P8InfrastructureDamageState.LowFloorInundationWarning, marker.CurrentState);
        Assert.IsTrue(marker.NonOfficialWarningRequired);
        Assert.IsFalse(marker.IsOfficialShelter);
        Assert.AreEqual(0, marker.GetComponents<BuildingShelter>().Length);
        Assert.AreEqual(0, marker.GetComponents<ShelterEntranceTrigger>().Length);
    }

    [UnityTest]
    public IEnumerator DamageProxyEvaluatesWithoutP9Systems()
    {
        GameObject proxyObject = CreateObject("P8D_DamageProxy_PlayMode");
        P8InfrastructureDamageProxy proxy = proxyObject.AddComponent<P8InfrastructureDamageProxy>();
        proxy.ConfigureForTests("damage_proxy", P8InfrastructureCategory.Entrance, true, true, 0f, 0f);

        yield return null;

        P8InfrastructureDamageEvaluation result = proxy.EvaluateDamage(
            CreateLayer(100f, 0.4f, 0.5f),
            P8InfrastructureDamageConfig.Default(),
            300f,
            new P8InfrastructureDamageEvaluationInput
            {
                targetId = "damage_proxy",
                category = P8InfrastructureCategory.Entrance
            });

        Assert.IsTrue(result.success, result.summary);
        Assert.AreEqual(P8InfrastructureDamageState.EntranceBlockedProxy, result.state);
        Assert.IsFalse(P8InfrastructureDamageProxy.AffectsGameplaySuccessFailureInP8D);
        Assert.IsFalse(P8InfrastructureDamageProxy.RequiresP9SystemsInP8D);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            if (createdObjects[i] != null)
            {
                Object.Destroy(createdObjects[i]);
            }
        }

        createdObjects.Clear();
        yield return null;
    }

    private P8DamageProxyMarker CreateMarker(string name, P8InfrastructureCategory category)
    {
        P8DamageProxyMarker marker = P8DamageProxyMarker.CreateCubeMarkerForTests(name, category);
        createdObjects.Add(marker.gameObject);
        return marker;
    }

    private GameObject CreateObject(string name)
    {
        var gameObject = new GameObject(name);
        createdObjects.Add(gameObject);
        return gameObject;
    }

    private static P8HazardLayerData CreateLayer(float arrivalTimeSeconds, float depthMeters, float hazardIntensity)
    {
        return new P8HazardLayerData
        {
            scenarioId = "p8d_playmode_test_layer",
            sourceMode = "official_tsunami_metropolitan",
            hazardLayerVersion = "p8d_playmode.1",
            timeOriginSeconds = 0f,
            features = new[]
            {
                new P8HazardFeature
                {
                    featureId = "p8d_playmode_test_feature",
                    sourceMode = "official_tsunami_metropolitan",
                    sourceCategory = "official_tsunami_metropolitan",
                    geometryType = "grid",
                    extractionStatus = "extracted",
                    spatialExtractionStatus = "extracted",
                    arrivalTimeSeconds = arrivalTimeSeconds,
                    inundationDepthMeters = depthMeters,
                    maxInundationDepthMeters = depthMeters,
                    waterLevelMeters = depthMeters,
                    tsunamiHeightMeters = 2f,
                    maxTsunamiHeightMeters = 2f,
                    inundationDepthStatus = "extracted_spatial_test",
                    boundaryStatus = "prototype_test_boundary",
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
                            xMeters = 0f,
                            yMeters = 0f,
                            longitude = 0f,
                            latitude = 0f,
                            inundationDepthMeters = depthMeters,
                            tsunamiHeightMeters = 2f
                        }
                    },
                    spatialSampleCount = 1,
                    hazardIntensity = hazardIntensity,
                    confidence = 0.9f,
                    evidenceSourceId = "tokyo_damage_estimation_map_tsunami",
                    visualHeightMeters = 1000f,
                    visualHeightIsCinematicOnly = true,
                    boundaryIsEvidenceBasedOrPrototype = "evidence_based",
                    affectedInfrastructureTypes = new P8AffectedInfrastructureTypes
                    {
                        entrances = true
                    },
                    buildingDamageState = "none",
                    collapseProxyState = "data_only",
                    collapseProbability = 0f,
                    collapseRandomSeed = 1,
                    hazardDrivenCollapse = false
                }
            }
        };
    }
}
