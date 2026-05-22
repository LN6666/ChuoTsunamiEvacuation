[CmdletBinding()]
param(
    [switch]$CreateRecord,
    [string]$ValidateRecordPath = "",
    [ValidateSet("P7-A", "P7-B", "P7-C", "P7-D")]
    [string]$Stage = "P7-B",
    [string]$BenchmarkId = ""
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptRoot "..\..")
$preflightScript = Join-Path $scriptRoot "run_p7_preflight.ps1"
$newRecordScript = Join-Path $scriptRoot "new_p7_benchmark_record.ps1"
$validateScript = Join-Path $scriptRoot "validate_p7_performance_log.ps1"

Write-Host "P7 benchmark preflight: starting"
Write-Host ""
Write-Host "P7 benchmark preflight: running base P7 preflight"

$preflightOutput = & powershell -ExecutionPolicy Bypass -File $preflightScript 2>&1
$preflightExitCode = $LASTEXITCODE
$preflightOutput | ForEach-Object { Write-Host $_ }

$createdRecordPath = ""
$recordStepExitCode = 0

if ($CreateRecord) {
    Write-Host ""
    Write-Host "P7 benchmark preflight: creating benchmark record skeleton"

    $recordArgs = @("-ExecutionPolicy", "Bypass", "-File", $newRecordScript, "-Stage", $Stage)
    if (-not [string]::IsNullOrWhiteSpace($BenchmarkId)) {
        $recordArgs += @("-BenchmarkId", $BenchmarkId)
    }

    $recordOutput = & powershell @recordArgs 2>&1
    $recordStepExitCode = $LASTEXITCODE
    $recordOutput | ForEach-Object { Write-Host $_ }

    foreach ($line in $recordOutput) {
        if ($line -match "^P7 benchmark record created:\s*(.+)$") {
            $createdRecordPath = $Matches[1].Trim()
        }
    }
}

$validationExitCode = 0
$pathToValidate = $ValidateRecordPath
if ([string]::IsNullOrWhiteSpace($pathToValidate) -and -not [string]::IsNullOrWhiteSpace($createdRecordPath)) {
    $pathToValidate = $createdRecordPath
}

if (-not [string]::IsNullOrWhiteSpace($pathToValidate)) {
    Write-Host ""
    Write-Host "P7 benchmark preflight: validating benchmark record"

    $validationOutput = & powershell -ExecutionPolicy Bypass -File $validateScript -Path $pathToValidate 2>&1
    $validationExitCode = $LASTEXITCODE
    $validationOutput | ForEach-Object { Write-Host $_ }
}
else {
    Write-Host ""
    Write-Host "P7 benchmark preflight: no benchmark record creation or validation requested"
}

Write-Host ""
if ($preflightExitCode -eq 0 -and $recordStepExitCode -eq 0 -and $validationExitCode -eq 0) {
    Write-Host "P7 benchmark preflight final result: PASS"
    exit 0
}

Write-Host "P7 benchmark preflight final result: FAIL"
exit 1
