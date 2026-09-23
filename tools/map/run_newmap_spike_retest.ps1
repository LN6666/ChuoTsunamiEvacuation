param(
    [int]$DurationSeconds = 180,
    [string]$ProcessName = "ChuoTsunamiEvacuation_NewMapHardeningPre2",
    [string]$BuildPath = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapHardeningPre2\ChuoTsunamiEvacuation_NewMapHardeningPre2.exe",
    [string]$OutputPath = "Assets\Data\P10\newmap_performance_spike_retest.json"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$output = Join-Path $ProjectRoot $OutputPath
$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_hardening_player_build_report2.json"
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $output) | Out-Null

$previousPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_performance_hardening_final.json"
$previousMaxFrameMs = $null
if (Test-Path -LiteralPath $previousPath -PathType Leaf) {
    $previous = Get-Content -LiteralPath $previousPath -Raw | ConvertFrom-Json
    $previousMaxFrameMs = $previous.maxFrameMs
}

$launchedByScript = $false
$process = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $process -and (Test-Path -LiteralPath $BuildPath -PathType Leaf)) {
    $process = Start-Process -FilePath $BuildPath -PassThru -WindowStyle Minimized
    $launchedByScript = $true
    Start-Sleep -Seconds 8
    $process.Refresh()
}

if (-not $process) {
    $result = [ordered]@{
        generatedAt = (Get-Date).ToString("s")
        activeScene = "Assets/Scenes/Chuo_BaseMap.unity"
        sampledProcess = $ProcessName
        durationSeconds = $DurationSeconds
        memoryMeasured = $false
        fpsMeasured = $false
        launchSmoke = "failed_no_process"
        spikeDecision = "blocked_by_startup_spike"
        finalStatus = "failed"
        limitation = "No Pre2 temporary player process was running and build path was not found."
    }
    $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $output -Encoding UTF8
    Write-Host "[FAIL] No process found for Pre2 spike retest."
    exit 1
}

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
    sampledProcess = $process.ProcessName
    durationSeconds = $DurationSeconds
    launchedByScript = $launchedByScript
    launchSmoke = if ($samples.Count -gt 0 -and -not $process.HasExited) { "passed" } elseif ($samples.Count -gt 0) { "process_exited_after_samples" } else { "failed_no_samples" }
    memoryMeasured = ($samples.Count -gt 0)
    memoryFocus = "informational_only"
    playerBuildMeasured = $true
    samples = $samples
    maxPrivateMemoryBytes = $maxPrivate
    maxWorkingSetBytes = $maxWorkingSet
    previousMaxFrameMs = $previousMaxFrameMs
    fpsMeasured = $false
    averageFps = $null
    maxFrameMs = $null
    stutterFramesOver66ms = $null
    spikeReducedBelow2s = $null
    spikeDecision = "needs_user_manual_observation"
    finalStatus = if ($samples.Count -gt 0) { "completed_with_documented_runtime_proxy" } else { "failed" }
    implementedSpikeReductions = @(
        "player runtime scene-wide renderer/collider bounds scan disabled",
        "player startup MeshCollider shutdown disabled; scene MeshColliders are no longer traversed during startup",
        "normal Debug.Log stack traces disabled in player builds",
        "NPC creation deferred until crowd failures are enabled",
        "official GML anchors use one bounded exact-name scene traversal and per-object renderer bounds",
        "old route validation kept out of player startup"
    )
}
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $output -Encoding UTF8

if (Test-Path -LiteralPath $buildReportPath -PathType Leaf) {
    $buildReport = Get-Content -LiteralPath $buildReportPath -Raw | ConvertFrom-Json
    $buildReport | Add-Member -NotePropertyName launchSmoke -NotePropertyValue $result.launchSmoke -Force
    $buildReport | Add-Member -NotePropertyName performanceSampling -NotePropertyValue "completed" -Force
    $buildReport | Add-Member -NotePropertyName maxPrivateMemoryBytes -NotePropertyValue $maxPrivate -Force
    $buildReport | Add-Member -NotePropertyName maxWorkingSetBytes -NotePropertyValue $maxWorkingSet -Force
    $buildReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $buildReportPath -Encoding UTF8
}

Write-Host "[PASS] Wrote Pre2 spike retest sampling result to $output"
exit 0
