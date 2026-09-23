[CmdletBinding()]
param(
    [string]$ExePath = "",

    [string]$BuildDirectory = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre",

    [int]$DurationSeconds = 180,

    [string]$ScenarioLabel = "baseline_idle",

    [string]$QualityProfile = "Low",

    [string]$WeatherMode = "clear_day"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$dataDir = Join-Path $repoRoot "Assets\Data\P10"
$summaryPath = Join-Path $dataDir "p10c_mm_fps_stutter_summary.json"

if ([string]::IsNullOrWhiteSpace($ExePath)) {
    $ExePath = Join-Path $BuildDirectory "ChuoTsunamiEvacuation_P10CPre.exe"
}

function Write-Json {
    param([object]$Value, [string]$Path)
    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }
    $Value | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function New-FallbackSummary {
    param([string]$Status, [string]$Reason)
    return [ordered]@{
        schemaVersion = "p10c_mm.fps_stutter_summary.v1"
        status = $Status
        p10cMinusMinusIsExtendedPerformanceGate = $true
        officialP10CReleaseDeferred = $true
        finalReleasePackageCreated = $false
        finalArchiveCreated = $false
        noP10EFG = $true
        scenarioLabel = $ScenarioLabel
        captureStartedAtLocal = (Get-Date).ToString("s")
        captureFinishedAtLocal = ""
        requestedCaptureDurationSeconds = $DurationSeconds
        actualCaptureDurationSeconds = 0
        sampleCapacity = 0
        sampleCount = 0
        averageFps = 0
        minFps = 0
        onePercentLowFps = 0
        minFrameTimeMs = 0
        averageFrameTimeMs = 0
        maxFrameTimeMs = 0
        longestFrameTimeMs = 0
        frameSpikeCountOver33Ms = 0
        frameSpikeCountOver50Ms = 0
        frameSpikeCountOver100Ms = 0
        stutterThresholdMs = 50
        stutterEventCount = 0
        startupTimeSeconds = 0
        sceneLoadingTimeSeconds = 0
        managedHeapMb = 0
        profilerAllocatedMemoryMb = 0
        gcCollectionDelta0 = 0
        gcCollectionDelta1 = 0
        gcCollectionDelta2 = 0
        runtimeState = [ordered]@{
            sceneName = ""
            scenePath = ""
            activeQualityProfile = $QualityProfile
            unityQualityLevelName = ""
            activeWeatherNightMode = $WeatherMode
            nightOverlayActive = $false
            greenFrameActive = $false
            greenFrameCount = 0
            greenFrameRuntimeCount = 0
            candidateMarkerRuntimeActive = $false
            candidateMarkerCount = 0
            lightCurtainActive = $false
            lightCurtainObjectCount = 0
            crowdActive = $false
            crowdAgentCount = 0
            resultPanelActive = $false
            resultPanelObjectCount = 0
            evidence = "Fallback summary written by PowerShell because the built player did not produce exporter JSON."
        }
        outputPath = $summaryPath
        limitations = @($Reason)
        summary = "P10-C-- FPS/stutter capture did not produce built-player exporter data. $Reason"
    }
}

Write-Host "P10-C-- FPS/stutter capture: starting"
Write-Host "EXE: $ExePath"
Write-Host "Duration seconds: $DurationSeconds"
Write-Host "Summary path: $summaryPath"

if (-not (Test-Path -LiteralPath $ExePath -PathType Leaf)) {
    $fallback = New-FallbackSummary -Status "capture_failed" -Reason "Temporary build EXE was not found."
    $fallback.captureFinishedAtLocal = (Get-Date).ToString("s")
    Write-Json -Value $fallback -Path $summaryPath
    throw "Temporary build EXE was not found: $ExePath"
}

New-Item -ItemType Directory -Force -Path $BuildDirectory | Out-Null
$playerLogPath = Join-Path $BuildDirectory "P10CMM_Player_FpsCapture.log"
if (Test-Path -LiteralPath $playerLogPath) {
    Remove-Item -LiteralPath $playerLogPath -Force
}
if (Test-Path -LiteralPath $summaryPath) {
    Remove-Item -LiteralPath $summaryPath -Force
}

$arguments = @(
    "-screen-width", "1920",
    "-screen-height", "1080",
    "-screen-fullscreen", "0",
    "-logFile", $playerLogPath,
    "-p10cMmFpsSummaryPath", $summaryPath,
    "-p10cMmCaptureSeconds", ([Math]::Max(1, $DurationSeconds)).ToString(),
    "-p10cMmScenario", $ScenarioLabel,
    "-p10cMmQualityProfile", $QualityProfile,
    "-p10cMmWeatherMode", $WeatherMode
)

$process = $null
$startedAt = Get-Date
try {
    $process = Start-Process -FilePath $ExePath -ArgumentList $arguments -WindowStyle Normal -PassThru
    $deadline = (Get-Date).AddSeconds([Math]::Max(1, $DurationSeconds) + 15)
    while ((Get-Date) -lt $deadline) {
        if ($process.HasExited) {
            break
        }
        if (Test-Path -LiteralPath $summaryPath -PathType Leaf) {
            break
        }
        Start-Sleep -Seconds 1
    }

    if ($process -and -not $process.HasExited) {
        if (-not $process.CloseMainWindow()) {
            Stop-Process -Id $process.Id -Force
        }
        elseif (-not $process.WaitForExit(10000)) {
            Stop-Process -Id $process.Id -Force
        }
    }

    Start-Sleep -Seconds 1
    if (-not (Test-Path -LiteralPath $summaryPath -PathType Leaf)) {
        $fallback = New-FallbackSummary -Status "capture_failed_no_exporter_summary" -Reason "The temporary player exited or was closed without writing p10c_mm_fps_stutter_summary.json. Rebuild the temporary player after adding the exporter."
        $fallback.captureStartedAtLocal = $startedAt.ToString("s")
        $fallback.captureFinishedAtLocal = (Get-Date).ToString("s")
        $fallback.actualCaptureDurationSeconds = [Math]::Round(((Get-Date) - $startedAt).TotalSeconds, 2)
        Write-Json -Value $fallback -Path $summaryPath
        throw "Built player did not write FPS/stutter summary: $summaryPath"
    }

    & (Join-Path $scriptRoot "parse_p10c_mm_player_log.ps1") -LogPath $playerLogPath
    Write-Host "P10-C-- FPS/stutter capture: PASS" -ForegroundColor Green
}
catch {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
    throw
}

exit 0
