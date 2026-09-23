param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$report = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_ground_height_realignment_round2.json") -Raw | ConvertFrom-Json
$bootstrap = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs") -Raw

$ok =
    [double]$report.supportToVisualToleranceMeters -le 0.35 -and
    [string]$report.runtimeGroundReference -eq "renderer_bounds_low_percentile_building_base" -and
    $bootstrap -match "LastVisualGroundReferenceY" -and
    $bootstrap -match "supportToVisualGroundDelta" -and
    $bootstrap -match "visualGroundSamples" -and
    $bootstrap -match "Round2FallbackSupportSurfaceY"

if (-not $ok) {
    Write-Host "[FAIL] Ground height alignment round-2 check failed."
    exit 1
}

Write-Host "[PASS] Ground height alignment round-2 check passed."
exit 0
