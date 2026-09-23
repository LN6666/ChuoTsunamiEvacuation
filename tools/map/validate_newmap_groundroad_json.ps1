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

function Require-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Add-Failure $Message
    }
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

$requiredJson = @(
    "Assets\Data\P10\newmap_ground_road_source_validation.json",
    "Assets\Data\P10\newmap_ground_road_height_samples.json",
    "Assets\Data\P10\newmap_ground_road_sampling_report.json",
    "Assets\Data\P10\newmap_adaptive_support_grid_config.json",
    "Assets\Data\P10\newmap_adaptive_support_grid_report.json",
    "Assets\Data\P10\newmap_blue_area_final_fix.json",
    "Assets\Data\P10\newmap_ground_road_merge_report.json",
    "Assets\Data\P10\newmap_height_integration_report.json",
    "Assets\Data\P10\newmap_floating_building_round4_report.json",
    "Assets\Data\P10\newmap_air_wall_regression_report.json",
    "Assets\Data\P10\newmap_groundroad_regression_status.json",
    "Assets\Data\P10\newmap_name_enrichment_config.json",
    "Assets\Data\P10\newmap_name_cache.json",
    "Assets\Data\P10\newmap_name_enrichment_report.json",
    "Assets\Data\P10\newmap_name_label_config.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json",
    "Assets\Data\P10\newmap_manual_playtest_checklist.json"
)

foreach ($relative in $requiredJson) {
    [void](Read-RequiredJson -RelativePath $relative)
}

$source = Read-RequiredJson "Assets\Data\P10\newmap_ground_road_source_validation.json"
if ($source) {
    Require-Condition ([bool]$source.sourceSceneExists) "Ground/road source scene validation says source scene is missing."
    Require-Condition ([bool]$source.sourceSceneValid) "Ground/road source scene validation did not pass."
    Require-Condition ([int]$source.rendererCount -gt 0) "Ground/road source scene has no renderers."
    Require-Condition (([int]$source.colliderCount -gt 0) -or ([int]$source.groundLikeRendererCount -gt 0)) "Ground/road source scene has no collider or ground-like renderer evidence."
    $rootNames = @($source.rootObjectNames)
    foreach ($requiredRoot in @("GroundRoadImportRoot", "ImportedRoadRoot", "ImportedTerrainRoot", "ImportedReliefRoot", "ImportedBridgeRoot", "ImportedWaterRoot", "ImportDiagnosticsRoot")) {
        Require-Condition ($rootNames -contains $requiredRoot) "Source validation is missing required root object: $requiredRoot"
    }
}

$samples = Read-RequiredJson "Assets\Data\P10\newmap_ground_road_height_samples.json"
if ($samples) {
    Require-Condition ([bool]$samples.valid) "Height sample cache is not valid."
    Require-Condition ([int]$samples.totalSamples -gt 0) "Height sample cache is empty."
    Require-Condition (@($samples.samples).Count -gt 0) "Height sample cache has no sample records."
}

$sampling = Read-RequiredJson "Assets\Data\P10\newmap_ground_road_sampling_report.json"
if ($sampling) {
    Require-Condition ([int]$sampling.totalSamples -gt 0) "Sampling report has no total samples."
    Require-Condition ([int]$sampling.usableSupportSamples -gt 0) "Sampling report has no usable support samples."
    $groundSamples = [int]$sampling.roadSamples + [int]$sampling.terrainSamples + [int]$sampling.reliefSamples + [int]$sampling.bridgeSamples + [int]$sampling.fallbackBuildingBaseSamples
    Require-Condition ($groundSamples -gt 0) "Sampling report has no road/terrain/relief/bridge/building fallback samples."
    Require-Condition ([double]$sampling.yMax -ge [double]$sampling.yMin) "Sampling report Y range is invalid."
}

$gridConfig = Read-RequiredJson "Assets\Data\P10\newmap_adaptive_support_grid_config.json"
if ($gridConfig) {
    Require-Condition ([bool]$gridConfig.enabled) "Adaptive support grid config must be enabled."
    Require-Condition (-not [bool]$gridConfig.debugVisualizationEnabled) "Adaptive support grid debug visualization must be disabled by default."
    Require-Condition (-not [bool]$gridConfig.rendererEnabledInNormalMode) "Adaptive support grid renderer must be disabled in normal mode."
}

$grid = Read-RequiredJson "Assets\Data\P10\newmap_adaptive_support_grid_report.json"
if ($grid) {
    Require-Condition ([int]$grid.gridCellCount -gt 0) "Adaptive support grid report has no cells."
    Require-Condition ([int]$grid.colliderCount -ge [int]$grid.gridCellCount) "Adaptive support grid must create colliders for all cells."
    Require-Condition ([bool]$grid.renderersDisabled) "Adaptive support grid renderers are not reported disabled."
    Require-Condition (-not [bool]$grid.blueSupportVisualActive) "Adaptive support grid reports a visible blue support surface."
    Require-Condition ([bool]$grid.playerUsesAdaptiveGrid) "Player is not reported using adaptive support grid."
    Require-Condition ([bool]$grid.npcUsesAdaptiveGrid) "NPCs are not reported using adaptive support grid."
    Require-Condition ([bool]$grid.targetsUseLocalHeight) "Targets are not reported using local support height."
    Require-Condition ([double]$grid.supportYMax -gt [double]$grid.supportYMin) "Adaptive support grid does not show varied local support heights."
    $nonGlobalCells = [int]$grid.cellsUsingRoadSamples + [int]$grid.cellsUsingTerrainSamples + [int]$grid.cellsUsingReliefSamples + [int]$grid.cellsUsingBridgeSamples + [int]$grid.cellsUsingBuildingBaseFallback
    Require-Condition ($nonGlobalCells -gt 0) "Adaptive support grid only uses global fallback cells."
}

$blue = Read-RequiredJson "Assets\Data\P10\newmap_blue_area_final_fix.json"
if ($blue) {
    Require-Condition ([int]$blue.normalModeVisibleSuspectCount -eq 0) "Blue area audit found visible normal-mode suspects."
    Require-Condition (-not [bool]$blue.supportGridRendererVisibleInNormalMode) "Support/grid renderer is visible in normal mode."
    Require-Condition (-not [bool]$blue.largeBlueSupportPlaneVisible) "Large blue support plane is visible."
    Require-Condition (-not [bool]$blue.debugGroundRootActiveByDefault) "Debug ground root is active by default."
}

$merge = Read-RequiredJson "Assets\Data\P10\newmap_ground_road_merge_report.json"
if ($merge) {
    Require-Condition ([bool]$merge.originalSourceScenePreserved) "Supplemental source scene is not reported preserved."
    Require-Condition (-not [bool]$merge.chuoBaseMapModified) "Ground/road merge report says Chuo_BaseMap was modified by report generation."
    Require-Condition ([int]$merge.copiedVisualObjectsCount -eq 0) "Unexpected visual objects were copied into Chuo_BaseMap."
}

$height = Read-RequiredJson "Assets\Data\P10\newmap_height_integration_report.json"
if ($height) {
    foreach ($property in @(
        "playerSpawnUsesAdaptiveSupportCell",
        "randomSpawnRejectsOutsideBounds",
        "randomSpawnRejectsBuildingOverlap",
        "randomSpawnSnapsToLocalSupportHeight",
        "npcSpawnUsesAdaptiveSupportCell",
        "officialShelterMarkersUseLocalSupportHeight",
        "nonOfficialCandidateMarkersUseLocalSupportHeight",
        "greenFramesUseLocalSupportHeight",
        "interactionZonesUseLocalSupportHeight",
        "routeTargetMarkersUseLocalSupportHeight",
        "noPlayerFallThroughExpected",
        "spawnStillAvoidsBuildings",
        "airWallsStillActive"
    )) {
        Require-Condition ([bool]$height.$property) "Height integration report missing true flag: $property"
    }
}

$floating = Read-RequiredJson "Assets\Data\P10\newmap_floating_building_round4_report.json"
if ($floating) {
    Require-Condition ([int]$floating.sampleAreasChecked -gt 0) "Floating-building report has no sampled areas."
    Require-Condition ([string]$floating.limitation -match "not randomly moved") "Floating-building limitation must state buildings were not randomly moved."
}

$air = Read-RequiredJson "Assets\Data\P10\newmap_air_wall_regression_report.json"
if ($air) {
    Require-Condition ([bool]$air.airWallsStillExist) "Air-wall report says air walls do not exist."
    Require-Condition ([bool]$air.airWallsInvisible) "Air-wall report says air walls are visible."
    Require-Condition ([bool]$air.blockMapBoundary) "Air-wall report does not confirm map boundary blocking."
    Require-Condition ([bool]$air.spawnCannotOccurOutside) "Air-wall report does not confirm spawn bounds rejection."
    Require-Condition ([int]$air.expectedColliderCount -eq 4) "Air-wall report must expect four boundary colliders."
    Require-Condition ([int]$air.expectedVisibleRendererCount -eq 0) "Air-wall report must expect no visible renderers."
}

$labelConfig = Read-RequiredJson "Assets\Data\P10\newmap_name_label_config.json"
if ($labelConfig) {
    Require-Condition ([bool]$labelConfig.enabled) "Name label config must be enabled."
    Require-Condition (-not [bool]$labelConfig.runtimeNetworkRequestsAllowed) "Name label runtime config allows network requests."
    Require-Condition (-not [bool]$labelConfig.showIdOnlyLabelsInDebug) "ID-only labels must be hidden in normal mode."
    Require-Condition ([int]$labelConfig.maxVisibleLabels -le 80) "Name labels must remain capped for runtime."
}

$enrichmentConfig = Read-RequiredJson "Assets\Data\P10\newmap_name_enrichment_config.json"
if ($enrichmentConfig) {
    Require-Condition (-not [bool]$enrichmentConfig.runtimeNetworkRequestsAllowed) "Name enrichment config must keep runtime network disabled."
    Require-Condition ([double]$enrichmentConfig.rateLimitSeconds -ge 1.1) "Online preprocessing rate limit is too low."
    Require-Condition ([int]$enrichmentConfig.maxQueriesPerRun -le 200) "Online preprocessing query cap is too high."
}

$nameCache = Read-RequiredJson "Assets\Data\P10\newmap_name_cache.json"
if ($nameCache) {
    Require-Condition (-not [bool]$nameCache.runtimeNetworkRequestsAllowed) "Name cache allows runtime network requests."
    $badLabels = @($nameCache.labels | Where-Object {
        ([bool]$_.idOnly) -or
        ([string]$_.name -match "^\s*(bldg|tran|dem|urf|obj|id)[:_\-]?[0-9a-fA-F\-]{4,}\s*$") -or
        ([string]$_.classification -match "address_only")
    })
    Require-Condition ($badLabels.Count -eq 0) "Name cache contains ID-only or address-only normal labels."
}

$enrichmentReport = Read-RequiredJson "Assets\Data\P10\newmap_name_enrichment_report.json"
if ($enrichmentReport) {
    Require-Condition (-not [bool]$enrichmentReport.runtimeNetworkRequestsAllowed) "Name enrichment report says runtime network is allowed."
    Require-Condition (@("completed", "pending_user_network_run") -contains [string]$enrichmentReport.status) "Unexpected name enrichment status: $($enrichmentReport.status)"
}

$readiness = Read-RequiredJson "Assets\Data\P10\newmap_manual_playtest_readiness.json"
if ($readiness) {
    Require-Condition (-not [string]::IsNullOrWhiteSpace([string]$readiness.manualReadinessDecision)) "Manual readiness decision is missing."
    Require-Condition (@("ready_for_manual_playtest", "ready_with_documented_ground_limitations", "needs_quick_fix_before_manual_test", "blocked") -contains [string]$readiness.manualReadinessDecision) "Manual readiness decision is invalid."
    Require-Condition ([bool]$readiness.runtimeDoesNotAccessNetwork) "Manual readiness must confirm runtime does not access network."
}

if ($script:Failures.Count -gt 0) {
    foreach ($failure in $script:Failures) {
        Write-Host "[FAIL] $failure"
    }
    exit 1
}

Write-Host "[PASS] NewMap ground/road merge JSON files validated."
exit 0
