using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class P5DRealQualifiedGameplayPlayModeTests
{
    private GameObject gameplayRoot;
    private GameObject testGround;
    private GameObject generatorObject;
    private GameObject gameManagerObject;
    private GameObject resultPanelControllerObject;
    private GameObject resultPanelRoot;
    private GameObject testShelterObject;
    private Text resultReasonText;

    private static readonly BindingFlags PrivateInstanceFlags =
        BindingFlags.Instance | BindingFlags.NonPublic;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        ShelterDataLoader.Reload();
        ShelterDataLoader.ClearRuntimeShelters();
        ResultExportService.ExportEnabled = false;
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        ResultExportService.ExportEnabled = ResultExportService.DefaultExportEnabled;
        DestroyIfPresent(generatorObject);
        DestroyIfPresent(gameManagerObject);
        DestroyIfPresent(resultPanelControllerObject);
        DestroyIfPresent(resultPanelRoot);
        DestroyIfPresent(testShelterObject);
        DestroyIfPresent(gameplayRoot);
        DestroyIfPresent(testGround);
        ShelterDataLoader.Reload();
        yield return null;
    }

    [UnityTest]
    public IEnumerator DefaultTestShelterEntryAndResultFlowStillWorks()
    {
        EvacuationGameManager gameManager = CreateConfiguredGameManager();
        BuildingShelter shelter = CreateTestShelter("test_shelter_001");

        gameManager.StartGame();
        yield return null;

        Assert.AreEqual(EvacuationGameManager.GameState.PreEvent, gameManager.CurrentState);

        gameManager.StartEvacuationEvent();
        gameManager.TryEnterShelter(shelter);
        yield return null;

        Assert.AreEqual(EvacuationGameManager.GameState.Climbing, gameManager.CurrentState);

        gameManager.HandleClimbCompleted(shelter);
        yield return null;

        Assert.AreEqual(EvacuationGameManager.GameState.Succeeded, gameManager.CurrentState);
        StringAssert.Contains("Outcome", resultReasonText.text);
        StringAssert.Contains("- Result: Success", resultReasonText.text);
        Assert.IsFalse(resultReasonText.text.Contains("P5-D real qualified feedback"));
    }

    [UnityTest]
    public IEnumerator RealQualifiedModeGeneratesPlayableTargetsWithMetadataAndTriggers()
    {
        CreateGameplayRoot();

        ShelterDataSourceResolver.ShelterDataSourceResult sourceResult = LoadRealQualifiedSourceResult();

        generatorObject = new GameObject("P5D_PlayMode_Generator");
        P5DRealQualifiedShelterRuntimeGenerator generator =
            generatorObject.AddComponent<P5DRealQualifiedShelterRuntimeGenerator>();

        int generatedCount = generator.Generate(sourceResult, null, null);
        yield return null;

        Assert.AreEqual(sourceResult.realQualifiedLoadResult.selectableCount, generatedCount);
        Assert.AreEqual(27, generatedCount);

        Transform runtimeRoot = gameplayRoot.transform.Find("P5D_RealQualifiedShelters_Runtime");
        Assert.NotNull(runtimeRoot);

        BuildingShelter[] shelters = runtimeRoot.GetComponentsInChildren<BuildingShelter>();
        ShelterEntranceTrigger[] entrances = runtimeRoot.GetComponentsInChildren<ShelterEntranceTrigger>();
        RealQualifiedShelterMetadata[] metadata = runtimeRoot.GetComponentsInChildren<RealQualifiedShelterMetadata>();

        Assert.AreEqual(generatedCount, shelters.Length);
        Assert.AreEqual(generatedCount, entrances.Length);
        Assert.AreEqual(generatedCount, metadata.Length);
        Assert.AreEqual(0, generator.LastGeneratedRouteLineCount);
        Assert.That(generator.LastRoutePreviewValidationReason, Does.Contain("no verified WGS84-to-Unity/PLATEAU transform exists"));

        foreach (BuildingShelter shelter in shelters)
        {
            Assert.AreEqual(RealQualifiedShelterDataLoader.SourceType, shelter.SourceType);
            Assert.IsTrue(shelter.CanEnter);
            Assert.IsTrue(shelter.IsOfficialShelter);
            StringAssert.Contains(P5CStaticDataLoader.EstimatedPrototypeRouteLabel, shelter.GetDisplayText());
        }
    }

    [UnityTest]
    public IEnumerator RealQualifiedEntryResultFeedbackWorksWithoutRouteQualificationSuccessRules()
    {
        CreateGameplayRoot();
        EvacuationGameManager gameManager = CreateConfiguredGameManager();
        ShelterDataSourceResolver.ShelterDataSourceResult sourceResult = LoadRealQualifiedSourceResult();

        generatorObject = new GameObject("P5D_PlayMode_Generator");
        P5DRealQualifiedShelterRuntimeGenerator generator =
            generatorObject.AddComponent<P5DRealQualifiedShelterRuntimeGenerator>();
        generator.Generate(sourceResult, gameManager, null);
        yield return null;

        BuildingShelter shelter = gameplayRoot.GetComponentInChildren<BuildingShelter>();
        Assert.NotNull(shelter);
        Assert.AreEqual(RealQualifiedShelterDataLoader.SourceType, shelter.SourceType);

        gameManager.StartGame();
        yield return null;

        gameManager.StartEvacuationEvent();
        gameManager.TryEnterShelter(shelter);
        yield return null;

        Assert.AreEqual(EvacuationGameManager.GameState.Climbing, gameManager.CurrentState);

        gameManager.HandleClimbCompleted(shelter);
        yield return null;

        Assert.AreEqual(EvacuationGameManager.GameState.Succeeded, gameManager.CurrentState);
        StringAssert.Contains("P5-D real qualified feedback", resultReasonText.text);
        StringAssert.Contains("Real qualified shelter:", resultReasonText.text);
        StringAssert.Contains(P5CStaticDataLoader.EstimatedPrototypeRouteLabel, resultReasonText.text);
        StringAssert.Contains("OSM/ODbL attribution:", resultReasonText.text);
        Assert.IsFalse(P5DRealQualifiedShelterRuntimeGenerator.AffectsGameplayRules);
        Assert.IsFalse(TsunamiHazardDebugVisualizer.AffectsGameplayRules);
    }

    [UnityTest]
    public IEnumerator HazardAndMetadataTogglesRemainFunctionalWithRealQualifiedTargets()
    {
        CreateGameplayRoot();
        ShelterDataSourceResolver.ShelterDataSourceResult sourceResult = LoadRealQualifiedSourceResult();

        generatorObject = new GameObject("P5D_PlayMode_Generator");
        P5DRealQualifiedShelterRuntimeGenerator generator =
            generatorObject.AddComponent<P5DRealQualifiedShelterRuntimeGenerator>();
        generator.Generate(sourceResult, null, null);
        yield return null;

        Transform runtimeRoot = gameplayRoot.transform.Find("P5D_RealQualifiedShelters_Runtime");
        TextMesh[] compactLabels = runtimeRoot.GetComponentsInChildren<TextMesh>();
        Assert.IsTrue(ContainsLabel(compactLabels, "Route:"));

        generator.ToggleDetailedMetadata();
        yield return null;

        TextMesh[] detailedLabels = runtimeRoot.GetComponentsInChildren<TextMesh>();
        Assert.IsTrue(ContainsLabel(detailedLabels, "OSM/ODbL attribution applies."));
        Assert.IsTrue(ContainsLabel(detailedLabels, RealQualifiedShelterDataLoader.NoVerifiedGeospatialPlacementNote));

        GameObject hazardObject = new GameObject("P5D_HazardVisualizer_Test");
        TsunamiHazardDebugVisualizer visualizer = hazardObject.AddComponent<TsunamiHazardDebugVisualizer>();
        visualizer.Show();
        yield return null;

        Transform hazardRoot = gameplayRoot.transform.Find("P4B_HazardDebugVisualization_Runtime");
        Assert.NotNull(hazardRoot);
        Assert.IsTrue(hazardRoot.gameObject.activeSelf);
        Assert.AreEqual(0, hazardRoot.GetComponentsInChildren<Collider>().Length);

        visualizer.Hide();
        yield return null;

        Assert.IsFalse(hazardRoot.gameObject.activeSelf);
        DestroyIfPresent(hazardObject);
    }

    [UnityTest]
    public IEnumerator UnsafeRouteTransformDoesNotRenderMisleadingRouteLine()
    {
        CreateGameplayRoot();
        ShelterDataSourceResolver.ShelterDataSourceResult sourceResult = LoadRealQualifiedSourceResult();

        generatorObject = new GameObject("P5D_PlayMode_Generator");
        P5DRealQualifiedShelterRuntimeGenerator generator =
            generatorObject.AddComponent<P5DRealQualifiedShelterRuntimeGenerator>();
        generator.Generate(sourceResult, null, null);
        yield return null;

        Assert.AreEqual(0, generator.LastGeneratedRouteLineCount);

        P5DRoutePreviewMetadata[] routeMetadata =
            gameplayRoot.GetComponentsInChildren<P5DRoutePreviewMetadata>();
        Assert.AreEqual(0, routeMetadata.Length);
        Assert.That(generator.LastRoutePreviewValidationReason, Does.Contain("no verified WGS84-to-Unity/PLATEAU transform exists"));
    }

    private EvacuationGameManager CreateConfiguredGameManager()
    {
        gameManagerObject = new GameObject("P5D_GameManager_Test");
        EvacuationGameManager gameManager = gameManagerObject.AddComponent<EvacuationGameManager>();
        ClimbSimulation climbSimulation = gameManagerObject.AddComponent<ClimbSimulation>();
        ResultPanelController resultPanel = CreateResultPanelController();

        SetPrivateField(gameManager, "startOnAwake", false);
        SetPrivateField(gameManager, "exportResultLogs", false);
        SetPrivateField(gameManager, "climbSimulation", climbSimulation);
        SetPrivateField(gameManager, "resultPanelController", resultPanel);
        return gameManager;
    }

    private ResultPanelController CreateResultPanelController()
    {
        resultPanelControllerObject = new GameObject("P5D_ResultPanelController_Test");
        ResultPanelController resultPanel = resultPanelControllerObject.AddComponent<ResultPanelController>();
        resultPanelRoot = new GameObject("P5D_ResultPanelRoot_Test", typeof(RectTransform));

        Text titleText = CreateText("TitleText");
        Text shelterText = CreateText("ShelterText");
        Text elapsedText = CreateText("ElapsedText");
        resultReasonText = CreateText("ReasonText");

        titleText.transform.SetParent(resultPanelRoot.transform, false);
        shelterText.transform.SetParent(resultPanelRoot.transform, false);
        elapsedText.transform.SetParent(resultPanelRoot.transform, false);
        resultReasonText.transform.SetParent(resultPanelRoot.transform, false);

        SetPrivateField(resultPanel, "panelRoot", resultPanelRoot);
        SetPrivateField(resultPanel, "titleText", titleText);
        SetPrivateField(resultPanel, "shelterText", shelterText);
        SetPrivateField(resultPanel, "elapsedTimeText", elapsedText);
        SetPrivateField(resultPanel, "reasonText", resultReasonText);
        return resultPanel;
    }

    private static Text CreateText(string name)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        return textObject.AddComponent<Text>();
    }

    private BuildingShelter CreateTestShelter(string shelterId)
    {
        ShelterDataLoader.Reload();
        Assert.IsTrue(ShelterDataLoader.TryGetShelter(shelterId, out ShelterDataLoader.ShelterData shelterData));

        testShelterObject = new GameObject($"P5D_TestShelter_{shelterId}");
        BuildingShelter shelter = testShelterObject.AddComponent<BuildingShelter>();
        shelter.ApplyShelterData(shelterData);
        return shelter;
    }

    private ShelterDataSourceResolver.ShelterDataSourceResult LoadRealQualifiedSourceResult()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config =
            ShelterSourceConfigLoader.CreateDefaultConfig();
        config.sourceMode = ShelterSourceConfigLoader.RealQualifiedSourceMode;

        ShelterDataSourceResolver.ShelterDataSourceResult sourceResult =
            ShelterDataSourceResolver.LoadFromConfig(config);

        Assert.IsTrue(sourceResult.success);
        Assert.IsTrue(sourceResult.IsRealQualified);
        return sourceResult;
    }

    private void CreateGameplayRoot()
    {
        gameplayRoot = new GameObject("GameplayTestRoot");
        testGround = new GameObject("TestGround");
        testGround.transform.position = new Vector3(0f, 80f, -250f);
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

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, PrivateInstanceFlags);
        Assert.NotNull(field, fieldName);
        field.SetValue(target, value);
    }

    private static void DestroyIfPresent(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Object.Destroy(gameObject);
        }
    }
}
