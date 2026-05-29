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

$audit = Read-RequiredJson "Assets\Data\P10\newmap_unexpected_airwall_hard_audit.json"
$cleanup = Read-RequiredJson "Assets\Data\P10\newmap_airwall_hard_cleanup_report.json"
$cleanupConfig = Read-RequiredJson "Assets\Data\P10\newmap_airwall_hard_cleanup_config.json"
$collisionConfig = Read-RequiredJson "Assets\Data\P10\newmap_player_npc_collision_config.json"
$collisionReport = Read-RequiredJson "Assets\Data\P10\newmap_player_npc_collision_report.json"
$npcRegression = Read-RequiredJson "Assets\Data\P10\newmap_npc_collision_regression_after_player_collision.json"
$labelRegression = Read-RequiredJson "Assets\Data\P10\newmap_airwall_npc_label_regression.json"
$nameCache = Read-RequiredJson "Assets\Data\P10\newmap_name_cache.json"
$nameConfig = Read-RequiredJson "Assets\Data\P10\newmap_name_enrichment_config.json"
$labelRuntime = Read-RequiredJson "Assets\Data\P10\newmap_name_label_runtime_report.json"
$readiness = Read-RequiredJson "Assets\Data\P10\newmap_manual_playtest_readiness.json"

Require-Condition ([bool]$cleanupConfig.enabled) "Airwall hard cleanup config is disabled."
Require-Condition ([bool]$cleanupConfig.keepBoundaryAirWalls) "Boundary air walls must be preserved."
Require-Condition ([double]$cleanupConfig.playerBuildingCollisionMarginMeters -le 0.1) "Player building collision margin remains too inflated."
Require-Condition ([bool]$cleanup.boundaryAirWallsPreserved) "Cleanup report does not preserve boundary air walls."
Require-Condition ([bool]$cleanup.debugTestCollidersInactiveInNormalMode) "Debug/test colliders are not disabled in normal mode."
Require-Condition ($audit.purposeClassifications -contains "unknown_blocker") "Audit must classify unknown blockers."

Require-Condition ([bool]$collisionConfig.enabled) "Player-NPC collision config disabled."
Require-Condition ([bool]$collisionConfig.preventDirectOverlap) "Player-NPC direct overlap prevention disabled."
Require-Condition ([string]$collisionConfig.mode -eq "soft_blocking_with_near_capsules") "Player-NPC collision mode changed unexpectedly."
Require-Condition ([bool]$collisionReport.playerCannotPassStraightThroughNearNpc) "Player-NPC report does not block direct pass-through."
Require-Condition (-not [bool]$collisionReport.physicsExplosionRisk) "Player-NPC collision report allows physics explosion risk."

Require-Condition ([bool]$npcRegression.npcBuildingAvoidancePreserved) "NPC building avoidance regression flagged."
Require-Condition ([bool]$npcRegression.npcRapidRefreshDisabled) "NPC rapid refresh must remain disabled."
Require-Condition ([int]$npcRegression.npcStoppedWithoutReasonCount -eq 0) "NPC stopped-without-reason count must be zero in report."

Require-Condition ([bool]$labelRegression.nameCacheLoadedRuntimeOffline) "Label regression does not preserve offline cache loading."
Require-Condition (-not [bool]$labelRegression.runtimeNetworkRequestsAllowed) "Runtime network requests must be false."
Require-Condition (-not [bool]$nameConfig.runtimeNetworkRequestsAllowed) "Name enrichment config allows runtime network requests."
Require-Condition ([int]$nameConfig.maxQueriesPerRun -ge 500) "Expanded name enrichment maxQueriesPerRun must be at least 500."
Require-Condition ([bool]$nameConfig.queryBuildingsNearGameplayArea) "Expanded building query scope disabled."
Require-Condition ($nameCache.labels.Count -ge 180) "Name cache must contain expanded label records after enrichment."
Require-Condition ([bool]$labelRuntime.nameCacheExists -or [bool]$labelRuntime.nameCacheLoaded) "Runtime label report does not confirm cache exists/loaded."

$allowed = @(
    "ready_for_manual_playtest",
    "ready_with_documented_label_or_collision_limitations",
    "ready_with_documented_building_visual_limitations",
    "needs_quick_fix_before_manual_test",
    "blocked"
)
Require-Condition ($allowed -contains [string]$readiness.manualReadinessDecision) "Manual readiness decision missing or invalid."

Write-Host "[PASS] NewMap airwall/NPC/label JSON files validated."
exit 0
