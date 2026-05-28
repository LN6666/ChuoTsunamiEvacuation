param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$required = @(
    "Assets\Data\P10\newmap_mouse_drag_look_config.json",
    "Assets\Data\P10\newmap_mouse_drag_look_status.json",
    "Assets\Data\P10\newmap_ground_height_realignment_round2.json",
    "Assets\Data\P10\newmap_building_material_texture_audit_round2.json",
    "Assets\Data\P10\newmap_lighting_profiles.json",
    "Assets\Data\P10\newmap_night_lighting_correction.json",
    "Assets\Data\P10\newmap_debug_cleanup_round2.json",
    "Assets\Data\P10\newmap_visual_round2_gameplay_regression.json",
    "Assets\Data\P10\newmap_visual_round2_player_report.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json"
)

$missing = @()
foreach ($relative in $required) {
    $path = Join-Path $root $relative
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $missing += $relative
        continue
    }

    Get-Content -LiteralPath $path -Raw | ConvertFrom-Json | Out-Null
}

if ($missing.Count -gt 0) {
    Write-Host "[FAIL] Missing round-2 JSON files: $($missing -join ', ')"
    exit 1
}

Write-Host "[PASS] NewMap visual round-2 JSON files validated."
exit 0
