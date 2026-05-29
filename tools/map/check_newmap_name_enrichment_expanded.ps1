param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$cachePath = Join-Path $root "Assets\Data\P10\newmap_name_cache.json"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_name_enrichment_report.json"
$queryPath = Join-Path $root "Assets\Data\P10\newmap_name_enrichment_query_list.json"
$configPath = Join-Path $root "Assets\Data\P10\newmap_name_enrichment_config.json"
$normalizationPath = Join-Path $root "Assets\Data\P10\newmap_name_normalization_report.json"

foreach ($path in @($cachePath, $reportPath, $queryPath, $configPath, $normalizationPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing name enrichment file: $path"
        exit 1
    }
}

$cache = Get-Content -Encoding UTF8 -LiteralPath $cachePath -Raw | ConvertFrom-Json
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json
$query = Get-Content -Encoding UTF8 -LiteralPath $queryPath -Raw | ConvertFrom-Json
$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json
$normalization = Get-Content -Encoding UTF8 -LiteralPath $normalizationPath -Raw | ConvertFrom-Json

$buildingCount = @($cache.labels | Where-Object { $_.objectType -eq "building" -and -not $_.disabled }).Count
$roadCount = @($cache.labels | Where-Object { $_.objectType -eq "road" -and -not $_.disabled }).Count

$badVisible = @($cache.labels | Where-Object {
    -not $_.disabled -and -not $_.hiddenInNormalMode -and (
        $_.finalDisplayName -match "^(bldg_|gml_|13102-bldg-)" -or
        $_.finalDisplayName -match "東京都.*中央区.*(丁目|番|号)" -or
        $_.finalDisplayName -match "^-?\d+(\.\d+)?,\s*-?\d+(\.\d+)?$"
    )
}).Count

$passed =
    (-not [bool]$config.runtimeNetworkRequestsAllowed) -and
    [bool]$config.allowOnlineLookup -and
    [int]$config.maxQueriesPerRun -ge 500 -and
    [bool]$config.queryBuildingsNearGameplayArea -and
    [bool]$config.queryRoadsNearRoutes -and
    $cache.labels.Count -ge 180 -and
    $buildingCount -ge 30 -and
    $roadCount -ge 80 -and
    [int]$query.queryCount -gt 0 -and
    [int]$report.onlineQueriesAttempted -gt 0 -and
    [int]$normalization.visibleIdOnlyLabelCount -eq 0 -and
    [int]$normalization.visibleAddressLikeLabelCount -eq 0 -and
    $badVisible -eq 0

if (-not $passed) {
    Write-Host "[FAIL] Expanded name enrichment validation failed. cache=$($cache.labels.Count) building=$buildingCount road=$roadCount online=$($report.onlineQueriesAttempted) badVisible=$badVisible"
    exit 1
}

Write-Host "[PASS] Expanded building/road name enrichment cache is present and normalized."
exit 0
