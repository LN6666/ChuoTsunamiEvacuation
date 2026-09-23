param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

$required = @(
    "Assets\Data\P10\newmap_gameplay_self_audit_matrix.json",
    "Assets\Data\P10\newmap_official_shelter_gameplay_check.json",
    "Assets\Data\P10\newmap_non_official_candidate_gameplay_check.json",
    "Assets\Data\P10\newmap_route_gameplay_check.json",
    "Assets\Data\P10\newmap_mode_gameplay_check.json",
    "Assets\Data\P10\newmap_interaction_flow_check.json",
    "Assets\Data\P10\newmap_outcome_scenario_check.json",
    "Assets\Data\P10\newmap_ui_ux_runtime_check.json",
    "Assets\Data\P10\newmap_gameplay_spike_quickfix.json",
    "Assets\Data\P10\newmap_gameplay_self_audit_player_report.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json",
    "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json"
)

$failures = @()
foreach ($relative in $required) {
    $path = Join-Path $root $relative
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures += "Missing $relative"
        continue
    }

    try {
        Get-Content -LiteralPath $path -Raw | ConvertFrom-Json | Out-Null
    }
    catch {
        $failures += "Invalid JSON $relative : $($_.Exception.Message)"
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Gameplay self-audit JSON files are present and valid."
exit 0
