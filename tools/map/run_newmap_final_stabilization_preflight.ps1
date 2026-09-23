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
    $sceneSize = (Get-Item -LiteralPath $scenePath).Length
    Add-Check "Chuo_BaseMap is active baseline asset" ($sceneSize -gt 1GB) "$sceneSize bytes"
}

$runtimeConstantsPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs"
$runtimeConstants = Get-Content -LiteralPath $runtimeConstantsPath -Raw
Add-Check "Runtime active scene constant" ($runtimeConstants -match "Assets/Scenes/Chuo_BaseMap\.unity") "NewMapRuntimeConstants.ScenePath"

$readinessJson = & (Join-Path $root "tools\map\check_plateau_sdk_readiness.ps1") -ProjectRoot $root -AsJson -NoExitCode | ConvertFrom-Json
Add-Check "Unity project valid" ([bool]$readinessJson.UnityProjectValid) $root
Add-Check "PLATEAU SDK ready" ([bool]$readinessJson.Ready) $readinessJson.PlateauManifestValue
Add-Check "Required scene roots" (@($readinessJson.MissingSceneRoots).Count -eq 0) "Missing: $(@($readinessJson.MissingSceneRoots) -join ', ')"

& (Join-Path $root "tools\map\validate_newmap_final_json.ps1") -ProjectRoot $root
Add-Check "Final JSON validation" ($LASTEXITCODE -eq 0) "validate_newmap_final_json.ps1"

& (Join-Path $root "tools\map\check_newmap_disabled_targets.ps1") -ProjectRoot $root
Add-Check "Disabled target validation" ($LASTEXITCODE -eq 0) "check_newmap_disabled_targets.ps1"

& (Join-Path $root "tools\map\check_newmap_no_old_map_active_refs.ps1") -ProjectRoot $root
Add-Check "No deprecated active map refs" ($LASTEXITCODE -eq 0) "check_newmap_no_old_map_active_refs.ps1"

& (Join-Path $root "tools\map\check_newmap_completion_matrix.ps1") -ProjectRoot $root
Add-Check "Completion matrix validation" ($LASTEXITCODE -eq 0) "check_newmap_completion_matrix.ps1"

$targetReportPath = Join-Path $root "Assets\Data\P10\newmap_active_target_final_report.json"
if (Test-Path -LiteralPath $targetReportPath -PathType Leaf) {
    $targetReport = Get-Content -LiteralPath $targetReportPath -Raw | ConvertFrom-Json
    Add-Check "Active target count > 0" ([int]$targetReport.activeTargetCount -gt 0) "active=$($targetReport.activeTargetCount)"
    Add-Check "Official absence documented if none active" (([int]$targetReport.activeOfficialShelterCount -gt 0) -or -not [string]::IsNullOrWhiteSpace($targetReport.officialShelterAbsenceReason)) $targetReport.officialShelterAbsenceReason
}
else {
    Add-Check "Active target report exists" $false $targetReportPath
}

$groundingPath = Join-Path $root "Assets\Data\P10\newmap_player_camera_grounding_final.json"
if (Test-Path -LiteralPath $groundingPath -PathType Leaf) {
    $grounding = Get-Content -LiteralPath $groundingPath -Raw | ConvertFrom-Json
    Add-Check "PlayerSpawnRoot used" ([bool]$grounding.playerSpawnRootUsed) "playerSpawnRootUsed=$($grounding.playerSpawnRootUsed)"
    Add-Check "Active camera documented" ([bool]$grounding.activeCamera) "activeCamera=$($grounding.activeCamera)"
}
else {
    Add-Check "Player/camera/grounding report exists" $false $groundingPath
}

$performancePath = Join-Path $root "Assets\Data\P10\newmap_performance_final_gate.json"
Add-Check "Performance report exists or scheduled" (Test-Path -LiteralPath $performancePath -PathType Leaf) $performancePath

$archiveArtifacts = @(Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object {
        $_.Name -match "\.(zip|7z|tar|tar\.gz)$" -and
        $_.FullName -notmatch "\\data_pipeline\\downloads\\" -and
        $_.FullName -notmatch "\\\.venv\\" -and
        $_.FullName -notmatch "\\Library\\" -and
        $_.FullName -notmatch "\\\.git\\"
    })
Add-Check "No final release/archive artifact" ($archiveArtifacts.Count -eq 0) (($archiveArtifacts | Select-Object -First 5 -ExpandProperty FullName) -join ", ")

$p10Forbidden = @(Get-ChildItem -LiteralPath (Join-Path $root "docs") -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "P10[-_](E|F|G)([-_.]|$)" })
Add-Check "No P10-E/F/G docs" ($p10Forbidden.Count -eq 0) ($p10Forbidden.Name -join ", ")

$cached = @(git -C $root diff --cached --name-only)
$stagedBuilds = @($cached | Where-Object { $_ -match "Builds|NewMapFinalPre|\.exe$|\.zip$|\.7z$" })
Add-Check "No build artifacts staged" ($stagedBuilds.Count -eq 0) ($stagedBuilds -join ", ")

Write-Host "NewMap final stabilization preflight"
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
