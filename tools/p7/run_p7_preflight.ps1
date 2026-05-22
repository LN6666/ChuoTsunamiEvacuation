[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$checkScript = Join-Path $scriptRoot "check_p7_scope.ps1"
$statusScript = Join-Path $scriptRoot "write_p7_status_report.ps1"

Write-Host "P7 preflight: starting"
Write-Host ""
Write-Host "P7 preflight: running scope guard"

$guardOutput = & $checkScript 2>&1
$guardExitCode = $LASTEXITCODE
$guardOutput | ForEach-Object { Write-Host $_ }

if ($guardExitCode -eq 0) {
    $guardResult = "PASS"
}
else {
    $guardResult = "FAIL"
}

Write-Host ""
Write-Host "P7 preflight: writing status report"

$statusOutput = & $statusScript -ScopeGuardResult $guardResult 2>&1
$statusExitCode = $LASTEXITCODE
$statusOutput | ForEach-Object { Write-Host $_ }

Write-Host ""
if ($guardExitCode -eq 0 -and $statusExitCode -eq 0) {
    Write-Host "P7 preflight final result: PASS"
    exit 0
}

Write-Host "P7 preflight final result: FAIL"
exit 1
