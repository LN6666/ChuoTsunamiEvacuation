param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Read-RequiredJson {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing $RelativePath"
        exit 1
    }

    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
}

function Require-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

$boundary = Read-RequiredJson "Assets\Data\P10\newmap_circular_boundary_config.json"
$boundaryReport = Read-RequiredJson "Assets\Data\P10\newmap_circular_boundary_report.json"
$boundaryUpdate = Read-RequiredJson "Assets\Data\P10\newmap_boundary_2_27km_update_report.json"
$playableBounds = Read-RequiredJson "Assets\Data\P10\newmap_playable_bounds_config.json"
$activeTargetReport = Read-RequiredJson "Assets\Data\P10\newmap_active_target_final_report.json"
$leaderboard = Read-RequiredJson "Assets\Data\P10\newmap_leaderboard_toggle_config.json"
$cleanup = Read-RequiredJson "Assets\Data\P10\newmap_airwall_hard_cleanup_config.json"
$whitelistReport = Read-RequiredJson "Assets\Data\P10\newmap_collision_whitelist_report.json"
$airwallCleanup = Read-RequiredJson "Assets\Data\P10\newmap_unexpected_airwall_final_cleanup.json"
$manual = Read-RequiredJson "Assets\Data\P10\newmap_manual_playtest_readiness.json"
$tsunami = Read-RequiredJson "Assets\Data\P10\newmap_tsunami_mode_hotfix_config.json"
$stamina = Read-RequiredJson "Assets\Data\P10\newmap_player_stamina_config.json"

$requiredReports = @(
    "Assets\Data\P10\newmap_collision_whitelist_audit.json",
    "Assets\Data\P10\newmap_collision_whitelist_report.json",
    "Assets\Data\P10\newmap_circular_boundary_report.json",
    "Assets\Data\P10\newmap_boundary_2_27km_update_report.json",
    "Assets\Data\P10\newmap_building_collision_after_whitelist.json",
    "Assets\Data\P10\newmap_npc_collision_after_whitelist.json",
    "Assets\Data\P10\newmap_ground_after_collision_whitelist.json",
    "Assets\Data\P10\newmap_unexpected_airwall_final_cleanup.json",
    "Assets\Data\P10\newmap_leaderboard_toggle_report.json",
    "Assets\Data\P10\newmap_collision_boundary_r_toggle_regression.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json"
)

foreach ($report in $requiredReports) {
    Require-Condition (Test-Path -LiteralPath (Join-Path $root $report) -PathType Leaf) "Missing report $report"
}

Require-Condition ([bool]$boundary.enabled) "Circular boundary disabled."
Require-Condition ($boundary.PSObject.Properties.Name -contains "radiusMeters") "Circular boundary radius missing."
Require-Condition ([double]$boundary.radiusMeters -eq 2270.0) "Circular boundary radius must be exactly 2270m."
Require-Condition ([double]$boundaryReport.radiusMeters -eq 2270.0) "Circular boundary report must reflect 2270m."
Require-Condition ([double]$boundaryUpdate.radiusMeters -eq 2270.0) "2.27km boundary update report must reflect 2270m."
Require-Condition ([double]$playableBounds.radiusMeters -eq 2270.0) "Playable bounds config must reflect 2270m."
Require-Condition ([string]$playableBounds.activeTargetBoundsPolicy -eq "disable_targets_outside_2_27km_circle") "Playable bounds active-target policy is stale."
Require-Condition ([double]$activeTargetReport.playableBoundaryRadiusMeters -eq 2270.0) "Active target report does not reflect 2270m."
Require-Condition ([bool]$activeTargetReport.activeTargetsOutside2_27kmDisabledAtRuntime) "Active target report does not confirm 2.27km out-of-bounds disabling."
Require-Condition (($activeTargetReport.PSObject.Properties.Name -notcontains "activeTargetsOutside1_5kmDisabledAtRuntime") -and ($activeTargetReport.PSObject.Properties.Name -notcontains "runtimeNonOfficialCandidateCountInside1_5km")) "Active target report still contains 1.5km boundary fields."
Require-Condition ([string]$boundary.boundaryMode -eq "runtime_circular_clamp") "Boundary mode must be runtime_circular_clamp."
Require-Condition ([bool]$boundary.affectsPlayer) "Circular boundary must affect player."
Require-Condition ([bool]$boundary.affectsNpc) "Circular boundary must affect NPCs."
Require-Condition (-not [bool]$boundary.visibleInNormalMode) "Circular boundary must be invisible in normal mode."
Require-Condition (-not [bool]$boundary.debugVisible) "Circular debug visualization must be off by default."

Require-Condition ([bool]$leaderboard.enabled) "Leaderboard toggle disabled."
Require-Condition ([string]$leaderboard.toggleKey -eq "R") "Leaderboard toggle key must be R."
Require-Condition ([bool]$leaderboard.hideWhenMenuOpen) "R toggle must be gated while menu is open."
Require-Condition ([bool]$leaderboard.preserveWarnings) "Official/non-official warning preservation disabled."

Require-Condition ([bool]$cleanup.enabled) "Airwall cleanup disabled."
Require-Condition (-not [bool]$cleanup.keepBoundaryAirWalls) "Old rectangular boundary air walls must not be preserved."
Require-Condition (-not [bool]$cleanup.keepInvalidZoneBlockers) "Invalid-zone blockers must not be preserved inside normal play."
Require-Condition ([string]$cleanup.strategy -eq "collision_whitelist_disable_old_airwalls_use_circular_boundary_clamp") "Cleanup strategy is stale."

Require-Condition (-not [bool]$whitelistReport.oldRectangularAirWallsAllowed) "Whitelist report still allows old rectangular air walls."
Require-Condition (-not [bool]$whitelistReport.routeLineVisualBlocksPlayer) "Route lines must not block player."
Require-Condition (-not [bool]$whitelistReport.greenFrameVisualBlocksPlayer) "Green frames must not block player."
Require-Condition (-not [bool]$whitelistReport.labelVisualBlocksPlayer) "Labels must not block player."
Require-Condition (-not [bool]$airwallCleanup.invalidZoneBlockersPreserved) "Invalid-zone blockers are still preserved."

Require-Condition ([double]$tsunami.tsunamiWarningDurationSeconds -eq 180.0) "Tsunami warning duration must remain 180s."
Require-Condition ([double]$tsunami.preWarningRandomMaxSeconds -eq 180.0) "Pre-warning random max must remain 180s."
Require-Condition ([bool]$tsunami.enableShelterDirectLines) "Shelter direct lines must remain enabled."
Require-Condition ([double]$stamina.baselineMaxStamina -eq 100.0) "Baseline stamina changed unexpectedly."
Require-Condition ([double]$stamina.staminaMultiplier -eq 130.0) "Final stamina multiplier must remain 130."
Require-Condition ([double]$stamina.sprintSpeedMultiplierAdditional -eq 1.1475) "Sprint additional multiplier must remain 1.1475."

$allowedReadiness = @(
    "ready_for_manual_playtest",
    "ready_with_documented_boundary_limitations",
    "ready_with_documented_visual_limitations",
    "needs_quick_fix_before_manual_test",
    "blocked"
)
Require-Condition ($allowedReadiness -contains [string]$manual.manualReadinessDecision) "Manual readiness decision missing or invalid."

Write-Host "[PASS] NewMap collision/boundary/leaderboard JSON validated."
exit 0
