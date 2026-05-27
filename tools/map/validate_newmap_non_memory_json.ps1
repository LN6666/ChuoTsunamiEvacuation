param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$allowedStatuses = @(
    "completed_on_new_chuo_basemap",
    "completed_with_documented_runtime_proxy",
    "disabled_missing_from_new_map",
    "blocked_needs_user_map_asset",
    "failed"
)
$allowedCoordinateStatuses = @(
    "transform_validated_from_official_anchors",
    "transform_estimated_but_not_validated",
    "blocked_insufficient_control_points",
    "blocked_missing_source_coordinates",
    "blocked_high_residual_error",
    "blocked_missing_coordinate_transform"
)
$allowedRouteClassifications = @(
    "validated_on_new_chuo_basemap",
    "estimated_proxy_on_new_chuo_basemap",
    "disabled_out_of_new_map",
    "disabled_transform_unvalidated",
    "invalid_geometry"
)

$requiredJson = @(
    "Assets\Data\P10\newmap_official_shelter_anchor_final_check.json",
    "Assets\Data\P10\newmap_coordinate_transform_anchor_fit.json",
    "Assets\Data\P10\newmap_route_geometry_final_validation.json",
    "Assets\Data\P10\newmap_p5_route_candidate_final_status.json",
    "Assets\Data\P10\newmap_candidate_green_frame_final_status.json",
    "Assets\Data\P10\newmap_player_interaction_final_check.json",
    "Assets\Data\P10\newmap_npc_crowd_final_check.json",
    "Assets\Data\P10\newmap_two_stage_tsunami_final_check.json",
    "Assets\Data\P10\newmap_scenario_final_status.json",
    "Assets\Data\P10\newmap_spike_hardening_final.json",
    "Assets\Data\P10\newmap_no_memory_focus_performance.json",
    "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json",
    "Assets\Data\P10\newmap_manual_playtest_checklist.json"
)

$failures = @()
foreach ($relativePath in $requiredJson) {
    $path = Join-Path $ProjectRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures += "Missing JSON: $relativePath"
        continue
    }

    try {
        $json = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
        if (($json.PSObject.Properties.Name -contains "finalStatus") -and ($allowedStatuses -notcontains $json.finalStatus)) {
            $failures += "Invalid finalStatus in $relativePath`: $($json.finalStatus)"
        }
        if (($json.PSObject.Properties.Name -contains "coordinateTransformStatus") -and ($allowedCoordinateStatuses -notcontains $json.coordinateTransformStatus)) {
            $failures += "Invalid coordinateTransformStatus in $relativePath`: $($json.coordinateTransformStatus)"
        }
    }
    catch {
        $failures += "Invalid JSON: $relativePath - $($_.Exception.Message)"
    }

    $text = Get-Content -LiteralPath $path -Raw
    foreach ($forbidden in @(("basic " + "complete"), ("mostly " + "done"), ("proxy" + "-ready"), "not tested but likely")) {
        if ($text -match [regex]::Escape($forbidden)) {
            $failures += "$relativePath contains forbidden wording/status: $forbidden"
        }
    }
}

$routePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_route_geometry_final_validation.json"
if (Test-Path -LiteralPath $routePath -PathType Leaf) {
    $routeReport = Get-Content -LiteralPath $routePath -Raw | ConvertFrom-Json
    foreach ($record in @($routeReport.records)) {
        if ($allowedRouteClassifications -notcontains $record.classification) {
            $failures += "Invalid route classification for $($record.routeId): $($record.classification)"
        }
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] NewMap non-memory final JSON files are present, parseable, and use strict statuses."
exit 0
