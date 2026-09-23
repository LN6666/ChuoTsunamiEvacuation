param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$routePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_route_geometry_final_validation.json"
$p5Path = Join-Path $ProjectRoot "Assets\Data\P10\newmap_p5_route_candidate_final_status.json"
$route = Get-Content -LiteralPath $routePath -Raw | ConvertFrom-Json
$p5 = Get-Content -LiteralPath $p5Path -Raw | ConvertFrom-Json
$failures = @()

if ([int]$route.officialRouteClaimCount -ne 0 -or [int]$p5.officialRouteClaimCount -ne 0) {
    $failures += "Official route claim count is not zero."
}

$claims = @($route.records | Where-Object { $_.isOfficialEvacuationRoute })
if ($claims.Count -gt 0) {
    $failures += "Route records contain official route claims: $($claims.routeId -join ', ')"
}

if ($p5.prototypeGuidanceWording -notmatch "not an official evacuation route") {
    $failures += "P5 prototype guidance wording does not explicitly reject official route status."
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] No official route false claims are present."
exit 0
