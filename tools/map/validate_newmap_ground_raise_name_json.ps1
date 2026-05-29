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

function Is-AddressOrIdName {
    param([string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name)) { return $true }
    $lower = $Name.ToLowerInvariant()
    return $lower.StartsWith("bldg_") -or
        $lower.StartsWith("gml_") -or
        $lower.StartsWith("13102-bldg-") -or
        $lower.Contains("_unknown_") -or
        $lower -eq "unknown" -or
        $lower -eq "unnamed" -or
        ($Name -match "^\s*[-+]?\d+(\.\d+)?\s*,\s*[-+]?\d+(\.\d+)?\s*$") -or
        ($Name -match "東京都.*中央区.*(丁目|番|号)")
}

$raiseConfig = Read-RequiredJson "Assets\Data\P10\newmap_ground_cover_raise_config.json"
$raiseReport = Read-RequiredJson "Assets\Data\P10\newmap_ground_cover_raise_report.json"
$resnap = Read-RequiredJson "Assets\Data\P10\newmap_ground_raise_runtime_resnap_status.json"
$floating = Read-RequiredJson "Assets\Data\P10\newmap_building_floating_after_ground_raise.json"
$nameConfig = Read-RequiredJson "Assets\Data\P10\newmap_name_enrichment_config.json"
$nameCache = Read-RequiredJson "Assets\Data\P10\newmap_name_cache.json"
$nameRules = Read-RequiredJson "Assets\Data\P10\newmap_name_normalization_rules.json"
$labelConfig = Read-RequiredJson "Assets\Data\P10\newmap_name_label_config.json"
$labelReport = Read-RequiredJson "Assets\Data\P10\newmap_name_label_runtime_report.json"
$playerCollision = Read-RequiredJson "Assets\Data\P10\newmap_player_building_collision_report.json"
$npcCollision = Read-RequiredJson "Assets\Data\P10\newmap_npc_building_collision_report.json"
$npcMovementConfig = Read-RequiredJson "Assets\Data\P10\newmap_npc_movement_config.json"
$npcMovementReport = Read-RequiredJson "Assets\Data\P10\newmap_npc_continuous_movement_report.json"
$readiness = Read-RequiredJson "Assets\Data\P10\newmap_manual_playtest_readiness.json"

if ($raiseConfig) {
    Require-Condition ([bool]$raiseConfig.enabled) "Ground cover raise config is disabled."
    Require-Condition ([bool]$raiseConfig.keepImportedBuildingsFixed) "Ground raise must keep imported buildings fixed."
    Require-Condition ([double]$raiseConfig.fallbackRaiseOffsetMeters -gt 0) "Fallback raise offset must be positive."
    Require-Condition ([double]$raiseConfig.maxRaiseOffsetMeters -le 10.0) "Ground raise max cap must be <= 10m."
}

if ($raiseReport) {
    Require-Condition ([bool]$raiseReport.groundCoverRaiseEnabled) "Ground raise report says disabled."
    Require-Condition (-not [bool]$raiseReport.buildingsMoved) "Ground raise report must not move buildings."
    Require-Condition ([double]$raiseReport.selectedRaiseOffset -gt 0) "Selected raise offset missing/zero."
    Require-Condition ([double]$raiseReport.selectedRaiseOffset -le [double]$raiseReport.maxRaiseOffsetMeters) "Selected raise exceeds cap."
}

if ($resnap) {
    Require-Condition ([bool]$resnap.groundCoverRaised) "Runtime resnap report missing raised ground state."
    Require-Condition ([bool]$resnap.npc100xDistributionPreserved) "NPC 100x distribution must be preserved."
    Require-Condition (-not [bool]$resnap.fallThroughRegression) "Fall-through regression flagged."
}

if ($floating) {
    Require-Condition (-not [bool]$floating.importedBuildingsMoved) "Imported buildings must remain fixed for this pass."
    Require-Condition (-not [bool]$floating.failedAdaptiveGridReenabled) "Failed adaptive grid must remain disabled."
}

if ($nameConfig) {
    Require-Condition ([bool]$nameConfig.preprocessingOnly) "Name enrichment must be preprocessing-only."
    Require-Condition (-not [bool]$nameConfig.runtimeNetworkRequestsAllowed) "Name enrichment config allows runtime network."
    Require-Condition ([int]$nameConfig.maxOnlineQueries -le 50) "Online query cap must be bounded."
    Require-Condition ([bool]$nameConfig.onlyQueryMissingNames) "Name enrichment must query only missing names."
}

if ($nameCache) {
    Require-Condition (-not [bool]$nameCache.runtimeNetworkRequestsAllowed) "Name cache allows runtime network."
    Require-Condition ($nameCache.labels.Count -ge 50) "Name cache should contain real source labels."
    $official = @($nameCache.labels | Where-Object { $_.objectType -eq "official_shelter" })
    $candidate = @($nameCache.labels | Where-Object { $_.objectType -eq "candidate" })
    $building = @($nameCache.labels | Where-Object { $_.objectType -eq "building" })
    $road = @($nameCache.labels | Where-Object { $_.objectType -eq "road" })
    Require-Condition ($official.Count -gt 0) "Official shelter labels missing from cache."
    Require-Condition ($candidate.Count -gt 0) "Non-official candidate labels missing from cache."
    Require-Condition ($building.Count -gt 0) "Building labels missing from cache."
    Require-Condition ($road.Count -gt 0) "Road labels missing from cache."
    foreach ($label in $nameCache.labels) {
        Require-Condition (-not (Is-AddressOrIdName ([string]$label.name))) "Address/ID-like label is visible in cache: $($label.id) = $($label.name)"
        Require-Condition ([double]$label.confidence -ge 0.6) "Low-confidence runtime label is present: $($label.id)"
    }
}

if ($nameRules) {
    Require-Condition ([bool]$nameRules.showOnlyMainName) "Name rules must show only main names."
    Require-Condition ([bool]$nameRules.hideFullAddress) "Name rules must hide full addresses."
    Require-Condition (-not [bool]$nameRules.machineTranslationAllowed) "Name rules must forbid machine translation."
    Require-Condition (-not [bool]$nameRules.fabricatedNamesAllowed) "Name rules must forbid fabricated names."
}

if ($labelConfig) {
    Require-Condition (-not [bool]$labelConfig.runtimeNetworkRequestsAllowed) "Runtime label config allows network."
    Require-Condition ([bool]$labelConfig.showOfficialShelterNames) "Official labels disabled."
    Require-Condition ([bool]$labelConfig.showNonOfficialCandidateNames) "Non-official labels disabled."
    Require-Condition ([bool]$labelConfig.showBuildingNames) "Reliable building labels must be enabled from cache."
    Require-Condition ([bool]$labelConfig.showRoadNames) "Reliable road labels must be enabled from cache."
    Require-Condition ([int]$labelConfig.maxVisibleLabels -le 300) "Label cap too high."
}

if ($labelReport) {
    Require-Condition (-not [bool]$labelReport.runtimeNetworkRequestsAllowed) "Runtime label report allows network."
    Require-Condition ([bool]$labelReport.nameCacheExists) "Runtime label report says cache missing."
    Require-Condition ([int]$labelReport.nameCacheRecordCount -ge 50) "Runtime label report cache count too low."
    Require-Condition ([bool]$labelReport.fullAddressesHidden) "Full addresses must be hidden."
    Require-Condition ([bool]$labelReport.idOnlyHiddenInNormalMode) "ID-only labels must be hidden in normal mode."
}

if ($playerCollision) {
    Require-Condition ([bool]$playerCollision.playerBuildingCollisionEnabled) "Player building collision disabled."
    Require-Condition ([bool]$playerCollision.spawnRejectsInsideBuilding) "Spawn must reject inside-building candidates."
}

if ($npcCollision) {
    Require-Condition ([bool]$npcCollision.npcBuildingAvoidanceEnabled) "NPC building avoidance disabled."
    Require-Condition ([bool]$npcCollision.npcSpawnRejectsBuildingOverlap) "NPC spawn must reject building overlap."
    Require-Condition ([bool]$npcCollision.npcMovementAvoidsBuildingBounds) "NPC movement must avoid building bounds."
}

if ($npcMovementConfig) {
    Require-Condition ([bool]$npcMovementConfig.continuousMovementEnabled) "NPC continuous movement disabled."
    Require-Condition ([bool]$npcMovementConfig.stuckRecoveryEnabled) "NPC stuck recovery disabled."
    Require-Condition ([bool]$npcMovementConfig.buildingAvoidanceEnabled) "NPC movement building avoidance disabled."
    Require-Condition (-not [bool]$npcMovementConfig.farNpcStaticProxyMode) "Far static proxy mode should be disabled for continuous movement validation."
}

if ($npcMovementReport) {
    Require-Condition ([bool]$npcMovementReport.continuousMovementEnabled) "NPC movement report says continuous movement disabled."
    Require-Condition (-not [bool]$npcMovementReport.heavyPerNpcPathfinding) "Heavy per-NPC pathfinding must stay disabled."
    Require-Condition ([int]$npcMovementReport.stoppedWithoutReasonCount -eq 0) "NPCs stopped without reason in report."
}

if ($readiness) {
    $allowed = @("ready_for_manual_playtest", "ready_with_documented_building_visual_limitations", "needs_quick_fix_before_manual_test", "blocked")
    Require-Condition ($allowed -contains [string]$readiness.manualReadinessDecision) "Manual readiness decision missing/invalid."
}

if ($script:Failures.Count -gt 0) {
    foreach ($failure in $script:Failures) {
        Write-Host "[FAIL] $failure"
    }
    exit 1
}

Write-Host "[PASS] NewMap ground raise + name JSON files validated."
exit 0
