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
$npcPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"
$setupPath = Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"
$readinessPath = Join-Path $root "Assets\Data\P10\newmap_manual_playtest_readiness.json"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Chuo_BaseMap scene exists" (Test-Path -LiteralPath $baseScene -PathType Leaf) $baseScene
Add-Check "BuildingSnapNpc100x player build method exists" ([bool](Select-String -LiteralPath $setupPath -Pattern "BuildNewMapBuildingSnapNpc100xPlayerCommandLine" -SimpleMatch -Quiet)) "NewMapSceneSetupUtility"

Invoke-ToolCheck "Building/NPC100x JSON validation" "tools\map\validate_newmap_building_snap_npc100x_json.ps1"
Invoke-ToolCheck "Floating building snapdown validation" "tools\map\check_newmap_floating_building_snapdown.ps1"
Invoke-ToolCheck "NPC 100x distribution validation" "tools\map\check_newmap_npc_100x_distribution.ps1"
Invoke-ToolCheck "Ground cover regression validation" "tools\map\check_newmap_ground_cover_regression.ps1"
Invoke-ToolCheck "Ground cover collider validation" "tools\map\check_newmap_ground_cover_colliders.ps1"
Invoke-ToolCheck "Spawn/NPC/target ground cover validation" "tools\map\check_newmap_spawn_on_ground_cover.ps1"
Invoke-ToolCheck "Fall-out prevention" "tools\map\check_newmap_fall_out_prevention.ps1"
Invoke-ToolCheck "Air-wall regression validation" "tools\map\check_newmap_air_walls.ps1"
Invoke-ToolCheck "Spawn building-overlap validation" "tools\map\check_newmap_spawn_not_inside_building.ps1"
Invoke-ToolCheck "Mouse drag-look validation" "tools\map\check_newmap_mouse_drag_buttons.ps1"
Invoke-ToolCheck "Name cache runtime-only validation" "tools\map\check_newmap_name_cache_runtime_only.ps1"

$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$npc = Get-Content -Encoding UTF8 -LiteralPath $npcPath -Raw
Add-Check "Runtime uses gameplay ground cover for snapdown" ($bootstrap -match "ApplyFloatingBuildingSnapdownToGameplayGroundCover" -and $bootstrap -match "groundCoverConfig") "NewMapRuntimeBootstrap"
Add-Check "Runtime logs snapdown diagnostics" ($bootstrap -match "buildingSnapdownScanned" -and $bootstrap -match "buildingsSnappedDown") "NewMapRuntimeBootstrap"
Add-Check "NPC avoids buildings and uses static far proxies" ($npc -match "avoidBuildings" -and $npc -match "farNpcStaticProxyMode" -and $npc -match "RejectedInsideBuildingCount") "NewMapNpcCrowdPrototype"
Add-Check "Runtime no web APIs" ($bootstrap -notmatch "UnityWebRequest|HttpClient|WebRequest" -and $npc -notmatch "UnityWebRequest|HttpClient|WebRequest") "runtime no-web"

if (Test-Path -LiteralPath $readinessPath -PathType Leaf) {
    $readiness = Get-Content -Encoding UTF8 -LiteralPath $readinessPath -Raw | ConvertFrom-Json
    $allowed = @("ready_for_manual_playtest", "ready_with_documented_building_visual_limitations", "needs_quick_fix_before_manual_test", "blocked")
    Add-Check "Manual readiness decision exists" ($allowed -contains [string]$readiness.manualReadinessDecision) ([string]$readiness.manualReadinessDecision)
    Add-Check "Manual readiness avoids GIS accuracy claim" (-not [bool]$readiness.buildingSnapdownClaimsGisAccuracy -and -not [bool]$readiness.roadTerrainAccuracyClaimed) "no GIS-grade claim"
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

Write-Host "NewMap building snapdown + NPC100x preflight"
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
