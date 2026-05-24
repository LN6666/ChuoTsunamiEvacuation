using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P10BPlusPlusOptimizationPlayModeTests
{
    [UnityTest]
    public IEnumerator GreenFrameRuntimeWarmsPoolBeforeTsunamiStart()
    {
        GameObject root = new GameObject("P10BPlusPlus_GreenFrameWarmup_Test");
        try
        {
            P10BGreenGroundFrameRuntime runtime = root.AddComponent<P10BGreenGroundFrameRuntime>();
            P10BGreenGroundFrameConfig config = CreateFrameConfig();
            config.warmupFrameCount = 2;
            runtime.Configure(config, CreateFrameTargets());

            yield return null;

            Assert.AreEqual(2, runtime.WarmedPoolObjectCount);
            Assert.AreEqual(2, runtime.CreatedPoolObjectCount);
            Assert.AreEqual(0, runtime.LastMetrics.activeFrameCount);
            runtime.SetTsunamiStarted(true);
            yield return null;

            Assert.AreEqual(2, runtime.LastMetrics.activeFrameCount);
            Assert.AreEqual(2, runtime.CreatedPoolObjectCount);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator RuntimeOptimizerDisablesDebugLayerByDefault()
    {
        GameObject root = new GameObject("P10BPlusPlus_RuntimeOptimizer_Test");
        GameObject debugLayer = new GameObject("DebugLayer");
        GameObject markerLayer = new GameObject("MarkerLayer");
        try
        {
            debugLayer.transform.SetParent(root.transform, false);
            markerLayer.transform.SetParent(root.transform, false);
            P10BPlusPlusRuntimeOptimizer optimizer = root.AddComponent<P10BPlusPlusRuntimeOptimizer>();
            SetPrivateLayerFieldsForTest(optimizer, markerLayer, debugLayer);

            P10BPlusPlusRuntimeOptimizerState state = optimizer.ApplyOptimizationConfig(
                P10BPlusPlusDataLoader.LoadOptimizationConfig().data);
            yield return null;

            Assert.IsFalse(debugLayer.activeSelf);
            Assert.IsTrue(markerLayer.activeSelf);
            Assert.IsFalse(state.debugLayerActive);
            Assert.AreEqual(120, state.npcCap);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator FrameSpikeDetectorSummarizesRuntimeState()
    {
        GameObject root = new GameObject("P10BPlusPlus_FrameSpikeDetector_Test");
        try
        {
            P10BPlusPlusFrameSpikeDetector detector = root.AddComponent<P10BPlusPlusFrameSpikeDetector>();
            detector.Configure(P10BPlusPlusDataLoader.LoadOptimizationConfig().data);
            detector.BeginRun("p10b_plus_plus_unit");
            detector.RecordFrame(0.016f);
            detector.RecordFrame(0.060f);
            detector.SetRuntimeState("Medium", "night_rain", "ja", true, true, false, true, 24, 111, 64);

            yield return null;

            P10BPlusPlusRuntimeMetricsSummary summary = detector.FinishRun();
            Assert.AreEqual("p10b_plus_plus_unit", summary.scenarioId);
            Assert.AreEqual("night_rain", summary.weatherMode);
            Assert.AreEqual(1, summary.frameTimeSummary.frameSpikeCountOverThreshold);
            Assert.AreEqual(24, summary.npcCount);
            Assert.IsTrue(summary.sprintActive);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    private static void SetPrivateLayerFieldsForTest(
        P10BPlusPlusRuntimeOptimizer optimizer,
        GameObject markerLayer,
        GameObject debugLayer)
    {
        typeof(P10BPlusPlusRuntimeOptimizer)
            .GetField("markerLayerRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(optimizer, markerLayer);
        typeof(P10BPlusPlusRuntimeOptimizer)
            .GetField("debugLayerRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(optimizer, debugLayer);
    }

    private static P10BGreenGroundFrameConfig CreateFrameConfig()
    {
        return new P10BGreenGroundFrameConfig
        {
            featureEnabled = true,
            tsunamiStartTriggered = true,
            debugPreviewBeforeTsunamiStart = false,
            warmupPoolBeforeTsunamiStart = true,
            maxFrameCount = 10,
            defaultProxyFootprintWidthMeters = 20f,
            defaultProxyFootprintDepthMeters = 16f,
            officialShelterProxyWidthMeters = 24f,
            officialShelterProxyDepthMeters = 18f,
            groundYOffsetMeters = 0.05f
        };
    }

    private static P10BGreenGroundFrameTarget[] CreateFrameTargets()
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
                footprintDepthMeters = 18f
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
                footprintDepthMeters = 16f
            }
        };
    }
}
