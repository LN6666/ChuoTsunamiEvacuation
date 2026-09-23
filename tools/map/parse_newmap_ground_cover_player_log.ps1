param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_ground_cover_player_log_summary.json"
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
    supportVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "supportVisibleRenderers")
    playableBoundsValid = As-Bool (Get-TokenValue $bootstrapLine "playableBoundsValid")
    airWallColliders = As-Int (Get-TokenValue $bootstrapLine "airWallColliders")
    airWallVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "airWallVisibleRenderers")
    supportSurfaceY = As-Double (Get-TokenValue $bootstrapLine "supportSurfaceY")
    spawnGroundDelta = As-Double (Get-TokenValue $bootstrapLine "spawnGroundDelta")
    spawnValidationPassed = As-Bool (Get-TokenValue $bootstrapLine "spawnValidationPassed")
    spawnRejectedInsideBuilding = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedInsideBuilding")
    spawnRejectedOutOfBounds = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedOutOfBounds")
    nearestBuildingDistance = As-Double (Get-TokenValue $bootstrapLine "nearestBuildingDistance")
    spawnX = As-Double (Get-TokenValue $bootstrapLine "spawnX")
    spawnY = As-Double (Get-TokenValue $bootstrapLine "spawnY")
    spawnZ = As-Double (Get-TokenValue $bootstrapLine "spawnZ")
    adaptiveGridEnabled = As-Bool (Get-TokenValue $bootstrapLine "adaptiveGridEnabled")
    adaptiveGridActive = As-Bool (Get-TokenValue $bootstrapLine "adaptiveGridActive")
    adaptiveGridVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "adaptiveGridVisibleRenderers")
    safeGroundEnabled = As-Bool (Get-TokenValue $bootstrapLine "safeGroundEnabled")
    safeGroundSupportY = As-Double (Get-TokenValue $bootstrapLine "safeGroundSupportY")
    safeGroundColliders = As-Int (Get-TokenValue $bootstrapLine "safeGroundColliders")
    safeGroundRendererHidden = As-Bool (Get-TokenValue $bootstrapLine "safeGroundRendererHidden")
    fallOutPreventionEnabled = As-Bool (Get-TokenValue $bootstrapLine "fallOutPreventionEnabled")
    visibleLargeBlueGroundRenderers = As-Int (Get-TokenValue $bootstrapLine "visibleLargeBlueGroundRenderers")
    gameplayGroundCoverEnabled = As-Bool (Get-TokenValue $bootstrapLine "gameplayGroundCoverEnabled")
    gameplayGroundCoverActive = As-Bool (Get-TokenValue $bootstrapLine "gameplayGroundCoverActive")
    gameplayGroundCoverTiles = As-Int (Get-TokenValue $bootstrapLine "gameplayGroundCoverTiles")
    gameplayGroundCoverColliders = As-Int (Get-TokenValue $bootstrapLine "gameplayGroundCoverColliders")
    gameplayGroundCoverRenderers = As-Int (Get-TokenValue $bootstrapLine "gameplayGroundCoverRenderers")
    gameplayGroundCoverVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "gameplayGroundCoverVisibleRenderers")
    gameplayGroundCoverY = As-Double (Get-TokenValue $bootstrapLine "gameplayGroundCoverY")
    gameplayGroundCoverArea = As-Double (Get-TokenValue $bootstrapLine "gameplayGroundCoverArea")
    gameplayGroundCoverMaterial = Get-TokenValue $bootstrapLine "gameplayGroundCoverMaterial"
    gameplayGroundCoverMaterialSource = Get-TokenValue $bootstrapLine "gameplayGroundCoverMaterialSource"
    gameplayGroundCoverOpacity = As-Double (Get-TokenValue $bootstrapLine "gameplayGroundCoverOpacity")
    gameplayGroundCoverBlueLike = As-Bool (Get-TokenValue $bootstrapLine "gameplayGroundCoverBlueLike")
    gameplayGroundCoverMagentaLike = As-Bool (Get-TokenValue $bootstrapLine "gameplayGroundCoverMagentaLike")
}

$labels = [ordered]@{
    availableLabels = As-Int (Get-TokenValue $nameLabelLine "availableLabels")
    activeLabels = As-Int (Get-TokenValue $nameLabelLine "activeLabels")
    officialShelterLabels = As-Int (Get-TokenValue $nameLabelLine "officialShelterLabels")
    nonOfficialCandidateLabels = As-Int (Get-TokenValue $nameLabelLine "nonOfficialCandidateLabels")
    roadNameLabels = As-Int (Get-TokenValue $nameLabelLine "roadNameLabels")
    buildingNameLabels = As-Int (Get-TokenValue $nameLabelLine "buildingNameLabels")
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

$groundCoverPassed = $bootstrap.gameplayGroundCoverEnabled -eq $true -and
    $bootstrap.gameplayGroundCoverActive -eq $true -and
    $bootstrap.gameplayGroundCoverTiles -gt 0 -and
    $bootstrap.gameplayGroundCoverColliders -eq $bootstrap.gameplayGroundCoverTiles -and
    $bootstrap.gameplayGroundCoverVisibleRenderers -eq $bootstrap.gameplayGroundCoverTiles -and
    $bootstrap.gameplayGroundCoverBlueLike -eq $false -and
    $bootstrap.gameplayGroundCoverMagentaLike -eq $false
$bluePassed = $bootstrap.supportVisibleRenderers -eq 0 -and
    $bootstrap.adaptiveGridVisibleRenderers -eq 0 -and
    $bootstrap.visibleLargeBlueGroundRenderers -eq 0 -and
    $groundCoverPassed
$fallPassed = $bootstrap.fallOutPreventionEnabled -eq $true -and $bootstrap.supportColliderActive -eq $true
$boundsPassed = $bootstrap.playableBoundsValid -eq $true -and $bootstrap.airWallColliders -eq 4 -and $bootstrap.airWallVisibleRenderers -eq 0
$spawnPassed = $bootstrap.spawnValidationPassed -eq $true -and [math]::Abs([double]$bootstrap.spawnGroundDelta) -le 0.5
$namePassed = $labels.runtimeNetworkRequestsAllowed -eq $false -and $labels.idOnlyLabels -eq 0
$launchPassed = $selfAuditCompleted -and $selfAuditFailures.Count -eq 0
$logPassed = $errors.Count -eq 0 -and $warnings.Count -eq 0 -and $groundCoverPassed -and $bluePassed -and $fallPassed -and $boundsPassed -and $spawnPassed -and $namePassed -and $launchPassed

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
    groundCoverDiagnosticsPassed = $groundCoverPassed
    blueAreaDiagnosticsPassed = $bluePassed
    fallOutPreventionDiagnosticsPassed = $fallPassed
    playerSpawnDiagnosticsPassed = $spawnPassed
    npcGroundingDiagnosticsPassed = $groundCoverPassed
    labelCacheDiagnosticsPassed = $namePassed
    playableBoundsPassed = $boundsPassed
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

$buildReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_ground_cover_player_report.json"
if (Test-Path -LiteralPath $buildReportPath -PathType Leaf) {
    $buildReport = Get-Content -Encoding UTF8 -LiteralPath $buildReportPath -Raw | ConvertFrom-Json
    $buildReport | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $buildReport | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $buildReport | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $buildReport | Add-Member -NotePropertyName groundCoverDiagnostics -NotePropertyValue ($(if ($groundCoverPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName blueAreaDiagnostics -NotePropertyValue ($(if ($bluePassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName fallOutPreventionDiagnostics -NotePropertyValue ($(if ($fallPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName playerSpawnDiagnostics -NotePropertyValue ($(if ($spawnPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName npcGroundingDiagnostics -NotePropertyValue ($(if ($groundCoverPassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName labelCacheDiagnostics -NotePropertyValue ($(if ($namePassed) { "passed" } else { "failed" })) -Force
    $buildReport | Add-Member -NotePropertyName fpsStutterSample -NotePropertyValue $performanceSample -Force
    $buildReport | Add-Member -NotePropertyName bootstrapDiagnostics -NotePropertyValue $bootstrap -Force
    $buildReport | Add-Member -NotePropertyName nameLabelDiagnostics -NotePropertyValue $labels -Force
    $buildReport | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed_player_log_parse" })) -Force
    $buildReport | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildReportPath -Encoding UTF8
}

$coverReportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_gameplay_ground_cover_report.json"
if (Test-Path -LiteralPath $coverReportPath -PathType Leaf) {
    $coverReport = Get-Content -Encoding UTF8 -LiteralPath $coverReportPath -Raw | ConvertFrom-Json
    $coverReport | Add-Member -NotePropertyName runtimeTileCount -NotePropertyValue $bootstrap.gameplayGroundCoverTiles -Force
    $coverReport | Add-Member -NotePropertyName colliderCount -NotePropertyValue $bootstrap.gameplayGroundCoverColliders -Force
    $coverReport | Add-Member -NotePropertyName rendererCount -NotePropertyValue $bootstrap.gameplayGroundCoverRenderers -Force
    $coverReport | Add-Member -NotePropertyName visibleRendererCount -NotePropertyValue $bootstrap.gameplayGroundCoverVisibleRenderers -Force
    $coverReport | Add-Member -NotePropertyName averageY -NotePropertyValue $bootstrap.gameplayGroundCoverY -Force
    $coverReport | Add-Member -NotePropertyName minY -NotePropertyValue $bootstrap.gameplayGroundCoverY -Force
    $coverReport | Add-Member -NotePropertyName maxY -NotePropertyValue $bootstrap.gameplayGroundCoverY -Force
    $coverReport | Add-Member -NotePropertyName estimatedCoveredAreaSquareMeters -NotePropertyValue $bootstrap.gameplayGroundCoverArea -Force
    $coverReport | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($groundCoverPassed) { "validated_by_player_smoke" } else { "failed_player_log_parse" })) -Force
    $coverReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $coverReportPath -Encoding UTF8
}

$docPath = Join-Path $ProjectRoot "docs\NEWMAP_GROUND_COVER_PLAYER_REPORT.md"
$doc = @(
    "# NewMap Ground Cover Player Report",
    "",
    "Generated: $(Get-Date -Format s)",
    "",
    "Build: D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapGroundCoverPre\ChuoTsunamiEvacuation_NewMapGroundCoverPre.exe",
    "",
    "Player.log: $LogPath",
    "",
    "Summary:",
    "- Launch smoke: $launchPassed",
    "- Errors: $($errors.Count)",
    "- Warnings: $($warnings.Count)",
    "- Ground cover: $groundCoverPassed",
    "- Blue areas covered/blocked: $bluePassed",
    "- Fall-out prevention: $fallPassed",
    "- Player spawn on cover: $spawnPassed",
    "- Air walls: $boundsPassed",
    "- Runtime labels offline/cache-only: $namePassed",
    "- Final status: $($summary.finalStatus)",
    "",
    "Ground cover diagnostics:",
    "- Tiles: $($bootstrap.gameplayGroundCoverTiles)",
    "- Colliders: $($bootstrap.gameplayGroundCoverColliders)",
    "- Visible renderers: $($bootstrap.gameplayGroundCoverVisibleRenderers)",
    "- Cover Y: $($bootstrap.gameplayGroundCoverY)",
    "- Material: $($bootstrap.gameplayGroundCoverMaterial)",
    "",
    "This is a temporary validation player, not a final release/archive."
)
$doc | Set-Content -LiteralPath $docPath -Encoding UTF8

Write-Host "[PASS] Ground Cover Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
