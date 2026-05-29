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
$setupPath = Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$playerPath = Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs"
$npcPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"
$labelPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNameLabelController.cs"
$readinessPath = Join-Path $root "Assets\Data\P10\newmap_manual_playtest_readiness.json"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Chuo_BaseMap scene exists" (Test-Path -LiteralPath $baseScene -PathType Leaf) $baseScene
Add-Check "Airwall/NPC/label build method exists" ([bool](Select-String -LiteralPath $setupPath -Pattern "BuildNewMapAirwallNpcLabelPlayerCommandLine" -SimpleMatch -Quiet)) "NewMapSceneSetupUtility"

Invoke-ToolCheck "Airwall/NPC/label JSON validation" "tools\map\validate_newmap_airwall_npc_label_json.ps1"
Invoke-ToolCheck "Unexpected airwall validation" "tools\map\check_newmap_unexpected_airwalls.ps1"
Invoke-ToolCheck "Player-NPC collision validation" "tools\map\check_newmap_player_npc_collision.ps1"
Invoke-ToolCheck "Expanded name enrichment validation" "tools\map\check_newmap_name_enrichment_expanded.ps1"
Invoke-ToolCheck "Runtime no-web validation" "tools\map\check_newmap_runtime_no_web_requests.ps1"

$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$player = Get-Content -Encoding UTF8 -LiteralPath $playerPath -Raw
$npc = Get-Content -Encoding UTF8 -LiteralPath $npcPath -Raw
$labels = Get-Content -Encoding UTF8 -LiteralPath $labelPath -Raw

Add-Check "Airwall runtime diagnostics logged" ($bootstrap -match "airwallHardCollidersScanned" -and $bootstrap -match "sampledValidPathsPassable") "NewMapRuntimeBootstrap"
Add-Check "Boundary air walls preserved" ($bootstrap -match "AirWall_North" -and $bootstrap -match "AirWall_South" -and $bootstrap -match "AirWall_East" -and $bootstrap -match "AirWall_West") "NewMapRuntimeBootstrap"
Add-Check "Player uses NPC soft blocking" ($player -match "ConfigurePlayerNpcCollision" -and $player -match "ApplyPlayerNpcCollisionCorrection") "NewMapPlayerController"
Add-Check "NPC exposes soft body collision" ($npc -match "ResolvePlayerPositionAgainstNpcs" -and $npc -match "CapsuleCollider" -and $npc -match "isTrigger = true") "NewMapNpcCrowdPrototype"
Add-Check "Runtime label source remains offline" ($labels -match "NewMapNameCache.Load" -and $labels -match "RuntimeNetworkRequestsAllowed => false" -and $labels -notmatch "UnityWebRequest|HttpClient|WebRequest|System\.Net|Nominatim|https?://") "NewMapNameLabelController"

if (Test-Path -LiteralPath $readinessPath -PathType Leaf) {
    $readiness = Get-Content -Encoding UTF8 -LiteralPath $readinessPath -Raw | ConvertFrom-Json
    $allowed = @("ready_for_manual_playtest", "ready_with_documented_label_or_collision_limitations", "ready_with_documented_building_visual_limitations", "needs_quick_fix_before_manual_test", "blocked")
    Add-Check "Manual readiness decision exists" ($allowed -contains [string]$readiness.manualReadinessDecision) ([string]$readiness.manualReadinessDecision)
}
else {
    Add-Check "Manual readiness decision exists" $false $readinessPath
}

$archiveFiles = @()
foreach ($pattern in @("*.zip", "*.7z", "*.tar", "*.gz", "*.tgz")) {
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

Write-Host "NewMap airwall/NPC/label fix preflight"
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
