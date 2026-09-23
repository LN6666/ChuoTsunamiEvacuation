param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$candidatePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_non_official_candidate_recovery.json"
$activePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_active_target_final_report.json"
$routePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_route_geometry_final_validation.json"
$p5Path = Join-Path $ProjectRoot "Assets\Data\P10\newmap_p5_route_candidate_final_status.json"
$failures = @()

$candidate = Get-Content -LiteralPath $candidatePath -Raw | ConvertFrom-Json
$active = Get-Content -LiteralPath $activePath -Raw | ConvertFrom-Json
$route = Get-Content -LiteralPath $routePath -Raw | ConvertFrom-Json
$p5 = Get-Content -LiteralPath $p5Path -Raw | ConvertFrom-Json

$badCandidates = @($candidate.records | Where-Object {
    $_.isOfficialShelter -or
    ($_.activeInGame -and (-not $_.nonOfficialWarningRequired -or $_.safeApprovedByDefault))
})
if ($badCandidates.Count -gt 0) {
    $failures += "Recovered non-official candidates have false official/safe semantics: $($badCandidates.id -join ', ')"
}

$badActive = @($active.targets | Where-Object {
    -not $_.isOfficialShelter -and ($_.nonOfficialWarningRequired -ne $true -or $_.safeApprovedByDefault -eq $true)
})
if ($badActive.Count -gt 0) {
    $failures += "Active non-official targets lack warning or are safe-approved: $($badActive.id -join ', ')"
}

if ([int]$active.nonOfficialTargetLabeledOfficialCount -ne 0) {
    $failures += "Active report contains non-official targets labeled official."
}

if ([int]$route.officialRouteClaimCount -ne 0 -or [int]$p5.officialRouteClaimCount -ne 0) {
    $failures += "Official route claim count is not zero."
}

$routeClaims = @($route.records | Where-Object { $_.isOfficialEvacuationRoute })
if ($routeClaims.Count -gt 0) {
    $failures += "Route records contain official route claims: $($routeClaims.routeId -join ', ')"
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Official shelters, non-official candidates, and prototype routes keep separate semantics."
exit 0
