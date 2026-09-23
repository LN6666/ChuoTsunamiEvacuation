[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

function Read-JsonFile {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing JSON file: $RelativePath"
    }
    try {
        return Get-Content -Raw -LiteralPath $path | ConvertFrom-Json
    }
    catch {
        throw "Invalid JSON in $RelativePath. $($_.Exception.Message)"
    }
}

function Assert-BooleanTrue {
    param([object]$Value, [string]$Context)
    if (-not [bool]$Value) { throw "$Context must be true." }
}

function Assert-BooleanFalse {
    param([object]$Value, [string]$Context)
    if ([bool]$Value) { throw "$Context must be false." }
}

function Assert-RequiredText {
    param([string]$Value, [string]$Context)
    if ([string]::IsNullOrWhiteSpace($Value)) { throw "$Context is required." }
}

function Assert-ArrayContains {
    param([object[]]$Values, [string]$Expected, [string]$Context)
    if (@($Values) -notcontains $Expected) { throw "$Context must include '$Expected'." }
}

function Test-GreenGroundFrameConfig {
    $config = Read-JsonFile "Assets/Data/P10/p10b_green_ground_frame_config.json"
    Assert-BooleanTrue $config.featureEnabled "green frame featureEnabled"
    Assert-BooleanTrue $config.tsunamiStartTriggered "green frame tsunamiStartTriggered"
    Assert-BooleanTrue $config.runtimeGenerated "green frame runtimeGenerated"
    Assert-BooleanFalse $config.debugPreviewBeforeTsunamiStart "debugPreviewBeforeTsunamiStart"
    Assert-BooleanFalse $config.mutatesPlateauAssets "mutatesPlateauAssets"
    Assert-BooleanFalse $config.mutatesHighDetailScene "mutatesHighDetailScene"
    Assert-BooleanFalse $config.claimsExactBuildingFootprint "claimsExactBuildingFootprint"
    Assert-BooleanFalse $config.claimsOfficialApprovalForHumanitarianCandidates "claimsOfficialApprovalForHumanitarianCandidates"
    if ([int]$config.maxFrameCount -lt 111) {
        throw "green frame maxFrameCount should cover current official sample plus 110 humanitarian candidates."
    }
    if (($config.frameMeaning -as [string]) -notlike "*evacuation_related*") {
        throw "green frame meaning must be evacuation-related, not official approval."
    }
    if (($config.humanitarianWarningText -as [string]) -notlike "*Not an official evacuation shelter*") {
        throw "humanitarian warning text must preserve non-official warning."
    }
}

function Test-GreenGroundFrameTargets {
    $targets = Read-JsonFile "Assets/Data/P10/p10b_green_ground_frame_targets_sample.json"
    Assert-BooleanFalse $targets.exactFootprintsProven "green frame target exactFootprintsProven"
    Assert-BooleanFalse $targets.officialRouteClaimed "green frame target officialRouteClaimed"
    Assert-BooleanTrue $targets.humanitarianWarningsPreserved "humanitarianWarningsPreserved"
    $records = @($targets.targets)
    if ($records.Count -lt 2) {
        throw "green frame target sample must include official and non-official examples."
    }
    $humanitarian = @($records | Where-Object { [bool]$_.isHumanitarianCandidate })
    if ($humanitarian.Count -lt 1) {
        throw "green frame target sample must include a humanitarian candidate."
    }
    foreach ($record in $humanitarian) {
        Assert-BooleanFalse $record.isOfficialShelter "humanitarian $($record.targetId) isOfficialShelter"
        Assert-BooleanTrue $record.nonOfficialWarningRequired "humanitarian $($record.targetId) nonOfficialWarningRequired"
        Assert-BooleanFalse $record.safeApprovedByDefault "humanitarian $($record.targetId) safeApprovedByDefault"
        Assert-BooleanFalse $record.exactFootprintProven "humanitarian $($record.targetId) exactFootprintProven"
        if (($record.warningText -as [string]) -notlike "*Not an official evacuation shelter*") {
            throw "humanitarian $($record.targetId) warning text must preserve non-official wording."
        }
    }
}

function Test-PerformanceConfig {
    $config = Read-JsonFile "Assets/Data/P10/p10b_performance_metrics_config.json"
    Assert-BooleanTrue $config.finalWindowsExeBuildDeferredToP10C "finalWindowsExeBuildDeferredToP10C"
    foreach ($flag in @(
        "collectAverageFps",
        "collectOnePercentLowFps",
        "collectFrameTimeMinAvgMax",
        "collectMemoryUsage",
        "collectGcProxy",
        "collectLoadingTime",
        "collectPlayerLogWarningsErrors",
        "collectNpcMarkerAndGreenFrameCounts",
        "collectLightCurtainImpact",
        "collectResultPanelImpact"
    )) {
        Assert-BooleanTrue $config.$flag $flag
    }
}

function Test-StressScenarios {
    $collection = Read-JsonFile "Assets/Data/P10/p10b_stress_scenarios.json"
    Assert-BooleanTrue $collection.finalWindowsExeBuildDeferredToP10C "stress finalWindowsExeBuildDeferredToP10C"
    Assert-BooleanTrue $collection.noThousandsOfNpcs "stress noThousandsOfNpcs"
    $ids = @($collection.scenarios | ForEach-Object { $_.scenarioId })
    foreach ($required in @(
        "baseline_low_marker_count",
        "all_candidates_green_frames_after_tsunami_start",
        "crowd_congestion_high_bounded",
        "light_curtain_green_frames_combined",
        "result_panel_long_warning",
        "collapse_debris_enabled",
        "debug_labels_disabled_performance"
    )) {
        Assert-ArrayContains $ids $required "stress scenarios"
    }
    foreach ($scenario in @($collection.scenarios)) {
        if ([int]$scenario.npcCap -gt 250) { throw "$($scenario.scenarioId) npcCap too high." }
        if ([int]$scenario.greenFrameCap -gt 160) { throw "$($scenario.scenarioId) greenFrameCap too high." }
    }
}

function Test-QualityPresets {
    $collection = Read-JsonFile "Assets/Data/P10/p10b_quality_presets.json"
    Assert-BooleanFalse $collection.projectSettingsChangeRequired "quality projectSettingsChangeRequired"
    $ids = @($collection.presets | ForEach-Object { $_.presetId })
    foreach ($required in @("Low", "Medium", "High")) {
        Assert-ArrayContains $ids $required "quality presets"
    }
}

function Test-ProfilingReportAndManualChecklist {
    $report = Read-JsonFile "Assets/Data/P10/p10b_profiling_report_sample.json"
    Assert-BooleanTrue $report.finalWindowsExeBuildDeferredToP10C "profiling finalWindowsExeBuildDeferredToP10C"
    Assert-BooleanTrue $report.highDetailManualPlaytestRequired "highDetailManualPlaytestRequired"
    Assert-BooleanTrue $report.beforeAfterOptimizationMetricsRequired "beforeAfterOptimizationMetricsRequired"
    foreach ($metric in @(
        "average_FPS",
        "1_percent_low_FPS_or_stutter_proxy",
        "green_ground_frame_count",
        "Player_log_warnings_errors"
    )) {
        Assert-ArrayContains @($report.metricsToCollect) $metric "profiling metrics"
    }

    $checklist = Read-JsonFile "Assets/Data/P10/p10b_manual_playtest_checklist.json"
    Assert-BooleanTrue $checklist.finalWindowsExeBuildDeferredToP10C "manual checklist finalWindowsExeBuildDeferredToP10C"
    Assert-BooleanTrue $checklist.highDetailSceneMustNotBeSavedDuringSmoke "highDetailSceneMustNotBeSavedDuringSmoke"
    Assert-BooleanTrue $checklist.userManualPlaytestPrepared "userManualPlaytestPrepared"
    if (@($checklist.checklist).Count -lt 10) {
        throw "manual playtest checklist should contain detailed smoke steps."
    }
}

Write-Host "P10-B JSON validation: starting"
Test-GreenGroundFrameConfig
Test-GreenGroundFrameTargets
Test-PerformanceConfig
Test-StressScenarios
Test-QualityPresets
Test-ProfilingReportAndManualChecklist
Write-Host "P10-B JSON validation: PASS" -ForegroundColor Green
exit 0
