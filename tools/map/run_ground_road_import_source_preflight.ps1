param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"

function Add-Check {
    param([string]$Name, [bool]$Passed, [string]$Detail)
    $script:Checks += [pscustomobject]@{ Name = $Name; Passed = $Passed; Detail = $Detail }
}

$script:Checks = @()

$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$sourceScene = Join-Path $root "Assets\Scenes\Chuo_GroundRoad_Import_Source.unity"
$baseScene = Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity"
$sourceDoc = Join-Path $root "docs\NEWMAP_GROUND_ROAD_IMPORT_SOURCE_SCENE.md"
$manualDoc = Join-Path $root "docs\NEWMAP_GROUND_ROAD_MANUAL_IMPORT_GUIDE.md"
$checkScript = Join-Path $root "tools\map\check_ground_road_import_source_scene.ps1"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Source scene exists" (Test-Path -LiteralPath $sourceScene -PathType Leaf) $sourceScene
Add-Check "Chuo_BaseMap still exists" (Test-Path -LiteralPath $baseScene -PathType Leaf) $baseScene
Add-Check "Import source doc exists" (Test-Path -LiteralPath $sourceDoc -PathType Leaf) $sourceDoc
Add-Check "Manual import guide exists" (Test-Path -LiteralPath $manualDoc -PathType Leaf) $manualDoc
Add-Check "Scene check script exists" (Test-Path -LiteralPath $checkScript -PathType Leaf) $checkScript

if (Test-Path -LiteralPath $checkScript -PathType Leaf) {
    & $checkScript -ProjectRoot $root
    Add-Check "Ground/road import source scene check" ($LASTEXITCODE -eq 0) "check_ground_road_import_source_scene.ps1"
}

Write-Host "Ground/road import source preflight"
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
