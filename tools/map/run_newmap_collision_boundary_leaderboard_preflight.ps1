param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$script:Checks = @()

function Add-Check {
    param([string]$Name, [bool]$Passed, [string]$Detail)
    $script:Checks += [pscustomobject]@{ Name = $Name; Passed = $Passed; Detail = $Detail }
}

function Invoke-ToolCheck {
    param([string]$Name, [string]$RelativeScript)
    $scriptPath = Join-Path $root $RelativeScript
    if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
        Add-Check $Name $false "missing $RelativeScript"
        return
    }

    & $scriptPath -ProjectRoot $root
    Add-Check $Name ($LASTEXITCODE -eq 0) $RelativeScript
}

function Get-NameMatches {
    param([string[]]$ScanRoots, [string]$Pattern)
    $matches = @()
    foreach ($scanRoot in $ScanRoots) {
        if (-not (Test-Path -LiteralPath $scanRoot)) {
            continue
        }

        $matches += @(Get-ChildItem -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue | Where-Object {
            $_.Name -match $Pattern
        } | ForEach-Object { $_.FullName })
    }

    return $matches
}

$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$scene = Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity"
$setup = Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"
$manual = Join-Path $root "Assets\Data\P10\newmap_manual_playtest_readiness.json"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Chuo_BaseMap exists" (Test-Path -LiteralPath $scene -PathType Leaf) $scene
Add-Check "Collision/boundary build method exists" ([bool](Select-String -LiteralPath $setup -Pattern "BuildNewMapCollisionBoundaryLeaderboardPlayerCommandLine" -SimpleMatch -Quiet)) "NewMapSceneSetupUtility"

Invoke-ToolCheck "JSON validation" "tools\map\validate_newmap_collision_boundary_leaderboard_json.ps1"
Invoke-ToolCheck "Collision whitelist validation" "tools\map\check_newmap_collision_whitelist.ps1"
Invoke-ToolCheck "Circular boundary validation" "tools\map\check_newmap_circular_boundary.ps1"
Invoke-ToolCheck "R leaderboard validation" "tools\map\check_newmap_r_leaderboard_toggle.ps1"

if (Test-Path -LiteralPath $manual -PathType Leaf) {
    $readiness = Get-Content -Encoding UTF8 -LiteralPath $manual -Raw | ConvertFrom-Json
    $allowed = @("ready_for_manual_playtest", "ready_with_documented_boundary_limitations", "needs_quick_fix_before_manual_test", "blocked")
    Add-Check "Manual readiness decision exists" ($allowed -contains [string]$readiness.manualReadinessDecision) ([string]$readiness.manualReadinessDecision)
}
else {
    Add-Check "Manual readiness decision exists" $false $manual
}

$archiveFiles = @()
foreach ($pattern in @("*.zip", "*.7z", "*.tar", "*.gz", "*.tgz")) {
    $archiveFiles += @(Get-ChildItem -LiteralPath $root -File -Filter $pattern -Force -ErrorAction SilentlyContinue)
}
$releaseLikeDirectories = @(Get-ChildItem -LiteralPath $root -Directory -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)(^|[-_])(release|archive)([-_]|$)"
})
Add-Check "No final release/archive artifacts" ($archiveFiles.Count -eq 0 -and $releaseLikeDirectories.Count -eq 0) (($archiveFiles.Name + $releaseLikeDirectories.Name) -join ", ")

$scanRoots = @(
    (Join-Path $root "docs"),
    (Join-Path $root "tools"),
    (Join-Path $root "Assets\Data"),
    (Join-Path $root "Assets\Scripts"),
    (Join-Path $root "codex_prompts")
)
$rootFileMatches = @(Get-ChildItem -LiteralPath $root -File -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)\bP10[-_](E|F|G)([-_.]|$)"
} | ForEach-Object { $_.FullName })
$forbiddenP10 = @($rootFileMatches + @(Get-NameMatches -ScanRoots $scanRoots -Pattern "(?i)\bP10[-_](E|F|G)([-_.]|$)"))
Add-Check "No P10-E/F/G artifacts" ($forbiddenP10.Count -eq 0) ($forbiddenP10 -join ", ")

Write-Host "NewMap collision/boundary/leaderboard preflight"
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
