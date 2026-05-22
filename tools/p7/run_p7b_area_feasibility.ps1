[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$selectorScript = Join-Path $scriptRoot "select_p7b_candidate_area.ps1"
$preflightScript = Join-Path $scriptRoot "run_p7_preflight.ps1"

function Invoke-RequiredScript {
    param(
        [string]$Path,
        [string]$Name
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Name not found: $Path"
    }
}

Invoke-RequiredScript -Path $selectorScript -Name "P7-B candidate-area selector"
Invoke-RequiredScript -Path $preflightScript -Name "P7 preflight"

Push-Location $repoRoot
try {
    Write-Host "P7-B area feasibility: starting"
    Write-Host ""
    Write-Host "P7-B area feasibility: running read-only candidate selector"

    $selectorOutput = & powershell -ExecutionPolicy Bypass -File $selectorScript 2>&1
    $selectorExitCode = $LASTEXITCODE
    $selectorOutput | ForEach-Object { Write-Host $_ }

    if ($selectorExitCode -ne 0) {
        Write-Host "P7-B area feasibility selector result: FAIL"
        exit $selectorExitCode
    }

    Write-Host "P7-B area feasibility selector result: PASS"
    Write-Host ""
    Write-Host "P7-B area feasibility: running P7 preflight"

    $preflightOutput = & powershell -ExecutionPolicy Bypass -File $preflightScript 2>&1
    $preflightExitCode = $LASTEXITCODE
    $preflightOutput | ForEach-Object { Write-Host $_ }

    if ($preflightExitCode -ne 0) {
        Write-Host "P7-B area feasibility final result: FAIL"
        exit $preflightExitCode
    }

    Write-Host ""
    Write-Host "P7-B area feasibility final result: PASS"
    exit 0
}
finally {
    Pop-Location
}
