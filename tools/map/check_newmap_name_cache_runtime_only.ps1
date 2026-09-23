param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$labelConfigPath = Join-Path $root "Assets\Data\P10\newmap_name_label_config.json"
$cachePath = Join-Path $root "Assets\Data\P10\newmap_name_cache.json"
$enrichmentConfigPath = Join-Path $root "Assets\Data\P10\newmap_name_enrichment_config.json"
$enrichmentReportPath = Join-Path $root "Assets\Data\P10\newmap_name_enrichment_report.json"
$runtimeSourcePath = Join-Path $root "Assets\Scripts\NewMap\NewMapNameLabelController.cs"
$pythonToolPath = Join-Path $root "tools\map\enrich_newmap_names_from_coordinates.py"
$psToolPath = Join-Path $root "tools\map\enrich_newmap_names_from_coordinates.ps1"

foreach ($path in @($labelConfigPath, $cachePath, $enrichmentConfigPath, $enrichmentReportPath, $runtimeSourcePath, $pythonToolPath, $psToolPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Name cache/runtime-only file missing: $path"
        exit 1
    }
}

$labelConfig = Get-Content -Encoding UTF8 -LiteralPath $labelConfigPath -Raw | ConvertFrom-Json
$cache = Get-Content -Encoding UTF8 -LiteralPath $cachePath -Raw | ConvertFrom-Json
$enrichmentConfig = Get-Content -Encoding UTF8 -LiteralPath $enrichmentConfigPath -Raw | ConvertFrom-Json
$enrichmentReport = Get-Content -Encoding UTF8 -LiteralPath $enrichmentReportPath -Raw | ConvertFrom-Json
$runtimeSource = Get-Content -Encoding UTF8 -LiteralPath $runtimeSourcePath -Raw
$pythonTool = Get-Content -Encoding UTF8 -LiteralPath $pythonToolPath -Raw

$badRuntimeTerms = @("UnityWebRequest", "HttpClient", "WebRequest", "System.Net", "nominatim", "http://", "https://")
$badTermFound = $false
foreach ($term in $badRuntimeTerms) {
    if ($runtimeSource -match [regex]::Escape($term)) {
        $badTermFound = $true
    }
}

$badLabels = @($cache.labels | Where-Object {
    ([bool]$_.idOnly) -or
    ([bool]$_.disabled) -or
    ([string]$_.classification -match "address_only") -or
    ([string]$_.name -match "^\s*(bldg|tran|dem|urf|obj|id)[:_\-]?[0-9a-fA-F\-]{4,}\s*$")
})

$ok =
    [bool]$labelConfig.enabled -and
    -not [bool]$labelConfig.runtimeNetworkRequestsAllowed -and
    -not [bool]$labelConfig.showIdOnlyLabelsInDebug -and
    [int]$labelConfig.maxVisibleLabels -le 80 -and
    -not [bool]$cache.runtimeNetworkRequestsAllowed -and
    -not [bool]$enrichmentConfig.runtimeNetworkRequestsAllowed -and
    @("completed", "pending_user_network_run") -contains [string]$enrichmentReport.status -and
    -not $badTermFound -and
    $badLabels.Count -eq 0 -and
    $runtimeSource -match "NewMapNameCache.Load" -and
    $runtimeSource -match "runtimeNetworkRequestsAllowed = false" -and
    $pythonTool -match "AllowOnline" -and
    $pythonTool -match "pending_user_network_run"

if (-not $ok) {
    Write-Host "[FAIL] Name cache/runtime-only validation failed."
    Write-Host "Bad runtime web term found: $badTermFound"
    Write-Host "Bad cache labels: $($badLabels.Count)"
    exit 1
}

Write-Host "[PASS] Name enrichment is preprocessing-only and runtime reads local cache without web requests."
exit 0
