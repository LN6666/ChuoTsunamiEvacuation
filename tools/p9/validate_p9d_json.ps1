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

function Assert-BooleanTrue {
    param([object]$Value, [string]$Context)
    if (-not [bool]$Value) {
        throw "$Context must be true."
    }
}

function Assert-BooleanFalse {
    param([object]$Value, [string]$Context)
    if ([bool]$Value) {
        throw "$Context must be false."
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

function Test-CoordinateAnchoringConfig {
    $config = Read-JsonFile "Assets/Data/P9/p9d_coordinate_anchoring_config.json"
    Assert-RequiredText $config.coordinatePolicy "coordinatePolicy"
    if ($config.coordinatePolicy -notlike "*proxy*" -or $config.coordinatePolicy -notlike "*nearest*") {
        throw "coordinatePolicy must label proxy/nearest-match anchoring."
    }
    Assert-BooleanTrue $config.coordinateBasedProxyRequired "coordinateBasedProxyRequired"
    Assert-BooleanTrue $config.nearestMatchAllowed "nearestMatchAllowed"
    Assert-BooleanFalse $config.exactPlateauObjectIdentityProofAvailable "exactPlateauObjectIdentityProofAvailable"
    Assert-BooleanFalse $config.claimsOfficialRouteStatus "claimsOfficialRouteStatus"
    Assert-BooleanFalse $config.claimsExactPlateauObjectBinding "claimsExactPlateauObjectBinding"
    Assert-BooleanTrue $config.p8HandoffConsumed "p8HandoffConsumed"
    if ([double]$config.maxNearestMatchDistanceMeters -le 0) {
        throw "maxNearestMatchDistanceMeters must be positive."
    }

    $records = @($config.anchors)
    if ($records.Count -lt 6) {
        throw "P9-D coordinate anchoring config requires at least six sample anchors."
    }
    $seen = @{}
    $types = @{}
    foreach ($record in $records) {
        Assert-UniqueId $seen $record.anchorId "P9-D anchor id"
        Assert-BooleanFalse $record.routeIsOfficial "anchor $($record.anchorId) routeIsOfficial"
        Assert-BooleanFalse $record.safeApprovedByDefault "anchor $($record.anchorId) safeApprovedByDefault"
        $types[$record.anchorType] = $true
        if ([bool]$record.isHumanitarianCandidate) {
            Assert-BooleanFalse $record.isOfficialShelter "humanitarian anchor $($record.anchorId) isOfficialShelter"
            Assert-BooleanTrue $record.nonOfficialWarningRequired "humanitarian anchor $($record.anchorId) nonOfficialWarningRequired"
        }
        if ($record.anchorType -eq "route_guidance_proxy") {
            Assert-BooleanTrue $record.routeEstimatedPrototypeGuidance "route proxy estimated guidance"
        }
    }

    foreach ($requiredType in @(
        "official_shelter_marker",
        "entrance_proxy",
        "safe_floor_proxy_target",
        "route_guidance_proxy",
        "hazard_grid_lookup",
        "spawn_zone_relation"
    )) {
        if (-not $types.ContainsKey($requiredType)) {
            throw "Missing P9-D anchor type: $requiredType"
        }
    }
}

function Test-AnchoringReportSample {
    $report = Read-JsonFile "Assets/Data/P9/p9d_anchoring_report_sample.json"
    if ([int]$report.humanitarianCandidateTotal -ne 110) {
        throw "P9-D report must track 110 humanitarian candidates."
    }
    if ([int]$report.namedHumanitarianCandidateCount -ne 28 -or [int]$report.idOnlyHumanitarianCandidateCount -ne 82) {
        throw "P9-D report must track 28 named and 82 ID-only humanitarian candidates."
    }
    Assert-BooleanTrue $report.allHumanitarianCandidatesRemainNonOfficial "allHumanitarianCandidatesRemainNonOfficial"
    Assert-BooleanTrue $report.allHumanitarianCandidatesRequireWarning "allHumanitarianCandidatesRequireWarning"
    Assert-BooleanTrue $report.routesRemainEstimatedPrototypeGuidance "routesRemainEstimatedPrototypeGuidance"
    Assert-BooleanFalse $report.exactPlateauObjectIdentityClaimed "exactPlateauObjectIdentityClaimed"
    Assert-BooleanTrue $report.p8HandoffConsumed "report p8HandoffConsumed"
}

function Test-FinalGameplayScenarioSample {
    $sample = Read-JsonFile "Assets/Data/P9/p9d_final_gameplay_scenario_sample.json"
    Assert-BooleanTrue $sample.p8HandoffConsumed "scenario p8HandoffConsumed"
    Assert-BooleanTrue $sample.fullGameplayChainRequired "fullGameplayChainRequired"
    $records = @($sample.scenarios)
    if ($records.Count -lt 7) {
        throw "P9-D final gameplay sample requires at least seven scenarios."
    }
    $seen = @{}
    foreach ($record in $records) {
        Assert-UniqueId $seen $record.scenarioId "P9-D scenario id"
        Assert-RequiredText $record.expectedOutcome "scenario expectedOutcome"
        Assert-RequiredText $record.expectedFinalReasonCode "scenario expectedFinalReasonCode"
        if ([double]$record.collapseDebrisFatalityProbability -lt 0 -or [double]$record.collapseDebrisFatalityProbability -gt 1) {
            throw "scenario $($record.scenarioId) collapseDebrisFatalityProbability must be 0..1."
        }
    }
    foreach ($required in @(
        "p9d_flow_success",
        "p9d_flow_congestion_delay",
        "p9d_flow_entrance_blocked_failure",
        "p9d_flow_safe_floor_failure",
        "p9d_flow_crowd_delay_hazard_failure",
        "p9d_flow_collapse_debris_fatality",
        "p9d_flow_collapse_disabled_success"
    )) {
        if (-not $seen.ContainsKey($required)) {
            throw "Missing P9-D final gameplay scenario: $required"
        }
    }
}

function Test-P10HandoffStatus {
    $handoff = Read-JsonFile "Assets/Data/P9/p9d_p10_handoff_status.json"
    Assert-BooleanTrue $handoff.p9Complete "p9Complete"
    Assert-BooleanFalse $handoff.p9EfgCreated "p9EfgCreated"
    Assert-BooleanFalse $handoff.p10ReleasePackagingStarted "p10ReleasePackagingStarted"
    Assert-BooleanTrue $handoff.windowsExeProfilingRequiredInP10 "windowsExeProfilingRequiredInP10"
    Assert-BooleanTrue $handoff.highDetailSceneArchiveRequiredInP10 "highDetailSceneArchiveRequiredInP10"
    Assert-BooleanTrue $handoff.officialInundationContourRefinementDeferredToP10 "officialInundationContourRefinementDeferredToP10"
    Assert-BooleanTrue $handoff.fullPlateauObjectSemanticCoverageDeferred "fullPlateauObjectSemanticCoverageDeferred"
    Assert-BooleanTrue $handoff.officialRouteValidationDeferred "officialRouteValidationDeferred"
}

Write-Host "P9-D JSON validation: starting"
Test-CoordinateAnchoringConfig
Test-AnchoringReportSample
Test-FinalGameplayScenarioSample
Test-P10HandoffStatus
Write-Host "P9-D JSON validation: PASS" -ForegroundColor Green
exit 0
