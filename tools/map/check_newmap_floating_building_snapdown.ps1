param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_floating_building_snapdown_report.json"
$configPath = Join-Path $root "Assets\Data\P10\newmap_floating_building_snapdown_config.json"

if (-not (Test-Path -LiteralPath $bootstrapPath -PathType Leaf)) { Write-Host "[FAIL] Missing runtime bootstrap."; exit 1 }
if (-not (Test-Path -LiteralPath $reportPath -PathType Leaf)) { Write-Host "[FAIL] Missing snapdown report."; exit 1 }
if (-not (Test-Path -LiteralPath $configPath -PathType Leaf)) { Write-Host "[FAIL] Missing snapdown config."; exit 1 }

$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json
$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json

$passed = [bool]$config.enabled -and
    [bool]$report.runtimeSnapdownEnabled -and
    [double]$report.maxSnapdownMeters -gt 0 -and
    [bool]$report.notGisGradeTerrainAccuracy -and
    $bootstrap -match "ApplyFloatingBuildingSnapdownToGameplayGroundCover" -and
    $bootstrap -match "ContainsSnapdownExcludedText" -and
    $bootstrap -match "buildingsSnappedDown" -and
    $bootstrap -match "buildingSnapdownRemainingFloating"

if (-not $passed) {
    Write-Host "[FAIL] Floating building snapdown implementation/report is incomplete."
    exit 1
}

Write-Host "[PASS] Floating building snapdown implementation/report present."
exit 0
