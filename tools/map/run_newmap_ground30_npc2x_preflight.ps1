param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$checks = @()

function Add-Check {
    param([string]$Name, [bool]$Passed, [string]$Detail = "")
    $script:checks += [pscustomobject]@{ Name = $Name; Passed = $Passed; Detail = $Detail }
}

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { return $null }
    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
}

function Read-Text {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { return "" }
    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw
}

& (Join-Path $PSScriptRoot "validate_newmap_ground30_npc2x_json.ps1") -ProjectRoot $root

$boundary = Read-Json "Assets\Data\P10\newmap_circular_boundary_config.json"
$npcConfig = Read-Json "Assets\Data\P10\newmap_npc_distribution_config.json"
$groundReport = Read-Json "Assets\Data\P10\newmap_ground_raise_30_report.json"
$doubleReport = Read-Json "Assets\Data\P10\newmap_npc_double_count_report.json"
$manual = Read-Json "Assets\Data\P10\newmap_manual_playtest_readiness.json"
$bootstrap = Read-Text "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$npc = Read-Text "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"
$editor = Read-Text "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"

Add-Check "Chuo_BaseMap exists" (Test-Path -LiteralPath (Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity") -PathType Leaf) "Assets/Scenes/Chuo_BaseMap.unity"
Add-Check "Ground raise 30 report exists" ($null -ne $groundReport) "Assets/Data/P10/newmap_ground_raise_30_report.json"
Add-Check "Old/new ground values present" ($groundReport -and $null -ne $groundReport.oldGroundY -and $null -ne $groundReport.newGroundY -and $null -ne $groundReport.oldRaiseOffset -and $null -ne $groundReport.newRaiseOffset) "old=$($groundReport.oldGroundY) new=$($groundReport.newGroundY)"
Add-Check "Ground changed by about 30 percent" ($groundReport -and [math]::Abs([double]$groundReport.actualRaisePercent - 0.30) -le 0.035) "actualRaisePercent=$($groundReport.actualRaisePercent)"
Add-Check "Boundary radius remains 2270" ($boundary -and [double]$boundary.radiusMeters -eq 2270.0) "radiusMeters=$($boundary.radiusMeters)"
Add-Check "NPC distribution radius is 2270" ($npcConfig -and [double]$npcConfig.distributionRadiusMeters -eq 2270.0) "distributionRadiusMeters=$($npcConfig.distributionRadiusMeters)"
Add-Check "NPC count doubled" ($doubleReport -and [int]$doubleReport.previousRequestedCount -eq 800 -and [int]$doubleReport.newRequestedCount -eq 1600 -and [int]$npcConfig.maxNpcCount -eq 1600) "800 -> $($doubleReport.newRequestedCount)"
Add-Check "Cap reason present if capped" ($doubleReport -and ((-not [bool]$doubleReport.capped) -or -not [string]::IsNullOrWhiteSpace([string]$doubleReport.capReason))) "capped=$($doubleReport.capped) reason=$($doubleReport.capReason)"
Add-Check "Runtime player spawn/root setup exists" ($bootstrap -match "PlayerSpawnRoot" -and $bootstrap -match "ResolveSpawnPosition") "NewMapRuntimeBootstrap"
Add-Check "Runtime source has no stale active 1500m/3500m circular boundary" ($bootstrap -notmatch "radiusMeters\s*=\s*1500f|radiusMeters\s*=\s*3500f|disabled_out_of_playable_bounds_1_5km|disabled_out_of_playable_bounds_3_5km" -and $npc -notmatch "distributionRadiusMeters\s*=\s*1000f|distributionRadiusMeters\s*=\s*1500f|distributionRadiusMeters\s*=\s*3500f") "runtime source"
Add-Check "Build method exists" ($editor -match "BuildNewMapGround30Npc2xPlayerCommandLine" -and $editor -match "NewMapGround30Npc2xPre") "NewMapSceneSetupUtility"
Add-Check "Manual readiness decision exists" ($manual -and -not [string]::IsNullOrWhiteSpace([string]$manual.manualReadinessDecision)) "decision=$($manual.manualReadinessDecision)"

$archiveFiles = @()
foreach ($pattern in @("*.zip", "*.7z", "*.tar", "*.gz", "*.tgz")) {
    $archiveFiles += @(Get-ChildItem -LiteralPath $root -File -Filter $pattern -Force -ErrorAction SilentlyContinue)
}
$releaseLikeDirectories = @(Get-ChildItem -LiteralPath $root -Directory -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)(^|[-_])(release|archive)([-_]|$)"
})
Add-Check "No final release/archive artifacts" ($archiveFiles.Count -eq 0 -and $releaseLikeDirectories.Count -eq 0) (($archiveFiles.Name + $releaseLikeDirectories.Name) -join ", ")

$forbiddenP10 = @(Get-ChildItem -LiteralPath (Join-Path $root "Assets\Data") -Recurse -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)\bP10[-_](E|F|G)([-_.]|$)"
})
$forbiddenP10 += @(Get-ChildItem -LiteralPath (Join-Path $root "docs") -Recurse -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)\bP10[-_](E|F|G)([-_.]|$)"
})
Add-Check "No P10-E/F/G artifacts" ($forbiddenP10.Count -eq 0) (($forbiddenP10 | ForEach-Object { $_.FullName }) -join ", ")

Write-Host "NewMap ground30/NPC2x preflight"
foreach ($check in $checks) {
    $label = if ($check.Passed) { "PASS" } else { "FAIL" }
    Write-Host ("[{0}] {1}: {2}" -f $label, $check.Name, $check.Detail)
}

$failures = @($checks | Where-Object { -not $_.Passed })
if ($failures.Count -gt 0) {
    Write-Host "Preflight result: FAIL"
    exit 1
}

Write-Host "Preflight result: PASS"
exit 0
