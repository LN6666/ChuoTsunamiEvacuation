param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$script:Failures = @()

function Add-Failure {
    param([string]$Message)
    $script:Failures += $Message
}

function Read-RequiredJson {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "Missing JSON: $RelativePath"
        return $null
    }

    try {
        return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
    }
    catch {
        Add-Failure "Invalid JSON: $RelativePath ($($_.Exception.Message))"
        return $null
    }
}

function Require-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Add-Failure $Message
    }
}

$required = @(
    "Assets\Data\P10\newmap_groundroad_merge_failure_analysis.json",
    "Assets\Data\P10\newmap_safe_ground_rollback_status.json",
    "Assets\Data\P10\newmap_safe_ground_config.json",
    "Assets\Data\P10\newmap_safe_ground_report.json",
    "Assets\Data\P10\newmap_blue_area_hard_removal.json",
    "Assets\Data\P10\newmap_fall_out_prevention_report.json",
    "Assets\Data\P10\newmap_building_floating_after_rollback.json",
    "Assets\Data\P10\newmap_ground_rollback_regression_status.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json",
    "Assets\Data\P10\newmap_adaptive_support_grid_config.json"
)

foreach ($relative in $required) {
    [void](Read-RequiredJson $relative)
}

$failure = Read-RequiredJson "Assets\Data\P10\newmap_groundroad_merge_failure_analysis.json"
if ($failure) {
    Require-Condition ([bool]$failure.manualFailureAcknowledged) "GroundRoadMergePre failure is not acknowledged."
    Require-Condition ([int]$failure.sourceRoadLikeRendererCount -eq 0) "Failure analysis must record zero road-like source renderers."
    Require-Condition (-not [bool]$failure.disabledOrRolledBack.adaptiveSupportGridDefaultEnabled) "Failure analysis says adaptive grid remains enabled."
}

$adaptiveConfig = Read-RequiredJson "Assets\Data\P10\newmap_adaptive_support_grid_config.json"
if ($adaptiveConfig) {
    Require-Condition (-not [bool]$adaptiveConfig.enabled) "Adaptive support grid config must be disabled for rollback."
    Require-Condition (-not [bool]$adaptiveConfig.rendererEnabledInNormalMode) "Adaptive support renderers must not be enabled."
}

$safeConfig = Read-RequiredJson "Assets\Data\P10\newmap_safe_ground_config.json"
if ($safeConfig) {
    Require-Condition ([bool]$safeConfig.enabled) "Safe ground config is disabled."
    Require-Condition ([bool]$safeConfig.forceFixedSupportY) "Safe ground must force a conservative fixed support Y."
    Require-Condition (-not [bool]$safeConfig.rendererEnabledInNormalMode) "Safe ground renderer must be hidden in normal mode."
    Require-Condition ([bool]$safeConfig.fallRecoveryEnabled) "Fall recovery must be enabled."
}

$safeReport = Read-RequiredJson "Assets\Data\P10\newmap_safe_ground_report.json"
if ($safeReport) {
    Require-Condition ([int]$safeReport.supportColliderCount -ge 1) "Safe ground report has no support collider."
    Require-Condition ([bool]$safeReport.supportRendererHidden) "Safe ground support renderer is not reported hidden."
    Require-Condition ([bool]$safeReport.adaptiveSupportGridDisabled) "Safe ground report does not disable adaptive grid."
}

$blue = Read-RequiredJson "Assets\Data\P10\newmap_blue_area_hard_removal.json"
if ($blue) {
    Require-Condition ([int]$blue.normalModeVisibleBlueSupportCount -eq 0) "Visible blue support count must be zero."
    Require-Condition ([int]$blue.largeVisibleBluePlaneCount -eq 0) "Large visible blue plane count must be zero."
    Require-Condition (-not [bool]$blue.supportGridRendererActive) "Support grid renderer is active."
}

$fall = Read-RequiredJson "Assets\Data\P10\newmap_fall_out_prevention_report.json"
if ($fall) {
    Require-Condition ([bool]$fall.fallRecoveryEnabled) "Fall recovery is not enabled."
    Require-Condition ([bool]$fall.playerCannotFallOutOfMap) "Fall-out prevention is not confirmed."
    Require-Condition (-not [bool]$fall.playerLogWarningSpamExpected) "Fall recovery should not spam Player.log warnings."
}

$readiness = Read-RequiredJson "Assets\Data\P10\newmap_manual_playtest_readiness.json"
if ($readiness) {
    Require-Condition (@("ready_for_manual_playtest", "ready_with_documented_ground_limitations", "needs_quick_fix_before_manual_test", "blocked") -contains [string]$readiness.manualReadinessDecision) "Manual readiness decision is invalid."
    Require-Condition ([bool]$readiness.adaptiveGroundRoadMergeDisabled) "Manual readiness must confirm adaptive merge disabled."
    Require-Condition ([bool]$readiness.fallOutPreventionEnabled) "Manual readiness must confirm fall-out prevention."
    Require-Condition (-not [bool]$readiness.roadTerrainAccuracyClaimed) "Readiness must not claim road/terrain accuracy."
}

if ($script:Failures.Count -gt 0) {
    foreach ($failureMessage in $script:Failures) {
        Write-Host "[FAIL] $failureMessage"
    }
    exit 1
}

Write-Host "[PASS] NewMap ground rollback JSON files validated."
exit 0
