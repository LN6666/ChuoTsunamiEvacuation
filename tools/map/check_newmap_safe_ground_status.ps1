param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$configPath = Join-Path $root "Assets\Data\P10\newmap_safe_ground_config.json"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_safe_ground_report.json"
$adaptivePath = Join-Path $root "Assets\Data\P10\newmap_adaptive_support_grid_config.json"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"

if (-not (Test-Path -LiteralPath $configPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $reportPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $adaptivePath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $bootstrapPath -PathType Leaf)) {
    Write-Host "[FAIL] Safe ground required files are missing."
    exit 1
}

$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json
$adaptive = Get-Content -Encoding UTF8 -LiteralPath $adaptivePath -Raw | ConvertFrom-Json
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw

$passed = [bool]$config.enabled -and
    [bool]$config.forceFixedSupportY -and
    -not [bool]$config.rendererEnabledInNormalMode -and
    [int]$report.supportColliderCount -ge 1 -and
    [bool]$report.supportRendererHidden -and
    [bool]$report.adaptiveSupportGridDisabled -and
    -not [bool]$adaptive.enabled -and
    $bootstrap -match "NewMapSafeGroundConfig" -and
    $bootstrap -match "ConfigureGroundSafety" -and
    $bootstrap -match "CreateGroundSupportProxy"

if (-not $passed) {
    Write-Host "[FAIL] Safe ground rollback status failed."
    exit 1
}

Write-Host "[PASS] Safe ground rollback status check passed."
exit 0
