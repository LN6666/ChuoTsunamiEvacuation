param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_boundary_2270_player_log_summary.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Get-LatestPlayerLog {
    $candidate = Join-Path $ProjectRoot "Logs\newmap_boundary_2270_player.log"
    if (Test-Path -LiteralPath $candidate -PathType Leaf) {
        return Get-Item -LiteralPath $candidate
    }

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
    if ([string]::IsNullOrWhiteSpace($Line)) { return $null }
    $pattern = "(?:^|\s)" + [regex]::Escape($Name) + "=(?<value>\S+)"
    $match = [regex]::Match($Line, $pattern)
    if ($match.Success) { return $match.Groups["value"].Value }
    return $null
}

function As-Int {
    param($Value)
    if ($null -eq $Value) { return $null }
    return [int]$Value
}

function As-Double {
    param($Value)
    if ($null -eq $Value) { return $null }
    return [double]$Value
}

function As-Bool {
    param($Value)
    if ($null -eq $Value) { return $null }
    return ([string]$Value) -eq "True" -or ([string]$Value) -eq "true"
}

function Update-JsonFile {
    param([string]$RelativePath, [scriptblock]$Updater)
    $path = Join-Path $ProjectRoot $RelativePath
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        $json = Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
        & $Updater $json
        $json | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $path -Encoding UTF8
    }
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $candidate = Get-LatestPlayerLog
    if ($candidate) { $LogPath = $candidate.FullName }
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
$exceptions = @($lines | Where-Object { $_ -match "Exception" -and $_ -notmatch "LogException" })
$missing = @($lines | Where-Object { $_ -match "(?i)missing.*(asset|config|file)|could not be loaded" })
$web = @($lines | Where-Object { $_ -match "UnityWebRequest|HttpClient|System\.Net|http://|https://" })

$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$scenarioNames = @(
    "collision_whitelist_visuals_nonblocking",
    "circular_boundary_player_clamp",
    "circular_boundary_npc_clamp",
    "evacuation_stage1_warning",
    "tsunami_warning_180s_before_active",
    "building_touch_e_entry",
    "tsunami_front_failure"
)
$scenarioResults = [ordered]@{}
foreach ($name in $scenarioNames) {
    $line = [string](@($lines | Where-Object { $_ -match "scenario=$name" } | Select-Object -Last 1) | Select-Object -First 1)
    $scenarioResults[$name] = [ordered]@{
        present = -not [string]::IsNullOrWhiteSpace($line)
        passed = $line -match "result=pass"
        line = $line
    }
}

$boundary = [ordered]@{
    enabled = As-Bool (Get-TokenValue $bootstrapLine "circularBoundaryEnabled")
    centerX = As-Double (Get-TokenValue $bootstrapLine "circularBoundaryCenterX")
    centerZ = As-Double (Get-TokenValue $bootstrapLine "circularBoundaryCenterZ")
    radiusMeters = As-Double (Get-TokenValue $bootstrapLine "circularBoundaryRadius")
    playerClamp = As-Bool (Get-TokenValue $bootstrapLine "circularBoundaryPlayerClamp")
    npcClamp = As-Bool (Get-TokenValue $bootstrapLine "circularBoundaryNpcClamp")
    diagnosticColliders = As-Int (Get-TokenValue $bootstrapLine "circularBoundaryDiagnosticColliders")
}

$spawn = [ordered]@{
    validationPassed = As-Bool (Get-TokenValue $bootstrapLine "spawnValidationPassed")
    mode = Get-TokenValue $bootstrapLine "spawnMode"
    attempts = As-Int (Get-TokenValue $bootstrapLine "spawnAttempts")
    accepted = As-Int (Get-TokenValue $bootstrapLine "spawnAccepted")
    rejectedOutOfBounds = As-Int (Get-TokenValue $bootstrapLine "spawnRejectedOutOfBounds")
    fallbackUsed = As-Bool (Get-TokenValue $bootstrapLine "spawnFallbackUsed")
    fallbackSafeSpawnId = Get-TokenValue $bootstrapLine "fallbackSafeSpawnId"
}

$targetRoute = [ordered]@{
    activeTargetsInsidePlayableBoundary = As-Int (Get-TokenValue $bootstrapLine "activeTargetsInsidePlayableBoundary")
    activeTargetsOutsidePlayableBoundaryDisabled = As-Int (Get-TokenValue $bootstrapLine "activeTargetsOutsidePlayableBoundaryDisabled")
    routeGuidesSuppressedOutsidePlayableBoundary = As-Int (Get-TokenValue $bootstrapLine "routeGuidesSuppressedOutsidePlayableBoundary")
}

$airWall = [ordered]@{
    airWallColliders = As-Int (Get-TokenValue $bootstrapLine "airWallColliders")
    airWallVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "airWallVisibleRenderers")
    boundaryAirWallsPreserved = As-Int (Get-TokenValue $bootstrapLine "boundaryAirWallsPreserved")
    routeVisualBlockers = As-Int (Get-TokenValue $bootstrapLine "routeVisualBlockers")
    greenFrameVisualBlockers = As-Int (Get-TokenValue $bootstrapLine "greenFrameVisualBlockers")
    labelVisualBlockers = As-Int (Get-TokenValue $bootstrapLine "labelVisualBlockers")
    hazardVisualBlockers = As-Int (Get-TokenValue $bootstrapLine "hazardVisualBlockers")
    sampledValidPathsPassable = As-Bool (Get-TokenValue $bootstrapLine "sampledValidPathsPassable")
}

$playerBoundaryPassed = $boundary.enabled -eq $true -and [math]::Abs([double]$boundary.radiusMeters - 2270.0) -le 0.01 -and $boundary.playerClamp -eq $true -and [bool]$scenarioResults["circular_boundary_player_clamp"].passed
$npcBoundaryPassed = $boundary.enabled -eq $true -and [math]::Abs([double]$boundary.radiusMeters - 2270.0) -le 0.01 -and $boundary.npcClamp -eq $true -and [bool]$scenarioResults["circular_boundary_npc_clamp"].passed
$spawnBoundaryPassed = $spawn.validationPassed -eq $true -and (($spawn.rejectedOutOfBounds -eq 0) -or ($spawn.rejectedOutOfBounds -gt 0)) -and [math]::Abs([double]$boundary.radiusMeters - 2270.0) -le 0.01
$targetRoutePassed = $targetRoute.activeTargetsInsidePlayableBoundary -ge 0 -and $targetRoute.activeTargetsOutsidePlayableBoundaryDisabled -ge 0 -and $targetRoute.routeGuidesSuppressedOutsidePlayableBoundary -ge 0
$airWallPassed = $airWall.airWallColliders -eq 0 -and $airWall.airWallVisibleRenderers -eq 0 -and $airWall.boundaryAirWallsPreserved -eq 0

$allRequiredScenariosPassed = $true
foreach ($key in @("circular_boundary_player_clamp", "circular_boundary_npc_clamp")) {
    if (-not [bool]$scenarioResults[$key].passed) {
        $allRequiredScenariosPassed = $false
    }
}

$logPassed =
    $errors.Count -eq 0 -and
    $warnings.Count -eq 0 -and
    $exceptions.Count -eq 0 -and
    $missing.Count -eq 0 -and
    $web.Count -eq 0 -and
    $selfAuditCompleted -and
    $selfAuditFailures.Count -eq 0 -and
    $playerBoundaryPassed -and
    $npcBoundaryPassed -and
    $spawnBoundaryPassed -and
    $targetRoutePassed -and
    $airWallPassed -and
    $allRequiredScenariosPassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    exceptionCount = $exceptions.Count
    missingAssetOrConfigCount = $missing.Count
    runtimeWebRequestCount = $web.Count
    boundaryDiagnostics = $boundary
    spawnDiagnostics = $spawn
    activeTargetRouteDiagnostics = $targetRoute
    airWallDiagnostics = $airWall
    scenarioResults = $scenarioResults
    selfAuditCompleted = $selfAuditCompleted
    selfAuditFailureCount = $selfAuditFailures.Count
    playerBoundaryStatus = if ($playerBoundaryPassed) { "passed_2270m_clamp" } else { "failed" }
    npcBoundaryStatus = if ($npcBoundaryPassed) { "passed_2270m_clamp" } else { "failed" }
    spawnBoundaryStatus = if ($spawnBoundaryPassed) { "passed_uses_2270m_playable_boundary" } else { "failed" }
    activeTargetRouteBoundaryStatus = if ($targetRoutePassed) { "passed_uses_2270m_playable_boundary" } else { "failed" }
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    firstExceptions = @($exceptions | Select-Object -First 20)
    firstMissing = @($missing | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

Update-JsonFile "Assets\Data\P10\newmap_boundary_2270_player_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $json | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $json | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $json | Add-Member -NotePropertyName playerLogExceptions -NotePropertyValue $exceptions.Count -Force
    $json | Add-Member -NotePropertyName boundaryDiagnostics -NotePropertyValue $boundary -Force
    $json | Add-Member -NotePropertyName spawnDiagnostics -NotePropertyValue $spawn -Force
    $json | Add-Member -NotePropertyName activeTargetRouteDiagnostics -NotePropertyValue $targetRoute -Force
    $json | Add-Member -NotePropertyName airWallDiagnostics -NotePropertyValue $airWall -Force
    $json | Add-Member -NotePropertyName playerBoundaryStatus -NotePropertyValue $summary.playerBoundaryStatus -Force
    $json | Add-Member -NotePropertyName npcBoundaryStatus -NotePropertyValue $summary.npcBoundaryStatus -Force
    $json | Add-Member -NotePropertyName spawnBoundaryStatus -NotePropertyValue $summary.spawnBoundaryStatus -Force
    $json | Add-Member -NotePropertyName activeTargetRouteBoundaryStatus -NotePropertyValue $summary.activeTargetRouteBoundaryStatus -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_boundary_2_27km_update_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName playerLogResult -NotePropertyValue ($(if ($logPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName playerRuntimeDiagnostics -NotePropertyValue $boundary -Force
    $json | Add-Member -NotePropertyName playerSpawnDiagnostics -NotePropertyValue $spawn -Force
    $json | Add-Member -NotePropertyName playerActiveTargetRouteDiagnostics -NotePropertyValue $targetRoute -Force
    $json | Add-Member -NotePropertyName tempExePath -NotePropertyValue "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapBoundary2270Pre\ChuoTsunamiEvacuation_NewMapBoundary2270Pre.exe" -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "validated_2_27km_player_build" } else { "failed_player_log_parse" })) -Force
}

Write-Host "[PASS] NewMap 2.27km boundary Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
