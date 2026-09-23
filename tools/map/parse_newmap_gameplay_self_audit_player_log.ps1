param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_gameplay_self_audit_player_report.json"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $localLow = Join-Path $env:USERPROFILE "AppData\LocalLow"
    $candidate = Get-ChildItem -LiteralPath $localLow -Recurse -Filter Player.log -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($candidate) {
        $LogPath = $candidate.FullName
    }
}

if ([string]::IsNullOrWhiteSpace($LogPath) -or -not (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
    Write-Host "[FAIL] Player.log not found."
    exit 1
}

$lines = @(Get-Content -LiteralPath $LogPath -ErrorAction SilentlyContinue | ForEach-Object { [string]$_ })
$errors = @($lines | Where-Object {
    $_ -match "NullReferenceException|MissingReferenceException|Unhandled Exception|UnityEngine\.Debug:LogError|LogType\.Error|^\s*Error:|Scripts have compiler errors"
})
$warnings = @($lines | Where-Object {
    $_ -match "UnityEngine\.Debug:LogWarning|LogType\.Warning|^\s*Warning:|^\s*WARNING:"
})

$performanceLine = [string](@($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1) | Select-Object -First 1)
$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$bootstrapTimingLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap timings:" } | Select-Object -Last 1) | Select-Object -First 1)

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

$smokeScenarios = @()
foreach ($line in $lines) {
    $match = [regex]::Match($line, "NewMap gameplay self-audit smoke:\s+scenario=(?<id>\S+)\s+result=(?<result>\S+)\s+detail=(?<detail>.*)$")
    if ($match.Success) {
        $smokeScenarios += [pscustomobject][ordered]@{
            scenarioId = $match.Groups["id"].Value
            result = $match.Groups["result"].Value
            detail = $match.Groups["detail"].Value
        }
    }
}

$requiredSmoke = @(
    "runtime_target_counts",
    "start_menu_visible",
    "tourism_free_roam_no_failure",
    "tourism_non_official_inspection_warning",
    "evacuation_stage1_warning",
    "evacuation_stage2_front_and_green_frames",
    "success_official_shelter",
    "success_non_official_candidate_with_warning",
    "route_proxy_wording",
    "crowd_delay_success_or_failure",
    "entrance_blocked_failure",
    "safe_floor_unavailable_failure",
    "collapse_debris_exposure_failure",
    "tsunami_front_failure",
    "collapse_disabled_success",
    "disabled_target_not_selectable",
    "ui_rules_pause_result_panel"
)

$missingSmoke = @()
$failedSmoke = @()
foreach ($id in $requiredSmoke) {
    $matches = @($smokeScenarios | Where-Object { $_.scenarioId -eq $id })
    if ($matches.Count -eq 0) {
        $missingSmoke += $id
        continue
    }
    if (-not (@($matches | Where-Object { $_.result -eq "pass" }).Count -gt 0)) {
        $failedSmoke += $id
    }
}

$previousMaxFrameMs = 6298.42
$performanceDecision = "blocked_by_map_runtime_bootstrap"
if ($performanceSample) {
    if ($performanceSample.maxFrameMs -lt 2000) {
        $performanceDecision = "spike_reduced_below_target"
    }
    elseif ($performanceSample.maxFrameMs -lt $previousMaxFrameMs) {
        $performanceDecision = "spike_reduced_but_above_target"
    }
    else {
        $performanceDecision = "spike_remaining_but_documented"
    }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
$previous = $null
if (Test-Path -LiteralPath $fullOutput -PathType Leaf) {
    $previous = Get-Content -LiteralPath $fullOutput -Raw | ConvertFrom-Json
}

$buildPath = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapGameplaySelfAuditPre\ChuoTsunamiEvacuation_NewMapGameplaySelfAuditPre.exe"
if ($previous -and $previous.buildPath) {
    $buildPath = [string]$previous.buildPath
}

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    activeScene = "Assets/Scenes/Chuo_BaseMap.unity"
    buildPath = $buildPath
    logPath = $LogPath
    launchSmoke = if ($previous -and $previous.launchSmoke) { [string]$previous.launchSmoke } else { "not_recorded" }
    playerBuildEvidence = ($errors.Count -eq 0 -and $warnings.Count -eq 0 -and $missingSmoke.Count -eq 0 -and $failedSmoke.Count -eq 0)
    commandLineSmokeArg = "-newmapSelfAuditSmoke"
    errorCount = $errors.Count
    warningCount = $warnings.Count
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    smokeScenarios = $smokeScenarios
    requiredSmokeScenarioCount = $requiredSmoke.Count
    missingSmokeScenarios = $missingSmoke
    failedSmokeScenarios = $failedSmoke
    performanceSample = $performanceSample
    bootstrapLine = $bootstrapLine
    bootstrapTimingLine = $bootstrapTimingLine
    previousMaxFrameMs = $previousMaxFrameMs
    performanceDecision = $performanceDecision
    memoryMeasured = if ($previous) { [bool]$previous.memoryMeasured } else { $false }
    memoryFocus = "informational_only"
    maxPrivateMemoryBytes = if ($previous) { $previous.maxPrivateMemoryBytes } else { $null }
    maxWorkingSetBytes = if ($previous) { $previous.maxWorkingSetBytes } else { $null }
    finalStatus = if ($errors.Count -eq 0 -and $warnings.Count -eq 0 -and $missingSmoke.Count -eq 0 -and $failedSmoke.Count -eq 0) { "completed_on_new_chuo_basemap" } elseif ($errors.Count -eq 0) { "completed_with_documented_runtime_proxy" } else { "failed" }
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_gameplay_self_audit_build_report.json"
if (Test-Path -LiteralPath $buildReportPath -PathType Leaf) {
    $buildReport = Get-Content -LiteralPath $buildReportPath -Raw | ConvertFrom-Json
    $buildReport | Add-Member -NotePropertyName launchSmoke -NotePropertyValue $summary.launchSmoke -Force
    $buildReport | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $buildReport | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $buildReport | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $buildReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $buildReportPath -Encoding UTF8
}

python (Join-Path $ProjectRoot "tools\map\write_newmap_gameplay_self_audit_reports.py")

if ($errors.Count -ne 0 -or $warnings.Count -ne 0 -or $missingSmoke.Count -ne 0 -or $failedSmoke.Count -ne 0) {
    Write-Host "[FAIL] Gameplay self-audit Player.log parse found errors, warnings, or failed smoke scenarios."
    Write-Host "Missing smoke: $($missingSmoke -join ', ')"
    Write-Host "Failed smoke: $($failedSmoke -join ', ')"
    exit 1
}

Write-Host "[PASS] Gameplay self-audit Player.log summary written to $fullOutput"
exit 0
