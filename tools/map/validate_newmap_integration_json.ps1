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

$requiredJson = @(
    "Assets\Data\P10\newmap_full_integration_audit.json",
    "Assets\Data\P10\newmap_player_grounding_status.json",
    "Assets\Data\P10\newmap_game_modes_config.json",
    "Assets\Data\P10\newmap_ui_status.json",
    "Assets\Data\P10\newmap_humanoid_visuals_status.json",
    "Assets\Data\P10\newmap_target_remap_status.json",
    "Assets\Data\P10\newmap_green_frame_targets.json",
    "Assets\Data\P10\newmap_p5_route_candidate_status.json",
    "Assets\Data\P10\newmap_p6_nav_npc_status.json",
    "Assets\Data\P10\newmap_p8_hazard_front_status.json",
    "Assets\Data\P10\newmap_p9_gameplay_status.json",
    "Assets\Data\P10\newmap_collapse_debris_status.json",
    "Assets\Data\P10\newmap_performance_results.json",
    "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json",
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
        Get-Content -LiteralPath $path -Raw | ConvertFrom-Json | Out-Null
    }
    catch {
        $failures += "Invalid JSON: $relativePath - $($_.Exception.Message)"
    }
}

$matrixPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json"
if (Test-Path -LiteralPath $matrixPath -PathType Leaf) {
    $matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
    foreach ($entry in $matrix.entries) {
        if ($allowedStatuses -notcontains $entry.finalStatus) {
            $failures += "Invalid completion status for $($entry.item): $($entry.finalStatus)"
        }
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] NewMap integration JSON files are present and valid."
exit 0
