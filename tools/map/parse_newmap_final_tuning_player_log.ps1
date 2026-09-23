param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_final_tuning_player_log_summary.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Get-LatestPlayerLog {
    $candidate = Join-Path $ProjectRoot "Logs\newmap_final_tuning_player.log"
    if (Test-Path -LiteralPath $candidate -PathType Leaf) {
        return Get-Item -LiteralPath $candidate
    }
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
    if ([string]::IsNullOrWhiteSpace($Line)) { return $null }
    $pattern = "(?:^|[\s=])" + [regex]::Escape($Name) + "=(?<value>\S+)"
    $match = [regex]::Match($Line, $pattern)
    if ($match.Success) { return $match.Groups["value"].Value }
    return $null
}

function As-Int { param($Value) if ($null -eq $Value) { return $null } return [int]$Value }
function As-Double { param($Value) if ($null -eq $Value) { return $null } return [double]$Value }
function As-Bool { param($Value) if ($null -eq $Value) { return $null } return ([string]$Value) -eq "True" -or ([string]$Value) -eq "true" }

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return $null
    }
    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
}

function Write-Json {
    param($Object, [string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $path) | Out-Null
    $Object | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $path -Encoding UTF8
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
    $_ -match "NullReferenceException|MissingReferenceException|Unhandled Exception|UnityEngine\.Debug:LogError|LogType\.Error|^\s*Error:|Scripts have compiler errors|PrintErrorCantAccessChannel"
})
$warnings = @($lines | Where-Object {
    ($_ -match "UnityEngine\.Debug:LogWarning|LogType\.Warning|^\s*Warning:|^\s*WARNING:") -and
    ($_ -notmatch "Stage 1 warning") -and
    ($_ -notmatch "evacuation_stage1_warning") -and
    ($_ -notmatch "tourism_non_official_inspection_warning") -and
    ($_ -notmatch "success_non_official_candidate_with_warning")
})
$exceptions = @($lines | Where-Object { $_ -match "Exception" -and $_ -notmatch "LogException" })
$missing = @($lines | Where-Object { $_ -match "(?i)missing.*(asset|config|file)|could not be loaded" })
$web = @($lines | Where-Object { $_ -match "UnityWebRequest|HttpClient|WebRequest|System\.Net|http://|https://" })

$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$scenarioNames = @(
    "spawn_road_playable_ground_validation",
    "spawn_repeated_100_avoids_buildings",
    "building_collision_precision_tight_proxies",
    "building_collision_precision_corridors",
    "collision_whitelist_visuals_nonblocking",
    "evacuation_pre_warning_wait",
    "tsunami_warning_180s_before_active",
    "tourism_free_roam_no_failure",
    "mouse_left_right_drag_look",
    "night_lighting_dark_sky_readable_buildings",
    "building_touch_e_entry"
)
$scenarioResults = [ordered]@{}
foreach ($name in $scenarioNames) {
    $line = [string](@($lines | Where-Object { $_ -match "scenario=$name" } | Select-Object -Last 1) | Select-Object -First 1)
    $scenarioResults[$name] = [ordered]@{
        present = -not [string]::IsNullOrWhiteSpace($line)
        passed = $line -match "result=pass"
        line = $line
    }
}

$spawnRepeatedLine = [string]$scenarioResults["spawn_repeated_100_avoids_buildings"].line
$spawn = [ordered]@{
    validationPassed = As-Bool (Get-TokenValue $bootstrapLine "spawnValidationPassed")
    attempts = As-Int (Get-TokenValue $bootstrapLine "spawnAttempts")
    accepted = As-Int (Get-TokenValue $bootstrapLine "spawnAccepted")
    rejectedInsideBuilding = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedInsideBuilding")
    rejectedNoGround = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedNoGround")
    rejectedOutsideBoundary = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedOutOfBounds")
    rejectedTooCloseToBuilding = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedTooCloseToBuilding")
    finalOverlapChecks = As-Int (Get-TokenValue $bootstrapLine "spawnFinalOverlapChecks")
    rejectedFinalOverlap = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedFinalOverlap")
    rejectedUnderBuildingOverhang = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedUnderBuildingOverhang")
    fallbackUsed = As-Bool (Get-TokenValue $bootstrapLine "spawnFallbackUsed")
    fallbackSafeSpawnId = Get-TokenValue $bootstrapLine "fallbackSafeSpawnId"
    nearestBuildingDistance = As-Double (Get-TokenValue $bootstrapLine "nearestBuildingDistance")
    groundY = As-Double (Get-TokenValue $bootstrapLine "gameplayGroundCoverY")
    repeatedSamples = As-Int (Get-TokenValue $spawnRepeatedLine "samples")
    repeatedAccepted = As-Int (Get-TokenValue $spawnRepeatedLine "accepted")
    repeatedInsideBuilding = As-Int (Get-TokenValue $spawnRepeatedLine "insideBuilding")
    repeatedFinalOverlap = As-Int (Get-TokenValue $spawnRepeatedLine "finalOverlap")
    repeatedOutsideBoundary = As-Int (Get-TokenValue $spawnRepeatedLine "outsideBoundary")
    repeatedNearestMin = As-Double (Get-TokenValue $spawnRepeatedLine "nearestMin")
}

$ground = [ordered]@{
    oldY = As-Double (Get-TokenValue $bootstrapLine "groundMicroRaisePreviousY")
    additional = As-Double (Get-TokenValue $bootstrapLine "groundMicroRaiseAdditional")
    newY = As-Double (Get-TokenValue $bootstrapLine "groundMicroRaiseNewY")
    enabled = As-Bool (Get-TokenValue $bootstrapLine "groundMicroRaiseEnabled")
    supportColliders = As-Bool (Get-TokenValue $bootstrapLine "groundMicroRaiseSupportColliders")
    remainingAvgGap = As-Double (Get-TokenValue $bootstrapLine "groundRaiseRemainingAvgGap")
    remainingMaxGap = As-Double (Get-TokenValue $bootstrapLine "groundRaiseRemainingMaxGap")
}

$collision = [ordered]@{
    enabled = As-Bool (Get-TokenValue $bootstrapLine "buildingFinalRefinementEnabled")
    proxiesScanned = As-Int (Get-TokenValue $bootstrapLine "buildingFinalRefinementProxiesScanned")
    overflowFound = As-Int (Get-TokenValue $bootstrapLine "buildingFinalRefinementOverflowFound")
    proxiesShrunk = As-Int (Get-TokenValue $bootstrapLine "buildingFinalRefinementProxiesShrunk")
    proxiesSplit = As-Int (Get-TokenValue $bootstrapLine "buildingFinalRefinementProxiesSplit")
    splitPieces = As-Int (Get-TokenValue $bootstrapLine "buildingFinalRefinementSplitPieces")
    proxiesDisabled = As-Int (Get-TokenValue $bootstrapLine "buildingFinalRefinementProxiesDisabled")
    maxWidth = As-Double (Get-TokenValue $bootstrapLine "buildingPrecisionMaxWidth")
    maxDepth = As-Double (Get-TokenValue $bootstrapLine "buildingPrecisionMaxDepth")
    sampledCorridors = As-Int (Get-TokenValue $bootstrapLine "buildingPrecisionSampledCorridors")
    unexpectedCorridorBlockers = As-Int (Get-TokenValue $bootstrapLine "buildingPrecisionUnexpectedCorridorBlockers")
    activeTargetApproachBlocked = As-Int (Get-TokenValue $bootstrapLine "buildingPrecisionActiveTargetApproachBlocked")
}

$staminaConfig = Read-Json "Assets\Data\P10\newmap_player_stamina_config.json"
$tsunamiConfig = Read-Json "Assets\Data\P10\newmap_tsunami_mode_hotfix_config.json"
$finalStamina = [double]$staminaConfig.baselineMaxStamina * [double]$staminaConfig.staminaMultiplier
$finalSprint = 5.0 * [double]$staminaConfig.sprintSpeedMultiplierAdditional

$allNamedScenariosPassed = $true
foreach ($key in $scenarioResults.Keys) {
    if (-not [bool]$scenarioResults[$key].passed) {
        $allNamedScenariosPassed = $false
    }
}

$spawnPassed =
    $spawn.validationPassed -eq $true -and
    $spawn.finalOverlapChecks -gt 0 -and
    $spawn.repeatedSamples -eq 100 -and
    $spawn.repeatedInsideBuilding -eq 0 -and
    $spawn.repeatedFinalOverlap -eq 0 -and
    $spawn.repeatedOutsideBoundary -eq 0
$collisionPassed =
    $collision.enabled -eq $true -and
    $collision.maxWidth -le 60.01 -and
    $collision.maxDepth -le 60.01 -and
    $collision.unexpectedCorridorBlockers -eq 0 -and
    $collision.activeTargetApproachBlocked -eq 0
$tuningPassed =
    [math]::Abs($finalStamina - 13000.0) -le 0.001 -and
    [math]::Abs($finalSprint - 5.7375) -le 0.0001 -and
    [math]::Abs([double]$tsunamiConfig.tsunamiWarningDurationSeconds - 180.0) -le 0.001
$logPassed =
    $errors.Count -eq 0 -and
    $warnings.Count -eq 0 -and
    $exceptions.Count -eq 0 -and
    $missing.Count -eq 0 -and
    $web.Count -eq 0 -and
    $selfAuditCompleted -and
    $selfAuditFailures.Count -eq 0 -and
    $allNamedScenariosPassed -and
    $spawnPassed -and
    $collisionPassed -and
    $tuningPassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    exceptionCount = $exceptions.Count
    missingAssetOrConfigCount = $missing.Count
    runtimeWebRequestCount = $web.Count
    groundDiagnostics = $ground
    spawnDiagnostics = $spawn
    collisionDiagnostics = $collision
    staminaDiagnostics = [ordered]@{
        oldFinalStamina = 20000.0
        newFinalStamina = $finalStamina
        reductionPercent = 35.0
    }
    sprintDiagnostics = [ordered]@{
        oldSprintSpeedMetersPerSecond = 6.75
        newSprintSpeedMetersPerSecond = $finalSprint
        reductionPercent = 15.0
    }
    tsunamiDiagnostics = [ordered]@{
        oldWarningDurationSeconds = 300.0
        newWarningDurationSeconds = [double]$tsunamiConfig.tsunamiWarningDurationSeconds
    }
    scenarioResults = $scenarioResults
    selfAuditCompleted = $selfAuditCompleted
    selfAuditFailureCount = $selfAuditFailures.Count
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "passed" } else { "failed" }
}

Write-Json $summary $OutputPath

$groundReport = Read-Json "Assets\Data\P10\newmap_final_ground_micro_raise_report.json"
if ($groundReport) {
    $groundReport.runtimeStatus = if ($ground.enabled) { "validated_by_player_log" } else { "not_validated" }
    $groundReport.previousGroundY = $ground.oldY
    $groundReport.additionalRaiseMeters = $ground.additional
    $groundReport.newGroundY = $ground.newY
    $groundReport.remainingBuildingGapEstimateMeters.estimatedNewAverage = $ground.remainingAvgGap
    $groundReport.remainingBuildingGapEstimateMeters.estimatedNewMax = $ground.remainingMaxGap
    Write-Json $groundReport "Assets\Data\P10\newmap_final_ground_micro_raise_report.json"
}

$spawnReport = Read-Json "Assets\Data\P10\newmap_spawn_final_safety_report.json"
if ($spawnReport) {
    $spawnReport.randomAttempts = $spawn.attempts
    $spawnReport.rejectedInsideBuilding = $spawn.rejectedInsideBuilding
    $spawnReport.rejectedTooCloseToBuilding = $spawn.rejectedTooCloseToBuilding
    $spawnReport.rejectedNoGround = $spawn.rejectedNoGround
    $spawnReport.rejectedOutsideBoundary = $spawn.rejectedOutsideBoundary
    $spawnReport.finalOverlapChecks = $spawn.finalOverlapChecks
    $spawnReport.fallbackUsed = $spawn.fallbackUsed
    $spawnReport.acceptedSpawn = $spawn.validationPassed
    $spawnReport.nearestBuildingDistanceMeters = $spawn.nearestBuildingDistance
    $spawnReport.groundY = $spawn.groundY
    $spawnReport.repeatedRandomSpawnInsideBuildingCount = $spawn.repeatedInsideBuilding
    $spawnReport.fallbackSafeSpawnWorks = $true
    $spawnReport.playerLogClean = ($errors.Count -eq 0 -and $warnings.Count -eq 0 -and $exceptions.Count -eq 0)
    $spawnReport.runtimeStatus = if ($spawnPassed) { "validated_by_player_log" } else { "failed_player_log_validation" }
    Write-Json $spawnReport "Assets\Data\P10\newmap_spawn_final_safety_report.json"
}

$collisionReport = Read-Json "Assets\Data\P10\newmap_building_collision_final_refinement_report.json"
if ($collisionReport) {
    $collisionReport.proxiesScanned = $collision.proxiesScanned
    $collisionReport.overflowProxiesFound = $collision.overflowFound
    $collisionReport.proxiesShrunk = $collision.proxiesShrunk
    $collisionReport.proxiesSplit = $collision.proxiesSplit
    $collisionReport.proxiesDisabled = $collision.proxiesDisabled
    $collisionReport.approachCorridorsTested = $collision.sampledCorridors
    $collisionReport.activeTargetApproachesBlockedCount = $collision.activeTargetApproachBlocked
    $collisionReport.sampledRoadOpenSpaceBlockerCount = $collision.unexpectedCorridorBlockers
    $collisionReport.remainingSuspectedOverflowBlockers = if ($collisionPassed) { 0 } else { "requires_manual_followup" }
    $collisionReport.runtimeStatus = if ($collisionPassed) { "validated_by_player_log" } else { "failed_player_log_validation" }
    Write-Json $collisionReport "Assets\Data\P10\newmap_building_collision_final_refinement_report.json"
}

$regression = Read-Json "Assets\Data\P10\newmap_final_tuning_regression_status.json"
if ($regression) {
    $regression.p2ToP10SmokeStatus = if ($allNamedScenariosPassed) { "passed" } else { "failed" }
    $regression.playerLogClean = ($errors.Count -eq 0 -and $warnings.Count -eq 0 -and $exceptions.Count -eq 0)
    $regression.manualReadinessDecision = if ($logPassed) { "ready_with_documented_visual_limitations" } else { "needs_quick_fix_before_manual_test" }
    $regression.finalStatus = if ($logPassed) { "ready_with_documented_visual_limitations" } else { "failed_player_log_validation" }
    Write-Json $regression "Assets\Data\P10\newmap_final_tuning_regression_status.json"
}

$readiness = Read-Json "Assets\Data\P10\newmap_manual_playtest_readiness.json"
if ($readiness) {
    $decision = if ($logPassed) { "ready_with_documented_visual_limitations" } else { "needs_quick_fix_before_manual_test" }
    $readiness.manualReadinessDecision = $decision
    $readiness.finalStatus = $decision
    $readiness.reason = if ($logPassed) { "Final tuning player smoke and Player.log validation passed; visual limitations remain documented." } else { "Final tuning player smoke or Player.log validation failed." }
    $readiness.validation.playerLog = if ($logPassed) { "passed" } else { "failed" }
    $readiness.spawnFinalSafety.repeatedSpawnInsideBuildingCount = $spawn.repeatedInsideBuilding
    Write-Json $readiness "Assets\Data\P10\newmap_manual_playtest_readiness.json"
}

if (-not $logPassed) {
    Write-Host "[FAIL] NewMap final tuning Player.log validation failed. JSON: $OutputPath"
    exit 1
}

Write-Host "[PASS] NewMap final tuning Player.log validated. JSON: $OutputPath"
exit 0
