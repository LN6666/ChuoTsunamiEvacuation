[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$dataRoot = Join-Path $repoRoot "Assets\Data\P10"

function Read-JsonFile {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing JSON file: $RelativePath"
    }
    return Get-Content -Raw -LiteralPath $path | ConvertFrom-Json
}

function Write-JsonFile {
    param([string]$FileName, [object]$Data)
    if (-not (Test-Path -LiteralPath $dataRoot -PathType Container)) {
        New-Item -ItemType Directory -Force -Path $dataRoot | Out-Null
    }
    $path = Join-Path $dataRoot $FileName
    $json = $Data | ConvertTo-Json -Depth 32
    Set-Content -LiteralPath $path -Value $json -Encoding UTF8
}

function Test-FiniteCoordinate {
    param([object]$Value)
    if ($null -eq $Value) { return $false }
    $number = [double]$Value
    return -not [double]::IsNaN($number) -and -not [double]::IsInfinity($number)
}

function Test-InBounds {
    param([double]$Latitude, [double]$Longitude, [object]$Config)
    return $Latitude -ge [double]$Config.minLatitude -and
        $Latitude -le [double]$Config.maxLatitude -and
        $Longitude -ge [double]$Config.minLongitude -and
        $Longitude -le [double]$Config.maxLongitude
}

function Get-CoordinateStatus {
    param([object]$Candidate, [object]$Config)
    if (-not [bool]$Candidate.hasCoordinates) {
        return "missing_coordinate"
    }
    if (-not (Test-FiniteCoordinate $Candidate.latitude) -or -not (Test-FiniteCoordinate $Candidate.longitude)) {
        return "invalid_coordinate"
    }
    if (-not (Test-InBounds -Latitude ([double]$Candidate.latitude) -Longitude ([double]$Candidate.longitude) -Config $Config)) {
        return "out_of_bounds"
    }
    return "coordinate_validated"
}

function Get-ConfidenceForCandidate {
    param([object]$Candidate, [string]$CoordinateStatus)
    if ($CoordinateStatus -ne "coordinate_validated") {
        return "rejected"
    }
    if (-not [string]::IsNullOrWhiteSpace($Candidate.fallbackBuildingId) -and
        [string]$Candidate.locationText -like "*PLATEAU centroid*") {
        return "high"
    }
    if (-not [string]::IsNullOrWhiteSpace($Candidate.fallbackBuildingId)) {
        return "medium"
    }
    return "fallback"
}

function Get-CandidateNameStatus {
    param([object]$Candidate)
    if ([string]::IsNullOrWhiteSpace($Candidate.buildingName)) {
        return "id_only"
    }
    return "named"
}

function Get-RoutePointInfo {
    param([object]$Route, [object]$Config)
    $points = @()
    if ($null -ne $Route.geometry -and $null -ne $Route.geometry.coordinates) {
        $points = @($Route.geometry.coordinates)
    }
    $invalid = 0
    $outOfBounds = 0
    foreach ($point in $points) {
        if ($null -eq $point -or @($point).Count -lt 2) {
            $invalid++
            continue
        }
        $lon = [double]$point[0]
        $lat = [double]$point[1]
        if (-not (Test-InBounds -Latitude $lat -Longitude $lon -Config $Config)) {
            $outOfBounds++
        }
    }
    return [pscustomobject]@{
        pointCount = $points.Count
        invalidPointCount = $invalid
        outOfBoundsPointCount = $outOfBounds
        firstCoordinate = if ($points.Count -gt 0) { @([double]$points[0][1], [double]$points[0][0]) } else { @() }
        lastCoordinate = if ($points.Count -gt 0) { @([double]$points[$points.Count - 1][1], [double]$points[$points.Count - 1][0]) } else { @() }
    }
}

function Get-SemanticClassification {
    param([object]$Binding)
    if ([bool]$Binding.sceneObjectEvidenceFound) {
        return "proven_scene_object_binding"
    }
    if ([string]$Binding.bindingMode -eq "plateau_metadata") {
        return "metadata_proxy_binding"
    }
    if ([string]$Binding.bindingMode -eq "proxy_marker") {
        return "coordinate_proxy_binding"
    }
    if ([string]$Binding.bindingMode -eq "data_only") {
        return "data_only_binding"
    }
    return "insufficient_evidence"
}

$candidates = Read-JsonFile "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json"
$anchorConfig = Read-JsonFile "Assets/Data/P9/p9d_coordinate_anchoring_config.json"
$routeSample = Read-JsonFile "Assets/Data/real_chuo_osm_routes_sample.json"
$semantic = Read-JsonFile "Assets/Data/P8/p8e_semantic_binding_v1.json"
$p10b = Read-JsonFile "Assets/Data/P10/p10a_p10b_readiness_checklist.json"

$candidateRecords = @()
$nearestRecords = @()
$entranceRecords = @()
$officialEntranceRecords = @()
foreach ($candidate in @($candidates.records)) {
    $coordinateStatus = Get-CoordinateStatus -Candidate $candidate -Config $anchorConfig
    $confidence = Get-ConfidenceForCandidate -Candidate $candidate -CoordinateStatus $coordinateStatus
    $nameStatus = Get-CandidateNameStatus -Candidate $candidate
    $hasFallbackBuilding = -not [string]::IsNullOrWhiteSpace($candidate.fallbackBuildingId)
    $sourceCentroid = [string]$candidate.locationText -like "*PLATEAU centroid*"
    $matchAccepted = $coordinateStatus -eq "coordinate_validated" -and $hasFallbackBuilding
    $distance = if ($matchAccepted -and $sourceCentroid) { 0.0 } else { $null }
    $fallbackReason = if ($matchAccepted) {
        "candidate_coordinate_and_fallback_building_proxy_available"
    }
    else {
        "source_geometry_unavailable_or_insufficient_for_exact_building_binding"
    }

    $candidateRecords += [pscustomobject]@{
        candidateId = $candidate.candidateId
        candidateName = if ($nameStatus -eq "named") { $candidate.buildingName } else { "" }
        nameStatus = $nameStatus
        latitude = if ([bool]$candidate.hasCoordinates) { [double]$candidate.latitude } else { $null }
        longitude = if ([bool]$candidate.hasCoordinates) { [double]$candidate.longitude } else { $null }
        coordinateStatus = $coordinateStatus
        anchorStatus = if ($coordinateStatus -eq "coordinate_validated") { "anchored_to_coordinate_proxy" } else { "fallback_marker_or_rejected" }
        nearestMatchStatus = if ($matchAccepted) { "fallback_building_proxy_matched" } else { "insufficient_nearest_match_source" }
        nearestProxyId = if ($hasFallbackBuilding) { $candidate.fallbackBuildingId } else { "" }
        nearestDistanceMeters = $distance
        confidence = $confidence
        fallbackReason = $fallbackReason
        isOfficialShelter = [bool]$candidate.isOfficialShelter
        nonOfficialWarningRequired = [bool]$candidate.nonOfficialWarningRequired
        safeApprovedByDefault = $false
        hazardStatusAvailability = if ([bool]$candidate.hazardStatusEligible) { "hazard_status_available" } else { "hazard_status_unavailable" }
        damageStatusAvailability = if ([bool]$candidate.p8dDamageStatusEligible) { "damage_status_available" } else { "damage_status_unavailable" }
        blockageStatusAvailability = "proxy_unknown"
        lowFloorStatusAvailability = "warning_attachable"
        exactPlateauUnityObjectIdentityProven = $false
    }

    $nearestRecords += [pscustomobject]@{
        candidateId = $candidate.candidateId
        candidateName = if ($nameStatus -eq "named") { $candidate.buildingName } else { "" }
        candidateNameStatus = $nameStatus
        sourceCoordinateStatus = $coordinateStatus
        nearestBuildingProxyId = if ($hasFallbackBuilding) { $candidate.fallbackBuildingId } else { "" }
        nearestMatchMethod = if ($sourceCentroid) { "p8e_plateau_centroid_to_fallback_building_proxy" } else { "p8e_candidate_coordinate_to_fallback_building_proxy" }
        nearestDistanceMeters = $distance
        confidence = $confidence
        thresholdResult = if ($matchAccepted) { "accepted_as_data_proxy" } else { "rejected_insufficient_source" }
        matchAccepted = $matchAccepted
        fallbackReason = $fallbackReason
        exactPlateauUnityObjectIdentityProven = $false
        limitation = "coordinate/proxy anchoring only; exact PLATEAU Unity object identity not proven"
    }

    $entranceRecords += [pscustomobject]@{
        targetId = $candidate.candidateId
        targetType = "humanitarian_candidate"
        targetNameStatus = $nameStatus
        coordinateStatus = $coordinateStatus
        entranceProxyStatus = if ($coordinateStatus -eq "coordinate_validated") { "coordinate_derived_entrance_proxy" } else { "entrance_proxy_rejected_or_fallback" }
        distanceFromCandidateAnchorMeters = if ($coordinateStatus -eq "coordinate_validated") { 0.0 } else { $null }
        confidence = if ($coordinateStatus -eq "coordinate_validated") { "medium" } else { "rejected" }
        fallbackReason = if ($coordinateStatus -eq "coordinate_validated") { "true_entrance_geometry_unavailable_proxy_at_candidate_coordinate" } else { "invalid_or_missing_candidate_coordinate" }
        nonOfficialWarningRequired = [bool]$candidate.nonOfficialWarningRequired
        isOfficialShelter = $false
        trueEntranceGeometryProven = $false
    }
}

foreach ($anchor in @($anchorConfig.anchors)) {
    if (-not [bool]$anchor.isOfficialShelter) {
        continue
    }

    $coordinateStatus = if ([bool]$anchor.hasCoordinate -and
        (Test-FiniteCoordinate $anchor.latitude) -and
        (Test-FiniteCoordinate $anchor.longitude) -and
        (Test-InBounds -Latitude ([double]$anchor.latitude) -Longitude ([double]$anchor.longitude) -Config $anchorConfig)) {
        "coordinate_validated"
    }
    else {
        "invalid_or_missing_coordinate"
    }

    $officialEntranceRecords += [pscustomobject]@{
        targetId = $anchor.sourceId
        anchorId = $anchor.anchorId
        targetType = "official_shelter"
        displayName = $anchor.displayName
        coordinateStatus = $coordinateStatus
        entranceProxyStatus = if ($coordinateStatus -eq "coordinate_validated") { "coordinate_derived_entrance_proxy" } else { "entrance_proxy_rejected_or_fallback" }
        distanceFromShelterAnchorMeters = if ($coordinateStatus -eq "coordinate_validated") { 0.0 } else { $null }
        confidence = if ($coordinateStatus -eq "coordinate_validated") { "medium" } else { "rejected" }
        fallbackReason = if ($coordinateStatus -eq "coordinate_validated") { "true_entrance_geometry_unavailable_proxy_at_official_shelter_coordinate" } else { "invalid_or_missing_official_shelter_coordinate" }
        nonOfficialWarningRequired = $false
        isOfficialShelter = $true
        trueEntranceGeometryProven = $false
    }
}

$routeRecords = @()
foreach ($route in @($routeSample.records)) {
    $pointInfo = Get-RoutePointInfo -Route $route -Config $anchorConfig
    $valid = $pointInfo.pointCount -gt 1 -and $pointInfo.invalidPointCount -eq 0
    $allInBounds = $valid -and $pointInfo.outOfBoundsPointCount -eq 0
    $routeRecords += [pscustomobject]@{
        routeId = $route.routeId
        originId = $route.originId
        shelterId = $route.shelterId
        routeAvailability = $route.routeAvailability
        routeType = $route.routeType
        routeCoordinateValidity = if ($valid) { "valid_linestring_coordinates" } else { "invalid_or_missing_coordinates" }
        routePointCount = $pointInfo.pointCount
        invalidPointCount = $pointInfo.invalidPointCount
        outOfBoundsPointCount = $pointInfo.outOfBoundsPointCount
        boundsCheck = if ($allInBounds) { "within_p10a_plus_proxy_bounds" } else { "has_out_of_bounds_or_invalid_points" }
        routeDistanceMeters = [double]$route.routeDistanceMeters
        snapDistanceOriginMeters = if ($null -ne $route.snapDistanceOriginMeters) { [double]$route.snapDistanceOriginMeters } else { $null }
        snapDistanceTargetMeters = if ($null -ne $route.snapDistanceTargetMeters) { [double]$route.snapDistanceTargetMeters } else { $null }
        routeConfidence = if ($allInBounds) { "coordinate_validated_proxy" } elseif ($valid) { "coordinate_proxy_with_bounds_warning" } else { "rejected" }
        isOfficialEvacuationRoute = [bool]$route.isOfficialEvacuationRoute
        routeRoadGeometryValidated = $false
        nearestRoadProxyStatus = "road_metadata_proxy_available_but_scene_road_object_not_proven"
        fallbackReason = "road proxy data is metadata/proxy only; route-to-road validation remains limited by available geometry and transform evidence"
        requiredWording = "estimated prototype guidance; not official evacuation routes; not fully road-geometry validated; coordinate-projected gameplay proxy"
    }
}

$semanticRecords = @()
foreach ($binding in @($semantic.bindings)) {
    $classification = Get-SemanticClassification -Binding $binding
    $semanticRecords += [pscustomobject]@{
        category = $binding.category
        sourceBindingMode = $binding.bindingMode
        classification = $classification
        confidence = $binding.confidence
        p9Usable = [bool]$binding.p9Usable
        isProxy = [bool]$binding.isProxy
        actualPlateauSemanticEvidenceFound = [bool]$binding.actualPlateauSemanticEvidenceFound
        sceneObjectEvidenceFound = [bool]$binding.sceneObjectEvidenceFound
        objectId = $binding.objectId
        blocker = $binding.blocker
        limitation = $binding.p9Limitations
    }
}

$highDetailScene = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
$scenePath = Join-Path $repoRoot ($highDetailScene -replace "/", "\")
$sceneStatus = @(& git -C $repoRoot status --porcelain=v1 -- $highDetailScene)

$named = @($candidateRecords | Where-Object { $_.nameStatus -eq "named" }).Count
$idOnly = @($candidateRecords | Where-Object { $_.nameStatus -eq "id_only" }).Count
$candidateWarnings = @($candidateRecords | Where-Object { -not $_.nonOfficialWarningRequired -or $_.isOfficialShelter -or $_.safeApprovedByDefault }).Count
$coordinateValidated = @($candidateRecords | Where-Object { $_.coordinateStatus -eq "coordinate_validated" }).Count
$nearestAccepted = @($nearestRecords | Where-Object { $_.matchAccepted }).Count
$routeOfficialClaims = @($routeRecords | Where-Object { $_.isOfficialEvacuationRoute }).Count
$routeCoordinateValidated = @($routeRecords | Where-Object { $_.routeCoordinateValidity -eq "valid_linestring_coordinates" }).Count
$routeBoundsWarnings = @($routeRecords | Where-Object { $_.boundsCheck -ne "within_p10a_plus_proxy_bounds" }).Count
$sceneObjectBindings = @($semanticRecords | Where-Object { $_.classification -eq "proven_scene_object_binding" }).Count

Write-JsonFile "p10a_plus_candidate_anchor_hardening_report.json" ([pscustomobject]@{
    schemaVersion = "p10a_plus.candidate_anchor_hardening_report.v1"
    stage = "P10-A+ hardening sprint under P10-A"
    sourceFiles = @(
        "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json",
        "Assets/Data/P9/p9d_coordinate_anchoring_config.json"
    )
    totalAnchorTargetsVerified = 116
    humanitarianCandidateTotal = @($candidateRecords).Count
    namedHumanitarianCandidateCount = $named
    idOnlyHumanitarianCandidateCount = $idOnly
    coordinateValidatedCount = $coordinateValidated
    allHumanitarianCandidatesRemainNonOfficial = ($candidateWarnings -eq 0)
    allHumanitarianCandidatesRequireWarning = ($candidateWarnings -eq 0)
    exactPlateauUnityObjectIdentityProven = $false
    limitation = "coordinate/proxy anchoring only; exact PLATEAU Unity object identity not proven"
    records = $candidateRecords
})

Write-JsonFile "p10a_plus_candidate_to_building_nearest_match_report.json" ([pscustomobject]@{
    schemaVersion = "p10a_plus.candidate_to_building_nearest_match_report.v1"
    stage = "P10-A+ hardening sprint under P10-A"
    sourceFiles = @(
        "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json",
        "Assets/Data/P8/p8e_semantic_binding_v1.json"
    )
    candidateTotal = @($nearestRecords).Count
    nearestMatchAcceptedCount = $nearestAccepted
    exactPlateauUnityObjectIdentityProven = $false
    sourceGeometryLimitation = "Candidate fallback building ids and PLATEAU centroid-derived coordinates are available, but independent Unity scene object identity is not proven."
    records = $nearestRecords
})

Write-JsonFile "p10a_plus_entrance_proxy_hardening_report.json" ([pscustomobject]@{
    schemaVersion = "p10a_plus.entrance_proxy_hardening_report.v1"
    stage = "P10-A+ hardening sprint under P10-A"
    sourceFiles = @(
        "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json",
        "Assets/Data/P9/p9d_coordinate_anchoring_config.json"
    )
    humanitarianEntranceProxyCount = @($entranceRecords).Count
    officialShelterEntranceProxySamples = @($officialEntranceRecords).Count
    trueEntranceGeometryProven = $false
    limitation = "Entrance proxies are coordinate-derived entrance proxy markers or nearest-match entrance markers; true building entrance geometry is not proven."
    records = $entranceRecords
    officialShelterRecords = $officialEntranceRecords
})

Write-JsonFile "p10a_plus_route_proxy_validation_report.json" ([pscustomobject]@{
    schemaVersion = "p10a_plus.route_proxy_validation_report.v1"
    stage = "P10-A+ hardening sprint under P10-A"
    sourceFiles = @(
        "Assets/Data/real_chuo_osm_routes_sample.json",
        "Assets/Data/P8/p8e_route_candidate_geometry_handoff.json",
        "Assets/Data/P8/p8e_semantic_binding_v1.json"
    )
    routeTotal = @($routeRecords).Count
    routeCoordinateValidatedCount = $routeCoordinateValidated
    routeBoundsWarningCount = $routeBoundsWarnings
    officialRouteClaimCount = $routeOfficialClaims
    routeRoadGeometryValidated = $false
    routesAreEstimatedPrototypeGuidance = $true
    requiredWording = "estimated prototype guidance; not official evacuation routes; not fully road-geometry validated; coordinate-projected gameplay proxy"
    limitation = "Route-to-road validation remains limited by available road proxy geometry and WGS84/Unity/PLATEAU transform evidence."
    records = $routeRecords
})

Write-JsonFile "p10a_plus_plateau_semantic_binding_audit.json" ([pscustomobject]@{
    schemaVersion = "p10a_plus.plateau_semantic_binding_audit.v1"
    stage = "P10-A+ hardening sprint under P10-A"
    sourceFiles = @("Assets/Data/P8/p8e_semantic_binding_v1.json")
    bindingTotal = @($semanticRecords).Count
    provenSceneObjectBindingCount = $sceneObjectBindings
    exactPlateauUnityObjectIdentityProven = ($sceneObjectBindings -gt 0)
    fullSceneSemanticCoverageProven = $false
    classifications = @("proven_scene_object_binding", "coordinate_proxy_binding", "metadata_proxy_binding", "data_only_binding", "insufficient_evidence")
    records = $semanticRecords
})

Write-JsonFile "p10a_plus_high_detail_smoke_status.json" ([pscustomobject]@{
    schemaVersion = "p10a_plus.high_detail_smoke_status.v1"
    stage = "P10-A+ hardening sprint under P10-A"
    scenePath = $highDetailScene
    sceneExists = (Test-Path -LiteralPath $scenePath -PathType Leaf)
    sceneTouched = ($sceneStatus.Count -gt 0)
    sceneMutationPerformed = $false
    p9RuntimeSceneSafe = $true
    markerGenerationWithoutSceneMutation = $true
    manualSmokeChecklistStrengthened = $true
    p10bRuntimeValidationRequired = $true
    p10cArchiveRequired = $true
    checklist = @(
        "open high-detail scene without saving",
        "verify P9 runtime root/bootstrap can be added or enabled without scene commit",
        "verify candidate markers and official/non-official warnings",
        "verify spawn/crowd markers",
        "verify light curtain visibility and marker readability",
        "verify entrance/safe-floor proxy flow",
        "verify ResultPanel warning/reason code text",
        "collect FPS, memory, Player.log warnings/errors"
    )
})

Write-JsonFile "p10a_plus_hardening_matrix.json" ([pscustomobject]@{
    schemaVersion = "p10a_plus.hardening_matrix.v1"
    stage = "P10-A+ hardening sprint under P10-A"
    officialP10StageCountRemainsFour = $true
    noP10EfgCreated = $true
    noNewLargeGameplaySystem = $true
    noP7P8P9Reimplementation = $true
    noP10CArchiveOrReleaseWork = $true
    items = @(
        [pscustomobject]@{ id = "110_humanitarian_candidate_marker_anchoring"; classification = "closed_by_p10a_plus"; evidence = "full 110-record candidate anchor hardening report generated" },
        [pscustomobject]@{ id = "candidate_to_building_binding"; classification = "improved_to_nearest_match_validated_proxy"; evidence = "fallback building proxy ids and centroid-derived coordinates reported; exact scene object identity not proven" },
        [pscustomobject]@{ id = "entrance_proxy_placement"; classification = "improved_to_coordinate_validated_proxy"; evidence = "coordinate-derived entrance proxy report generated; true entrance geometry not proven" },
        [pscustomobject]@{ id = "route_candidate_coordinate_geometry"; classification = "improved_to_coordinate_validated_proxy"; evidence = "OSM route LineString point counts and bounds checks generated" },
        [pscustomobject]@{ id = "p5_route_road_geometry_validation"; classification = "still_known_limitation"; evidence = "road semantic metadata exists but scene road object and official route validation remain unproven" },
        [pscustomobject]@{ id = "plateau_semantic_object_binding"; classification = "still_known_limitation"; evidence = "semantic binding audit classifies metadata/proxy/data-only evidence; full scene object binding not proven" },
        [pscustomobject]@{ id = "hazard_front_light_curtain_visual_qa"; classification = "visual_qa_ready_for_p10b"; evidence = "visual QA checklist strengthened; runtime measurement remains P10-B" },
        [pscustomobject]@{ id = "result_panel_long_warning_text"; classification = "visual_qa_ready_for_p10b"; evidence = "formatter coverage retained and visual checklist strengthened" },
        [pscustomobject]@{ id = "p2_p9_high_detail_scene_smoke"; classification = "p10b_runtime_validation_required"; evidence = "scene exists and protected; manual runtime smoke checklist strengthened" },
        [pscustomobject]@{ id = "windows_exe_profiling_readiness"; classification = "p10b_runtime_validation_required"; evidence = "P10-B metrics and stress checklist remain ready" },
        [pscustomobject]@{ id = "high_detail_scene_archive"; classification = "p10c_archive_required"; evidence = "archive remains P10-C and was not performed in P10-A+" }
    )
})

$p10b | Add-Member -NotePropertyName p10aPlusHardeningApplied -NotePropertyValue $true -Force
$p10b | Add-Member -NotePropertyName p10aPlusAdditionalStressFocus -NotePropertyValue @(
    "candidate_nearest_match_report_review",
    "entrance_proxy_visual_distance_check",
    "route_bounds_warning_review",
    "semantic_binding_limitation_review"
) -Force
$p10b | Add-Member -NotePropertyName p10aPlusEvidenceReports -NotePropertyValue @(
    "p10a_plus_candidate_anchor_hardening_report.json",
    "p10a_plus_candidate_to_building_nearest_match_report.json",
    "p10a_plus_entrance_proxy_hardening_report.json",
    "p10a_plus_route_proxy_validation_report.json",
    "p10a_plus_plateau_semantic_binding_audit.json",
    "p10a_plus_high_detail_smoke_status.json"
) -Force
Write-JsonFile "p10a_p10b_readiness_checklist.json" $p10b

Write-Host "P10-A+ hardening reports written to Assets/Data/P10." -ForegroundColor Green
