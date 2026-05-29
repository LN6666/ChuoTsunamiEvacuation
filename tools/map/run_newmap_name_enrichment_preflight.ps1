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
$baseScene = Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity"
$setupPath = Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"
$labelPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNameLabelController.cs"
$enrichmentScript = Join-Path $root "tools\map\enrich_newmap_names_from_coordinates.py"
$readinessPath = Join-Path $root "Assets\Data\P10\newmap_manual_playtest_readiness.json"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Chuo_BaseMap scene exists" (Test-Path -LiteralPath $baseScene -PathType Leaf) $baseScene
Add-Check "Name enrichment build method exists" ([bool](Select-String -LiteralPath $setupPath -Pattern "BuildNewMapNameEnrichmentPlayerCommandLine" -SimpleMatch -Quiet)) "NewMapSceneSetupUtility"
Add-Check "Preprocessing enrichment tool exists" (Test-Path -LiteralPath $enrichmentScript -PathType Leaf) "tools/map/enrich_newmap_names_from_coordinates.py"

Invoke-ToolCheck "Name enrichment JSON validation" "tools\map\validate_newmap_name_enrichment_json.ps1"
Invoke-ToolCheck "Name cache loaded validation" "tools\map\check_newmap_name_cache_loaded.ps1"
Invoke-ToolCheck "Runtime no-web validation" "tools\map\check_newmap_runtime_no_web_requests.ps1"

$labels = Get-Content -Encoding UTF8 -LiteralPath $labelPath -Raw
Add-Check "Runtime loads local name cache" ($labels -match "NewMapNameCache.Load" -and $labels -match "finalDisplayName" -and $labels -match "DisplayNameForRuntime") "NewMapNameLabelController"
Add-Check "Runtime does not contain web request APIs" ($labels -notmatch "UnityWebRequest|HttpClient|WebRequest|System\.Net|Nominatim|https?://") "NewMapNameLabelController"
Add-Check "Runtime forces no-web flag" ($labels -match "RuntimeNetworkRequestsAllowed => false" -and $labels -match "config.runtimeNetworkRequestsAllowed = false") "NewMapNameLabelController"

if (Test-Path -LiteralPath $readinessPath -PathType Leaf) {
    $readiness = Get-Content -Encoding UTF8 -LiteralPath $readinessPath -Raw | ConvertFrom-Json
    $allowed = @("ready_for_manual_playtest", "ready_with_documented_building_visual_limitations", "ready_with_documented_ground_cover_limitations", "needs_quick_fix_before_manual_test", "blocked")
    Add-Check "Manual readiness decision exists" ($allowed -contains [string]$readiness.manualReadinessDecision) ([string]$readiness.manualReadinessDecision)
}
else {
    Add-Check "Manual readiness decision exists" $false $readinessPath
}

$rootArchivePatterns = @("*.zip", "*.7z", "*.tar", "*.gz", "*.tgz")
$archiveFiles = @()
foreach ($pattern in $rootArchivePatterns) {
    $archiveFiles += @(Get-ChildItem -LiteralPath $root -File -Filter $pattern -Force -ErrorAction SilentlyContinue)
}
$releaseLikeDirectories = @(Get-ChildItem -LiteralPath $root -Directory -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)(^|[-_])(release|archive)([-_]|$)"
})
Add-Check "No final release/archive artifacts" ($archiveFiles.Count -eq 0 -and $releaseLikeDirectories.Count -eq 0) (($archiveFiles.Name + $releaseLikeDirectories.Name) -join ", ")

$forbiddenRoots = @(
    (Join-Path $root "docs"),
    (Join-Path $root "tools"),
    (Join-Path $root "Assets\Data"),
    (Join-Path $root "Assets\Scripts"),
    (Join-Path $root "codex_prompts")
)
$rootFileMatches = @(Get-ChildItem -LiteralPath $root -File -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)\bP10[-_](E|F|G)([-_.]|$)"
} | ForEach-Object { $_.FullName })
$forbiddenP10 = @($rootFileMatches + @(Get-NameMatches -ScanRoots $forbiddenRoots -Pattern "(?i)\bP10[-_](E|F|G)([-_.]|$)"))
Add-Check "No P10-E/F/G artifacts" ($forbiddenP10.Count -eq 0) ($forbiddenP10 -join ", ")

Write-Host "NewMap name enrichment preflight"
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
