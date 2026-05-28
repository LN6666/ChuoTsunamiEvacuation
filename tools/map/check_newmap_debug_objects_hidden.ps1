param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_debug_object_cleanup_report.json"
$bootstrap = Get-Content -LiteralPath $bootstrapPath -Raw
$report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json

$checks = @(
    ($bootstrap -match "DebugDiagnosticsRoot"),
    ($bootstrap -match "gameObject\.SetActive\(false\)"),
    ($bootstrap -match "ShouldEnableLocalTrainingProxyTargets"),
    ($bootstrap -match "EnableLocalTrainingProxyTargetsForDiagnostics"),
    ($bootstrap -match "GameplaySupportRoot"),
    ([bool]$report.productionManualMode.debugDiagnosticsRootInactiveByDefault),
    (-not [bool]$report.productionManualMode.localTrainingProxyTargetsActiveInProduction),
    (-not [bool]$report.productionManualMode.runtimeGroundSupportVisible)
)

if ($checks -contains $false) {
    Write-Host "[FAIL] Debug object cleanup check failed."
    exit 1
}

Write-Host "[PASS] Debug object cleanup check passed."
exit 0
