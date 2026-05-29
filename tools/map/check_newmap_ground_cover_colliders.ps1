param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$reportPath = Join-Path $root "Assets\Data\P10\newmap_gameplay_ground_cover_report.json"
$configPath = Join-Path $root "Assets\Data\P10\newmap_gameplay_ground_cover_config.json"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"

if (-not (Test-Path -LiteralPath $reportPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $configPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $bootstrapPath -PathType Leaf)) {
    Write-Host "[FAIL] Ground cover collider check files are missing."
    exit 1
}

$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json
$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw

$passed = [bool]$config.enabled -and
    [bool]$config.colliderEnabled -and
    [bool]$config.rendererEnabledInNormalMode -and
    [int]$report.runtimeTileCount -gt 0 -and
    [int]$report.colliderCount -eq [int]$report.runtimeTileCount -and
    [int]$report.visibleRendererCount -eq [int]$report.runtimeTileCount -and
    $bootstrap -match "EnsureGameplayGroundCover" -and
    $bootstrap -match "GroundCover_Tile_" -and
    $bootstrap -match "collider\.enabled = config\.colliderEnabled" -and
    $bootstrap -match "renderer\.enabled = config\.rendererEnabledInNormalMode"

if (-not $passed) {
    Write-Host "[FAIL] Ground cover collider/renderer validation failed."
    exit 1
}

Write-Host "[PASS] Ground cover tiles are configured as visible collidable gameplay ground."
exit 0
