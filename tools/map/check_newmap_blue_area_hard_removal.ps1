param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$jsonPath = Join-Path $root "Assets\Data\P10\newmap_blue_area_hard_removal.json"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"

if (-not (Test-Path -LiteralPath $jsonPath -PathType Leaf) -or -not (Test-Path -LiteralPath $bootstrapPath -PathType Leaf)) {
    Write-Host "[FAIL] Blue hard-removal files are missing."
    exit 1
}

$report = Get-Content -Encoding UTF8 -LiteralPath $jsonPath -Raw | ConvertFrom-Json
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw

$passed = [bool]$report.hardRemovalEnabled -and
    [int]$report.normalModeVisibleBlueSupportCount -eq 0 -and
    [int]$report.largeVisibleBluePlaneCount -eq 0 -and
    -not [bool]$report.supportGridRendererActive -and
    -not [bool]$report.knownBlueDebugObjectActive -and
    $bootstrap -match "IsLargeBlueGroundSurfaceCandidate" -and
    $bootstrap -match "LastVisibleLargeBlueGroundRendererCount" -and
    $bootstrap -match "renderer.enabled = false"

if (-not $passed) {
    Write-Host "[FAIL] Blue hard-removal check failed."
    exit 1
}

Write-Host "[PASS] Blue hard-removal check passed."
exit 0
