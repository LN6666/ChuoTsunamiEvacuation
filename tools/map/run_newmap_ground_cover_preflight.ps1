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
$setupPath = Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"
$readinessPath = Join-Path $root "Assets\Data\P10\newmap_manual_playtest_readiness.json"
$materialPath = Join-Path $root "Assets\Resources\NewMap\P10_NewMap_RoadGroundCover.mat"
$adaptiveConfigPath = Join-Path $root "Assets\Data\P10\newmap_adaptive_support_grid_config.json"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Chuo_BaseMap scene exists" (Test-Path -LiteralPath $baseScene -PathType Leaf) $baseScene
Add-Check "Ground cover player build method exists" ([bool](Select-String -LiteralPath $setupPath -Pattern "BuildNewMapGroundCoverPlayerCommandLine" -SimpleMatch -Quiet)) "NewMapSceneSetupUtility"
Add-Check "Ground cover material asset exists" (Test-Path -LiteralPath $materialPath -PathType Leaf) "P10_NewMap_RoadGroundCover.mat"

Invoke-ToolCheck "Ground cover JSON validation" "tools\map\validate_newmap_ground_cover_json.ps1"
Invoke-ToolCheck "Ground cover collider validation" "tools\map\check_newmap_ground_cover_colliders.ps1"
Invoke-ToolCheck "Blue area cover validation" "tools\map\check_newmap_blue_area_covered.ps1"
Invoke-ToolCheck "Spawn/NPC/target ground cover validation" "tools\map\check_newmap_spawn_on_ground_cover.ps1"
Invoke-ToolCheck "Fall-out prevention" "tools\map\check_newmap_fall_out_prevention.ps1"
Invoke-ToolCheck "Air-wall regression validation" "tools\map\check_newmap_air_walls.ps1"
Invoke-ToolCheck "Name cache runtime-only validation" "tools\map\check_newmap_name_cache_runtime_only.ps1"
Invoke-ToolCheck "Spawn building-overlap validation" "tools\map\check_newmap_spawn_not_inside_building.ps1"
Invoke-ToolCheck "Mouse drag-look validation" "tools\map\check_newmap_mouse_drag_buttons.ps1"

if (Test-Path -LiteralPath $adaptiveConfigPath -PathType Leaf) {
    $adaptiveConfig = Get-Content -Encoding UTF8 -LiteralPath $adaptiveConfigPath -Raw | ConvertFrom-Json
    Add-Check "Adaptive support grid disabled by default" (-not [bool]$adaptiveConfig.enabled) "newmap_adaptive_support_grid_config.json"
}
else {
    Add-Check "Adaptive support grid disabled by default" $false $adaptiveConfigPath
}

$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$player = Get-Content -Encoding UTF8 -LiteralPath $playerPath -Raw
$npc = Get-Content -Encoding UTF8 -LiteralPath $npcPath -Raw
Add-Check "Runtime creates visible ground cover" ($bootstrap -match "EnsureGameplayGroundCover" -and $bootstrap -match "GameplayGroundCoverRoot" -and $bootstrap -match "renderer\.enabled = config\.rendererEnabledInNormalMode") "NewMapRuntimeBootstrap"
Add-Check "Ground cover colliders are runtime blocking colliders" ($bootstrap -match "collider\.enabled = config\.colliderEnabled" -and $bootstrap -match "collider\.isTrigger = false") "NewMapRuntimeBootstrap"
Add-Check "Support/debug blue renderers hidden" ($bootstrap -match "IsLargeBlueGroundSurfaceCandidate" -and $bootstrap -match "LastVisibleLargeBlueGroundRendererCount") "NewMapRuntimeBootstrap"
Add-Check "Player fall recovery exists without warning spam" ($player -match "ConfigureGroundSafety" -and $player -match "RecoverIfFalling" -and $player -notmatch 'Debug\.LogWarning\("NewMap player fall recovery') "NewMapPlayerController"
Add-Check "NPC uses cover/support fallback Y" ($npc -match "snapped = new Vector3\(candidate\.x, Mathf\.Clamp\(fallbackY") "NewMapNpcCrowdPrototype"
Add-Check "Runtime does not use web APIs for labels" ($bootstrap -notmatch "UnityWebRequest|HttpClient|WebRequest" -and $player -notmatch "UnityWebRequest|HttpClient|WebRequest") "runtime no-web"

if (Test-Path -LiteralPath $readinessPath -PathType Leaf) {
    $readiness = Get-Content -Encoding UTF8 -LiteralPath $readinessPath -Raw | ConvertFrom-Json
    $allowed = @("ready_for_manual_playtest", "ready_with_documented_ground_cover_limitations", "needs_quick_fix_before_manual_test", "blocked")
    Add-Check "Manual readiness decision exists" ($allowed -contains [string]$readiness.manualReadinessDecision) ([string]$readiness.manualReadinessDecision)
    Add-Check "Manual readiness does not claim GIS-grade road/terrain accuracy" (-not [bool]$readiness.roadTerrainAccuracyClaimed) "roadTerrainAccuracyClaimed=false"
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

Write-Host "NewMap ground cover preflight"
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
