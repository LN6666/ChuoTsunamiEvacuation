param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$routePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_route_geometry_final_validation.json"
$fitPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_coordinate_transform_anchor_fit.json"
foreach ($path in @($routePath, $fitPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing route validation input: $path"
        exit 1
    }
}

$route = Get-Content -LiteralPath $routePath -Raw | ConvertFrom-Json
$fit = Get-Content -LiteralPath $fitPath -Raw | ConvertFrom-Json
$failures = @()

if ([int]$route.officialRouteClaimCount -ne 0) {
    $failures += "officialRouteClaimCount is not zero."
}

$officialClaims = @($route.records | Where-Object { $_.isOfficialEvacuationRoute })
if ($officialClaims.Count -gt 0) {
    $failures += "Route records claim official evacuation route status: $($officialClaims.routeId -join ', ')"
}

$activeInvalid = @($route.records | Where-Object { $_.activeInGame -and $_.classification -ne "validated_on_new_chuo_basemap" })
if ($activeInvalid.Count -gt 0) {
    $failures += "Route transform active for unvalidated route: $($activeInvalid.routeId -join ', ')"
}

if ($fit.coordinateTransformStatus -eq "transform_validated_from_official_anchors") {
    if ([int]$route.validatedOnNewChuoBaseMapCount -lt 1) {
        $failures += "Transform is validated but no route geometry validated on Chuo_BaseMap."
    }

    $badValidated = @($route.records | Where-Object {
        $_.classification -eq "validated_on_new_chuo_basemap" -and (
            -not $_.allFinite -or
            [int]$_.transformedPointsInsideMapBounds -ne [int]$_.pointCount -or
            [double]$_.endpointDistanceToActiveAnchorMeters -gt [double]$fit.fit.thresholds.routeEndpointResidualMetersMax -or
            [double]$_.maxSegmentMetersAfterTransform -gt [double]$fit.fit.thresholds.routeMaxSegmentMetersMax
        )
    })
    if ($badValidated.Count -gt 0) {
        $failures += "Validated routes fail finite/bounds/endpoint/segment checks: $($badValidated.routeId -join ', ')"
    }
}
else {
    $badValidated = @($route.records | Where-Object { $_.classification -eq "validated_on_new_chuo_basemap" -or $_.activeInGame })
    if ($badValidated.Count -gt 0) {
        $failures += "Routes validated or active without a validated transform: $($badValidated.routeId -join ', ')"
    }
}

if ($route.routeRuntimePolicy -notmatch "estimated" -or $route.routeRuntimePolicy -notmatch "official") {
    $failures += "Route runtime policy does not include estimated/non-official wording."
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Route geometry validation is evidence-backed and has no official route false claim."
exit 0
