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

function Assert-ArrayContains {
    param([object[]]$Values, [string]$Expected, [string]$Context)
    if (@($Values) -notcontains $Expected) {
        throw "$Context must include '$Expected'."
    }
}

function Assert-AllowedHardeningClassification {
    param([string]$Value, [string]$Context)
    $allowed = @(
        "closed_by_p10a_plus",
        "improved_to_coordinate_validated_proxy",
        "improved_to_nearest_match_validated_proxy",
        "visual_qa_ready_for_p10b",
        "still_known_limitation",
        "future_gis_refinement",
        "p10b_runtime_validation_required",
        "p10c_archive_required",
        "p10d_final_review_required"
    )
    if ($allowed -notcontains $Value) {
        throw "$Context has invalid hardening classification '$Value'."
    }
}

function Test-HardeningMatrix {
    $matrix = Read-JsonFile "Assets/Data/P10/p10a_plus_hardening_matrix.json"
    Assert-BooleanTrue $matrix.officialP10StageCountRemainsFour "officialP10StageCountRemainsFour"
    Assert-BooleanTrue $matrix.noP10EfgCreated "noP10EfgCreated"
    Assert-BooleanTrue $matrix.noNewLargeGameplaySystem "noNewLargeGameplaySystem"
    Assert-BooleanTrue $matrix.noP7P8P9Reimplementation "noP7P8P9Reimplementation"
    Assert-BooleanTrue $matrix.noP10CArchiveOrReleaseWork "noP10CArchiveOrReleaseWork"
    $records = @($matrix.items)
    if ($records.Count -lt 11) {
        throw "P10-A+ hardening matrix must classify the requested hardening items."
    }
    $seen = @{}
    foreach ($record in $records) {
        Assert-RequiredText $record.id "hardening item id"
        if ($seen.ContainsKey($record.id)) {
            throw "Duplicate hardening matrix item '$($record.id)'."
        }
        $seen[$record.id] = $true
        Assert-AllowedHardeningClassification $record.classification "hardening item $($record.id)"
        Assert-RequiredText $record.evidence "hardening item $($record.id) evidence"
    }
    foreach ($required in @(
        "110_humanitarian_candidate_marker_anchoring",
        "candidate_to_building_binding",
        "entrance_proxy_placement",
        "route_candidate_coordinate_geometry",
        "p5_route_road_geometry_validation",
        "plateau_semantic_object_binding",
        "hazard_front_light_curtain_visual_qa",
        "result_panel_long_warning_text",
        "p2_p9_high_detail_scene_smoke",
        "windows_exe_profiling_readiness",
        "high_detail_scene_archive"
    )) {
        if (-not $seen.ContainsKey($required)) {
            throw "Missing hardening matrix item: $required"
        }
    }
}

function Test-CandidateAnchorReport {
    $report = Read-JsonFile "Assets/Data/P10/p10a_plus_candidate_anchor_hardening_report.json"
    if ([int]$report.totalAnchorTargetsVerified -ne 116) {
        throw "Candidate anchor hardening report must verify 116 total anchor targets."
    }
    if ([int]$report.humanitarianCandidateTotal -ne 110) {
        throw "Candidate anchor hardening report must verify 110 humanitarian candidates."
    }
    if ([int]$report.namedHumanitarianCandidateCount -ne 28 -or [int]$report.idOnlyHumanitarianCandidateCount -ne 82) {
        throw "Candidate anchor hardening report must preserve 28 named and 82 ID-only counts."
    }
    Assert-BooleanTrue $report.allHumanitarianCandidatesRemainNonOfficial "allHumanitarianCandidatesRemainNonOfficial"
    Assert-BooleanTrue $report.allHumanitarianCandidatesRequireWarning "allHumanitarianCandidatesRequireWarning"
    Assert-BooleanFalse $report.exactPlateauUnityObjectIdentityProven "exactPlateauUnityObjectIdentityProven"
    if (@($report.records).Count -ne 110) {
        throw "Candidate anchor hardening report must contain 110 candidate records."
    }
    foreach ($record in @($report.records)) {
        Assert-BooleanFalse $record.isOfficialShelter "candidate $($record.candidateId) isOfficialShelter"
        Assert-BooleanTrue $record.nonOfficialWarningRequired "candidate $($record.candidateId) nonOfficialWarningRequired"
        Assert-BooleanFalse $record.safeApprovedByDefault "candidate $($record.candidateId) safeApprovedByDefault"
        Assert-BooleanFalse $record.exactPlateauUnityObjectIdentityProven "candidate $($record.candidateId) exactPlateauUnityObjectIdentityProven"
        Assert-RequiredText $record.coordinateStatus "candidate $($record.candidateId) coordinateStatus"
        Assert-RequiredText $record.anchorStatus "candidate $($record.candidateId) anchorStatus"
        Assert-RequiredText $record.confidence "candidate $($record.candidateId) confidence"
    }
}

function Test-NearestMatchReport {
    $report = Read-JsonFile "Assets/Data/P10/p10a_plus_candidate_to_building_nearest_match_report.json"
    if ([int]$report.candidateTotal -ne 110) {
        throw "Nearest-match report must contain 110 candidates."
    }
    Assert-BooleanFalse $report.exactPlateauUnityObjectIdentityProven "nearest-match exactPlateauUnityObjectIdentityProven"
    Assert-RequiredText $report.sourceGeometryLimitation "nearest-match sourceGeometryLimitation"
    foreach ($record in @($report.records)) {
        Assert-RequiredText $record.candidateId "nearest-match candidateId"
        Assert-RequiredText $record.thresholdResult "nearest-match thresholdResult"
        Assert-BooleanFalse $record.exactPlateauUnityObjectIdentityProven "nearest-match candidate $($record.candidateId) exactPlateauUnityObjectIdentityProven"
        if (($record.limitation -as [string]) -notlike "*exact PLATEAU Unity object identity not proven*") {
            throw "Nearest-match candidate $($record.candidateId) must preserve exact-identity limitation."
        }
    }
}

function Test-EntranceProxyReport {
    $report = Read-JsonFile "Assets/Data/P10/p10a_plus_entrance_proxy_hardening_report.json"
    if ([int]$report.humanitarianEntranceProxyCount -ne 110) {
        throw "Entrance proxy report must cover 110 humanitarian entrance proxies."
    }
    if ([int]$report.officialShelterEntranceProxySamples -lt 1) {
        throw "Entrance proxy report must include the available official shelter entrance proxy sample."
    }
    Assert-BooleanFalse $report.trueEntranceGeometryProven "trueEntranceGeometryProven"
    foreach ($record in @($report.records)) {
        Assert-RequiredText $record.entranceProxyStatus "entrance proxy status"
        Assert-BooleanFalse $record.isOfficialShelter "entrance proxy $($record.targetId) isOfficialShelter"
        Assert-BooleanFalse $record.trueEntranceGeometryProven "entrance proxy $($record.targetId) trueEntranceGeometryProven"
    }
    foreach ($record in @($report.officialShelterRecords)) {
        Assert-RequiredText $record.entranceProxyStatus "official entrance proxy status"
        Assert-BooleanTrue $record.isOfficialShelter "official entrance proxy $($record.targetId) isOfficialShelter"
        Assert-BooleanFalse $record.nonOfficialWarningRequired "official entrance proxy $($record.targetId) nonOfficialWarningRequired"
        Assert-BooleanFalse $record.trueEntranceGeometryProven "official entrance proxy $($record.targetId) trueEntranceGeometryProven"
    }
}

function Test-RouteProxyReport {
    $report = Read-JsonFile "Assets/Data/P10/p10a_plus_route_proxy_validation_report.json"
    if ([int]$report.routeTotal -lt 100) {
        throw "Route proxy validation report should cover the P5 OSM route sample."
    }
    if ([int]$report.officialRouteClaimCount -ne 0) {
        throw "Route proxy validation report must not contain official route claims."
    }
    Assert-BooleanFalse $report.routeRoadGeometryValidated "routeRoadGeometryValidated"
    Assert-BooleanTrue $report.routesAreEstimatedPrototypeGuidance "routesAreEstimatedPrototypeGuidance"
    foreach ($required in @(
        "estimated prototype guidance",
        "not official evacuation routes",
        "not fully road-geometry validated",
        "coordinate-projected gameplay proxy"
    )) {
        if (($report.requiredWording -as [string]) -notlike "*$required*") {
            throw "Route report required wording must include '$required'."
        }
    }
    foreach ($record in @($report.records)) {
        Assert-BooleanFalse $record.isOfficialEvacuationRoute "route $($record.routeId) isOfficialEvacuationRoute"
        Assert-BooleanFalse $record.routeRoadGeometryValidated "route $($record.routeId) routeRoadGeometryValidated"
        if ([int]$record.routePointCount -lt 2) {
            throw "route $($record.routeId) must have at least two route points."
        }
    }
}

function Test-SemanticAudit {
    $audit = Read-JsonFile "Assets/Data/P10/p10a_plus_plateau_semantic_binding_audit.json"
    Assert-BooleanFalse $audit.fullSceneSemanticCoverageProven "fullSceneSemanticCoverageProven"
    foreach ($classification in @(
        "proven_scene_object_binding",
        "coordinate_proxy_binding",
        "metadata_proxy_binding",
        "data_only_binding",
        "insufficient_evidence"
    )) {
        Assert-ArrayContains @($audit.classifications) $classification "semantic classifications"
    }
    $categories = @($audit.records | ForEach-Object { $_.category })
    foreach ($required in @("road", "building", "bridge", "underground", "entrance")) {
        Assert-ArrayContains $categories $required "semantic audit categories"
    }
}

function Test-HighDetailSmoke {
    $status = Read-JsonFile "Assets/Data/P10/p10a_plus_high_detail_smoke_status.json"
    Assert-BooleanTrue $status.sceneExists "sceneExists"
    Assert-BooleanFalse $status.sceneTouched "sceneTouched"
    Assert-BooleanFalse $status.sceneMutationPerformed "sceneMutationPerformed"
    Assert-BooleanTrue $status.p9RuntimeSceneSafe "p9RuntimeSceneSafe"
    Assert-BooleanTrue $status.markerGenerationWithoutSceneMutation "markerGenerationWithoutSceneMutation"
    Assert-BooleanTrue $status.p10bRuntimeValidationRequired "p10bRuntimeValidationRequired"
    Assert-BooleanTrue $status.p10cArchiveRequired "p10cArchiveRequired"
}

function Test-P10BReadinessUpdate {
    $readiness = Read-JsonFile "Assets/Data/P10/p10a_p10b_readiness_checklist.json"
    Assert-BooleanFalse $readiness.p10bBuildStarted "p10bBuildStarted"
    Assert-BooleanFalse $readiness.p10cReleasePackagingStarted "p10cReleasePackagingStarted"
    Assert-BooleanTrue $readiness.p10aPlusHardeningApplied "p10aPlusHardeningApplied"
    Assert-BooleanTrue $readiness.beforeAfterMetricsRequiredForOptimization "beforeAfterMetricsRequiredForOptimization"
    foreach ($metric in @(
        "FPS",
        "1_percent_low_or_stutter",
        "CPU_usage",
        "memory_usage",
        "GC_allocations_if_available",
        "loading_time",
        "Player_log_errors_warnings",
        "NPC_count",
        "marker_count",
        "light_curtain_impact",
        "UI_ResultPanel_impact"
    )) {
        Assert-ArrayContains @($readiness.metricsToCollect) $metric "P10-B metricsToCollect"
    }
}

Write-Host "P10-A+ JSON validation: starting"
Test-HardeningMatrix
Test-CandidateAnchorReport
Test-NearestMatchReport
Test-EntranceProxyReport
Test-RouteProxyReport
Test-SemanticAudit
Test-HighDetailSmoke
Test-P10BReadinessUpdate
Write-Host "P10-A+ JSON validation: PASS" -ForegroundColor Green
exit 0
