param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_collision_boundary_leaderboard_player_log_summary.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Get-LatestPlayerLog {
    $candidate = Join-Path $ProjectRoot "Logs\newmap_collision_boundary_leaderboard_player.log"
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
$exceptions = @($lines | Where-Object { $_ -match "Exception" -and $_ -notmatch "LogException" })
$missing = @($lines | Where-Object { $_ -match "(?i)missing.*(asset|config|file)|could not be loaded" })
$web = @($lines | Where-Object { $_ -match "UnityWebRequest|HttpClient|WebRequest|System\.Net|http://|https://" })

$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$scenarioNames = @(
    "collision_whitelist_visuals_nonblocking",
    "circular_boundary_player_clamp",
    "circular_boundary_npc_clamp",
    "r_leaderboard_toggle_show_hide",
    "shelter_direct_lines_created",
    "evacuation_pre_warning_wait",
    "tsunami_warning_300s_before_active",
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

$collision = [ordered]@{
    airWallColliders = As-Int (Get-TokenValue $bootstrapLine "airWallColliders")
    airWallVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "airWallVisibleRenderers")
    boundaryAirWallsPreserved = As-Int (Get-TokenValue $bootstrapLine "boundaryAirWallsPreserved")
    oldRectangularAirWallsDisabled = As-Int (Get-TokenValue $bootstrapLine "oldRectangularAirWallsDisabled")
    unknownBlockersInsidePlayableArea = As-Int (Get-TokenValue $bootstrapLine "unknownBlockersInsidePlayableArea")
    routeVisualBlockers = As-Int (Get-TokenValue $bootstrapLine "routeVisualBlockers")
    greenFrameVisualBlockers = As-Int (Get-TokenValue $bootstrapLine "greenFrameVisualBlockers")
    labelVisualBlockers = As-Int (Get-TokenValue $bootstrapLine "labelVisualBlockers")
    hazardVisualBlockers = As-Int (Get-TokenValue $bootstrapLine "hazardVisualBlockers")
    sampledValidPathsPassable = As-Bool (Get-TokenValue $bootstrapLine "sampledValidPathsPassable")
    directLineColliderScenarioPassed = [bool]$scenarioResults["shelter_direct_lines_created"].passed
}

$stamina = [ordered]@{
    expectedMaxStamina = 20000.0
    expectedEvacuationSprintSpeed = 6.75
}

$allNamedScenariosPassed = $true
foreach ($key in $scenarioResults.Keys) {
    if (-not [bool]$scenarioResults[$key].passed) {
        $allNamedScenariosPassed = $false
    }
}

$collisionPassed =
    $boundary.enabled -eq $true -and
    [math]::Abs([double]$boundary.radiusMeters - 2270.0) -le 0.01 -and
    $boundary.playerClamp -eq $true -and
    $boundary.npcClamp -eq $true -and
    $collision.airWallColliders -eq 0 -and
    $collision.airWallVisibleRenderers -eq 0 -and
    $collision.boundaryAirWallsPreserved -eq 0 -and
    $collision.unknownBlockersInsidePlayableArea -eq 0 -and
    $collision.routeVisualBlockers -eq 0 -and
    $collision.greenFrameVisualBlockers -eq 0 -and
    $collision.labelVisualBlockers -eq 0 -and
    $collision.hazardVisualBlockers -eq 0 -and
    $collision.sampledValidPathsPassable -eq $true

$logPassed =
    $errors.Count -eq 0 -and
    $warnings.Count -eq 0 -and
    $exceptions.Count -eq 0 -and
    $missing.Count -eq 0 -and
    $web.Count -eq 0 -and
    $selfAuditCompleted -and
    $selfAuditFailures.Count -eq 0 -and
    $collisionPassed -and
    $allNamedScenariosPassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    exceptionCount = $exceptions.Count
    missingAssetOrConfigCount = $missing.Count
    runtimeWebRequestCount = $web.Count
    boundaryDiagnostics = $boundary
    collisionWhitelistDiagnostics = $collision
    staminaDiagnostics = $stamina
    scenarioResults = $scenarioResults
    selfAuditCompleted = $selfAuditCompleted
    selfAuditFailureCount = $selfAuditFailures.Count
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    firstExceptions = @($exceptions | Select-Object -First 20)
    firstMissing = @($missing | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

Update-JsonFile "Assets\Data\P10\newmap_collision_whitelist_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName runtimeCountersFromPlayerLog -NotePropertyValue $collision -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($collisionPassed) { "validated_by_player_log" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_circular_boundary_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName runtimeCenterFromPlayerLog -NotePropertyValue ("{0},{1}" -f $boundary.centerX, $boundary.centerZ) -Force
    $json | Add-Member -NotePropertyName testsRun -NotePropertyValue "player_log_self_audit_smoke" -Force
    $json | Add-Member -NotePropertyName runtimeDiagnostics -NotePropertyValue $boundary -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($collisionPassed) { "validated_by_player_log" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_unexpected_airwall_final_cleanup.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName runtimeCountersFromPlayerLog -NotePropertyValue $collision -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($collisionPassed) { "validated_by_player_log" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_leaderboard_toggle_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName runtimeToggleSmokePassed -NotePropertyValue ([bool]$scenarioResults["r_leaderboard_toggle_show_hide"].passed) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ([bool]$scenarioResults["r_leaderboard_toggle_show_hide"].passed) { "validated_by_player_log" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_collision_boundary_r_toggle_regression.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName p2ToP10SmokeStatus -NotePropertyValue ($(if ($selfAuditCompleted -and $selfAuditFailures.Count -eq 0) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName playerLogStatus -NotePropertyValue ($(if ($errors.Count -eq 0 -and $warnings.Count -eq 0) { "clean" } else { "has_errors_or_warnings" })) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "validated" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_collision_boundary_leaderboard_player_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $json | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $json | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $json | Add-Member -NotePropertyName playerLogExceptions -NotePropertyValue $exceptions.Count -Force
    $json | Add-Member -NotePropertyName missingAssetOrConfigCount -NotePropertyValue $missing.Count -Force
    $json | Add-Member -NotePropertyName runtimeWebRequestCount -NotePropertyValue $web.Count -Force
    $json | Add-Member -NotePropertyName boundaryDiagnostics -NotePropertyValue $boundary -Force
    $json | Add-Member -NotePropertyName collisionWhitelistDiagnostics -NotePropertyValue $collision -Force
    $json | Add-Member -NotePropertyName rLeaderboardToggleSmokePassed -NotePropertyValue ([bool]$scenarioResults["r_leaderboard_toggle_show_hide"].passed) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_manual_playtest_readiness.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName playerLogErrorCount -NotePropertyValue $errors.Count -Force
    $json | Add-Member -NotePropertyName playerLogWarningCount -NotePropertyValue $warnings.Count -Force
    $json | Add-Member -NotePropertyName circularBoundaryRuntimeDiagnostics -NotePropertyValue $boundary -Force
    $json | Add-Member -NotePropertyName collisionWhitelistRuntimeDiagnostics -NotePropertyValue $collision -Force
    $json | Add-Member -NotePropertyName rLeaderboardToggleRuntimePassed -NotePropertyValue ([bool]$scenarioResults["r_leaderboard_toggle_show_hide"].passed) -Force
    $json | Add-Member -NotePropertyName manualReadinessDecision -NotePropertyValue ($(if ($logPassed) { "ready_with_documented_boundary_limitations" } else { "needs_quick_fix_before_manual_test" })) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "ready_with_documented_boundary_limitations" } else { "needs_quick_fix_before_manual_test" })) -Force
}

Write-Host "[PASS] NewMap collision/boundary/leaderboard Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
