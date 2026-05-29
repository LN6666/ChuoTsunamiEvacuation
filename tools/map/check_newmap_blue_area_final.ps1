param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$bluePath = Join-Path $root "Assets\Data\P10\newmap_blue_area_final_fix.json"
$gridPath = Join-Path $root "Assets\Data\P10\newmap_adaptive_support_grid_report.json"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"

foreach ($path in @($bluePath, $gridPath, $bootstrapPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Blue-area check file missing: $path"
        exit 1
    }
}

$blue = Get-Content -Encoding UTF8 -LiteralPath $bluePath -Raw | ConvertFrom-Json
$grid = Get-Content -Encoding UTF8 -LiteralPath $gridPath -Raw | ConvertFrom-Json
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw

$ok =
    [int]$blue.normalModeVisibleSuspectCount -eq 0 -and
    -not [bool]$blue.supportGridRendererVisibleInNormalMode -and
    -not [bool]$blue.largeBlueSupportPlaneVisible -and
    -not [bool]$blue.debugGroundRootActiveByDefault -and
    [bool]$grid.renderersDisabled -and
    -not [bool]$grid.blueSupportVisualActive -and
    $bootstrap -match "EnforceSupportSurfaceVisibility" -and
    $bootstrap -match "renderer.enabled = false"

if (-not $ok) {
    Write-Host "[FAIL] Large blue/support area visibility check failed."
    exit 1
}

Write-Host "[PASS] Blue support/debug surfaces are hidden in normal mode."
exit 0
