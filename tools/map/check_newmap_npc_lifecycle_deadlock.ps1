param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$configPath = Join-Path $root "Assets\Data\P10\newmap_npc_lifecycle_config.json"
$auditPath = Join-Path $root "Assets\Data\P10\newmap_npc_lifecycle_deadlock_audit.json"
$refreshPath = Join-Path $root "Assets\Data\P10\newmap_npc_no_rapid_refresh_fix.json"
$deadlockPath = Join-Path $root "Assets\Data\P10\newmap_npc_player_collision_deadlock_fix.json"
$smokePath = Join-Path $root "Assets\Data\P10\newmap_npc_180s_lifecycle_smoke.json"
$npcPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"

foreach ($path in @($configPath, $auditPath, $refreshPath, $deadlockPath, $smokePath, $npcPath, $bootstrapPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing NPC lifecycle validation input: $path"
        exit 1
    }
}

$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json
$audit = Get-Content -Encoding UTF8 -LiteralPath $auditPath -Raw | ConvertFrom-Json
$refresh = Get-Content -Encoding UTF8 -LiteralPath $refreshPath -Raw | ConvertFrom-Json
$deadlock = Get-Content -Encoding UTF8 -LiteralPath $deadlockPath -Raw | ConvertFrom-Json
$npc = Get-Content -Encoding UTF8 -LiteralPath $npcPath -Raw
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw

$passed =
    [bool]$config.generateOnModeStartOnly -and
    (-not [bool]$config.allowGlobalRefresh) -and
    [double]$config.globalRespawnIntervalSeconds -eq 0.0 -and
    (-not [bool]$config.farNpcDespawnEnabled) -and
    [bool]$config.collisionWithPlayerDoesNotGlobalPause -and
    [int]$audit.globalRespawnCount -eq 0 -and
    [int]$refresh.globalRespawnIntervalSeconds -eq 0 -and
    [bool]$deadlock.collisionAffectsOnlyLocalPair -and
    [bool]$deadlock.collisionWithPlayerDoesNotGlobalPause -and
    $npc -match "SetReferenceTransform" -and
    $npc -match "PreventAllStopDeadlock" -and
    $npc -match "ResolveNpcTargetAvoidingPlayerContact" -and
    $npc -match "PlayerContactEventCount" -and
    $bootstrap -match "RunNpcLifecycleDiagnosticSmoke"

if (-not $passed) {
    Write-Host "[FAIL] NPC lifecycle/deadlock validation failed."
    exit 1
}

Write-Host "[PASS] NPC lifecycle config and local contact deadlock fix are present."
exit 0
