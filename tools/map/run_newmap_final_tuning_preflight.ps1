param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$script:Checks = @()
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

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

function Read-JsonOrNull {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return $null
    }
    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
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

$scene = Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity"
$setup = Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Chuo_BaseMap exists" (Test-Path -LiteralPath $scene -PathType Leaf) $scene
Add-Check "Final tuning build method exists" ([bool](Select-String -LiteralPath $setup -Pattern "BuildNewMapFinalTuningPlayerCommandLine" -SimpleMatch -Quiet)) "NewMapSceneSetupUtility"
Add-Check "No stale build scene target" (-not [bool](Select-String -LiteralPath $setup -Pattern "Chuo_GroundRoad_Import_Source" -SimpleMatch -Quiet)) "Build scene target remains Chuo_BaseMap"

Invoke-ToolCheck "Final tuning JSON validation" "tools\map\validate_newmap_final_tuning_json.ps1"
Invoke-ToolCheck "Spawn final safety validation" "tools\map\check_newmap_spawn_final_safety.ps1"
Invoke-ToolCheck "Collision final refinement validation" "tools\map\check_newmap_collision_final_refinement.ps1"
Invoke-ToolCheck "Stamina/sprint/warning validation" "tools\map\check_newmap_stamina_sprint_warning_tuning.ps1"

$spawnReport = Read-JsonOrNull "Assets\Data\P10\newmap_spawn_final_safety_report.json"
$collisionReport = Read-JsonOrNull "Assets\Data\P10\newmap_building_collision_final_refinement_report.json"
$readiness = Read-JsonOrNull "Assets\Data\P10\newmap_manual_playtest_readiness.json"
Add-Check "Spawn final safety report exists" ($null -ne $spawnReport) "Assets\Data\P10\newmap_spawn_final_safety_report.json"
Add-Check "Collision final refinement report exists" ($null -ne $collisionReport) "Assets\Data\P10\newmap_building_collision_final_refinement_report.json"
$allowedReadiness = @("ready_for_manual_playtest", "ready_with_documented_visual_limitations", "needs_quick_fix_before_manual_test", "blocked")
Add-Check "Manual readiness decision exists" ($readiness -and ($allowedReadiness -contains [string]$readiness.manualReadinessDecision)) ($readiness.manualReadinessDecision)

$archiveFiles = @()
foreach ($pattern in @("*.zip", "*.7z", "*.tar", "*.gz", "*.tgz")) {
    $archiveFiles += @(Get-ChildItem -LiteralPath $root -File -Filter $pattern -Force -ErrorAction SilentlyContinue)
}
$releaseLikeDirectories = @(Get-ChildItem -LiteralPath $root -Directory -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)(^|[-_])(release|archive)([-_]|$)"
})
Add-Check "No final release/archive artifacts" ($archiveFiles.Count -eq 0 -and $releaseLikeDirectories.Count -eq 0) (($archiveFiles.Name + $releaseLikeDirectories.Name) -join ", ")

$scanRoots = @(
    (Join-Path $root "docs"),
    (Join-Path $root "tools"),
    (Join-Path $root "Assets\Data"),
    (Join-Path $root "Assets\Scripts"),
    (Join-Path $root "codex_prompts")
)
$rootFileMatches = @(Get-ChildItem -LiteralPath $root -File -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)\bP10[-_](E|F|G)([-_.]|$)"
} | ForEach-Object { $_.FullName })
$forbiddenP10 = @($rootFileMatches + @(Get-NameMatches -ScanRoots $scanRoots -Pattern "(?i)\bP10[-_](E|F|G)([-_.]|$)"))
Add-Check "No P10-E/F/G artifacts" ($forbiddenP10.Count -eq 0) ($forbiddenP10 -join ", ")

Write-Host "NewMap final tuning preflight"
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
