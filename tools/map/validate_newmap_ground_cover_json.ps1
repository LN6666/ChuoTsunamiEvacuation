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

$config = Read-RequiredJson "Assets\Data\P10\newmap_gameplay_ground_cover_config.json"
$material = Read-RequiredJson "Assets\Data\P10\newmap_ground_cover_material_report.json"
$cover = Read-RequiredJson "Assets\Data\P10\newmap_gameplay_ground_cover_report.json"
$raise = Read-RequiredJson "Assets\Data\P10\newmap_ground_raise_alignment_report.json"
$fall = Read-RequiredJson "Assets\Data\P10\newmap_full_fall_prevention_report.json"
$blue = Read-RequiredJson "Assets\Data\P10\newmap_blue_area_cover_status.json"
$spawn = Read-RequiredJson "Assets\Data\P10\newmap_ground_cover_spawn_npc_target_status.json"
$readiness = Read-RequiredJson "Assets\Data\P10\newmap_manual_playtest_readiness.json"

Require-Condition (Test-Path -LiteralPath (Join-Path $root "Assets\Resources\NewMap\P10_NewMap_RoadGroundCover.mat") -PathType Leaf) "Ground cover material asset is missing."

if ($config) {
    Require-Condition ([bool]$config.enabled) "Ground cover config is disabled."
    Require-Condition ([bool]$config.rendererEnabledInNormalMode) "Ground cover renderer must be enabled in normal mode."
    Require-Condition ([bool]$config.colliderEnabled) "Ground cover collider must be enabled."
    Require-Condition ([double]$config.materialAlpha -eq 1.0) "Ground cover material must be opaque."
}

if ($material) {
    Require-Condition ([bool]$material.opaque) "Ground cover material is not opaque."
    Require-Condition ([bool]$material.notBlue) "Ground cover material is blue-like."
    Require-Condition ([bool]$material.notMagenta) "Ground cover material is magenta-like."
    Require-Condition (-not [bool]$material.debugMaterial) "Ground cover material is marked debug."
}

if ($cover) {
    Require-Condition ([bool]$cover.groundCoverEnabled) "Ground cover report says cover disabled."
    Require-Condition ([int]$cover.runtimeTileCount -gt 0) "Ground cover report has no tiles."
    Require-Condition ([int]$cover.colliderCount -eq [int]$cover.runtimeTileCount) "Not every ground cover tile has a collider."
    Require-Condition ([int]$cover.visibleRendererCount -eq [int]$cover.runtimeTileCount) "Not every ground cover tile renders in normal mode."
    Require-Condition (-not [bool]$cover.remainingUncoveredFallRisk) "Ground cover report leaves an uncovered fall risk."
    Require-Condition ([bool]$cover.notGisGradeTerrainAccuracy) "Ground cover report must avoid GIS-grade terrain accuracy claims."
}

if ($raise) {
    Require-Condition ([bool]$raise.playerStandsOnVisibleCover) "Player is not reported standing on visible cover."
    Require-Condition ([bool]$raise.npcUsesSameGroundReference) "NPCs are not reported using the same ground reference."
    Require-Condition ([bool]$raise.targetUsesSameGroundReference) "Targets are not reported using the same ground reference."
    Require-Condition ([bool]$raise.notGisGradeTerrainAccuracy) "Ground raise report must avoid GIS-grade claims."
}

if ($fall) {
    Require-Condition ([bool]$fall.allVisibleGroundCoverTilesHaveColliders) "Fall prevention report says visible cover lacks colliders."
    Require-Condition ([int]$fall.airWallCount -eq 4) "Fall prevention report must keep four air walls."
    Require-Condition ([bool]$fall.playerCannotFallThroughGroundCover) "Fall prevention report does not block ground-cover fall-through."
    Require-Condition ([bool]$fall.playerCannotLeaveMapBounds) "Fall prevention report does not block map exit."
}

if ($blue) {
    Require-Condition ([bool]$blue.visibleRoadLikeCoverEnabled) "Blue-area cover report says visible cover disabled."
    Require-Condition (-not [bool]$blue.coverMaterialBlue) "Ground cover is reported blue."
    Require-Condition (-not [bool]$blue.largeBlueSupportRendererAllowed) "Large blue support renderers are allowed."
    Require-Condition (-not [bool]$blue.knownBlueFallZonesAccessible) "Known blue fall zones are accessible."
}

if ($spawn) {
    Require-Condition ([bool]$spawn.playerSpawnUsesGroundCoverY) "Player spawn does not use ground-cover Y."
    Require-Condition ([bool]$spawn.npcDistributionUsesGroundCoverY) "NPC distribution does not use ground-cover Y."
    Require-Condition ([bool]$spawn.greenFramesAlignToGroundCover) "Green frames do not align to ground cover."
}

if ($readiness) {
    $allowed = @(
        "ready_for_manual_playtest",
        "ready_with_documented_ground_cover_limitations",
        "needs_quick_fix_before_manual_test",
        "blocked"
    )
    Require-Condition ($allowed -contains [string]$readiness.manualReadinessDecision) "Manual readiness decision is invalid for ground cover."
    Require-Condition (-not [bool]$readiness.roadTerrainAccuracyClaimed) "Manual readiness must not claim road/terrain accuracy."
}

if ($script:Failures.Count -gt 0) {
    foreach ($failure in $script:Failures) {
        Write-Host "[FAIL] $failure"
    }
    exit 1
}

Write-Host "[PASS] NewMap ground cover JSON files validated."
exit 0
