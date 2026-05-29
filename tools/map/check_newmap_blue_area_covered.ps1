param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$bluePath = Join-Path $root "Assets\Data\P10\newmap_blue_area_cover_status.json"
$hardRemovalPath = Join-Path $root "Assets\Data\P10\newmap_blue_area_hard_removal.json"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"

if (-not (Test-Path -LiteralPath $bluePath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $hardRemovalPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $bootstrapPath -PathType Leaf)) {
    Write-Host "[FAIL] Blue area cover files are missing."
    exit 1
}

$blue = Get-Content -Encoding UTF8 -LiteralPath $bluePath -Raw | ConvertFrom-Json
$hardRemoval = Get-Content -Encoding UTF8 -LiteralPath $hardRemovalPath -Raw | ConvertFrom-Json
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw

$passed = [bool]$blue.supportDebugRenderersHidden -and
    [bool]$blue.visibleRoadLikeCoverEnabled -and
    -not [bool]$blue.coverMaterialBlue -and
    -not [bool]$blue.largeBlueSupportRendererAllowed -and
    -not [bool]$blue.knownBlueFallZonesAccessible -and
    [bool]$blue.blueFallThroughAreasInsidePlayableBoundsCovered -and
    [int]$hardRemoval.normalModeVisibleBlueSupportCount -eq 0 -and
    -not [bool]$hardRemoval.supportGridRendererActive -and
    $bootstrap -match "IsLargeBlueGroundSurfaceCandidate" -and
    $bootstrap -match "EnsureGameplayGroundCover"

if (-not $passed) {
    Write-Host "[FAIL] Blue area cover validation failed."
    exit 1
}

Write-Host "[PASS] Blue/fall-through areas are configured to be covered or blocked."
exit 0
