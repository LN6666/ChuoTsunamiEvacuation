param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$configPath = Join-Path $root "Assets\Data\P10\newmap_airwall_hard_cleanup_config.json"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_airwall_hard_cleanup_report.json"

if (-not (Test-Path -LiteralPath $bootstrapPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $configPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $reportPath -PathType Leaf)) {
    Write-Host "[FAIL] Airwall cleanup source/config/report is missing."
    exit 1
}

$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json

$passed =
    [bool]$config.enabled -and
    -not [bool]$config.keepBoundaryAirWalls -and
    -not [bool]$config.keepInvalidZoneBlockers -and
    [bool]$config.convertInteractionBlockersToTriggers -and
    [double]$config.playerBuildingCollisionMarginMeters -le 0.1 -and
    -not [bool]$report.boundaryAirWallsPreserved -and
    [bool]$report.debugTestCollidersInactiveInNormalMode -and
    ($bootstrap -match "AuditAndCleanupUnexpectedAirwallColliders") -and
    ($bootstrap -match "ClassifyBlockingCollider") -and
    ($bootstrap -match "TryBuildConservativeBuildingObstacleBounds") -and
    ($bootstrap -match "ResolveCircularBoundary") -and
    ($bootstrap -match "LastOldRectangularAirWallDisabledCount")

if (-not $passed) {
    Write-Host "[FAIL] Unexpected airwall cleanup validation failed."
    exit 1
}

Write-Host "[PASS] Unexpected airwall cleanup uses the circular boundary and removes/reclassifies accidental blockers."
exit 0
