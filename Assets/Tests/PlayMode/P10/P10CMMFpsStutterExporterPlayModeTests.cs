using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P10CMMFpsStutterExporterPlayModeTests
{
    [UnityTest]
    public IEnumerator ExporterCapturesRuntimeStateAndFrameSummary()
    {
        GameObject root = new GameObject("P10CMM_FpsExporter_Test");
        try
        {
            P10CMMFpsStutterExporter exporter = root.AddComponent<P10CMMFpsStutterExporter>();
            exporter.Configure(
                "playmode_sample",
                "Low",
                "night_rain",
                string.Empty,
                10f,
                120,
                50f);
            exporter.BeginCapture();
            exporter.RecordFrameForTest(0.016f);
            exporter.RecordFrameForTest(0.040f);
            exporter.RecordFrameForTest(0.060f);

            yield return null;

            P10CMMFpsStutterSummary summary = exporter.FinishCaptureForTest(1f);

            Assert.AreEqual("test_finished", summary.status);
            Assert.AreEqual("playmode_sample", summary.scenarioLabel);
            Assert.GreaterOrEqual(summary.sampleCount, 3);
            Assert.AreEqual("Low", summary.runtimeState.activeQualityProfile);
            Assert.AreEqual("night_rain", summary.runtimeState.activeWeatherNightMode);
            Assert.Greater(summary.averageFps, 0f);
            Assert.AreEqual(1, summary.frameSpikeCountOver50Ms);
        }
        finally
        {
            Object.Destroy(root);
        }
    }
}
