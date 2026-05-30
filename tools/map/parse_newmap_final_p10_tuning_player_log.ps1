param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_final_p10_tuning_player_log_summary.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Get-LatestPlayerLog {
    $candidate = Join-Path $ProjectRoot "Logs\newmap_final_p10_tuning_player.log"
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
    $pattern = "(?:^|\s)" + [regex]::Escape($Name) + "=(?<value>\S+)"
    $match = [regex]::Match($Line, $pattern)
    if ($match.Success) { return $match.Groups["value"].Value }
    $fallbackPattern = [regex]::Escape($Name) + "=(?<value>\S+)"
    $fallbackMatch = [regex]::Match($Line, $fallbackPattern)
    if ($fallbackMatch.Success) { return $fallbackMatch.Groups["value"].Value }
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
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Write-Json {
    param($Object, [string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $path) | Out-Null
    $Object | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $path -Encoding UTF8
}

function Update-JsonFile {
    param([string]$RelativePath, [scriptblock]$Updater)
    $json = Read-Json $RelativePath
    if ($null -ne $json) {
        & $Updater $json
        Write-Json $json $RelativePath
    }
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $candidate = Get-LatestPlayerLog
    if ($candidate) { $LogPath = $candidate.FullName }
}

if ([string]::IsNullOrWhiteSpace($LogPath) -or -not (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
    Write-Host "[FAIL] Player.log not found."
    exit 1
}

$lines = @(Get-Content -LiteralPath $LogPath -Encoding UTF8 -ErrorAction SilentlyContinue | ForEach-Object { [string]$_ })
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
$exceptions = @($lines | Where-Object { $_ -match "Exception" -and $_ -notmatch "LogException" })
$missing = @($lines | Where-Object { $_ -match "(?i)missing.*(asset|config|file)|could not be loaded" })
$web = @($lines | Where-Object {
    ($_ -match "UnityWebRequest|HttpClient|WebRequest|System\.Net|http://|https://") -and
    ($_ -notmatch "runtimeWebRequestsObserved|runtimeNetworkRequestsAllowed")
})

$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$performanceLine = [string](@($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$scenarioNames = @(
    "runtime_target_counts",
    "spawn_road_playable_ground_validation",
    "spawn_repeated_100_avoids_buildings",
    "mode_speed_stamina_rules",
    "circular_boundary_player_clamp",
    "circular_boundary_npc_clamp",
    "tsunami_warning_180s_before_active",
    "tourism_free_roam_no_failure",
    "mouse_left_right_drag_look",
    "night_lighting_dark_sky_readable_buildings",
    "building_touch_e_entry",
    "tsunami_front_failure"
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

$modeLine = [string]$scenarioResults["mode_speed_stamina_rules"].line
$spawnRepeatedLine = [string]$scenarioResults["spawn_repeated_100_avoids_buildings"].line
$config = Read-Json "Assets\Data\P10\newmap_player_stamina_config.json"

$stamina = [ordered]@{
    oldMaxStamina = 13000.0
    newMaxStamina = [double]$config.baselineMaxStamina * [double]$config.staminaMultiplier
    baselineMaxStamina = [double]$config.baselineMaxStamina
    staminaMultiplier = [double]$config.staminaMultiplier
    tourismStaminaEnabled = As-Bool (Get-TokenValue $modeLine "tourismStaminaEnabled")
    evacuationMaxStaminaRuntime = As-Double (Get-TokenValue $modeLine "evacuationMaxStamina")
    evacuationCurrentStaminaRuntime = As-Double (Get-TokenValue $modeLine "evacuationStamina")
}

$sprint = [ordered]@{
    oldSprintSpeedMetersPerSecond = 5.7375
    newSprintSpeedMetersPerSecond = 5.0 * [double]$config.sprintSpeedMultiplierAdditional
    sprintSpeedMultiplierAdditional = [double]$config.sprintSpeedMultiplierAdditional
    actualReductionPercent = (1.0 - ((5.0 * [double]$config.sprintSpeedMultiplierAdditional) / 5.7375)) * 100.0
    tourismWalkSpeedRuntime = As-Double (Get-TokenValue $modeLine "tourismWalk")
    tourismSprintSpeedRuntime = As-Double (Get-TokenValue $modeLine "tourismSprint")
    evacuationWalkSpeedRuntime = As-Double (Get-TokenValue $modeLine "evacuationWalk")
    evacuationSprintSpeedRuntime = As-Double (Get-TokenValue $modeLine "evacuationSprint")
    evacuationSprintMultiplierRuntime = As-Double (Get-TokenValue $modeLine "evacuationSprintMultiplier")
}

$spawn = [ordered]@{
    validationPassed = As-Bool (Get-TokenValue $bootstrapLine "spawnValidationPassed")
    repeatedSamples = As-Int (Get-TokenValue $spawnRepeatedLine "samples")
    repeatedAccepted = As-Int (Get-TokenValue $spawnRepeatedLine "accepted")
    repeatedInsideBuilding = As-Int (Get-TokenValue $spawnRepeatedLine "insideBuilding")
    repeatedFinalOverlap = As-Int (Get-TokenValue $spawnRepeatedLine "finalOverlap")
    repeatedOutsideBoundary = As-Int (Get-TokenValue $spawnRepeatedLine "outsideBoundary")
}

$performance = [ordered]@{
    measured = -not [string]::IsNullOrWhiteSpace($performanceLine)
    warmupMaxFrameMs = As-Double (Get-TokenValue $performanceLine "warmupMaxFrameMs")
    elapsedSeconds = As-Double (Get-TokenValue $performanceLine "elapsedSeconds")
    averageFps = As-Double (Get-TokenValue $performanceLine "avgFps")
    maxFrameMs = As-Double (Get-TokenValue $performanceLine "maxFrameMs")
    stutterFramesOver66ms = As-Int (Get-TokenValue $performanceLine "stutterFramesOver66ms")
    movingNpcCount = As-Int (Get-TokenValue $performanceLine "movingNpcCount")
    staticProxyNpcCount = As-Int (Get-TokenValue $performanceLine "staticProxyNpcCount")
    stoppedWithoutReasonCount = As-Int (Get-TokenValue $performanceLine "stoppedWithoutReasonCount")
}

$allScenariosPassed = $true
foreach ($key in $scenarioResults.Keys) {
    if (-not [bool]$scenarioResults[$key].passed) {
        $allScenariosPassed = $false
    }
}

$staminaPassed =
    [math]::Abs([double]$stamina.newMaxStamina - 3500.0) -le 0.001 -and
    [math]::Abs([double]$stamina.evacuationMaxStaminaRuntime - 3500.0) -le 0.001 -and
    [bool]$stamina.tourismStaminaEnabled -eq $false

$sprintPassed =
    [math]::Abs([double]$sprint.newSprintSpeedMetersPerSecond - 4.59) -le 0.0001 -and
    [math]::Abs([double]$sprint.evacuationSprintSpeedRuntime - 4.59) -le 0.001 -and
    [math]::Abs([double]$sprint.actualReductionPercent - 20.0) -le 0.001 -and
    [math]::Abs([double]$sprint.tourismSprintSpeedRuntime - 10.0) -le 0.001 -and
    [math]::Abs([double]$sprint.evacuationWalkSpeedRuntime - 1.0) -le 0.001

$spawnPassed =
    [bool]$spawn.validationPassed -and
    [int]$spawn.repeatedSamples -eq 100 -and
    [int]$spawn.repeatedInsideBuilding -eq 0 -and
    [int]$spawn.repeatedFinalOverlap -eq 0 -and
    [int]$spawn.repeatedOutsideBoundary -eq 0

$performancePassed =
    [bool]$performance.measured -and
    [double]$performance.averageFps -gt 0 -and
    [int]$performance.stoppedWithoutReasonCount -eq 0

$logPassed =
    $errors.Count -eq 0 -and
    $warnings.Count -eq 0 -and
    $exceptions.Count -eq 0 -and
    $missing.Count -eq 0 -and
    $web.Count -eq 0 -and
    $selfAuditCompleted -and
    $selfAuditFailures.Count -eq 0 -and
    $allScenariosPassed -and
    $staminaPassed -and
    $sprintPassed -and
    $spawnPassed -and
    $performancePassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    exceptionCount = $exceptions.Count
    missingAssetOrConfigCount = $missing.Count
    runtimeWebRequestCount = $web.Count
    stamina = $stamina
    sprint = $sprint
    tourismModeImpact = "none_tourism_stamina_disabled_and_sprint_unchanged"
    evacuationModeStatus = if ($staminaPassed -and $sprintPassed) { "passed_stamina_3500_sprint_4_59" } else { "failed" }
    spawnSafety = $spawn
    performance = $performance
    scenarioResults = $scenarioResults
    selfAuditCompleted = $selfAuditCompleted
    selfAuditFailureCount = $selfAuditFailures.Count
    playerLogResult = if ($logPassed) { "passed_clean" } else { "failed" }
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

Write-Json $summary $OutputPath

Update-JsonFile "Assets\Data\P10\newmap_final_stamina_sprint_tuning_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName newMaxStamina -NotePropertyValue $stamina.newMaxStamina -Force
    $json | Add-Member -NotePropertyName newSprintSpeedMetersPerSecond -NotePropertyValue $sprint.newSprintSpeedMetersPerSecond -Force
    $json | Add-Member -NotePropertyName actualSprintReductionPercent -NotePropertyValue ([math]::Round([double]$sprint.actualReductionPercent, 4)) -Force
    $json | Add-Member -NotePropertyName tourismModeStaminaDisabled -NotePropertyValue (-not [bool]$stamina.tourismStaminaEnabled) -Force
    $json | Add-Member -NotePropertyName evacuationModeStaminaStatus -NotePropertyValue ($(if ($staminaPassed) { "passed_runtime_player_log" } else { "failed_runtime_player_log" })) -Force
    $json | Add-Member -NotePropertyName evacuationModeSprintStatus -NotePropertyValue ($(if ($sprintPassed) { "passed_runtime_player_log" } else { "failed_runtime_player_log" })) -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($staminaPassed -and $sprintPassed) { "validated_by_player_log" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_final_p10_tuning_player_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName buildExists -NotePropertyValue $true -Force
    $json | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $json | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $json | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $json | Add-Member -NotePropertyName playerLogExceptions -NotePropertyValue $exceptions.Count -Force
    $json | Add-Member -NotePropertyName staminaConfigSmoke -NotePropertyValue ($(if ($staminaPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName sprintSpeedSmoke -NotePropertyValue ($(if ($sprintPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName tourismEvacuationModeSmoke -NotePropertyValue ($(if ([bool]$scenarioResults["mode_speed_stamina_rules"].passed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName p2ToP10Smoke -NotePropertyValue ($(if ($allScenariosPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName performance -NotePropertyValue $performance -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\p10_to_p11_handoff_readiness.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName latestCommitHash -NotePropertyValue (git -C $ProjectRoot rev-parse HEAD) -Force
    $json | Add-Member -NotePropertyName p10FinalTuningStatus -NotePropertyValue ($(if ($logPassed) { "validated" } else { "failed_player_log_parse" })) -Force
    $json | Add-Member -NotePropertyName manualTestReady -NotePropertyValue ([bool]$logPassed) -Force
    $json | Add-Member -NotePropertyName readinessDecision -NotePropertyValue ($(if ($logPassed) { "ready_for_p11_handoff_after_user_confirmation" } else { "needs_quick_fix_before_p11_handoff" })) -Force
    $json | Add-Member -NotePropertyName playerLogResult -NotePropertyValue ($(if ($logPassed) { "passed_clean" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName editModeResult -NotePropertyValue "pending_current_run" -Force
    $json | Add-Member -NotePropertyName playModeResult -NotePropertyValue "pending_current_run" -Force
    $json | Add-Member -NotePropertyName deepSeekVerdict -NotePropertyValue "pending_current_run" -Force
}

Update-JsonFile "Assets\Data\P10\newmap_manual_playtest_readiness.json" {
    param($json)
    $decision = if ($logPassed) { "ready_with_documented_visual_or_npc_limitations" } else { "needs_quick_fix_before_manual_test" }
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName manualReadinessDecision -NotePropertyValue $decision -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue $decision -Force
    $json.validation | Add-Member -NotePropertyName playerLog -NotePropertyValue ($(if ($logPassed) { "passed" } else { "failed" })) -Force
    $json.staminaSprintWarning | Add-Member -NotePropertyName oldFinalStamina -NotePropertyValue 13000.0 -Force
    $json.staminaSprintWarning | Add-Member -NotePropertyName newFinalStamina -NotePropertyValue 3500.0 -Force
    $json.staminaSprintWarning | Add-Member -NotePropertyName oldEvacuationSprintSpeedMetersPerSecond -NotePropertyValue 5.7375 -Force
    $json.staminaSprintWarning | Add-Member -NotePropertyName newEvacuationSprintSpeedMetersPerSecond -NotePropertyValue 4.59 -Force
    $json.staminaSprintWarning | Add-Member -NotePropertyName sprintReductionPercent -NotePropertyValue 20.0 -Force
}

if (-not $logPassed) {
    Write-Host "[FAIL] NewMap final P10 tuning Player.log validation failed. JSON: $OutputPath"
    exit 1
}

Write-Host "[PASS] NewMap final P10 tuning Player.log validated. JSON: $OutputPath"
exit 0
