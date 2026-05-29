param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Require-Text {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -notmatch $Pattern) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$directLinePath = Join-Path $root "Assets\Scripts\NewMap\NewMapShelterDirectLineController.cs"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_collision_whitelist_report.json"
$cleanupPath = Join-Path $root "Assets\Data\P10\newmap_airwall_hard_cleanup_config.json"

$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$directLine = Get-Content -Encoding UTF8 -LiteralPath $directLinePath -Raw
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json
$cleanup = Get-Content -Encoding UTF8 -LiteralPath $cleanupPath -Raw | ConvertFrom-Json

Require-Text $bootstrap "IsAllowedPlayerBlockingCategory" "Whitelist function missing."
Require-Text $bootstrap "ground_support" "Ground support category missing."
Require-Text $bootstrap "building_obstacle" "Building obstacle category missing."
Require-Text $bootstrap "npc_body" "NPC body category missing."
Require-Text $bootstrap "map_boundary" "Map boundary category missing."
Require-Text $bootstrap "old_air_wall" "Old air-wall category missing."
Require-Text $bootstrap "route_line_visual" "Route-line visual category missing."
Require-Text $bootstrap "green_frame_visual" "Green-frame visual category missing."
Require-Text $bootstrap "hazard_visual" "Hazard visual category missing."
Require-Text $bootstrap "LastPlayableAirWallColliderCount = 0" "Runtime must not create old air-wall colliders."
Require-Text $bootstrap "EnsureCircularBoundaryDiagnostics" "Circular boundary diagnostic method missing."
Require-Text $bootstrap "LastRouteVisualBlockingColliderCount" "Route visual blocker diagnostics missing."
Require-Text $directLine "CountLineCollidersForDiagnostics" "Direct-line collider diagnostic missing."
Require-Text $directLine "LineRenderer" "Direct-line visual should use LineRenderer."

if ([bool]$cleanup.keepBoundaryAirWalls -or [bool]$cleanup.keepInvalidZoneBlockers) {
    Write-Host "[FAIL] Cleanup config still preserves old boundary or invalid blockers."
    exit 1
}

if ([bool]$report.routeLineVisualBlocksPlayer -or [bool]$report.greenFrameVisualBlocksPlayer -or [bool]$report.labelVisualBlocksPlayer -or [bool]$report.hazardVisualBlocksPlayer) {
    Write-Host "[FAIL] Whitelist report allows a visual object to block player movement."
    exit 1
}

Write-Host "[PASS] NewMap collision whitelist source/config checks passed."
exit 0
