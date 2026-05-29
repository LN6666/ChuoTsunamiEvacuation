param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$cachePath = Join-Path $root "Assets\Data\P10\newmap_name_cache.json"
$labelConfigPath = Join-Path $root "Assets\Data\P10\newmap_name_label_config.json"
$controllerPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNameLabelController.cs"

$cache = Get-Content -Encoding UTF8 -LiteralPath $cachePath -Raw | ConvertFrom-Json
$labelConfig = Get-Content -Encoding UTF8 -LiteralPath $labelConfigPath -Raw | ConvertFrom-Json
$controller = Get-Content -Encoding UTF8 -LiteralPath $controllerPath -Raw
$failures = @()

if ([bool]$cache.runtimeNetworkRequestsAllowed) { $failures += "Name cache allows runtime network." }
if ($cache.labels.Count -lt 50) { $failures += "Name cache has too few labels." }
if ((@($cache.labels | Where-Object { $_.objectType -eq "official_shelter" })).Count -le 0) { $failures += "Official shelter labels missing." }
if ((@($cache.labels | Where-Object { $_.objectType -eq "candidate" })).Count -le 0) { $failures += "Candidate labels missing." }
if ((@($cache.labels | Where-Object { $_.objectType -eq "building" })).Count -le 0) { $failures += "Building labels missing." }
if ((@($cache.labels | Where-Object { $_.objectType -eq "road" })).Count -le 0) { $failures += "Road labels missing." }
if (-not [bool]$labelConfig.showBuildingNames) { $failures += "Runtime building labels are disabled." }
if ([bool]$labelConfig.runtimeNetworkRequestsAllowed) { $failures += "Label config allows runtime network." }
if ($controller -notmatch "NewMapNameCache.Load") { $failures += "Runtime label controller does not load name cache." }
if ($controller -notmatch "NormalizeMainName") { $failures += "Runtime label controller does not normalize main names." }
if ($controller -notmatch "runtimeNetworkRequestsAllowed=\{controller.RuntimeNetworkRequestsAllowed\}") { $failures += "Runtime label controller does not log no-web status." }

foreach ($label in $cache.labels) {
    $name = [string]$label.name
    $lower = $name.ToLowerInvariant()
    if ($lower.StartsWith("bldg_") -or $lower.StartsWith("gml_") -or $lower.StartsWith("13102-bldg-") -or $name -match "東京都.*中央区.*(丁目|番|号)" -or $name -match "^\s*[-+]?\d+(\.\d+)?\s*,") {
        $failures += "Bad visible name: $($label.id) = $name"
    }
}

if ($failures.Count -gt 0) {
    foreach ($failure in $failures) { Write-Host "[FAIL] $failure" }
    exit 1
}

Write-Host "[PASS] NewMap name cache is populated and runtime-loaded."
exit 0
