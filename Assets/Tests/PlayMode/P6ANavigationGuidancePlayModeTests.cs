using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class P6ANavigationGuidancePlayModeTests
{
    private GameObject controllerObject;
    private GameObject playerObject;
    private GameObject targetObject;
    private GameObject displayObject;
    private GameObject arrowObject;

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        DestroyIfPresent(controllerObject);
        DestroyIfPresent(playerObject);
        DestroyIfPresent(targetObject);
        DestroyIfPresent(displayObject);
        DestroyIfPresent(arrowObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator GuidanceControllerCanBeCreatedAndUpdatesIndicator()
    {
        playerObject = new GameObject("P6A_Player_Test");
        playerObject.transform.position = Vector3.zero;
        targetObject = new GameObject("P6A_Target_Test");
        targetObject.transform.position = new Vector3(0f, 0f, 10f);

        NavigationGuidanceDisplay display = CreateDisplay(out Text distanceText, out Text statusText);
        NavigationGuidanceController controller = CreateController(display);
        controller.SetPlayerTransform(playerObject.transform);
        controller.SetTargetTransform(targetObject.transform);

        NavigationGuidanceResult result = controller.RefreshGuidance();
        yield return null;

        Assert.NotNull(result);
        Assert.IsTrue(result.hasTarget);
        Assert.AreEqual(10f, result.distanceMeters, 0.0001f);
        Assert.IsTrue(arrowObject.activeSelf);
        StringAssert.Contains("10", distanceText.text);
        StringAssert.Contains("Not official navigation", statusText.text);

        playerObject.transform.position = new Vector3(0f, 0f, 5f);
        Assert.DoesNotThrow(() => controller.RefreshGuidance());
        Assert.AreEqual(5f, controller.LastGuidance.distanceMeters, 0.0001f);
    }

    [UnityTest]
    public IEnumerator GuidanceIsDisplayOnlyAndDoesNotRequireGameplayManagers()
    {
        playerObject = new GameObject("P6A_Player_DisplayOnly_Test");
        targetObject = new GameObject("P6A_Shelter_DisplayOnly_Test");
        targetObject.transform.position = new Vector3(3f, 0f, 4f);
        BuildingShelter shelter = targetObject.AddComponent<BuildingShelter>();
        shelter.ApplyShelterData(new ShelterDataLoader.ShelterData
        {
            shelterId = "display_only_shelter",
            shelterName = "Display Only Shelter",
            shelterRank = "A",
            isOfficialShelter = true,
            canEnter = true,
            climbTimeSeconds = 10f
        });

        NavigationGuidanceDisplay display = CreateDisplay(out Text distanceText, out Text statusText);
        NavigationGuidanceController controller = CreateController(display);
        controller.SetPlayerTransform(playerObject.transform);
        controller.SetTargetShelter(shelter);

        NavigationGuidanceResult result = controller.RefreshGuidance();
        yield return null;

        Assert.IsFalse(NavigationGuidanceController.AffectsGameplayRules);
        Assert.IsTrue(result.hasTarget);
        Assert.AreEqual("Display Only Shelter", result.targetName);
        StringAssert.Contains("5", distanceText.text);
        StringAssert.Contains("Not official evacuation guidance", statusText.text);
    }

    private NavigationGuidanceController CreateController(NavigationGuidanceDisplay display)
    {
        controllerObject = new GameObject("P6A_NavigationGuidanceController_Test");
        NavigationGuidanceController controller = controllerObject.AddComponent<NavigationGuidanceController>();
        controller.SetDisplay(display);
        controller.SetWalkingSpeedMetersPerSecond(1.2f);
        return controller;
    }

    private NavigationGuidanceDisplay CreateDisplay(out Text distanceText, out Text statusText)
    {
        displayObject = new GameObject("P6A_NavigationGuidanceDisplay_Test");
        NavigationGuidanceDisplay display = displayObject.AddComponent<NavigationGuidanceDisplay>();

        arrowObject = new GameObject("P6A_Arrow_Test", typeof(RectTransform));
        Text targetText = CreateText("P6A_TargetText_Test");
        distanceText = CreateText("P6A_DistanceText_Test");
        Text estimatedTimeText = CreateText("P6A_EstimatedTimeText_Test");
        statusText = CreateText("P6A_StatusText_Test");

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

    private static Text CreateText(string name)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        return textObject.AddComponent<Text>();
    }

    private static void DestroyIfPresent(GameObject gameObject)
    {
        if (gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }
}
