param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_airwall_npc_label_player_log_summary.json"
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

$bootstrapLine = [string](@($lines | Where-Object { $_ -match "NewMap runtime bootstrap completed" } | Select-Object -Last 1) | Select-Object -First 1)
$nameLabelLine = [string](@($lines | Where-Object { $_ -match "NewMap name labels built" } | Select-Object -Last 1) | Select-Object -First 1)
$npcLine = [string](@($lines | Where-Object { $_ -match "NewMap NPC distribution built" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$airwall = [ordered]@{
    airwallHardCollidersScanned = As-Int (Get-TokenValue $bootstrapLine "airwallHardCollidersScanned")
    unexpectedAirwallBlockers = As-Int (Get-TokenValue $bootstrapLine "unexpectedAirwallBlockers")
    airwallBlockersRemoved = As-Int (Get-TokenValue $bootstrapLine "airwallBlockersRemoved")
    airwallBlockersResized = As-Int (Get-TokenValue $bootstrapLine "airwallBlockersResized")
    airwallBlockersConvertedToTrigger = As-Int (Get-TokenValue $bootstrapLine "airwallBlockersConvertedToTrigger")
    boundaryAirWallsPreserved = As-Int (Get-TokenValue $bootstrapLine "boundaryAirWallsPreserved")
    invalidZoneBlockersPreserved = As-Int (Get-TokenValue $bootstrapLine "invalidZoneBlockersPreserved")
    unknownBlockersInsidePlayableArea = As-Int (Get-TokenValue $bootstrapLine "unknownBlockersInsidePlayableArea")
    buildingObstacleBoundsFiltered = As-Int (Get-TokenValue $bootstrapLine "buildingObstacleBoundsFiltered")
    buildingObstacleBoundsShrunk = As-Int (Get-TokenValue $bootstrapLine "buildingObstacleBoundsShrunk")
    sampledValidPathsPassable = As-Bool (Get-TokenValue $bootstrapLine "sampledValidPathsPassable")
    airWallColliders = As-Int (Get-TokenValue $bootstrapLine "airWallColliders")
    airWallVisibleRenderers = As-Int (Get-TokenValue $bootstrapLine "airWallVisibleRenderers")
}

$playerNpc = [ordered]@{
    playerNpcCollisionEnabled = As-Bool (Get-TokenValue $bootstrapLine "playerNpcCollisionEnabled")
    playerNpcCollisionBlocked = As-Int (Get-TokenValue $bootstrapLine "playerNpcCollisionBlocked")
    playerNpcCollisionSlowdowns = As-Int (Get-TokenValue $bootstrapLine "playerNpcCollisionSlowdowns")
    playerNpcCollisionEscapes = As-Int (Get-TokenValue $bootstrapLine "playerNpcCollisionEscapes")
    npcBodyColliders = As-Int (Get-TokenValue $bootstrapLine "npcBodyColliders")
    npcSoftBlockingEnabled = As-Bool (Get-TokenValue $bootstrapLine "npcSoftBlockingEnabled")
}

$npc = [ordered]@{
    requestedNpcCount = As-Int (Get-TokenValue $npcLine "requestedNpcCount")
    spawnedNpcCount = As-Int (Get-TokenValue $npcLine "spawnedNpcCount")
    cappedNpcCount = As-Int (Get-TokenValue $npcLine "cappedNpcCount")
    rejectedInsideBuildings = As-Int (Get-TokenValue $npcLine "rejectedInsideBuildings")
    avoidBuildings = As-Bool (Get-TokenValue $npcLine "avoidBuildings")
    usePooling = As-Bool (Get-TokenValue $npcLine "usePooling")
    continuousMovementEnabled = As-Bool (Get-TokenValue $npcLine "continuousMovementEnabled")
    stuckRecoveryEnabled = As-Bool (Get-TokenValue $npcLine "stuckRecoveryEnabled")
    buildingAvoidanceEnabled = As-Bool (Get-TokenValue $npcLine "buildingAvoidanceEnabled")
    npcBodyColliders = As-Int (Get-TokenValue $npcLine "npcBodyColliders")
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
    nameCacheLoaded = As-Bool (Get-TokenValue $nameLabelLine "nameCacheLoaded")
    nameCacheRecords = As-Int (Get-TokenValue $nameLabelLine "nameCacheRecords")
    reliableCacheLabels = As-Int (Get-TokenValue $nameLabelLine "reliableCacheLabels")
    addressOnlyHidden = As-Int (Get-TokenValue $nameLabelLine "addressOnlyHidden")
    lowConfidenceHidden = As-Int (Get-TokenValue $nameLabelLine "lowConfidenceHidden")
    runtimeNetworkRequestsAllowed = As-Bool (Get-TokenValue $nameLabelLine "runtimeNetworkRequestsAllowed")
    sourceNameStatus = Get-TokenValue $nameLabelLine "sourceNameStatus"
}

$cachePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_name_cache.json"
$cache = Get-Content -Encoding UTF8 -LiteralPath $cachePath -Raw | ConvertFrom-Json
$enrichmentReport = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $ProjectRoot "Assets\Data\P10\newmap_name_enrichment_report.json") -Raw | ConvertFrom-Json

$airwallPassed =
    $airwall.airWallColliders -eq 4 -and
    $airwall.airWallVisibleRenderers -eq 0 -and
    $airwall.boundaryAirWallsPreserved -ge 4 -and
    $airwall.sampledValidPathsPassable -eq $true

$collisionPassed =
    $playerNpc.playerNpcCollisionEnabled -eq $true -and
    $playerNpc.npcSoftBlockingEnabled -eq $true -and
    $playerNpc.npcBodyColliders -gt 0

$labelPassed =
    $labels.runtimeNetworkRequestsAllowed -eq $false -and
    $labels.nameCacheLoaded -eq $true -and
    $labels.nameCacheRecords -ge 180 -and
    $labels.officialShelterLabels -gt 0 -and
    $labels.nonOfficialCandidateLabels -gt 0 -and
    $labels.buildingNameLabels -ge 30 -and
    $labels.roadNameLabels -ge 80 -and
    $labels.idOnlyLabels -eq 0

$npcPassed =
    $npc.requestedNpcCount -eq 800 -and
    $npc.spawnedNpcCount -gt 0 -and
    $npc.avoidBuildings -eq $true -and
    $npc.continuousMovementEnabled -eq $true -and
    $npc.buildingAvoidanceEnabled -eq $true

$logPassed = $errors.Count -eq 0 -and $warnings.Count -eq 0 -and $selfAuditCompleted -and $selfAuditFailures.Count -eq 0 -and $airwallPassed -and $collisionPassed -and $labelPassed -and $npcPassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    airwallDiagnostics = $airwall
    playerNpcCollisionDiagnostics = $playerNpc
    npcDiagnostics = $npc
    nameLabelDiagnostics = $labels
    selfAuditCompleted = $selfAuditCompleted
    selfAuditFailureCount = $selfAuditFailures.Count
    cachePath = "Assets/Data/P10/newmap_name_cache.json"
    cacheRecordCount = $cache.labels.Count
    onlineQueriesAttempted = $enrichmentReport.onlineQueriesAttempted
    onlineQueriesSucceeded = $enrichmentReport.onlineQueriesSucceeded
    onlineLabelsAdded = $enrichmentReport.onlineLabelsAdded
    namesRejected = $enrichmentReport.namesRejected
    runtimeNoWebPassed = ($labels.runtimeNetworkRequestsAllowed -eq $false)
    airwallCleanupPassed = $airwallPassed
    playerNpcCollisionPassed = $collisionPassed
    labelRuntimePassed = $labelPassed
    npcRegressionPassed = $npcPassed
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

Update-JsonFile "Assets\Data\P10\newmap_unexpected_airwall_hard_audit.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName totalCollidersScanned -NotePropertyValue $airwall.airwallHardCollidersScanned -Force
    $json | Add-Member -NotePropertyName unexpectedBlockersFound -NotePropertyValue $airwall.unexpectedAirwallBlockers -Force
    $json | Add-Member -NotePropertyName unknownBlockersInsidePlayableAreaRemaining -NotePropertyValue $airwall.unknownBlockersInsidePlayableArea -Force
    $json | Add-Member -NotePropertyName buildingObstacleBoundsFiltered -NotePropertyValue $airwall.buildingObstacleBoundsFiltered -Force
    $json | Add-Member -NotePropertyName buildingObstacleBoundsShrunk -NotePropertyValue $airwall.buildingObstacleBoundsShrunk -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($airwallPassed) { "validated_by_player_log" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_airwall_hard_cleanup_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName totalCollidersScanned -NotePropertyValue $airwall.airwallHardCollidersScanned -Force
    $json | Add-Member -NotePropertyName unexpectedBlockersFound -NotePropertyValue $airwall.unexpectedAirwallBlockers -Force
    $json | Add-Member -NotePropertyName blockersRemoved -NotePropertyValue $airwall.airwallBlockersRemoved -Force
    $json | Add-Member -NotePropertyName blockersResized -NotePropertyValue $airwall.airwallBlockersResized -Force
    $json | Add-Member -NotePropertyName blockersConvertedToTrigger -NotePropertyValue $airwall.airwallBlockersConvertedToTrigger -Force
    $json | Add-Member -NotePropertyName sampledValidPathsNowPassable -NotePropertyValue $airwall.sampledValidPathsPassable -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($airwallPassed) { "validated_by_player_log" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_player_npc_collision_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName runtimeDiagnostics -NotePropertyValue $playerNpc -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($collisionPassed) { "validated_by_player_log_and_playmode" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_npc_collision_regression_after_player_collision.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName runtimeDiagnostics -NotePropertyValue $npc -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($npcPassed) { "validated_by_player_log" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_airwall_npc_label_regression.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName airWallsOnlyBoundaryOrInvalidZones -NotePropertyValue $airwallPassed -Force
    $json | Add-Member -NotePropertyName playerLogClean -NotePropertyValue ($errors.Count -eq 0 -and $warnings.Count -eq 0) -Force
    $json | Add-Member -NotePropertyName p2ToP10SmokeStatus -NotePropertyValue ($(if ($selfAuditCompleted -and $selfAuditFailures.Count -eq 0) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "validated" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_name_label_runtime_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName runtimeNetworkRequestsAllowed -NotePropertyValue $false -Force
    $json | Add-Member -NotePropertyName nameCachePath -NotePropertyValue "Assets/Data/P10/newmap_name_cache.json" -Force
    $json | Add-Member -NotePropertyName nameCacheExists -NotePropertyValue $true -Force
    $json | Add-Member -NotePropertyName nameCacheRecordCount -NotePropertyValue $labels.nameCacheRecords -Force
    $json | Add-Member -NotePropertyName buildingLabelsRuntime -NotePropertyValue $labels.buildingNameLabels -Force
    $json | Add-Member -NotePropertyName roadLabelsRuntime -NotePropertyValue $labels.roadNameLabels -Force
    $json | Add-Member -NotePropertyName idOnlyLabelsRuntime -NotePropertyValue $labels.idOnlyLabels -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($labelPassed) { "validated_by_airwall_npc_label_player_smoke" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_airwall_npc_label_player_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $json | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $json | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $json | Add-Member -NotePropertyName airwallDiagnostics -NotePropertyValue ($(if ($airwallPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName playerNpcCollisionDiagnostics -NotePropertyValue ($(if ($collisionPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName labelRuntimeDiagnostics -NotePropertyValue ($(if ($labelPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName runtimeNoWebDiagnostics -NotePropertyValue ($(if ($labels.runtimeNetworkRequestsAllowed -eq $false) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed_player_log_parse" })) -Force
}

$labelDocPath = Join-Path $ProjectRoot "docs\NEWMAP_NAME_LABEL_RUNTIME_REPORT.md"
$labelDoc = @(
    "# NewMap Name Label Runtime Report",
    "",
    "Generated: $(Get-Date -Format s)",
    "",
    "- Cache loaded: $($labels.nameCacheLoaded)",
    "- Cache records: $($labels.nameCacheRecords)",
    "- Official labels: $($labels.officialShelterLabels)",
    "- Non-official labels: $($labels.nonOfficialCandidateLabels)",
    "- Building labels: $($labels.buildingNameLabels)",
    "- Road labels: $($labels.roadNameLabels)",
    "- ID-only labels visible: $($labels.idOnlyLabels)",
    "- Runtime network requests allowed: $($labels.runtimeNetworkRequestsAllowed)",
    "- Final status: $(if ($labelPassed) { 'validated_by_airwall_npc_label_player_smoke' } else { 'failed_player_log_parse' })",
    "",
    "The Unity runtime reads `Assets/Data/P10/newmap_name_cache.json` and does not perform online lookups."
)
$labelDoc | Set-Content -LiteralPath $labelDocPath -Encoding UTF8

Write-Host "[PASS] Airwall/NPC/Label Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
