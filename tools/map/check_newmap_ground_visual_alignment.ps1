param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$bootstrap = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs") -Raw
$player = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs") -Raw
$status = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_ground_visual_alignment_status.json") -Raw | ConvertFrom-Json

$checks = @(
    ($bootstrap -match "LastRuntimeGroundSurfaceY"),
    ($bootstrap -match "renderer\.enabled = false"),
    ($bootstrap -match "LastPlayerSpawnGroundDelta"),
    ($player -match "GroundSkinOffset"),
    ([double]$status.playerSpawnGroundDeltaMeters -le [double]$status.spawnHeightToleranceMeters),
    (-not [bool]$status.supportSurfaceVisible),
    ([bool]$status.localTrainingProxyYOffsetFixed),
    ([bool]$status.interactionTargetHeightAligned)
)

if ($checks -contains $false) {
    Write-Host "[FAIL] Ground visual alignment check failed."
    exit 1
}

Write-Host "[PASS] Ground visual alignment check passed."
exit 0
