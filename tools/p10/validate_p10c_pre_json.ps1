[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$allowedReadiness = @(
    "pass_for_p10c",
    "pass_with_limitations",
    "needs_quick_fix",
    "needs_major_optimization_before_release",
    "blocked"
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

function Assert-BooleanTrue {
    param([object]$Value, [string]$Context)
    if (-not [bool]$Value) { throw "$Context must be true." }
}

function Assert-BooleanFalse {
    param([object]$Value, [string]$Context)
    if ([bool]$Value) { throw "$Context must be false." }
}

function Assert-Readiness {
    param([string]$Value, [string]$Context)
    if ($allowedReadiness -notcontains $Value) {
        throw "$Context must be one of: $($allowedReadiness -join ', ')"
    }
}

function Test-PerformanceGateConfig {
    $config = Read-JsonFile "Assets/Data/P10/p10c_pre_performance_gate_config.json"
    Assert-BooleanTrue $config.p10cPreIsPerformanceGate "gate p10cPreIsPerformanceGate"
    Assert-BooleanTrue $config.finalReleaseBuildDeferredToP10C "gate finalReleaseBuildDeferredToP10C"
    Assert-BooleanTrue $config.finalReleasePackageDeferredToP10C "gate finalReleasePackageDeferredToP10C"
    Assert-BooleanTrue $config.finalArchiveDeferredToP10C "gate finalArchiveDeferredToP10C"
    Assert-BooleanTrue $config.temporaryWindowsProfilingBuildAllowed "gate temporaryWindowsProfilingBuildAllowed"
    Assert-BooleanTrue $config.buildArtifactsMustStayUncommitted "gate buildArtifactsMustStayUncommitted"
    Assert-BooleanTrue $config.noP10EFG "gate noP10EFG"
    Assert-BooleanTrue $config.noProjectSettingsChangeRequired "gate noProjectSettingsChangeRequired"
    Assert-BooleanTrue $config.noPackagesChangeRequired "gate noPackagesChangeRequired"
    Assert-BooleanTrue $config.noPlateauAssetMutation "gate noPlateauAssetMutation"
    Assert-BooleanTrue $config.noHighDetailSceneMutation "gate noHighDetailSceneMutation"
    Assert-BooleanTrue $config.noAddressablesOrAssetBundlesAdded "gate noAddressablesOrAssetBundlesAdded"
    Assert-BooleanFalse $config.trueChunkStreamingImplemented "gate trueChunkStreamingImplemented"
    if ([double]$config.lowQualityTargetAverageFps -lt 24.0) { throw "Low quality target FPS is too low for the gate." }
    if ([double]$config.lowQualityMinimumOnePercentLowFps -lt 15.0) { throw "Low quality 1 percent low target is too low." }
    if ([double]$config.maxFrameSpikeThresholdMs -lt 16.0) { throw "Frame spike threshold is too low." }
    if ([double]$config.maxShortRunMemoryGrowthMb -le 0.0) { throw "Memory growth threshold must be positive." }
    foreach ($classification in $allowedReadiness) {
        if (@($config.readinessClassifications) -notcontains $classification) {
            throw "Gate readiness classifications must include $classification."
        }
    }
    $repoRootFull = (Resolve-Path -LiteralPath $repoRoot).ProviderPath.TrimEnd("\")
    $configuredOutput = [string]$config.temporaryBuildOutputPath
    if ($configuredOutput.StartsWith($repoRootFull, [System.StringComparison]::OrdinalIgnoreCase) -and
        $configuredOutput -notmatch "\\Builds?\\") {
        throw "Temporary build output must be outside the repo or under an ignored Builds directory."
    }
}

function Test-QualityProfiles {
    $quality = Read-JsonFile "Assets/Data/P10/p10c_pre_quality_profiles.json"
    Assert-BooleanFalse $quality.projectSettingsChangeRequired "quality projectSettingsChangeRequired"
    Assert-BooleanFalse $quality.packageChangeRequired "quality packageChangeRequired"
    Assert-BooleanFalse $quality.renderPipelineChangeRequired "quality renderPipelineChangeRequired"
    $ids = @($quality.profiles | ForEach-Object { $_.profileId })
    foreach ($required in @("Low", "Medium", "High")) {
        if ($ids -notcontains $required) { throw "Quality profiles must include $required." }
    }
    foreach ($profile in @($quality.profiles)) {
        if ([int]$profile.maxNpcCount -gt 250) { throw "$($profile.profileId) maxNpcCount too high." }
        if ([int]$profile.maxMarkerCount -gt 300) { throw "$($profile.profileId) maxMarkerCount too high." }
        if ([int]$profile.maxGreenFrameCount -gt 160) { throw "$($profile.profileId) maxGreenFrameCount too high." }
        if ([int]$profile.metricsSampleCapacity -lt 60) { throw "$($profile.profileId) metricsSampleCapacity too low." }
        if ([double]$profile.lightCurtainIntensityScale -lt 0.0 -or [double]$profile.lightCurtainIntensityScale -gt 1.0) {
            throw "$($profile.profileId) lightCurtainIntensityScale must be 0..1."
        }
        Assert-BooleanFalse $profile.debugLabelsEnabled "$($profile.profileId) debugLabelsEnabled"
        Assert-BooleanFalse $profile.uiDebugOverlaysEnabled "$($profile.profileId) uiDebugOverlaysEnabled"
    }
}

function Test-BuiltPlayerSummary {
    $summary = Read-JsonFile "Assets/Data/P10/p10c_pre_built_player_profile_summary.json"
    Assert-BooleanTrue $summary.p10cPreIsPerformanceGate "summary p10cPreIsPerformanceGate"
    Assert-BooleanFalse $summary.finalReleasePackageCreated "summary finalReleasePackageCreated"
    Assert-BooleanFalse $summary.finalArchiveCreated "summary finalArchiveCreated"
    if (($summary.buildType -as [string]).IndexOf("temporary", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Built-player summary buildType must identify a temporary build."
    }
    Assert-Readiness ([string]$summary.readinessClassification) "summary readinessClassification"
    if ($summary.profileRunSucceeded -and -not [bool]$summary.metrics.measured) {
        throw "Profile summary cannot claim success without measured metrics."
    }
}

function Test-OptimizationDecision {
    $decision = Read-JsonFile "Assets/Data/P10/p10c_pre_optimization_decision.json"
    Assert-Readiness ([string]$decision.readinessDecision) "decision readinessDecision"
    Assert-BooleanTrue $decision.officialP10CReleaseDeferred "decision officialP10CReleaseDeferred"
    Assert-BooleanFalse $decision.finalReleasePackageCreated "decision finalReleasePackageCreated"
    Assert-BooleanFalse $decision.finalArchiveCreated "decision finalArchiveCreated"
    Assert-BooleanFalse $decision.trueChunkStreamingImplemented "decision trueChunkStreamingImplemented"
    Assert-BooleanFalse $decision.addressablesOrAssetBundlesAdded "decision addressablesOrAssetBundlesAdded"
    Assert-BooleanFalse $decision.projectSettingsChanged "decision projectSettingsChanged"
    Assert-BooleanFalse $decision.packagesChanged "decision packagesChanged"
    Assert-BooleanFalse $decision.plateauAssetsChanged "decision plateauAssetsChanged"
    Assert-BooleanFalse $decision.highDetailSceneChanged "decision highDetailSceneChanged"
    if (($decision.streamingDecision -as [string]).IndexOf("No true production chunk streaming", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Streaming decision must honestly state that true production chunk streaming is not implemented."
    }
    if (($decision.antiAliasingDecision -as [string]).IndexOf("not confirmed", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "AA decision must avoid claiming built-player AA is confirmed."
    }
}

function Test-ManualPlaytestChecklist {
    $checklist = Read-JsonFile "Assets/Data/P10/p10c_pre_manual_playtest_checklist.json"
    Assert-BooleanTrue $checklist.temporaryBuildOnly "checklist temporaryBuildOnly"
    Assert-BooleanFalse $checklist.finalReleasePackageCreated "checklist finalReleasePackageCreated"
    if (@($checklist.launchSteps).Count -lt 1) { throw "Manual checklist must include launch steps." }
    if (@($checklist.observations).Count -lt 3) { throw "Manual checklist must include performance observations." }
    if (@($checklist.blockingIssues).Count -lt 1) { throw "Manual checklist must include blocking issues." }
}

Write-Host "P10-C-Pre JSON validation: starting"
Test-PerformanceGateConfig
Test-QualityProfiles
Test-BuiltPlayerSummary
Test-OptimizationDecision
Test-ManualPlaytestChecklist
Write-Host "P10-C-Pre JSON validation: PASS" -ForegroundColor Green
exit 0
