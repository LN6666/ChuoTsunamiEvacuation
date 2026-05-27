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
Add-Check "Chuo_BaseMap active scene asset exists" (Test-Path -LiteralPath $scenePath -PathType Leaf) $scenePath
if (Test-Path -LiteralPath $scenePath -PathType Leaf) {
    Add-Check "Chuo_BaseMap is local generated scene" ((Get-Item -LiteralPath $scenePath).Length -gt 1GB) "$((Get-Item -LiteralPath $scenePath).Length) bytes"
}

$runtimeConstantsPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs"
$runtimeConstants = Get-Content -LiteralPath $runtimeConstantsPath -Raw
Add-Check "Runtime active scene constant" ($runtimeConstants -match "Assets/Scenes/Chuo_BaseMap\.unity") "NewMapRuntimeConstants.ScenePath"

$fullSceneRootCheck = Select-String -LiteralPath $scenePath -Pattern "m_Name: PlayerSpawnRoot" -SimpleMatch -Quiet
Add-Check "PlayerSpawnRoot exists" ([bool]$fullSceneRootCheck) "m_Name: PlayerSpawnRoot"
Add-Check "Runtime camera creation exists" ((Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs") -Raw) -match "Main Camera") "NewMapPlayerController creates/uses Main Camera"

& (Join-Path $root "tools\map\validate_newmap_non_memory_json.ps1") -ProjectRoot $root
Add-Check "Non-memory JSON validation" ($LASTEXITCODE -eq 0) "validate_newmap_non_memory_json.ps1"

& (Join-Path $root "tools\map\check_newmap_official_anchor_fit.ps1") -ProjectRoot $root
Add-Check "Official anchor fit validation" ($LASTEXITCODE -eq 0) "check_newmap_official_anchor_fit.ps1"

& (Join-Path $root "tools\map\check_newmap_route_validation.ps1") -ProjectRoot $root
Add-Check "Route validation" ($LASTEXITCODE -eq 0) "check_newmap_route_validation.ps1"

& (Join-Path $root "tools\map\check_newmap_no_false_route_claims.ps1") -ProjectRoot $root
Add-Check "No false route claims" ($LASTEXITCODE -eq 0) "check_newmap_no_false_route_claims.ps1"

& (Join-Path $root "tools\map\check_newmap_disabled_targets_inactive.ps1") -ProjectRoot $root
Add-Check "Disabled targets inactive" ($LASTEXITCODE -eq 0) "check_newmap_disabled_targets_inactive.ps1"

$activePath = Join-Path $root "Assets\Data\P10\newmap_active_target_final_report.json"
if (Test-Path -LiteralPath $activePath -PathType Leaf) {
    $active = Get-Content -LiteralPath $activePath -Raw | ConvertFrom-Json
    Add-Check "Active target flow exists" ([int]$active.activeTargetCount -gt 0) "active=$($active.activeTargetCount)"
    Add-Check "No unverified official active" ([int]$active.nonOfficialTargetLabeledOfficialCount -eq 0) "nonOfficialTargetLabeledOfficialCount=$($active.nonOfficialTargetLabeledOfficialCount)"
}
else {
    Add-Check "Active target report exists" $false $activePath
}

$forbiddenDocs = @(Get-ChildItem -LiteralPath (Join-Path $root "docs") -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "P10[-_](E|F|G)([-_.]|$)" })
Add-Check "No P10-E/F/G docs" ($forbiddenDocs.Count -eq 0) ($forbiddenDocs.Name -join ", ")

$archives = @(Get-ChildItem -LiteralPath $root -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "\.(zip|7z|tar|gz)$" })
Add-Check "No root final release/archive artifact" ($archives.Count -eq 0) ($archives.Name -join ", ")

$cached = @(git -C $root diff --cached --name-only)
$stagedBuilds = @($cached | Where-Object { $_ -match "Builds|NewMapNoMemoryFocusPre|\.exe$|\.zip$|\.7z$" })
Add-Check "No build artifacts staged" ($stagedBuilds.Count -eq 0) ($stagedBuilds -join ", ")

Write-Host "NewMap non-memory hardening preflight"
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
