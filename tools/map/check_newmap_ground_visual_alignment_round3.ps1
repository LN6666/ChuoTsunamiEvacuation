param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$source = Get-Content -LiteralPath $bootstrapPath -Raw

if ($source -notmatch 'IsUsableRoadOrGroundSample' -or
    $source -notmatch 'LastSupportToRoadDelta' -or
    $source -notmatch 'LastSupportToBuildingBaseDelta' -or
    $source -notmatch 'EnsurePlayableBoundsAirWalls' -or
    $source -notmatch 'CreateAirWall') {
    Write-Host "[FAIL] Round 3 ground alignment and air-wall runtime diagnostics are missing."
    exit 1
}

$alignment = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_ground_visual_alignment_round3.json") -Raw | ConvertFrom-Json
if ([bool]$alignment.supportRendererVisible -or [double]$alignment.supportToRoadDeltaToleranceMeters -gt 1.0) {
    Write-Host "[FAIL] Ground alignment report must disallow visible support and keep tight visual delta tolerance."
    exit 1
}

$bounds = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_playable_bounds_report.json") -Raw | ConvertFrom-Json
if (-not [bool]$bounds.spawnValidationUsesPlayableBounds -or -not [bool]$bounds.npcDistributionUsesPlayableBounds) {
    Write-Host "[FAIL] Playable bounds report must confirm spawn/NPC bounds integration."
    exit 1
}

Write-Host "[PASS] Round 3 ground visual alignment checks passed."
exit 0
