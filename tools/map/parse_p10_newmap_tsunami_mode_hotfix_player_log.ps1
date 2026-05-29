param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\p10_newmap_tsunami_mode_hotfix_player_log_summary.json"
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
$hazardLine = [string](@($lines | Where-Object { $_ -match "NewMap tsunami configured" } | Select-Object -Last 1) | Select-Object -First 1)
$warningLine = [string](@($lines | Where-Object { $_ -match "warning_duration_seconds" -and $_ -match "phase=WARNING" } | Select-Object -Last 1) | Select-Object -First 1)
$activeLine = [string](@($lines | Where-Object { $_ -match "active tsunami state entered" } | Select-Object -Last 1) | Select-Object -First 1)
$warningSmoke = [string](@($lines | Where-Object { $_ -match "scenario=tsunami_warning_300s_before_active result=pass" } | Select-Object -Last 1) | Select-Object -First 1)
$buildingTouchSmoke = [string](@($lines | Where-Object { $_ -match "scenario=building_touch_e_entry result=pass" } | Select-Object -Last 1) | Select-Object -First 1)
$frontFailureSmoke = [string](@($lines | Where-Object { $_ -match "scenario=tsunami_front_failure result=pass" } | Select-Object -Last 1) | Select-Object -First 1)
$resultSmoke = [string](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -Last 1) | Select-Object -First 1)
$lifecycleLine = [string](@($lines | Where-Object { $_ -match "NewMap NPC lifecycle smoke" } | Select-Object -Last 1) | Select-Object -First 1)

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    task = "P10 NewMap tsunami mode hotfix"
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    concaveMeshTriggerErrorCount = @($lines | Where-Object { $_ -match "Triggers on concave MeshColliders are not supported" }).Count
    bootstrapSeen = -not [string]::IsNullOrWhiteSpace($bootstrapLine)
    selfAuditCompleted = -not [string]::IsNullOrWhiteSpace($resultSmoke)
    tsunamiWarning300SmokePassed = -not [string]::IsNullOrWhiteSpace($warningSmoke)
    buildingTouchEntrySmokePassed = -not [string]::IsNullOrWhiteSpace($buildingTouchSmoke)
    tsunamiFrontFailureSmokePassed = -not [string]::IsNullOrWhiteSpace($frontFailureSmoke)
    spawnValidationPassed = As-Bool (Get-TokenValue $bootstrapLine "spawnValidationPassed")
    spawnValidationSource = Get-TokenValue $bootstrapLine "spawnValidationSource"
    spawnRandomSeedUsed = As-Int (Get-TokenValue $bootstrapLine "spawnRandomSeedUsed")
    spawnDeterministicSeed = As-Bool (Get-TokenValue $bootstrapLine "spawnDeterministicSeed")
    spawnX = As-Double (Get-TokenValue $bootstrapLine "spawnX")
    spawnY = As-Double (Get-TokenValue $bootstrapLine "spawnY")
    spawnZ = As-Double (Get-TokenValue $bootstrapLine "spawnZ")
    airWallColliders = As-Int (Get-TokenValue $bootstrapLine "airWallColliders")
    boundaryAirWallsPreserved = As-Int (Get-TokenValue $bootstrapLine "boundaryAirWallsPreserved")
    unexpectedAirwallBlockers = As-Int (Get-TokenValue $bootstrapLine "unexpectedAirwallBlockers")
    airwallBlockersConvertedToTrigger = As-Int (Get-TokenValue $bootstrapLine "airwallBlockersConvertedToTrigger")
    unknownBlockersInsidePlayableArea = As-Int (Get-TokenValue $bootstrapLine "unknownBlockersInsidePlayableArea")
    buildingEntryTriggers = As-Int (Get-TokenValue $bootstrapLine "buildingEntryTriggers")
    buildingEntryPhysicalBlockers = As-Int (Get-TokenValue $bootstrapLine "buildingEntryPhysicalBlockers")
    warningDurationSeconds = As-Double (Get-TokenValue $warningLine "warning_duration_seconds")
    warningStartTimeSeconds = As-Double (Get-TokenValue $warningLine "warning_start_time")
    activeTsunamiStartTimeSeconds = As-Double (Get-TokenValue $activeLine "tsunami_active_start_time")
    tsunamiStartSide = Get-TokenValue $hazardLine "startSide"
    tsunamiDirection = Get-TokenValue $hazardLine "direction"
    curtainHeightMeters = As-Double (Get-TokenValue $hazardLine "curtainHeight")
    curtainLengthMeters = As-Double (Get-TokenValue $hazardLine "curtainLength")
    globalRespawnCount = As-Int (Get-TokenValue $lifecycleLine "globalRespawnCount")
    allStopEventCount = As-Int (Get-TokenValue $lifecycleLine "allStopEventCount")
    stoppedWithoutReason = As-Int (Get-TokenValue $lifecycleLine "stoppedWithoutReason")
    finalStatus = if ($errors.Count -eq 0 -and $warnings.Count -eq 0 -and -not [string]::IsNullOrWhiteSpace($resultSmoke) -and -not [string]::IsNullOrWhiteSpace($warningSmoke) -and -not [string]::IsNullOrWhiteSpace($buildingTouchSmoke)) { "passed" } else { "failed" }
}

$outputFullPath = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $outputFullPath -Encoding UTF8

if ($summary.finalStatus -ne "passed") {
    Write-Host "[FAIL] P10 NewMap tsunami-mode Player.log validation failed. errors=$($errors.Count) warnings=$($warnings.Count)"
    exit 1
}

Write-Host "[PASS] P10 NewMap tsunami-mode Player.log clean. errors=0 warnings=0"
