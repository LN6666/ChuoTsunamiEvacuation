param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$routePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_route_geometry_validation.json"
$coordPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_coordinate_transform_validation.json"
foreach ($path in @($routePath, $coordPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing route/coordinate validation input: $path"
        exit 1
    }
}

$routeReport = Get-Content -LiteralPath $routePath -Raw | ConvertFrom-Json
$coordReport = Get-Content -LiteralPath $coordPath -Raw | ConvertFrom-Json
$failures = @()

if ([bool]$coordReport.wgs84ToUnityTransformAvailable) {
    if ([int]$routeReport.validatedOnNewMapCount -lt 1) {
        $failures += "Coordinate transform is marked available but no route is validated on the new map."
    }
}
else {
    $badValidated = @($routeReport.records | Where-Object { $_.classification -eq "validated_on_new_map" -or $_.activeInGame })
    if ($badValidated.Count -gt 0) {
        $failures += "Routes are active/validated without a verified transform: $($badValidated.routeId -join ', ')"
    }
}

$officialClaims = @($routeReport.records | Where-Object { $_.isOfficialEvacuationRoute })
if ($officialClaims.Count -gt 0 -or [int]$routeReport.officialRouteClaimCount -ne 0) {
    $failures += "Official route false claim detected."
}

$badBlocked = @($routeReport.records | Where-Object {
    $_.classification -eq "blocked_transform_unknown" -and $_.disabledReason -notmatch "blocked_transform_unknown"
})
if ($badBlocked.Count -gt 0) {
    $failures += "Blocked route records lack transform-blocked reason: $($badBlocked.routeId -join ', ')"
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Route geometry validation is honest: no active old routes or official route claims without a verified transform."
exit 0
