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

Add-Check "Workspace" ((Resolve-Path -LiteralPath (Get-Location).Path).Path -ieq $root) "Current=$(Get-Location) Expected=$root"
Add-Check "Chuo_BaseMap scene exists" (Test-Path -LiteralPath $scenePath -PathType Leaf) $scenePath

$runtimeTypes = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs") -Raw
Add-Check "Active scene constant is Chuo_BaseMap" ($runtimeTypes -match "Assets/Scenes/Chuo_BaseMap\.unity") "NewMapRuntimeConstants.ScenePath"

& (Join-Path $root "tools\map\validate_newmap_gameplay_self_audit_json.ps1") -ProjectRoot $root
Add-Check "Gameplay self-audit JSON validates" ($LASTEXITCODE -eq 0) "validate_newmap_gameplay_self_audit_json.ps1"

& (Join-Path $root "tools\map\check_newmap_gameplay_reachability.ps1") -ProjectRoot $root
Add-Check "Gameplay reachability matrix" ($LASTEXITCODE -eq 0) "check_newmap_gameplay_reachability.ps1"

& (Join-Path $root "tools\map\check_newmap_disabled_targets_not_selectable.ps1") -ProjectRoot $root
Add-Check "Disabled targets not selectable" ($LASTEXITCODE -eq 0) "check_newmap_disabled_targets_not_selectable.ps1"

& (Join-Path $root "tools\map\check_newmap_no_false_official_nonofficial_claims.ps1") -ProjectRoot $root
Add-Check "No false official/non-official claims" ($LASTEXITCODE -eq 0) "check_newmap_no_false_official_nonofficial_claims.ps1"

& (Join-Path $root "tools\map\check_newmap_no_false_route_claims.ps1") -ProjectRoot $root
Add-Check "No false route claims" ($LASTEXITCODE -eq 0) "check_newmap_no_false_route_claims.ps1"

& (Join-Path $root "tools\map\check_newmap_no_old_map_active_refs.ps1") -ProjectRoot $root
Add-Check "No old map active refs" ($LASTEXITCODE -eq 0) "check_newmap_no_old_map_active_refs.ps1"

$matrix = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_gameplay_self_audit_matrix.json") -Raw | ConvertFrom-Json
Add-Check "Official target flow represented" ([int]$matrix.activeOfficialShelterCount -ge 1) "activeOfficial=$($matrix.activeOfficialShelterCount)"
Add-Check "Non-official target flow represented" ([int]$matrix.activeNonOfficialTrainingTargetCount -ge 1) "activeNonOfficial=$($matrix.activeNonOfficialTrainingTargetCount)"

$ui = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_ui_ux_runtime_check.json") -Raw | ConvertFrom-Json
Add-Check "ResultPanel flow represented" ([bool]$ui.resultPanelReadable) "resultPanelReadable=$($ui.resultPanelReadable)"

$mode = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_mode_gameplay_check.json") -Raw | ConvertFrom-Json
Add-Check "Tourism Mode represented" ($mode.tourism.tsunamiWarning -eq "disabled") "tourism.tsunamiWarning=$($mode.tourism.tsunamiWarning)"
Add-Check "Evacuation Mode represented" ($mode.evacuation.twoStageTsunami -eq "enabled") "evacuation.twoStageTsunami=$($mode.evacuation.twoStageTsunami)"

$releaseArchives = @(Get-ChildItem -LiteralPath $root -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match "\.(zip|7z|tar|gz)$" })
Add-Check "No root release/archive artifact" ($releaseArchives.Count -eq 0) ($releaseArchives.Name -join ", ")

$p10Efg = @(Get-ChildItem -LiteralPath (Join-Path $root "docs") -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match "P10[-_](E|F|G)([-_.]|$)" })
Add-Check "No P10-E/F/G docs" ($p10Efg.Count -eq 0) ($p10Efg.Name -join ", ")

$cached = @(git -C $root diff --cached --name-only)
$stagedBuilds = @($cached | Where-Object { $_ -match "Builds|NewMapGameplaySelfAuditPre|\.exe$|\.zip$|\.7z$" })
Add-Check "No build artifacts staged" ($stagedBuilds.Count -eq 0) ($stagedBuilds -join ", ")

Write-Host "NewMap gameplay self-audit preflight"
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
