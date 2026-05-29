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
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs") -Raw
$player = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs") -Raw
$npc = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs") -Raw

Require-Condition ([bool]$config.enabled) "Circular boundary disabled."
Require-Condition ([double]$config.radiusMeters -eq 3500.0) "Circular boundary radius must be 3500m."
Require-Condition ([string]$config.centerSource -eq "original_map_center") "Boundary center source must remain original_map_center."
Require-Condition ([string]$config.boundaryMode -eq "runtime_circular_clamp") "Boundary mode must be runtime_circular_clamp."
Require-Condition (-not [bool]$config.visibleInNormalMode) "Boundary must be invisible in normal mode."
Require-Condition ([bool]$config.affectsPlayer -and [bool]$config.affectsNpc) "Boundary must affect both player and NPC."
Require-Condition ($bootstrap -match "ResolveCircularBoundary" -and $bootstrap -match "LastCircularBoundary") "Runtime circular boundary resolution missing."
Require-Condition ($bootstrap -match "P10_CircularBoundary_RuntimeClamp_Diagnostic" -and $bootstrap -notmatch "P10_BoundaryAirWall_North") "Old rectangular air-wall creation must be absent."
Require-Condition ($player -match "ApplyCircularBoundaryClamp" -and $player -match "CircularBoundaryClampEnabled") "Player circular clamp missing."
Require-Condition ($npc -match "ClampToMovementBoundary" -and $npc -match "CircularBoundaryClampEnabled") "NPC circular clamp missing."

Write-Host "[PASS] NewMap 3.5km circular boundary checks passed."
exit 0
