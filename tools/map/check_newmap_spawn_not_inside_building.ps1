param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$testsPath = Join-Path $root "Assets\Tests\PlayMode\NewMapRuntimePlayModeTests.cs"
$spawnConfigPath = Join-Path $root "Assets\Data\P10\newmap_spawn_config.json"
$safeSpawnPath = Join-Path $root "Assets\Data\P10\newmap_safe_spawn_points.json"

$bootstrap = Get-Content -LiteralPath $bootstrapPath -Raw
$tests = Get-Content -LiteralPath $testsPath -Raw
$spawn = Get-Content -LiteralPath $spawnConfigPath -Raw | ConvertFrom-Json
$safe = Get-Content -LiteralPath $safeSpawnPath -Raw | ConvertFrom-Json

$ok =
    $spawn.spawnMode -eq "road_or_playable_ground_only" -and
    [bool]$spawn.useBuildingBoundsRejection -and
    [bool]$spawn.useGroundProbe -and
    [bool]$spawn.useGroundSupportFallback -and
    [int]$spawn.maxSpawnAttempts -ge 50 -and
    @($safe.records).Count -ge 3 -and
    $bootstrap -match "buildingAvoidanceBounds" -and
    $bootstrap -match "IsInsideBuildingBounds" -and
    $bootstrap -match "CalculateNearestBuildingDistance" -and
    $bootstrap -match "LastSpawnRejectedInsideBuildingCount" -and
    $bootstrap -match "fallbackSafeSpawnId" -and
    $tests -match "RuntimeSpawnValidationRejectsBuildingOverlapAndUsesPlayableSupport"

if (-not $ok) {
    Write-Host "[FAIL] Spawn building-overlap/playable-ground validation check failed."
    exit 1
}

Write-Host "[PASS] Spawn building-overlap/playable-ground validation check passed."
exit 0
