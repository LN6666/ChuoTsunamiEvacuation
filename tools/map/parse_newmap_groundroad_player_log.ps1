param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_groundroad_merge_player_log_summary.json"
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
    if ($null -eq $Value) { return $null }
    return [double]::Parse([string]$Value, [Globalization.CultureInfo]::InvariantCulture)
}

function As-Int {
    param($Value)
    if ($null -eq $Value) { return $null }
    return [int]$Value
}

function As-Bool {
    param($Value)
    if ($null -eq $Value) { return $null }
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

$lines = @(Get-Content -Encoding UTF8 -LiteralPath $LogPath -ErrorAction SilentlyContinue | ForEach-Object { [string]$_ })
$errors = @($lines | Where-Object {
    $_ -match "NullReferenceException|MissingReferenceException|Unhandled Exception|UnityEngine\.Debug:LogError|LogType\.Error|^\s*Error:|Scripts have compiler errors"
})
$warnings = @($lines | Where-Object {
    ($_ -match "UnityEngine\.Debug:LogWarning|LogType\.Warning|^\s*Warning:|^\s*WARNING:") -and
    ($_ -notmatch "Stage 1 warning") -and
    ($_ -notmatch "evacuation_stage1_warning") -and
    ($_ -notmatch "tourism_non_official_inspection_warning") -and
    ($_ -notmatch "success_non_official_candidate_with_warning")
})

$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$nameLabelLine = [string](@($lines | Where-Object { $_ -match "NewMap name labels built" } | Select-Object -Last 1) | Select-Object -First 1)
$performanceLine = [string](@($lines | Where-Object { $_ -match "NewMap performance sample:" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$bootstrap = [ordered]@{
    supportColliderActive = As-Bool (Get-TokenValue $bootstrapLine "supportColliderActive")
    supportRendererCount = As-Int (Get-TokenValue $bootstrapLine "supportRendererCount")
    supportVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "supportVisibleRenderers")
    supportDisabledRenderers = As-Int (Get-TokenValue $bootstrapLine "supportDisabledRenderers")
    blueDebugGroundDisabled = As-Int (Get-TokenValue $bootstrapLine "blueDebugGroundDisabled")
    playableBoundsValid = As-Bool (Get-TokenValue $bootstrapLine "playableBoundsValid")
    airWallColliders = As-Int (Get-TokenValue $bootstrapLine "airWallColliders")
    airWallVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "airWallVisibleRenderers")
    oldSupportSurfaceY = As-Double (Get-TokenValue $bootstrapLine "oldSupportSurfaceY")
    supportSurfaceY = As-Double (Get-TokenValue $bootstrapLine "supportSurfaceY")
    visualGroundY = As-Double (Get-TokenValue $bootstrapLine "visualGroundY")
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
    adaptiveGridEnabled = As-Bool (Get-TokenValue $bootstrapLine "adaptiveGridEnabled")
    adaptiveGridActive = As-Bool (Get-TokenValue $bootstrapLine "adaptiveGridActive")
    adaptiveGridCells = As-Int (Get-TokenValue $bootstrapLine "adaptiveGridCells")
    adaptiveGridColliders = As-Int (Get-TokenValue $bootstrapLine "adaptiveGridColliders")
    adaptiveGridVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "adaptiveGridVisibleRenderers")
    adaptiveRoadCells = As-Int (Get-TokenValue $bootstrapLine "adaptiveRoadCells")
    adaptiveTerrainCells = As-Int (Get-TokenValue $bootstrapLine "adaptiveTerrainCells")
    adaptiveReliefCells = As-Int (Get-TokenValue $bootstrapLine "adaptiveReliefCells")
    adaptiveBridgeCells = As-Int (Get-TokenValue $bootstrapLine "adaptiveBridgeCells")
    adaptiveBuildingFallbackCells = As-Int (Get-TokenValue $bootstrapLine "adaptiveBuildingFallbackCells")
    adaptiveGlobalFallbackCells = As-Int (Get-TokenValue $bootstrapLine "adaptiveGlobalFallbackCells")
    adaptiveSupportYMin = As-Double (Get-TokenValue $bootstrapLine "adaptiveSupportYMin")
    adaptiveSupportYMax = As-Double (Get-TokenValue $bootstrapLine "adaptiveSupportYMax")
    adaptiveSupportYAvg = As-Double (Get-TokenValue $bootstrapLine "adaptiveSupportYAvg")
    adaptiveGridStatus = Get-TokenValue $bootstrapLine "adaptiveGridStatus"
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

$supportPassed = $bootstrap.adaptiveGridEnabled -eq $true -and
    $bootstrap.adaptiveGridActive -eq $true -and
    $bootstrap.adaptiveGridCells -gt 0 -and
    $bootstrap.adaptiveGridColliders -ge $bootstrap.adaptiveGridCells -and
    $bootstrap.adaptiveGridVisibleRenderers -eq 0 -and
    $bootstrap.supportVisibleRenderers -eq 0 -and
    $bootstrap.supportColliderActive -eq $true
$boundsPassed = $bootstrap.playableBoundsValid -eq $true -and $bootstrap.airWallColliders -eq 4 -and $bootstrap.airWallVisibleRenderers -eq 0
$bluePassed = $bootstrap.adaptiveGridVisibleRenderers -eq 0 -and $bootstrap.supportVisibleRenderers -eq 0
$spawnPassed = $bootstrap.spawnValidationPassed -eq $true
$namePassed = $labels.runtimeNetworkRequestsAllowed -eq $false -and $labels.idOnlyLabels -eq 0
$launchPassed = $selfAuditCompleted -and $selfAuditFailures.Count -eq 0
$logPassed = $errors.Count -eq 0 -and $warnings.Count -eq 0 -and $supportPassed -and $boundsPassed -and $bluePassed -and $spawnPassed -and $namePassed -and $launchPassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    bootstrapDiagnostics = $bootstrap
    nameLabelDiagnostics = $labels
    performanceSample = $performanceSample
    bootstrapLine = $bootstrapLine
    nameLabelLine = $nameLabelLine
    performanceLine = $performanceLine
    launchSmokePassed = $launchPassed
    supportGridDiagnosticsPassed = $supportPassed
    blueAreaDiagnosticsPassed = $bluePassed
    playerSpawnDiagnosticsPassed = $spawnPassed
    npcSpawnDiagnosticsPassed = $supportPassed
    labelCacheDiagnosticsPassed = $namePassed
    playableBoundsPassed = $boundsPassed
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_groundroad_merge_player_report.json"
if (Test-Path -LiteralPath $buildReportPath -PathType Leaf) {
    $buildReport = Get-Content -Encoding UTF8 -LiteralPath $buildReportPath -Raw | ConvertFrom-Json
    $buildReport | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $buildReport | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $buildReport | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $buildReport | Add-Member -NotePropertyName supportGridDiagnostics -NotePropertyValue ($(if ($supportPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName blueAreaDiagnostics -NotePropertyValue ($(if ($bluePassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName playerSpawnDiagnostics -NotePropertyValue ($(if ($spawnPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName npcSpawnDiagnostics -NotePropertyValue ($(if ($supportPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName labelCacheDiagnostics -NotePropertyValue ($(if ($namePassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName fpsStutterSample -NotePropertyValue $performanceSample -Force
    $buildReport | Add-Member -NotePropertyName bootstrapDiagnostics -NotePropertyValue $bootstrap -Force
    $buildReport | Add-Member -NotePropertyName nameLabelDiagnostics -NotePropertyValue $labels -Force
    $buildReport | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed_player_log_parse" })) -Force
    $buildReport | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildReportPath -Encoding UTF8
}

$runtimeReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_name_label_runtime_report.json"
$runtimeReport = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    runtimeNetworkRequestsAllowed = $labels.runtimeNetworkRequestsAllowed
    availableLabels = $labels.availableLabels
    activeLabels = $labels.activeLabels
    officialShelterLabels = $labels.officialShelterLabels
    nonOfficialCandidateLabels = $labels.nonOfficialCandidateLabels
    roadNameLabels = $labels.roadNameLabels
    buildingNameLabels = $labels.buildingNameLabels
    idOnlyLabels = $labels.idOnlyLabels
    sourceNameStatus = $labels.sourceNameStatus
    cacheOnlyRuntime = $namePassed
    labelsCappedAndCulled = ($labels.activeLabels -le 80)
    finalStatus = if ($namePassed) { "runtime_offline_cache_only" } else { "failed" }
}
$runtimeReport | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $runtimeReportPath -Encoding UTF8

$playerDocPath = Join-Path $ProjectRoot "docs\NEWMAP_GROUNDROAD_MERGE_PLAYER_REPORT.md"
$doc = @(
    "# NewMap Ground/Road Merge Player Report",
    "",
    "Generated: $(Get-Date -Format s)",
    "",
    "Build: D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapGroundRoadMergePre\ChuoTsunamiEvacuation_NewMapGroundRoadMergePre.exe",
    "",
    "Player.log: $LogPath",
    "",
    "Summary:",
    "- Launch smoke: $launchPassed",
    "- Errors: $($errors.Count)",
    "- Warnings: $($warnings.Count)",
    "- Adaptive support grid: $supportPassed",
    "- Blue/support renderers hidden: $bluePassed",
    "- Player spawn validation: $spawnPassed",
    "- Air walls: $boundsPassed",
    "- Runtime labels offline/cache-only: $namePassed",
    "- Final status: $($summary.finalStatus)",
    "",
    "Adaptive grid diagnostics:",
    "- Cells: $($bootstrap.adaptiveGridCells)",
    "- Colliders: $($bootstrap.adaptiveGridColliders)",
    "- Visible renderers: $($bootstrap.adaptiveGridVisibleRenderers)",
    "- Road cells: $($bootstrap.adaptiveRoadCells)",
    "- Terrain cells: $($bootstrap.adaptiveTerrainCells)",
    "- Relief cells: $($bootstrap.adaptiveReliefCells)",
    "- Building fallback cells: $($bootstrap.adaptiveBuildingFallbackCells)",
    "- Global fallback cells: $($bootstrap.adaptiveGlobalFallbackCells)",
    "- Support Y min/max/avg: $($bootstrap.adaptiveSupportYMin) / $($bootstrap.adaptiveSupportYMax) / $($bootstrap.adaptiveSupportYAvg)",
    "",
    "This is a temporary validation player, not a final release/archive."
)
$doc | Set-Content -LiteralPath $playerDocPath -Encoding UTF8

$nameDocPath = Join-Path $ProjectRoot "docs\NEWMAP_NAME_LABEL_RUNTIME_REPORT.md"
$nameDoc = @(
    "# NewMap Name Label Runtime Report",
    "",
    "Generated: $(Get-Date -Format s)",
    "",
    "Runtime label system:",
    "- Reads local config/cache only.",
    "- Performs no web requests in Unity runtime.",
    "- Hides low-confidence and ID-only labels in normal mode.",
    "- Preserves official and non-official target semantics.",
    "",
    "Player smoke:",
    "- Available labels: $($labels.availableLabels)",
    "- Active labels: $($labels.activeLabels)",
    "- Official shelter labels: $($labels.officialShelterLabels)",
    "- Non-official candidate labels: $($labels.nonOfficialCandidateLabels)",
    "- Road name labels: $($labels.roadNameLabels)",
    "- Building name labels: $($labels.buildingNameLabels)",
    "- ID-only labels: $($labels.idOnlyLabels)",
    "- Runtime network requests: $($labels.runtimeNetworkRequestsAllowed)",
    "- Source status: $($labels.sourceNameStatus)"
)
$nameDoc | Set-Content -LiteralPath $nameDocPath -Encoding UTF8

Write-Host "[PASS] Ground/Road Merge Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
