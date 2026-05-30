param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$runtimeRoot = Join-Path $root "Assets\Scripts\NewMap"
$patterns = "UnityWebRequest|HttpClient|WebRequest|System\.Net|Nominatim|https?://"
$matches = @(Get-ChildItem -LiteralPath $runtimeRoot -Recurse -Filter "*.cs" | Select-String -Pattern $patterns | Where-Object {
    $_.Line -notmatch "RuntimeWebRequestsObserved"
})
$labelConfig = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_name_label_config.json") -Raw | ConvertFrom-Json
$cache = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_name_cache.json") -Raw | ConvertFrom-Json

if ($matches.Count -gt 0) {
    $matches | ForEach-Object { Write-Host "[FAIL] Runtime web pattern: $($_.Path):$($_.LineNumber) $($_.Line.Trim())" }
    exit 1
}

if ([bool]$labelConfig.runtimeNetworkRequestsAllowed -or [bool]$cache.runtimeNetworkRequestsAllowed) {
    Write-Host "[FAIL] Runtime network flag is enabled in name config/cache."
    exit 1
}

Write-Host "[PASS] NewMap runtime scripts and label cache are offline-only."
exit 0
