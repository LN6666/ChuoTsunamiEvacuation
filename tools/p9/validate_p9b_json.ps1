[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

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

function Assert-BooleanFalse {
    param([object]$Value, [string]$Context)
    if ([bool]$Value) {
        throw "$Context must be false."
    }
}

function Assert-BooleanTrue {
    param([object]$Value, [string]$Context)
    if (-not [bool]$Value) {
        throw "$Context must be true."
    }
}

function Assert-RequiredText {
    param([string]$Value, [string]$Context)
    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "$Context is required."
    }
}

function Assert-UniqueId {
    param([hashtable]$Seen, [string]$Id, [string]$Context)
    Assert-RequiredText $Id $Context
    if ($Seen.ContainsKey($Id)) {
        throw "Duplicate $Context '$Id'."
    }
    $Seen[$Id] = $true
}

function Assert-Position {
    param([object]$Position, [string]$Context)
    if ($null -eq $Position) {
        throw "$Context requires a position object."
    }
    foreach ($field in @("x", "y", "z")) {
        if ($null -eq $Position.$field) {
            throw "$Context position.$field is required."
        }
    }
}

function Test-WeightedSpawn {
    $config = Read-JsonFile "Assets/Data/P9/p9b_weighted_spawn_config.json"
    Assert-RequiredText $config.scenarioPresetId "weighted spawn scenarioPresetId"
    if ([int]$config.spawnCountCap -le 0) {
        throw "weighted spawn spawnCountCap must be positive."
    }
    Assert-BooleanTrue $config.excludeAlreadyFloodedZones "excludeAlreadyFloodedZones"
    Assert-BooleanTrue $config.rejectUnsafeZeroPositions "rejectUnsafeZeroPositions"

    $zones = Read-JsonFile "Assets/Data/P9/p9b_spawn_zones_sample.json"
    $records = @($zones.zones)
    if ($records.Count -lt 6) {
        throw "P9-B spawn zones require at least six records."
    }

    $seen = @{}
    $hasHighRisk = $false
    $hasBlocked = $false
    $hasFlooded = $false
    $hasInvalidZero = $false
    foreach ($record in $records) {
        Assert-UniqueId $seen $record.zoneId "spawn zone id"
        Assert-Position $record.position "spawn zone $($record.zoneId)"
        if ([int]$record.capacity -lt 0) {
            throw "spawn zone $($record.zoneId) capacity must be non-negative."
        }
        if ([double]$record.baseWeight -lt 0) {
            throw "spawn zone $($record.zoneId) baseWeight must be non-negative."
        }
        if ([bool]$record.isCoastalOrWaterfront -and [bool]$record.isLowElevation -and [bool]$record.hasHighInundationExposure) {
            $hasHighRisk = $true
        }
        if ([bool]$record.explicitlyBlocked) { $hasBlocked = $true }
        if ([bool]$record.alreadyFlooded) { $hasFlooded = $true }
        if (-not [bool]$record.validPosition -and [double]$record.position.x -eq 0 -and [double]$record.position.z -eq 0) {
            $hasInvalidZero = $true
        }
    }
    Assert-BooleanTrue $hasHighRisk "spawn zones high-risk coverage"
    Assert-BooleanTrue $hasBlocked "spawn zones blocked exclusion fixture"
    Assert-BooleanTrue $hasFlooded "spawn zones flooded exclusion fixture"
    Assert-BooleanTrue $hasInvalidZero "spawn zones unsafe zero fixture"
}

function Test-CrowdScenario {
    $scenario = Read-JsonFile "Assets/Data/P9/p9b_runtime_crowd_scenario_sample.json"
    Assert-RequiredText $scenario.scenarioId "crowd scenarioId"
    if ([int]$scenario.maxSpawnedAgents -le 0 -or [int]$scenario.maxActiveCrowdAgents -le 0) {
        throw "crowd max agent counts must be positive."
    }
    if ([int]$scenario.maxSpawnedAgents -gt 64 -or [int]$scenario.maxActiveCrowdAgents -gt 64) {
        throw "P9-B crowd cap must remain lightweight."
    }
    Assert-BooleanTrue $scenario.metricsOnlyNoFailureEffect "crowd metricsOnlyNoFailureEffect"
}

function Test-HumanitarianMarkers {
    $config = Read-JsonFile "Assets/Data/P9/p9b_humanitarian_marker_runtime_config.json"
    if ([int]$config.expectedTotalCandidates -ne 110) {
        throw "P9-B humanitarian runtime config must expect 110 candidates."
    }
    Assert-BooleanFalse $config.isOfficialShelter "humanitarian runtime config isOfficialShelter"
    Assert-BooleanTrue $config.nonOfficialWarningRequired "humanitarian runtime config nonOfficialWarningRequired"
    Assert-BooleanFalse $config.selectableGameplayEnabled "humanitarian runtime config selectableGameplayEnabled"
    Assert-BooleanFalse $config.affectsGameplaySuccessFailure "humanitarian runtime config affectsGameplaySuccessFailure"

    $handoff = Read-JsonFile "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json"
    if ([int]$handoff.totalCandidates -ne 110 -or @($handoff.records).Count -ne 110) {
        throw "P8-E humanitarian marker handoff must contain 110 records."
    }
    if ([int]$handoff.namedMarkerCount -ne 28 -or [int]$handoff.idOnlyMarkerCount -ne 82) {
        throw "P8-E humanitarian marker named/id-only counts must be 28/82."
    }
    Assert-BooleanTrue $handoff.allCandidatesNonOfficial "P8-E allCandidatesNonOfficial"
    Assert-BooleanTrue $handoff.allCandidatesRequireNonOfficialWarning "P8-E allCandidatesRequireNonOfficialWarning"
    foreach ($record in @($handoff.records)) {
        Assert-BooleanFalse $record.isOfficialShelter "humanitarian candidate $($record.candidateId) isOfficialShelter"
        Assert-BooleanTrue $record.nonOfficialWarningRequired "humanitarian candidate $($record.candidateId) nonOfficialWarningRequired"
        Assert-BooleanFalse $record.selectableGameplayEnabled "humanitarian candidate $($record.candidateId) selectableGameplayEnabled"
        Assert-BooleanFalse $record.affectsGameplaySuccessFailure "humanitarian candidate $($record.candidateId) affectsGameplaySuccessFailure"
    }
}

function Test-EntranceMarkers {
    $data = Read-JsonFile "Assets/Data/P9/p9b_entrance_marker_assignments_sample.json"
    $records = @($data.proxies)
    if ($records.Count -lt 3) {
        throw "P9-B entrance marker sample requires at least three proxies."
    }
    $seen = @{}
    foreach ($record in $records) {
        Assert-UniqueId $seen $record.proxyId "entrance proxy id"
        Assert-Position $record.entrancePosition "entrance proxy $($record.proxyId)"
        if ([bool]$record.humanitarianCandidateFlag) {
            Assert-BooleanFalse $record.isOfficialShelter "humanitarian entrance proxy $($record.proxyId) isOfficialShelter"
            Assert-BooleanTrue $record.nonOfficialWarningRequired "humanitarian entrance proxy $($record.proxyId) nonOfficialWarningRequired"
        }
    }
}

function Test-CollapseZones {
    $data = Read-JsonFile "Assets/Data/P9/p9b_collapse_debris_risk_zones_sample.json"
    $records = @($data.zones)
    if ($records.Count -lt 1) {
        throw "P9-B collapse/debris zones require at least one record."
    }
    $seen = @{}
    foreach ($record in $records) {
        Assert-UniqueId $seen $record.zoneId "collapse/debris zone id"
        Assert-Position $record.position "collapse/debris zone $($record.zoneId)"
        Assert-BooleanTrue $record.warningOnly "collapse/debris zone $($record.zoneId) warningOnly"
        Assert-BooleanFalse $record.affectsGameplaySuccessFailure "collapse/debris zone $($record.zoneId) affectsGameplaySuccessFailure"
        Assert-BooleanFalse $record.canKillPlayer "collapse/debris zone $($record.zoneId) canKillPlayer"
        Assert-BooleanTrue $record.noPhysicsCollapse "collapse/debris zone $($record.zoneId) noPhysicsCollapse"
    }
}

function Test-P8RouteAndSemanticHandoff {
    $route = Read-JsonFile "Assets/Data/P8/p8e_route_candidate_geometry_handoff.json"
    Assert-BooleanFalse $route.routesAreOfficial "P8-E routesAreOfficial"
    Assert-BooleanTrue $route.routesAreEstimatedPrototypeGuidance "P8-E routesAreEstimatedPrototypeGuidance"
    Assert-BooleanFalse $route.routeRoadGeometryValidated "P8-E routeRoadGeometryValidated"
    Assert-BooleanTrue $route.candidatesProxyOrDataBound "P8-E candidatesProxyOrDataBound"

    $semantic = Read-JsonFile "Assets/Data/P8/p8e_semantic_binding_v1.json"
    Assert-BooleanFalse $semantic.completeSceneSemanticBinding "P8-E completeSceneSemanticBinding"
    Assert-BooleanFalse $semantic.sceneMutationPerformed "P8-E sceneMutationPerformed"
    if (@($semantic.bindings).Count -lt 1) {
        throw "P8-E semantic binding requires at least one binding."
    }
}

Write-Host "P9-B JSON validation: starting"
Test-WeightedSpawn
Test-CrowdScenario
Test-HumanitarianMarkers
Test-EntranceMarkers
Test-CollapseZones
Test-P8RouteAndSemanticHandoff
Write-Host "P9-B JSON validation: PASS" -ForegroundColor Green
exit 0
