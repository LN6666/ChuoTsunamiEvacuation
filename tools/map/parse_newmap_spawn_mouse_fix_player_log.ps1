param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_spawn_mouse_fix_player_log_summary.json"
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

$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$performanceLine = [string](@($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })
$mouseLine = [string](@($lines | Where-Object { $_ -match "scenario=mouse_left_right_drag_look" } | Select-Object -Last 1) | Select-Object -First 1)
$spawnSmokeLine = [string](@($lines | Where-Object { $_ -match "scenario=spawn_road_playable_ground_validation" } | Select-Object -Last 1) | Select-Object -First 1)
$nightLine = [string](@($lines | Where-Object { $_ -match "scenario=night_lighting_dark_sky_readable_buildings" } | Select-Object -Last 1) | Select-Object -First 1)

$spawnSample = $null
if ($bootstrapLine) {
    $spawnPattern = "spawnValidationPassed=(?<passed>True|False)\s+spawnMode=(?<mode>\S+)\s+spawnAttempts=(?<attempts>[0-9]+)\s+spawnAccepted=(?<accepted>[0-9]+)\s+spawnRejectedInsideBuilding=(?<inside>[0-9]+)\s+spawnRejectedNoGround=(?<noGround>[0-9]+)\s+spawnRejectedOutOfBounds=(?<outOfBounds>[0-9]+)\s+spawnRejectedTooCloseToBuilding=(?<tooClose>[0-9]+)\s+spawnFallbackUsed=(?<fallback>True|False)\s+fallbackSafeSpawnId=(?<fallbackId>\S+)\s+nearestBuildingDistance=(?<nearest>[0-9.]+)\s+buildingBoundsCached=(?<bounds>[0-9]+)\s+spawnX=(?<x>-?[0-9.]+)\s+spawnY=(?<y>-?[0-9.]+)\s+spawnZ=(?<z>-?[0-9.]+)"
    $match = [regex]::Match($bootstrapLine, $spawnPattern)
    if ($match.Success) {
        $spawnSample = [ordered]@{
            spawnValidationPassed = $match.Groups["passed"].Value -eq "True"
            spawnMode = $match.Groups["mode"].Value
            spawnAttempts = [int]$match.Groups["attempts"].Value
            spawnAccepted = [int]$match.Groups["accepted"].Value
            rejectedInsideBuilding = [int]$match.Groups["inside"].Value
            rejectedNoGround = [int]$match.Groups["noGround"].Value
            rejectedOutOfBounds = [int]$match.Groups["outOfBounds"].Value
            rejectedTooCloseToBuilding = [int]$match.Groups["tooClose"].Value
            fallbackUsed = $match.Groups["fallback"].Value -eq "True"
            fallbackSafeSpawnId = $match.Groups["fallbackId"].Value
            nearestBuildingDistance = [double]$match.Groups["nearest"].Value
            buildingBoundsCached = [int]$match.Groups["bounds"].Value
            finalSpawnPosition = [ordered]@{
                x = [double]$match.Groups["x"].Value
                y = [double]$match.Groups["y"].Value
                z = [double]$match.Groups["z"].Value
            }
        }
    }
}

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

$spawnSmokePassed = $spawnSmokeLine -match "result=pass"
$mouseSmokePassed = $mouseLine -match "result=pass"
$logPassed = $errors.Count -eq 0 -and $spawnSample -ne $null -and [bool]$spawnSample.spawnValidationPassed -and $spawnSmokePassed -and $mouseSmokePassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    spawnValidationSample = $spawnSample
    performanceSample = $performanceSample
    bootstrapLine = $bootstrapLine
    mouseLeftRightDragSmokeLine = $mouseLine
    spawnValidationSmokeLine = $spawnSmokeLine
    nightLightingRegressionLine = $nightLine
    launchSmokePassed = ($selfAuditCompleted -and $selfAuditFailures.Count -eq 0)
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_spawn_mouse_fix_player_report.json"
if (Test-Path -LiteralPath $buildReportPath -PathType Leaf) {
    $buildReport = Get-Content -LiteralPath $buildReportPath -Raw | ConvertFrom-Json
    $buildReport | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $buildReport | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $buildReport | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $buildReport | Add-Member -NotePropertyName spawnValidationSmoke -NotePropertyValue ($(if ($spawnSmokePassed) { "passed" } else { "pending_or_failed" })) -Force
    $buildReport | Add-Member -NotePropertyName mouseLeftRightDragSmoke -NotePropertyValue ($(if ($mouseSmokePassed) { "passed" } else { "pending_or_failed" })) -Force
    $buildReport | Add-Member -NotePropertyName launchSmoke -NotePropertyValue ($(if ($selfAuditCompleted -and $selfAuditFailures.Count -eq 0) { "completed_self_audit_smoke_passed" } else { "pending_or_failed" })) -Force
    $buildReport | Add-Member -NotePropertyName spawnValidationSample -NotePropertyValue $spawnSample -Force
    $buildReport | Add-Member -NotePropertyName fpsStutterSample -NotePropertyValue $performanceSample -Force
    $buildReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $buildReportPath -Encoding UTF8
}

Write-Host "[PASS] Spawn/mouse-fix Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
