$ErrorActionPreference = "Stop"

$PipelineRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$VenvPath = Join-Path $PipelineRoot ".venv"
$VenvPython = Join-Path $VenvPath "Scripts\python.exe"
$BaseRequirements = Join-Path $PipelineRoot "requirements.txt"
$P5Requirements = Join-Path $PipelineRoot "requirements-p5.txt"

Write-Host "[p5-env] Pipeline root: $PipelineRoot"
Write-Host "[p5-env] Virtual environment: $VenvPath"

if (-not (Test-Path $VenvPython)) {
    Write-Host "[p5-env] Creating project-local virtual environment..."
    python -m venv $VenvPath
} else {
    Write-Host "[p5-env] Reusing existing project-local virtual environment."
}

if (-not (Test-Path $VenvPython)) {
    throw "[p5-env] Virtual environment python was not found after setup: $VenvPython"
}

Write-Host "[p5-env] Upgrading pip inside the project-local virtual environment..."
& $VenvPython -m pip install --upgrade pip

if (Test-Path $BaseRequirements) {
    Write-Host "[p5-env] Installing baseline data-pipeline requirements..."
    & $VenvPython -m pip install -r $BaseRequirements
} else {
    Write-Host "[p5-env] Baseline requirements.txt not found; skipping."
}

if (-not (Test-Path $P5Requirements)) {
    throw "[p5-env] P5 requirements file not found: $P5Requirements"
}

Write-Host "[p5-env] Installing P5 validation/GIS requirements..."
& $VenvPython -m pip install -r $P5Requirements

$Checks = @(
    @{ Name = "jsonschema"; ImportName = "jsonschema" },
    @{ Name = "pytest"; ImportName = "pytest" },
    @{ Name = "geopandas"; ImportName = "geopandas" },
    @{ Name = "shapely"; ImportName = "shapely" },
    @{ Name = "pyproj"; ImportName = "pyproj" },
    @{ Name = "networkx"; ImportName = "networkx" },
    @{ Name = "osmnx"; ImportName = "osmnx" }
)

$Failures = @()
Write-Host "[p5-env] Running import checks..."
foreach ($Check in $Checks) {
    $ImportName = $Check.ImportName
    & $VenvPython -c "import $ImportName; print('$ImportName ok')"
    if ($LASTEXITCODE -ne 0) {
        $Failures += $Check.Name
    }
}

if ($Failures.Count -gt 0) {
    Write-Host "[p5-env] Import check failures: $($Failures -join ', ')" -ForegroundColor Red
    exit 1
}

Write-Host "[p5-env] All P5 import checks passed." -ForegroundColor Green
Write-Host "[p5-env] No OSM network downloads, GIS processing, or global installs were performed by this script."
