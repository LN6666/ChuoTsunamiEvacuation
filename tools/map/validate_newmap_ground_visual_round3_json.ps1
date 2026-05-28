param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$required = @(
    "Assets\Data\P10\newmap_blue_ground_diagnosis.json",
    "Assets\Data\P10\newmap_support_surface_visibility_status.json",
    "Assets\Data\P10\newmap_ground_visual_alignment_round3.json",
    "Assets\Data\P10\newmap_road_visual_sanity_round3.json",
    "Assets\Data\P10\newmap_building_floating_round3.json",
    "Assets\Data\P10\newmap_playable_bounds_config.json",
    "Assets\Data\P10\newmap_playable_bounds_report.json",
    "Assets\Data\P10\newmap_spawn_config.json",
    "Assets\Data\P10\newmap_mouse_drag_look_config.json",
    "Assets\Data\P10\newmap_object_name_label_source_report.json",
    "Assets\Data\P10\newmap_name_label_config.json",
    "Assets\Data\P10\newmap_name_label_cache.json",
    "Assets\Data\P10\newmap_name_cache.json",
    "Assets\Data\P10\newmap_name_label_runtime_report.json",
    "Assets\Data\P10\newmap_name_enrichment_config.json",
    "Assets\Data\P10\newmap_name_enrichment_report.json",
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
    Write-Host "[FAIL] Missing Round 3 JSON files: $($missing -join ', ')"
    exit 1
}

$bounds = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_playable_bounds_config.json") -Raw | ConvertFrom-Json
if (-not [bool]$bounds.enabled -or [bool]$bounds.debugVisualizationEnabled -or [double]$bounds.boundaryHeightMeters -lt 10 -or [double]$bounds.boundaryThicknessMeters -le 0) {
    Write-Host "[FAIL] Playable bounds config must enable invisible non-debug BoxCollider air walls."
    exit 1
}

$spawn = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_spawn_config.json") -Raw | ConvertFrom-Json
if ($spawn.spawnMode -ne "road_or_playable_ground_only" -or -not [bool]$spawn.useBuildingBoundsRejection -or -not [bool]$spawn.useGroundProbe -or [double]$spawn.minDistanceFromAirWallMeters -lt 1.0) {
    Write-Host "[FAIL] Spawn config must use playable-ground, building rejection, ground probe, and air-wall inset validation."
    exit 1
}

$mouse = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_mouse_drag_look_config.json") -Raw | ConvertFrom-Json
$buttons = @($mouse.allowedButtons)
if (-not [bool]$mouse.lookRequiresMouseButton -or ($buttons -notcontains "LeftMouse") -or ($buttons -notcontains "RightMouse")) {
    Write-Host "[FAIL] Mouse drag-look config must keep both LeftMouse and RightMouse."
    exit 1
}

$labelConfig = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_name_label_config.json") -Raw | ConvertFrom-Json
if (-not [bool]$labelConfig.enabled -or [bool]$labelConfig.runtimeNetworkRequestsAllowed -or [int]$labelConfig.maxVisibleLabels -gt 300 -or [double]$labelConfig.labelUpdateIntervalSeconds -lt 0.05) {
    Write-Host "[FAIL] Name label config must be offline, capped, and throttled."
    exit 1
}

$enrichment = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_name_enrichment_config.json") -Raw | ConvertFrom-Json
if ([bool]$enrichment.runtimeNetworkRequestsAllowed -or [double]$enrichment.rateLimitSeconds -lt 1.1 -or [int]$enrichment.maxQueriesPerRun -gt 200) {
    Write-Host "[FAIL] Name enrichment config must keep runtime offline, rate-limited, and capped."
    exit 1
}

$cache = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_name_cache.json") -Raw | ConvertFrom-Json
if ([bool]$cache.runtimeNetworkRequestsAllowed) {
    Write-Host "[FAIL] Runtime name cache may not allow network requests."
    exit 1
}

$support = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_support_surface_visibility_status.json") -Raw | ConvertFrom-Json
if ([bool]$support.supportRendererAllowedInNormalMode -or [bool]$support.blueDebugGroundMaterialAllowedInNormalMode) {
    Write-Host "[FAIL] Support/blue debug ground renderers must be disallowed in normal mode."
    exit 1
}

Write-Host "[PASS] NewMap Ground Visual Round 3 JSON files validated."
exit 0
