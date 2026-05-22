[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$scanScript = Join-Path $scriptRoot "scan_p7_assets.ps1"
$reportScript = Join-Path $scriptRoot "write_p7_asset_inventory_report.ps1"
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

Invoke-RequiredScript -Path $scanScript -Name "P7 asset scanner"
Invoke-RequiredScript -Path $reportScript -Name "P7 asset report writer"
Invoke-RequiredScript -Path $preflightScript -Name "P7 preflight"

Push-Location $repoRoot
try {
    Write-Host "P7-A asset inventory: starting"
    Write-Host ""
    Write-Host "P7-A asset inventory: running read-only scanner"

    $scanOutput = & $scanScript -ProjectRoot $repoRoot 2>&1
    $scanExitCode = $LASTEXITCODE
    if ($scanExitCode -ne 0) {
        $scanOutput | ForEach-Object { Write-Host $_ }
        Write-Host "P7-A asset inventory scanner result: FAIL"
        exit $scanExitCode
    }

    $scanJson = ($scanOutput -join [Environment]::NewLine)
    $scan = $scanJson | ConvertFrom-Json
    Write-Host "P7-A asset inventory scanner result: PASS"
    Write-Host "P7-A asset inventory scanned files: $($scan.totals.fileCount)"
    Write-Host "P7-A asset inventory scanned bytes: $($scan.totals.totalBytes)"
    Write-Host ""
    Write-Host "P7-A asset inventory: writing Markdown reports"

    $reportOutput = & $reportScript -ProjectRoot $repoRoot -ScanJson $scanJson 2>&1
    $reportExitCode = $LASTEXITCODE
    $reportOutput | ForEach-Object { Write-Host $_ }
    if ($reportExitCode -ne 0) {
        Write-Host "P7-A asset inventory report result: FAIL"
        exit $reportExitCode
    }

    Write-Host "P7-A asset inventory report result: PASS"
    Write-Host ""
    Write-Host "P7-A asset inventory: running P7 preflight"

    $preflightOutput = & $preflightScript 2>&1
    $preflightExitCode = $LASTEXITCODE
    $preflightOutput | ForEach-Object { Write-Host $_ }
    if ($preflightExitCode -ne 0) {
        Write-Host "P7-A asset inventory final result: FAIL"
        exit $preflightExitCode
    }

    Write-Host ""
    Write-Host "P7-A asset inventory final result: PASS"
    exit 0
}
finally {
    Pop-Location
}
