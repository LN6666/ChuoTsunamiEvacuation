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

$concaveAudit = Read-RequiredJson "Assets\Data\P10\newmap_concave_mesh_trigger_audit.json"
$concaveFix = Read-RequiredJson "Assets\Data\P10\newmap_concave_mesh_trigger_fix.json"
$lifecycleConfig = Read-RequiredJson "Assets\Data\P10\newmap_npc_lifecycle_config.json"
$lifecycleAudit = Read-RequiredJson "Assets\Data\P10\newmap_npc_lifecycle_deadlock_audit.json"
$refreshFix = Read-RequiredJson "Assets\Data\P10\newmap_npc_no_rapid_refresh_fix.json"
$contactFix = Read-RequiredJson "Assets\Data\P10\newmap_npc_player_collision_deadlock_fix.json"
$smoke = Read-RequiredJson "Assets\Data\P10\newmap_npc_180s_lifecycle_smoke.json"
$labelAudit = Read-RequiredJson "Assets\Data\P10\newmap_building_road_label_coverage_audit.json"
$labelRuntime = Read-RequiredJson "Assets\Data\P10\newmap_building_road_label_runtime_report.json"
$nameCache = Read-RequiredJson "Assets\Data\P10\newmap_name_cache.json"
$nameConfig = Read-RequiredJson "Assets\Data\P10\newmap_name_enrichment_config.json"
$nameRuntime = Read-RequiredJson "Assets\Data\P10\newmap_name_label_runtime_report.json"
$regression = Read-RequiredJson "Assets\Data\P10\newmap_runtime_error_npc_label_regression.json"
$readiness = Read-RequiredJson "Assets\Data\P10\newmap_manual_playtest_readiness.json"

Require-Condition ([int]$concaveAudit.sceneConcaveMeshTriggerOffenders -eq 0) "Scene concave MeshCollider triggers remain."
Require-Condition ([bool]$concaveFix.runtimeFixImplemented) "Concave MeshCollider runtime fix missing."
Require-Condition ([string]$concaveFix.runtimeErrorExpectedAbsent -match "concave MeshColliders") "Concave error expected flag is malformed."

Require-Condition ([bool]$lifecycleConfig.generateOnModeStartOnly) "NPC lifecycle generateOnModeStartOnly disabled."
Require-Condition (-not [bool]$lifecycleConfig.allowGlobalRefresh) "NPC global refresh enabled."
Require-Condition ([double]$lifecycleConfig.globalRespawnIntervalSeconds -eq 0.0) "NPC global respawn interval must be 0."
Require-Condition (-not [bool]$lifecycleConfig.farNpcDespawnEnabled) "Far NPC despawn must stay disabled."
Require-Condition ([bool]$contactFix.collisionAffectsOnlyLocalPair) "Player-NPC contact fix is not local."
Require-Condition ([bool]$contactFix.collisionWithPlayerDoesNotGlobalPause) "Player-NPC contact may globally pause NPCs."
Require-Condition ([int]$refreshFix.npcCreatedAtStartupExpected -ge 1) "NPC startup creation expectation missing."
Require-Condition ([int]$smoke.globalRespawnCount -eq 0) "NPC 180s smoke report starts with global respawn non-zero."

Require-Condition (-not [bool]$nameConfig.runtimeNetworkRequestsAllowed) "Name enrichment config allows runtime web."
Require-Condition (-not [bool]$nameRuntime.runtimeNetworkRequestsAllowed) "Runtime name label report allows web."
Require-Condition (-not [bool]$nameCache.runtimeNetworkRequestsAllowed) "Name cache allows runtime web."
Require-Condition ([int]$labelAudit.ordinaryBuildingLabels -ge 100) "Expanded ordinary building labels are insufficient."
Require-Condition ([int]$labelAudit.roadLabels -ge 150) "Expanded road labels are insufficient."
Require-Condition ([int]$labelRuntime.ordinaryBuildingLabelsAvailable -ge 100) "Runtime building label availability report is insufficient."
Require-Condition ([bool]$regression.playerNpcCollisionPreserved) "Regression report does not preserve player-NPC collision."

$allowed = @(
    "ready_for_manual_playtest",
    "ready_with_documented_npc_or_label_limitations",
    "ready_with_documented_label_or_collision_limitations",
    "needs_quick_fix_before_manual_test",
    "blocked"
)
Require-Condition ($allowed -contains [string]$readiness.manualReadinessDecision) "Manual readiness decision missing or invalid."

Write-Host "[PASS] NewMap runtime/NPC/label JSON files validated."
exit 0
