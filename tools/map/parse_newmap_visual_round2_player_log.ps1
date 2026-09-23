param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_visual_round2_player_log_summary.json"
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
$warnings = @($lines | Where-Object {
    ($_ -match "Warning") -and
    ($_ -notmatch "Stage 1 warning") -and
    ($_ -notmatch "evacuation_stage1_warning") -and
    ($_ -notmatch "tourism_non_official_inspection_warning") -and
    ($_ -notmatch "success_non_official_candidate_with_warning")
} | ForEach-Object { [string]$_ })

$performanceLine = [string](@($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1) | Select-Object -First 1)
$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$npcLine = [string](@($lines | Where-Object { $_ -match "NewMap NPC distribution built" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })
$mouseDragLine = [string](@($lines | Where-Object { $_ -match "scenario=mouse_drag_look_requires_button" } | Select-Object -Last 1) | Select-Object -First 1)
$nightLine = [string](@($lines | Where-Object { $_ -match "scenario=night_lighting_dark_sky_readable_buildings" } | Select-Object -Last 1) | Select-Object -First 1)

$performanceSample = $null
if ($performanceLine) {
    $pattern = "elapsedSeconds=(?<elapsed>[0-9.]+)\s+frameCount=(?<frames>[0-9]+)\s+avgFps=(?<fps>[0-9.]+)\s+maxFrameMs=(?<maxFrameMs>[0-9.]+)\s+stutterFramesOver66ms=(?<stutters>[0-9]+)(?:\s+requestedNpcCount=(?<requested>[0-9]+)\s+spawnedNpcCount=(?<spawned>[0-9]+)\s+cappedNpcCount=(?<capped>[0-9]+)\s+activeNpcCount=(?<active>[0-9]+))?"
    $match = [regex]::Match($performanceLine, $pattern)
    if ($match.Success) {
        $performanceSample = [ordered]@{
            elapsedSeconds = [double]$match.Groups["elapsed"].Value
            frameCount = [int]$match.Groups["frames"].Value
            averageFps = [double]$match.Groups["fps"].Value
            maxFrameMs = [double]$match.Groups["maxFrameMs"].Value
            stutterFramesOver66ms = [int]$match.Groups["stutters"].Value
            requestedNpcCount = if ($match.Groups["requested"].Success) { [int]$match.Groups["requested"].Value } else { $null }
            spawnedNpcCount = if ($match.Groups["spawned"].Success) { [int]$match.Groups["spawned"].Value } else { $null }
            cappedNpcCount = if ($match.Groups["capped"].Success) { [int]$match.Groups["capped"].Value } else { $null }
            activeNpcCount = if ($match.Groups["active"].Success) { [int]$match.Groups["active"].Value } else { $null }
        }
    }
}

$ground = $null
if ($bootstrapLine) {
    $groundPattern = "oldSupportSurfaceY=(?<old>-?[0-9.]+)\s+supportSurfaceY=(?<support>-?[0-9.]+)\s+visualGroundY=(?<visual>-?[0-9.]+)\s+sampledBuildingBaseY=(?<base>-?[0-9.]+)\s+sampledMapMinY=(?<min>-?[0-9.]+)\s+visualGroundSamples=(?<samples>[0-9]+)\s+supportToVisualGroundDelta=(?<delta>[0-9.]+)\s+spawnGroundDelta=(?<spawnDelta>[0-9.]+)\s+activeTargetMaxHeightOffset=(?<targetOffset>[0-9.]+)\s+activeTargetHeightOffsetViolations=(?<violations>[0-9]+)"
    $match = [regex]::Match($bootstrapLine, $groundPattern)
    if ($match.Success) {
        $ground = [ordered]@{
            oldSupportSurfaceY = [double]$match.Groups["old"].Value
            supportSurfaceY = [double]$match.Groups["support"].Value
            visualGroundY = [double]$match.Groups["visual"].Value
            sampledBuildingBaseY = [double]$match.Groups["base"].Value
            sampledMapMinY = [double]$match.Groups["min"].Value
            visualGroundSamples = [int]$match.Groups["samples"].Value
            supportToVisualGroundDelta = [double]$match.Groups["delta"].Value
            spawnGroundDelta = [double]$match.Groups["spawnDelta"].Value
            activeTargetMaxHeightOffset = [double]$match.Groups["targetOffset"].Value
            activeTargetHeightOffsetViolations = [int]$match.Groups["violations"].Value
        }
    }
}

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    performanceSample = $performanceSample
    groundAlignmentSample = $ground
    bootstrapLine = $bootstrapLine
    npcDistributionLine = $npcLine
    mouseDragLookSmokeLine = $mouseDragLine
    nightLightingSmokeLine = $nightLine
    launchSmokePassed = ($selfAuditCompleted -and $selfAuditFailures.Count -eq 0)
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($errors.Count -eq 0) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_visual_round2_player_report.json"
if (Test-Path -LiteralPath $buildReportPath -PathType Leaf) {
    $buildReport = Get-Content -LiteralPath $buildReportPath -Raw | ConvertFrom-Json
    $buildReport | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $buildReport | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $buildReport | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $buildReport | Add-Member -NotePropertyName fpsStutterSample -NotePropertyValue $performanceSample -Force
    $buildReport | Add-Member -NotePropertyName groundAlignmentSmoke -NotePropertyValue $ground -Force
    $buildReport | Add-Member -NotePropertyName launchSmoke -NotePropertyValue ($(if ($selfAuditCompleted -and $selfAuditFailures.Count -eq 0) { "completed_self_audit_smoke_passed" } else { "pending_or_failed" })) -Force
    $buildReport | Add-Member -NotePropertyName mouseDragLookSmoke -NotePropertyValue ($(if ($mouseDragLine -match "result=pass") { "passed" } else { "pending_or_failed" })) -Force
    $buildReport | Add-Member -NotePropertyName dayNightLightingSmoke -NotePropertyValue ($(if ($nightLine -match "result=pass") { "passed" } else { "pending_or_failed" })) -Force
    $buildReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $buildReportPath -Encoding UTF8
}

Write-Host "[PASS] Visual round-2 Player.log summary written to $fullOutput"
if ($errors.Count -gt 0) {
    exit 1
}
exit 0
