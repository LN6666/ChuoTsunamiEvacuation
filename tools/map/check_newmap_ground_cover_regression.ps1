param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$coverPath = Join-Path $root "Assets\Data\P10\newmap_gameplay_ground_cover_report.json"
$fallPath = Join-Path $root "Assets\Data\P10\newmap_full_fall_prevention_report.json"
$bluePath = Join-Path $root "Assets\Data\P10\newmap_blue_area_cover_status.json"
$readinessPath = Join-Path $root "Assets\Data\P10\newmap_manual_playtest_readiness.json"

foreach ($path in @($coverPath, $fallPath, $bluePath, $readinessPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing regression input: $path"
        exit 1
    }
}

$cover = Get-Content -Encoding UTF8 -LiteralPath $coverPath -Raw | ConvertFrom-Json
$fall = Get-Content -Encoding UTF8 -LiteralPath $fallPath -Raw | ConvertFrom-Json
$blue = Get-Content -Encoding UTF8 -LiteralPath $bluePath -Raw | ConvertFrom-Json
$readiness = Get-Content -Encoding UTF8 -LiteralPath $readinessPath -Raw | ConvertFrom-Json

$passed = [bool]$cover.groundCoverEnabled -and
    [int]$cover.colliderCount -gt 0 -and
    -not [bool]$cover.remainingUncoveredFallRisk -and
    [bool]$fall.playerCannotFallThroughGroundCover -and
    [bool]$fall.playerCannotLeaveMapBounds -and
    [int]$fall.airWallCount -eq 4 -and
    -not [bool]$blue.knownBlueFallZonesAccessible -and
    -not [bool]$readiness.roadTerrainAccuracyClaimed

if (-not $passed) {
    Write-Host "[FAIL] Ground-cover/fall-prevention regression check failed."
    exit 1
}

Write-Host "[PASS] Ground cover and fall prevention regression checks passed."
exit 0
