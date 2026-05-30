param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
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
        if (-not (Test-Path -LiteralPath $scanRoot)) { continue }
        $matches += @(Get-ChildItem -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue | Where-Object {
            $_.Name -match $Pattern
        } | ForEach-Object { $_.FullName })
    }
    return $matches
}

$requiredFiles = @(
    "Assets\Data\P10\newmap_non_official_candidate_name_audit.json",
    "Assets\Data\P10\newmap_non_official_candidate_name_query_list.json",
    "Assets\Data\P10\newmap_non_official_name_enrichment_config.json",
    "Assets\Data\P10\newmap_non_official_name_enrichment_report.json",
    "Assets\Data\P10\newmap_non_official_name_normalization_report.json",
    "Assets\Data\P10\newmap_non_official_name_cache_writeback_report.json",
    "Assets\Data\P10\newmap_name_cache.json",
    "Assets\Data\P10\newmap_name_label_runtime_report.json",
    "docs\NEWMAP_NON_OFFICIAL_CANDIDATE_NAME_AUDIT.md",
    "docs\NEWMAP_NON_OFFICIAL_CANDIDATE_NAME_QUERY_LIST.md",
    "docs\NEWMAP_NON_OFFICIAL_NAME_ENRICHMENT_FROM_COORDINATES.md",
    "docs\NEWMAP_NON_OFFICIAL_NAME_NORMALIZATION.md",
    "docs\NEWMAP_NON_OFFICIAL_NAME_CACHE_WRITEBACK.md",
    "docs\NEWMAP_NAME_LABEL_RUNTIME_REPORT.md",
    "deepseek_review_prompt_newmap_nonofficial_name_fix.md",
    "codex_prompts\newmap_nonofficial_name_fix.md",
    "tools\map\enrich_non_official_candidate_names_from_coordinates.py",
    "tools\map\enrich_non_official_candidate_names_from_coordinates.ps1",
    "tools\map\validate_newmap_nonofficial_name_fix_json.ps1",
    "tools\map\check_newmap_nonofficial_name_cache_loaded.ps1",
    "tools\map\build_newmap_nonofficial_name_fix_player.ps1",
    "tools\map\parse_newmap_nonofficial_name_fix_player_log.ps1"
)

foreach ($relative in $requiredFiles) {
    Add-Check "Required file exists" (Test-Path -LiteralPath (Join-Path $root $relative) -PathType Leaf) $relative
}

Invoke-ToolCheck "Non-official name JSON validation" "tools\map\validate_newmap_nonofficial_name_fix_json.ps1"
Invoke-ToolCheck "Non-official name cache loaded" "tools\map\check_newmap_nonofficial_name_cache_loaded.ps1"
Invoke-ToolCheck "Runtime no-web validation" "tools\map\check_newmap_runtime_no_web_requests.ps1"

$labelController = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapNameLabelController.cs") -Raw
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs") -Raw
$editor = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs") -Raw

$labelControllerWebScan = $labelController -replace "RuntimeWebRequestsObserved", "RuntimeOnlineDiagnosticsObserved"
Add-Check "Runtime label cache load is local-only" ($labelController -match "NewMapNameCache.Load" -and $labelController -match "RuntimeWebRequestsObserved => 0" -and $labelControllerWebScan -notmatch "UnityWebRequest|HttpClient|WebRequest|System\.Net|https?://") "NewMapNameLabelController"
Add-Check "Non-official label wording avoids official implication" ($labelController -match "Non-official Candidate:" -and $labelController -match "Not an official shelter") "NewMapNameLabelController"
Add-Check "Runtime recovered candidates stay non-official" ($bootstrap -match "NonOfficialWarningRequired = true" -and $bootstrap -match "IsOfficialShelter = false") "NewMapRuntimeBootstrap"
Add-Check "Temporary build command exists" ($editor -match "BuildNewMapNonOfficialNameFixPlayerCommandLine") "NewMapSceneSetupUtility"

$readinessPath = Join-Path $root "Assets\Data\P10\newmap_manual_playtest_readiness.json"
if (Test-Path -LiteralPath $readinessPath -PathType Leaf) {
    $readiness = Get-Content -Encoding UTF8 -LiteralPath $readinessPath -Raw | ConvertFrom-Json
    Add-Check "Manual readiness decision exists" (-not [string]::IsNullOrWhiteSpace([string]$readiness.manualReadinessDecision)) ([string]$readiness.manualReadinessDecision)
}
else {
    Add-Check "Manual readiness decision exists" $false $readinessPath
}

$archiveFiles = @()
foreach ($pattern in @("*.zip", "*.7z", "*.tar", "*.gz", "*.tgz")) {
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

Write-Host "NewMap non-official candidate name-fix preflight"
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
