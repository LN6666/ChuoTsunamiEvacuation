param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$script:Failures = @()

function Add-Failure {
    param([string]$Message)
    $script:Failures += $Message
}

function Read-RequiredJson {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "Missing JSON: $RelativePath"
        return $null
    }

    try {
        return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
    }
    catch {
        Add-Failure "Invalid JSON: $RelativePath ($($_.Exception.Message))"
        return $null
    }
}

function Require-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Add-Failure $Message
    }
}

$snapConfig = Read-RequiredJson "Assets\Data\P10\newmap_floating_building_snapdown_config.json"
$candidates = Read-RequiredJson "Assets\Data\P10\newmap_floating_building_snapdown_candidates.json"
$snapReport = Read-RequiredJson "Assets\Data\P10\newmap_floating_building_snapdown_report.json"
$targetReport = Read-RequiredJson "Assets\Data\P10\newmap_target_height_after_building_snapdown.json"
$visual = Read-RequiredJson "Assets\Data\P10\newmap_building_snapdown_visual_validation.json"
$npcConfig = Read-RequiredJson "Assets\Data\P10\newmap_npc_distribution_config.json"
$npcReport = Read-RequiredJson "Assets\Data\P10\newmap_npc_100x_distribution_report.json"
$npcGameplay = Read-RequiredJson "Assets\Data\P10\newmap_npc_crowd_100x_gameplay_status.json"
$npcPerformance = Read-RequiredJson "Assets\Data\P10\newmap_npc_100x_performance_report.json"
$groundCover = Read-RequiredJson "Assets\Data\P10\newmap_gameplay_ground_cover_report.json"
$readiness = Read-RequiredJson "Assets\Data\P10\newmap_manual_playtest_readiness.json"

if ($snapConfig) {
    Require-Condition ([bool]$snapConfig.enabled) "Building snapdown config is disabled."
    Require-Condition ([double]$snapConfig.floatingGapThresholdMeters -gt 0) "Snapdown threshold must be positive."
    Require-Condition ([double]$snapConfig.maxSnapdownMeters -ge [double]$snapConfig.floatingGapThresholdMeters) "Snapdown max offset must be >= threshold."
    Require-Condition ([bool]$snapConfig.useGameplayGroundCoverAsReference) "Snapdown must use gameplay ground cover as reference."
}

if ($candidates) {
    Require-Condition ([string]$candidates.referenceSurface -match "gameplay ground cover") "Candidates must use gameplay ground cover reference."
    Require-Condition ([string]$candidates.knownSafetyPolicy -match "building-like") "Candidates must document building-only safety policy."
}

if ($snapReport) {
    Require-Condition ([bool]$snapReport.runtimeSnapdownEnabled) "Snapdown report says runtime snapdown disabled."
    Require-Condition ([bool]$snapReport.notGisGradeTerrainAccuracy) "Snapdown report must avoid GIS-grade accuracy claims."
    Require-Condition ([double]$snapReport.maxSnapdownMeters -gt 0) "Snapdown max offset missing."
}

if ($targetReport) {
    Require-Condition ([bool]$targetReport.greenFramesAlignToGroundCover) "Green frames must align to ground cover after snapdown."
    Require-Condition ([bool]$targetReport.interactionZonesAlignToGroundCover) "Interaction zones must align to ground cover after snapdown."
    Require-Condition (-not [bool]$targetReport.resultPanelTargetDataMovedSeparately) "ResultPanel target data should not be separately moved."
}

if ($visual) {
    Require-Condition ([bool]$visual.playerNpcGroundCoverRemainAligned) "Player/NPC/ground cover alignment not preserved."
    Require-Condition (-not [bool]$visual.blueGroundFixRegressed) "Blue ground fix regressed."
}

if ($npcConfig) {
    Require-Condition ([int]$npcConfig.npcCountMultiplier -eq 100) "NPC multiplier must be 100."
    Require-Condition ([int]$npcConfig.maxNpcCount -gt 0) "NPC cap must be present and positive."
    Require-Condition ([int]$npcConfig.maxNpcCount -le 1000) "NPC cap must remain bounded."
    Require-Condition ([bool]$npcConfig.useSectorDistribution) "NPC sector distribution must be enabled."
    Require-Condition ([bool]$npcConfig.snapToGroundCover) "NPCs must snap to ground cover."
    Require-Condition ([bool]$npcConfig.avoidBuildings) "NPCs must avoid building bounds."
    Require-Condition ([bool]$npcConfig.usePooling) "NPC pooling/build-once reuse must be enabled."
    Require-Condition ([bool]$npcConfig.farNpcUpdateThrottle) "Far NPC update throttle must be enabled."
}

if ($npcReport) {
    Require-Condition ([int]$npcReport.requestedNpcCount -eq 800) "NPC 100x report must request 800 NPCs from baseline 8."
    Require-Condition ([int]$npcReport.maxNpcCount -eq 800) "NPC 100x report must cap at 800."
    Require-Condition ([bool]$npcReport.wideDistributionRequired) "NPC wide distribution requirement missing."
}

if ($npcGameplay) {
    Require-Condition ([bool]$npcGameplay.tourismCrowdFailureDisabled) "Tourism NPC failure must be disabled."
    Require-Condition ([bool]$npcGameplay.evacuationCrowdDelayBounded) "Evacuation crowd delay must be bounded."
    Require-Condition (-not [bool]$npcGameplay.npcDirectPlayerKill) "NPCs must not directly kill the player."
    Require-Condition (-not [bool]$npcGameplay.heavyPerNpcPathfinding) "Heavy per-NPC pathfinding must remain disabled."
}

if ($npcPerformance) {
    Require-Condition ([int]$npcPerformance.npcCap -eq 800) "NPC performance report must record cap."
}

if ($groundCover) {
    Require-Condition ([bool]$groundCover.groundCoverEnabled) "Ground cover report missing/disabled."
    Require-Condition ([int]$groundCover.colliderCount -gt 0) "Ground cover colliders missing."
}

if ($readiness) {
    $allowed = @(
        "ready_for_manual_playtest",
        "ready_with_documented_building_visual_limitations",
        "needs_quick_fix_before_manual_test",
        "blocked"
    )
    Require-Condition ($allowed -contains [string]$readiness.manualReadinessDecision) "Manual readiness decision invalid for snapdown/NPC100x."
    Require-Condition (-not [bool]$readiness.buildingSnapdownClaimsGisAccuracy) "Readiness must not claim GIS-grade building accuracy."
    Require-Condition (-not [bool]$readiness.roadTerrainAccuracyClaimed) "Readiness must not claim road/terrain accuracy."
}

if ($script:Failures.Count -gt 0) {
    foreach ($failure in $script:Failures) {
        Write-Host "[FAIL] $failure"
    }
    exit 1
}

Write-Host "[PASS] NewMap building snapdown + NPC100x JSON files validated."
exit 0
