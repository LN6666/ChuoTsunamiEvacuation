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
    Add-Check "Chuo_BaseMap size" ($sceneSize -gt 1GB) "$sceneSize bytes"
}

$readinessJson = & (Join-Path $root "tools\map\check_plateau_sdk_readiness.ps1") -ProjectRoot $root -AsJson -NoExitCode | ConvertFrom-Json
Add-Check "Unity project valid" ([bool]$readinessJson.UnityProjectValid) $root
Add-Check "PLATEAU SDK ready" ([bool]$readinessJson.Ready) $readinessJson.PlateauManifestValue
Add-Check "Required scene roots" (@($readinessJson.MissingSceneRoots).Count -eq 0) "Missing: $(@($readinessJson.MissingSceneRoots) -join ', ')"

& (Join-Path $root "tools\map\validate_newmap_integration_json.ps1") -ProjectRoot $root
Add-Check "Integration JSON validation" ($LASTEXITCODE -eq 0) "validate_newmap_integration_json.ps1"

& (Join-Path $root "tools\map\check_disabled_missing_targets.ps1") -ProjectRoot $root
Add-Check "Disabled missing target validation" ($LASTEXITCODE -eq 0) "check_disabled_missing_targets.ps1"

& (Join-Path $root "tools\map\check_no_old_map_active_refs.ps1") -ProjectRoot $root
Add-Check "No old active map refs" ($LASTEXITCODE -eq 0) "check_no_old_map_active_refs.ps1"

$requiredDocs = @(
    "docs\NEWMAP_FULL_INTEGRATION_AUDIT.md",
    "docs\NEWMAP_CHUO_BASEMAP_SCENE_ROOTS.md",
    "docs\NEWMAP_PLAYER_CAMERA_GROUNDING.md",
    "docs\NEWMAP_TOURISM_AND_EVACUATION_MODES.md",
    "docs\GAME_RULES_EN.md",
    "docs\GAME_RULES_JA.md",
    "docs\NEWMAP_UI_LOCALIZATION_CHECK.md",
    "docs\NEWMAP_HUMANOID_VISUALS.md",
    "docs\NEWMAP_TARGET_REMAP_STATUS.md",
    "docs\NEWMAP_GREEN_FRAMES_AND_TARGET_MARKERS.md",
    "docs\NEWMAP_P5_ROUTE_CANDIDATE_STATUS.md",
    "docs\NEWMAP_P6_NAV_NPC_STATUS.md",
    "docs\NEWMAP_P8_HAZARD_FRONT_STATUS.md",
    "docs\NEWMAP_P9_GAMEPLAY_STATUS.md",
    "docs\NEWMAP_COLLAPSE_DEBRIS_STATUS.md",
    "docs\NEWMAP_PERFORMANCE_RESULTS.md",
    "docs\NEWMAP_P2_P10_FULL_COMPLETION_MATRIX.md",
    "docs\NEWMAP_MANUAL_PLAYTEST_CHECKLIST.md"
)
foreach ($relativePath in $requiredDocs) {
    Add-Check "Required doc $relativePath" (Test-Path -LiteralPath (Join-Path $root $relativePath) -PathType Leaf) $relativePath
}

$p10Forbidden = @(Get-ChildItem -LiteralPath (Join-Path $root "docs") -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "P10[-_](E|F|G)([-_.]|$)" })
Add-Check "No P10-E/F/G docs" ($p10Forbidden.Count -eq 0) ($p10Forbidden.Name -join ", ")

$archiveArtifacts = @(Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object {
        $_.Name -match "\.(zip|7z|tar|tar\.gz)$" -and
        $_.FullName -notmatch "\\data_pipeline\\downloads\\" -and
        $_.FullName -notmatch "\\\.venv\\" -and
        $_.FullName -notmatch "\\Library\\" -and
        $_.FullName -notmatch "\\\.git\\"
    })
$archiveDetail = (($archiveArtifacts | Select-Object -First 5 -ExpandProperty FullName) -join ", ")
Add-Check "No final release/archive artifact" ($archiveArtifacts.Count -eq 0) $archiveDetail

$cached = @(git -C $root diff --cached --name-only)
$stagedBuilds = @($cached | Where-Object { $_ -match "Builds|NewMapPre|\.exe$|\.zip$|\.7z$" })
Add-Check "No build artifacts staged" ($stagedBuilds.Count -eq 0) ($stagedBuilds -join ", ")

Write-Host "NewMap full integration preflight"
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
