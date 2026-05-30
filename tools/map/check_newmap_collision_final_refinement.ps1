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

function Require-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

$config = Read-Json "Assets\Data\P10\newmap_building_collision_final_refinement.json"
$report = Read-Json "Assets\Data\P10\newmap_building_collision_final_refinement_report.json"
$precision = Read-Json "Assets\Data\P10\newmap_building_collision_precision_config.json"
$source = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs") -Raw

Require-True ([bool]$config.enabled) "Building collision final refinement disabled."
Require-True ([double]$config.defaultShrinkFactorXZ -eq 0.85) "Default shrink factor must be 0.85."
Require-True ([double]$config.nearRoadShrinkFactorXZ -eq 0.75) "Near-road/overflow shrink factor must be 0.75."
Require-True ([double]$config.nearTargetClearanceMeters -eq 4.0) "Near-target clearance must be 4m."
Require-True ([double]$config.nearSpawnClearanceMeters -eq 6.0) "Near-spawn clearance must be 6m."
Require-True ([double]$config.maxProxySizeMeters -eq 60.0) "Max proxy size must be 60m."
Require-True ([bool]$config.splitOversizedProxies) "Oversized proxies must be split."
Require-True ([bool]$config.disableProxyIfStillBlocksApproach) "Problematic blockers must be disableable."
Require-True ([bool]$precision.enabled) "Base precision config must remain enabled."
Require-True ([bool]$precision.disableClusterRootColliders) "Old inflated cluster/root colliders must stay disabled/skipped."
Require-True ($source.Contains("AddBuildingObstacleBoundsWithFinalRefinement")) "Runtime source missing final refinement splitter."
Require-True ($source.Contains("CarveBuildingPrecisionSpawnClearance")) "Runtime source missing spawn clearance carving."
Require-True ($source.Contains("LastBuildingFinalRefinement")) "Runtime source missing final refinement diagnostics."
Require-True (-not [bool]$report.oldInflatedCollidersReenabled) "Report says old inflated colliders were re-enabled."
Require-True ([bool]$report.buildingBlockingPreserved) "Report must preserve building blocking."
Require-True ([bool]$report.notGisGradeFootprintAccuracyClaim) "Report must avoid GIS-grade footprint claims."

Write-Host "[PASS] NewMap building collision final refinement validated."
exit 0
