using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class P6CNavigationNpcIntegrationPlayModeTests
{
    private readonly List<GameObject> objectsToDestroy = new List<GameObject>();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject objectToDestroy in objectsToDestroy)
        {
            if (objectToDestroy != null)
            {
                Object.Destroy(objectToDestroy);
            }
        }

        objectsToDestroy.Clear();
        yield return null;
    }

    [UnityTest]
    public IEnumerator NavigationAndNpcComponentsCoexistWithoutSharedResultState()
    {
        GameObject playerObject = CreateTrackedObject("P6C_Player_Test");
        playerObject.transform.position = Vector3.zero;

        BuildingShelter shelter = CreateShelterTarget();

        NavigationGuidanceDisplay display = CreateDisplay(out Text targetText, out Text statusText);
        NavigationGuidanceController guidanceController =
            CreateTrackedObject("P6C_NavigationGuidanceController_Test").AddComponent<NavigationGuidanceController>();
        guidanceController.SetPlayerTransform(playerObject.transform);
        guidanceController.SetTargetShelter(shelter);
        guidanceController.SetDisplay(display);
        guidanceController.SetWalkingSpeedMetersPerSecond(1.2f);

        NpcEvacuationAgent npcAgent =
            CreateTrackedObject("P6C_NpcAgent_Test").AddComponent<NpcEvacuationAgent>();
        npcAgent.ConfigureMovement(2f, 0.1f);
        npcAgent.ConfigureTargets(new[]
        {
            NpcEvacuationTargetInfo.FromBuildingShelter(shelter)
        }, true);

        NavigationGuidanceResult initialGuidance = guidanceController.RefreshGuidance();
        NpcEvacuationState npcStateBeforeNavigationRefresh = npcAgent.CurrentState;
        NpcEvacuationTargetInfo npcTargetBeforeNavigationRefresh = npcAgent.CurrentTarget;

        guidanceController.RefreshGuidance();

        Assert.AreEqual(npcStateBeforeNavigationRefresh, npcAgent.CurrentState);
        Assert.AreSame(npcTargetBeforeNavigationRefresh, npcAgent.CurrentTarget);

        float guidanceDistanceBeforeNpcMovement = initialGuidance.distanceMeters;
        string guidanceStatusBeforeNpcMovement = initialGuidance.statusText;

        npcAgent.Tick(0.5f);
        NavigationGuidanceResult guidanceAfterNpcMovement = guidanceController.RefreshGuidance();

        Assert.IsTrue(initialGuidance.hasTarget);
        Assert.AreEqual("P6C Shared Shelter", targetText.text);
        Assert.AreEqual(guidanceDistanceBeforeNpcMovement, guidanceAfterNpcMovement.distanceMeters, 0.0001f);
        Assert.AreEqual(guidanceStatusBeforeNpcMovement, guidanceAfterNpcMovement.statusText);
        Assert.AreEqual(NpcEvacuationState.MovingToTarget, npcAgent.CurrentState);
        Assert.Greater(npcAgent.transform.position.z, 0f);
        StringAssert.Contains("Not official navigation", statusText.text);
        StringAssert.Contains("Not official evacuation guidance", statusText.text);

        Assert.IsFalse(NavigationGuidanceController.AffectsGameplayRules);
        Assert.IsFalse(NpcEvacuationAgent.AffectsPlayerSuccessFailure);
        Assert.AreEqual(0, Object.FindObjectsOfType<EvacuationGameManager>().Length);
        Assert.AreEqual(0, Object.FindObjectsOfType<ResultPanelController>().Length);

        yield return null;
    }

    private BuildingShelter CreateShelterTarget()
    {
        GameObject shelterObject = CreateTrackedObject("P6C_SharedShelter_Test");
        shelterObject.transform.position = new Vector3(0f, 0f, 10f);

        BuildingShelter shelter = shelterObject.AddComponent<BuildingShelter>();
        shelter.ApplyShelterData(new ShelterDataLoader.ShelterData
        {
            shelterId = "p6c_shared_shelter",
            shelterName = "P6C Shared Shelter",
            shelterRank = "A",
            isOfficialShelter = true,
            canEnter = true,
            entryDelaySeconds = 0f,
            climbTimeSeconds = 8f,
            crowdingDelaySeconds = 0f,
            sourceType = "test"
        });

        return shelter;
    }

    private NavigationGuidanceDisplay CreateDisplay(out Text targetText, out Text statusText)
    {
        GameObject displayObject = CreateTrackedObject("P6C_NavigationGuidanceDisplay_Test");
        NavigationGuidanceDisplay display = displayObject.AddComponent<NavigationGuidanceDisplay>();

        GameObject arrowObject = CreateTrackedObject("P6C_Arrow_Test", typeof(RectTransform));
        targetText = CreateText("P6C_TargetText_Test");
        Text distanceText = CreateText("P6C_DistanceText_Test");
        Text estimatedTimeText = CreateText("P6C_EstimatedTimeText_Test");
        statusText = CreateText("P6C_StatusText_Test");

        arrowObject.transform.SetParent(displayObject.transform, false);
        targetText.transform.SetParent(displayObject.transform, false);
        distanceText.transform.SetParent(displayObject.transform, false);
        estimatedTimeText.transform.SetParent(displayObject.transform, false);
        statusText.transform.SetParent(displayObject.transform, false);

        display.BindForTests(
            arrowObject.GetComponent<RectTransform>(),
            targetText,
            distanceText,
            estimatedTimeText,
            statusText);
        return display;
    }

    private Text CreateText(string name)
    {
        return CreateTrackedObject(name, typeof(RectTransform)).AddComponent<Text>();
    }

    private GameObject CreateTrackedObject(string name, params System.Type[] components)
    {
        GameObject gameObject = components == null || components.Length == 0
            ? new GameObject(name)
            : new GameObject(name, components);
        objectsToDestroy.Add(gameObject);
        return gameObject;
    }
}
