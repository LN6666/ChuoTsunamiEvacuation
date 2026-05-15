using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstPlayableSceneBuilderTests
{
    [Test]
    public void BuildFirstPlayableTestSetupCreatesMultiShelterField()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        FirstPlayableSceneBuilder.BuildFirstPlayableTestSetup();

        GameObject root = GameObject.Find("GameplayTestRoot");
        Assert.NotNull(root);

        BuildingShelter[] shelters = Object.FindObjectsOfType<BuildingShelter>();
        Assert.GreaterOrEqual(shelters.Length, 4);

        foreach (BuildingShelter shelter in shelters)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(shelter.ShelterId));
            Assert.NotNull(shelter.GetComponentInChildren<ShelterEntranceTrigger>());
        }
    }

    [TearDown]
    public void CleanupScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid())
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
    }
}
