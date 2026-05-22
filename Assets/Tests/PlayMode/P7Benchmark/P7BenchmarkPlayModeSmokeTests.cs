using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P7BenchmarkPlayModeSmokeTests
{
    private GameObject testObject;

    [UnityTest]
    public IEnumerator MetricsRecorderSamplesTemporaryRuntimeObject()
    {
        testObject = new GameObject("P7BenchmarkMetricsRecorder_PlayModeTest");
        P7BenchmarkMetricsRecorder recorder = testObject.AddComponent<P7BenchmarkMetricsRecorder>();

        recorder.BeginRecording();
        yield return null;
        yield return null;
        recorder.StopRecording();

        Assert.Greater(recorder.SampleCount, 0);
        Assert.Greater(recorder.AverageFps, 0f);
        Assert.That(recorder.ExportSummaryString(), Does.Contain("scope=p7benchmark_sandbox_chunk_loading_metadata_only"));
    }

    [UnityTest]
    public IEnumerator MarkerCanExistOnTemporaryRuntimeObject()
    {
        testObject = new GameObject("P7BenchmarkMarker_PlayModeTest");
        P7BenchmarkMarker marker = testObject.AddComponent<P7BenchmarkMarker>();

        yield return null;

        Assert.AreEqual(P7BenchmarkMarker.BenchmarkStage, marker.Stage);
        Assert.AreEqual(P7BenchmarkMarker.BenchmarkLabel, marker.Label);
        Assert.IsNull(testObject.GetComponent("EvacuationGameManager"));
    }

    [UnityTest]
    public IEnumerator ChunkControllerTogglesRuntimePlaceholderSafely()
    {
        testObject = new GameObject("P7BenchmarkChunkController_PlayModeTest");
        P7BenchmarkChunkController controller = testObject.AddComponent<P7BenchmarkChunkController>();
        GameObject chunkRoot = new GameObject("P7BenchmarkChunk_PlayModeTest");
        chunkRoot.transform.SetParent(testObject.transform);

        controller.RegisterChunkRoot("53393690_bldg", chunkRoot);

        yield return null;

        Assert.AreEqual(1, controller.ActiveChunkCount);
        Assert.IsTrue(controller.SetChunkActive("53393690_bldg", false));
        Assert.IsFalse(chunkRoot.activeSelf);
        Assert.IsFalse(P7BenchmarkChunkController.AffectsGameplaySuccessFailure);
        Assert.IsNull(testObject.GetComponent("EvacuationGameManager"));
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (testObject != null)
        {
            UnityEngine.Object.Destroy(testObject);
            testObject = null;
        }

        yield return null;
    }
}
