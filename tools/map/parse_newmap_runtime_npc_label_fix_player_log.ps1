param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_runtime_npc_label_player_log_summary.json"
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
        $json | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $path -Encoding UTF8
    }
}

function Lifecycle-Sample {
    param([string]$Line)
    return [ordered]@{
        seconds = As-Int (Get-TokenValue $Line "seconds")
        activeNpcCount = As-Int (Get-TokenValue $Line "activeNpcCount")
        createdAtStartup = As-Int (Get-TokenValue $Line "createdAtStartup")
        globalRespawnCount = As-Int (Get-TokenValue $Line "globalRespawnCount")
        individualRespawnCount = As-Int (Get-TokenValue $Line "individualRespawnCount")
        poolRecycleCount = As-Int (Get-TokenValue $Line "poolRecycleCount")
        destroyedDuringSmoke = As-Int (Get-TokenValue $Line "destroyedDuringSmoke")
        instantiateAfterStartup = As-Int (Get-TokenValue $Line "instantiateAfterStartup")
        allStopEventCount = As-Int (Get-TokenValue $Line "allStopEventCount")
        playerContactEvents = As-Int (Get-TokenValue $Line "playerContactEvents")
        stoppedWithoutReason = As-Int (Get-TokenValue $Line "stoppedWithoutReason")
        moving = As-Int (Get-TokenValue $Line "moving")
        arrived = As-Int (Get-TokenValue $Line "arrived")
        queued = As-Int (Get-TokenValue $Line "queued")
        stuck = As-Int (Get-TokenValue $Line "stuck")
        static = As-Int (Get-TokenValue $Line "static")
        averageSpeed = As-Double (Get-TokenValue $Line "averageSpeed")
        allowGlobalRefresh = As-Bool (Get-TokenValue $Line "allowGlobalRefresh")
        globalRespawnIntervalSeconds = As-Double (Get-TokenValue $Line "globalRespawnIntervalSeconds")
        collisionWithPlayerDoesNotGlobalPause = As-Bool (Get-TokenValue $Line "collisionWithPlayerDoesNotGlobalPause")
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
$concaveErrors = @($lines | Where-Object { $_ -match "Triggers on concave MeshColliders are not supported" })
$errors = @($lines | Where-Object {
    $_ -match "NullReferenceException|MissingReferenceException|Unhandled Exception|UnityEngine\.Debug:LogError|LogType\.Error|^\s*Error:|Scripts have compiler errors|Triggers on concave MeshColliders are not supported"
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
$lifecycleLines = @($lines | Where-Object { $_ -match "NewMap NPC lifecycle smoke" })
$lifecycleSamples = @($lifecycleLines | ForEach-Object { Lifecycle-Sample $_ })
$lastLifecycle = if ($lifecycleSamples.Count -gt 0) { $lifecycleSamples[-1] } else { $null }
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$concave = [ordered]@{
    concaveMeshTriggerErrorCount = $concaveErrors.Count
    concaveMeshTriggerOffenders = As-Int (Get-TokenValue $bootstrapLine "concaveMeshTriggerOffenders")
    concaveMeshTriggerFixed = As-Int (Get-TokenValue $bootstrapLine "concaveMeshTriggerFixed")
    concaveMeshTriggerProxies = As-Int (Get-TokenValue $bootstrapLine "concaveMeshTriggerProxies")
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
    playerNpcCollisionEnabled = As-Bool (Get-TokenValue $npcLine "playerNpcCollisionEnabled")
    npcBodyColliders = As-Int (Get-TokenValue $npcLine "npcBodyColliders")
    allowGlobalRefresh = As-Bool (Get-TokenValue $npcLine "allowGlobalRefresh")
    globalRespawnIntervalSeconds = As-Double (Get-TokenValue $npcLine "globalRespawnIntervalSeconds")
    createdAtStartup = As-Int (Get-TokenValue $npcLine "createdAtStartup")
    instantiateAfterStartup = As-Int (Get-TokenValue $npcLine "instantiateAfterStartup")
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

$cache = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $ProjectRoot "Assets\Data\P10\newmap_name_cache.json") -Raw | ConvertFrom-Json
$enrichmentReport = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $ProjectRoot "Assets\Data\P10\newmap_name_enrichment_report.json") -Raw | ConvertFrom-Json

$concavePassed = $concaveErrors.Count -eq 0
$npcLifecyclePassed =
    $npc.allowGlobalRefresh -eq $false -and
    [double]$npc.globalRespawnIntervalSeconds -eq 0.0 -and
    $npc.instantiateAfterStartup -eq 0 -and
    $lastLifecycle -ne $null -and
    $lastLifecycle.globalRespawnCount -eq 0 -and
    $lastLifecycle.allStopEventCount -eq 0 -and
    $lastLifecycle.stoppedWithoutReason -eq 0 -and
    $lastLifecycle.playerContactEvents -gt 0
$labelPassed =
    $labels.runtimeNetworkRequestsAllowed -eq $false -and
    $labels.nameCacheLoaded -eq $true -and
    $labels.nameCacheRecords -ge 300 -and
    $labels.buildingNameLabels -ge 100 -and
    $labels.roadNameLabels -ge 150 -and
    $labels.idOnlyLabels -eq 0
$npcRegressionPassed =
    $npc.requestedNpcCount -eq 800 -and
    $npc.spawnedNpcCount -gt 0 -and
    $npc.avoidBuildings -eq $true -and
    $npc.continuousMovementEnabled -eq $true -and
    $npc.buildingAvoidanceEnabled -eq $true -and
    $npc.playerNpcCollisionEnabled -eq $true

$logPassed =
    $errors.Count -eq 0 -and
    $warnings.Count -eq 0 -and
    $selfAuditCompleted -and
    $selfAuditFailures.Count -eq 0 -and
    $concavePassed -and
    $npcLifecyclePassed -and
    $labelPassed -and
    $npcRegressionPassed

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    concaveMeshTriggerDiagnostics = $concave
    npcDistributionDiagnostics = $npc
    npcLifecycleSamples = $lifecycleSamples
    nameLabelDiagnostics = $labels
    selfAuditCompleted = $selfAuditCompleted
    selfAuditFailureCount = $selfAuditFailures.Count
    cachePath = "Assets/Data/P10/newmap_name_cache.json"
    cacheRecordCount = $cache.labels.Count
    onlineQueriesAttempted = $enrichmentReport.onlineQueriesAttempted
    onlineQueriesSucceeded = $enrichmentReport.onlineQueriesSucceeded
    overpassQueriesAttempted = $enrichmentReport.overpassQueriesAttempted
    overpassQueriesSucceeded = $enrichmentReport.overpassQueriesSucceeded
    onlineLabelsAdded = $enrichmentReport.onlineLabelsAdded
    runtimeNoWebPassed = ($labels.runtimeNetworkRequestsAllowed -eq $false)
    concaveMeshTriggerPassed = $concavePassed
    npcLifecyclePassed = $npcLifecyclePassed
    labelRuntimePassed = $labelPassed
    npcRegressionPassed = $npcRegressionPassed
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

Update-JsonFile "Assets\Data\P10\newmap_concave_mesh_trigger_fix.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName playerLogStatus -NotePropertyValue ($(if ($concavePassed) { "no_concave_mesh_trigger_errors" } else { "failed_concave_mesh_trigger_errors_present" })) -Force
    $json | Add-Member -NotePropertyName concaveMeshTriggerErrorCount -NotePropertyValue $concaveErrors.Count -Force
    $json | Add-Member -NotePropertyName runtimeOffendersFixed -NotePropertyValue $concave.concaveMeshTriggerFixed -Force
    $json | Add-Member -NotePropertyName primitiveTriggerProxyCount -NotePropertyValue $concave.concaveMeshTriggerProxies -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($concavePassed) { "validated_by_player_log" } else { "failed_player_log" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_npc_lifecycle_deadlock_audit.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName npcCountRequested -NotePropertyValue $npc.requestedNpcCount -Force
    $json | Add-Member -NotePropertyName npcCountSpawned -NotePropertyValue $npc.spawnedNpcCount -Force
    $json | Add-Member -NotePropertyName globalRespawnCount -NotePropertyValue $lastLifecycle.globalRespawnCount -Force
    $json | Add-Member -NotePropertyName individualRespawnCount -NotePropertyValue $lastLifecycle.individualRespawnCount -Force
    $json | Add-Member -NotePropertyName poolRecycleCount -NotePropertyValue $lastLifecycle.poolRecycleCount -Force
    $json | Add-Member -NotePropertyName destroyCount -NotePropertyValue $lastLifecycle.destroyedDuringSmoke -Force
    $json | Add-Member -NotePropertyName instantiateCountAfterStartup -NotePropertyValue $lastLifecycle.instantiateAfterStartup -Force
    $json | Add-Member -NotePropertyName allStopEventDetected -NotePropertyValue ($lastLifecycle.allStopEventCount -gt 0) -Force
    $json | Add-Member -NotePropertyName stoppedWithoutReasonCount -NotePropertyValue $lastLifecycle.stoppedWithoutReason -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($npcLifecyclePassed) { "validated_by_player_log" } else { "failed_player_log" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_npc_no_rapid_refresh_fix.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName globalRespawnCount -NotePropertyValue $lastLifecycle.globalRespawnCount -Force
    $json | Add-Member -NotePropertyName destroyedDuringSmoke -NotePropertyValue $lastLifecycle.destroyedDuringSmoke -Force
    $json | Add-Member -NotePropertyName instantiateAfterStartup -NotePropertyValue $lastLifecycle.instantiateAfterStartup -Force
    $json | Add-Member -NotePropertyName poolRecycleCount -NotePropertyValue $lastLifecycle.poolRecycleCount -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($npcLifecyclePassed) { "validated_no_rapid_refresh" } else { "failed_player_log" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_npc_player_collision_deadlock_fix.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName playerContactEvents -NotePropertyValue $lastLifecycle.playerContactEvents -Force
    $json | Add-Member -NotePropertyName allStopEventCount -NotePropertyValue $lastLifecycle.allStopEventCount -Force
    $json | Add-Member -NotePropertyName stoppedWithoutReasonCount -NotePropertyValue $lastLifecycle.stoppedWithoutReason -Force
    $json | Add-Member -NotePropertyName playerContactSmokeStatus -NotePropertyValue ($(if ($npcLifecyclePassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($npcLifecyclePassed) { "validated_no_collision_deadlock" } else { "failed_player_log" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_npc_180s_lifecycle_smoke.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName samples -NotePropertyValue $lifecycleSamples -Force
    $json | Add-Member -NotePropertyName globalRespawnCount -NotePropertyValue $lastLifecycle.globalRespawnCount -Force
    $json | Add-Member -NotePropertyName allStopEventCount -NotePropertyValue $lastLifecycle.allStopEventCount -Force
    $json | Add-Member -NotePropertyName stoppedWithoutReasonCount -NotePropertyValue $lastLifecycle.stoppedWithoutReason -Force
    $json | Add-Member -NotePropertyName playerContactEvents -NotePropertyValue $lastLifecycle.playerContactEvents -Force
    $json | Add-Member -NotePropertyName playerLogStatus -NotePropertyValue ($(if ($npcLifecyclePassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($npcLifecyclePassed) { "validated_180s_smoke" } else { "failed_player_log" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_building_road_label_runtime_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName nameCacheLoaded -NotePropertyValue $labels.nameCacheLoaded -Force
    $json | Add-Member -NotePropertyName nameCacheRecordCount -NotePropertyValue $labels.nameCacheRecords -Force
    $json | Add-Member -NotePropertyName ordinaryBuildingLabelsRuntime -NotePropertyValue $labels.buildingNameLabels -Force
    $json | Add-Member -NotePropertyName roadLabelsRuntime -NotePropertyValue $labels.roadNameLabels -Force
    $json | Add-Member -NotePropertyName idOnlyLabelsRuntime -NotePropertyValue $labels.idOnlyLabels -Force
    $json | Add-Member -NotePropertyName runtimeSmokeStatus -NotePropertyValue ($(if ($labelPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($labelPassed) { "validated_by_player_log" } else { "failed_player_log" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_name_label_runtime_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName runtimeNetworkRequestsAllowed -NotePropertyValue $false -Force
    $json | Add-Member -NotePropertyName nameCacheRecordCount -NotePropertyValue $labels.nameCacheRecords -Force
    $json | Add-Member -NotePropertyName buildingLabelsRuntime -NotePropertyValue $labels.buildingNameLabels -Force
    $json | Add-Member -NotePropertyName roadLabelsRuntime -NotePropertyValue $labels.roadNameLabels -Force
    $json | Add-Member -NotePropertyName idOnlyLabelsRuntime -NotePropertyValue $labels.idOnlyLabels -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($labelPassed) { "validated_by_runtime_npc_label_player_smoke" } else { "failed_player_log" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_runtime_error_npc_label_regression.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName concaveMeshColliderTriggerErrorCount -NotePropertyValue $concaveErrors.Count -Force
    $json | Add-Member -NotePropertyName npcGlobalRefreshCount -NotePropertyValue $lastLifecycle.globalRespawnCount -Force
    $json | Add-Member -NotePropertyName npcAllStopCount -NotePropertyValue $lastLifecycle.allStopEventCount -Force
    $json | Add-Member -NotePropertyName playerContactEvents -NotePropertyValue $lastLifecycle.playerContactEvents -Force
    $json | Add-Member -NotePropertyName playerLogClean -NotePropertyValue ($errors.Count -eq 0 -and $warnings.Count -eq 0) -Force
    $json | Add-Member -NotePropertyName p2ToP10SmokeStatus -NotePropertyValue ($(if ($selfAuditCompleted -and $selfAuditFailures.Count -eq 0) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "validated" } else { "failed_player_log" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_runtime_npc_label_player_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $json | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $json | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $json | Add-Member -NotePropertyName concaveMeshTriggerDiagnostics -NotePropertyValue ($(if ($concavePassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName npcLifecycleDiagnostics -NotePropertyValue ($(if ($npcLifecyclePassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName playerNpcContactDiagnostics -NotePropertyValue ($(if ($npcLifecyclePassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName labelRuntimeDiagnostics -NotePropertyValue ($(if ($labelPassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName runtimeNoWebDiagnostics -NotePropertyValue ($(if ($labels.runtimeNetworkRequestsAllowed -eq $false) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed_player_log_parse" })) -Force
}

$labelDocPath = Join-Path $ProjectRoot "docs\NEWMAP_BUILDING_ROAD_LABEL_RUNTIME_REPORT.md"
$labelDoc = @(
    "# NewMap Building Road Label Runtime Report",
    "",
    "Generated: $(Get-Date -Format s)",
    "",
    "- Cache loaded: $($labels.nameCacheLoaded)",
    "- Cache records: $($labels.nameCacheRecords)",
    "- Building labels: $($labels.buildingNameLabels)",
    "- Road labels: $($labels.roadNameLabels)",
    "- ID-only labels visible: $($labels.idOnlyLabels)",
    "- Runtime network requests allowed: $($labels.runtimeNetworkRequestsAllowed)",
    "- Final status: $(if ($labelPassed) { 'validated_by_runtime_npc_label_player_smoke' } else { 'failed_player_log' })",
    "",
    "The Unity runtime reads `Assets/Data/P10/newmap_name_cache.json` and does not perform online lookups."
)
$labelDoc | Set-Content -LiteralPath $labelDocPath -Encoding UTF8

Write-Host "[PASS] Runtime/NPC/Label Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
