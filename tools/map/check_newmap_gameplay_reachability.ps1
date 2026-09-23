param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$matrixPath = Join-Path $root "Assets\Data\P10\newmap_gameplay_self_audit_matrix.json"
$officialPath = Join-Path $root "Assets\Data\P10\newmap_official_shelter_gameplay_check.json"
$nonOfficialPath = Join-Path $root "Assets\Data\P10\newmap_non_official_candidate_gameplay_check.json"
$modePath = Join-Path $root "Assets\Data\P10\newmap_mode_gameplay_check.json"
$interactionPath = Join-Path $root "Assets\Data\P10\newmap_interaction_flow_check.json"
$outcomePath = Join-Path $root "Assets\Data\P10\newmap_outcome_scenario_check.json"

$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$official = Get-Content -LiteralPath $officialPath -Raw | ConvertFrom-Json
$nonOfficial = Get-Content -LiteralPath $nonOfficialPath -Raw | ConvertFrom-Json
$mode = Get-Content -LiteralPath $modePath -Raw | ConvertFrom-Json
$interaction = Get-Content -LiteralPath $interactionPath -Raw | ConvertFrom-Json
$outcome = Get-Content -LiteralPath $outcomePath -Raw | ConvertFrom-Json

$allowed = @(
    "completed_on_new_chuo_basemap",
    "completed_with_documented_runtime_proxy",
    "disabled_missing_from_new_map",
    "blocked_needs_user_map_asset",
    "failed"
)

$failures = @()
foreach ($feature in @($matrix.features)) {
    if ($allowed -notcontains [string]$feature.finalStatus) {
        $failures += "Invalid feature status $($feature.featureId): $($feature.finalStatus)"
    }

    if ($feature.activeOnChuoBaseMap -and -not $feature.runtimeReachable) {
        $failures += "Active feature is not runtime reachable: $($feature.featureId)"
    }

    $testedByCount = @($feature.testedBy).Count
    if ($feature.runtimeReachable -and $testedByCount -eq 0) {
        $failures += "Runtime feature has no test evidence: $($feature.featureId)"
    }
}

if ([int]$matrix.activeOfficialShelterCount -lt 1 -or [int]$official.activeOfficialShelterCount -lt 1) {
    $failures += "No active official target flow evidence."
}

if ([int]$matrix.activeNonOfficialTrainingTargetCount -lt 1 -or [int]$nonOfficial.activeNonOfficialTrainingTargetCount -lt 1) {
    $failures += "No active non-official target flow evidence."
}

if (-not [bool]$nonOfficial.allActiveNonOfficialHaveWarningRequired) {
    $failures += "Non-official warning requirement missing."
}

if ([bool]$nonOfficial.allActiveNonOfficialAreOfficial) {
    $failures += "Non-official candidates are incorrectly marked official."
}

if ($mode.tourism.tsunamiWarning -ne "disabled" -or $mode.evacuation.twoStageTsunami -ne "enabled") {
    $failures += "Tourism/Evacuation mode policy is not represented."
}

if ($interaction.finalStatus -notin $allowed -or -not [string]$interaction.testOnlyHelper) {
    $failures += "Interaction flow check is incomplete."
}

$requiredScenarios = @(
    "success_official_shelter",
    "success_non_official_candidate_with_warning",
    "crowd_delay_success_or_failure",
    "entrance_blocked_failure",
    "safe_floor_unavailable_failure",
    "collapse_debris_exposure_failure",
    "collapse_disabled_success",
    "tsunami_front_failure",
    "tourism_free_roam_no_failure",
    "disabled_target_not_selectable"
)
foreach ($id in $requiredScenarios) {
    $match = @($outcome.scenarios | Where-Object { $_.scenarioId -eq $id })
    if ($match.Count -eq 0) {
        $failures += "Missing outcome scenario: $id"
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Gameplay reachability audit is strict and complete."
exit 0
