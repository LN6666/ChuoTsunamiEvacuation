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

    [Test]
    public void NonMemoryFinalReportsRecordAnchorFitAndRouteValidation()
    {
        string anchorPath = Path.Combine(Application.dataPath, "Data/P10/newmap_official_shelter_anchor_final_check.json");
        string transformPath = Path.Combine(Application.dataPath, "Data/P10/newmap_coordinate_transform_anchor_fit.json");
        string routePath = Path.Combine(Application.dataPath, "Data/P10/newmap_route_geometry_final_validation.json");
        Assert.IsTrue(File.Exists(anchorPath), "Official anchor final check JSON must exist.");
        Assert.IsTrue(File.Exists(transformPath), "Coordinate transform anchor-fit JSON must exist.");
        Assert.IsTrue(File.Exists(routePath), "Route geometry final validation JSON must exist.");

        string anchor = File.ReadAllText(anchorPath);
        string transform = File.ReadAllText(transformPath);
        string route = File.ReadAllText(routePath);

        StringAssert.Contains("\"activeOfficialShelterCount\": 15", anchor);
        StringAssert.Contains("\"activeOfficialCountEqualsVerifiedAnchors\": true", anchor);
        StringAssert.Contains("transform_validated_from_official_anchors", transform);
        StringAssert.Contains("\"controlPointCount\": 15", transform);
        StringAssert.Contains("\"validatedOnNewChuoBaseMapCount\": 60", route);
        StringAssert.Contains("\"officialRouteClaimCount\": 0", route);
        StringAssert.Contains("not a GIS-grade", transform);
    }

    [Test]
    public void NonMemoryFinalReportsKeepRoutesAndDisabledTargetsHonest()
    {
        string routePath = Path.Combine(Application.dataPath, "Data/P10/newmap_route_geometry_final_validation.json");
        string p5Path = Path.Combine(Application.dataPath, "Data/P10/newmap_p5_route_candidate_final_status.json");
        string disabledPath = Path.Combine(Application.dataPath, "Data/P10/newmap_disabled_targets_final.json");
        Assert.IsTrue(File.Exists(routePath), "Route validation JSON must exist.");
        Assert.IsTrue(File.Exists(p5Path), "P5 final status JSON must exist.");
        Assert.IsTrue(File.Exists(disabledPath), "Disabled-target final JSON must exist.");

        string route = File.ReadAllText(routePath);
        string p5 = File.ReadAllText(p5Path);
        string disabled = File.ReadAllText(disabledPath);

        Assert.IsFalse(route.Contains("\"isOfficialEvacuationRoute\": true"), "No route record may claim official evacuation route status.");
        StringAssert.Contains("\"runtimeOldRouteLineSpawnCount\": 0", route);
        StringAssert.Contains("not an official evacuation route", p5);
        StringAssert.Contains("\"preflightMustFailIfDisabledTargetActive\": true", disabled);
        Assert.IsFalse(disabled.Contains("\"activeInGame\": true"), "Disabled-target report must not contain active records.");
    }

    [Test]
    public void RemainingHardeningRecoversNonOfficialCandidatesWithWarnings()
    {
        string recoveryPath = Path.Combine(Application.dataPath, "Data/P10/newmap_non_official_candidate_recovery.json");
        string activePath = Path.Combine(Application.dataPath, "Data/P10/newmap_active_target_final_report.json");
        string greenPath = Path.Combine(Application.dataPath, "Data/P10/newmap_candidate_green_frame_final_status.json");
        Assert.IsTrue(File.Exists(recoveryPath), "Non-official candidate recovery JSON must exist.");
        Assert.IsTrue(File.Exists(activePath), "Active target report JSON must exist.");
        Assert.IsTrue(File.Exists(greenPath), "Green-frame status JSON must exist.");

        string recovery = File.ReadAllText(recoveryPath);
        string active = File.ReadAllText(activePath);
        string green = File.ReadAllText(greenPath);

        StringAssert.Contains("\"totalCandidateRecordsLoaded\": 110", recovery);
        StringAssert.Contains("\"finalActiveNonOfficialCandidateCount\": 78", recovery);
        StringAssert.Contains("\"activeExactGmlAnchorCount\": 61", recovery);
        StringAssert.Contains("\"activeNearestBuildingAnchorCount\": 3", recovery);
        StringAssert.Contains("\"activeCoordinateProxyAnchorCount\": 14", recovery);
        StringAssert.Contains("\"disabledOutOfNewMapCount\": 32", recovery);
        StringAssert.Contains("\"nonOfficialWarningRequired\": true", recovery);
        StringAssert.Contains("\"safeApprovedByDefault\": false", recovery);
        Assert.IsFalse(recovery.Contains("\"isOfficialShelter\": true"), "Recovered non-official candidates must not become official shelters.");

        StringAssert.Contains("\"activeNonOfficialHumanitarianCandidateCount\": 78", active);
        StringAssert.Contains("\"activeNonOfficialTrainingTargetCount\": 82", active);
        StringAssert.Contains("\"nonOfficialTargetLabeledOfficialCount\": 0", active);
        StringAssert.Contains("\"disabledSelectableCount\": 0", active);
        StringAssert.Contains("\"resultPanelWarningTextExists\": true", green);
    }
}
