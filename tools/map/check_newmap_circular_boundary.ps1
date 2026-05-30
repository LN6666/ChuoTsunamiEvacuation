param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Require-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

$config = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_circular_boundary_config.json") -Raw | ConvertFrom-Json
$report = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_circular_boundary_report.json") -Raw | ConvertFrom-Json
$playable = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_playable_bounds_config.json") -Raw | ConvertFrom-Json
$activeTargets = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_active_target_final_report.json") -Raw | ConvertFrom-Json
$updateReport = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_boundary_2_27km_update_report.json") -Raw | ConvertFrom-Json
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs") -Raw
$player = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs") -Raw
$npc = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs") -Raw

Require-Condition ([bool]$config.enabled) "Circular boundary disabled."
Require-Condition ($config.PSObject.Properties.Name -contains "radiusMeters") "Circular boundary radius is missing."
Require-Condition ([double]$config.radiusMeters -eq 2270.0) "Circular boundary radius must be exactly 2270m."
Require-Condition ([double]$report.radiusMeters -eq 2270.0) "Circular boundary report must be exactly 2270m."
Require-Condition ([double]$playable.radiusMeters -eq 2270.0) "Playable bounds config must be exactly 2270m."
Require-Condition ([double]$activeTargets.playableBoundaryRadiusMeters -eq 2270.0) "Active target report must reflect the 2270m boundary."
Require-Condition ([double]$updateReport.radiusMeters -eq 2270.0) "2.27km boundary update report must be exactly 2270m."
Require-Condition ([string]$config.centerSource -eq "original_map_center") "Boundary center source must remain original_map_center."
Require-Condition ([string]$config.boundaryMode -eq "runtime_circular_clamp") "Boundary mode must be runtime_circular_clamp."
Require-Condition (-not [bool]$config.visibleInNormalMode) "Boundary must be invisible in normal mode."
Require-Condition (-not [bool]$config.debugVisible) "Debug boundary visualization must be off by default."
Require-Condition ([bool]$config.affectsPlayer -and [bool]$config.affectsNpc) "Boundary must affect both player and NPC."
Require-Condition ($bootstrap -match "ResolveCircularBoundary" -and $bootstrap -match "LastCircularBoundary") "Runtime circular boundary resolution missing."
Require-Condition ($bootstrap -match "P10_CircularBoundary_RuntimeClamp_Diagnostic" -and $bootstrap -notmatch "P10_BoundaryAirWall_North") "Old rectangular air-wall creation must be absent."
Require-Condition ($bootstrap -match "FilterTargetsByPlayableBoundary" -and $bootstrap -match "LastActiveTargetsOutsidePlayableBoundaryDisabledCount") "Active target playable-boundary filtering missing."
Require-Condition ($bootstrap -match "LastRouteGuidesSuppressedOutsidePlayableBoundaryCount") "Route guidance out-of-bounds suppression reporting missing."
Require-Condition ([bool]$report.activeTargetsOutsideBoundaryDisabled -and [bool]$report.routeGuidanceOutsideBoundaryDisabled) "Boundary report must confirm active target and route guidance filtering."
Require-Condition ([bool]$activeTargets.activeTargetsOutside2_27kmDisabledAtRuntime) "Active target report must confirm 2.27km out-of-bounds filtering."
Require-Condition ($activeTargets.PSObject.Properties.Name -notcontains "activeTargetsOutside1_5kmDisabledAtRuntime") "Active target report still contains stale 1.5km fields."
Require-Condition ($player -match "ApplyCircularBoundaryClamp" -and $player -match "CircularBoundaryClampEnabled") "Player circular clamp missing."
Require-Condition ($npc -match "ClampToMovementBoundary" -and $npc -match "CircularBoundaryClampEnabled") "NPC circular clamp missing."
Require-Condition ($bootstrap -notmatch "3500f|3500\.0|3\.5km|ThreePointFive") "Runtime bootstrap still contains stale 3.5km boundary text."
Require-Condition ($bootstrap -notmatch "radiusMeters\s*=\s*1500f|disabled_out_of_playable_bounds_1_5km|1\.5km circular|1500m circular") "Runtime bootstrap still contains stale 1.5km boundary text."
Require-Condition ($player -notmatch "3500f|3500\.0|3\.5km|ThreePointFive") "Player boundary code still contains stale 3.5km boundary text."
Require-Condition ($npc -notmatch "3500f|3500\.0|3\.5km|ThreePointFive") "NPC boundary code still contains stale 3.5km boundary text."

Write-Host "[PASS] NewMap 2.27km circular boundary checks passed."
exit 0
