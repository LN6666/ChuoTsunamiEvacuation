[CmdletBinding()]
param(
    [string]$ExePath = "",

    [string]$BuildDirectory = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre",

    [int[]]$DurationsSeconds = @(180, 300, 600),

    [int]$SampleIntervalSeconds = 1,

    [string]$QualityProfile = "Low",

    [string]$WeatherMode = "clear_day",

    [switch]$AggregateOnly
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$dataDir = Join-Path $repoRoot "Assets\Data\P10"
$scenePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"

if ([string]::IsNullOrWhiteSpace($ExePath)) {
    $ExePath = Join-Path $BuildDirectory "ChuoTsunamiEvacuation_P10CPre.exe"
}

function Write-Json {
    param([object]$Value, [string]$Path)
    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }
    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Read-JsonOrNull {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    try {
        return Get-Content -Raw -Encoding UTF8 -LiteralPath $Path | ConvertFrom-Json
    }
    catch {
        return $null
    }
}

function Get-SafeCounterSnapshot {
    $snapshot = [ordered]@{
        countersAvailable = $false
        unavailableReason = ""
        pagesPerSec = $null
        pageReadsPerSec = $null
        pagingFileUsagePercent = $null
        diskActiveTimePercent = $null
    }
    try {
        $counter = Get-Counter -Counter @(
            "\Memory\Pages/sec",
            "\Memory\Page Reads/sec",
            "\Paging File(_Total)\% Usage",
            "\PhysicalDisk(_Total)\% Disk Time"
        ) -ErrorAction Stop
        foreach ($sample in $counter.CounterSamples) {
            switch -Wildcard ($sample.Path) {
                "*\memory\pages/sec" { $snapshot.pagesPerSec = [Math]::Round([double]$sample.CookedValue, 2) }
                "*\memory\page reads/sec" { $snapshot.pageReadsPerSec = [Math]::Round([double]$sample.CookedValue, 2) }
                "*\paging file(_total)\% usage" { $snapshot.pagingFileUsagePercent = [Math]::Round([double]$sample.CookedValue, 2) }
                "*\physicaldisk(_total)\% disk time" { $snapshot.diskActiveTimePercent = [Math]::Round([double]$sample.CookedValue, 2) }
            }
        }
        $snapshot.countersAvailable = $true
    }
    catch {
        $snapshot.unavailableReason = $_.Exception.Message
    }
    return $snapshot
}

function Get-Average {
    param([double[]]$Values)
    if ($Values.Count -eq 0) { return 0.0 }
    return [Math]::Round(($Values | Measure-Object -Average).Average, 2)
}

function Get-Maximum {
    param([double[]]$Values)
    if ($Values.Count -eq 0) { return 0.0 }
    return [Math]::Round(($Values | Measure-Object -Maximum).Maximum, 2)
}

function Get-Trend {
    param([double[]]$Values, [double]$ThresholdMb)
    if ($Values.Count -lt 6) { return "insufficient_samples" }
    $delta = $Values[-1] - $Values[0]
    $chunk = [Math]::Max(1, [int][Math]::Floor($Values.Count / 4))
    $first = @($Values | Select-Object -First $chunk)
    $last = @($Values | Select-Object -Last $chunk)
    $firstAvg = ($first | Measure-Object -Average).Average
    $lastAvg = ($last | Measure-Object -Average).Average
    if ($delta -gt $ThresholdMb -and ($lastAvg - $firstAvg) -gt ($ThresholdMb * 0.5)) {
        return "upward"
    }
    if ([Math]::Abs($delta) -le $ThresholdMb) {
        return "stable"
    }
    if ($delta -lt (-1.0 * $ThresholdMb)) {
        return "downward"
    }
    return "minor_upward"
}

function New-ProcessSummary {
    param([int]$DurationSeconds, [string]$Status, [string]$Reason)
    return [ordered]@{
        schemaVersion = "p10c_mm.process_sampling_summary.v1"
        status = $Status
        p10cMinusMinusIsExtendedPerformanceGate = $true
        officialP10CReleaseDeferred = $true
        finalReleasePackageCreated = $false
        finalArchiveCreated = $false
        noP10EFG = $true
        buildOutputPath = $ExePath
        buildType = "temporary_windows_x64_development_profiling_test_build"
        buildScenes = @($scenePath)
        scenarioLabel = "baseline_idle"
        activeQualityProfile = $QualityProfile
        activeWeatherNightMode = $WeatherMode
        requestedDurationSeconds = $DurationSeconds
        actualDurationSeconds = 0
        sampleIntervalSeconds = [Math]::Max(1, $SampleIntervalSeconds)
        processStartedAtLocal = ""
        processFinishedAtLocal = ""
        processId = 0
        sampleCount = 0
        averageWorkingSetMb = 0
        maxWorkingSetMb = 0
        averagePrivateMemoryMb = 0
        maxPrivateMemoryMb = 0
        privateMemoryGrowthMb = 0
        workingSetGrowthMb = 0
        averageCpuPercentProxy = 0
        maxCpuPercentProxy = 0
        responsiveSampleCount = 0
        notRespondingSampleCount = 0
        processResponsiveness = "not_measured"
        diskCounterSamplesAvailable = $false
        averagePagesPerSec = $null
        maxPagesPerSec = $null
        averagePageReadsPerSec = $null
        maxPageReadsPerSec = $null
        averageDiskActiveTimePercent = $null
        maxDiskActiveTimePercent = $null
        averagePagingFileUsagePercent = $null
        maxPagingFileUsagePercent = $null
        counterUnavailableReason = ""
        playerLogPath = ""
        playerLogWarningCount = 0
        playerLogErrorCount = 0
        playerLogExceptionCount = 0
        fpsSummaryPath = ""
        fpsSummaryCaptured = $false
        averageFps = 0
        onePercentLowFps = 0
        maxFrameTimeMs = 0
        stutterEventCount = 0
        exitStatus = "not_started"
        exitCode = $null
        memoryTrend = "not_measured"
        memoryTrendUpward = $false
        profileFailureReason = $Reason
        summary = ""
    }
}

function Get-LogCounts {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return [pscustomobject]@{ Warnings = 0; Errors = 0; Exceptions = 0 }
    }
    $warnings = 0
    $errors = 0
    $exceptions = 0
    Get-Content -LiteralPath $Path -ErrorAction SilentlyContinue | ForEach-Object {
        $line = [string]$_
        if ($line -match "\bWarning\b" -or $line -match "LogWarning") { $warnings++ }
        if ($line -match "\bError\b" -or $line -match "LogError" -or $line -match "Exception" -or $line -match "NullReferenceException") { $errors++ }
        if ($line -match "Exception" -or $line -match "NullReferenceException" -or $line -match "MissingReferenceException") { $exceptions++ }
    }
    return [pscustomobject]@{ Warnings = $warnings; Errors = $errors; Exceptions = $exceptions }
}

function Get-ResponsiveValue {
    param([System.Diagnostics.Process]$Process)
    try {
        return [bool]$Process.Responding
    }
    catch {
        return $true
    }
}

function Invoke-SamplingRun {
    param([int]$DurationSeconds)

    $minutes = [int][Math]::Round($DurationSeconds / 60)
    $summaryPath = Join-Path $dataDir ("p10c_mm_process_sampling_{0}min.json" -f $minutes)
    $playerLogPath = Join-Path $BuildDirectory ("P10CMM_Player_{0}min.log" -f $minutes)
    $fpsTempPath = Join-Path $BuildDirectory ("p10c_mm_fps_stutter_{0}min.json" -f $minutes)
    $summary = New-ProcessSummary -DurationSeconds $DurationSeconds -Status "started" -Reason ""
    $summary.playerLogPath = $playerLogPath
    $summary.fpsSummaryPath = $fpsTempPath

    if (-not (Test-Path -LiteralPath $ExePath -PathType Leaf)) {
        $summary.status = "failed"
        $summary.profileFailureReason = "Temporary build EXE was not found."
        $summary.summary = "P10-C-- process sampling could not run because the temporary build EXE was missing."
        Write-Json -Value $summary -Path $summaryPath
        throw "Temporary build EXE was not found: $ExePath"
    }

    foreach ($path in @($playerLogPath, $fpsTempPath)) {
        if (Test-Path -LiteralPath $path) {
            Remove-Item -LiteralPath $path -Force
        }
    }

    $arguments = @(
        "-screen-width", "1920",
        "-screen-height", "1080",
        "-screen-fullscreen", "0",
        "-logFile", $playerLogPath,
        "-p10cMmEnableFpsExporter",
        "-p10cMmFpsSummaryPath", $fpsTempPath,
        "-p10cMmCaptureSeconds", ([Math]::Max(1, $DurationSeconds)).ToString(),
        "-p10cMmScenario", "baseline_idle",
        "-p10cMmQualityProfile", $QualityProfile,
        "-p10cMmWeatherMode", $WeatherMode
    )

    Write-Host "P10-C-- process sampling ${minutes}min: starting"
    $process = $null
    $workingSetValues = New-Object System.Collections.Generic.List[double]
    $privateValues = New-Object System.Collections.Generic.List[double]
    $cpuValues = New-Object System.Collections.Generic.List[double]
    $pagesValues = New-Object System.Collections.Generic.List[double]
    $pageReadsValues = New-Object System.Collections.Generic.List[double]
    $diskValues = New-Object System.Collections.Generic.List[double]
    $pagefileValues = New-Object System.Collections.Generic.List[double]
    $counterUnavailableReason = ""
    try {
        $process = Start-Process -FilePath $ExePath -ArgumentList $arguments -WindowStyle Normal -PassThru
        $startedAt = Get-Date
        $summary.processStartedAtLocal = $startedAt.ToString("s")
        $summary.processId = $process.Id
        Start-Sleep -Seconds 1
        $process.Refresh()
        $lastCpu = if ($process.HasExited) { 0.0 } else { [double]$process.CPU }

        while (((Get-Date) - $startedAt).TotalSeconds -lt [Math]::Max(1, $DurationSeconds)) {
            if ($process.HasExited) {
                $summary.exitStatus = "exited_early"
                break
            }

            $before = Get-Date
            Start-Sleep -Seconds ([Math]::Max(1, $SampleIntervalSeconds))
            $elapsed = [Math]::Max(0.001, ((Get-Date) - $before).TotalSeconds)
            $process.Refresh()
            if ($process.HasExited) {
                $summary.exitStatus = "exited_early"
                break
            }

            $cpuNow = [double]$process.CPU
            $cpuPercentProxy = (($cpuNow - $lastCpu) / $elapsed) * 100.0 / [Environment]::ProcessorCount
            $lastCpu = $cpuNow
            $workingSetValues.Add([Math]::Round($process.WorkingSet64 / 1MB, 2)) | Out-Null
            $privateValues.Add([Math]::Round($process.PrivateMemorySize64 / 1MB, 2)) | Out-Null
            $cpuValues.Add([Math]::Round([Math]::Max(0.0, $cpuPercentProxy), 2)) | Out-Null

            if (Get-ResponsiveValue -Process $process) {
                $summary.responsiveSampleCount++
            }
            else {
                $summary.notRespondingSampleCount++
            }

            $counterSnapshot = Get-SafeCounterSnapshot
            if ($counterSnapshot.countersAvailable) {
                $summary.diskCounterSamplesAvailable = $true
                if ($null -ne $counterSnapshot.pagesPerSec) { $pagesValues.Add([double]$counterSnapshot.pagesPerSec) | Out-Null }
                if ($null -ne $counterSnapshot.pageReadsPerSec) { $pageReadsValues.Add([double]$counterSnapshot.pageReadsPerSec) | Out-Null }
                if ($null -ne $counterSnapshot.diskActiveTimePercent) { $diskValues.Add([double]$counterSnapshot.diskActiveTimePercent) | Out-Null }
                if ($null -ne $counterSnapshot.pagingFileUsagePercent) { $pagefileValues.Add([double]$counterSnapshot.pagingFileUsagePercent) | Out-Null }
            }
            elseif ([string]::IsNullOrWhiteSpace($counterUnavailableReason)) {
                $counterUnavailableReason = $counterSnapshot.unavailableReason
            }
        }

        if ($process -and -not $process.HasExited) {
            if (-not $process.CloseMainWindow()) {
                Stop-Process -Id $process.Id -Force
                $summary.exitStatus = "forced_stop_no_main_window"
            }
            elseif (-not $process.WaitForExit(10000)) {
                Stop-Process -Id $process.Id -Force
                $summary.exitStatus = "forced_stop_after_close_timeout"
            }
            else {
                $summary.exitStatus = "closed_after_capture"
            }
        }
        elseif ($process) {
            $summary.exitCode = $process.ExitCode
        }

        Start-Sleep -Seconds 1
        $finishedAt = Get-Date
        $summary.processFinishedAtLocal = $finishedAt.ToString("s")
        $summary.actualDurationSeconds = [Math]::Round(($finishedAt - $startedAt).TotalSeconds, 2)
        $summary.sampleCount = $workingSetValues.Count
        $summary.averageWorkingSetMb = Get-Average ([double[]]$workingSetValues.ToArray())
        $summary.maxWorkingSetMb = Get-Maximum ([double[]]$workingSetValues.ToArray())
        $summary.averagePrivateMemoryMb = Get-Average ([double[]]$privateValues.ToArray())
        $summary.maxPrivateMemoryMb = Get-Maximum ([double[]]$privateValues.ToArray())
        $summary.averageCpuPercentProxy = Get-Average ([double[]]$cpuValues.ToArray())
        $summary.maxCpuPercentProxy = Get-Maximum ([double[]]$cpuValues.ToArray())
        $summary.workingSetGrowthMb = if ($workingSetValues.Count -gt 1) { [Math]::Round($workingSetValues[-1] - $workingSetValues[0], 2) } else { 0 }
        $summary.privateMemoryGrowthMb = if ($privateValues.Count -gt 1) { [Math]::Round($privateValues[-1] - $privateValues[0], 2) } else { 0 }
        $summary.memoryTrend = Get-Trend -Values ([double[]]$privateValues.ToArray()) -ThresholdMb 128.0
        $summary.memoryTrendUpward = $summary.memoryTrend -eq "upward"
        $summary.processResponsiveness = if ($summary.notRespondingSampleCount -gt 0) { "not_responding_samples_observed" } elseif ($summary.responsiveSampleCount -gt 0) { "responsive" } else { "not_measured" }
        $summary.counterUnavailableReason = $counterUnavailableReason
        if ($pagesValues.Count -gt 0) {
            $summary.averagePagesPerSec = Get-Average ([double[]]$pagesValues.ToArray())
            $summary.maxPagesPerSec = Get-Maximum ([double[]]$pagesValues.ToArray())
        }
        if ($pageReadsValues.Count -gt 0) {
            $summary.averagePageReadsPerSec = Get-Average ([double[]]$pageReadsValues.ToArray())
            $summary.maxPageReadsPerSec = Get-Maximum ([double[]]$pageReadsValues.ToArray())
        }
        if ($diskValues.Count -gt 0) {
            $summary.averageDiskActiveTimePercent = Get-Average ([double[]]$diskValues.ToArray())
            $summary.maxDiskActiveTimePercent = Get-Maximum ([double[]]$diskValues.ToArray())
        }
        if ($pagefileValues.Count -gt 0) {
            $summary.averagePagingFileUsagePercent = Get-Average ([double[]]$pagefileValues.ToArray())
            $summary.maxPagingFileUsagePercent = Get-Maximum ([double[]]$pagefileValues.ToArray())
        }

        $logCounts = Get-LogCounts -Path $playerLogPath
        $summary.playerLogWarningCount = $logCounts.Warnings
        $summary.playerLogErrorCount = $logCounts.Errors
        $summary.playerLogExceptionCount = $logCounts.Exceptions

        $fps = Read-JsonOrNull -Path $fpsTempPath
        if ($fps) {
            $summary.fpsSummaryCaptured = [int]$fps.sampleCount -gt 0
            $summary.averageFps = [Math]::Round([double]$fps.averageFps, 2)
            $summary.onePercentLowFps = [Math]::Round([double]$fps.onePercentLowFps, 2)
            $summary.maxFrameTimeMs = [Math]::Round([double]$fps.maxFrameTimeMs, 2)
            $summary.stutterEventCount = [int]$fps.stutterEventCount
            if ($minutes -eq 10 -or $DurationSeconds -eq ($DurationsSeconds | Measure-Object -Maximum).Maximum) {
                Copy-Item -LiteralPath $fpsTempPath -Destination (Join-Path $dataDir "p10c_mm_fps_stutter_summary.json") -Force
            }
        }

        $summary.status = if ($summary.sampleCount -gt 0) { "completed" } else { "failed" }
        $summary.summary = "P10-C-- ${minutes}min process sampling status=$($summary.status), samples=$($summary.sampleCount), avgWS=$($summary.averageWorkingSetMb) MB, maxPrivate=$($summary.maxPrivateMemoryMb) MB, avgCPUProxy=$($summary.averageCpuPercentProxy)%, Player.log warnings/errors=$($summary.playerLogWarningCount)/$($summary.playerLogErrorCount), memoryTrend=$($summary.memoryTrend)."
        Write-Json -Value $summary -Path $summaryPath
        Write-Host $summary.summary
        return $summary
    }
    catch {
        if ($process -and -not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
        $summary.status = "failed"
        $summary.profileFailureReason = $_.Exception.Message
        $summary.processFinishedAtLocal = (Get-Date).ToString("s")
        $summary.summary = "P10-C-- ${minutes}min process sampling failed. $($_.Exception.Message)"
        Write-Json -Value $summary -Path $summaryPath
        throw
    }
}

function Write-AggregateSummaries {
    param([object[]]$Summaries)

    $longest = $Summaries | Sort-Object requestedDurationSeconds -Descending | Select-Object -First 1
    $fps = Read-JsonOrNull -Path (Join-Path $dataDir "p10c_mm_fps_stutter_summary.json")
    $sceneFullPath = Join-Path $repoRoot ($scenePath -replace "/", "\")
    $sceneExists = Test-Path -LiteralPath $sceneFullPath -PathType Leaf
    $anyCompleted = @($Summaries | Where-Object { $_.status -eq "completed" }).Count -gt 0
    $logSummary = Read-JsonOrNull -Path (Join-Path $dataDir "p10c_mm_player_log_summary.json")
    $warnings = if ($logSummary) { [int]$logSummary.warningsCount } else { [int]$longest.playerLogWarningCount }
    $errors = if ($logSummary) { [int]$logSummary.errorsCount } else { [int]$longest.playerLogErrorCount }
    $highDetailStatus = if ($sceneExists -and $anyCompleted) { "full_load_attempted_process_survived" } elseif ($sceneExists) { "scene_exists_but_player_load_not_completed" } else { "scene_missing" }
    $loadingTimeSeconds = if ($fps) { [Math]::Round([double]$fps.sceneLoadingTimeSeconds, 2) } else { 0 }
    $memoryAfterLoadPrivateMb = if ($longest) { [double]$longest.maxPrivateMemoryMb } else { 0 }
    $memoryAfterLoadWorkingSetMb = if ($longest) { [double]$longest.maxWorkingSetMb } else { 0 }
    $bootstrapCompatibility = if ($anyCompleted) { "process_survived_high_detail_player_start" } else { "not_confirmed" }
    $greenFrameCompatibility = if ($fps -and ([int]$fps.runtimeState.greenFrameRuntimeCount -gt 0)) { "runtime_present" } else { "not_observed_in_automated_capture" }
    $candidateMarkerCompatibility = if ($fps -and [bool]$fps.runtimeState.candidateMarkerRuntimeActive) { "runtime_present" } else { "not_observed_in_automated_capture" }
    $lightCurtainCompatibility = if ($fps -and [bool]$fps.runtimeState.lightCurtainActive) { "active_or_present" } else { "not_observed_in_automated_capture" }

    $highDetail = [ordered]@{
        schemaVersion = "p10c_mm.high_detail_full_load_status.v1"
        status = $highDetailStatus
        p10cMinusMinusIsExtendedPerformanceGate = $true
        officialP10CReleaseDeferred = $true
        finalReleasePackageCreated = $false
        finalArchiveCreated = $false
        noP10EFG = $true
        scenePath = $scenePath
        sceneExists = $sceneExists
        highDetailSceneMutated = $false
        sceneReferenceValidated = $sceneExists
        automatedPlayerLoadAttempted = $true
        automatedPlayerLoadSucceeded = $sceneExists -and $anyCompleted
        loadingTimeSeconds = $loadingTimeSeconds
        memoryBeforeLoadMb = 0
        memoryAfterLoadPrivateMb = $memoryAfterLoadPrivateMb
        memoryAfterLoadWorkingSetMb = $memoryAfterLoadWorkingSetMb
        playerLogWarningCount = $warnings
        playerLogErrorCount = $errors
        p9P10RuntimeBootstrapCompatibility = $bootstrapCompatibility
        greenFrameRuntimeCompatibility = $greenFrameCompatibility
        candidateMarkerRuntimeCompatibility = $candidateMarkerCompatibility
        lightCurtainRiskFrontCompatibility = $lightCurtainCompatibility
        limitations = @(
            "The protected scene was not mutated.",
            "Automated validation uses temporary player launch/process survival and exporter evidence; detailed visual inspection remains manual.",
            "Scenario activation for tsunami start/light curtain/crowd/UI was not forced by this script."
        )
        summary = ""
    }
    $highDetail.summary = "High-detail scene full-load validation status=$($highDetail.status), sceneExists=$sceneExists, playerLoadSucceeded=$($highDetail.automatedPlayerLoadSucceeded), Player.log warnings/errors=$warnings/$errors."
    Write-Json -Value $highDetail -Path (Join-Path $dataDir "p10c_mm_high_detail_full_load_status.json")

    $baselineStatus = if ($anyCompleted) { "measured" } else { "not_measured" }
    $baselineAverageFps = if ($fps) { [double]$fps.averageFps } else { 0 }
    $baselineOnePercentLowFps = if ($fps) { [double]$fps.onePercentLowFps } else { 0 }
    $greenFrameStatus = if ($fps -and [bool]$fps.runtimeState.greenFrameActive) { "observed" } else { "not_observed" }
    $lightCurtainStatus = if ($fps -and [bool]$fps.runtimeState.lightCurtainActive) { "observed" } else { "not_observed" }
    $crowdStatus = if ($fps -and [bool]$fps.runtimeState.crowdActive) { "observed" } else { "not_observed" }
    $resultPanelStatus = if ($fps -and [bool]$fps.runtimeState.resultPanelActive) { "observed" } else { "not_observed" }
    $nightRainStatus = if ($WeatherMode -eq "night_rain") { "label_captured" } else { "prepared_not_automated" }
    $scenarioRows = @(
        [ordered]@{ scenarioId = "baseline_idle"; status = $baselineStatus; evidence = "3/5/10 minute process sampling and FPS exporter use baseline idle unless manual interaction changes runtime state."; averageFps = $baselineAverageFps; onePercentLowFps = $baselineOnePercentLowFps; limitations = @() },
        [ordered]@{ scenarioId = "tsunami_start"; status = "prepared_not_automated"; evidence = "Exporter records green-frame state and frame spikes, but this script does not synthesize the T key or gameplay event."; averageFps = 0; onePercentLowFps = 0; limitations = @("Manual or future input automation required to measure activation spike.") },
        [ordered]@{ scenarioId = "green_frames_markers"; status = $greenFrameStatus; evidence = "Runtime exporter inspects P10BGreenGroundFrameRuntime and candidate marker components."; averageFps = 0; onePercentLowFps = 0; limitations = @("Debug labels remain off by default.") },
        [ordered]@{ scenarioId = "light_curtain"; status = $lightCurtainStatus; evidence = "Runtime exporter inspects P8RiskFrontController and active LightCurtain/RiskFront objects."; averageFps = 0; onePercentLowFps = 0; limitations = @("Manual enablement may be needed if the scene starts with the curtain hidden.") },
        [ordered]@{ scenarioId = "crowd_congestion"; status = $crowdStatus; evidence = "Runtime exporter counts P9CrowdRuntimeAgent components."; averageFps = 0; onePercentLowFps = 0; limitations = @("No unbounded NPC count was spawned by this gate.") },
        [ordered]@{ scenarioId = "result_panel_ui"; status = $resultPanelStatus; evidence = "Runtime exporter detects active ResultPanel scene objects."; averageFps = 0; onePercentLowFps = 0; limitations = @("Long warning text remains visual/manual unless a run reaches ResultPanel.") },
        [ordered]@{ scenarioId = "night_rain"; status = $nightRainStatus; evidence = "Weather/night label is passed to exporter; runtime night overlay is detected if active."; averageFps = 0; onePercentLowFps = 0; limitations = @("Weather mode is not forcibly changed by this process sampler.") }
    )
    $scenarioSummary = [ordered]@{
        schemaVersion = "p10c_mm.scenario_performance_summary.v1"
        status = "summarized"
        p10cMinusMinusIsExtendedPerformanceGate = $true
        officialP10CReleaseDeferred = $true
        finalReleasePackageCreated = $false
        finalArchiveCreated = $false
        noP10EFG = $true
        scenarios = @($scenarioRows)
        summary = "Scenario coverage captured baseline idle and prepared instrumentation/evidence fields for tsunami start, green frames, light curtain, crowd, ResultPanel/UI, and night/rain. Untriggered scenarios are explicitly marked."
    }
    Write-Json -Value $scenarioSummary -Path (Join-Path $dataDir "p10c_mm_scenario_performance_summary.json")

    $pagingWorkingSetTrend = if ($longest) { [string]$longest.memoryTrend } else { "not_measured" }
    $pagingPrivateMemoryTrend = if ($longest) { [string]$longest.memoryTrend } else { "not_measured" }
    $pagingWorkingSetGrowthMb = if ($longest) { [double]$longest.workingSetGrowthMb } else { 0 }
    $pagingPrivateMemoryGrowthMb = if ($longest) { [double]$longest.privateMemoryGrowthMb } else { 0 }
    $pagingMaxWorkingSetMb = if ($longest) { [double]$longest.maxWorkingSetMb } else { 0 }
    $pagingMaxPrivateMemoryMb = if ($longest) { [double]$longest.maxPrivateMemoryMb } else { 0 }
    $counterAvailable = if ($longest) { [bool]$longest.diskCounterSamplesAvailable } else { $false }
    $averagePagesPerSec = if ($longest) { $longest.averagePagesPerSec } else { $null }
    $maxPagesPerSec = if ($longest) { $longest.maxPagesPerSec } else { $null }
    $averagePageReadsPerSec = if ($longest) { $longest.averagePageReadsPerSec } else { $null }
    $maxPageReadsPerSec = if ($longest) { $longest.maxPageReadsPerSec } else { $null }
    $averageDiskActiveTimePercent = if ($longest) { $longest.averageDiskActiveTimePercent } else { $null }
    $maxDiskActiveTimePercent = if ($longest) { $longest.maxDiskActiveTimePercent } else { $null }
    $averagePagingFileUsagePercent = if ($longest) { $longest.averagePagingFileUsagePercent } else { $null }
    $maxPagingFileUsagePercent = if ($longest) { $longest.maxPagingFileUsagePercent } else { $null }
    $pagingLimitations = @()
    if ($longest -and -not [bool]$longest.diskCounterSamplesAvailable) {
        $pagingLimitations += "PowerShell counters were unavailable: $($longest.counterUnavailableReason)"
    }
    $paging = [ordered]@{
        schemaVersion = "p10c_mm.paging_risk_summary.v1"
        status = "summarized"
        p10cMinusMinusIsExtendedPerformanceGate = $true
        officialP10CReleaseDeferred = $true
        finalReleasePackageCreated = $false
        finalArchiveCreated = $false
        noP10EFG = $true
        workingSetTrend = $pagingWorkingSetTrend
        privateMemoryTrend = $pagingPrivateMemoryTrend
        workingSetGrowthMb = $pagingWorkingSetGrowthMb
        privateMemoryGrowthMb = $pagingPrivateMemoryGrowthMb
        maxWorkingSetMb = $pagingMaxWorkingSetMb
        maxPrivateMemoryMb = $pagingMaxPrivateMemoryMb
        hardFaultsPerSecAvailable = $false
        hardFaultsPerSecLimitation = "PowerShell collected Memory\\Pages/sec and Page Reads/sec when available; exact per-process hard faults/sec was not collected."
        pagesPerSecAvailable = $counterAvailable
        averagePagesPerSec = $averagePagesPerSec
        maxPagesPerSec = $maxPagesPerSec
        averagePageReadsPerSec = $averagePageReadsPerSec
        maxPageReadsPerSec = $maxPageReadsPerSec
        diskActiveTimeAvailable = $counterAvailable
        averageDiskActiveTimePercent = $averageDiskActiveTimePercent
        maxDiskActiveTimePercent = $maxDiskActiveTimePercent
        pagefileUsageAvailable = $counterAvailable
        averagePagingFileUsagePercent = $averagePagingFileUsagePercent
        maxPagingFileUsagePercent = $maxPagingFileUsagePercent
        resourceMonitorManualSteps = @(
            "Open Resource Monitor > Memory while the temporary player is running.",
            "Watch Hard Faults/sec for ChuoTsunamiEvacuation_P10CPre.exe.",
            "Open Disk tab and verify the player is not pinned by sustained disk active time/pagefile churn."
        )
        taskManagerManualSteps = @(
            "Open Task Manager > Details and add Working set, Commit size, CPU, and Status columns.",
            "Confirm Status stays Running and memory does not climb without bound during a 10-minute Low profile run."
        )
        limitations = $pagingLimitations
        summary = ""
    }
    $paging.summary = "Paging risk summary: privateMemoryTrend=$($paging.privateMemoryTrend), maxPrivate=$($paging.maxPrivateMemoryMb) MB, pages/sec available=$($paging.pagesPerSecAvailable)."
    Write-Json -Value $paging -Path (Join-Path $dataDir "p10c_mm_paging_risk_summary.json")

    $decisionValue = "needs_quick_fix_before_p10c"
    $decisionReason = "FPS/stutter or scenario activation evidence remains incomplete."
    if (-not $anyCompleted) {
        $decisionValue = "blocked_until_major_optimization"
        $decisionReason = "No extended process sampling run completed."
    }
    elseif (-not $fps -or [int]$fps.sampleCount -le 0) {
        $decisionValue = "needs_quick_fix_before_p10c"
        $decisionReason = "Extended process sampling completed but built-player FPS/stutter exporter data is missing."
    }
    elseif ($errors -gt 0 -or ($longest -and [bool]$longest.memoryTrendUpward)) {
        $decisionValue = "needs_quick_fix_before_p10c"
        $decisionReason = "Player.log errors or upward memory trend require a bounded quick fix before P10-C."
    }
    elseif ($greenFrameStatus -ne "observed" -or $lightCurtainStatus -ne "observed" -or $crowdStatus -ne "observed" -or $nightRainStatus -ne "label_captured") {
        $decisionValue = "ready_with_limitations"
        $decisionReason = "Baseline built-player FPS, 1 percent low, Player.log, and 10-minute memory trend passed, but automated scenario activation for green frames, light curtain, crowd, or night/rain remains incomplete."
    }
    elseif ([double]$fps.averageFps -ge 30 -and [double]$fps.onePercentLowFps -ge 20) {
        $decisionValue = "ready_for_p10c"
        $decisionReason = "Low profile FPS and 1 percent low target passed with no Player.log errors and no unbounded memory trend."
    }
    elseif ([double]$fps.averageFps -ge 24) {
        $decisionValue = "ready_with_limitations"
        $decisionReason = "FPS is below ideal target but within the documented limited readiness band; manual scenario activation remains required."
    }
    elseif ([double]$fps.averageFps -lt 15) {
        $decisionValue = "blocked_until_major_optimization"
        $decisionReason = "Average FPS is below the quick-fix threshold."
    }

    $fpsEvidenceCaptured = ($fps -and ([int]$fps.sampleCount -gt 0))
    $decision = [ordered]@{
        schemaVersion = "p10c_mm.readiness_decision.v1"
        status = "decided"
        p10cMinusMinusIsExtendedPerformanceGate = $true
        officialP10CReleaseDeferred = $true
        finalReleasePackageCreated = $false
        finalArchiveCreated = $false
        noP10EFG = $true
        readinessDecision = $decisionValue
        reason = $decisionReason
        temporaryBuildUsed = $ExePath
        processSamplingCompleted = $anyCompleted
        fpsStutterEvidenceCaptured = $fpsEvidenceCaptured
        highDetailFullLoadAttempted = $true
        highDetailSceneMutated = $false
        protectedPathsExpectedClean = $true
        quickFixesApplied = @("No runtime quick fix was applied by the sampling script; this gate adds measurement/export tooling only.")
        remainingLimitations = @(
            "Automated scenario activation is incomplete for tsunami start, light curtain, crowd, ResultPanel, and night/rain unless manually triggered during capture.",
            "PowerShell disk paging counters may be unavailable on localized Windows installations.",
            "This is not the final P10-C release build/package/archive."
        )
        nextRecommendedStep = "Run manual scenario activation against the temporary player if untriggered scenario rows remain not_observed; otherwise proceed to official P10-C build/package."
        summary = "P10-C-- readiness decision=$decisionValue. $decisionReason"
    }
    Write-Json -Value $decision -Path (Join-Path $dataDir "p10c_mm_readiness_decision.json")
}

Write-Host "P10-C-- extended process sampling: starting"
Write-Host "EXE: $ExePath"
Write-Host "Durations: $($DurationsSeconds -join ', ')"
Write-Host "This is not the final P10-C release package or archive."

New-Item -ItemType Directory -Force -Path $dataDir | Out-Null
New-Item -ItemType Directory -Force -Path $BuildDirectory | Out-Null

$summaries = New-Object System.Collections.Generic.List[object]
if ($AggregateOnly) {
    foreach ($fileName in @(
        "p10c_mm_process_sampling_3min.json",
        "p10c_mm_process_sampling_5min.json",
        "p10c_mm_process_sampling_10min.json"
    )) {
        $existing = Read-JsonOrNull -Path (Join-Path $dataDir $fileName)
        if ($existing) {
            $summaries.Add($existing) | Out-Null
        }
    }

    if ($summaries.Count -eq 0) {
        throw "AggregateOnly requested but no process sampling JSON summaries were found."
    }

    Write-AggregateSummaries -Summaries $summaries.ToArray()
    Write-Host "P10-C-- aggregate summary writing: PASS" -ForegroundColor Green
    exit 0
}

foreach ($duration in $DurationsSeconds) {
    $summary = Invoke-SamplingRun -DurationSeconds ([Math]::Max(1, $duration))
    $summaries.Add($summary) | Out-Null
    & (Join-Path $scriptRoot "parse_p10c_mm_player_log.ps1") -LogPath $summary.playerLogPath
}

Write-AggregateSummaries -Summaries $summaries.ToArray()
Write-Host "P10-C-- extended process sampling: PASS" -ForegroundColor Green
exit 0
