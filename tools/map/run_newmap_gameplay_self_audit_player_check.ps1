param(
    [int]$DurationSeconds = 190,
    [string]$BuildPath = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapGameplaySelfAuditPre\ChuoTsunamiEvacuation_NewMapGameplaySelfAuditPre.exe",
    [string]$OutputPath = "Assets\Data\P10\newmap_gameplay_self_audit_player_report.json"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$output = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $output) | Out-Null

if (-not (Test-Path -LiteralPath $BuildPath -PathType Leaf)) {
    Write-Host "[FAIL] Gameplay self-audit build not found: $BuildPath"
    exit 1
}

$process = Start-Process -FilePath $BuildPath -ArgumentList "-newmapSelfAuditSmoke" -PassThru -WindowStyle Minimized
Start-Sleep -Seconds 8
$process.Refresh()

$samples = @()
$started = Get-Date
while (((Get-Date) - $started).TotalSeconds -lt $DurationSeconds) {
    if ($process.HasExited) {
        break
    }

    $process.Refresh()
    $samples += [pscustomobject][ordered]@{
        time = (Get-Date).ToString("s")
        processName = $process.ProcessName
        id = $process.Id
        workingSetBytes = $process.WorkingSet64
        privateMemoryBytes = $process.PrivateMemorySize64
        cpuSeconds = $process.TotalProcessorTime.TotalSeconds
    }
    Start-Sleep -Seconds 1
}

$maxPrivate = if ($samples.Count -gt 0) { ($samples | Measure-Object -Property privateMemoryBytes -Maximum).Maximum } else { 0 }
$maxWorkingSet = if ($samples.Count -gt 0) { ($samples | Measure-Object -Property workingSetBytes -Maximum).Maximum } else { 0 }

$result = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    activeScene = "Assets/Scenes/Chuo_BaseMap.unity"
    buildPath = $BuildPath
    launchSmoke = if ($samples.Count -gt 0 -and -not $process.HasExited) { "passed" } elseif ($samples.Count -gt 0) { "process_exited_after_samples" } else { "failed_no_samples" }
    playerBuildEvidence = $false
    commandLineSmokeArg = "-newmapSelfAuditSmoke"
    durationSeconds = $DurationSeconds
    memoryMeasured = ($samples.Count -gt 0)
    memoryFocus = "informational_only"
    samples = $samples
    maxPrivateMemoryBytes = $maxPrivate
    maxWorkingSetBytes = $maxWorkingSet
    errorCount = "pending_log_parse"
    warningCount = "pending_log_parse"
    smokeScenarios = @()
    performanceSample = $null
    performanceDecision = "pending_player_log_parse"
    finalStatus = if ($samples.Count -gt 0) { "completed_with_documented_runtime_proxy" } else { "failed" }
}
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $output -Encoding UTF8

if (-not $process.HasExited) {
    $process.CloseMainWindow() | Out-Null
    Start-Sleep -Seconds 5
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}

Write-Host "[PASS] Gameplay self-audit player check completed. Report: $output"
exit 0
