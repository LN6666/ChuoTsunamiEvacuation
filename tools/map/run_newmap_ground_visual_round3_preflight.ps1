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
$setupPath = Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Chuo_BaseMap scene exists" (Test-Path -LiteralPath $scenePath -PathType Leaf) $scenePath
Add-Check "Chuo_BaseMap active scene path referenced" ([bool](Select-String -LiteralPath $setupPath -Pattern 'Assets/Scenes/Chuo_BaseMap.unity' -SimpleMatch -Quiet)) "NewMapSceneSetupUtility"
Add-Check "Round 3 build method present" ([bool](Select-String -LiteralPath $setupPath -Pattern 'BuildNewMapGroundVisualRound3PlayerCommandLine' -SimpleMatch -Quiet)) "BuildNewMapGroundVisualRound3PlayerCommandLine"

& (Join-Path $root "tools\map\validate_newmap_ground_visual_round3_json.ps1") -ProjectRoot $root
Add-Check "Round 3 JSON validation" ($LASTEXITCODE -eq 0) "validate_newmap_ground_visual_round3_json.ps1"

& (Join-Path $root "tools\map\check_newmap_support_surface_visibility.ps1") -ProjectRoot $root
Add-Check "Support surface renderer hidden" ($LASTEXITCODE -eq 0) "check_newmap_support_surface_visibility.ps1"

& (Join-Path $root "tools\map\check_newmap_ground_visual_alignment_round3.ps1") -ProjectRoot $root
Add-Check "Ground alignment and air walls" ($LASTEXITCODE -eq 0) "check_newmap_ground_visual_alignment_round3.ps1"

& (Join-Path $root "tools\map\check_newmap_mouse_drag_buttons.ps1") -ProjectRoot $root
Add-Check "Left/right mouse drag-look preserved" ($LASTEXITCODE -eq 0) "check_newmap_mouse_drag_buttons.ps1"

& (Join-Path $root "tools\map\check_newmap_spawn_not_inside_building.ps1") -ProjectRoot $root
Add-Check "Spawn building rejection preserved" ($LASTEXITCODE -eq 0) "check_newmap_spawn_not_inside_building.ps1"

$labelSource = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapNameLabelController.cs") -Raw
Add-Check "Runtime labels have no web request code" ($labelSource -notmatch "UnityWebRequest|HttpClient|WebRequest|nominatim|http://|https://") "offline runtime labels only"
Add-Check "Name enrichment tool exists" (Test-Path -LiteralPath (Join-Path $root "tools\map\enrich_newmap_names_from_coordinates.py") -PathType Leaf) "preprocessing-only coordinate enrichment"

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

Write-Host "NewMap Ground Visual Round 3 preflight"
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
