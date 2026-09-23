using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class P7HighDetailSceneMetadataTests
{
    private GameObject createdObject;

    [Test]
    public void HighDetailMetadataDefinesP7DTargetSceneWithoutChuoBaseMapDependency()
    {
        Assert.AreEqual("P7-C", P7HighDetailSceneMetadata.Stage);
        Assert.AreEqual("P7_HighDetail_Chuo", P7HighDetailSceneMetadata.SceneName);
        Assert.AreEqual("Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity", P7HighDetailSceneMetadata.ScenePath);
        Assert.IsTrue(P7HighDetailSceneMetadata.IsP7DTargetScenePath(P7HighDetailSceneMetadata.ScenePath));
        Assert.IsFalse(P7HighDetailSceneMetadata.ScenePath.Contains("Chuo_BaseMap"));
    }

    [Test]
    public void ExpectedLayerRootsCoverHighDetailChuoCategoriesAndCompatibilityRoot()
    {
        string[] expected =
        {
            "Buildings",
            "Roads",
            "Bridges",
            "Underground",
            "CityFurniture",
            "Water",
            "Vegetation",
            "Relief",
            "DisasterRisk",
            "LandUse",
            "UrbanPlanningDecision",
            "P2P6Compatibility"
        };

        CollectionAssert.AreEquivalent(expected, P7HighDetailSceneMetadata.ExpectedLayerRootNames);

        foreach (string expectedRoot in expected)
        {
            Assert.IsTrue(P7HighDetailSceneMetadata.IsExpectedLayerRootName(expectedRoot));
        }
    }

    [Test]
    public void MetadataDefaultsMakePendingImportAndRuntimeSmokeExplicit()
    {
        createdObject = new GameObject("P7HighDetailSceneMetadata_EditModeTest");
        P7HighDetailSceneMetadata metadata = createdObject.AddComponent<P7HighDetailSceneMetadata>();

        metadata.ResetToDefaults();

        Assert.IsTrue(metadata.IsP7DProfilingTarget);
        Assert.IsFalse(metadata.ActualPlateauAssetsLoaded);
        Assert.IsTrue(P7HighDetailSceneMetadata.IsPendingImportStatus(metadata.ImportStatus));
        Assert.AreEqual(P7HighDetailSceneMetadata.RuntimeSmokePendingStatus, metadata.CompatibilityStatus);
        Assert.AreEqual(P7HighDetailSceneMetadata.BaselineIntentStatus, metadata.BaselineIntent);
        Assert.IsFalse(metadata.AffectsGameplaySuccessFailure);
        Assert.That(metadata.GetSummaryText(), Does.Contain("actualPlateauAssetsLoaded=False"));
        Assert.That(metadata.GetSummaryText(), Does.Contain("affectsGameplaySuccessFailure=False"));
    }

    [Test]
    public void P7HighDetailMetadataDoesNotReferenceGameplaySuccessFailureManagers()
    {
        Assert.IsFalse(P7HighDetailSceneMetadata.ModifiesGameplaySuccessFailure);

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

        FieldInfo[] fields = typeof(P7HighDetailSceneMetadata).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo field in fields)
        {
            Assert.IsFalse(
                forbiddenFieldTypeFragments.Any(fragment => field.FieldType.Name.Contains(fragment)),
                $"P7HighDetailSceneMetadata.{field.Name} references gameplay type {field.FieldType.Name}");
        }

        MethodInfo[] methods = typeof(P7HighDetailSceneMetadata).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        foreach (MethodInfo method in methods)
        {
            Assert.IsFalse(
                forbiddenMethodNames.Contains(method.Name),
                $"P7HighDetailSceneMetadata.{method.Name} should not modify gameplay success/failure state");
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (createdObject != null)
        {
            UnityEngine.Object.DestroyImmediate(createdObject);
            createdObject = null;
        }
    }
}
