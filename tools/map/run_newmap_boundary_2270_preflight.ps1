param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$script:Checks = @()

function Add-Check {
    param([string]$Name, [bool]$Passed, [string]$Detail = "")
    $script:Checks += [pscustomobject]@{ Name = $Name; Passed = $Passed; Detail = $Detail }
}

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
}

function Read-Text {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return ""
    }

    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw
}

$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

$boundaryConfig = Read-Json "Assets\Data\P10\newmap_circular_boundary_config.json"
$boundaryReport = Read-Json "Assets\Data\P10\newmap_circular_boundary_report.json"
$boundsConfig = Read-Json "Assets\Data\P10\newmap_playable_bounds_config.json"
$boundsReport = Read-Json "Assets\Data\P10\newmap_playable_bounds_report.json"
$updateReport = Read-Json "Assets\Data\P10\newmap_boundary_2_27km_update_report.json"
$activeTargetReport = Read-Json "Assets\Data\P10\newmap_active_target_final_report.json"
$airWallReport = Read-Json "Assets\Data\P10\newmap_air_wall_regression_report.json"

$bootstrap = Read-Text "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$player = Read-Text "Assets\Scripts\NewMap\NewMapPlayerController.cs"
$npc = Read-Text "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"
$tests = Read-Text "Assets\Tests\PlayMode\NewMapRuntimePlayModeTests.cs"
$editorBuild = Read-Text "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"

Add-Check "Chuo_BaseMap exists" (Test-Path -LiteralPath (Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity") -PathType Leaf) "Assets/Scenes/Chuo_BaseMap.unity"
Add-Check "Boundary config exists" ($null -ne $boundaryConfig) "Assets/Data/P10/newmap_circular_boundary_config.json"
Add-Check "Boundary radius exactly 2270m" ($boundaryConfig -and ($boundaryConfig.PSObject.Properties.Name -contains "radiusMeters") -and [double]$boundaryConfig.radiusMeters -eq 2270.0) "radiusMeters=$($boundaryConfig.radiusMeters)"
Add-Check "Boundary is not stale 1500m" ($boundaryConfig -and [double]$boundaryConfig.radiusMeters -ne 1500.0) "radiusMeters=$($boundaryConfig.radiusMeters)"
Add-Check "Boundary is not stale 3500m" ($boundaryConfig -and [double]$boundaryConfig.radiusMeters -ne 3500.0) "radiusMeters=$($boundaryConfig.radiusMeters)"
Add-Check "Boundary center source original map center" ($boundaryConfig -and [string]$boundaryConfig.centerSource -eq "original_map_center") "centerSource=$($boundaryConfig.centerSource)"
Add-Check "Boundary invisible in normal mode" ($boundaryConfig -and -not [bool]$boundaryConfig.visibleInNormalMode) "visibleInNormalMode=$($boundaryConfig.visibleInNormalMode)"
Add-Check "Debug boundary hidden by default" ($boundaryConfig -and -not [bool]$boundaryConfig.debugVisible) "debugVisible=$($boundaryConfig.debugVisible)"
Add-Check "Boundary affects player and NPC" ($boundaryConfig -and [bool]$boundaryConfig.affectsPlayer -and [bool]$boundaryConfig.affectsNpc) "affectsPlayer=$($boundaryConfig.affectsPlayer) affectsNpc=$($boundaryConfig.affectsNpc)"

Add-Check "Circular boundary report reflects 2270m" ($boundaryReport -and [double]$boundaryReport.radiusMeters -eq 2270.0) "radiusMeters=$($boundaryReport.radiusMeters)"
Add-Check "Playable bounds config reflects 2270m" ($boundsConfig -and [double]$boundsConfig.radiusMeters -eq 2270.0) "radiusMeters=$($boundsConfig.radiusMeters)"
Add-Check "Playable bounds report reflects 2270m" ($boundsReport -and [double]$boundsReport.radiusMeters -eq 2270.0) "radiusMeters=$($boundsReport.radiusMeters)"
Add-Check "2.27km update report exists" ($null -ne $updateReport) "Assets/Data/P10/newmap_boundary_2_27km_update_report.json"
Add-Check "2.27km update report validated" ($updateReport -and [double]$updateReport.radiusMeters -eq 2270.0 -and [string]$updateReport.finalStatus -eq "validated_2_27km_runtime_boundary") "finalStatus=$($updateReport.finalStatus)"
Add-Check "Air-wall report reflects 2270m" ($airWallReport -and [double]$airWallReport.circularBoundaryRadiusMeters -eq 2270.0 -and -not [bool]$airWallReport.airWallsStillExist -and [bool]$airWallReport.airWallsInvisible) "radiusMeters=$($airWallReport.circularBoundaryRadiusMeters)"

Add-Check "Active target report reflects 2270m" ($activeTargetReport -and [double]$activeTargetReport.playableBoundaryRadiusMeters -eq 2270.0) "playableBoundaryRadiusMeters=$($activeTargetReport.playableBoundaryRadiusMeters)"
Add-Check "Active targets outside 2270m disabled" ($activeTargetReport -and [bool]$activeTargetReport.activeTargetsOutside2_27kmDisabledAtRuntime) "activeTargetsOutside2_27kmDisabledAtRuntime=$($activeTargetReport.activeTargetsOutside2_27kmDisabledAtRuntime)"
Add-Check "Active target report has no stale 1.5km fields" ($activeTargetReport -and ($activeTargetReport.PSObject.Properties.Name -notcontains "activeTargetsOutside1_5kmDisabledAtRuntime") -and ($activeTargetReport.PSObject.Properties.Name -notcontains "runtimeNonOfficialCandidateCountInside1_5km")) "active target report"

Add-Check "Player boundary clamp code present" ($player -match "ApplyCircularBoundaryClamp" -and $player -match "CircularBoundaryClampEnabled") "NewMapPlayerController"
Add-Check "NPC boundary clamp code present" ($npc -match "ClampToMovementBoundary" -and $npc -match "CircularBoundaryClampEnabled") "NewMapNpcCrowdPrototype"
Add-Check "Spawn validation uses active playable boundary" ($bootstrap -match "TryValidateSpawnCandidate" -and $bootstrap -match "IsInsideActivePlayableBoundary" -and $bootstrap -match "LastSpawnRejectedOutOfBoundsCount") "NewMapRuntimeBootstrap"
Add-Check "Safe spawn fallback uses spawn validation" ($bootstrap -match "fallback_safe_spawn:" -and $bootstrap -match "TryValidateSpawnCandidate") "NewMapRuntimeBootstrap"
Add-Check "Active target filtering uses playable boundary" ($bootstrap -match "FilterTargetsByPlayableBoundary" -and $bootstrap -match "LastActiveTargetsOutsidePlayableBoundaryDisabledCount") "NewMapRuntimeBootstrap"
Add-Check "Route guidance suppression uses playable boundary" ($bootstrap -match "LastRouteGuidesSuppressedOutsidePlayableBoundaryCount" -and $bootstrap -match "RouteGuideFactory = null") "NewMapRuntimeBootstrap"
Add-Check "Old rectangular air walls are not created" ($bootstrap -match "EnsureCircularBoundaryDiagnostics" -and $bootstrap -notmatch "P10_BoundaryAirWall_North") "NewMapRuntimeBootstrap"
Add-Check "Runtime source has no stale 3500m boundary literal" ($bootstrap -notmatch "3500f|3500\.0|3\.5km|ThreePointFive" -and $player -notmatch "3500f|3500\.0|3\.5km|ThreePointFive" -and $npc -notmatch "3500f|3500\.0|3\.5km|ThreePointFive") "player/NPC/spawn/target/route source"
Add-Check "Runtime source has no stale 1500m boundary literal" ($bootstrap -notmatch "radiusMeters\s*=\s*1500f|disabled_out_of_playable_bounds_1_5km|1\.5km circular|1500m circular" -and $tests -notmatch "Assert\.AreEqual\(1500f,\s*bootstrap\.LastCircularBoundary\.RadiusMeters") "player/NPC/spawn/target/route source"

Add-Check "PlayMode tests assert 2270m boundary" ($tests -match "2270f" -and $tests -match "RuntimeCircularBoundaryClampsPlayerAndNpcsInsideTwoPointTwoSevenKm") "NewMapRuntimePlayModeTests"
Add-Check "Boundary 2270 build method exists" ($editorBuild -match "BuildNewMapBoundary2270PlayerCommandLine" -and $editorBuild -match "NewMapBoundary2270Pre" -and $editorBuild -match "ChuoTsunamiEvacuation_NewMapBoundary2270Pre\.exe") "NewMapSceneSetupUtility"

$archiveFiles = @()
foreach ($pattern in @("*.zip", "*.7z", "*.tar", "*.gz", "*.tgz")) {
    $archiveFiles += @(Get-ChildItem -LiteralPath $root -File -Filter $pattern -Force -ErrorAction SilentlyContinue)
}
$releaseLikeDirectories = @(Get-ChildItem -LiteralPath $root -Directory -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)(^|[-_])(release|archive)([-_]|$)"
})
Add-Check "No final release/archive artifacts" ($archiveFiles.Count -eq 0 -and $releaseLikeDirectories.Count -eq 0) (($archiveFiles.Name + $releaseLikeDirectories.Name) -join ", ")

$forbiddenP10 = @(Get-ChildItem -LiteralPath (Join-Path $root "Assets\Data") -Recurse -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)\bP10[-_](E|F|G)([-_.]|$)"
})
$forbiddenP10 += @(Get-ChildItem -LiteralPath (Join-Path $root "docs") -Recurse -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)\bP10[-_](E|F|G)([-_.]|$)"
})
Add-Check "No P10-E/F/G artifacts" ($forbiddenP10.Count -eq 0) (($forbiddenP10 | ForEach-Object { $_.FullName }) -join ", ")

Write-Host "NewMap 2.27km boundary EXE preflight"
foreach ($check in $script:Checks) {
    $label = if ($check.Passed) { "PASS" } else { "FAIL" }
    Write-Host ("[{0}] {1}: {2}" -f $label, $check.Name, $check.Detail)
}

$failures = @($script:Checks | Where-Object { -not $_.Passed })
if ($failures.Count -gt 0) {
    Write-Host "Preflight result: FAIL"
    exit 1
}

Write-Host "Preflight result: PASS"
exit 0
