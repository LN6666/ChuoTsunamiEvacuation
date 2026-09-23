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

$runtime = Read-Json "Assets\Data\P10\newmap_spawn_config.json"
$final = Read-Json "Assets\Data\P10\newmap_spawn_final_safety_config.json"
$report = Read-Json "Assets\Data\P10\newmap_spawn_final_safety_report.json"
$source = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs") -Raw

Require-True ([bool]$final.enabled) "Final spawn safety config disabled."
Require-True ([int]$runtime.maxSpawnAttempts -eq 500 -and [int]$final.maxSpawnAttempts -eq 500) "Max spawn attempts must be 500."
Require-True ([double]$runtime.minDistanceFromBuildingMeters -eq 4.0 -and [double]$final.minDistanceFromBuildingMeters -eq 4.0) "Spawn building clearance must be 4m."
Require-True ([bool]$runtime.finalOverlapCheck -and [bool]$final.finalOverlapCheck) "Final overlap check must be enabled."
Require-True ([bool]$runtime.fallbackSafeSpawnEnabled -and [bool]$final.fallbackSafeSpawnEnabled) "Fallback safe spawn must be enabled."
Require-True ([string]$runtime.fallbackSafeSpawnId -eq "newmap_safe_spawn_01") "Fallback safe spawn id must be newmap_safe_spawn_01."
Require-True ([bool]$runtime.useBuildingProxyCache -and [bool]$runtime.useRendererBoundsCache) "Runtime spawn must use building proxy and renderer bounds caches."
Require-True ([bool]$runtime.useGroundCoverHit) "Runtime spawn must require ground cover/support hit."
Require-True ([double]$runtime.rejectInsideAirWallMarginMeters -eq 3.0) "Spawn air-wall rejection margin must be 3m."
Require-True ($source.Contains("HasSpawnFinalOverlap")) "Runtime source missing final spawn overlap check."
Require-True ($source.Contains("buildingRendererBounds")) "Runtime source missing renderer building-bounds cache."
Require-True ($source.Contains("spawn_repeated_100_avoids_buildings")) "Runtime self-audit missing repeated spawn safety sample."
Require-True ([string]$report.runtimeStatus -ne "") "Spawn final safety report missing runtime status."

Write-Host "[PASS] NewMap spawn final safety validated."
exit 0
