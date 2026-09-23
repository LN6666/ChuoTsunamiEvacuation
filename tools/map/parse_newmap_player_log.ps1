param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_player_log_summary.json"
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
    Write-Host "[WARN] Player.log not found."
    exit 0
}

$lines = Get-Content -LiteralPath $LogPath -ErrorAction SilentlyContinue
$errors = @($lines | Where-Object { $_ -match "Exception|Error|NullReferenceException" })
$warnings = @($lines | Where-Object { $_ -match "Warning" })
$performanceLine = @($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1)
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
$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    performanceSample = $performanceSample
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $fullOutput -Encoding UTF8
if ($performanceSample) {
    $performanceOutput = Join-Path $ProjectRoot "Assets\Data\P10\newmap_performance_results.json"
    if (Test-Path -LiteralPath $performanceOutput -PathType Leaf) {
        $performanceResult = Get-Content -LiteralPath $performanceOutput -Raw | ConvertFrom-Json
        $performanceResult | Add-Member -NotePropertyName fpsMeasured -NotePropertyValue $true -Force
        $performanceResult | Add-Member -NotePropertyName averageFps -NotePropertyValue $performanceSample.averageFps -Force
        $performanceResult | Add-Member -NotePropertyName maxFrameMs -NotePropertyValue $performanceSample.maxFrameMs -Force
        $performanceResult | Add-Member -NotePropertyName stutterFramesOver66ms -NotePropertyValue $performanceSample.stutterFramesOver66ms -Force
        $performanceResult | Add-Member -NotePropertyName fpsSampleElapsedSeconds -NotePropertyValue $performanceSample.elapsedSeconds -Force
        $performanceResult | Add-Member -NotePropertyName limitations -NotePropertyValue @(
            "Private memory exceeds the 12 GB ordinary-PC risk gate.",
            "FPS/stutter sample was captured from Player.log during an automated minimized-window run; a manual visual check is still required."
        ) -Force
        $performanceResult | Add-Member -NotePropertyName finalStatus -NotePropertyValue "ready_with_limitations_memory_risk" -Force
        $performanceResult | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $performanceOutput -Encoding UTF8
    }
}

Write-Host "[PASS] Player.log summary written to $fullOutput"
exit 0
