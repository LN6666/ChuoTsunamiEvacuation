param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

$required = @(
    "Assets\Data\P10\newmap_manual_blocker_audit.json",
    "Assets\Data\P10\newmap_mouse_look_camera_status.json",
    "Assets\Data\P10\newmap_debug_object_cleanup_report.json",
    "Assets\Data\P10\newmap_lighting_visual_status.json",
    "Assets\Data\P10\newmap_material_visual_quality_status.json",
    "Assets\Data\P10\newmap_ground_visual_alignment_status.json",
    "Assets\Data\P10\newmap_building_clipping_status.json",
    "Assets\Data\P10\newmap_post_visual_fix_gameplay_status.json",
    "Assets\Data\P10\newmap_npc_distribution_config.json",
    "Assets\Data\P10\newmap_npc_distribution_report.json",
    "Assets\Data\P10\newmap_visual_fix_player_report.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json",
    "Assets\Data\P10\newmap_manual_playtest_checklist.json"
)

$failures = @()
foreach ($relative in $required) {
    $path = Join-Path $root $relative
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures += "missing $relative"
        continue
    }

    try {
        Get-Content -LiteralPath $path -Raw | ConvertFrom-Json | Out-Null
    }
    catch {
        $failures += "invalid json $relative"
    }
}

$readiness = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_manual_playtest_readiness.json") -Raw | ConvertFrom-Json
$allowed = @("ready_for_manual_playtest", "ready_with_documented_visual_limitations", "needs_quick_fix_before_manual_test", "blocked")
if ($allowed -notcontains [string]$readiness.manualReadinessDecision) {
    $failures += "invalid readiness decision: $($readiness.manualReadinessDecision)"
}

$npcConfig = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_npc_distribution_config.json") -Raw | ConvertFrom-Json
if ([int]$npcConfig.npcCountMultiplier -ne 20) { $failures += "npcCountMultiplier must be 20" }
if ([int]$npcConfig.distributionRadiusMeters -ne 1000) { $failures += "distributionRadiusMeters must be 1000" }
if ([int]$npcConfig.maxNpcCount -le 0) { $failures += "maxNpcCount must be positive" }

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] NewMap manual blocker JSON files validated."
exit 0
