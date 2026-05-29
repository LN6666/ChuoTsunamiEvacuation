param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$configPath = Join-Path $root "Assets\Data\P10\newmap_playable_bounds_config.json"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_air_wall_regression_report.json"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$playModeTestsPath = Join-Path $root "Assets\Tests\PlayMode\NewMapRuntimePlayModeTests.cs"

foreach ($path in @($configPath, $reportPath, $bootstrapPath, $playModeTestsPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Air-wall validation file missing: $path"
        exit 1
    }
}

$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$tests = Get-Content -Encoding UTF8 -LiteralPath $playModeTestsPath -Raw

$ok =
    [bool]$config.enabled -and
    -not [bool]$config.debugVisualizationEnabled -and
    [double]$config.boundaryHeightMeters -gt 10.0 -and
    [double]$config.boundaryThicknessMeters -gt 0.0 -and
    [bool]$report.airWallsStillExist -and
    [bool]$report.airWallsInvisible -and
    [bool]$report.blockMapBoundary -and
    [bool]$report.spawnCannotOccurOutside -and
    [bool]$report.activeTargetsOutsideBoundsDisabled -and
    [int]$report.expectedColliderCount -eq 4 -and
    [int]$report.expectedVisibleRendererCount -eq 0 -and
    $bootstrap -match "EnsurePlayableBoundsAirWalls" -and
    $bootstrap -match "CreateAirWall" -and
    $bootstrap -match "LastPlayableAirWallColliderCount" -and
    $bootstrap -match "LastPlayableAirWallVisibleRendererCount" -and
    $tests -match "Air walls must not render" -and
    $tests -match "PlayableBounds.ContainsXZ"

if (-not $ok) {
    Write-Host "[FAIL] Air-wall regression validation failed."
    exit 1
}

Write-Host "[PASS] Air walls are configured as invisible boundary colliders and covered by tests."
exit 0
