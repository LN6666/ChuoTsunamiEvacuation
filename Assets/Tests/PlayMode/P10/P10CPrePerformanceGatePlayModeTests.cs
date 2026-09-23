using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P10CPrePerformanceGatePlayModeTests
{
    [UnityTest]
    public IEnumerator RuntimeGateRecordsBoundedFrameSummary()
    {
        GameObject root = new GameObject("P10CPre_RuntimeGate_Test");
        try
        {
            P10CPreRuntimePerformanceGate gate = root.AddComponent<P10CPreRuntimePerformanceGate>();
            gate.Configure(P10CPreDataLoader.LoadPerformanceGateConfig().data);
            gate.BeginGateRun("p10c_pre_playmode");
            gate.RecordFrame(0.016f);
            gate.RecordFrame(0.020f);
            gate.RecordFrame(0.024f);
            gate.SetRuntimeState("Low", "clear_day", false, false, 12, 30, 20);

            yield return null;

            P10CPreBuiltPlayerProfileSummary summary = gate.FinishGateRun(0, 0);
            Assert.IsTrue(summary.profileRunSucceeded);
            Assert.AreEqual("pass_for_p10c", summary.readinessClassification);
            Assert.AreEqual(12, summary.metrics.npcCount);
            Assert.AreEqual(20, summary.metrics.greenFrameCount);
            Assert.Greater(summary.metrics.averageFps, 30f);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator QualityProfileApplierAppliesLowProfileCaps()
    {
        GameObject root = new GameObject("P10CPre_QualityProfileApplier_Test");
        GameObject markerLayer = new GameObject("MarkerLayer");
        GameObject crowdLayer = new GameObject("CrowdLayer");
        GameObject lightCurtain = new GameObject("LightCurtain");
        GameObject debugLayer = new GameObject("DebugLayer");
        try
        {
            markerLayer.transform.SetParent(root.transform, false);
            crowdLayer.transform.SetParent(root.transform, false);
            lightCurtain.transform.SetParent(root.transform, false);
            debugLayer.transform.SetParent(root.transform, false);
            P10CPreQualityProfileApplier applier = root.AddComponent<P10CPreQualityProfileApplier>();
            SetPrivateField(applier, "profiles", P10CPreDataLoader.LoadQualityProfiles().data);
            SetPrivateField(applier, "markerLayerRoot", markerLayer);
            SetPrivateField(applier, "crowdLayerRoot", crowdLayer);
            SetPrivateField(applier, "lightCurtainRoot", lightCurtain);
            SetPrivateField(applier, "debugLayerRoot", debugLayer);

            P10CPreQualityProfileApplyResult result = applier.ApplyProfile("Low");
            yield return null;

            Assert.IsTrue(result.applied);
            Assert.IsTrue(markerLayer.activeSelf);
            Assert.IsTrue(crowdLayer.activeSelf);
            Assert.IsFalse(lightCurtain.activeSelf);
            Assert.IsFalse(debugLayer.activeSelf);
            Assert.AreEqual(40, result.npcCap);
            Assert.AreEqual(90, result.greenFrameCap);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    [UnityTest]
    public IEnumerator GreenFrameRuntimeSkipsRedundantSameStateRefresh()
    {
        GameObject root = new GameObject("P10CPre_GreenFrame_NoRedundantRefresh_Test");
        try
        {
            P10BGreenGroundFrameRuntime runtime = root.AddComponent<P10BGreenGroundFrameRuntime>();
            P10BGreenGroundFrameConfig config = CreateFrameConfig();
            runtime.Configure(config, CreateFrameTargets());
            runtime.SetTsunamiStarted(true);
            P10BGreenGroundFrameMetrics firstMetrics = runtime.LastMetrics;

            yield return null;

            runtime.SetTsunamiStarted(true);
            Assert.AreSame(firstMetrics, runtime.LastMetrics);
            Assert.AreEqual(1, runtime.LastMetrics.activeFrameCount);
        }
        finally
        {
            Object.Destroy(root);
        }
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);
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
            warmupFrameCount = 1,
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
            }
        };
    }
}
