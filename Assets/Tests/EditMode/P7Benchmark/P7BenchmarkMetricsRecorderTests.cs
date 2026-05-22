using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class P7BenchmarkMetricsRecorderTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [Test]
    public void MetricsSummaryCanBeGenerated()
    {
        P7BenchmarkMetricsRecorder recorder = CreateRecorder();

        recorder.RecordFrameTime(0.016f);
        recorder.RecordFrameTime(0.02f);
        recorder.RecordFrameTime(0.025f);

        string summary = recorder.ExportSummaryString();

        Assert.That(summary, Does.Contain("P7BenchmarkMetricsSummary"));
        Assert.That(summary, Does.Contain("stage=P7-B Wave 2-A"));
        Assert.That(summary, Does.Contain("samples=3"));
        Assert.Greater(recorder.AverageFps, 0f);
        Assert.Greater(recorder.ApproximateOnePercentLowFps, 0f);
        Assert.Greater(recorder.ElapsedSeconds, 0f);
    }

    [Test]
    public void OnePercentLowCalculationHandlesEmptyAndSmallSamplesSafely()
    {
        Assert.AreEqual(0f, P7BenchmarkMetricsRecorder.CalculateApproximateOnePercentLowFps(null));
        Assert.AreEqual(0f, P7BenchmarkMetricsRecorder.CalculateApproximateOnePercentLowFps(new float[0]));

        float singleSampleLow = P7BenchmarkMetricsRecorder.CalculateApproximateOnePercentLowFps(new[] { 0.02f });
        float smallSampleLow = P7BenchmarkMetricsRecorder.CalculateApproximateOnePercentLowFps(new[] { 0.01f, 0.02f, 0.04f });

        Assert.AreEqual(50f, singleSampleLow, 0.001f);
        Assert.AreEqual(25f, smallSampleLow, 0.001f);
    }

    [Test]
    public void RecorderDoesNotDependOnGameplayManagers()
    {
        FieldInfo[] fields = typeof(P7BenchmarkMetricsRecorder).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.IsFalse(fields.Any(field => field.FieldType.Name.Contains("EvacuationGameManager")));
        Assert.IsFalse(fields.Any(field => field.FieldType.Name.Contains("Shelter")));
        Assert.IsFalse(fields.Any(field => field.FieldType.Name.Contains("Tsunami")));

        P7BenchmarkMetricsRecorder recorder = CreateRecorder();
        recorder.RecordFrameTime(0.016f);

        Assert.AreEqual(1, recorder.SampleCount);
        Assert.IsNull(recorder.GetComponent("EvacuationGameManager"));
    }

    [Test]
    public void MarkerConstantsAndLabelsAreStable()
    {
        GameObject gameObject = CreateObject("P7BenchmarkMarker_EditModeTest");
        P7BenchmarkMarker marker = gameObject.AddComponent<P7BenchmarkMarker>();

        Assert.AreEqual("P7-B Wave 2-A", P7BenchmarkMarker.BenchmarkStage);
        Assert.AreEqual("P7 Benchmark Skeleton", P7BenchmarkMarker.BenchmarkLabel);
        Assert.AreEqual("P7_Benchmark_Skeleton", P7BenchmarkMarker.BenchmarkSceneName);
        Assert.AreEqual("Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity", P7BenchmarkMarker.BenchmarkScenePath);
        Assert.AreEqual("P7BenchmarkRoot", P7BenchmarkMarker.BenchmarkRootName);
        Assert.AreEqual("isolated_skeleton_no_real_assets", P7BenchmarkMarker.BenchmarkScope);
        Assert.AreEqual(P7BenchmarkMarker.BenchmarkStage, marker.Stage);
        Assert.AreEqual(P7BenchmarkMarker.BenchmarkLabel, marker.Label);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject gameObject in createdObjects)
        {
            if (gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        createdObjects.Clear();
    }

    private P7BenchmarkMetricsRecorder CreateRecorder()
    {
        return CreateObject("P7BenchmarkMetricsRecorder_EditModeTest").AddComponent<P7BenchmarkMetricsRecorder>();
    }

    private GameObject CreateObject(string name)
    {
        GameObject gameObject = new GameObject(name);
        createdObjects.Add(gameObject);
        return gameObject;
    }
}
