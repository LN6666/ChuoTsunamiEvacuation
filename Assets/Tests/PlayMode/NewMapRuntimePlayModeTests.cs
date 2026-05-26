using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NewMapRuntimePlayModeTests
{
    [TearDown]
    public void TearDown()
    {
        foreach (NewMapRuntimeBootstrap bootstrap in Object.FindObjectsOfType<NewMapRuntimeBootstrap>())
        {
            Object.DestroyImmediate(bootstrap.gameObject);
        }

        foreach (NewMapPlayerController player in Object.FindObjectsOfType<NewMapPlayerController>())
        {
            Object.DestroyImmediate(player.gameObject);
        }

        foreach (Canvas canvas in Object.FindObjectsOfType<Canvas>())
        {
            Object.DestroyImmediate(canvas.gameObject);
        }
    }

    [UnityTest]
    public IEnumerator RuntimeBootstrapCreatesPlayerCameraAndPreventsFallThrough()
    {
        NewMapRuntimeBootstrap bootstrap = NewMapRuntimeBootstrap.CreateForCurrentScene();
        Assert.NotNull(bootstrap);

        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        Assert.NotNull(player);
        Assert.NotNull(controller);
        Assert.IsTrue(player.HasActiveCamera);

        controller.StartTourismMode();
        float startY = player.transform.position.y;
        yield return new WaitForSeconds(2f);
        Assert.Greater(player.transform.position.y, startY - 8f, "Player should not fall endlessly through the map/support proxy.");
    }

    [UnityTest]
    public IEnumerator RuntimeModesApplySpeedAndStaminaRules()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        Assert.NotNull(player);
        Assert.NotNull(controller);

        controller.SetWeather(NewMapWeatherPreset.ClearDay);
        controller.StartTourismMode();
        yield return null;
        Assert.AreEqual(2.0f, player.WalkSpeedMetersPerSecond, 0.001f);
        Assert.AreEqual(10.0f, player.SprintSpeedMetersPerSecond, 0.001f);
        Assert.IsFalse(player.StaminaEnabled);

        controller.StartEvacuationMode();
        yield return null;
        Assert.AreEqual(1.0f, player.WalkSpeedMetersPerSecond, 0.001f);
        Assert.AreEqual(5.0f, player.SprintSpeedMetersPerSecond, 0.001f);
        Assert.IsTrue(player.StaminaEnabled);

        controller.SetWeather(NewMapWeatherPreset.NightRain);
        yield return null;
        Assert.AreEqual(0.65f, player.WalkSpeedMetersPerSecond, 0.001f);
        Assert.AreEqual(3.25f, player.SprintSpeedMetersPerSecond, 0.001f);
    }
}
