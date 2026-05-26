param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"
$path = Join-Path $ProjectRoot "Assets\Data\P10\newmap_target_remap_status.json"

if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
    Write-Host "[FAIL] Missing target remap status JSON: $path"
    exit 1
}

$status = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
$badOldActive = @($status.targets | Where-Object {
    ($_.sourcePhase -eq "P3/P4" -or $_.sourcePhase -eq "P5") -and $_.activeInGame
})
$badNonOfficialSafety = @($status.targets | Where-Object {
    $_.activeInGame -and -not $_.official -and ($_.safeApprovedByDefault -eq $true)
})

if ($badOldActive.Count -gt 0) {
    Write-Host "[FAIL] Old-map targets are active:"
    $badOldActive | ForEach-Object { Write-Host "  $($_.sourcePhase) $($_.id) $($_.name)" }
    exit 1
}

if ($badNonOfficialSafety.Count -gt 0) {
    Write-Host "[FAIL] Non-official active targets are marked safe-approved by default:"
    $badNonOfficialSafety | ForEach-Object { Write-Host "  $($_.id) $($_.name)" }
    exit 1
}

Write-Host "[PASS] Missing old-map targets are disabled and non-official targets are not safe-approved by default."
exit 0
