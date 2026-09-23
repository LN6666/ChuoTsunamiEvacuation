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

& (Join-Path $root "tools\map\validate_newmap_visual_round2_json.ps1") -ProjectRoot $root
Add-Check "Round-2 JSON validation" ($LASTEXITCODE -eq 0) "validate_newmap_visual_round2_json.ps1"

& (Join-Path $root "tools\map\check_newmap_mouse_drag_look.ps1") -ProjectRoot $root
Add-Check "Mouse drag-look" ($LASTEXITCODE -eq 0) "check_newmap_mouse_drag_look.ps1"

& (Join-Path $root "tools\map\check_newmap_ground_height_alignment.ps1") -ProjectRoot $root
Add-Check "Ground height alignment" ($LASTEXITCODE -eq 0) "check_newmap_ground_height_alignment.ps1"

& (Join-Path $root "tools\map\check_newmap_night_lighting.ps1") -ProjectRoot $root
Add-Check "Night lighting" ($LASTEXITCODE -eq 0) "check_newmap_night_lighting.ps1"

& (Join-Path $root "tools\map\check_newmap_materials_round2.ps1") -ProjectRoot $root
Add-Check "Material audit" ($LASTEXITCODE -eq 0) "check_newmap_materials_round2.ps1"

$debug = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_debug_cleanup_round2.json") -Raw | ConvertFrom-Json
Add-Check "Debug cleanup report" ([bool]$debug.debugDiagnosticsRootInactiveByDefault -and -not [bool]$debug.localTrainingProxyTargetsProductionVisible) "DebugDiagnosticsRoot inactive; proxies hidden"

$forbiddenDocs = @(Get-ChildItem -LiteralPath (Join-Path $root "docs") -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match "P10[-_](E|F|G)([-_.]|$)" })
Add-Check "No P10-E/F/G docs" ($forbiddenDocs.Count -eq 0) ($forbiddenDocs.Name -join ", ")

$oldFailedScenes = @(Get-ChildItem -LiteralPath (Join-Path $root "Assets") -Filter "InitTestScene*.unity" -File -ErrorAction SilentlyContinue)
Add-Check "No old failed init test scenes" ($oldFailedScenes.Count -eq 0) ($oldFailedScenes.Name -join ", ")

$archives = @(Get-ChildItem -LiteralPath $root -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match "\.(zip|7z|tar|gz)$" })
Add-Check "No root release archive" ($archives.Count -eq 0) ($archives.Name -join ", ")

Write-Host "NewMap visual round-2 preflight"
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
