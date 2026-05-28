param(
    [switch]$AllowOnline,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$script = Join-Path $root "tools\map\enrich_newmap_names_from_coordinates.py"

$argsList = @($script)
if ($AllowOnline) {
    $argsList += "--allow-online"
}
if ($DryRun) {
    $argsList += "--dry-run"
}

python @argsList
if ($LASTEXITCODE -ne 0) {
    throw "Name enrichment failed with exit code $LASTEXITCODE"
}
