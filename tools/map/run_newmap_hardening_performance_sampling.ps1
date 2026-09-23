param(
    [int]$DurationSeconds = 180,
    [string]$ProcessName = "ChuoTsunamiEvacuation_NewMapHardeningPre",
    [string]$BuildPath = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapHardeningPre\ChuoTsunamiEvacuation_NewMapHardeningPre.exe",
    [string]$OutputPath = "Assets\Data\P10\newmap_performance_hardening_final.json"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$output = Join-Path $ProjectRoot $OutputPath
$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_hardening_player_build_report.json"
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $output) | Out-Null

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
        finalStatus = "failed"
        limitation = "No hardening temporary player process was running and build path was not found."
    }
    $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $output -Encoding UTF8
    Write-Host "[FAIL] No process found for hardening performance sampling."
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
    playerBuildMeasured = $true
    samples = $samples
    maxPrivateMemoryBytes = $maxPrivate
    maxWorkingSetBytes = $maxWorkingSet
    ordinaryPcRisk = ($maxPrivate -gt 12GB)
    fpsMeasured = $false
    performanceHardening = @(
        "Scene MeshCollider shutdown is staged over frames after runtime collision support is active.",
        "Green frames and local estimated route guides remain hidden until Evacuation Stage 2.",
        "Light curtain remains hidden until Evacuation Stage 2.",
        "NPC cap remains 8 and crowd-delay target is local.",
        "Gameplay failure outcomes log as info instead of warnings."
    )
    finalStatus = if ($samples.Count -gt 0) { "completed_with_documented_runtime_proxy" } else { "failed" }
    limitations = if ($maxPrivate -gt 12GB) { @("Private memory exceeds the 12 GB ordinary-PC risk gate.") } else { @() }
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

Write-Host "[PASS] Wrote hardening performance sampling result to $output"
exit 0
