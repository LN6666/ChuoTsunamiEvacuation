[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$dataRoot = Join-Path $repoRoot "Assets\Data\P9"

function Read-JsonFile {
    param([string]$RelativePath)

    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing JSON file: $RelativePath"
    }

    try {
        return Get-Content -Raw -LiteralPath $path | ConvertFrom-Json
    }
    catch {
        throw "Invalid JSON in $RelativePath. $($_.Exception.Message)"
    }
}

function Assert-Allowed {
    param(
        [string]$Value,
        [string[]]$Allowed,
        [string]$Context
    )

    if ([string]::IsNullOrWhiteSpace($Value) -or ($Allowed -notcontains $Value)) {
        throw "$Context has unsupported value '$Value'. Allowed: $($Allowed -join ', ')"
    }
}

function Assert-UniqueRequiredId {
    param(
        [hashtable]$Seen,
        [string]$Id,
        [string]$Context
    )

    if ([string]::IsNullOrWhiteSpace($Id)) {
        throw "$Context requires a non-empty id."
    }

    if ($Seen.ContainsKey($Id)) {
        throw "$Context duplicate id: $Id"
    }

    $Seen[$Id] = $true
}

function Assert-Position {
    param(
        [object]$Position,
        [string]$Context
    )

    if ($null -eq $Position) {
        throw "$Context requires a position object."
    }

    foreach ($field in @("x", "y", "z")) {
        if ($null -eq $Position.$field) {
            throw "$Context position.$field is required."
        }
    }
}

function Assert-NumberNotNegative {
    param(
        [double]$Value,
        [string]$Context
    )

    if ($Value -lt 0) {
        throw "$Context must be non-negative."
    }
}

function Test-SpawnData {
    $spawn = Read-JsonFile "Assets/Data/P9/p9_spawn_points_sample.json"
    Assert-Allowed $spawn.sourceMode @("test", "rule_based", "manual_sample", "p8_handoff") "spawn collection sourceMode"
    $records = @($spawn.spawnPoints)
    if ($records.Count -lt 1) {
        throw "Spawn sample requires at least one spawn point."
    }

    $allowedTypes = @("player_start", "resident_origin", "office_worker_origin", "station_exit", "underground_exit", "waterfront_origin", "test_origin")
    $allowedModes = @("test", "rule_based", "manual_sample", "p8_handoff")
    $seen = @{}
    foreach ($record in $records) {
        Assert-UniqueRequiredId $seen $record.spawnPointId "spawnPointId"
        Assert-Allowed $record.spawnType $allowedTypes "spawnType for $($record.spawnPointId)"
        Assert-Allowed $record.sourceMode $allowedModes "sourceMode for $($record.spawnPointId)"
        Assert-Position $record.position "spawn point $($record.spawnPointId)"
        if ($null -eq $record.coordinates -or [string]::IsNullOrWhiteSpace($record.coordinates.coordinateSystem)) {
            throw "spawn point $($record.spawnPointId) requires coordinates.coordinateSystem."
        }
        Assert-NumberNotNegative ([double]$record.capacity) "capacity for $($record.spawnPointId)"
        Assert-NumberNotNegative ([double]$record.weight) "weight for $($record.spawnPointId)"
        if ($null -eq $record.activeScenarioIds) {
            throw "spawn point $($record.spawnPointId) requires activeScenarioIds."
        }
    }
}

function Test-CrowdAgentData {
    $agents = Read-JsonFile "Assets/Data/P9/p9_crowd_agents_sample.json"
    Assert-Allowed $agents.sourceMode @("test", "rule_based", "manual_sample", "p8_handoff") "crowd collection sourceMode"
    $records = @($agents.agentProfiles)
    if ($records.Count -lt 1) {
        throw "Crowd agent sample requires at least one profile."
    }

    $allowedTypes = @("resident", "office_worker", "visitor", "elderly_proxy", "test_agent")
    $allowedModes = @("test", "rule_based", "manual_sample", "p8_handoff")
    $seen = @{}
    foreach ($record in $records) {
        Assert-UniqueRequiredId $seen $record.agentProfileId "agentProfileId"
        Assert-Allowed $record.agentType $allowedTypes "agentType for $($record.agentProfileId)"
        Assert-Allowed $record.sourceMode $allowedModes "sourceMode for $($record.agentProfileId)"
        Assert-NumberNotNegative ([double]$record.speedMetersPerSecond) "speedMetersPerSecond for $($record.agentProfileId)"
        Assert-NumberNotNegative ([double]$record.crowdRadius) "crowdRadius for $($record.agentProfileId)"
        if ([double]$record.panicLevelProxy -lt 0 -or [double]$record.panicLevelProxy -gt 1) {
            throw "panicLevelProxy for $($record.agentProfileId) must be 0..1."
        }
    }
}

function Test-EntranceProxyData {
    $proxies = Read-JsonFile "Assets/Data/P9/p9_entrance_safe_floor_proxy_sample.json"
    Assert-Allowed $proxies.sourceMode @("test", "rule_based", "manual_sample", "p8_handoff") "proxy collection sourceMode"
    $records = @($proxies.proxies)
    if ($records.Count -lt 1) {
        throw "Entrance proxy sample requires at least one proxy."
    }

    $allowedStatuses = @("available", "warning", "restricted", "blocked_proxy", "unknown")
    $allowedModes = @("external_proxy", "entrance_marker", "safe_floor_status")
    $seen = @{}
    foreach ($record in $records) {
        Assert-UniqueRequiredId $seen $record.proxyId "proxyId"
        Assert-Position $record.entrancePosition "entrance proxy $($record.proxyId)"
        Assert-Allowed $record.verticalEvacuationStatus $allowedStatuses "verticalEvacuationStatus for $($record.proxyId)"
        Assert-Allowed $record.interactionMode $allowedModes "interactionMode for $($record.proxyId)"

        if ([bool]$record.humanitarianCandidateFlag) {
            if ([bool]$record.isOfficialShelter) {
                throw "Humanitarian candidate must remain non-official: $($record.proxyId)"
            }

            if (-not [bool]$record.nonOfficialWarningRequired) {
                throw "Humanitarian candidate requires non-official warning: $($record.proxyId)"
            }
        }
    }
}

Write-Host "P9 JSON validation: starting"
foreach ($schemaFile in @(
    "Assets/Data/P9/p9_spawn_point_schema.json",
    "Assets/Data/P9/p9_crowd_agent_schema.json",
    "Assets/Data/P9/p9_entrance_safe_floor_proxy_schema.json"
)) {
    Read-JsonFile $schemaFile | Out-Null
}

Test-SpawnData
Test-CrowdAgentData
Test-EntranceProxyData

Write-Host "P9 JSON validation: PASS" -ForegroundColor Green
exit 0
