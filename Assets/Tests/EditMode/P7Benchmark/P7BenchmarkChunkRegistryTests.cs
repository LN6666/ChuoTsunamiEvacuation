using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class P7BenchmarkChunkRegistryTests
{
    private readonly List<UnityEngine.Object> createdObjects = new List<UnityEngine.Object>();

    [Test]
    public void RegistryHandlesEmptyInputSafely()
    {
        P7BenchmarkChunkRegistry registry = CreateRegistry();

        registry.Configure("53393690", "Assets/P7Benchmark/Imported/53393690", null);

        Assert.AreEqual("53393690", registry.CandidateId);
        Assert.AreEqual(0, registry.ChunkCount);
        Assert.AreEqual(0, registry.TotalSourceFileCount);
        Assert.AreEqual(0L, registry.TotalSourceBytes);
        Assert.That(registry.GetSummaryText(), Does.Contain("chunks=0"));
        Assert.IsNull(registry.FindChunkById("missing"));
        Assert.IsEmpty(registry.FindChunksByCategory("bldg"));
    }

    [Test]
    public void RegistryRecordsCandidate53393690Metadata()
    {
        P7BenchmarkChunkRegistry registry = CreateRegistry();
        P7BenchmarkChunkInfo buildingChunk = CreateChunk("53393690_bldg", "bldg", 5181, 272392412L, 1, 5180, 0);
        P7BenchmarkChunkInfo transportChunk = CreateChunk("53393690_tran", "tran", 1, 46558793L, 1, 0, 0);

        registry.Configure(
            "53393690",
            "Assets/P7Benchmark/Imported/53393690",
            new[] { buildingChunk, transportChunk });

        Assert.AreEqual(2, registry.ChunkCount);
        Assert.AreEqual(5182, registry.TotalSourceFileCount);
        Assert.AreEqual(318951205L, registry.TotalSourceBytes);
        Assert.AreEqual(2, registry.TotalCityGmlFileCount);
        Assert.AreEqual(5180, registry.TotalTextureFileCount);
        Assert.AreEqual(0, registry.TotalRenderableAssetCount);
        Assert.AreEqual(2, registry.PlaceholderOnlyChunkCount);
        Assert.AreSame(buildingChunk, registry.FindChunkById("53393690_bldg"));
        Assert.AreEqual(1, registry.FindChunksByCategory("tran").Count());
        Assert.That(registry.GetDetailedSummaryText(), Does.Contain("candidate=53393690"));
        Assert.That(registry.GetDetailedSummaryText(), Does.Contain("placeholderOnly=True"));
    }

    [Test]
    public void ChunkControllerEnableDisableBehaviorIsSafe()
    {
        P7BenchmarkChunkRegistry registry = CreateRegistry();
        registry.Configure(
            "53393690",
            "Assets/P7Benchmark/Imported/53393690",
            new[]
            {
                CreateChunk("53393690_bldg", "bldg", 1, 10L, 1, 0, 0),
                CreateChunk("53393690_tran", "tran", 1, 20L, 1, 0, 0)
            });

        GameObject controllerObject = CreateGameObject("P7C_Controller_EditModeTest");
        P7BenchmarkChunkController controller = controllerObject.AddComponent<P7BenchmarkChunkController>();
        controller.SetRegistry(registry);

        GameObject buildingRoot = CreateGameObject("P7C_BuildingChunk_EditModeTest");
        GameObject transportRoot = CreateGameObject("P7C_TransportChunk_EditModeTest");
        controller.RegisterChunkRoot("53393690_bldg", buildingRoot);
        controller.RegisterChunkRoot("53393690_tran", transportRoot);

        Assert.AreEqual(2, controller.BindingCount);
        Assert.AreEqual(2, controller.ActiveChunkCount);
        Assert.IsTrue(controller.SetChunkActive("53393690_bldg", false));
        Assert.IsFalse(buildingRoot.activeSelf);
        Assert.IsTrue(transportRoot.activeSelf);
        Assert.AreEqual(1, controller.SetGroupActiveByCategory("bldg", true));
        Assert.IsTrue(buildingRoot.activeSelf);
        Assert.IsFalse(controller.SetChunkActive("missing", false));

        controller.SetAllChunksActive(false);

        Assert.AreEqual(0, controller.ActiveChunkCount);
        Assert.That(controller.GetSummaryText(), Does.Contain("affectsGameplaySuccessFailure=False"));
    }

    [Test]
    public void MetricsRecorderIncludesChunkContextAndStillWorks()
    {
        P7BenchmarkChunkRegistry registry = CreateRegistry();
        registry.Configure(
            "53393690",
            "Assets/P7Benchmark/Imported/53393690",
            new[] { CreateChunk("53393690_bldg", "bldg", 3, 300L, 1, 2, 0) });

        GameObject root = CreateGameObject("P7C_MetricsRoot_EditModeTest");
        P7BenchmarkChunkController controller = root.AddComponent<P7BenchmarkChunkController>();
        controller.SetRegistry(registry);
        controller.RegisterChunkRoot("53393690_bldg", CreateGameObject("P7C_MetricsChunk_EditModeTest"));

        P7BenchmarkMetricsRecorder recorder = root.AddComponent<P7BenchmarkMetricsRecorder>();
        recorder.ConfigureChunkContext(registry, controller);
        recorder.RecordFrameTime(0.016f);
        recorder.RecordFrameTime(0.02f);

        string summary = recorder.ExportSummaryString();

        Assert.That(summary, Does.Contain("P7BenchmarkMetricsSummary"));
        Assert.That(summary, Does.Contain("activeChunks=1"));
        Assert.That(summary, Does.Contain("chunkBindings=1"));
        Assert.That(summary, Does.Contain("importedCandidate=53393690"));
        Assert.That(summary, Does.Contain("importedFiles=3"));
        Assert.Greater(recorder.AverageFps, 0f);
    }

    [Test]
    public void P7BenchmarkComponentsDoNotReferenceGameplayManagersOrSuccessFailureState()
    {
        Assert.IsFalse(P7BenchmarkChunkController.AffectsGameplaySuccessFailure);

        Type[] p7BenchmarkTypes =
        {
            typeof(P7BenchmarkChunkInfo),
            typeof(P7BenchmarkChunkRegistry),
            typeof(P7BenchmarkChunkController),
            typeof(P7BenchmarkMetricsRecorder),
            typeof(P7BenchmarkMarker)
        };

        string[] forbiddenFieldTypeFragments =
        {
            "EvacuationGameManager",
            "BuildingShelter",
            "ShelterEntranceTrigger",
            "MovingTsunamiWall",
            "RiskZone"
        };

        string[] forbiddenMethodNames =
        {
            "TriggerFailure",
            "CompleteSuccess",
            "CompleteFailure",
            "ShowSuccess",
            "ShowFailure"
        };

        foreach (Type type in p7BenchmarkTypes)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                Assert.IsFalse(
                    forbiddenFieldTypeFragments.Any(fragment => field.FieldType.Name.Contains(fragment)),
                    $"{type.Name}.{field.Name} references gameplay type {field.FieldType.Name}");
            }

            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                Assert.IsFalse(
                    forbiddenMethodNames.Contains(method.Name),
                    $"{type.Name}.{method.Name} should not modify gameplay success/failure state");
            }
        }
    }

    [TearDown]
    public void TearDown()
    {
        foreach (UnityEngine.Object createdObject in createdObjects)
        {
            if (createdObject != null)
            {
                UnityEngine.Object.DestroyImmediate(createdObject);
            }
        }

        createdObjects.Clear();
    }

    private P7BenchmarkChunkRegistry CreateRegistry()
    {
        P7BenchmarkChunkRegistry registry = ScriptableObject.CreateInstance<P7BenchmarkChunkRegistry>();
        createdObjects.Add(registry);
        return registry;
    }

    private GameObject CreateGameObject(string name)
    {
        GameObject gameObject = new GameObject(name);
        createdObjects.Add(gameObject);
        return gameObject;
    }

    private static P7BenchmarkChunkInfo CreateChunk(
        string chunkId,
        string category,
        int fileCount,
        long bytes,
        int gmlCount,
        int textureCount,
        int renderableCount)
    {
        return new P7BenchmarkChunkInfo(
            chunkId,
            "53393690",
            chunkId,
            category,
            $"Assets/P7Benchmark/Imported/53393690/udx/{category}",
            fileCount,
            bytes,
            gmlCount,
            textureCount,
            renderableCount,
            renderableCount == 0,
            true,
            "test metadata");
    }
}
