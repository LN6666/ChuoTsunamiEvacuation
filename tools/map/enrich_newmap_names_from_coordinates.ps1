param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$env:PYTHONIOENCODING = "utf-8"
$scriptPath = Join-Path $root "tools\map\enrich_newmap_names_from_coordinates.py"

if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
    Write-Host "[FAIL] Missing name enrichment script: $scriptPath"
    exit 1
}

python $scriptPath --project-root $root
if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] NewMap name enrichment failed."
    exit $LASTEXITCODE
}

Write-Host "[PASS] NewMap preprocessing name cache generated."
exit 0
