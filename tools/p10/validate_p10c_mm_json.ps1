[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$allowedReadiness = @(
    "ready_for_p10c",
    "ready_with_limitations",
    "needs_quick_fix_before_p10c",
    "blocked_until_major_optimization"
)

function Read-JsonFile {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing JSON file: $RelativePath"
    }
    try {
        return Get-Content -Raw -Encoding UTF8 -LiteralPath $path | ConvertFrom-Json
    }
    catch {
        throw "Invalid JSON in $RelativePath. $($_.Exception.Message)"
    }
}

function Assert-False {
    param([object]$Value, [string]$Context)
    if ([bool]$Value) { throw "$Context must be false." }
}

function Assert-True {
    param([object]$Value, [string]$Context)
    if (-not [bool]$Value) { throw "$Context must be true." }
}

function Assert-Readiness {
    param([string]$Value, [string]$Context)
    if ($allowedReadiness -notcontains $Value) {
        throw "$Context must be one of: $($allowedReadiness -join ', ')"
    }
}

function Test-ProcessSummary {
    param([string]$RelativePath, [int]$ExpectedDuration)
    $summary = Read-JsonFile $RelativePath
    Assert-True $summary.p10cMinusMinusIsExtendedPerformanceGate "$RelativePath gate flag"
    Assert-False $summary.finalReleasePackageCreated "$RelativePath finalReleasePackageCreated"
    Assert-False $summary.finalArchiveCreated "$RelativePath finalArchiveCreated"
    Assert-True $summary.noP10EFG "$RelativePath noP10EFG"
    if ([int]$summary.requestedDurationSeconds -ne $ExpectedDuration) {
        throw "$RelativePath requestedDurationSeconds must be $ExpectedDuration."
    }
    if ([string]$summary.status -eq "completed") {
        if ([int]$summary.sampleCount -le 0) { throw "$RelativePath completed without samples." }
        if ([double]$summary.maxPrivateMemoryMb -le 0) { throw "$RelativePath maxPrivateMemoryMb must be positive when completed." }
    }
}

function Test-FpsSummary {
    $summary = Read-JsonFile "Assets/Data/P10/p10c_mm_fps_stutter_summary.json"
    Assert-True $summary.p10cMinusMinusIsExtendedPerformanceGate "FPS gate flag"
    Assert-False $summary.finalReleasePackageCreated "FPS finalReleasePackageCreated"
    Assert-False $summary.finalArchiveCreated "FPS finalArchiveCreated"
    Assert-True $summary.noP10EFG "FPS noP10EFG"
    if ([string]$summary.status -in @("captured", "duration_reached", "application_quit", "test_finished")) {
        if ([int]$summary.sampleCount -le 0) { throw "FPS summary captured without samples." }
        if ([double]$summary.averageFrameTimeMs -le 0) { throw "FPS summary averageFrameTimeMs must be positive." }
    }
}

function Test-HighDetailStatus {
    $status = Read-JsonFile "Assets/Data/P10/p10c_mm_high_detail_full_load_status.json"
    Assert-True $status.p10cMinusMinusIsExtendedPerformanceGate "high-detail gate flag"
    Assert-False $status.highDetailSceneMutated "high-detail scene mutation flag"
    Assert-False $status.finalReleasePackageCreated "high-detail finalReleasePackageCreated"
    if (-not [string]$status.scenePath) { throw "high-detail status must include scenePath." }
}

function Test-ScenarioSummary {
    $summary = Read-JsonFile "Assets/Data/P10/p10c_mm_scenario_performance_summary.json"
    Assert-True $summary.p10cMinusMinusIsExtendedPerformanceGate "scenario gate flag"
    Assert-False $summary.finalReleasePackageCreated "scenario finalReleasePackageCreated"
    foreach ($required in @("baseline_idle", "tsunami_start", "green_frames_markers", "light_curtain", "crowd_congestion", "result_panel_ui", "night_rain")) {
        $match = @($summary.scenarios | Where-Object { $_.scenarioId -eq $required })
        if ($match.Count -eq 0) { throw "scenario summary must include $required." }
    }
}

function Test-LogSummary {
    $summary = Read-JsonFile "Assets/Data/P10/p10c_mm_player_log_summary.json"
    Assert-True $summary.p10cMinusMinusIsExtendedPerformanceGate "log gate flag"
    Assert-False $summary.finalReleasePackageCreated "log finalReleasePackageCreated"
    if ($null -eq $summary.warningsCount -or $null -eq $summary.errorsCount) {
        throw "Player.log summary must include warning/error counts."
    }
}

function Test-PagingSummary {
    $summary = Read-JsonFile "Assets/Data/P10/p10c_mm_paging_risk_summary.json"
    Assert-True $summary.p10cMinusMinusIsExtendedPerformanceGate "paging gate flag"
    Assert-False $summary.finalReleasePackageCreated "paging finalReleasePackageCreated"
    if (-not [string]$summary.workingSetTrend) { throw "paging summary must include workingSetTrend." }
    if (-not [string]$summary.privateMemoryTrend) { throw "paging summary must include privateMemoryTrend." }
}

function Test-Readiness {
    $decision = Read-JsonFile "Assets/Data/P10/p10c_mm_readiness_decision.json"
    Assert-True $decision.p10cMinusMinusIsExtendedPerformanceGate "readiness gate flag"
    Assert-False $decision.finalReleasePackageCreated "readiness finalReleasePackageCreated"
    Assert-False $decision.finalArchiveCreated "readiness finalArchiveCreated"
    Assert-True $decision.officialP10CReleaseDeferred "readiness officialP10CReleaseDeferred"
    Assert-True $decision.noP10EFG "readiness noP10EFG"
    Assert-Readiness ([string]$decision.readinessDecision) "readinessDecision"
}

Write-Host "P10-C-- JSON validation: starting"
Test-ProcessSummary "Assets/Data/P10/p10c_mm_process_sampling_3min.json" 180
Test-ProcessSummary "Assets/Data/P10/p10c_mm_process_sampling_5min.json" 300
Test-ProcessSummary "Assets/Data/P10/p10c_mm_process_sampling_10min.json" 600
Test-FpsSummary
Test-HighDetailStatus
Test-ScenarioSummary
Test-LogSummary
Test-PagingSummary
Test-Readiness
Write-Host "P10-C-- JSON validation: PASS" -ForegroundColor Green
exit 0
