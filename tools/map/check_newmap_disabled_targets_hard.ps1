param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$activePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_active_target_final_report.json"
$disabledPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_disabled_targets_final.json"
$hardPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_disabled_targets_hard_final.json"

foreach ($path in @($activePath, $disabledPath, $hardPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing disabled-target validation input: $path"
        exit 1
    }
}

$activeReport = Get-Content -LiteralPath $activePath -Raw | ConvertFrom-Json
$disabledReport = Get-Content -LiteralPath $disabledPath -Raw | ConvertFrom-Json
$hardReport = Get-Content -LiteralPath $hardPath -Raw | ConvertFrom-Json
$failures = @()

$disabledButActive = @($activeReport.targets | Where-Object {
    $_.activeInGame -and (
        -not [string]::IsNullOrWhiteSpace($_.disabledReason) -or
        $_.mapAnchorStatus -match "disabled|missing|out_of_map|no_verified"
    )
})
if ($disabledButActive.Count -gt 0) {
    $failures += "Disabled/out-of-map targets are active: $($disabledButActive.id -join ', ')"
}

$badDisabledRows = @($disabledReport.disabledTargets | Where-Object {
    $_.activeInGame -or
    $_.markerBehavior -notmatch "disabled|not spawned" -or
    $_.greenFrameBehavior -notmatch "disabled|not spawned" -or
    $_.selectableBehavior -notmatch "disabled|not selectable" -or
    $_.routeBehavior -notmatch "disabled|not spawned|not active" -or
    $_.resultPanelBehavior -notmatch "cannot produce"
})
if ($badDisabledRows.Count -gt 0) {
    $failures += "Disabled records have active behavior: $($badDisabledRows.id -join ', ')"
}

if ([int]$hardReport.disabledTargetCount -ne [int]$disabledReport.disabledTargetCount) {
    $failures += "Hard disabled count does not match source disabled count."
}

if (-not [bool]$hardReport.preflightMustFailIfDisabledTargetActive) {
    $failures += "Hard disabled report does not require preflight failure for active disabled targets."
}

if ([int]$activeReport.activeTargetCount -lt 1) {
    $failures += "No active target flow is available."
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Disabled targets are inactive, unselectable, hidden from routes/green frames, and excluded from result success."
exit 0
