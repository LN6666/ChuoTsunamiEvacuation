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
        return Get-Content -Raw -Encoding UTF8 -LiteralPath $path | ConvertFrom-Json
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

function Assert-ArrayContains {
    param([object[]]$Values, [string]$Expected, [string]$Context)
    if (@($Values) -notcontains $Expected) { throw "$Context must include '$Expected'." }
}

function Test-OptimizationConfig {
    $config = Read-JsonFile "Assets/Data/P10/p10b_plus_plus_optimization_config.json"
    Assert-BooleanTrue $config.finalWindowsExeBuildDeferredToP10C "optimization finalWindowsExeBuildDeferredToP10C"
    Assert-BooleanTrue $config.noAddressablesOrPackagesAdded "optimization noAddressablesOrPackagesAdded"
    Assert-BooleanFalse $config.projectSettingsChangeRequired "optimization projectSettingsChangeRequired"
    Assert-BooleanFalse $config.mutatesPlateauAssets "optimization mutatesPlateauAssets"
    Assert-BooleanFalse $config.mutatesHighDetailScene "optimization mutatesHighDetailScene"
    Assert-BooleanFalse $config.createsReleaseOrArchiveArtifacts "optimization createsReleaseOrArchiveArtifacts"
    Assert-BooleanFalse $config.debugLayersEnabledByDefault "optimization debugLayersEnabledByDefault"
    if ([int]$config.maxNpcCount -gt 250) { throw "maxNpcCount must stay bounded." }
    if ([int]$config.maxMarkerCount -gt 300) { throw "maxMarkerCount must stay bounded." }
    if ([int]$config.maxGreenFrameCount -gt 160) { throw "maxGreenFrameCount must stay bounded." }
    if ([int]$config.metricsRingBufferCapacity -lt 60) { throw "metricsRingBufferCapacity must be useful." }
    if ([double]$config.frameSpikeThresholdMs -lt 16.0) { throw "frameSpikeThresholdMs is too low." }
}

function Test-PerformanceAuditSample {
    $sample = Read-JsonFile "Assets/Data/P10/p10b_plus_plus_performance_audit_sample.json"
    Assert-BooleanTrue $sample.finalWindowsExeBuildDeferredToP10C "sample finalWindowsExeBuildDeferredToP10C"
    if (($sample.cpuMeasurementPolicy -as [string]) -notlike "*OS-level CPU*") {
        throw "Performance audit sample must document manual OS-level CPU measurement."
    }
    if (($sample.diskPagingMeasurementPolicy -as [string]) -notlike "*Disk paging*") {
        throw "Performance audit sample must document disk paging observation."
    }
}

function Test-QualityRecommendations {
    $quality = Read-JsonFile "Assets/Data/P10/p10b_plus_plus_quality_recommendations.json"
    Assert-BooleanFalse $quality.projectSettingsChangeRequired "quality projectSettingsChangeRequired"
    Assert-BooleanFalse $quality.antiAliasingModeClaimed "quality antiAliasingModeClaimed"
    if (($quality.antiAliasingAuditPolicy -as [string]) -notlike "*Do not claim*") {
        throw "Quality recommendations must avoid false AA claims."
    }
    $ids = @($quality.presets | ForEach-Object { $_.presetId })
    foreach ($required in @("Low", "Medium", "High")) {
        Assert-ArrayContains $ids $required "quality presets"
    }
    foreach ($preset in @($quality.presets)) {
        if ([int]$preset.npcCap -gt 250) { throw "$($preset.presetId) npcCap too high." }
        if ([int]$preset.markerCap -gt 300) { throw "$($preset.presetId) markerCap too high." }
        if ([int]$preset.greenFrameCap -gt 160) { throw "$($preset.presetId) greenFrameCap too high." }
        Assert-BooleanFalse $preset.debugLabelsEnabled "$($preset.presetId) debugLabelsEnabled"
    }
}

function Test-GreenFrameConfigHardening {
    $config = Read-JsonFile "Assets/Data/P10/p10b_green_ground_frame_config.json"
    Assert-BooleanTrue $config.warmupPoolBeforeTsunamiStart "green frame warmupPoolBeforeTsunamiStart"
    if ([int]$config.warmupFrameCount -lt 1) { throw "green frame warmupFrameCount must be positive." }
    if ([int]$config.activationBudgetPerFrame -lt 1) { throw "green frame activationBudgetPerFrame must be positive." }
    Assert-BooleanFalse $config.debugLabelsEnabled "green frame debugLabelsEnabled"
    Assert-BooleanFalse $config.mutatesPlateauAssets "green frame mutatesPlateauAssets"
    Assert-BooleanFalse $config.mutatesHighDetailScene "green frame mutatesHighDetailScene"
}

function Test-PerformanceConfigHardening {
    $config = Read-JsonFile "Assets/Data/P10/p10b_performance_metrics_config.json"
    Assert-BooleanTrue $config.collectOnePercentLowFps "collectOnePercentLowFps"
    if ([double]$config.frameSpikeThresholdMs -lt 16.0) { throw "P10-B frameSpikeThresholdMs is too low." }
}

Write-Host "P10-B++ JSON validation: starting"
Test-OptimizationConfig
Test-PerformanceAuditSample
Test-QualityRecommendations
Test-GreenFrameConfigHardening
Test-PerformanceConfigHardening
Write-Host "P10-B++ JSON validation: PASS" -ForegroundColor Green
exit 0
