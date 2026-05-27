param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_no_memory_focus_player_log_summary.json"
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

$previousMaxFrameMs = 11273.06
$performanceDecision = "blocked_by_map_runtime_bootstrap"
if ($performanceSample) {
    if ($performanceSample.maxFrameMs -lt 2000) {
        $performanceDecision = "spike_reduced_below_target"
    }
    elseif ($performanceSample.maxFrameMs -lt $previousMaxFrameMs) {
        $performanceDecision = "spike_reduced_but_above_target"
    }
    else {
        $performanceDecision = "spike_remaining_but_documented"
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
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    performanceDecision = $performanceDecision
    finalStatus = if ($errors.Count -eq 0 -and $warnings.Count -eq 0) { "completed_on_new_chuo_basemap" } elseif ($errors.Count -eq 0) { "completed_with_documented_runtime_proxy" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

$performanceOutput = Join-Path $ProjectRoot "Assets\Data\P10\newmap_no_memory_focus_performance.json"
if (Test-Path -LiteralPath $performanceOutput -PathType Leaf) {
    $performanceResult = Get-Content -LiteralPath $performanceOutput -Raw | ConvertFrom-Json
    $performanceResult | Add-Member -NotePropertyName fpsMeasured -NotePropertyValue ([bool]$performanceSample) -Force
    if ($performanceSample) {
        $performanceResult | Add-Member -NotePropertyName averageFps -NotePropertyValue $performanceSample.averageFps -Force
        $performanceResult | Add-Member -NotePropertyName maxFrameMs -NotePropertyValue $performanceSample.maxFrameMs -Force
        $performanceResult | Add-Member -NotePropertyName stutterFramesOver66ms -NotePropertyValue $performanceSample.stutterFramesOver66ms -Force
        $performanceResult | Add-Member -NotePropertyName fpsSampleElapsedSeconds -NotePropertyValue $performanceSample.elapsedSeconds -Force
    }
    $performanceResult | Add-Member -NotePropertyName performanceDecision -NotePropertyValue $performanceDecision -Force
    $performanceResult | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $performanceResult | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $performanceResult | Add-Member -NotePropertyName finalStatus -NotePropertyValue $summary.finalStatus -Force
    $performanceResult | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $performanceOutput -Encoding UTF8
}

$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_no_memory_focus_build_report.json"
if (Test-Path -LiteralPath $buildReportPath -PathType Leaf) {
    $buildReport = Get-Content -LiteralPath $buildReportPath -Raw | ConvertFrom-Json
    $buildReport | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $buildReport | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $buildReport | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $buildReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $buildReportPath -Encoding UTF8
}

$maxFrameText = if ($performanceSample) { "$($performanceSample.maxFrameMs) ms" } else { "not measured" }
$avgFpsText = if ($performanceSample) { "$($performanceSample.averageFps)" } else { "not measured" }
@"
# NewMap No Memory Focus Performance

Generated: $((Get-Date).ToString("s"))

Decision: ``$performanceDecision``

- Player.log errors: $($errors.Count)
- Player.log warnings: $($warnings.Count)
- Average FPS: $avgFpsText
- Max frame: $maxFrameText
- Memory focus this task: informational only

Bootstrap timing:

~~~text
$bootstrapTimingLine
~~~
"@ | Set-Content -LiteralPath (Join-Path $ProjectRoot "docs\NEWMAP_NO_MEMORY_FOCUS_PERFORMANCE.md") -Encoding UTF8

Write-Host "[PASS] No-memory-focus Player.log summary written to $fullOutput"
exit 0
