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
    "Assets\Data\P10\newmap_completion_hardening_blockers.json",
    "Assets\Data\P10\newmap_active_target_hardening_report.json",
    "Assets\Data\P10\newmap_p3_p4_recovery_final.json",
    "Assets\Data\P10\newmap_p5_route_candidate_hardening.json",
    "Assets\Data\P10\newmap_p2_player_interaction_completion.json",
    "Assets\Data\P10\newmap_p6_npc_crowd_completion.json",
    "Assets\Data\P10\newmap_p8_tsunami_front_completion.json",
    "Assets\Data\P10\newmap_p9_scenario_completion.json",
    "Assets\Data\P10\newmap_p10_ui_mode_completion.json",
    "Assets\Data\P10\newmap_disabled_targets_hard_final.json",
    "Assets\Data\P10\newmap_performance_hardening_final.json",
    "Assets\Data\P10\newmap_hardening_player_build_report.json",
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
    }
    catch {
        $failures += "Invalid JSON: $relativePath - $($_.Exception.Message)"
    }

    $text = Get-Content -LiteralPath $path -Raw
    foreach ($forbidden in @(("basic " + "complete"), ("mostly " + "complete"), ("proxy" + "-ready"), "maybe works", "should work", "not tested but likely")) {
        if ($text -match [regex]::Escape($forbidden)) {
            $failures += "$relativePath contains forbidden wording/status: $forbidden"
        }
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] NewMap hardening JSON files are present, parseable, and avoid vague completion wording."
exit 0
