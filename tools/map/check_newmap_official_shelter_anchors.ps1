param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$path = Join-Path $ProjectRoot "Assets\Data\P10\newmap_official_shelter_anchor_report.json"
if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
    Write-Host "[FAIL] Missing official shelter anchor report: $path"
    exit 1
}

$report = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
$failures = @()

$allowedStatus = @(
    "official_shelters_recovered",
    "no_verified_official_shelter_anchor_found",
    "blocked_missing_coordinate_transform",
    "blocked_missing_official_source_data"
)
if ($allowedStatus -notcontains $report.officialShelterStatus) {
    $failures += "Invalid officialShelterStatus: $($report.officialShelterStatus)"
}

$active = @($report.records | Where-Object { $_.activeInGame })
if ([int]$report.activeOfficialShelterCount -ne $active.Count) {
    $failures += "activeOfficialShelterCount does not match active records."
}

$badActive = @($active | Where-Object {
    -not $_.sceneExactGmlObjectFound -or
    $_.matchMethod -ne "contains" -or
    $_.matchConfidence -ne "high" -or
    $_.manualReviewNeeded -or
    [string]::IsNullOrWhiteSpace($_.plateauGmlId)
})
if ($badActive.Count -gt 0) {
    $failures += "Unverified official shelter is active: $($badActive.id -join ', ')"
}

$badDisabled = @($report.records | Where-Object {
    -not $_.activeInGame -and [string]::IsNullOrWhiteSpace($_.disabledReason)
})
if ($badDisabled.Count -gt 0) {
    $failures += "Disabled official records lack disabledReason: $($badDisabled.id -join ', ')"
}

if ($report.officialShelterStatus -eq "official_shelters_recovered" -and $active.Count -lt 1) {
    $failures += "Report claims official shelter recovery but has no active official records."
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Official shelter anchors are strict: active records require exact GML, high confidence, contains match, and no manual review."
exit 0
