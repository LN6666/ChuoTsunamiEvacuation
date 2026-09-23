param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$path = Join-Path $ProjectRoot "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json"
if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
    Write-Host "[FAIL] Missing completion matrix JSON: $path"
    exit 1
}

$allowedStatuses = @(
    "completed_on_new_chuo_basemap",
    "completed_with_documented_runtime_proxy",
    "disabled_missing_from_new_map",
    "blocked_needs_user_map_asset",
    "failed"
)

$matrix = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
$text = Get-Content -LiteralPath $path -Raw
$failures = @()

foreach ($forbidden in @(("basic " + "complete"), ("mostly " + "complete"), ("proxy" + "-ready"), "maybe works", "should work")) {
    if ($text -match [regex]::Escape($forbidden)) {
        $failures += "Completion matrix contains forbidden wording/status: $forbidden"
    }
}

foreach ($phase in @("P2", "P3/P4", "P5", "P6", "P8", "P9", "P10")) {
    if (-not @($matrix.entries | Where-Object { $_.phase -eq $phase })) {
        $failures += "Completion matrix missing phase: $phase"
    }
}

foreach ($entry in @($matrix.entries)) {
    foreach ($field in @("phase", "feature", "finalStatus", "activeOnNewMap", "targetUsed", "testEvidence", "disabledReason", "blocker", "nextAction")) {
        if (-not ($entry.PSObject.Properties.Name -contains $field)) {
            $failures += "Entry for $($entry.phase) $($entry.feature) missing field: $field"
        }
    }

    if ($allowedStatuses -notcontains $entry.finalStatus) {
        $failures += "Invalid status for $($entry.phase) $($entry.feature): $($entry.finalStatus)"
    }
}

if ([int]$matrix.activeTargetCount -lt 1) {
    $failures += "Completion matrix reports no active target flow."
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] NewMap final matrix uses strict statuses and hardening evidence fields."
exit 0
