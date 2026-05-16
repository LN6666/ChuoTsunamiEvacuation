using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P4BDebugLayerPlayModeTests
{
    private GameObject gameplayRoot;
    private GameObject testGround;
    private GameObject generatorObject;
    private GameObject visualizerObject;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        ShelterDataLoader.Reload();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        DestroyIfPresent(visualizerObject);
        DestroyIfPresent(generatorObject);
        DestroyIfPresent(gameplayRoot);
        DestroyIfPresent(testGround);

        ShelterDataLoader.Reload();
        yield return null;
    }

    [UnityTest]
    public IEnumerator RealShelterRuntimeGeneratorCreatesGameplayCompatibleMarkers()
    {
        gameplayRoot = new GameObject("GameplayTestRoot");
        testGround = new GameObject("TestGround");
        testGround.transform.position = new Vector3(0f, 80f, -250f);

        RealShelterDataLoader.RealShelterLoadResult realLoad = RealShelterDataLoader.LoadRealSample();
        var sourceResult = new ShelterDataSourceResolver.ShelterDataSourceResult
        {
            sourceMode = ShelterSourceConfigLoader.RealSampleSourceMode,
            success = realLoad.success,
            sourcePath = realLoad.sourcePath,
            realShelters = realLoad.shelters
        };

        generatorObject = new GameObject("P4BV_RealMarkerGenerator");
        RealShelterMarkerRuntimeGenerator generator =
            generatorObject.AddComponent<RealShelterMarkerRuntimeGenerator>();

        int generatedCount = generator.Generate(sourceResult, null, null);
        yield return null;

        Assert.Greater(generatedCount, 1);

        BuildingShelter[] shelters = gameplayRoot.GetComponentsInChildren<BuildingShelter>();
        ShelterEntranceTrigger[] entrances = gameplayRoot.GetComponentsInChildren<ShelterEntranceTrigger>();
        TextMesh[] labels = gameplayRoot.GetComponentsInChildren<TextMesh>();

        Assert.AreEqual(generatedCount, CountRealSampleShelters(shelters));
        Assert.AreEqual(generatedCount, entrances.Length);
        Assert.IsTrue(ContainsLabel(labels, "Harumi Sample Evacuation Building"));
        Assert.IsTrue(ContainsLabel(labels, "Capacity: 150"));
        Assert.IsTrue(ContainsLabel(labels, "Safe floor estimate: 5"));
    }

    [UnityTest]
    public IEnumerator HazardDebugVisualizerTogglesTemporaryColliderFreeObjects()
    {
        gameplayRoot = new GameObject("GameplayTestRoot");
        testGround = new GameObject("TestGround");
        testGround.transform.position = new Vector3(0f, 80f, -250f);

        TsunamiHazardFixtureLoader.HazardFixtureLoadResult hazardLoad =
            TsunamiHazardFixtureLoader.LoadHazardFixture();

        visualizerObject = new GameObject("P4BV_HazardVisualizer");
        TsunamiHazardDebugVisualizer visualizer =
            visualizerObject.AddComponent<TsunamiHazardDebugVisualizer>();

        visualizer.Show();
        yield return null;

        Transform visualizationRootTransform = gameplayRoot.transform.Find("P4B_HazardDebugVisualization_Runtime");
        GameObject visualizationRoot = visualizationRootTransform != null ? visualizationRootTransform.gameObject : null;
        Assert.NotNull(visualizationRoot);
        Assert.IsTrue(visualizationRoot.activeSelf);
        Assert.AreEqual(hazardLoad.zones.Length * 2, visualizationRoot.transform.childCount);
        Assert.AreEqual(0, visualizationRoot.GetComponentsInChildren<Collider>().Length);
        Assert.IsTrue(ContainsLabel(visualizationRoot.GetComponentsInChildren<TextMesh>(), "Sample Harumi Low Hazard Zone"));
        Assert.IsFalse(TsunamiHazardDebugVisualizer.AffectsGameplayRules);

        visualizer.Hide();
        yield return null;

        Assert.IsFalse(visualizationRoot.activeSelf);
    }

    private static int CountRealSampleShelters(BuildingShelter[] shelters)
    {
        int count = 0;

        foreach (BuildingShelter shelter in shelters)
        {
            if (shelter != null && shelter.SourceType == ShelterSourceConfigLoader.RealSampleSourceMode)
            {
                count++;
            }
        }

        return count;
    }

    private static bool ContainsLabel(TextMesh[] labels, string expectedText)
    {
        foreach (TextMesh label in labels)
        {
            if (label != null && label.text.Contains(expectedText))
            {
                return true;
            }
        }

        return false;
    }

    private static void DestroyIfPresent(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Object.Destroy(gameObject);
        }
    }
}
