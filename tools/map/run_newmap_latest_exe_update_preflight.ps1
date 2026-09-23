$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

$checks = New-Object System.Collections.Generic.List[object]

function Add-Check {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Detail = ""
    )

    $checks.Add([pscustomobject][ordered]@{
        name = $Name
        passed = $Passed
        detail = $Detail
    })
}

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
}

function Read-Text {
    param([string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return ""
    }

    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw
}

$scenePath = Join-Path $ProjectRoot "Assets\Scenes\Chuo_BaseMap.unity"
$staminaConfigPath = "Assets\Data\P10\newmap_player_stamina_config.json"
$tsunamiConfigPath = "Assets\Data\P10\newmap_tsunami_mode_hotfix_config.json"
$boundaryConfigPath = "Assets\Data\P10\newmap_circular_boundary_config.json"
$boundaryReportPath = "Assets\Data\P10\newmap_boundary_2_27km_update_report.json"
$directLineControllerPath = "Assets\Scripts\NewMap\NewMapShelterDirectLineController.cs"
$gameControllerPath = "Assets\Scripts\NewMap\NewMapGameController.cs"
$runtimeBootstrapPath = "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$runtimeTypesPath = "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs"
$playerControllerPath = "Assets\Scripts\NewMap\NewMapPlayerController.cs"
$npcControllerPath = "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"
$editorBuildPath = "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"

$staminaConfig = Read-Json $staminaConfigPath
$tsunamiConfig = Read-Json $tsunamiConfigPath
$boundaryConfig = Read-Json $boundaryConfigPath
$boundaryReport = Read-Json $boundaryReportPath
$directLineController = Read-Text $directLineControllerPath
$gameController = Read-Text $gameControllerPath
$runtimeBootstrap = Read-Text $runtimeBootstrapPath
$runtimeTypes = Read-Text $runtimeTypesPath
$playerController = Read-Text $playerControllerPath
$npcController = Read-Text $npcControllerPath
$editorBuild = Read-Text $editorBuildPath

Add-Check "Chuo_BaseMap exists" (Test-Path -LiteralPath $scenePath -PathType Leaf) $scenePath
Add-Check "Stamina config exists" ($null -ne $staminaConfig) $staminaConfigPath
Add-Check "Tsunami hotfix config exists" ($null -ne $tsunamiConfig) $tsunamiConfigPath
Add-Check "2.27km boundary config exists" ($null -ne $boundaryConfig) $boundaryConfigPath
Add-Check "2.27km boundary update report exists" ($null -ne $boundaryReport) $boundaryReportPath
Add-Check "Shelter direct-line controller exists" (Test-Path -LiteralPath (Join-Path $ProjectRoot $directLineControllerPath) -PathType Leaf) $directLineControllerPath
Add-Check "Runtime active scene is Chuo_BaseMap" ($runtimeTypes -match "Assets/Scenes/Chuo_BaseMap\.unity") "NewMapRuntimeConstants.ScenePath"
Add-Check "Latest EXE build target path is configured" ($editorBuild -match "NewMapLatestHotfixPre" -and $editorBuild -match "ChuoTsunamiEvacuation_NewMapLatestHotfixPre\.exe") $editorBuildPath

if ($staminaConfig) {
    Add-Check "Stamina multiplier is latest reduced value" ([double]$staminaConfig.staminaMultiplier -eq 130.0) "staminaMultiplier=$($staminaConfig.staminaMultiplier)"
    Add-Check "Sprint additional multiplier is latest reduced value" ([double]$staminaConfig.sprintSpeedMultiplierAdditional -eq 1.1475) "sprintSpeedMultiplierAdditional=$($staminaConfig.sprintSpeedMultiplierAdditional)"
}

if ($tsunamiConfig) {
    Add-Check "Warning duration is 180 seconds" ([double]$tsunamiConfig.tsunamiWarningDurationSeconds -eq 180.0) "tsunamiWarningDurationSeconds=$($tsunamiConfig.tsunamiWarningDurationSeconds)"
    Add-Check "Pre-warning random max is configured" ([double]$tsunamiConfig.preWarningRandomMaxSeconds -eq 180.0) "preWarningRandomMaxSeconds=$($tsunamiConfig.preWarningRandomMaxSeconds)"
    Add-Check "Direct shelter lines enabled" ([bool]$tsunamiConfig.enableShelterDirectLines) "enableShelterDirectLines=$($tsunamiConfig.enableShelterDirectLines)"
    Add-Check "Direct shelter lines default to all" ([int]$tsunamiConfig.maxDisplayedShelterLines -eq 0) "maxDisplayedShelterLines=$($tsunamiConfig.maxDisplayedShelterLines)"
}

if ($boundaryConfig) {
    Add-Check "Boundary radius is exactly 2270m" (($boundaryConfig.PSObject.Properties.Name -contains "radiusMeters") -and [double]$boundaryConfig.radiusMeters -eq 2270.0) "radiusMeters=$($boundaryConfig.radiusMeters)"
    Add-Check "Boundary center uses original map center" ([string]$boundaryConfig.centerSource -eq "original_map_center") "centerSource=$($boundaryConfig.centerSource)"
    Add-Check "Boundary affects player and NPC" ([bool]$boundaryConfig.affectsPlayer -and [bool]$boundaryConfig.affectsNpc) "affectsPlayer=$($boundaryConfig.affectsPlayer) affectsNpc=$($boundaryConfig.affectsNpc)"
    Add-Check "Boundary invisible by default" (-not [bool]$boundaryConfig.visibleInNormalMode -and -not [bool]$boundaryConfig.debugVisible) "visibleInNormalMode=$($boundaryConfig.visibleInNormalMode) debugVisible=$($boundaryConfig.debugVisible)"
}

if ($boundaryReport) {
    Add-Check "Boundary report reflects 2.27km radius" ([double]$boundaryReport.radiusMeters -eq 2270.0 -and [bool]$boundaryReport.activeTargetPlayableBoundsCheckUpdated) "radiusMeters=$($boundaryReport.radiusMeters)"
    Add-Check "Route guidance bounds policy reported" ([bool]$boundaryReport.routeGuidanceBoundsCheckUpdated) "routeGuidanceBoundsCheckUpdated=$($boundaryReport.routeGuidanceBoundsCheckUpdated)"
}

Add-Check "Tourism disables tsunami/failure" ($gameController -match "StartTourismMode" -and $gameController -match "NewMapTsunamiStage\.Inactive" -and $gameController -match "Hazards, crowd failure, collapse/debris failure, and stamina drain are disabled") $gameControllerPath
Add-Check "Evacuation starts with PRE_WARNING_WAIT" ($gameController -match "NewMapTsunamiStage\.PreWarningWait" -and $runtimeTypes -match "PreWarningWait") "$gameControllerPath; $runtimeTypesPath"
Add-Check "Warning transitions to active tsunami" ($gameController -match "StartWarningPhase" -and $gameController -match "StartActiveTsunamiPhase" -and $gameController -match "warningStartedAtSeconds \+ stage1WarningSeconds") $gameControllerPath
Add-Check "Hazard inactive before ACTIVE_TSUNAMI" ($gameController -match "hazard\?\.SetStage\(NewMapTsunamiStage\.PreWarningWait\)" -and $gameController -match "hazard\?\.SetStage\(NewMapTsunamiStage\.Warning\)") $gameControllerPath
Add-Check "Direct lines use LineRenderer" ($directLineController -match "LineRenderer" -and $directLineController -match "SetPosition") $directLineControllerPath
Add-Check "Direct-line guidance has collider diagnostic" ($directLineController -match "CountLineCollidersForDiagnostics") $directLineControllerPath
Add-Check "Direct-line controller does not add colliders" ($directLineController -notmatch "AddComponent<.*Collider>") $directLineControllerPath
Add-Check "Official/non-official status preserved" ($directLineController -match "IsOfficialShelter" -and $directLineController -match "Category" -and $directLineController -match "Humanitarian" -and $directLineController -match "Proxy" -and $directLineController -match "Official" -and $directLineController -match "Non-official") $directLineControllerPath
Add-Check "Nearest line red behavior present" ($directLineController -match "NearestLineColor" -and $directLineController -match "OfficialLineColor" -and $directLineController -match "NonOfficialLineColor") $directLineControllerPath
Add-Check "Self-audit checks direct-line colliders" ($runtimeBootstrap -match "shelter_direct_lines_created" -and $runtimeBootstrap -match "CountShelterDirectLineCollidersForDiagnostics") $runtimeBootstrapPath
Add-Check "Runtime filters out-of-bounds targets" ($runtimeBootstrap -match "FilterTargetsByPlayableBoundary" -and $runtimeBootstrap -match "LastActiveTargetsOutsidePlayableBoundaryDisabledCount") $runtimeBootstrapPath
Add-Check "Player clamps to circular boundary" ($playerController -match "ApplyCircularBoundaryClamp" -and $playerController -match "CircularBoundaryClampEnabled") $playerControllerPath
Add-Check "NPC spawn/movement clamps to circular boundary" ($npcController -match "ClampToMovementBoundary" -and $npcController -match "CircularBoundaryClampEnabled") $npcControllerPath
Add-Check "Runtime boundary code has no stale 3500m literal" ($runtimeBootstrap -notmatch "3500f|3500\.0|3\.5km|ThreePointFive" -and $playerController -notmatch "3500f|3500\.0|3\.5km|ThreePointFive" -and $npcController -notmatch "3500f|3500\.0|3\.5km|ThreePointFive") "NewMap runtime/player/NPC"
Add-Check "Runtime boundary code has no stale smaller boundary literal" ($runtimeBootstrap -notmatch "radiusMeters\s*=\s*1500f|disabled_out_of_playable_bounds_1_5km|1\.5km circular|1500m circular") "NewMap runtime bootstrap"
Add-Check "No runtime web request in NewMap scripts" (-not [bool](Select-String -Path (Join-Path $ProjectRoot "Assets\Scripts\NewMap\*.cs") -Pattern "UnityWebRequest|HttpClient|System\.Net|DownloadString|DownloadFile" -Quiet)) "Assets/Scripts/NewMap"
Add-Check "No official route false claim in guidance code" ($directLineController -notmatch "official evacuation route" -and $directLineController -notmatch "official route guidance") $directLineControllerPath

$forbiddenPaths = @(
    "Assets\Data\P10-E",
    "Assets\Data\P10-F",
    "Assets\Data\P10-G",
    "docs\P10-E",
    "docs\P10-F",
    "docs\P10-G",
    "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10-E",
    "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10-F",
    "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10-G"
)

foreach ($relative in $forbiddenPaths) {
    $path = if ([System.IO.Path]::IsPathRooted($relative)) { $relative } else { Join-Path $ProjectRoot $relative }
    Add-Check "Forbidden phase path absent: $relative" (-not (Test-Path -LiteralPath $path)) $path
}

$archiveCandidates = @(
    "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapLatestHotfixPre.zip",
    "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapLatestHotfixPre.7z",
    "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapLatestHotfixPre_Final"
)

foreach ($path in $archiveCandidates) {
    Add-Check "No final release/archive artifact: $path" (-not (Test-Path -LiteralPath $path)) $path
}

$failed = @($checks | Where-Object { -not $_.passed })
foreach ($check in $checks) {
    $status = if ($check.passed) { "[PASS]" } else { "[FAIL]" }
    Write-Host "$status $($check.name) $($check.detail)"
}

if ($failed.Count -gt 0) {
    Write-Host "[FAIL] NewMap latest hotfix EXE update preflight failed. failures=$($failed.Count)"
    exit 1
}

Write-Host "[PASS] NewMap latest hotfix EXE update preflight passed. checks=$($checks.Count)"
