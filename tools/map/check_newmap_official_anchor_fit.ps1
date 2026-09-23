param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$anchorPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_official_shelter_anchor_final_check.json"
$fitPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_coordinate_transform_anchor_fit.json"
$failures = @()

foreach ($path in @($anchorPath, $fitPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing anchor/fit input: $path"
        exit 1
    }
}

$anchor = Get-Content -LiteralPath $anchorPath -Raw | ConvertFrom-Json
$fit = Get-Content -LiteralPath $fitPath -Raw | ConvertFrom-Json
$active = @($anchor.records | Where-Object { $_.activeInGame })
$disabled = @($anchor.records | Where-Object { -not $_.activeInGame })

if ([int]$anchor.activeOfficialShelterCount -ne $active.Count) {
    $failures += "activeOfficialShelterCount does not match active records."
}

$badActive = @($active | Where-Object {
    -not $_.sceneExactGmlObjectFound -or
    $_.matchMethod -ne "contains" -or
    $_.matchConfidence -ne "high" -or
    $_.manualReviewNeeded -or
    -not $_.markerInsideChuoBaseMapBounds -or
    -not $_.interactionTargetExists -or
    -not $_.officialUiMarkerCorrect -or
    $_.usesNonOfficialWarning
})
if ($badActive.Count -gt 0) {
    $failures += "Unverified or incorrectly marked official shelter is active: $($badActive.id -join ', ')"
}

$badDisabled = @($disabled | Where-Object { [string]::IsNullOrWhiteSpace($_.disabledReason) })
if ($badDisabled.Count -gt 0) {
    $failures += "Disabled official records lack disabledReason: $($badDisabled.id -join ', ')"
}

if ($fit.coordinateTransformStatus -eq "transform_validated_from_official_anchors") {
    if ([int]$fit.fit.controlPointCount -lt 8) {
        $failures += "Validated transform has too few control points."
    }
    if ([double]$fit.fit.maxResidualMeters -gt [double]$fit.fit.thresholds.maxResidualMetersMax) {
        $failures += "Validated transform max residual exceeds threshold."
    }
    if ([double]$fit.fit.rmsResidualMeters -gt [double]$fit.fit.thresholds.rmsResidualMetersMax) {
        $failures += "Validated transform RMS residual exceeds threshold."
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Official anchors and coordinate transform fit are strict and evidence-backed."
exit 0
