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
$scenePath = Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity"

Add-Check "Workspace" ((Resolve-Path -LiteralPath (Get-Location).Path).Path -ieq $root) "Current=$(Get-Location) Expected=$root"
Add-Check "Chuo_BaseMap exists" (Test-Path -LiteralPath $scenePath -PathType Leaf) $scenePath
if (Test-Path -LiteralPath $scenePath -PathType Leaf) {
    Add-Check "Chuo_BaseMap is large local scene" ((Get-Item -LiteralPath $scenePath).Length -gt 1GB) "$((Get-Item -LiteralPath $scenePath).Length) bytes"
}

$runtimeConstantsPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs"
$runtimeConstants = Get-Content -LiteralPath $runtimeConstantsPath -Raw
Add-Check "Runtime active scene constant" ($runtimeConstants -match "Assets/Scenes/Chuo_BaseMap\.unity") "NewMapRuntimeConstants.ScenePath"

& (Join-Path $root "tools\map\validate_newmap_official_route_json.ps1") -ProjectRoot $root
Add-Check "Official route JSON validation" ($LASTEXITCODE -eq 0) "validate_newmap_official_route_json.ps1"

& (Join-Path $root "tools\map\check_newmap_official_shelter_anchors.ps1") -ProjectRoot $root
Add-Check "Official shelter anchor validation" ($LASTEXITCODE -eq 0) "check_newmap_official_shelter_anchors.ps1"

& (Join-Path $root "tools\map\check_newmap_route_geometry_validation.ps1") -ProjectRoot $root
Add-Check "Route geometry validation" ($LASTEXITCODE -eq 0) "check_newmap_route_geometry_validation.ps1"

& (Join-Path $root "tools\map\check_newmap_final_matrix_status.ps1") -ProjectRoot $root
Add-Check "Strict matrix validation" ($LASTEXITCODE -eq 0) "check_newmap_final_matrix_status.ps1"

$activeReportPath = Join-Path $root "Assets\Data\P10\newmap_active_target_final_report.json"
if (Test-Path -LiteralPath $activeReportPath -PathType Leaf) {
    $activeReport = Get-Content -LiteralPath $activeReportPath -Raw | ConvertFrom-Json
    Add-Check "Active target flow exists" ([int]$activeReport.activeTargetCount -ge 1) "active=$($activeReport.activeTargetCount)"
    Add-Check "Official shelters are strict or absent" (
        ([int]$activeReport.activeOfficialShelterCount -ge 1 -and $activeReport.officialShelterStatus -eq "official_shelters_recovered") -or
        ([int]$activeReport.activeOfficialShelterCount -eq 0)
    ) "activeOfficial=$($activeReport.activeOfficialShelterCount) status=$($activeReport.officialShelterStatus)"
    Add-Check "No non-official official label" ([int]$activeReport.nonOfficialTargetLabeledOfficialCount -eq 0) "count=$($activeReport.nonOfficialTargetLabeledOfficialCount)"
    Add-Check "No disabled selectable targets" ([int]$activeReport.disabledSelectableCount -eq 0) "count=$($activeReport.disabledSelectableCount)"
}
else {
    Add-Check "Active target report exists" $false $activeReportPath
}

$routeReportPath = Join-Path $root "Assets\Data\P10\newmap_route_geometry_validation.json"
if (Test-Path -LiteralPath $routeReportPath -PathType Leaf) {
    $routeReport = Get-Content -LiteralPath $routeReportPath -Raw | ConvertFrom-Json
    Add-Check "No official route false claim" ([int]$routeReport.officialRouteClaimCount -eq 0) "officialRouteClaimCount=$($routeReport.officialRouteClaimCount)"
}

$forbiddenDocs = @(Get-ChildItem -LiteralPath (Join-Path $root "docs") -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "P10[-_](E|F|G)([-_.]|$)" })
Add-Check "No P10-E/F/G docs" ($forbiddenDocs.Count -eq 0) ($forbiddenDocs.Name -join ", ")

$archives = @(Get-ChildItem -LiteralPath $root -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "\.(zip|7z|tar|gz)$" })
Add-Check "No root release/archive artifact" ($archives.Count -eq 0) ($archives.Name -join ", ")

$cached = @(git -C $root diff --cached --name-only)
$stagedBuilds = @($cached | Where-Object { $_ -match "Builds|NewMapHardeningPre2|\.exe$|\.zip$|\.7z$" })
Add-Check "No build artifacts staged" ($stagedBuilds.Count -eq 0) ($stagedBuilds -join ", ")

Write-Host "NewMap official shelter / route hardening preflight"
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
