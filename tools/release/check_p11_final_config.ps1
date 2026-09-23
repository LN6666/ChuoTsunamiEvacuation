param(
    [string]$OutputJson = "Assets\Data\P10\p11_final_config_check.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required JSON: $RelativePath"
    }
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Write-Json {
    param($Object, [string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $path) | Out-Null
    $Object | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $path -Encoding UTF8
}

$activeScene = "Assets/Scenes/Chuo_BaseMap.unity"
$stamina = Read-Json "Assets\Data\P10\newmap_player_stamina_config.json"
$boundary = Read-Json "Assets\Data\P10\newmap_circular_boundary_config.json"
$tsunami = Read-Json "Assets\Data\P10\newmap_tsunami_mode_hotfix_config.json"
$nameLabel = Read-Json "Assets\Data\P10\newmap_name_label_config.json"
$enrichment = Read-Json "Assets\Data\P10\newmap_non_official_name_enrichment_config.json"
$candidateRecovery = Read-Json "Assets\Data\P10\newmap_non_official_candidate_recovery.json"
$activeTargetReport = Read-Json "Assets\Data\P10\newmap_active_target_final_report.json"

$maxStamina = [double]$stamina.baselineMaxStamina * [double]$stamina.staminaMultiplier
$sprintSpeed = 5.0 * [double]$stamina.sprintSpeedMultiplierAdditional
$sprintReductionPercent = (1.0 - ($sprintSpeed / 5.7375)) * 100.0

$sourceFiles = @(
    "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs",
    "Assets\Scripts\NewMap\NewMapPlayerController.cs",
    "Assets\Scripts\NewMap\NewMapRuntimeUI.cs",
    "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
)
$sourceText = ""
foreach ($file in $sourceFiles) {
    $sourceText += [Environment]::NewLine + (Get-Content -LiteralPath (Join-Path $ProjectRoot $file) -Raw -Encoding UTF8)
}

$nonOfficial = $candidateRecovery.nonOfficialSemantics
$routeClaimCount = [int]$activeTargetReport.officialRouteClaimCount
$nonOfficialMarkedOfficial = [int]$activeTargetReport.nonOfficialTargetLabeledOfficialCount

$checks = [ordered]@{
    activeSceneExists = Test-Path -LiteralPath (Join-Path $ProjectRoot $activeScene) -PathType Leaf
    activeScene = $activeScene
    maxStamina = $maxStamina
    maxStaminaIs3500 = [math]::Abs($maxStamina - 3500.0) -le 0.001
    sprintSpeedMetersPerSecond = [math]::Round($sprintSpeed, 4)
    sprintReductionPercent = [math]::Round($sprintReductionPercent, 4)
    sprintReductionIs20Percent = [math]::Abs($sprintReductionPercent - 20.0) -le 0.001
    tourismNoStaminaSourcePreserved = $sourceText -match "Tourism" -and $sourceText -match "stamina drain are disabled"
    tsunamiWarningDurationSeconds = [double]$tsunami.tsunamiWarningDurationSeconds
    tsunamiWarningIs180Seconds = [math]::Abs([double]$tsunami.tsunamiWarningDurationSeconds - 180.0) -le 0.001
    circularBoundaryRadiusMeters = [double]$boundary.radiusMeters
    circularBoundaryLatestRadius = [math]::Abs([double]$boundary.radiusMeters - 2270.0) -le 0.001
    circularBoundaryInvisibleNormal = (-not [bool]$boundary.visibleInNormalMode) -and (-not [bool]$boundary.debugVisible)
    runtimeNetworkRequestsAllowed = [bool]$nameLabel.runtimeNetworkRequestsAllowed -or [bool]$enrichment.runtimeNetworkRequestsAllowed
    nonOfficialIsOfficialShelter = [bool]$nonOfficial.isOfficialShelter
    nonOfficialWarningRequired = [bool]$nonOfficial.nonOfficialWarningRequired
    nonOfficialSafeApprovedByDefault = [bool]$nonOfficial.safeApprovedByDefault
    officialRouteClaimCount = $routeClaimCount
    nonOfficialTargetLabeledOfficialCount = $nonOfficialMarkedOfficial
    finalBuildSceneMethodTargetsChuoBaseMap = $sourceText -match "BuildP11FinalPlayerCommandLine" -and $sourceText -match "Chuo_BaseMap.unity"
}

$passed =
    [bool]$checks.activeSceneExists -and
    [bool]$checks.maxStaminaIs3500 -and
    [bool]$checks.sprintReductionIs20Percent -and
    [bool]$checks.tourismNoStaminaSourcePreserved -and
    [bool]$checks.tsunamiWarningIs180Seconds -and
    [bool]$checks.circularBoundaryLatestRadius -and
    [bool]$checks.circularBoundaryInvisibleNormal -and
    (-not [bool]$checks.runtimeNetworkRequestsAllowed) -and
    (-not [bool]$checks.nonOfficialIsOfficialShelter) -and
    [bool]$checks.nonOfficialWarningRequired -and
    (-not [bool]$checks.nonOfficialSafeApprovedByDefault) -and
    $checks.officialRouteClaimCount -eq 0 -and
    $checks.nonOfficialTargetLabeledOfficialCount -eq 0 -and
    [bool]$checks.finalBuildSceneMethodTargetsChuoBaseMap

$result = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    workspace = $ProjectRoot
    activeScene = $activeScene
    checks = $checks
    finalStatus = if ($passed) { "passed" } else { "failed" }
}

Write-Json $result $OutputJson

$doc = @"
# P11 Final Config Check

Generated: $($result.generatedAt)

- Active scene: $activeScene
- Evacuation max stamina: $maxStamina
- Evacuation sprint speed: $([math]::Round($sprintSpeed, 4)) m/s
- Sprint reduction from previous P10 speed: $([math]::Round($sprintReductionPercent, 4))%
- Tourism no-stamina behavior preserved: $($checks.tourismNoStaminaSourcePreserved)
- Tsunami warning duration: $($checks.tsunamiWarningDurationSeconds) seconds
- Circular boundary radius: $($checks.circularBoundaryRadiusMeters) m
- Runtime web requests allowed: $($checks.runtimeNetworkRequestsAllowed)
- Non-official candidates marked official: $($checks.nonOfficialIsOfficialShelter)
- Non-official warning required: $($checks.nonOfficialWarningRequired)
- Official route claim count: $($checks.officialRouteClaimCount)

Result: $($result.finalStatus)
"@
$doc | Set-Content -LiteralPath (Join-Path $ProjectRoot "docs\P11_FINAL_CONFIG_CHECK.md") -Encoding UTF8

if (-not $passed) {
    Write-Host "[FAIL] P11 final config check failed. JSON: $OutputJson"
    exit 1
}

Write-Host "[PASS] P11 final config check passed. JSON: $OutputJson"
exit 0
