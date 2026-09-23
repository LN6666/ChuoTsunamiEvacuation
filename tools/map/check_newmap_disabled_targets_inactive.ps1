param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$activePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_active_target_final_report.json"
$disabledPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_disabled_targets_final.json"
$active = Get-Content -LiteralPath $activePath -Raw | ConvertFrom-Json
$disabled = Get-Content -LiteralPath $disabledPath -Raw | ConvertFrom-Json
$failures = @()

if ([int]$active.activeTargetCount -lt 1) {
    $failures += "No active target flow is available."
}
if ([int]$active.disabledSelectableCount -ne 0) {
    $failures += "Active report says disabled selectable targets exist."
}

$badDisabled = @($disabled.disabledTargets | Where-Object {
    $_.activeInGame -or
    $_.markerBehavior -notmatch "disabled|not spawned" -or
    $_.greenFrameBehavior -notmatch "disabled|not spawned" -or
    $_.selectableBehavior -notmatch "disabled|not selectable" -or
    $_.routeBehavior -notmatch "disabled|not spawned|not active" -or
    $_.resultPanelBehavior -notmatch "cannot produce"
})
if ($badDisabled.Count -gt 0) {
    $failures += "Disabled records expose active behavior: $($badDisabled.id -join ', ')"
}

if (-not [bool]$disabled.preflightMustFailIfDisabledTargetActive) {
    $failures += "Disabled target report does not require preflight failure on active disabled targets."
}

if ([int]$disabled.disabledTargetCount -ne [int]$active.disabledTargetCount) {
    $failures += "Active and disabled reports disagree on disabled target count."
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Disabled targets are inactive, unselectable, and hidden from green frames/routes/results."
exit 0
