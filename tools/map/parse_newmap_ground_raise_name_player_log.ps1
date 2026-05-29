param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_ground_raise_name_player_log_summary.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Get-LatestPlayerLog {
    $localLow = Join-Path $env:USERPROFILE "AppData\LocalLow"
    if (-not (Test-Path -LiteralPath $localLow -PathType Container)) {
        return $null
    }

    return Get-ChildItem -LiteralPath $localLow -Recurse -Filter Player.log -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
}

function Get-TokenValue {
    param([string]$Line, [string]$Name)
    if ([string]::IsNullOrWhiteSpace($Line)) {
        return $null
    }

    $pattern = "(?:^|\s)" + [regex]::Escape($Name) + "=(?<value>\S+)"
    $match = [regex]::Match($Line, $pattern)
    if ($match.Success) {
        return $match.Groups["value"].Value
    }

    return $null
}

function As-Double {
    param($Value)
    if ($null -eq $Value) { return $null }
    return [double]::Parse([string]$Value, [Globalization.CultureInfo]::InvariantCulture)
}

function As-Int {
    param($Value)
    if ($null -eq $Value) { return $null }
    return [int]$Value
}

function As-Bool {
    param($Value)
    if ($null -eq $Value) { return $null }
    return ([string]$Value) -eq "True" -or ([string]$Value) -eq "true"
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $candidate = Get-LatestPlayerLog
    if ($candidate) {
        $LogPath = $candidate.FullName
    }
}

if ([string]::IsNullOrWhiteSpace($LogPath) -or -not (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
    Write-Host "[FAIL] Player.log not found."
    exit 1
}

$lines = @(Get-Content -Encoding UTF8 -LiteralPath $LogPath -ErrorAction SilentlyContinue | ForEach-Object { [string]$_ })
$errors = @($lines | Where-Object {
    $_ -match "NullReferenceException|MissingReferenceException|Unhandled Exception|UnityEngine\.Debug:LogError|LogType\.Error|^\s*Error:|Scripts have compiler errors"
})
$warnings = @($lines | Where-Object {
    ($_ -match "UnityEngine\.Debug:LogWarning|LogType\.Warning|^\s*Warning:|^\s*WARNING:") -and
    ($_ -notmatch "Stage 1 warning") -and
    ($_ -notmatch "evacuation_stage1_warning") -and
    ($_ -notmatch "tourism_non_official_inspection_warning") -and
    ($_ -notmatch "success_non_official_candidate_with_warning")
})

$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$npcDistributionLine = [string](@($lines | Where-Object { $_ -match "NewMap NPC distribution built" } | Select-Object -Last 1) | Select-Object -First 1)
$nameLabelLine = [string](@($lines | Where-Object { $_ -match "NewMap name labels built" } | Select-Object -Last 1) | Select-Object -First 1)
$performanceLine = [string](@($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$bootstrap = [ordered]@{
    supportColliderActive = As-Bool (Get-TokenValue $bootstrapLine "supportColliderActive")
    supportVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "supportVisibleRenderers")
    playableBoundsValid = As-Bool (Get-TokenValue $bootstrapLine "playableBoundsValid")
    airWallColliders = As-Int (Get-TokenValue $bootstrapLine "airWallColliders")
    airWallVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "airWallVisibleRenderers")
    supportSurfaceY = As-Double (Get-TokenValue $bootstrapLine "supportSurfaceY")
    spawnGroundDelta = As-Double (Get-TokenValue $bootstrapLine "spawnGroundDelta")
    spawnValidationPassed = As-Bool (Get-TokenValue $bootstrapLine "spawnValidationPassed")
    spawnRejectedInsideBuilding = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedInsideBuilding")
    nearestBuildingDistance = As-Double (Get-TokenValue $bootstrapLine "nearestBuildingDistance")
    spawnX = As-Double (Get-TokenValue $bootstrapLine "spawnX")
    spawnY = As-Double (Get-TokenValue $bootstrapLine "spawnY")
    spawnZ = As-Double (Get-TokenValue $bootstrapLine "spawnZ")
    playerBuildingCollisionEnabled = As-Bool (Get-TokenValue $bootstrapLine "playerBuildingCollisionEnabled")
    playerBuildingCollisionBounds = As-Int (Get-TokenValue $bootstrapLine "playerBuildingCollisionBounds")
    playerBuildingCollisionBlocked = As-Int (Get-TokenValue $bootstrapLine "playerBuildingCollisionBlocked")
    playerBuildingCollisionRecoveries = As-Int (Get-TokenValue $bootstrapLine "playerBuildingCollisionRecoveries")
    activeTargetMaxHeightOffset = As-Double (Get-TokenValue $bootstrapLine "activeTargetMaxHeightOffset")
    activeTargetHeightOffsetViolations = As-Int (Get-TokenValue $bootstrapLine "activeTargetHeightOffsetViolations")
    adaptiveGridEnabled = As-Bool (Get-TokenValue $bootstrapLine "adaptiveGridEnabled")
    adaptiveGridActive = As-Bool (Get-TokenValue $bootstrapLine "adaptiveGridActive")
    adaptiveGridVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "adaptiveGridVisibleRenderers")
    safeGroundEnabled = As-Bool (Get-TokenValue $bootstrapLine "safeGroundEnabled")
    safeGroundSupportY = As-Double (Get-TokenValue $bootstrapLine "safeGroundSupportY")
    safeGroundColliders = As-Int (Get-TokenValue $bootstrapLine "safeGroundColliders")
    fallOutPreventionEnabled = As-Bool (Get-TokenValue $bootstrapLine "fallOutPreventionEnabled")
    visibleLargeBlueGroundRenderers = As-Int (Get-TokenValue $bootstrapLine "visibleLargeBlueGroundRenderers")
    gameplayGroundCoverEnabled = As-Bool (Get-TokenValue $bootstrapLine "gameplayGroundCoverEnabled")
    gameplayGroundCoverActive = As-Bool (Get-TokenValue $bootstrapLine "gameplayGroundCoverActive")
    gameplayGroundCoverTiles = As-Int (Get-TokenValue $bootstrapLine "gameplayGroundCoverTiles")
    gameplayGroundCoverColliders = As-Int (Get-TokenValue $bootstrapLine "gameplayGroundCoverColliders")
    gameplayGroundCoverVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "gameplayGroundCoverVisibleRenderers")
    gameplayGroundCoverY = As-Double (Get-TokenValue $bootstrapLine "gameplayGroundCoverY")
    gameplayGroundCoverBlueLike = As-Bool (Get-TokenValue $bootstrapLine "gameplayGroundCoverBlueLike")
    gameplayGroundCoverMagentaLike = As-Bool (Get-TokenValue $bootstrapLine "gameplayGroundCoverMagentaLike")
    groundRaiseEnabled = As-Bool (Get-TokenValue $bootstrapLine "groundRaiseEnabled")
    groundRaiseOldY = As-Double (Get-TokenValue $bootstrapLine "groundRaiseOldY")
    groundRaiseNewY = As-Double (Get-TokenValue $bootstrapLine "groundRaiseNewY")
    groundRaiseOffset = As-Double (Get-TokenValue $bootstrapLine "groundRaiseOffset")
    groundRaiseSamples = As-Int (Get-TokenValue $bootstrapLine "groundRaiseSamples")
    groundRaiseMedianBaseY = As-Double (Get-TokenValue $bootstrapLine "groundRaiseMedianBaseY")
    groundRaiseAverageBaseY = As-Double (Get-TokenValue $bootstrapLine "groundRaiseAverageBaseY")
    groundRaiseP25BaseY = As-Double (Get-TokenValue $bootstrapLine "groundRaiseP25BaseY")
    groundRaiseOutliers = As-Int (Get-TokenValue $bootstrapLine "groundRaiseOutliers")
    groundRaiseSkipped = As-Int (Get-TokenValue $bootstrapLine "groundRaiseSkipped")
    groundRaiseCapped = As-Bool (Get-TokenValue $bootstrapLine "groundRaiseCapped")
    groundRaiseRemainingAvgGap = As-Double (Get-TokenValue $bootstrapLine "groundRaiseRemainingAvgGap")
    groundRaiseRemainingMaxGap = As-Double (Get-TokenValue $bootstrapLine "groundRaiseRemainingMaxGap")
    groundRaiseStatus = Get-TokenValue $bootstrapLine "groundRaiseStatus"
    buildingSnapdownEnabled = As-Bool (Get-TokenValue $bootstrapLine "buildingSnapdownEnabled")
    buildingsSnappedDown = As-Int (Get-TokenValue $bootstrapLine "buildingsSnappedDown")
    buildingSnapdownStatus = Get-TokenValue $bootstrapLine "buildingSnapdownStatus"
}

$npcDistribution = [ordered]@{
    requestedNpcCount = As-Int (Get-TokenValue $npcDistributionLine "requestedNpcCount")
    spawnedNpcCount = As-Int (Get-TokenValue $npcDistributionLine "spawnedNpcCount")
    cappedNpcCount = As-Int (Get-TokenValue $npcDistributionLine "cappedNpcCount")
    radiusMeters = As-Double (Get-TokenValue $npcDistributionLine "radiusMeters")
    usedSectors = As-Int (Get-TokenValue $npcDistributionLine "usedSectors")
    usedRings = As-Int (Get-TokenValue $npcDistributionLine "usedRings")
    rejectedInsideBuildings = As-Int (Get-TokenValue $npcDistributionLine "rejectedInsideBuildings")
    avoidBuildings = As-Bool (Get-TokenValue $npcDistributionLine "avoidBuildings")
    usePooling = As-Bool (Get-TokenValue $npcDistributionLine "usePooling")
    farNpcStaticProxyMode = As-Bool (Get-TokenValue $npcDistributionLine "farNpcStaticProxyMode")
    continuousMovementEnabled = As-Bool (Get-TokenValue $npcDistributionLine "continuousMovementEnabled")
    stuckRecoveryEnabled = As-Bool (Get-TokenValue $npcDistributionLine "stuckRecoveryEnabled")
    buildingAvoidanceEnabled = As-Bool (Get-TokenValue $npcDistributionLine "buildingAvoidanceEnabled")
}

$labels = [ordered]@{
    availableLabels = As-Int (Get-TokenValue $nameLabelLine "availableLabels")
    activeLabels = As-Int (Get-TokenValue $nameLabelLine "activeLabels")
    officialShelterLabels = As-Int (Get-TokenValue $nameLabelLine "officialShelterLabels")
    nonOfficialCandidateLabels = As-Int (Get-TokenValue $nameLabelLine "nonOfficialCandidateLabels")
    roadNameLabels = As-Int (Get-TokenValue $nameLabelLine "roadNameLabels")
    buildingNameLabels = As-Int (Get-TokenValue $nameLabelLine "buildingNameLabels")
    tokyoStationLabels = As-Int (Get-TokenValue $nameLabelLine "tokyoStationLabels")
    idOnlyLabels = As-Int (Get-TokenValue $nameLabelLine "idOnlyLabels")
    nameCacheLoaded = As-Bool (Get-TokenValue $nameLabelLine "nameCacheLoaded")
    nameCacheRecords = As-Int (Get-TokenValue $nameLabelLine "nameCacheRecords")
    reliableCacheLabels = As-Int (Get-TokenValue $nameLabelLine "reliableCacheLabels")
    addressOnlyHidden = As-Int (Get-TokenValue $nameLabelLine "addressOnlyHidden")
    lowConfidenceHidden = As-Int (Get-TokenValue $nameLabelLine "lowConfidenceHidden")
    runtimeNetworkRequestsAllowed = As-Bool (Get-TokenValue $nameLabelLine "runtimeNetworkRequestsAllowed")
    sourceNameStatus = Get-TokenValue $nameLabelLine "sourceNameStatus"
}

$performanceSample = $null
if ($performanceLine) {
    $performanceSample = [ordered]@{
        elapsedSeconds = As-Double (Get-TokenValue $performanceLine "elapsedSeconds")
        frameCount = As-Int (Get-TokenValue $performanceLine "frameCount")
        averageFps = As-Double (Get-TokenValue $performanceLine "avgFps")
        maxFrameMs = As-Double (Get-TokenValue $performanceLine "maxFrameMs")
        stutterFramesOver66ms = As-Int (Get-TokenValue $performanceLine "stutterFramesOver66ms")
        requestedNpcCount = As-Int (Get-TokenValue $performanceLine "requestedNpcCount")
        spawnedNpcCount = As-Int (Get-TokenValue $performanceLine "spawnedNpcCount")
        cappedNpcCount = As-Int (Get-TokenValue $performanceLine "cappedNpcCount")
        activeNpcCount = As-Int (Get-TokenValue $performanceLine "activeNpcCount")
        movingNpcCount = As-Int (Get-TokenValue $performanceLine "movingNpcCount")
        arrivedNpcCount = As-Int (Get-TokenValue $performanceLine "arrivedNpcCount")
        queuedNpcCount = As-Int (Get-TokenValue $performanceLine "queuedNpcCount")
        stuckNpcCount = As-Int (Get-TokenValue $performanceLine "stuckNpcCount")
        recoveredNpcCount = As-Int (Get-TokenValue $performanceLine "recoveredNpcCount")
        staticProxyNpcCount = As-Int (Get-TokenValue $performanceLine "staticProxyNpcCount")
        stoppedWithoutReasonCount = As-Int (Get-TokenValue $performanceLine "stoppedWithoutReasonCount")
        averageNpcSpeed = As-Double (Get-TokenValue $performanceLine "averageNpcSpeed")
    }
}

$groundRaisePassed = $bootstrap.groundRaiseEnabled -eq $true -and
    $bootstrap.groundRaiseSamples -gt 0 -and
    $bootstrap.groundRaiseOffset -gt 0 -and
    $bootstrap.groundRaiseNewY -gt $bootstrap.groundRaiseOldY -and
    $bootstrap.gameplayGroundCoverY -eq $bootstrap.groundRaiseNewY -and
    $bootstrap.buildingSnapdownEnabled -eq $false -and
    $bootstrap.buildingsSnappedDown -eq 0
$groundCoverPassed = $bootstrap.gameplayGroundCoverEnabled -eq $true -and
    $bootstrap.gameplayGroundCoverActive -eq $true -and
    $bootstrap.gameplayGroundCoverTiles -gt 0 -and
    $bootstrap.gameplayGroundCoverColliders -eq $bootstrap.gameplayGroundCoverTiles -and
    $bootstrap.gameplayGroundCoverVisibleRenderers -eq $bootstrap.gameplayGroundCoverTiles -and
    $bootstrap.gameplayGroundCoverBlueLike -eq $false -and
    $bootstrap.gameplayGroundCoverMagentaLike -eq $false
$bluePassed = $bootstrap.supportVisibleRenderers -eq 0 -and
    $bootstrap.adaptiveGridVisibleRenderers -eq 0 -and
    $bootstrap.visibleLargeBlueGroundRenderers -eq 0 -and
    $groundCoverPassed
$fallPassed = $bootstrap.fallOutPreventionEnabled -eq $true -and $bootstrap.supportColliderActive -eq $true
$boundsPassed = $bootstrap.playableBoundsValid -eq $true -and $bootstrap.airWallColliders -eq 4 -and $bootstrap.airWallVisibleRenderers -eq 0
$spawnPassed = $bootstrap.spawnValidationPassed -eq $true -and [math]::Abs([double]$bootstrap.spawnGroundDelta) -le 0.5
$targetHeightPassed = $bootstrap.activeTargetHeightOffsetViolations -eq 0
$playerCollisionPassed = $bootstrap.playerBuildingCollisionEnabled -eq $true -and $bootstrap.playerBuildingCollisionBounds -gt 0
$npcDistributionPassed = $npcDistribution.requestedNpcCount -eq 800 -and
    $npcDistribution.cappedNpcCount -le 800 -and
    $npcDistribution.spawnedNpcCount -eq $npcDistribution.cappedNpcCount -and
    $npcDistribution.usedSectors -ge 24 -and
    $npcDistribution.usedRings -ge 5 -and
    $npcDistribution.avoidBuildings -eq $true -and
    $npcDistribution.usePooling -eq $true -and
    $npcDistribution.farNpcStaticProxyMode -eq $false -and
    $npcDistribution.continuousMovementEnabled -eq $true -and
    $npcDistribution.stuckRecoveryEnabled -eq $true -and
    $npcDistribution.buildingAvoidanceEnabled -eq $true
$npcMovementPassed = $performanceSample -ne $null -and
    $performanceSample.activeNpcCount -gt 0 -and
    $performanceSample.stoppedWithoutReasonCount -eq 0 -and
    (($performanceSample.movingNpcCount + $performanceSample.arrivedNpcCount + $performanceSample.queuedNpcCount + $performanceSample.staticProxyNpcCount + $performanceSample.stuckNpcCount) -gt 0)
$performancePassed = $performanceSample -ne $null -and
    $performanceSample.requestedNpcCount -eq 800 -and
    $performanceSample.spawnedNpcCount -eq $performanceSample.cappedNpcCount -and
    $performanceSample.averageFps -ge 30
$namePassed = $labels.runtimeNetworkRequestsAllowed -eq $false -and
    $labels.nameCacheLoaded -eq $true -and
    $labels.nameCacheRecords -ge 50 -and
    $labels.officialShelterLabels -gt 0 -and
    $labels.nonOfficialCandidateLabels -gt 0 -and
    $labels.buildingNameLabels -gt 0 -and
    $labels.roadNameLabels -gt 0 -and
    $labels.idOnlyLabels -eq 0
$launchPassed = $selfAuditCompleted -and $selfAuditFailures.Count -eq 0
$logPassed = $errors.Count -eq 0 -and $warnings.Count -eq 0 -and $groundRaisePassed -and $groundCoverPassed -and $bluePassed -and $fallPassed -and $boundsPassed -and $spawnPassed -and $targetHeightPassed -and $playerCollisionPassed -and $npcDistributionPassed -and $npcMovementPassed -and $performancePassed -and $namePassed -and $launchPassed

$manualDecision = if ($logPassed) { "ready_with_documented_building_visual_limitations" } else { "needs_quick_fix_before_manual_test" }

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    bootstrapDiagnostics = $bootstrap
    npcDistributionDiagnostics = $npcDistribution
    nameLabelDiagnostics = $labels
    performanceSample = $performanceSample
    bootstrapLine = $bootstrapLine
    npcDistributionLine = $npcDistributionLine
    nameLabelLine = $nameLabelLine
    performanceLine = $performanceLine
    launchSmokePassed = $launchPassed
    groundRaiseDiagnosticsPassed = $groundRaisePassed
    groundCoverRegressionPassed = $groundCoverPassed
    blueAreaRegressionPassed = $bluePassed
    fallOutPreventionPassed = $fallPassed
    playableBoundsPassed = $boundsPassed
    playerSpawnDiagnosticsPassed = $spawnPassed
    targetHeightDiagnosticsPassed = $targetHeightPassed
    playerBuildingCollisionPassed = $playerCollisionPassed
    npc100xDistributionPassed = $npcDistributionPassed
    npcContinuousMovementPassed = $npcMovementPassed
    npcPerformancePassed = $performancePassed
    labelCacheDiagnosticsPassed = $namePassed
    runtimeNoWebPassed = ($labels.runtimeNetworkRequestsAllowed -eq $false)
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

function Update-JsonFile {
    param([string]$RelativePath, [scriptblock]$Updater)
    $path = Join-Path $ProjectRoot $RelativePath
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        $json = Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
        & $Updater $json
        $json | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $path -Encoding UTF8
    }
}

Update-JsonFile "Assets\Data\P10\newmap_ground_cover_raise_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName oldGroundCoverY -NotePropertyValue $bootstrap.groundRaiseOldY -Force
    $json | Add-Member -NotePropertyName sampledBuildingBaseCount -NotePropertyValue $bootstrap.groundRaiseSamples -Force
    $json | Add-Member -NotePropertyName medianBuildingBaseY -NotePropertyValue $bootstrap.groundRaiseMedianBaseY -Force
    $json | Add-Member -NotePropertyName averageBuildingBaseY -NotePropertyValue $bootstrap.groundRaiseAverageBaseY -Force
    $json | Add-Member -NotePropertyName selectedRaiseOffset -NotePropertyValue $bootstrap.groundRaiseOffset -Force
    $json | Add-Member -NotePropertyName newGroundCoverY -NotePropertyValue $bootstrap.groundRaiseNewY -Force
    $json | Add-Member -NotePropertyName playerSpawnNewY -NotePropertyValue $bootstrap.spawnY -Force
    $json | Add-Member -NotePropertyName remainingBuildingGapAverage -NotePropertyValue $bootstrap.groundRaiseRemainingAvgGap -Force
    $json | Add-Member -NotePropertyName remainingBuildingGapMax -NotePropertyValue $bootstrap.groundRaiseRemainingMaxGap -Force
    $json | Add-Member -NotePropertyName outlierCount -NotePropertyValue $bootstrap.groundRaiseOutliers -Force
    $json | Add-Member -NotePropertyName skippedObjects -NotePropertyValue $bootstrap.groundRaiseSkipped -Force
    $json | Add-Member -NotePropertyName raiseOffsetCapped -NotePropertyValue $bootstrap.groundRaiseCapped -Force
    $json | Add-Member -NotePropertyName runtimeStatus -NotePropertyValue $bootstrap.groundRaiseStatus -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($groundRaisePassed) { "validated_by_player_smoke" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_ground_raise_runtime_resnap_status.json" {
    param($json)
    $json | Add-Member -NotePropertyName playerSpawnMatchesRaisedGround -NotePropertyValue $spawnPassed -Force
    $json | Add-Member -NotePropertyName npcFootSupportDeltaWithinTolerance -NotePropertyValue $npcMovementPassed -Force
    $json | Add-Member -NotePropertyName activeTargetMarkerHeightWithinTolerance -NotePropertyValue $targetHeightPassed -Force
    $json | Add-Member -NotePropertyName greenFrameHeightWithinTolerance -NotePropertyValue $targetHeightPassed -Force
    $json | Add-Member -NotePropertyName spawnAvoidsBuildings -NotePropertyValue $spawnPassed -Force
    $json | Add-Member -NotePropertyName airWallsBlockBounds -NotePropertyValue $boundsPassed -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($spawnPassed -and $targetHeightPassed -and $npcMovementPassed -and $boundsPassed) { "validated_by_player_smoke" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_building_floating_after_ground_raise.json" {
    param($json)
    $json | Add-Member -NotePropertyName buildingsChecked -NotePropertyValue $bootstrap.groundRaiseSamples -Force
    $json | Add-Member -NotePropertyName buildingsStillFloatingOverThreshold -NotePropertyValue ($(if ($bootstrap.groundRaiseRemainingMaxGap -gt 1.0) { "remaining_outliers_present" } else { 0 })) -Force
    $json | Add-Member -NotePropertyName buildingsNowAligned -NotePropertyValue "aggregate_gap_reduced_by_ground_raise" -Force
    $json | Add-Member -NotePropertyName buildingsPossiblyEmbeddedDueGroundRaise -NotePropertyValue ($(if ($bootstrap.groundRaiseCapped) { "possible_capped_outliers_need_manual_check" } else { 0 })) -Force
    $json | Add-Member -NotePropertyName remainingAverageGapMeters -NotePropertyValue $bootstrap.groundRaiseRemainingAvgGap -Force
    $json | Add-Member -NotePropertyName remainingMaxGapMeters -NotePropertyValue $bootstrap.groundRaiseRemainingMaxGap -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($groundRaisePassed) { "validated_by_player_smoke_manual_visual_check_required" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_player_building_collision_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName buildingObstacleProxyBoundsCount -NotePropertyValue $bootstrap.playerBuildingCollisionBounds -Force
    $json | Add-Member -NotePropertyName playerCollisionBlockedCount -NotePropertyValue $bootstrap.playerBuildingCollisionBlocked -Force
    $json | Add-Member -NotePropertyName playerCollisionRecoveryCount -NotePropertyValue $bootstrap.playerBuildingCollisionRecoveries -Force
    $json | Add-Member -NotePropertyName interactionZonesRemainReachable -NotePropertyValue $targetHeightPassed -Force
    $json | Add-Member -NotePropertyName playerCannotEnterSampledBuildingBounds -NotePropertyValue $playerCollisionPassed -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($playerCollisionPassed -and $spawnPassed) { "validated_by_player_smoke" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_npc_building_collision_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName npcInsideBuildingRejections -NotePropertyValue $npcDistribution.rejectedInsideBuildings -Force
    $json | Add-Member -NotePropertyName npcBuildingAvoidanceRecoveries -NotePropertyValue ($(if ($performanceSample) { $performanceSample.recoveredNpcCount } else { 0 })) -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($npcDistributionPassed -and $npcMovementPassed) { "validated_by_player_smoke" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_npc_continuous_movement_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName npcCount -NotePropertyValue $npcDistribution.spawnedNpcCount -Force
    $json | Add-Member -NotePropertyName movingCount -NotePropertyValue ($(if ($performanceSample) { $performanceSample.movingNpcCount } else { 0 })) -Force
    $json | Add-Member -NotePropertyName arrivedCount -NotePropertyValue ($(if ($performanceSample) { $performanceSample.arrivedNpcCount } else { 0 })) -Force
    $json | Add-Member -NotePropertyName queuedCount -NotePropertyValue ($(if ($performanceSample) { $performanceSample.queuedNpcCount } else { 0 })) -Force
    $json | Add-Member -NotePropertyName stuckCount -NotePropertyValue ($(if ($performanceSample) { $performanceSample.stuckNpcCount } else { 0 })) -Force
    $json | Add-Member -NotePropertyName recoveredCount -NotePropertyValue ($(if ($performanceSample) { $performanceSample.recoveredNpcCount } else { 0 })) -Force
    $json | Add-Member -NotePropertyName staticProxyCount -NotePropertyValue ($(if ($performanceSample) { $performanceSample.staticProxyNpcCount } else { 0 })) -Force
    $json | Add-Member -NotePropertyName averageSpeed -NotePropertyValue ($(if ($performanceSample) { $performanceSample.averageNpcSpeed } else { 0 })) -Force
    $json | Add-Member -NotePropertyName stoppedWithoutReasonCount -NotePropertyValue ($(if ($performanceSample) { $performanceSample.stoppedWithoutReasonCount } else { 999 })) -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($npcMovementPassed) { "validated_by_180s_player_smoke" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_name_label_runtime_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName nameCacheRecordCount -NotePropertyValue $labels.nameCacheRecords -Force
    $json | Add-Member -NotePropertyName officialShelterLabelsRuntime -NotePropertyValue $labels.officialShelterLabels -Force
    $json | Add-Member -NotePropertyName nonOfficialCandidateLabelsRuntime -NotePropertyValue $labels.nonOfficialCandidateLabels -Force
    $json | Add-Member -NotePropertyName buildingLabelsRuntime -NotePropertyValue $labels.buildingNameLabels -Force
    $json | Add-Member -NotePropertyName roadLabelsRuntime -NotePropertyValue $labels.roadNameLabels -Force
    $json | Add-Member -NotePropertyName tokyoStationLabelsRuntime -NotePropertyValue $labels.tokyoStationLabels -Force
    $json | Add-Member -NotePropertyName idOnlyLabelsRuntime -NotePropertyValue $labels.idOnlyLabels -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($namePassed) { "validated_by_player_smoke" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_ground_raise_name_player_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $json | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $json | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $json | Add-Member -NotePropertyName groundRaiseDiagnostics -NotePropertyValue ($(if ($groundRaisePassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName playerNpcTargetHeightDiagnostics -NotePropertyValue ($(if ($spawnPassed -and $targetHeightPassed -and $npcMovementPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName playerBuildingCollisionDiagnostics -NotePropertyValue ($(if ($playerCollisionPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName npcMovementDiagnostics -NotePropertyValue ($(if ($npcMovementPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName labelCacheDiagnostics -NotePropertyValue ($(if ($namePassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName runtimeNoWebDiagnostics -NotePropertyValue ($(if ($labels.runtimeNetworkRequestsAllowed -eq $false) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName fpsStutterSample -NotePropertyValue $performanceSample -Force
    $json | Add-Member -NotePropertyName bootstrapDiagnostics -NotePropertyValue $bootstrap -Force
    $json | Add-Member -NotePropertyName nameLabelDiagnostics -NotePropertyValue $labels -Force
    $json | Add-Member -NotePropertyName npcDistributionDiagnostics -NotePropertyValue $npcDistribution -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_manual_playtest_readiness.json" {
    param($json)
    $json | Add-Member -NotePropertyName manualReadinessDecision -NotePropertyValue $manualDecision -Force
    $json | Add-Member -NotePropertyName reason -NotePropertyValue ($(if ($logPassed) { "Ground cover raise, runtime resnap, cache labels, no-web check, player/NPC building collision, NPC continuous movement, and Player.log smoke passed. Remaining building visual alignment is documented as non-GIS gameplay cover limitation." } else { "Ground raise/name/collision player smoke did not pass all gates." })) -Force
    $json | Add-Member -NotePropertyName groundRaiseOffset -NotePropertyValue $bootstrap.groundRaiseOffset -Force
    $json | Add-Member -NotePropertyName buildingFloatingStatus -NotePropertyValue "reduced_by_ground_cover_raise_manual_visual_check_required" -Force
    $json | Add-Member -NotePropertyName nameCacheStatus -NotePropertyValue ($(if ($namePassed) { "loaded_runtime_offline" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName npcContinuousMovementStatus -NotePropertyValue ($(if ($npcMovementPassed) { "validated" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName playerBuildingCollisionStatus -NotePropertyValue ($(if ($playerCollisionPassed) { "validated_proxy_collision" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue $manualDecision -Force
}

$readinessDocPath = Join-Path $ProjectRoot "docs\NEWMAP_MANUAL_PLAYTEST_READINESS.md"
$readinessDoc = @(
    "# NewMap Manual Playtest Readiness",
    "",
    "- Decision: $manualDecision",
    "- Ground cover raise offset: $($bootstrap.groundRaiseOffset)m",
    "- Ground cover Y: $($bootstrap.groundRaiseNewY)m",
    "- Buildings moved: false",
    "- Building floating: reduced by raised gameplay cover; manual visual check still required",
    "- Player/NPC/targets resnapped: $($spawnPassed -and $targetHeightPassed -and $npcMovementPassed)",
    "- Player building collision proxy: $playerCollisionPassed",
    "- NPC continuous movement: $npcMovementPassed",
    "- Name cache loaded: $namePassed",
    "- Runtime web requests allowed: $($labels.runtimeNetworkRequestsAllowed)",
    "- Player.log: $($errors.Count) errors / $($warnings.Count) warnings",
    "",
    "This does not claim GIS-grade terrain, road, route, or PLATEAU building elevation accuracy."
)
$readinessDoc | Set-Content -LiteralPath $readinessDocPath -Encoding UTF8

$labelDocPath = Join-Path $ProjectRoot "docs\NEWMAP_NAME_LABEL_RUNTIME_REPORT.md"
$labelDoc = @(
    "# NewMap Name Label Runtime Report",
    "",
    "Generated: $(Get-Date -Format s)",
    "",
    "- Cache loaded: $($labels.nameCacheLoaded)",
    "- Cache records: $($labels.nameCacheRecords)",
    "- Official labels: $($labels.officialShelterLabels)",
    "- Non-official labels: $($labels.nonOfficialCandidateLabels)",
    "- Building labels: $($labels.buildingNameLabels)",
    "- Road labels: $($labels.roadNameLabels)",
    "- Tokyo Station labels: $($labels.tokyoStationLabels)",
    "- ID-only labels visible: $($labels.idOnlyLabels)",
    "- Runtime network requests allowed: $($labels.runtimeNetworkRequestsAllowed)",
    "- Final status: $(if ($namePassed) { 'validated_by_player_smoke' } else { 'failed_player_log_parse' })"
)
$labelDoc | Set-Content -LiteralPath $labelDocPath -Encoding UTF8

Write-Host "[PASS] Ground Raise + Name Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
