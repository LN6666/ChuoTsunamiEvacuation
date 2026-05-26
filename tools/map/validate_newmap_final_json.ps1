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
    "Assets\Data\P10\newmap_final_stabilization_triage.json",
    "Assets\Data\P10\newmap_player_camera_grounding_final.json",
    "Assets\Data\P10\newmap_active_target_final_report.json",
    "Assets\Data\P10\newmap_p3_p4_shelter_loading_final.json",
    "Assets\Data\P10\newmap_p5_route_candidate_final.json",
    "Assets\Data\P10\newmap_p6_nav_npc_final.json",
    "Assets\Data\P10\newmap_p8_hazard_front_final.json",
    "Assets\Data\P10\newmap_p9_gameplay_final.json",
    "Assets\Data\P10\newmap_p10_ui_modes_final.json",
    "Assets\Data\P10\newmap_disabled_targets_final.json",
    "Assets\Data\P10\newmap_performance_final_gate.json",
    "Assets\Data\P10\newmap_temp_player_final_pre.json",
    "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json",
    "Assets\Data\P10\newmap_manual_playtest_checklist.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json"
)

$failures = @()
$parsed = @{}
foreach ($relativePath in $requiredJson) {
    $path = Join-Path $ProjectRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures += "Missing JSON: $relativePath"
        continue
    }

    try {
        $parsed[$relativePath] = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    }
    catch {
        $failures += "Invalid JSON: $relativePath - $($_.Exception.Message)"
    }
}

$matrixKey = "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json"
if ($parsed.ContainsKey($matrixKey)) {
    $matrix = $parsed[$matrixKey]
    if (-not $matrix.entries) {
        $failures += "Completion matrix has no entries."
    }

    foreach ($entry in @($matrix.entries)) {
        foreach ($field in @("feature", "phase", "activeOnNewMap", "finalStatus", "evidence", "testName", "disabledReason", "blocker", "nextAction")) {
            if (-not ($entry.PSObject.Properties.Name -contains $field)) {
                $failures += "Completion matrix entry missing $field for $($entry.feature)"
            }
        }

        if ($allowedStatuses -notcontains $entry.finalStatus) {
            $failures += "Invalid completion status for $($entry.feature): $($entry.finalStatus)"
        }
    }
}

foreach ($relativePath in $requiredJson) {
    $path = Join-Path $ProjectRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        continue
    }

    $text = Get-Content -LiteralPath $path -Raw
    $forbiddenTerms = @(
        ("basic " + "complete"),
        ("mostly " + "complete"),
        ("proxy" + "-ready")
    )
    foreach ($forbidden in $forbiddenTerms) {
        if ($text -match [regex]::Escape($forbidden)) {
            $failures += "$relativePath contains forbidden wording/status: $forbidden"
        }
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] NewMap final JSON files are present, parseable, and use strict statuses."
exit 0
