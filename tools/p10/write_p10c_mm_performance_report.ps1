[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$dataDir = Join-Path $repoRoot "Assets\Data\P10"
$docsDir = Join-Path $repoRoot "docs"

function Read-Json {
    param([string]$FileName)
    $path = Join-Path $dataDir $FileName
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return $null
    }
    try {
        return Get-Content -Raw -Encoding UTF8 -LiteralPath $path | ConvertFrom-Json
    }
    catch {
        return $null
    }
}

function Value-Or {
    param([object]$Value, [string]$Fallback = "n/a")
    if ($null -eq $Value) { return $Fallback }
    if ($Value -is [string] -and [string]::IsNullOrWhiteSpace($Value)) { return $Fallback }
    return "$Value"
}

function Write-Doc {
    param([string]$FileName, [string]$Content)
    $path = Join-Path $docsDir $FileName
    Set-Content -LiteralPath $path -Value $Content.TrimStart() -Encoding UTF8
    Write-Host "Wrote docs/$FileName"
}

function Process-Row {
    param([object]$Summary)
    if (-not $Summary) { return "| missing | n/a | n/a | n/a | n/a | n/a | n/a |" }
    return "| $($Summary.requestedDurationSeconds)s | $($Summary.status) | $($Summary.sampleCount) | $($Summary.averageWorkingSetMb) MB | $($Summary.maxPrivateMemoryMb) MB | $($Summary.averageCpuPercentProxy)% | $($Summary.memoryTrend) |"
}

$p3 = Read-Json "p10c_mm_process_sampling_3min.json"
$p5 = Read-Json "p10c_mm_process_sampling_5min.json"
$p10 = Read-Json "p10c_mm_process_sampling_10min.json"
$fps = Read-Json "p10c_mm_fps_stutter_summary.json"
$high = Read-Json "p10c_mm_high_detail_full_load_status.json"
$scenarios = Read-Json "p10c_mm_scenario_performance_summary.json"
$log = Read-Json "p10c_mm_player_log_summary.json"
$paging = Read-Json "p10c_mm_paging_risk_summary.json"
$decision = Read-Json "p10c_mm_readiness_decision.json"
$date = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

$fpsStatus = Value-Or $fps.status
$fpsScenario = Value-Or $fps.scenarioLabel
$fpsSamples = Value-Or $fps.sampleCount
$fpsAverage = Value-Or $fps.averageFps
$fpsMin = Value-Or $fps.minFps
$fpsOnePercentLow = Value-Or $fps.onePercentLowFps
$fpsAverageFrameMs = Value-Or $fps.averageFrameTimeMs
$fpsMaxFrameMs = Value-Or $fps.maxFrameTimeMs
$fpsOver33 = Value-Or $fps.frameSpikeCountOver33Ms
$fpsOver50 = Value-Or $fps.frameSpikeCountOver50Ms
$fpsOver100 = Value-Or $fps.frameSpikeCountOver100Ms
$fpsStutters = Value-Or $fps.stutterEventCount
$fpsRuntimeQuality = Value-Or $fps.runtimeState.activeQualityProfile
$fpsRuntimeWeather = Value-Or $fps.runtimeState.activeWeatherNightMode
$fpsRuntimeGreenFrame = Value-Or $fps.runtimeState.greenFrameActive
$fpsRuntimeLightCurtain = Value-Or $fps.runtimeState.lightCurtainActive
$fpsRuntimeCrowd = Value-Or $fps.runtimeState.crowdActive
$fpsRuntimeResultPanel = Value-Or $fps.runtimeState.resultPanelActive
$fpsLimitations = if ($fps -and $fps.limitations) { ($fps.limitations -join "; ") } else { "none recorded" }

$highScenePath = Value-Or $high.scenePath
$highStatus = Value-Or $high.status
$highSceneExists = Value-Or $high.sceneExists
$highLoadAttempted = Value-Or $high.automatedPlayerLoadAttempted
$highLoadSucceeded = Value-Or $high.automatedPlayerLoadSucceeded
$highLoadingSeconds = Value-Or $high.loadingTimeSeconds
$highPrivateMb = Value-Or $high.memoryAfterLoadPrivateMb
$highWarnings = Value-Or $high.playerLogWarningCount
$highErrors = Value-Or $high.playerLogErrorCount
$highMutated = Value-Or $high.highDetailSceneMutated
$highBootstrap = Value-Or $high.p9P10RuntimeBootstrapCompatibility
$highGreen = Value-Or $high.greenFrameRuntimeCompatibility
$highCandidate = Value-Or $high.candidateMarkerRuntimeCompatibility
$highLight = Value-Or $high.lightCurtainRiskFrontCompatibility

$logStatus = Value-Or $log.status
$logPath = Value-Or $log.logPath
$logLines = Value-Or $log.lineCount
$logWarnings = Value-Or $log.warningsCount
$logErrors = Value-Or $log.errorsCount
$logExceptions = Value-Or $log.exceptionsCount
$logMissingAssets = Value-Or $log.missingAssetReferenceCount
$logShaderMaterials = Value-Or $log.shaderMaterialWarningCount
$logPerformance = Value-Or $log.performanceWarningCount
$logRepeated = Value-Or $log.repeatedLogSpamCount

$pagingWorkingTrend = Value-Or $paging.workingSetTrend
$pagingPrivateTrend = Value-Or $paging.privateMemoryTrend
$pagingWorkingGrowth = Value-Or $paging.workingSetGrowthMb
$pagingPrivateGrowth = Value-Or $paging.privateMemoryGrowthMb
$pagingMaxWorking = Value-Or $paging.maxWorkingSetMb
$pagingMaxPrivate = Value-Or $paging.maxPrivateMemoryMb
$pagingPagesAvailable = Value-Or $paging.pagesPerSecAvailable
$pagingAvgPages = Value-Or $paging.averagePagesPerSec
$pagingMaxPages = Value-Or $paging.maxPagesPerSec
$pagingAvgReads = Value-Or $paging.averagePageReadsPerSec
$pagingMaxReads = Value-Or $paging.maxPageReadsPerSec
$pagingDiskAvailable = Value-Or $paging.diskActiveTimeAvailable
$pagingPagefileAvailable = Value-Or $paging.pagefileUsageAvailable

$decisionValue = Value-Or $decision.readinessDecision
$decisionReason = Value-Or $decision.reason
$decisionProcessDone = Value-Or $decision.processSamplingCompleted
$decisionFpsDone = Value-Or $decision.fpsStutterEvidenceCaptured
$decisionHighDetailDone = Value-Or $decision.highDetailFullLoadAttempted
$decisionHighDetailMutated = Value-Or $decision.highDetailSceneMutated
$decisionTempBuild = Value-Or $decision.temporaryBuildUsed
$decisionLimitations = if ($decision -and $decision.remainingLimitations) { ($decision.remainingLimitations | ForEach-Object { "- $_" }) -join "`n" } else { "- None recorded." }

Write-Doc "P10C_MM_EXTENDED_PERFORMANCE_SAMPLING.md" @"
# P10-C-- Extended Performance Sampling

Generated: $date local time.

P10-C-- is an extended pre-release performance gate before official P10-C. It is not official P10-C, not the final release, and not release packaging or archive work. No final release package is created here. No P10-E, P10-F, or P10-G stage is created.

## Scope

- Run 3-minute, 5-minute, and 10-minute process sampling against the temporary Windows profiling/test player.
- Capture built-player FPS, frame time, 1 percent low proxy, and stutter evidence through a player-only runtime exporter.
- Attempt high-detail full-load validation for `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` without mutating the scene.
- Parse Player.log and summarize warnings, errors, exceptions, missing references, shader/material issues, repeated spam, and performance warning patterns.
- Collect memory trend and paging-risk proxies where Windows counters are available.

P10-C remains the official Windows EXE build, release package, documentation package, and archive stage.
"@

Write-Doc "P10C_MM_EXTENDED_PROCESS_SAMPLING_REPORT.md" @"
# P10-C-- Extended Process Sampling Report

Generated: $date local time.

| Duration | Status | Samples | Avg Working Set | Max Private Memory | Avg CPU Proxy | Memory Trend |
|---|---:|---:|---:|---:|---:|---|
$(Process-Row $p3)
$(Process-Row $p5)
$(Process-Row $p10)

## Notes

- CPU percent is a process CPU-time delta proxy divided by logical processor count.
- Disk/pagefile counters are included only when PowerShell performance counters are available.
- Build/player logs remain outside Git; only summarized JSON is committed.
"@

Write-Doc "P10C_MM_FPS_FRAME_STUTTER_REPORT.md" @"
# P10-C-- FPS / Frame Time / Stutter Report

Generated: $date local time.

Status: ``$fpsStatus``

| Metric | Value |
|---|---:|
| Scenario | ``$fpsScenario`` |
| Samples | $fpsSamples |
| Average FPS | $fpsAverage |
| Minimum FPS proxy | $fpsMin |
| 1 percent low FPS proxy | $fpsOnePercentLow |
| Average frame time | $fpsAverageFrameMs ms |
| Max frame time | $fpsMaxFrameMs ms |
| Frames > 33 ms | $fpsOver33 |
| Frames > 50 ms | $fpsOver50 |
| Frames > 100 ms | $fpsOver100 |
| Stutter events | $fpsStutters |

Runtime state: quality=``$fpsRuntimeQuality``, weather=``$fpsRuntimeWeather``, greenFrameActive=``$fpsRuntimeGreenFrame``, lightCurtainActive=``$fpsRuntimeLightCurtain``, crowdActive=``$fpsRuntimeCrowd``, ResultPanel active=``$fpsRuntimeResultPanel``.

Limitations: $fpsLimitations
"@

Write-Doc "P10C_MM_HIGH_DETAIL_FULL_LOAD_VALIDATION.md" @"
# P10-C-- High-Detail Full-Load Validation

Generated: $date local time.

Scene: ``$highScenePath``

Status: ``$highStatus``

- Scene exists: $highSceneExists
- Automated player load attempted: $highLoadAttempted
- Automated player load succeeded: $highLoadSucceeded
- Loading time: $highLoadingSeconds seconds
- Max private memory after load/sample: $highPrivateMb MB
- Player.log warnings/errors: $highWarnings/$highErrors
- High-detail scene mutated: $highMutated

Compatibility evidence:

- P9/P10 bootstrap: $highBootstrap
- Green frame runtime: $highGreen
- Candidate marker runtime: $highCandidate
- Light curtain/risk front: $highLight

The protected scene is not mutated by this gate.
"@

$scenarioLines = if ($scenarios -and $scenarios.scenarios) {
    @($scenarios.scenarios | ForEach-Object { "| ``$($_.scenarioId)`` | $($_.status) | $($_.evidence) |" }) -join "`n"
} else {
    "| missing | n/a | n/a |"
}
Write-Doc "P10C_MM_SCENARIO_PERFORMANCE_REPORT.md" @"
# P10-C-- Scenario Performance Report

Generated: $date local time.

| Scenario | Status | Evidence |
|---|---|---|
$scenarioLines

Untriggered rows are honest limitations, not pass claims. The exporter is present for built-player capture, but gameplay activation still needs manual or future input automation where marked.
"@

Write-Doc "P10C_MM_PLAYER_LOG_REPORT.md" @"
# P10-C-- Player.log Report

Generated: $date local time.

Status: ``$logStatus``

- Log path: ``$logPath``
- Lines parsed: $logLines
- Warnings: $logWarnings
- Errors: $logErrors
- Exceptions: $logExceptions
- Missing asset/reference patterns: $logMissingAssets
- Shader/material warning patterns: $logShaderMaterials
- Performance warning patterns: $logPerformance
- Repeated log spam entries: $logRepeated

The report stores bounded samples only; raw logs are not committed.
"@

Write-Doc "P10C_MM_DISK_PAGING_MEMORY_SWAP_REPORT.md" @"
# P10-C-- Disk Paging / Memory Swap Report

Generated: $date local time.

- Working set trend: $pagingWorkingTrend
- Private memory trend: $pagingPrivateTrend
- Working set growth: $pagingWorkingGrowth MB
- Private memory growth: $pagingPrivateGrowth MB
- Max working set: $pagingMaxWorking MB
- Max private memory: $pagingMaxPrivate MB
- Pages/sec available: $pagingPagesAvailable
- Avg/max Pages/sec: $pagingAvgPages/$pagingMaxPages
- Avg/max Page Reads/sec: $pagingAvgReads/$pagingMaxReads
- Disk active time available: $pagingDiskAvailable
- Pagefile usage available: $pagingPagefileAvailable

No OS pagefile settings were changed.

Manual follow-up: use Resource Monitor and Task Manager with the steps stored in `Assets/Data/P10/p10c_mm_paging_risk_summary.json`.
"@

Write-Doc "P10C_MM_READINESS_DECISION.md" @"
# P10-C-- Readiness Decision

Generated: $date local time.

Decision: ``$decisionValue``

Reason: $decisionReason

P10-C-- is not official P10-C and not the final release. No final release package or archive was created. No P10-E, P10-F, or P10-G was created. P10-C remains the official Windows EXE build and release package stage.

## Evidence

- 3/5/10 minute process sampling completed: $decisionProcessDone
- FPS/stutter evidence captured: $decisionFpsDone
- High-detail full-load attempted: $decisionHighDetailDone
- High-detail scene mutated: $decisionHighDetailMutated
- Temporary build used: ``$decisionTempBuild``

## Remaining Limitations

$decisionLimitations
"@

Write-Doc "P10C_MM_TEST_RESULTS.md" @"
# P10-C-- Test Results

Generated: $date local time.

Validation commands for this gate:

- `powershell -ExecutionPolicy Bypass -File tools/p10/run_p10c_mm_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/p10/run_p10c_mm_extended_process_sampling.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p10/run_p10c_mm_fps_stutter_capture.ps1`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p10c_mm.md`

Current summarized evidence:

- 3-minute sampling: $(Value-Or $p3.status), samples=$(Value-Or $p3.sampleCount)
- 5-minute sampling: $(Value-Or $p5.status), samples=$(Value-Or $p5.sampleCount)
- 10-minute sampling: $(Value-Or $p10.status), samples=$(Value-Or $p10.sampleCount)
- FPS summary: $fpsStatus, samples=$fpsSamples
- Player.log warnings/errors: $logWarnings/$logErrors
- Readiness: ``$decisionValue``

No final release package/archive is created by P10-C--.
"@

Write-Doc "P10C_MM_NEXT_STEPS_TO_P10C.md" @"
# P10-C-- Next Steps To P10-C

P10-C remains the official Windows EXE build, release package, documentation package, and archive stage.

Do not create P10-E, P10-F, or P10-G.

Before official P10-C:

- Confirm P10-C-- preflight, EditMode, PlayMode, and DeepSeek pass.
- Confirm no final release package/archive or build artifacts are staged.
- Confirm protected paths are clean, including `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- If scenario rows remain not observed, run manual high-detail player checks for tsunami start, green frames, light curtain, crowd, ResultPanel, and night/rain.
- Proceed to P10-C only after the readiness decision is `ready_for_p10c` or an approved `ready_with_limitations`.

Current decision: ``$decisionValue``.
"@

Write-Host "P10-C-- report writing complete." -ForegroundColor Green
exit 0
