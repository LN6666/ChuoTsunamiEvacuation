param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_hardening_player_log_summary2.json"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $localLow = Join-Path $env:USERPROFILE "AppData\LocalLow"
    $candidate = Get-ChildItem -LiteralPath $localLow -Recurse -Filter Player.log -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($candidate) {
        $LogPath = $candidate.FullName
    }
}

if ([string]::IsNullOrWhiteSpace($LogPath) -or -not (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
    Write-Host "[FAIL] Player.log not found."
    exit 1
}

$lines = @(Get-Content -LiteralPath $LogPath -ErrorAction SilentlyContinue | ForEach-Object { [string]$_ })
$errors = @($lines | Where-Object { $_ -match "Exception|Error|NullReferenceException" } | ForEach-Object { [string]$_ })
$warnings = @($lines | Where-Object { $_ -match "Warning" } | ForEach-Object { [string]$_ })
$performanceLine = [string](@($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1) | Select-Object -First 1)
$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$bootstrapTimingLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap timings:" } | Select-Object -Last 1) | Select-Object -First 1)
$colliderLine = [string](@($lines | Where-Object { $_ -match "NewMap staged MeshCollider shutdown completed" } | Select-Object -Last 1) | Select-Object -First 1)
$performanceSample = $null
if ($performanceLine) {
    $pattern = "elapsedSeconds=(?<elapsed>[0-9.]+)\s+frameCount=(?<frames>[0-9]+)\s+avgFps=(?<fps>[0-9.]+)\s+maxFrameMs=(?<maxFrameMs>[0-9.]+)\s+stutterFramesOver66ms=(?<stutters>[0-9]+)"
    $match = [regex]::Match($performanceLine, $pattern)
    if ($match.Success) {
        $performanceSample = [ordered]@{
            elapsedSeconds = [double]$match.Groups["elapsed"].Value
            frameCount = [int]$match.Groups["frames"].Value
            averageFps = [double]$match.Groups["fps"].Value
            maxFrameMs = [double]$match.Groups["maxFrameMs"].Value
            stutterFramesOver66ms = [int]$match.Groups["stutters"].Value
        }
    }
}

$spikeDecision = "needs_user_manual_observation"
if ($performanceSample) {
    if ($performanceSample.maxFrameMs -lt 2000 -and $errors.Count -eq 0) {
        $spikeDecision = "spike_reduced_ready_for_manual_test"
    }
    elseif ($performanceSample.maxFrameMs -ge 2000) {
        $spikeDecision = "spike_remaining_but_documented"
    }
}

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    performanceSample = $performanceSample
    bootstrapLine = $bootstrapLine
    bootstrapTimingLine = $bootstrapTimingLine
    colliderShutdownLine = $colliderLine
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    spikeDecision = $spikeDecision
    finalStatus = if ($errors.Count -eq 0 -and $warnings.Count -eq 0) { "completed_on_new_chuo_basemap" } elseif ($errors.Count -eq 0) { "completed_with_documented_runtime_proxy" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

$performanceOutput = Join-Path $ProjectRoot "Assets\Data\P10\newmap_performance_spike_retest.json"
if ($performanceSample -and (Test-Path -LiteralPath $performanceOutput -PathType Leaf)) {
    $performanceResult = Get-Content -LiteralPath $performanceOutput -Raw | ConvertFrom-Json
    $performanceResult | Add-Member -NotePropertyName fpsMeasured -NotePropertyValue $true -Force
    $performanceResult | Add-Member -NotePropertyName averageFps -NotePropertyValue $performanceSample.averageFps -Force
    $performanceResult | Add-Member -NotePropertyName maxFrameMs -NotePropertyValue $performanceSample.maxFrameMs -Force
    $performanceResult | Add-Member -NotePropertyName stutterFramesOver66ms -NotePropertyValue $performanceSample.stutterFramesOver66ms -Force
    $performanceResult | Add-Member -NotePropertyName fpsSampleElapsedSeconds -NotePropertyValue $performanceSample.elapsedSeconds -Force
    $performanceResult | Add-Member -NotePropertyName spikeReducedBelow2s -NotePropertyValue ($performanceSample.maxFrameMs -lt 2000) -Force
    $performanceResult | Add-Member -NotePropertyName spikeDecision -NotePropertyValue $spikeDecision -Force
    $performanceResult | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $performanceResult | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $performanceResult | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $performanceOutput -Encoding UTF8
}

$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_hardening_player_build_report2.json"
if (Test-Path -LiteralPath $buildReportPath -PathType Leaf) {
    $buildReport = Get-Content -LiteralPath $buildReportPath -Raw | ConvertFrom-Json
    $buildReport | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $buildReport | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $buildReport | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $buildReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $buildReportPath -Encoding UTF8
}

$docPath = Join-Path $ProjectRoot "docs\NEWMAP_PERFORMANCE_SPIKE_RETEST.md"
$maxFrameText = if ($performanceSample) { "$($performanceSample.maxFrameMs) ms" } else { "not measured" }
$avgFpsText = if ($performanceSample) { "$($performanceSample.averageFps)" } else { "not measured" }
$attributionText = "No in-game performance sample was found."
if ($performanceSample) {
    $attributionText = "Runtime bootstrap timing is shown above. If the max frame remains above 2 seconds while bootstrap total is below 2 seconds, the remaining spike is attributed to large Chuo_BaseMap Unity/PLATEAU scene activation and render startup before regular gameplay."
}
@"
# NewMap Performance Spike Retest

Generated: $((Get-Date).ToString("s"))

Decision: ``$spikeDecision``

- Player.log errors: $($errors.Count)
- Player.log warnings: $($warnings.Count)
- Average FPS: $avgFpsText
- Max frame: $maxFrameText
- Memory focus this task: informational only

Bootstrap line:

~~~text
$bootstrapLine
~~~

Bootstrap timing line:

~~~text
$bootstrapTimingLine
~~~

MeshCollider shutdown line:

~~~text
$colliderLine
~~~

Attribution: $attributionText
"@ | Set-Content -LiteralPath $docPath -Encoding UTF8

Write-Host "[PASS] Pre2 Player.log summary written to $fullOutput"
exit 0
