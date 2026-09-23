param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$cachePath = Join-Path $root "Assets\Data\P10\newmap_name_cache.json"
$resourcePath = Join-Path $root "Assets\Resources\NewMap\newmap_runtime_non_official_candidates.json"
$labelControllerPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNameLabelController.cs"
$runtimeReportPath = Join-Path $root "Assets\Data\P10\newmap_name_label_runtime_report.json"

foreach ($path in @($cachePath, $resourcePath, $labelControllerPath, $runtimeReportPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing required file: $path"
        exit 1
    }
}

$cache = Get-Content -Encoding UTF8 -LiteralPath $cachePath -Raw | ConvertFrom-Json
$resource = Get-Content -Encoding UTF8 -LiteralPath $resourcePath -Raw | ConvertFrom-Json
$source = Get-Content -Encoding UTF8 -LiteralPath $labelControllerPath -Raw
$runtime = Get-Content -Encoding UTF8 -LiteralPath $runtimeReportPath -Raw | ConvertFrom-Json

$candidateLabels = @($cache.labels | Where-Object {
    $_.objectType -eq "candidate" -and
    -not [bool]$_.hiddenInNormalMode -and
    -not [bool]$_.disabled -and
    [double]$_.confidence -ge 0.6
})
$enrichedLabels = @($candidateLabels | Where-Object {
    [string]$_.source -match "local_osm_cache|online_osm|online_gsi|source_metadata"
})
$updatedResourceNames = @($resource.records | Where-Object {
    [bool]$_.activeInGame -and
    [bool]$_.nonOfficialWarningRequired -and
    -not [bool]$_.isOfficialShelter -and
    [string]$_.displayName -notmatch "^13102-bldg-|^bldg_|^gml_|^sample_plateau|^p8_plateau_highrise_candidate_"
})

$ok =
    -not [bool]$cache.runtimeNetworkRequestsAllowed -and
    $candidateLabels.Count -ge 40 -and
    $enrichedLabels.Count -ge 20 -and
    $updatedResourceNames.Count -ge 40 -and
    [int]$runtime.runtimeWebRequestsObserved -eq 0 -and
    -not [bool]$runtime.runtimeNetworkRequestsAllowed -and
    $source -match "NewMapNameCache.Load" -and
    $source -match "Non-official Candidate:" -and
    $source -match "RuntimeWebRequestsObserved => 0" -and
    ($source -replace "RuntimeWebRequestsObserved", "RuntimeOnlineDiagnosticsObserved") -notmatch "UnityWebRequest|HttpClient|WebRequest|System\.Net|Nominatim|https?://"

if (-not $ok) {
    Write-Host "[FAIL] Non-official candidate name cache/runtime load check failed."
    Write-Host "candidateLabels=$($candidateLabels.Count) enrichedLabels=$($enrichedLabels.Count) updatedResourceNames=$($updatedResourceNames.Count)"
    exit 1
}

Write-Host "[PASS] Non-official candidate cache is populated and runtime-loaded without web requests."
exit 0
