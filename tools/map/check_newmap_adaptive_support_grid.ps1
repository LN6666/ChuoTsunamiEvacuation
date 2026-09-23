param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$gridScriptPath = Join-Path $root "Assets\Scripts\NewMap\NewMapAdaptiveSupportGrid.cs"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$configPath = Join-Path $root "Assets\Data\P10\newmap_adaptive_support_grid_config.json"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_adaptive_support_grid_report.json"
$samplePath = Join-Path $root "Assets\Data\P10\newmap_ground_road_height_samples.json"

if (-not (Test-Path -LiteralPath $gridScriptPath -PathType Leaf)) {
    Write-Host "[FAIL] Adaptive support grid runtime script is missing."
    exit 1
}

foreach ($path in @($bootstrapPath, $configPath, $reportPath, $samplePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Required adaptive grid file missing: $path"
        exit 1
    }
}

$gridScript = Get-Content -Encoding UTF8 -LiteralPath $gridScriptPath -Raw
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json
$samples = Get-Content -Encoding UTF8 -LiteralPath $samplePath -Raw | ConvertFrom-Json

$ok =
    [bool]$config.enabled -and
    -not [bool]$config.debugVisualizationEnabled -and
    -not [bool]$config.rendererEnabledInNormalMode -and
    [int]$report.gridCellCount -gt 0 -and
    [int]$report.colliderCount -ge [int]$report.gridCellCount -and
    [bool]$report.renderersDisabled -and
    -not [bool]$report.blueSupportVisualActive -and
    [bool]$report.playerUsesAdaptiveGrid -and
    [bool]$report.npcUsesAdaptiveGrid -and
    [bool]$report.targetsUseLocalHeight -and
    [double]$report.supportYMax -gt [double]$report.supportYMin -and
    [bool]$samples.valid -and
    [int]$samples.totalSamples -gt 0 -and
    $gridScript -match "AddComponent<BoxCollider>" -and
    $gridScript -match "rendererEnabledInNormalMode = false" -and
    $gridScript -match "debugVisualizationEnabled = false" -and
    $gridScript -notmatch "AddComponent<MeshRenderer>" -and
    $gridScript -notmatch "CreatePrimitive" -and
    $bootstrap -match "NewMapAdaptiveSupportGridRuntime.Load" -and
    $bootstrap -match "ResolveLocalSupportSurfaceY" -and
    $bootstrap -match "adaptiveGridActive"

if (-not $ok) {
    Write-Host "[FAIL] Adaptive support grid runtime/report check failed."
    exit 1
}

Write-Host "[PASS] Adaptive support grid is configured as invisible local-height collision support."
exit 0
