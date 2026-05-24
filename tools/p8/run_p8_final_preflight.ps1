[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "P8 final preflight: starting"

$failed = $false
try {
    foreach ($script in @(
        "run_p8e_preflight.ps1",
        "run_p8d_preflight.ps1",
        "run_p8_humanitarian_candidate_audit_preflight.ps1",
        "run_p8bc_consolidation_preflight.ps1",
        "run_p8c_preflight.ps1",
        "run_p8b_evidence_spatial_gate.ps1",
        "run_p8b_front_v1_preflight.ps1",
        "run_p8b_guard_preflight.ps1",
        "run_p8a_preflight.ps1",
        "run_p8a_compat_preflight.ps1"
    )) {
        Write-Host ""
        Write-Host "Running $script"
        & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot $script)
        if ($LASTEXITCODE -ne 0) {
            throw "$script failed."
        }
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8 final preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8 final preflight: PASS" -ForegroundColor Green
exit 0
