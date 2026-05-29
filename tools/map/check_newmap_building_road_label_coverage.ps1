param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$cachePath = Join-Path $root "Assets\Data\P10\newmap_name_cache.json"
$runtimePath = Join-Path $root "Assets\Data\P10\newmap_building_road_label_runtime_report.json"
$auditPath = Join-Path $root "Assets\Data\P10\newmap_building_road_label_coverage_audit.json"
$labelConfigPath = Join-Path $root "Assets\Data\P10\newmap_name_label_config.json"
$enrichmentPath = Join-Path $root "Assets\Data\P10\newmap_name_enrichment_report.json"

foreach ($path in @($cachePath, $runtimePath, $auditPath, $labelConfigPath, $enrichmentPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing building/road label coverage input: $path"
        exit 1
    }
}

$cache = Get-Content -Encoding UTF8 -LiteralPath $cachePath -Raw | ConvertFrom-Json
$runtime = Get-Content -Encoding UTF8 -LiteralPath $runtimePath -Raw | ConvertFrom-Json
$audit = Get-Content -Encoding UTF8 -LiteralPath $auditPath -Raw | ConvertFrom-Json
$labelConfig = Get-Content -Encoding UTF8 -LiteralPath $labelConfigPath -Raw | ConvertFrom-Json
$enrichment = Get-Content -Encoding UTF8 -LiteralPath $enrichmentPath -Raw | ConvertFrom-Json

$buildingCount = @($cache.labels | Where-Object { $_.objectType -eq "building" -and -not $_.disabled }).Count
$roadCount = @($cache.labels | Where-Object { $_.objectType -eq "road" -and -not $_.disabled }).Count
$badVisible = @($cache.labels | Where-Object {
    -not $_.disabled -and -not $_.hiddenInNormalMode -and (
        $_.finalDisplayName -match "^(bldg_|gml_|13102-bldg-)" -or
        $_.finalDisplayName -match "^-?\d+(\.\d+)?,\s*-?\d+(\.\d+)?$"
    )
}).Count

$passed =
    (-not [bool]$cache.runtimeNetworkRequestsAllowed) -and
    (-not [bool]$labelConfig.runtimeNetworkRequestsAllowed) -and
    [bool]$labelConfig.showBuildingNames -and
    [bool]$labelConfig.showRoadNames -and
    [int]$labelConfig.maxVisibleLabels -ge 180 -and
    [int]$labelConfig.maxVisibleBuildingLabels -ge 80 -and
    [int]$labelConfig.maxVisibleRoadLabels -ge 50 -and
    [double]$labelConfig.labelMaxDistanceMeters -ge 350.0 -and
    $cache.labels.Count -ge 300 -and
    $buildingCount -ge 100 -and
    $roadCount -ge 150 -and
    [int]$audit.ordinaryBuildingLabels -ge 100 -and
    [int]$runtime.ordinaryBuildingLabelsAvailable -ge 100 -and
    [int]$enrichment.onlineQueriesAttempted -gt 0 -and
    [int]$enrichment.overpassQueriesAttempted -gt 0 -and
    $badVisible -eq 0

if (-not $passed) {
    Write-Host "[FAIL] Building/road label coverage validation failed. cache=$($cache.labels.Count) building=$buildingCount road=$roadCount badVisible=$badVisible"
    exit 1
}

Write-Host "[PASS] Expanded building/road label cache and runtime config are present."
exit 0
