using System.IO;
using NUnit.Framework;
using UnityEngine;

public class NewMapIntegrationConfigTests
{
    [Test]
    public void WeatherAndModeSpeedsMatchNewMapPolicy()
    {
        Assert.AreEqual(1.0f, NewMapRuntimeConstants.EvacuationWalkSpeed);
        Assert.AreEqual(5.0f, NewMapRuntimeConstants.EvacuationSprintSpeed);
        Assert.AreEqual(2.0f, NewMapRuntimeConstants.TourismWalkSpeed);
        Assert.AreEqual(10.0f, NewMapRuntimeConstants.TourismSprintSpeed);
        Assert.AreEqual(1.0f, NewMapRuntimeConstants.GetWeatherModifier(NewMapWeatherPreset.ClearDay));
        Assert.AreEqual(0.75f, NewMapRuntimeConstants.GetWeatherModifier(NewMapWeatherPreset.RainyDay));
        Assert.AreEqual(0.85f, NewMapRuntimeConstants.GetWeatherModifier(NewMapWeatherPreset.NightClear));
        Assert.AreEqual(0.65f, NewMapRuntimeConstants.GetWeatherModifier(NewMapWeatherPreset.NightRain));
    }

    [Test]
    public void NewMapStatusFilesUseStrictCompletionStatuses()
    {
        string matrixPath = Path.Combine(Application.dataPath, "Data/P10/newmap_p2_p10_full_completion_matrix.json");
        Assert.IsTrue(File.Exists(matrixPath), "Completion matrix JSON must exist.");
        string matrix = File.ReadAllText(matrixPath);
        Assert.IsFalse(matrix.Contains("basic " + "complete"));
        Assert.IsFalse(matrix.Contains("mostly " + "done"));
        Assert.IsFalse(matrix.Contains("proxy" + "-ready"));
        StringAssert.Contains("completed_with_documented_runtime_proxy", matrix);
        StringAssert.Contains("disabled_missing_from_new_map", matrix);
    }

    [Test]
    public void MissingOldTargetsAreNotActive()
    {
        string targetStatusPath = Path.Combine(Application.dataPath, "Data/P10/newmap_target_remap_status.json");
        Assert.IsTrue(File.Exists(targetStatusPath), "Target remap status JSON must exist.");
        string status = File.ReadAllText(targetStatusPath);
        Assert.IsFalse(status.Contains("\"sourcePhase\":\"P5\"") && status.Contains("\"activeInGame\":true"));
        StringAssert.Contains("disabled_missing_from_new_map", status);
        StringAssert.Contains("newmap_proxy_safe_floor", status);
    }
}
