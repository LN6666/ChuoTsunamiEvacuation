using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class P6DPlayableBehaviorValidationPlayModeTests
{
    private P6DPlayableBehaviorScenarioContext context;

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (context != null)
        {
            context.DestroyGeneratedObjects();
            context = null;
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator GeneratedScenarioValidatesNavigationAndNpcCoexistence()
    {
        AssertNoChuoBaseMapScene();
        AssertNoResultManagers();

        context = P6DPlayableBehaviorScenarioBuilder.Create(new P6DPlayableBehaviorScenarioConfig
        {
            scenarioName = "P6D_PlayMode_GeneratedScenario",
            playerStartPosition = Vector3.zero,
            shelterPosition = new Vector3(0f, 0f, 4f),
            npcCount = 3,
            npcSpawnOrigin = Vector3.zero,
            npcSpawnAreaSize = Vector2.zero,
            npcMoveSpeed = 8f,
            npcArrivalDistance = 0.1f
        });

        NavigationGuidanceResult initialGuidance = context.RefreshGuidance();
        Assert.IsTrue(initialGuidance.hasTarget);
        Assert.AreEqual(4f, initialGuidance.distanceMeters, 0.001f);
        Assert.AreEqual(3, context.NpcAgents.Length);

        context.PlayerTransform.position = new Vector3(0f, 0f, 2f);
        NavigationGuidanceResult guidanceAfterPlayerMove = context.RefreshGuidance();
        Assert.Less(guidanceAfterPlayerMove.distanceMeters, initialGuidance.distanceMeters);

        context.TickNpcs(0.5f);
        yield return null;

        NavigationGuidanceResult finalGuidance = context.RefreshGuidance();
        P6DBehaviorValidationResult result = context.Evaluate(
            initialGuidance,
            finalGuidance,
            HasNoResultManagers());

        Assert.IsTrue(result.Passed, result.ToSummaryText());
        Assert.AreEqual(3, result.selectedTargetCount);
        Assert.AreEqual(3, result.npcArrivedCount);
        Assert.IsTrue(result.npcNonBlocking);
        Assert.AreEqual("P6-D Generated Test Shelter", context.TargetText.text);
        StringAssert.Contains("Not official navigation", context.StatusText.text);
        StringAssert.Contains("Not official evacuation guidance", context.StatusText.text);
        Assert.IsFalse(NavigationGuidanceController.AffectsGameplayRules);
        Assert.IsFalse(NpcEvacuationAgent.AffectsPlayerSuccessFailure);
        Assert.IsFalse(NpcEvacuationSpawner.AffectsPlayerSuccessFailure);
        AssertNoResultManagers();
        AssertNoChuoBaseMapScene();
    }

    private static bool HasNoResultManagers()
    {
        return Object.FindObjectsOfType<EvacuationGameManager>().Length == 0 &&
            Object.FindObjectsOfType<ResultPanelController>().Length == 0;
    }

    private static void AssertNoResultManagers()
    {
        Assert.AreEqual(0, Object.FindObjectsOfType<EvacuationGameManager>().Length);
        Assert.AreEqual(0, Object.FindObjectsOfType<ResultPanelController>().Length);
    }

    private static void AssertNoChuoBaseMapScene()
    {
        string activeScenePath = SceneManager.GetActiveScene().path.Replace("\\", "/");
        Assert.AreNotEqual("Assets/Scenes/Chuo_BaseMap.unity", activeScenePath);
    }
}
