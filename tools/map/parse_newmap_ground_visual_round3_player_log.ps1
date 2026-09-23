param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_ground_visual_round3_player_log_summary.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Get-LatestPlayerLog {
    $localLow = Join-Path $env:USERPROFILE "AppData\LocalLow"
    if (-not (Test-Path -LiteralPath $localLow -PathType Container)) {
        return $null
    }

    return Get-ChildItem -LiteralPath $localLow -Recurse -Filter Player.log -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
}

function Get-TokenValue {
    param([string]$Line, [string]$Name)
    if ([string]::IsNullOrWhiteSpace($Line)) {
        return $null
    }

    $pattern = "(?:^|\s)" + [regex]::Escape($Name) + "=(?<value>\S+)"
    $match = [regex]::Match($Line, $pattern)
    if ($match.Success) {
        return $match.Groups["value"].Value
    }

    return $null
}

function As-Double {
    param($Value)
    if ($null -eq $Value) {
        return $null
    }
    return [double]::Parse([string]$Value, [Globalization.CultureInfo]::InvariantCulture)
}

function As-Int {
    param($Value)
    if ($null -eq $Value) {
        return $null
    }
    return [int]$Value
}

function As-Bool {
    param($Value)
    if ($null -eq $Value) {
        return $null
    }
    return ([string]$Value) -eq "True" -or ([string]$Value) -eq "true"
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $candidate = Get-LatestPlayerLog
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
$nameLabelLine = [string](@($lines | Where-Object { $_ -match "NewMap name labels built" } | Select-Object -Last 1) | Select-Object -First 1)
$performanceLine = [string](@($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })
$spawnSmokeLine = [string](@($lines | Where-Object { $_ -match "scenario=spawn_road_playable_ground_validation" } | Select-Object -Last 1) | Select-Object -First 1)
$mouseSmokeLine = [string](@($lines | Where-Object { $_ -match "scenario=mouse_left_right_drag_look" } | Select-Object -Last 1) | Select-Object -First 1)
$nameSmokeLine = [string](@($lines | Where-Object { $_ -match "scenario=name_labels_offline_real_sources_only" } | Select-Object -Last 1) | Select-Object -First 1)
$nightLine = [string](@($lines | Where-Object { $_ -match "scenario=night_lighting_dark_sky_readable_buildings" } | Select-Object -Last 1) | Select-Object -First 1)

$bootstrap = [ordered]@{
    supportColliderActive = As-Bool (Get-TokenValue $bootstrapLine "supportColliderActive")
    supportRendererCount = As-Int (Get-TokenValue $bootstrapLine "supportRendererCount")
    supportVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "supportVisibleRenderers")
    supportDisabledRenderers = As-Int (Get-TokenValue $bootstrapLine "supportDisabledRenderers")
    blueDebugGroundDisabled = As-Int (Get-TokenValue $bootstrapLine "blueDebugGroundDisabled")
    playableBoundsValid = As-Bool (Get-TokenValue $bootstrapLine "playableBoundsValid")
    playableBoundsSource = Get-TokenValue $bootstrapLine "playableBoundsSource"
    playableMinX = As-Double (Get-TokenValue $bootstrapLine "playableMinX")
    playableMaxX = As-Double (Get-TokenValue $bootstrapLine "playableMaxX")
    playableMinZ = As-Double (Get-TokenValue $bootstrapLine "playableMinZ")
    playableMaxZ = As-Double (Get-TokenValue $bootstrapLine "playableMaxZ")
    airWallColliders = As-Int (Get-TokenValue $bootstrapLine "airWallColliders")
    airWallVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "airWallVisibleRenderers")
    oldSupportSurfaceY = As-Double (Get-TokenValue $bootstrapLine "oldSupportSurfaceY")
    supportSurfaceY = As-Double (Get-TokenValue $bootstrapLine "supportSurfaceY")
    visualGroundY = As-Double (Get-TokenValue $bootstrapLine "visualGroundY")
    sampledBuildingBaseY = As-Double (Get-TokenValue $bootstrapLine "sampledBuildingBaseY")
    sampledMapMinY = As-Double (Get-TokenValue $bootstrapLine "sampledMapMinY")
    roadSampleY = As-Double (Get-TokenValue $bootstrapLine "roadSampleY")
    roadSamples = As-Int (Get-TokenValue $bootstrapLine "roadSamples")
    supportToVisualGroundDelta = As-Double (Get-TokenValue $bootstrapLine "supportToVisualGroundDelta")
    supportToRoadDelta = As-Double (Get-TokenValue $bootstrapLine "supportToRoadDelta")
    supportToBuildingBaseDelta = As-Double (Get-TokenValue $bootstrapLine "supportToBuildingBaseDelta")
    buildingRoadYOffsetApplied = As-Double (Get-TokenValue $bootstrapLine "buildingRoadYOffsetApplied")
    buildingRoadAlignedRoots = As-Int (Get-TokenValue $bootstrapLine "buildingRoadAlignedRoots")
    buildingRoadAlignmentStatus = Get-TokenValue $bootstrapLine "buildingRoadAlignmentStatus"
    spawnValidationPassed = As-Bool (Get-TokenValue $bootstrapLine "spawnValidationPassed")
    spawnAttempts = As-Int (Get-TokenValue $bootstrapLine "spawnAttempts")
    spawnRejectedInsideBuilding = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedInsideBuilding")
    spawnRejectedOutOfBounds = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedOutOfBounds")
    spawnX = As-Double (Get-TokenValue $bootstrapLine "spawnX")
    spawnY = As-Double (Get-TokenValue $bootstrapLine "spawnY")
    spawnZ = As-Double (Get-TokenValue $bootstrapLine "spawnZ")
}

$labels = [ordered]@{
    availableLabels = As-Int (Get-TokenValue $nameLabelLine "availableLabels")
    activeLabels = As-Int (Get-TokenValue $nameLabelLine "activeLabels")
    officialShelterLabels = As-Int (Get-TokenValue $nameLabelLine "officialShelterLabels")
    nonOfficialCandidateLabels = As-Int (Get-TokenValue $nameLabelLine "nonOfficialCandidateLabels")
    roadNameLabels = As-Int (Get-TokenValue $nameLabelLine "roadNameLabels")
    buildingNameLabels = As-Int (Get-TokenValue $nameLabelLine "buildingNameLabels")
    tokyoStationLabels = As-Int (Get-TokenValue $nameLabelLine "tokyoStationLabels")
    idOnlyLabels = As-Int (Get-TokenValue $nameLabelLine "idOnlyLabels")
    runtimeNetworkRequestsAllowed = As-Bool (Get-TokenValue $nameLabelLine "runtimeNetworkRequestsAllowed")
    sourceNameStatus = Get-TokenValue $nameLabelLine "sourceNameStatus"
}

$supportPassed = $bootstrap.supportColliderActive -eq $true -and $bootstrap.supportVisibleRenderers -eq 0 -and $bootstrap.airWallVisibleRenderers -eq 0
$boundsPassed = $bootstrap.playableBoundsValid -eq $true -and $bootstrap.airWallColliders -eq 4
$roadDeltaPassed = $true
if ($null -ne $bootstrap.roadSamples -and $bootstrap.roadSamples -gt 0 -and $null -ne $bootstrap.supportToRoadDelta) {
    $roadDeltaPassed = $bootstrap.supportToRoadDelta -le 0.5
}
$groundPassed = $null -ne $bootstrap.supportToVisualGroundDelta -and $bootstrap.supportToVisualGroundDelta -le 0.5 -and $roadDeltaPassed
$spawnPassed = $bootstrap.spawnValidationPassed -eq $true
$mousePassed = $mouseSmokeLine -match "result=pass"
$namePassed = $labels.runtimeNetworkRequestsAllowed -eq $false -and $labels.idOnlyLabels -eq 0
$launchPassed = $selfAuditCompleted -and $selfAuditFailures.Count -eq 0
$logPassed = $errors.Count -eq 0 -and $supportPassed -and $boundsPassed -and $groundPassed -and $spawnPassed -and $namePassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    bootstrapDiagnostics = $bootstrap
    nameLabelDiagnostics = $labels
    bootstrapLine = $bootstrapLine
    nameLabelLine = $nameLabelLine
    performanceLine = $performanceLine
    spawnValidationSmokeLine = $spawnSmokeLine
    mouseLeftRightDragSmokeLine = $mouseSmokeLine
    nameLabelSmokeLine = $nameSmokeLine
    nightLightingRegressionLine = $nightLine
    launchSmokePassed = $launchPassed
    supportVisibilityPassed = $supportPassed
    playableBoundsPassed = $boundsPassed
    groundAlignmentPassed = $groundPassed
    spawnRegressionPassed = $spawnPassed
    mouseDragRegressionPassed = $mousePassed
    nameLabelOfflinePassed = $namePassed
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_ground_visual_round3_player_report.json"
if (Test-Path -LiteralPath $buildReportPath -PathType Leaf) {
    $buildReport = Get-Content -LiteralPath $buildReportPath -Raw | ConvertFrom-Json
    $buildReport | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $buildReport | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $buildReport | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $buildReport | Add-Member -NotePropertyName launchSmoke -NotePropertyValue ($(if ($launchPassed) { "completed_self_audit_smoke_passed" } else { "pending_or_failed" })) -Force
    $buildReport | Add-Member -NotePropertyName supportVisibilitySmoke -NotePropertyValue ($(if ($supportPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName groundAlignmentSmoke -NotePropertyValue ($(if ($groundPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName playableBoundsSmoke -NotePropertyValue ($(if ($boundsPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName nameLabelSmoke -NotePropertyValue ($(if ($namePassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName bootstrapDiagnostics -NotePropertyValue $bootstrap -Force
    $buildReport | Add-Member -NotePropertyName nameLabelDiagnostics -NotePropertyValue $labels -Force
    $buildReport | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildReportPath -Encoding UTF8
}

Write-Host "[PASS] Ground Visual Round 3 Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
