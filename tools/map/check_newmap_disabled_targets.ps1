param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$targetReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_active_target_final_report.json"
$disabledReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_disabled_targets_final.json"

if (-not (Test-Path -LiteralPath $targetReportPath -PathType Leaf)) {
    Write-Host "[FAIL] Missing active target final report: $targetReportPath"
    exit 1
}

if (-not (Test-Path -LiteralPath $disabledReportPath -PathType Leaf)) {
    Write-Host "[FAIL] Missing disabled targets final report: $disabledReportPath"
    exit 1
}

$targetReport = Get-Content -LiteralPath $targetReportPath -Raw | ConvertFrom-Json
$disabledReport = Get-Content -LiteralPath $disabledReportPath -Raw | ConvertFrom-Json
$failures = @()

$disabledButActive = @($targetReport.targets | Where-Object {
    $_.activeInGame -and (
        -not [string]::IsNullOrWhiteSpace($_.disabledReason) -or
        $_.mapAnchorStatus -match "disabled|missing|out_of_map|no_verified"
    )
})
if ($disabledButActive.Count -gt 0) {
    $failures += "Disabled/out-of-map targets are still active: $($disabledButActive.id -join ', ')"
}

$disabledMarkerActive = @($disabledReport.disabledTargets | Where-Object {
    $_.activeInGame -or
    $_.markerBehavior -notmatch "disabled|not spawned" -or
    $_.greenFrameBehavior -notmatch "disabled|not spawned" -or
    $_.selectableBehavior -notmatch "disabled|not selectable" -or
    $_.routeBehavior -notmatch "disabled|not spawned|not active"
})
if ($disabledMarkerActive.Count -gt 0) {
    $failures += "Disabled targets have active marker/selectable/route behavior: $($disabledMarkerActive.id -join ', ')"
}

$falseSafety = @($targetReport.targets | Where-Object {
    $_.activeInGame -and $_.officialNonOfficial -eq "non-official" -and $_.warningBehavior -notmatch "Non-official"
})
if ($falseSafety.Count -gt 0) {
    $failures += "Active non-official targets lack non-official warning behavior: $($falseSafety.id -join ', ')"
}

if ([int]$targetReport.activeTargetCount -le 0) {
    $failures += "No active targets are available for manual test."
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Disabled/out-of-map targets are inactive and active non-official targets carry warnings."
exit 0
