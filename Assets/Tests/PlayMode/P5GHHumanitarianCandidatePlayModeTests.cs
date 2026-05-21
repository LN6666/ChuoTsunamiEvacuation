using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class P5GHHumanitarianCandidatePlayModeTests
{
    private GameObject gameplayRoot;
    private GameObject testGround;
    private GameObject generatorObject;
    private GameObject gameManagerObject;
    private GameObject resultPanelControllerObject;
    private GameObject resultPanelRoot;
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
        DestroyIfPresent(gameplayRoot);
        DestroyIfPresent(testGround);
        ShelterDataLoader.Reload();
        yield return null;
    }

    [UnityTest]
    public IEnumerator DefaultHumanitarianConfigCreatesNoCandidateMarkers()
    {
        CreateGameplayRoot();
        generatorObject = new GameObject("P5GH_PlayMode_Generator");
        P5GHHumanitarianCandidateRuntimeGenerator generator =
            generatorObject.AddComponent<P5GHHumanitarianCandidateRuntimeGenerator>();

        ShelterSourceConfigLoader.ShelterSourceConfig config =
            ShelterSourceConfigLoader.CreateDefaultConfig();

        int generated = generator.GenerateFromConfig(config, null, null);
        yield return null;

        Assert.AreEqual(0, generated);
        Assert.IsNull(gameplayRoot.transform.Find("P5GH_HumanitarianCandidates_Runtime"));
        Assert.IsFalse(config.enableHumanitarianCandidates);
        Assert.IsFalse(config.enableLifeFirstCandidateSelection);
    }

    [UnityTest]
    public IEnumerator DisplayOnlyModeCreatesNonPlayableCandidateMarkers()
    {
        CreateGameplayRoot();
        generatorObject = new GameObject("P5GH_PlayMode_Generator");
        P5GHHumanitarianCandidateRuntimeGenerator generator =
            generatorObject.AddComponent<P5GHHumanitarianCandidateRuntimeGenerator>();

        ShelterSourceConfigLoader.ShelterSourceConfig config =
            ShelterSourceConfigLoader.CreateDefaultConfig();
        config.enableHumanitarianCandidates = true;
        config.enableLifeFirstCandidateSelection = false;

        int generated = generator.GenerateFromConfig(config, null, null);
        yield return null;

        Assert.AreEqual(5, generated);
        Assert.AreEqual(5, generator.LastGeneratedDisplayOnlyCount);
        Assert.AreEqual(0, generator.LastGeneratedSelectableCount);

        Transform root = gameplayRoot.transform.Find("P5GH_HumanitarianCandidates_Runtime");
        Assert.NotNull(root);
        Assert.AreEqual(5, root.GetComponentsInChildren<HumanitarianCandidateMetadata>().Length);
        Assert.AreEqual(0, root.GetComponentsInChildren<BuildingShelter>().Length);
        Assert.AreEqual(0, root.GetComponentsInChildren<ShelterEntranceTrigger>().Length);
        Assert.AreEqual(0, root.GetComponentsInChildren<Collider>().Length);
        Assert.AreEqual(0, root.GetComponentsInChildren<Rigidbody>().Length);
        Assert.IsFalse(P5GHHumanitarianCandidateRuntimeGenerator.AffectsGameplayRules);
        AssertNoHumanitarianRuntimeShelterRegistration();
        AssertDisplayOnlyMarkersHaveNoRaycastHit(root);
        Assert.IsTrue(ContainsLabel(root.GetComponentsInChildren<TextMesh>(), HumanitarianCandidateDataLoader.HumanitarianCandidateLabel));
        Assert.IsTrue(ContainsLabel(root.GetComponentsInChildren<TextMesh>(), HumanitarianCandidateDataLoader.NotOfficiallyDesignatedLabel));
    }

    [UnityTest]
    public IEnumerator LifeFirstSelectionFlagAloneCreatesNoCandidateMarkers()
    {
        CreateGameplayRoot();
        generatorObject = new GameObject("P5GH_PlayMode_Generator");
        P5GHHumanitarianCandidateRuntimeGenerator generator =
            generatorObject.AddComponent<P5GHHumanitarianCandidateRuntimeGenerator>();

        ShelterSourceConfigLoader.ShelterSourceConfig config =
            ShelterSourceConfigLoader.CreateDefaultConfig();
        config.enableHumanitarianCandidates = false;
        config.enableLifeFirstCandidateSelection = true;

        int generated = generator.GenerateFromConfig(config, null, null);
        yield return null;

        Assert.AreEqual(0, generated);
        Assert.AreEqual(0, generator.LastGeneratedDisplayOnlyCount);
        Assert.AreEqual(0, generator.LastGeneratedSelectableCount);
        Assert.IsNull(gameplayRoot.transform.Find("P5GH_HumanitarianCandidates_Runtime"));
        AssertNoHumanitarianRuntimeShelterRegistration();
    }

    [UnityTest]
    public IEnumerator LifeFirstSelectionCreatesOnlyAllowedNonOfficialCandidateProxies()
    {
        CreateGameplayRoot();
        generatorObject = new GameObject("P5GH_PlayMode_Generator");
        P5GHHumanitarianCandidateRuntimeGenerator generator =
            generatorObject.AddComponent<P5GHHumanitarianCandidateRuntimeGenerator>();

        ShelterSourceConfigLoader.ShelterSourceConfig config =
            ShelterSourceConfigLoader.CreateDefaultConfig();
        config.enableHumanitarianCandidates = true;
        config.enableLifeFirstCandidateSelection = true;

        int generated = generator.GenerateFromConfig(config, null, null);
        yield return null;

        Assert.AreEqual(5, generated);
        Assert.AreEqual(3, generator.LastGeneratedDisplayOnlyCount);
        Assert.AreEqual(2, generator.LastGeneratedSelectableCount);

        Transform root = gameplayRoot.transform.Find("P5GH_HumanitarianCandidates_Runtime");
        Assert.NotNull(root);

        BuildingShelter[] shelters = root.GetComponentsInChildren<BuildingShelter>();
        ShelterEntranceTrigger[] entrances = root.GetComponentsInChildren<ShelterEntranceTrigger>();
        HumanitarianCandidateMetadata[] metadata = root.GetComponentsInChildren<HumanitarianCandidateMetadata>();

        Assert.AreEqual(2, shelters.Length);
        Assert.AreEqual(2, entrances.Length);
        Assert.AreEqual(5, metadata.Length);

        foreach (BuildingShelter shelter in shelters)
        {
            Assert.AreEqual(HumanitarianCandidateDataLoader.SourceType, shelter.SourceType);
            Assert.IsFalse(shelter.IsOfficialShelter);
            Assert.IsTrue(shelter.CanEnter);
            StringAssert.Contains(HumanitarianCandidateDataLoader.HumanitarianCandidateLabel, shelter.GetDisplayText());
            StringAssert.Contains(HumanitarianCandidateDataLoader.NotOfficiallyDesignatedLabel, shelter.GetDisplayText());
            StringAssert.Contains(HumanitarianCandidateDataLoader.LifeFirstAssumptionLabel, shelter.GetDisplayText());
        }

        foreach (HumanitarianCandidateMetadata item in metadata)
        {
            Assert.AreEqual(HumanitarianCandidateDataLoader.CandidateLayer, item.CandidateLayer);
            Assert.AreNotEqual("official_designated", item.OfficialDesignationStatus);
            if (item.SelectableInLifeFirstMode)
            {
                Assert.IsTrue(HumanitarianCandidateDataLoader.IsDefaultLifeFirstSelectableStatus(item.HumanitarianCandidateStatus));
            }
        }
    }

    [UnityTest]
    public IEnumerator LifeFirstCandidateSelectionUsesExistingResultFlowWithoutOfficialClaim()
    {
        CreateGameplayRoot();
        EvacuationGameManager gameManager = CreateConfiguredGameManager();

        generatorObject = new GameObject("P5GH_PlayMode_Generator");
        P5GHHumanitarianCandidateRuntimeGenerator generator =
            generatorObject.AddComponent<P5GHHumanitarianCandidateRuntimeGenerator>();

        HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult loadResult =
            HumanitarianCandidateDataLoader.LoadFromAssetsData();
        generator.Generate(loadResult, true, gameManager, null);
        yield return null;

        Transform root = gameplayRoot.transform.Find("P5GH_HumanitarianCandidates_Runtime");
        BuildingShelter shelter = root.GetComponentInChildren<BuildingShelter>();
        Assert.NotNull(shelter);
        Assert.IsFalse(shelter.IsOfficialShelter);
        Assert.AreEqual(HumanitarianCandidateDataLoader.SourceType, shelter.SourceType);

        gameManager.StartGame();
        yield return null;

        gameManager.StartEvacuationEvent();
        gameManager.TryEnterShelter(shelter);
        yield return null;

        Assert.AreEqual(EvacuationGameManager.GameState.Climbing, gameManager.CurrentState);

        gameManager.HandleClimbCompleted(shelter);
        yield return null;

        Assert.AreEqual(EvacuationGameManager.GameState.Succeeded, gameManager.CurrentState);
        StringAssert.Contains("P5-GH humanitarian candidate feedback", resultReasonText.text);
        StringAssert.Contains(HumanitarianCandidateDataLoader.HumanitarianCandidateLabel, resultReasonText.text);
        StringAssert.Contains(HumanitarianCandidateDataLoader.NotOfficiallyDesignatedLabel, resultReasonText.text);
        StringAssert.Contains(HumanitarianCandidateDataLoader.LifeFirstAssumptionLabel, resultReasonText.text);
        StringAssert.Contains(HumanitarianCandidateDataLoader.ControlledSampleNotice, resultReasonText.text);
        StringAssert.Contains("- Rank/official: Life-first / no", resultReasonText.text);
        Assert.IsFalse(P5GHHumanitarianCandidateRuntimeGenerator.AffectsGameplayRules);
    }

    private EvacuationGameManager CreateConfiguredGameManager()
    {
        gameManagerObject = new GameObject("P5GH_GameManager_Test");
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
        resultPanelControllerObject = new GameObject("P5GH_ResultPanelController_Test");
        ResultPanelController resultPanel = resultPanelControllerObject.AddComponent<ResultPanelController>();
        resultPanelRoot = new GameObject("P5GH_ResultPanelRoot_Test", typeof(RectTransform));

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

    private static void AssertNoHumanitarianRuntimeShelterRegistration()
    {
        foreach (ShelterDataLoader.ShelterData shelter in ShelterDataLoader.GetAllShelters())
        {
            Assert.AreNotEqual(HumanitarianCandidateDataLoader.SourceType, shelter.sourceType);
        }
    }

    private static void AssertDisplayOnlyMarkersHaveNoRaycastHit(Transform root)
    {
        int displayMarkerCount = 0;
        foreach (Transform transform in root.GetComponentsInChildren<Transform>())
        {
            if (transform == null || !transform.name.StartsWith("P5GH_DisplayCandidateMarker_"))
            {
                continue;
            }

            displayMarkerCount++;
            Vector3 origin = transform.position + Vector3.up * 10f;
            bool hit = Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, 20f);
            Assert.IsFalse(
                hit,
                hit ? $"Display-only marker '{transform.name}' unexpectedly hit '{hitInfo.collider.name}'." : transform.name);
        }

        Assert.AreEqual(5, displayMarkerCount);
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
