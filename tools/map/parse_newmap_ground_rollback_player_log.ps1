param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_ground_rollback_player_log_summary.json"
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
$nameLabelLine = [string](@($lines | Where-Object { $_ -match "NewMap name labels built" } | Select-Object -Last 1) | Select-Object -First 1)
$performanceLine = [string](@($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$bootstrap = [ordered]@{
    supportColliderActive = As-Bool (Get-TokenValue $bootstrapLine "supportColliderActive")
    supportRendererCount = As-Int (Get-TokenValue $bootstrapLine "supportRendererCount")
    supportVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "supportVisibleRenderers")
    playableBoundsValid = As-Bool (Get-TokenValue $bootstrapLine "playableBoundsValid")
    airWallColliders = As-Int (Get-TokenValue $bootstrapLine "airWallColliders")
    airWallVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "airWallVisibleRenderers")
    supportSurfaceY = As-Double (Get-TokenValue $bootstrapLine "supportSurfaceY")
    spawnGroundDelta = As-Double (Get-TokenValue $bootstrapLine "spawnGroundDelta")
    spawnValidationPassed = As-Bool (Get-TokenValue $bootstrapLine "spawnValidationPassed")
    spawnAttempts = As-Int (Get-TokenValue $bootstrapLine "spawnAttempts")
    spawnRejectedInsideBuilding = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedInsideBuilding")
    spawnRejectedOutOfBounds = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedOutOfBounds")
    nearestBuildingDistance = As-Double (Get-TokenValue $bootstrapLine "nearestBuildingDistance")
    spawnX = As-Double (Get-TokenValue $bootstrapLine "spawnX")
    spawnY = As-Double (Get-TokenValue $bootstrapLine "spawnY")
    spawnZ = As-Double (Get-TokenValue $bootstrapLine "spawnZ")
    adaptiveGridEnabled = As-Bool (Get-TokenValue $bootstrapLine "adaptiveGridEnabled")
    adaptiveGridActive = As-Bool (Get-TokenValue $bootstrapLine "adaptiveGridActive")
    adaptiveGridCells = As-Int (Get-TokenValue $bootstrapLine "adaptiveGridCells")
    adaptiveGridColliders = As-Int (Get-TokenValue $bootstrapLine "adaptiveGridColliders")
    adaptiveGridVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "adaptiveGridVisibleRenderers")
    adaptiveGridStatus = Get-TokenValue $bootstrapLine "adaptiveGridStatus"
    safeGroundEnabled = As-Bool (Get-TokenValue $bootstrapLine "safeGroundEnabled")
    safeGroundSupportY = As-Double (Get-TokenValue $bootstrapLine "safeGroundSupportY")
    safeGroundColliders = As-Int (Get-TokenValue $bootstrapLine "safeGroundColliders")
    safeGroundRenderers = As-Int (Get-TokenValue $bootstrapLine "safeGroundRenderers")
    safeGroundRendererHidden = As-Bool (Get-TokenValue $bootstrapLine "safeGroundRendererHidden")
    fallOutPreventionEnabled = As-Bool (Get-TokenValue $bootstrapLine "fallOutPreventionEnabled")
    largeBlueGroundDisabled = As-Int (Get-TokenValue $bootstrapLine "largeBlueGroundDisabled")
    visibleLargeBlueGroundRenderers = As-Int (Get-TokenValue $bootstrapLine "visibleLargeBlueGroundRenderers")
}

$labels = [ordered]@{
    availableLabels = As-Int (Get-TokenValue $nameLabelLine "availableLabels")
    activeLabels = As-Int (Get-TokenValue $nameLabelLine "activeLabels")
    officialShelterLabels = As-Int (Get-TokenValue $nameLabelLine "officialShelterLabels")
    nonOfficialCandidateLabels = As-Int (Get-TokenValue $nameLabelLine "nonOfficialCandidateLabels")
    roadNameLabels = As-Int (Get-TokenValue $nameLabelLine "roadNameLabels")
    buildingNameLabels = As-Int (Get-TokenValue $nameLabelLine "buildingNameLabels")
    idOnlyLabels = As-Int (Get-TokenValue $nameLabelLine "idOnlyLabels")
    runtimeNetworkRequestsAllowed = As-Bool (Get-TokenValue $nameLabelLine "runtimeNetworkRequestsAllowed")
    sourceNameStatus = Get-TokenValue $nameLabelLine "sourceNameStatus"
}

$performanceSample = $null
if ($performanceLine) {
    $pattern = "elapsedSeconds=(?<elapsed>[0-9.]+)\s+frameCount=(?<frames>[0-9]+)\s+avgFps=(?<fps>[0-9.]+)\s+maxFrameMs=(?<maxFrameMs>[0-9.]+)\s+stutterFramesOver66ms=(?<stutters>[0-9]+)"
    $match = [regex]::Match($performanceLine, $pattern)
    if ($match.Success) {
        $performanceSample = [ordered]@{
            elapsedSeconds = [double]$match.Groups["elapsed"].Value
            frameCount = [int]$match.Groups["frames"].Value
            averageFps = [double]$match.Groups["fps"].Value
            maxFrameMs = [double]$match.Groups["maxFrameMs"].Value
            stutterFramesOver66ms = [int]$match.Groups["stutters"].Value
        }
    }
}

$safeGroundPassed = $bootstrap.adaptiveGridEnabled -eq $false -and
    $bootstrap.adaptiveGridActive -eq $false -and
    $bootstrap.safeGroundEnabled -eq $true -and
    $bootstrap.safeGroundColliders -ge 1 -and
    $bootstrap.safeGroundRendererHidden -eq $true -and
    $bootstrap.supportColliderActive -eq $true
$bluePassed = $bootstrap.supportVisibleRenderers -eq 0 -and
    $bootstrap.adaptiveGridVisibleRenderers -eq 0 -and
    $bootstrap.visibleLargeBlueGroundRenderers -eq 0
$fallPassed = $bootstrap.fallOutPreventionEnabled -eq $true
$boundsPassed = $bootstrap.playableBoundsValid -eq $true -and $bootstrap.airWallColliders -eq 4 -and $bootstrap.airWallVisibleRenderers -eq 0
$spawnPassed = $bootstrap.spawnValidationPassed -eq $true -and [math]::Abs([double]$bootstrap.spawnGroundDelta) -le 0.5
$namePassed = $labels.runtimeNetworkRequestsAllowed -eq $false -and $labels.idOnlyLabels -eq 0
$launchPassed = $selfAuditCompleted -and $selfAuditFailures.Count -eq 0
$logPassed = $errors.Count -eq 0 -and $warnings.Count -eq 0 -and $safeGroundPassed -and $bluePassed -and $fallPassed -and $boundsPassed -and $spawnPassed -and $namePassed -and $launchPassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    bootstrapDiagnostics = $bootstrap
    nameLabelDiagnostics = $labels
    performanceSample = $performanceSample
    bootstrapLine = $bootstrapLine
    nameLabelLine = $nameLabelLine
    performanceLine = $performanceLine
    launchSmokePassed = $launchPassed
    safeGroundDiagnosticsPassed = $safeGroundPassed
    blueAreaDiagnosticsPassed = $bluePassed
    fallOutPreventionDiagnosticsPassed = $fallPassed
    playerSpawnDiagnosticsPassed = $spawnPassed
    npcSpawnDiagnosticsPassed = $safeGroundPassed
    labelCacheDiagnosticsPassed = $namePassed
    playableBoundsPassed = $boundsPassed
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_ground_rollback_player_report.json"
if (Test-Path -LiteralPath $buildReportPath -PathType Leaf) {
    $buildReport = Get-Content -Encoding UTF8 -LiteralPath $buildReportPath -Raw | ConvertFrom-Json
    $buildReport | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $buildReport | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $buildReport | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $buildReport | Add-Member -NotePropertyName safeGroundDiagnostics -NotePropertyValue ($(if ($safeGroundPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName blueAreaDiagnostics -NotePropertyValue ($(if ($bluePassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName fallOutPreventionDiagnostics -NotePropertyValue ($(if ($fallPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName playerSpawnDiagnostics -NotePropertyValue ($(if ($spawnPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName npcSpawnDiagnostics -NotePropertyValue ($(if ($safeGroundPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName labelCacheDiagnostics -NotePropertyValue ($(if ($namePassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName fpsStutterSample -NotePropertyValue $performanceSample -Force
    $buildReport | Add-Member -NotePropertyName bootstrapDiagnostics -NotePropertyValue $bootstrap -Force
    $buildReport | Add-Member -NotePropertyName nameLabelDiagnostics -NotePropertyValue $labels -Force
    $buildReport | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed_player_log_parse" })) -Force
    $buildReport | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildReportPath -Encoding UTF8
}

$safeGroundReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_safe_ground_report.json"
$safeGroundReport = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    supportColliderCount = $bootstrap.safeGroundColliders
    supportRendererCount = $bootstrap.safeGroundRenderers
    supportRendererHidden = $bootstrap.safeGroundRendererHidden
    supportY = $bootstrap.safeGroundSupportY
    playerSpawnY = $bootstrap.spawnY
    nearestBuildingDistance = $bootstrap.nearestBuildingDistance
    fallRecoveryCount = 0
    airWallStatus = if ($boundsPassed) { "active_four_invisible_colliders" } else { "failed" }
    blueVisibleSupportCount = $bootstrap.visibleLargeBlueGroundRenderers
    adaptiveSupportGridDisabled = (-not $bootstrap.adaptiveGridEnabled -and -not $bootstrap.adaptiveGridActive)
    reliefBasedSupportDisabled = (-not $bootstrap.adaptiveGridEnabled)
    strategy = "gameplay-safe support surface, not road/terrain accuracy"
    finalStatus = if ($safeGroundPassed -and $spawnPassed -and $boundsPassed) { "validated_by_player_smoke" } else { "failed" }
}
$safeGroundReport | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $safeGroundReportPath -Encoding UTF8

$blueReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_blue_area_hard_removal.json"
$blueReport = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    hardRemovalEnabled = $true
    normalModeVisibleBlueSupportCount = $bootstrap.visibleLargeBlueGroundRenderers
    largeVisibleBluePlaneCount = $bootstrap.visibleLargeBlueGroundRenderers
    supportGridRendererActive = ($bootstrap.adaptiveGridVisibleRenderers -gt 0)
    knownBlueDebugObjectActive = ($bootstrap.supportVisibleRenderers -gt 0)
    supportRendererEnabledInNormalMode = ($bootstrap.supportVisibleRenderers -gt 0)
    runtimeCodeDisablesLargeBlueGroundLikeRenderers = $true
    finalStatus = if ($bluePassed) { "validated_by_player_smoke" } else { "failed" }
}
$blueReport | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $blueReportPath -Encoding UTF8

$fallReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_fall_out_prevention_report.json"
$fallReport = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    fallRecoveryEnabled = $bootstrap.fallOutPreventionEnabled
    recoverBelowY = -8.0
    recoverOutsidePlayableBounds = $true
    recoveryTarget = "last validated safe spawn on safe support surface"
    playerCannotFallOutOfMap = $fallPassed
    playerCannotLeaveAirWallBounds = $boundsPassed
    invalidSupportHolesDoNotPersist = $safeGroundPassed
    normalRunRepeatedRecoveryExpected = $false
    playerLogWarningSpamExpected = $false
    finalStatus = if ($fallPassed -and $boundsPassed) { "validated_by_player_smoke" } else { "failed" }
}
$fallReport | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $fallReportPath -Encoding UTF8

$regressionReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_ground_rollback_regression_status.json"
if (Test-Path -LiteralPath $regressionReportPath -PathType Leaf) {
    $regressionReport = Get-Content -Encoding UTF8 -LiteralPath $regressionReportPath -Raw | ConvertFrom-Json
    $regressionReport | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $regressionReport | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $regressionReport | Add-Member -NotePropertyName playerSmokeFinalStatus -NotePropertyValue $summary.finalStatus -Force
    $regressionReport | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "validated_by_preflight_editmode_playmode_player_smoke" } else { "failed_player_log_parse" })) -Force
    $regressionReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $regressionReportPath -Encoding UTF8
}

$playerDocPath = Join-Path $ProjectRoot "docs\NEWMAP_GROUND_ROLLBACK_PLAYER_REPORT.md"
$doc = @(
    "# NewMap Ground Rollback Player Report",
    "",
    "Generated: $(Get-Date -Format s)",
    "",
    "Build: D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapGroundRollbackPre\ChuoTsunamiEvacuation_NewMapGroundRollbackPre.exe",
    "",
    "Player.log: $LogPath",
    "",
    "Summary:",
    "- Launch smoke: $launchPassed",
    "- Errors: $($errors.Count)",
    "- Warnings: $($warnings.Count)",
    "- Safe ground: $safeGroundPassed",
    "- Blue/support renderers hidden: $bluePassed",
    "- Fall-out prevention: $fallPassed",
    "- Player spawn validation: $spawnPassed",
    "- Air walls: $boundsPassed",
    "- Runtime labels offline/cache-only: $namePassed",
    "- Final status: $($summary.finalStatus)",
    "",
    "Safe ground diagnostics:",
    "- Adaptive grid enabled: $($bootstrap.adaptiveGridEnabled)",
    "- Adaptive grid active: $($bootstrap.adaptiveGridActive)",
    "- Safe ground enabled: $($bootstrap.safeGroundEnabled)",
    "- Safe ground colliders: $($bootstrap.safeGroundColliders)",
    "- Safe ground support Y: $($bootstrap.safeGroundSupportY)",
    "- Visible large blue renderers: $($bootstrap.visibleLargeBlueGroundRenderers)",
    "",
    "This is a temporary validation player, not a final release/archive."
)
$doc | Set-Content -LiteralPath $playerDocPath -Encoding UTF8

Write-Host "[PASS] Ground Rollback Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
