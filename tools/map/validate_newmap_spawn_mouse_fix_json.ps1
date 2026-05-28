param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$required = @(
    "Assets\Data\P10\newmap_mouse_drag_look_config.json",
    "Assets\Data\P10\newmap_mouse_drag_look_status.json",
    "Assets\Data\P10\newmap_spawn_config.json",
    "Assets\Data\P10\newmap_safe_spawn_points.json",
    "Assets\Data\P10\newmap_spawn_validation_report.json",
    "Assets\Data\P10\newmap_building_bounds_cache_status.json",
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
    Write-Host "[FAIL] Missing spawn/mouse-fix JSON files: $($missing -join ', ')"
    exit 1
}

$mouse = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_mouse_drag_look_config.json") -Raw | ConvertFrom-Json
$allowed = @($mouse.allowedButtons)
if (-not [bool]$mouse.enabled -or -not [bool]$mouse.lookRequiresMouseButton -or
    ($allowed -notcontains "LeftMouse") -or ($allowed -notcontains "RightMouse")) {
    Write-Host "[FAIL] Mouse drag-look JSON must enable button-gated LeftMouse and RightMouse."
    exit 1
}

$spawn = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_spawn_config.json") -Raw | ConvertFrom-Json
if ($spawn.spawnMode -ne "road_or_playable_ground_only" -or
    -not [bool]$spawn.useBuildingBoundsRejection -or
    -not [bool]$spawn.useGroundProbe -or
    [double]$spawn.minDistanceFromBuildingMeters -lt 1.0 -or
    [int]$spawn.maxSpawnAttempts -lt 50) {
    Write-Host "[FAIL] Spawn config does not enforce playable-ground/building-overlap validation."
    exit 1
}

$safe = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_safe_spawn_points.json") -Raw | ConvertFrom-Json
if (@($safe.records).Count -lt 3) {
    Write-Host "[FAIL] At least three safe spawn points are required."
    exit 1
}

Write-Host "[PASS] NewMap spawn/mouse-fix JSON files validated."
exit 0
