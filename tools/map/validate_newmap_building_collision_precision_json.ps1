param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Read-RequiredJson {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing $RelativePath"
        exit 1
    }

    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
}

function Require-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

$config = Read-RequiredJson "Assets\Data\P10\newmap_building_collision_precision_config.json"
$precisionReport = Read-RequiredJson "Assets\Data\P10\newmap_building_collision_precision_report.json"
$corridors = Read-RequiredJson "Assets\Data\P10\newmap_walkable_corridor_collision_report.json"
$finalWhitelist = Read-RequiredJson "Assets\Data\P10\newmap_collision_whitelist_final_report.json"
$manual = Read-RequiredJson "Assets\Data\P10\newmap_manual_playtest_readiness.json"

$requiredReports = @(
    "Assets\Data\P10\newmap_building_collision_precision_audit.json",
    "Assets\Data\P10\newmap_building_collision_precision_report.json",
    "Assets\Data\P10\newmap_walkable_corridor_collision_report.json",
    "Assets\Data\P10\newmap_building_collision_gameplay_validation.json",
    "Assets\Data\P10\newmap_npc_after_building_collision_precision.json",
    "Assets\Data\P10\newmap_collision_whitelist_final_report.json"
)

foreach ($report in $requiredReports) {
    Require-Condition (Test-Path -LiteralPath (Join-Path $root $report) -PathType Leaf) "Missing report $report"
}

Require-Condition ([bool]$config.enabled) "Building collision precision config disabled."
Require-Condition ([string]$config.collisionMode -eq "tight_footprint_box_proxies") "Collision mode must be tight_footprint_box_proxies."
Require-Condition ([math]::Abs([double]$config.shrinkFactorXZ - 0.90) -le 0.001) "shrinkFactorXZ must be 0.90."
Require-Condition ([double]$config.maxColliderWidthMeters -eq 80.0) "maxColliderWidthMeters must be 80.0."
Require-Condition ([double]$config.maxColliderDepthMeters -eq 80.0) "maxColliderDepthMeters must be 80.0."
Require-Condition ([double]$config.colliderHeightMeters -eq 5.0) "colliderHeightMeters must be 5.0."
Require-Condition ([bool]$config.useGroundCoverY) "Building precision bounds must use ground-cover Y."
Require-Condition ([bool]$config.disableClusterRootColliders) "Inflated cluster/root bounds must be disabled/skipped."
Require-Condition ([bool]$config.sampleWalkCorridorValidation) "Walkable corridor validation disabled."
Require-Condition ([bool]$config.carveActiveTargetInteractionClearance) "Active target interaction clearance carving disabled."
Require-Condition ([double]$config.targetInteractionClearanceMeters -ge 8.0) "Target interaction clearance must be large enough for approach sampling."
Require-Condition ([bool]$precisionReport.notGisGradeFootprintAccuracyClaim) "Report must not claim GIS-grade footprint accuracy."
Require-Condition ($corridors.expectedBlockers -contains "building_tight_proxy_expected") "Corridor report missing expected building-tight-proxy blocker category."
Require-Condition (-not [bool]$finalWhitelist.buildingCollisionDisabledEntirely) "Final whitelist report says building collision is disabled."

$allowedReadiness = @(
    "ready_for_manual_playtest",
    "ready_with_documented_boundary_limitations",
    "needs_quick_fix_before_manual_test",
    "blocked"
)
Require-Condition ($allowedReadiness -contains [string]$manual.manualReadinessDecision) "Manual readiness decision missing or invalid."

Write-Host "[PASS] NewMap building collision precision JSON validated."
exit 0
