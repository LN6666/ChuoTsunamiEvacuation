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
    Add-Check "Chuo_BaseMap is large local active scene" ((Get-Item -LiteralPath $scenePath).Length -gt 1GB) "$((Get-Item -LiteralPath $scenePath).Length) bytes"
}

$runtimeConstantsPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs"
$runtimeConstants = Get-Content -LiteralPath $runtimeConstantsPath -Raw
Add-Check "Runtime active scene constant" ($runtimeConstants -match "Assets/Scenes/Chuo_BaseMap\.unity") "NewMapRuntimeConstants.ScenePath"

& (Join-Path $root "tools\map\validate_newmap_completion_json.ps1") -ProjectRoot $root
Add-Check "Hardening JSON validation" ($LASTEXITCODE -eq 0) "validate_newmap_completion_json.ps1"

& (Join-Path $root "tools\map\check_newmap_disabled_targets_hard.ps1") -ProjectRoot $root
Add-Check "Disabled targets hard validation" ($LASTEXITCODE -eq 0) "check_newmap_disabled_targets_hard.ps1"

& (Join-Path $root "tools\map\check_newmap_no_old_map_refs_hard.ps1") -ProjectRoot $root
Add-Check "No deprecated active map refs" ($LASTEXITCODE -eq 0) "check_newmap_no_old_map_refs_hard.ps1"

& (Join-Path $root "tools\map\check_newmap_final_matrix_status.ps1") -ProjectRoot $root
Add-Check "Strict final matrix validation" ($LASTEXITCODE -eq 0) "check_newmap_final_matrix_status.ps1"

$targetReportPath = Join-Path $root "Assets\Data\P10\newmap_active_target_final_report.json"
if (Test-Path -LiteralPath $targetReportPath -PathType Leaf) {
    $targetReport = Get-Content -LiteralPath $targetReportPath -Raw | ConvertFrom-Json
    Add-Check "Active target flow exists" ([int]$targetReport.activeTargetCount -ge 1) "active=$($targetReport.activeTargetCount)"
    Add-Check "No false official activation" ([int]$targetReport.activeOfficialShelterCount -eq 0 -and -not [string]::IsNullOrWhiteSpace($targetReport.officialShelterAbsenceReason)) $targetReport.officialShelterAbsenceReason
}
else {
    Add-Check "Active target report exists" $false $targetReportPath
}

$matrixPath = Join-Path $root "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json"
if (Test-Path -LiteralPath $matrixPath -PathType Leaf) {
    $matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
    Add-Check "Matrix active target count matches runtime report" ([int]$matrix.activeTargetCount -ge 1) "active=$($matrix.activeTargetCount)"
}

$cached = @(git -C $root diff --cached --name-only)
$stagedBuilds = @($cached | Where-Object { $_ -match "Builds|NewMapHardeningPre|\.exe$|\.zip$|\.7z$" })
Add-Check "No build artifacts staged" ($stagedBuilds.Count -eq 0) ($stagedBuilds -join ", ")

Write-Host "NewMap completion hardening preflight"
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
