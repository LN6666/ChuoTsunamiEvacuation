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
$allowedOfficialStatuses = @(
    "official_shelters_recovered",
    "no_verified_official_shelter_anchor_found",
    "blocked_missing_coordinate_transform",
    "blocked_missing_official_source_data"
)
$allowedRouteClassifications = @(
    "validated_on_new_map",
    "proxy_guidance_only",
    "disabled_out_of_map",
    "blocked_transform_unknown",
    "invalid_geometry"
)

$requiredJson = @(
    "Assets\Data\P10\newmap_official_shelter_anchor_report.json",
    "Assets\Data\P10\newmap_coordinate_transform_validation.json",
    "Assets\Data\P10\newmap_route_geometry_validation.json",
    "Assets\Data\P10\newmap_p3_p4_recovery_final.json",
    "Assets\Data\P10\newmap_p5_route_candidate_hardening.json",
    "Assets\Data\P10\newmap_active_target_final_report.json",
    "Assets\Data\P10\newmap_spike_reduction_report.json",
    "Assets\Data\P10\newmap_scenario_sanity_after_hardening.json",
    "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json"
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
        if (($json.PSObject.Properties.Name -contains "officialShelterStatus") -and ($allowedOfficialStatuses -notcontains $json.officialShelterStatus)) {
            $failures += "Invalid officialShelterStatus in $relativePath`: $($json.officialShelterStatus)"
        }
    }
    catch {
        $failures += "Invalid JSON: $relativePath - $($_.Exception.Message)"
    }

    $text = Get-Content -LiteralPath $path -Raw
    foreach ($forbidden in @(("basic " + "complete"), ("mostly " + "complete"), ("proxy" + "-ready"), "maybe works", ("should " + "work"), "not tested but likely")) {
        if ($text -match [regex]::Escape($forbidden)) {
            $failures += "$relativePath contains forbidden wording/status: $forbidden"
        }
    }
}

$routePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_route_geometry_validation.json"
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

Write-Host "[PASS] NewMap official/route hardening JSON files are present, parseable, and use strict statuses."
exit 0
