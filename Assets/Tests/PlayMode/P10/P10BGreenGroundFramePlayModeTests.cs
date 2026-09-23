using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P10BGreenGroundFramePlayModeTests
{
    [UnityTest]
    public IEnumerator FramesRemainHiddenBeforeTsunamiStart()
    {
        GameObject root = new GameObject("P10B_FrameRuntime_HiddenBeforeStart");
        try
        {
            P10BGreenGroundFrameRuntime runtime = root.AddComponent<P10BGreenGroundFrameRuntime>();
            runtime.Configure(CreateConfig(), CreateTargets());
            runtime.SetTsunamiStarted(false);

            yield return null;

            Assert.IsTrue(runtime.LastMetrics.hiddenBeforeTsunamiStart);
            Assert.AreEqual(0, runtime.LastMetrics.activeFrameCount);
            Assert.AreEqual(0, runtime.LastMetrics.generatedFrameCount);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator FramesAppearAfterTsunamiStartForOfficialAndNonOfficialTargets()
    {
        GameObject root = new GameObject("P10B_FrameRuntime_AfterStart");
        try
        {
            P10BGreenGroundFrameRuntime runtime = root.AddComponent<P10BGreenGroundFrameRuntime>();
            runtime.Configure(CreateConfig(), CreateTargets());
            runtime.SetTsunamiStarted(true);

            yield return null;

            Assert.AreEqual(2, runtime.LastMetrics.activeFrameCount);
            Assert.AreEqual(1, runtime.LastMetrics.officialShelterFrameCount);
            Assert.AreEqual(1, runtime.LastMetrics.humanitarianCandidateFrameCount);
            Assert.IsTrue(runtime.LastMetrics.allHumanitarianWarningsPreserved);
            Assert.IsTrue(runtime.LastMetrics.allHumanitarianCandidatesRemainNonOfficial);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator StableTargetsReusePooledFrameObjects()
    {
        GameObject root = new GameObject("P10B_FrameRuntime_Pooling");
        try
        {
            P10BGreenGroundFrameRuntime runtime = root.AddComponent<P10BGreenGroundFrameRuntime>();
            runtime.Configure(CreateConfig(), CreateTargets());
            runtime.SetTsunamiStarted(true);
            yield return null;

            int createdAfterFirstStart = runtime.CreatedPoolObjectCount;
            runtime.SetTsunamiStarted(true);
            yield return null;

            Assert.AreEqual(createdAfterFirstStart, runtime.CreatedPoolObjectCount);
            Assert.AreEqual(2, runtime.LastMetrics.activeFrameCount);
            Assert.IsTrue(runtime.LastMetrics.noPerFrameObjectCreationRequired);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator DisablingFeatureHidesGeneratedFrames()
    {
        GameObject root = new GameObject("P10B_FrameRuntime_Disabled");
        try
        {
            P10BGreenGroundFrameConfig config = CreateConfig();
            config.featureEnabled = false;
            P10BGreenGroundFrameRuntime runtime = root.AddComponent<P10BGreenGroundFrameRuntime>();
            runtime.Configure(config, CreateTargets());
            runtime.SetTsunamiStarted(true);

            yield return null;

            Assert.AreEqual(0, runtime.LastMetrics.activeFrameCount);
            Assert.IsFalse(runtime.LastMetrics.featureEnabled);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator CoordinateProxyFallbackFrameCarriesWarningMetadata()
    {
        GameObject root = new GameObject("P10B_FrameRuntime_Metadata");
        try
        {
            P10BGreenGroundFrameRuntime runtime = root.AddComponent<P10BGreenGroundFrameRuntime>();
            runtime.Configure(CreateConfig(), CreateTargets());
            runtime.SetTsunamiStarted(true);

            yield return null;

            P10BGreenGroundFrameMarker[] markers = root.GetComponentsInChildren<P10BGreenGroundFrameMarker>();
            Assert.AreEqual(2, markers.Length);
            P10BGreenGroundFrameMarker candidate = null;
            for (int i = 0; i < markers.Length; i++)
            {
                if (markers[i].IsHumanitarianCandidate)
                {
                    candidate = markers[i];
                    break;
                }
            }

            Assert.IsNotNull(candidate);
            Assert.IsTrue(candidate.CoordinateDerivedProxy);
            Assert.IsFalse(candidate.ExactFootprintProven);
            Assert.IsFalse(candidate.IsOfficialShelter);
            Assert.IsTrue(candidate.NonOfficialWarningRequired);
            Assert.IsTrue(candidate.PreservesHumanitarianSemantics);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    private static P10BGreenGroundFrameConfig CreateConfig()
    {
        return new P10BGreenGroundFrameConfig
        {
            featureEnabled = true,
            tsunamiStartTriggered = true,
            debugPreviewBeforeTsunamiStart = false,
            maxFrameCount = 10,
            defaultProxyFootprintWidthMeters = 20f,
            defaultProxyFootprintDepthMeters = 16f,
            officialShelterProxyWidthMeters = 24f,
            officialShelterProxyDepthMeters = 18f,
            groundYOffsetMeters = 0.05f
        };
    }

    private static P10BGreenGroundFrameTarget[] CreateTargets()
    {
        return new[]
        {
            new P10BGreenGroundFrameTarget
            {
                targetId = "official_test_shelter",
                targetType = "official_evacuation_building",
                isOfficialShelter = true,
                hasCoordinate = true,
                latitude = 35.68f,
                longitude = 139.77f,
                proxyCenter = new Vector3(4f, 0f, 6f),
                footprintWidthMeters = 24f,
                footprintDepthMeters = 18f,
                exactFootprintProven = false,
                coordinateDerivedProxy = true
            },
            new P10BGreenGroundFrameTarget
            {
                targetId = "non_official_candidate",
                targetType = "non_official_humanitarian_vertical_candidate",
                isHumanitarianCandidate = true,
                nonOfficialWarningRequired = true,
                safeApprovedByDefault = false,
                hasCoordinate = true,
                latitude = 35.666f,
                longitude = 139.778f,
                proxyCenter = new Vector3(12f, 0f, 14f),
                footprintWidthMeters = 20f,
                footprintDepthMeters = 16f,
                exactFootprintProven = false,
                coordinateDerivedProxy = true,
                warningText = P9CVerticalEvacuationTargetDecision.NonOfficialWarningText
            }
        };
    }
}
