param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing $RelativePath"
        exit 1
    }

    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
}

$boundary = Read-Json "Assets\Data\P10\newmap_circular_boundary_config.json"
$npcConfig = Read-Json "Assets\Data\P10\newmap_npc_distribution_config.json"
$wideConfig = Read-Json "Assets\Data\P10\newmap_npc_boundary_wide_distribution_config.json"
$sync = Read-Json "Assets\Data\P10\newmap_boundary_npc_sync_report.json"

$errors = @()
if ([double]$boundary.radiusMeters -ne 2270.0) { $errors += "Circular boundary radius must be exactly 2270." }
if ([double]$npcConfig.distributionRadiusMeters -ne 2270.0) { $errors += "NPC runtime distribution radius must be exactly 2270." }
if ([double]$wideConfig.distributionRadiusMeters -ne 2270.0) { $errors += "Boundary-wide NPC config radius must be exactly 2270." }
if (-not [bool]$wideConfig.matchCircularBoundary) { $errors += "NPC boundary-wide config must match circular boundary." }
if ([string]$wideConfig.centerSource -ne "same_as_circular_boundary") { $errors += "NPC distribution center must match circular boundary." }
if ([int]$npcConfig.sectorCount -lt 48 -or [int]$npcConfig.ringCount -lt 8) { $errors += "NPC sector/ring coverage is too low." }
if (-not [bool]$npcConfig.avoidBuildings) { $errors += "NPC building avoidance must remain enabled." }
if (-not [bool]$npcConfig.snapToGroundCover) { $errors += "NPCs must snap to raised ground cover." }
if ([double]$sync.circularBoundaryRadiusMeters -ne 2270.0 -or [double]$sync.npcDistributionRadiusMeters -ne 2270.0) { $errors += "Boundary/NPC sync report radius mismatch." }
if ([bool]$sync.stale1500BoundaryValueActive -or [bool]$sync.stale3500BoundaryValueActive) { $errors += "Boundary sync report marks old radius active." }

if ($errors.Count -gt 0) {
    foreach ($errorMessage in $errors) { Write-Host "[FAIL] $errorMessage" }
    exit 1
}

Write-Host "[PASS] NewMap NPC 2.27km boundary-wide distribution check passed."
exit 0
