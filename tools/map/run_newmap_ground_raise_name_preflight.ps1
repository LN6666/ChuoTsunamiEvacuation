param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$script:Checks = @()

function Add-Check {
    param([string]$Name, [bool]$Passed, [string]$Detail)
    $script:Checks += [pscustomobject]@{ Name = $Name; Passed = $Passed; Detail = $Detail }
}

function Invoke-ToolCheck {
    param([string]$Name, [string]$RelativeScript)
    $scriptPath = Join-Path $root $RelativeScript
    if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
        Add-Check $Name $false "missing $RelativeScript"
        return
    }

    & $scriptPath -ProjectRoot $root
    Add-Check $Name ($LASTEXITCODE -eq 0) $RelativeScript
}

function Get-NameMatches {
    param([string[]]$ScanRoots, [string]$Pattern)
    $matches = @()
    foreach ($scanRoot in $ScanRoots) {
        if (-not (Test-Path -LiteralPath $scanRoot)) {
            continue
        }

        $matches += @(Get-ChildItem -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue | Where-Object {
            $_.Name -match $Pattern
        } | ForEach-Object { $_.FullName })
    }

    return $matches
}

$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$baseScene = Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$playerPath = Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs"
$npcPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"
$labelPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNameLabelController.cs"
$setupPath = Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"
$readinessPath = Join-Path $root "Assets\Data\P10\newmap_manual_playtest_readiness.json"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Chuo_BaseMap scene exists" (Test-Path -LiteralPath $baseScene -PathType Leaf) $baseScene
Add-Check "GroundRaiseName player build method exists" ([bool](Select-String -LiteralPath $setupPath -Pattern "BuildNewMapGroundRaiseNamePlayerCommandLine" -SimpleMatch -Quiet)) "NewMapSceneSetupUtility"

Invoke-ToolCheck "Ground raise/name JSON validation" "tools\map\validate_newmap_ground_raise_name_json.ps1"
Invoke-ToolCheck "Ground raise alignment validation" "tools\map\check_newmap_ground_raise_alignment.ps1"
Invoke-ToolCheck "Name cache loaded validation" "tools\map\check_newmap_name_cache_loaded.ps1"
Invoke-ToolCheck "Runtime no-web validation" "tools\map\check_newmap_runtime_no_web_requests.ps1"
Invoke-ToolCheck "Ground cover collider regression" "tools\map\check_newmap_ground_cover_colliders.ps1"
Invoke-ToolCheck "Blue area cover regression" "tools\map\check_newmap_blue_area_covered.ps1"
Invoke-ToolCheck "Fall-out prevention regression" "tools\map\check_newmap_fall_out_prevention.ps1"
Invoke-ToolCheck "Spawn building-overlap regression" "tools\map\check_newmap_spawn_not_inside_building.ps1"
Invoke-ToolCheck "Mouse drag-look regression" "tools\map\check_newmap_mouse_drag_buttons.ps1"

$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$player = Get-Content -Encoding UTF8 -LiteralPath $playerPath -Raw
$npc = Get-Content -Encoding UTF8 -LiteralPath $npcPath -Raw
$labels = Get-Content -Encoding UTF8 -LiteralPath $labelPath -Raw

Add-Check "Bootstrap raises ground cover, not buildings" ($bootstrap -match "ResolveRaisedGroundCoverY" -and $bootstrap -match "DisableBuildingVerticalMovesForGroundCoverRaise" -and $bootstrap -match "disabled_by_ground_cover_raise_buildings_fixed") "NewMapRuntimeBootstrap"
Add-Check "Ground raise diagnostics logged" ($bootstrap -match "groundRaiseEnabled" -and $bootstrap -match "groundRaiseOffset" -and $bootstrap -match "playerBuildingCollisionEnabled") "NewMapRuntimeBootstrap"
Add-Check "Player building collision correction exists" ($player -match "ConfigureBuildingCollision" -and $player -match "ApplyBuildingCollisionCorrection") "NewMapPlayerController"
Add-Check "NPC continuous movement and recovery exists" ($npc -match "NewMapNpcMovementState" -and $npc -match "StoppedWithoutReasonCount" -and $npc -match "ResolveNpcPositionAfterBuildingCollision") "NewMapNpcCrowdPrototype"
Add-Check "Runtime labels load local cache only" ($labels -match "NewMapNameCache.Load" -and $labels -match "RuntimeNetworkRequestsAllowed => false" -and $labels -notmatch "UnityWebRequest|HttpClient|WebRequest") "NewMapNameLabelController"

if (Test-Path -LiteralPath $readinessPath -PathType Leaf) {
    $readiness = Get-Content -Encoding UTF8 -LiteralPath $readinessPath -Raw | ConvertFrom-Json
    $allowed = @("ready_for_manual_playtest", "ready_with_documented_building_visual_limitations", "needs_quick_fix_before_manual_test", "blocked")
    Add-Check "Manual readiness decision exists" ($allowed -contains [string]$readiness.manualReadinessDecision) ([string]$readiness.manualReadinessDecision)
    Add-Check "Readiness avoids GIS/route accuracy claims" (-not [bool]$readiness.roadTerrainAccuracyClaimed -and -not [bool]$readiness.officialRouteValidationClaimed) "no GIS/official route claim"
}
else {
    Add-Check "Manual readiness decision exists" $false $readinessPath
}

$rootArchivePatterns = @("*.zip", "*.7z", "*.tar", "*.gz", "*.tgz")
$archiveFiles = @()
foreach ($pattern in $rootArchivePatterns) {
    $archiveFiles += @(Get-ChildItem -LiteralPath $root -File -Filter $pattern -Force -ErrorAction SilentlyContinue)
}
$releaseLikeDirectories = @(Get-ChildItem -LiteralPath $root -Directory -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)(^|[-_])(release|archive)([-_]|$)"
})
Add-Check "No final release/archive artifacts" ($archiveFiles.Count -eq 0 -and $releaseLikeDirectories.Count -eq 0) (($archiveFiles.Name + $releaseLikeDirectories.Name) -join ", ")

$forbiddenRoots = @(
    (Join-Path $root "docs"),
    (Join-Path $root "tools"),
    (Join-Path $root "Assets\Data"),
    (Join-Path $root "Assets\Scripts"),
    (Join-Path $root "codex_prompts")
)
$rootFileMatches = @(Get-ChildItem -LiteralPath $root -File -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)\bP10[-_](E|F|G)([-_.]|$)"
} | ForEach-Object { $_.FullName })
$forbiddenP10 = @($rootFileMatches + @(Get-NameMatches -ScanRoots $forbiddenRoots -Pattern "(?i)\bP10[-_](E|F|G)([-_.]|$)"))
Add-Check "No P10-E/F/G artifacts" ($forbiddenP10.Count -eq 0) ($forbiddenP10 -join ", ")

Write-Host "NewMap ground raise + name completion preflight"
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
