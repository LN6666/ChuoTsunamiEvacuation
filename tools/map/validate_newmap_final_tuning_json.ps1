param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing $RelativePath"
        exit 1
    }
    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
}

function Require-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

$required = @(
    "Assets\Data\P10\newmap_final_ground_micro_raise_config.json",
    "Assets\Data\P10\newmap_final_ground_micro_raise_report.json",
    "Assets\Data\P10\newmap_building_collision_final_refinement.json",
    "Assets\Data\P10\newmap_building_collision_final_refinement_report.json",
    "Assets\Data\P10\newmap_spawn_final_safety_config.json",
    "Assets\Data\P10\newmap_spawn_final_safety_report.json",
    "Assets\Data\P10\newmap_stamina_final_tuning_report.json",
    "Assets\Data\P10\newmap_sprint_speed_final_tuning_report.json",
    "Assets\Data\P10\newmap_tsunami_warning_final_tuning_report.json",
    "Assets\Data\P10\newmap_final_tuning_regression_status.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json"
)

foreach ($relative in $required) {
    Require-True (Test-Path -LiteralPath (Join-Path $root $relative) -PathType Leaf) "Missing required final tuning file $relative"
}

$ground = Read-Json "Assets\Data\P10\newmap_final_ground_micro_raise_config.json"
$readiness = Read-Json "Assets\Data\P10\newmap_manual_playtest_readiness.json"
Require-True ([bool]$ground.enabled) "Ground micro-raise config disabled."
Require-True ([double]$ground.additionalGroundRaiseMeters -gt 0 -and [double]$ground.additionalGroundRaiseMeters -le 1.0) "Ground micro-raise must be >0 and <=1.0m."
Require-True ([bool]$ground.applyToGroundCover -and [bool]$ground.applyToSupportColliders) "Ground micro-raise must apply to cover/support."

$allowedReadiness = @("ready_for_manual_playtest", "ready_with_documented_visual_limitations", "needs_quick_fix_before_manual_test", "blocked")
Require-True ($allowedReadiness -contains [string]$readiness.manualReadinessDecision) "Manual readiness decision missing or invalid."
Require-True (-not [bool]$readiness.finalReleaseArchiveCreated) "Manual readiness says final release/archive was created."
Require-True (-not [bool]$readiness.p10EFGCreated) "Manual readiness says P10-E/F/G was created."

Write-Host "[PASS] NewMap final tuning JSON validated."
exit 0
