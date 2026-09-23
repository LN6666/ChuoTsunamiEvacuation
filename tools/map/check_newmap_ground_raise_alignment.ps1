param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$config = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_ground_cover_raise_config.json") -Raw | ConvertFrom-Json
$report = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_ground_cover_raise_report.json") -Raw | ConvertFrom-Json
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw

$failures = @()
if (-not [bool]$config.enabled) { $failures += "Ground raise config is disabled." }
if (-not [bool]$config.keepImportedBuildingsFixed) { $failures += "Ground raise must keep buildings fixed." }
if ([double]$config.maxRaiseOffsetMeters -gt 10.0) { $failures += "Ground raise cap exceeds 10m." }
if (-not [bool]$report.groundCoverRaiseEnabled) { $failures += "Ground raise report disabled." }
if ([bool]$report.buildingsMoved) { $failures += "Ground raise report moved buildings." }
if ([double]$report.selectedRaiseOffset -le 0.0) { $failures += "Selected raise offset must be positive." }
if ($bootstrap -notmatch "DisableBuildingVerticalMovesForGroundCoverRaise") { $failures += "Bootstrap does not disable building moves while ground raise is active." }
if ($bootstrap -notmatch "ResolveRaisedGroundCoverY") { $failures += "Bootstrap does not resolve raised ground cover Y." }
if ($bootstrap -notmatch "groundRaiseStatus") { $failures += "Bootstrap does not log ground raise diagnostics." }

if ($failures.Count -gt 0) {
    foreach ($failure in $failures) { Write-Host "[FAIL] $failure" }
    exit 1
}

Write-Host "[PASS] NewMap ground raise alignment configuration validated."
exit 0
