param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation",
    [switch]$NoOnline
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$env:PYTHONIOENCODING = "utf-8"
$scriptPath = Join-Path $root "tools\map\enrich_non_official_candidate_names_from_coordinates.py"

if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
    Write-Host "[FAIL] Missing non-official candidate name enrichment script: $scriptPath"
    exit 1
}

$arguments = @($scriptPath, "--project-root", $root)
if ($NoOnline) {
    $arguments += "--no-online"
}

python @arguments
if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Non-official candidate name enrichment failed."
    exit $LASTEXITCODE
}

Write-Host "[PASS] Non-official candidate name enrichment preprocessing completed."
exit 0
