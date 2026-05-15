param(
    [string]$PythonExecutable = $env:PYTHON
)

$ErrorActionPreference = "Stop"

$PipelineRoot = $PSScriptRoot
$RepoRoot = Split-Path -Parent $PipelineRoot

function Test-PythonDependencies {
    param([string]$Candidate)

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        & $Candidate -c "import pytest, jsonschema" > $null 2>&1
        return $LASTEXITCODE -eq 0
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }
}

function Resolve-PipelinePython {
    if ($PythonExecutable) {
        if (-not (Test-Path $PythonExecutable) -and -not (Get-Command $PythonExecutable -ErrorAction SilentlyContinue)) {
            throw "Python executable not found: $PythonExecutable"
        }
        return $PythonExecutable
    }

    $pythonCommand = Get-Command python -ErrorAction SilentlyContinue
    if ($pythonCommand -and (Test-PythonDependencies $pythonCommand.Source)) {
        return $pythonCommand.Source
    }

    $repoVenvPython = Join-Path $RepoRoot ".venv\Scripts\python.exe"
    if (Test-Path $repoVenvPython) {
        return $repoVenvPython
    }

    if ($pythonCommand) {
        return $pythonCommand.Source
    }

    throw "Python was not found. Activate a Python environment with data_pipeline requirements installed."
}

function Invoke-PythonStep {
    param(
        [string]$Title,
        [string[]]$Arguments
    )

    Write-Host ""
    Write-Host "== $Title =="
    & $Python @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Title failed with exit code $LASTEXITCODE"
    }
}

$Python = Resolve-PipelinePython

Write-Host "Phase 3-00 real shelter data pipeline"
Write-Host "Repository root: $RepoRoot"
Write-Host "Python: $Python"

Push-Location $RepoRoot
try {
    Invoke-PythonStep "1/3 Export Unity-ready shelter sample data" @("data_pipeline/scripts/export_unity_shelters.py")
    Invoke-PythonStep "2/3 Validate exported shelter data" @("data_pipeline/scripts/validate_real_shelters.py")
    Invoke-PythonStep "3/3 Run pytest" @("-m", "pytest", "data_pipeline/tests")
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Phase 3-00 pipeline completed successfully."
