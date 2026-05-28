param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$script:Checks = @()
function Add-Check {
    param([string]$Name, [bool]$Passed, [string]$Detail)
    $script:Checks += [pscustomobject]@{ Name = $Name; Passed = $Passed; Detail = $Detail }
}

$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$scenePath = Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Chuo_BaseMap scene exists" (Test-Path -LiteralPath $scenePath -PathType Leaf) $scenePath
Add-Check "Chuo_BaseMap active scene path referenced" ([bool](Select-String -LiteralPath (Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs") -Pattern 'Assets/Scenes/Chuo_BaseMap.unity' -SimpleMatch -Quiet)) "NewMapSceneSetupUtility"
Add-Check "Spawn/mouse build method present" ([bool](Select-String -LiteralPath (Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs") -Pattern 'BuildNewMapSpawnMouseFixPlayerCommandLine' -SimpleMatch -Quiet)) "BuildNewMapSpawnMouseFixPlayerCommandLine"

& (Join-Path $root "tools\map\validate_newmap_spawn_mouse_fix_json.ps1") -ProjectRoot $root
Add-Check "Spawn/mouse JSON validation" ($LASTEXITCODE -eq 0) "validate_newmap_spawn_mouse_fix_json.ps1"

& (Join-Path $root "tools\map\check_newmap_mouse_drag_buttons.ps1") -ProjectRoot $root
Add-Check "Left/right mouse drag-look" ($LASTEXITCODE -eq 0) "check_newmap_mouse_drag_buttons.ps1"

& (Join-Path $root "tools\map\check_newmap_spawn_not_inside_building.ps1") -ProjectRoot $root
Add-Check "Spawn not inside building" ($LASTEXITCODE -eq 0) "check_newmap_spawn_not_inside_building.ps1"

$forbiddenDocs = @(Get-ChildItem -LiteralPath (Join-Path $root "docs") -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match "P10[-_](E|F|G)([-_.]|$)" })
Add-Check "No P10-E/F/G docs" ($forbiddenDocs.Count -eq 0) ($forbiddenDocs.Name -join ", ")

$oldFailedScenes = @(Get-ChildItem -LiteralPath (Join-Path $root "Assets") -Filter "InitTestScene*.unity" -File -ErrorAction SilentlyContinue)
Add-Check "No old failed init test scenes" ($oldFailedScenes.Count -eq 0) ($oldFailedScenes.Name -join ", ")

$archives = @(Get-ChildItem -LiteralPath $root -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match "\.(zip|7z|tar|gz)$" })
Add-Check "No root release archive" ($archives.Count -eq 0) ($archives.Name -join ", ")

$latestLog = $null
$localLow = Join-Path $env:USERPROFILE "AppData\LocalLow"
if (Test-Path -LiteralPath $localLow -PathType Container) {
    $latestLog = Get-ChildItem -LiteralPath $localLow -Recurse -Filter Player.log -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
}

if ($latestLog) {
    $logText = Get-Content -LiteralPath $latestLog.FullName -Raw -ErrorAction SilentlyContinue
    Add-Check "Latest Player.log has no errors" ($logText -notmatch "Exception|Error|NullReferenceException") $latestLog.FullName
}
else {
    Add-Check "Latest Player.log optional before build smoke" $true "not found before new smoke run"
}

Write-Host "NewMap spawn/mouse-fix preflight"
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
