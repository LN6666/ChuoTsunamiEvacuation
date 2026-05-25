[CmdletBinding()]
param(
    [string]$ExePath = "",

    [string]$BuildDirectory = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre",

    [int]$DurationSeconds = 60,

    [int]$SampleIntervalSeconds = 1,

    [string]$QualityProfile = "Low"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$summaryPath = Join-Path $repoRoot "Assets\Data\P10\p10c_pre_built_player_profile_summary.json"

if ([string]::IsNullOrWhiteSpace($ExePath)) {
    $ExePath = Join-Path $BuildDirectory "ChuoTsunamiEvacuation_P10CPre.exe"
}

$playerLogPath = Join-Path $BuildDirectory "P10CPre_Player.log"

function New-BaseSummary {
    param([string]$Status, [string]$FailureReason)
    return [ordered]@{
        schemaVersion = "p10c_pre.built_player_profile_summary.v1"
        status = $Status
        p10cPreIsPerformanceGate = $true
        finalReleasePackageCreated = $false
        finalArchiveCreated = $false
        buildAttempted = $true
        buildSucceeded = (Test-Path -LiteralPath $ExePath -PathType Leaf)
        buildType = "temporary_windows_x64_development_profiling_test_build"
        buildOutputPath = $ExePath
        buildScenes = @("Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity")
        buildStartedAtLocal = ""
        buildFinishedAtLocal = ""
        buildFailureReason = ""
        profileRunAttempted = $true
        profileRunSucceeded = $false
        profileStartedAtLocal = (Get-Date).ToString("s")
        profileFinishedAtLocal = ""
        profileFailureReason = $FailureReason
        playerLogWarningCount = 0
        playerLogErrorCount = 0
        metrics = [ordered]@{
            measured = $false
            startupTimeSeconds = 0.0
            sceneLoadingTimeSeconds = 0.0
            averageFps = 0.0
            onePercentLowFps = 0.0
            minFrameTimeMs = 0.0
            averageFrameTimeMs = 0.0
            maxFrameTimeMs = 0.0
            frameSpikeThresholdMs = 50.0
            frameSpikeCountOverThreshold = 0
            managedHeapMb = 0.0
            profilerAllocatedMemoryMb = 0.0
            workingSetMb = 0.0
            privateMemoryMb = 0.0
            memoryGrowthMb = 0.0
            memoryContinuouslyGrowing = $false
            gcCollectionDelta0 = 0
            gcCollectionDelta1 = 0
            gcCollectionDelta2 = 0
            pagingSymptomsObserved = $false
            fatalFreezeObserved = $false
            npcCount = 0
            markerCount = 0
            greenFrameCount = 0
            lightCurtainEnabled = $false
            resultPanelActive = $false
            weatherMode = "manual_observation_required"
            qualityProfile = $QualityProfile
            notes = "PowerShell process metrics only. Unity runtime FPS/GC markers require in-game instrumentation or manual observation."
        }
        readinessClassification = "blocked"
        summary = ""
    }
}

function Write-Summary {
    param([object]$Summary)
    $Summary["profileFinishedAtLocal"] = (Get-Date).ToString("s")
    $Summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding UTF8
}

function Get-PlayerLogCounts {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return [pscustomobject]@{ Warnings = 0; Errors = 0 }
    }
    $lines = @(Get-Content -LiteralPath $Path -ErrorAction SilentlyContinue)
    $warnings = @($lines | Where-Object { $_ -match "\bWarning\b" -or $_ -match "LogWarning" }).Count
    $errors = @($lines | Where-Object { $_ -match "\bError\b" -or $_ -match "Exception" -or $_ -match "NullReferenceException" }).Count
    return [pscustomobject]@{ Warnings = $warnings; Errors = $errors }
}

Write-Host "P10-C-Pre built-player profile: starting"
Write-Host "EXE: $ExePath"
Write-Host "Duration seconds: $DurationSeconds"
Write-Host "Summary path: $summaryPath"
Write-Host "This profiles a temporary test build, not the final P10-C release package."

if (-not (Test-Path -LiteralPath $ExePath -PathType Leaf)) {
    $summary = New-BaseSummary -Status "profile_failed" -FailureReason "Temporary build EXE was not found."
    $summary.summary = "P10-C-Pre profile could not run because the temporary build EXE was missing."
    Write-Summary -Summary $summary
    throw "Temporary build EXE was not found: $ExePath"
}

if (Test-Path -LiteralPath $playerLogPath) {
    Remove-Item -LiteralPath $playerLogPath -Force
}

$arguments = @(
    "-screen-width", "1920",
    "-screen-height", "1080",
    "-screen-fullscreen", "0",
    "-logFile", $playerLogPath
)

$summary = New-BaseSummary -Status "profile_started" -FailureReason ""
$samples = New-Object System.Collections.Generic.List[object]
$process = $null
try {
    $process = Start-Process -FilePath $ExePath -ArgumentList $arguments -WindowStyle Normal -PassThru
    $startedAt = Get-Date
    $lastCpu = 0.0
    Start-Sleep -Seconds ([Math]::Max(1, $SampleIntervalSeconds))
    $process.Refresh()
    if (-not $process.HasExited) {
        $lastCpu = [double]$process.CPU
    }

    while (((Get-Date) - $startedAt).TotalSeconds -lt [Math]::Max(1, $DurationSeconds)) {
        if ($process.HasExited) {
            break
        }

        $before = Get-Date
        Start-Sleep -Seconds ([Math]::Max(1, $SampleIntervalSeconds))
        $elapsed = [Math]::Max(0.001, ((Get-Date) - $before).TotalSeconds)
        $process.Refresh()
        if ($process.HasExited) {
            break
        }

        $cpuNow = [double]$process.CPU
        $cpuPercentProxy = (($cpuNow - $lastCpu) / $elapsed) * 100.0 / [Environment]::ProcessorCount
        $lastCpu = $cpuNow
        $samples.Add([pscustomobject]@{
            WorkingSetMb = $process.WorkingSet64 / 1MB
            PrivateMemoryMb = $process.PrivateMemorySize64 / 1MB
            CpuPercentProxy = [Math]::Max(0.0, $cpuPercentProxy)
        }) | Out-Null
    }

    if ($process -and -not $process.HasExited) {
        if (-not $process.CloseMainWindow()) {
            Stop-Process -Id $process.Id -Force
        }
        elseif (-not $process.WaitForExit(10000)) {
            Stop-Process -Id $process.Id -Force
        }
    }

    $logCounts = Get-PlayerLogCounts -Path $playerLogPath
    $workingSets = @($samples | ForEach-Object { [double]$_.WorkingSetMb })
    $privateSets = @($samples | ForEach-Object { [double]$_.PrivateMemoryMb })
    $avgWorkingSet = if ($workingSets.Count -gt 0) { ($workingSets | Measure-Object -Average).Average } else { 0.0 }
    $maxWorkingSet = if ($workingSets.Count -gt 0) { ($workingSets | Measure-Object -Maximum).Maximum } else { 0.0 }
    $avgPrivate = if ($privateSets.Count -gt 0) { ($privateSets | Measure-Object -Average).Average } else { 0.0 }
    $maxPrivate = if ($privateSets.Count -gt 0) { ($privateSets | Measure-Object -Maximum).Maximum } else { 0.0 }
    $memoryGrowth = if ($privateSets.Count -gt 1) { [Math]::Max(0.0, $privateSets[-1] - $privateSets[0]) } else { 0.0 }

    $summary.status = "profile_completed"
    $summary.profileRunSucceeded = $samples.Count -gt 0
    $summary.playerLogWarningCount = $logCounts.Warnings
    $summary.playerLogErrorCount = $logCounts.Errors
    $summary.metrics.measured = $samples.Count -gt 0
    $summary.metrics.workingSetMb = [Math]::Round($maxWorkingSet, 2)
    $summary.metrics.privateMemoryMb = [Math]::Round($maxPrivate, 2)
    $summary.metrics.memoryGrowthMb = [Math]::Round($memoryGrowth, 2)
    $summary.metrics.memoryContinuouslyGrowing = $false
    $summary.metrics.notes = "PowerShell sampled process metrics: avgWorkingSetMb=$([Math]::Round($avgWorkingSet, 2)), avgPrivateMemoryMb=$([Math]::Round($avgPrivate, 2)). FPS and Unity markers still require runtime/manual observation."
    if (-not $summary.profileRunSucceeded) {
        $summary.readinessClassification = "blocked"
    }
    elseif ($summary.playerLogErrorCount -gt 0 -or $memoryGrowth -gt 2048.0) {
        $summary.readinessClassification = "needs_major_optimization_before_release"
    }
    else {
        $summary.readinessClassification = "needs_quick_fix"
    }
    $summary.summary = "P10-C-Pre temporary player process profile completed with $($samples.Count) samples, maxWorkingSetMb=$([Math]::Round($maxWorkingSet, 2)), maxPrivateMemoryMb=$([Math]::Round($maxPrivate, 2)), Player.log warnings=$($logCounts.Warnings), errors=$($logCounts.Errors)."
    Write-Summary -Summary $summary
    Write-Host $summary.summary
    Write-Host "P10-C-Pre built-player profile: PASS" -ForegroundColor Green
}
catch {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
    $summary.status = "profile_failed"
    $summary.profileFailureReason = $_.Exception.Message
    $summary.summary = "P10-C-Pre built-player profile failed. $($_.Exception.Message)"
    Write-Summary -Summary $summary
    throw
}

exit 0
