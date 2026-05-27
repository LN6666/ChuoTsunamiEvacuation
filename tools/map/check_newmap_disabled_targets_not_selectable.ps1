param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$disabledPath = Join-Path $root "Assets\Data\P10\newmap_disabled_targets_hard_final.json"
$activePath = Join-Path $root "Assets\Data\P10\newmap_active_target_final_report.json"
$outcomePath = Join-Path $root "Assets\Data\P10\newmap_outcome_scenario_check.json"

$disabled = Get-Content -LiteralPath $disabledPath -Raw | ConvertFrom-Json
$active = Get-Content -LiteralPath $activePath -Raw | ConvertFrom-Json
$outcome = Get-Content -LiteralPath $outcomePath -Raw | ConvertFrom-Json

$failures = @()
if ([int]$active.disabledSelectableCount -ne 0) {
    $failures += "Active target report says disabledSelectableCount=$($active.disabledSelectableCount)"
}

if ([int]$active.disabledGreenFrameCount -ne 0) {
    $failures += "Active target report says disabledGreenFrameCount=$($active.disabledGreenFrameCount)"
}

if ([int]$disabled.disabledTargetCount -lt 1) {
    $failures += "Disabled target report is empty."
}

$scenario = @($outcome.scenarios | Where-Object { $_.scenarioId -eq "disabled_target_not_selectable" } | Select-Object -First 1)
if ($scenario.Count -eq 0) {
    $failures += "disabled_target_not_selectable scenario is missing."
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Disabled targets are documented as non-selectable and inactive."
exit 0
