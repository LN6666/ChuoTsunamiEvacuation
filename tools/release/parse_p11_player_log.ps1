param(
    [string]$LogPath = "",
    [string]$OutputJson = "Assets\Data\P10\p11_local_final_smoke_report.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Get-TokenValue {
    param([string]$Line, [string]$Name)
    if ([string]::IsNullOrWhiteSpace($Line)) { return $null }
    $match = [regex]::Match($Line, [regex]::Escape($Name) + "=(?<value>\S+)")
    if ($match.Success) { return $match.Groups["value"].Value }
    return $null
}

function As-Int { param($Value) if ($null -eq $Value) { return $null } return [int]$Value }
function As-Double { param($Value) if ($null -eq $Value) { return $null } return [double]$Value }
function As-Bool { param($Value) if ($null -eq $Value) { return $null } return ([string]$Value) -eq "True" -or ([string]$Value) -eq "true" }

function Write-Json {
    param($Object, [string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $path) | Out-Null
    $Object | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $path -Encoding UTF8
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $LogPath = Join-Path $ProjectRoot "Logs\p11_final_player.log"
}
if (-not (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
    Write-Host "[FAIL] Player.log not found: $LogPath"
    exit 1
}

$lines = @(Get-Content -LiteralPath $LogPath -Encoding UTF8 -ErrorAction SilentlyContinue | ForEach-Object { [string]$_ })
$errors = @($lines | Where-Object { $_ -match "NullReferenceException|MissingReferenceException|Unhandled Exception|UnityEngine\.Debug:LogError|LogType\.Error|^\s*Error:|Scripts have compiler errors" })
$warnings = @($lines | Where-Object {
    ($_ -match "UnityEngine\.Debug:LogWarning|LogType\.Warning|^\s*Warning:|^\s*WARNING:") -and
    ($_ -notmatch "Stage 1 warning|evacuation_stage1_warning|tourism_non_official_inspection_warning|success_non_official_candidate_with_warning")
})
$exceptions = @($lines | Where-Object { $_ -match "Exception" -and $_ -notmatch "LogException" })
$missing = @($lines | Where-Object { $_ -match "(?i)missing.*(asset|config|file)|could not be loaded" })
$web = @($lines | Where-Object {
    ($_ -match "UnityWebRequest|HttpClient|WebRequest|System\.Net|http://|https://") -and
    ($_ -notmatch "runtimeWebRequestsObserved|runtimeNetworkRequestsAllowed")
})

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
$performanceLine = [string](@($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$allScenariosPassed = $true
foreach ($key in $scenarioResults.Keys) {
    if (-not [bool]$scenarioResults[$key].passed) { $allScenariosPassed = $false }
}

$stamina = [ordered]@{
    evacuationMaxStaminaRuntime = As-Double (Get-TokenValue $modeLine "evacuationMaxStamina")
    evacuationCurrentStaminaRuntime = As-Double (Get-TokenValue $modeLine "evacuationStamina")
    tourismStaminaEnabled = As-Bool (Get-TokenValue $modeLine "tourismStaminaEnabled")
}
$sprint = [ordered]@{
    tourismSprintSpeedRuntime = As-Double (Get-TokenValue $modeLine "tourismSprint")
    evacuationSprintSpeedRuntime = As-Double (Get-TokenValue $modeLine "evacuationSprint")
    evacuationSprintMultiplierRuntime = As-Double (Get-TokenValue $modeLine "evacuationSprintMultiplier")
}
$performance = [ordered]@{
    measured = -not [string]::IsNullOrWhiteSpace($performanceLine)
    warmupMaxFrameMs = As-Double (Get-TokenValue $performanceLine "warmupMaxFrameMs")
    elapsedSeconds = As-Double (Get-TokenValue $performanceLine "elapsedSeconds")
    averageFps = As-Double (Get-TokenValue $performanceLine "avgFps")
    maxFrameMs = As-Double (Get-TokenValue $performanceLine "maxFrameMs")
    stutterFramesOver66ms = As-Int (Get-TokenValue $performanceLine "stutterFramesOver66ms")
    activeNpcCount = As-Int (Get-TokenValue $performanceLine "activeNpcCount")
    stoppedWithoutReasonCount = As-Int (Get-TokenValue $performanceLine "stoppedWithoutReasonCount")
}

$staminaPassed = [math]::Abs([double]$stamina.evacuationMaxStaminaRuntime - 3500.0) -le 0.001 -and (-not [bool]$stamina.tourismStaminaEnabled)
$sprintPassed = [math]::Abs([double]$sprint.evacuationSprintSpeedRuntime - 4.59) -le 0.001 -and [math]::Abs([double]$sprint.tourismSprintSpeedRuntime - 10.0) -le 0.001
$performancePassed = [bool]$performance.measured -and [double]$performance.averageFps -gt 0 -and [int]$performance.stoppedWithoutReasonCount -eq 0
$passed = $errors.Count -eq 0 -and $warnings.Count -eq 0 -and $exceptions.Count -eq 0 -and $missing.Count -eq 0 -and $web.Count -eq 0 -and $selfAuditCompleted -and $selfAuditFailures.Count -eq 0 -and $allScenariosPassed -and $staminaPassed -and $sprintPassed -and $performancePassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    playerLogResult = if ($passed) { "passed_clean" } else { "failed" }
    errorCount = $errors.Count
    warningCount = $warnings.Count
    exceptionCount = $exceptions.Count
    missingAssetOrConfigCount = $missing.Count
    runtimeWebRequestCount = $web.Count
    stamina = $stamina
    sprint = $sprint
    scenarios = $scenarioResults
    selfAuditCompleted = $selfAuditCompleted
    selfAuditFailureCount = $selfAuditFailures.Count
    performance = $performance
    localSmokeResult = if ($passed) { "passed" } else { "failed" }
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
}
Write-Json $summary $OutputJson

$perf = [ordered]@{
    generatedAt = $summary.generatedAt
    sourceLog = $LogPath
    durationSeconds = $performance.elapsedSeconds
    averageFps = $performance.averageFps
    maxFrameMs = $performance.maxFrameMs
    stutterFramesOver66ms = $performance.stutterFramesOver66ms
    activeNpcCount = $performance.activeNpcCount
    stoppedWithoutReasonCount = $performance.stoppedWithoutReasonCount
    playerLogErrors = $errors.Count
    playerLogWarnings = $warnings.Count
    finalStatus = if ($performancePassed -and $errors.Count -eq 0 -and $warnings.Count -eq 0) { "passed_with_documented_startup_stutter_limitations" } else { "failed" }
}
Write-Json $perf "Assets\Data\P10\p11_final_performance_summary.json"

if (-not $passed) {
    Write-Host "[FAIL] P11 Player.log validation failed. JSON: $OutputJson"
    exit 1
}

Write-Host "[PASS] P11 Player.log validated. JSON: $OutputJson"
exit 0
