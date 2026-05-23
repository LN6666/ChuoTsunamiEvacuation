[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$requiredDocs = @(
    "docs/P7D_WINDOWS_EXE_PROFILING_REPORT.md",
    "docs/P7D_EDITOR_VS_EXE_BENCHMARK.md",
    "docs/P7D_PERFORMANCE_SUMMARY.md"
)

$failed = $false
foreach ($doc in $requiredDocs) {
    $path = Join-Path $repoRoot ($doc -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "FAIL: missing performance/profiling doc: $doc"
        $failed = $true
    }
}

if ($failed) {
    exit 1
}

$profilingReport = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "docs\P7D_WINDOWS_EXE_PROFILING_REPORT.md")
$baselineDecision = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "docs\P7D_NEW_MAP_BASELINE_DECISION.md")

if ($profilingReport.IndexOf("Windows EXE profiling status: BLOCKED", [System.StringComparison]::OrdinalIgnoreCase) -lt 0 -and
    $profilingReport.IndexOf("Windows EXE profiling status: PREPARED", [System.StringComparison]::OrdinalIgnoreCase) -lt 0 -and
    $profilingReport.IndexOf("Windows EXE profiling status: COMPLETE", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
    Write-Host "FAIL: profiling report must say BLOCKED, PREPARED, or COMPLETE."
    exit 1
}

if ($baselineDecision.IndexOf("Decision: BLOCKED", [System.StringComparison]::OrdinalIgnoreCase) -lt 0 -and
    $baselineDecision.IndexOf("Decision: PASS", [System.StringComparison]::OrdinalIgnoreCase) -lt 0 -and
    $baselineDecision.IndexOf("Decision: CONDITIONAL PASS", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
    Write-Host "FAIL: baseline decision is not explicit."
    exit 1
}

Write-Host "P7-D performance record validation: PASS_WITH_BLOCKER_STATUS"
exit 0
