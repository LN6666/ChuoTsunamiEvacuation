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

function Assert-AllowedClassification {
    param([string]$Value, [string]$Context)
    $allowed = @(
        "closed_by_p10a",
        "gameplay_usable_proxy",
        "p10b_performance_qa",
        "p10c_release_archive",
        "p10d_final_review",
        "known_limitation",
        "future_work"
    )
    if ($allowed -notcontains $Value) {
        throw "$Context has invalid classification '$Value'."
    }
}

function Test-GapClosureMatrix {
    $matrix = Read-JsonFile "Assets/Data/P10/p10a_gap_closure_matrix.json"
    Assert-BooleanTrue $matrix.noP10EfgCreated "noP10EfgCreated"
    Assert-BooleanFalse $matrix.newGameplaySystemAdded "newGameplaySystemAdded"
    Assert-BooleanFalse $matrix.p8P9Reimplemented "p8P9Reimplemented"

    $records = @($matrix.items)
    if ($records.Count -lt 14) {
        throw "P10-A gap closure matrix must contain the expected gap review items."
    }

    $seen = @{}
    foreach ($record in $records) {
        Assert-RequiredText $record.id "gap item id"
        if ($seen.ContainsKey($record.id)) {
            throw "Duplicate P10-A gap item id '$($record.id)'."
        }
        $seen[$record.id] = $true
        Assert-AllowedClassification $record.classification "gap item $($record.id)"
        Assert-RequiredText $record.status "gap item $($record.id) status"
    }

    foreach ($required in @(
        "coordinate_anchoring_not_gis_grade",
        "exact_plateau_object_identity_not_claimed",
        "p5_route_road_geometry_validation",
        "humanitarian_candidate_markers",
        "high_detail_scene_smoke",
        "windows_exe_profiling",
        "release_package_archive"
    )) {
        if (-not $seen.ContainsKey($required)) {
            throw "Missing P10-A gap matrix item: $required"
        }
    }
}

function Test-HighDetailSceneQaStatus {
    $status = Read-JsonFile "Assets/Data/P10/p10a_high_detail_scene_qa_status.json"
    Assert-RequiredText $status.scenePath "high-detail scene path"
    Assert-BooleanTrue $status.sceneExistsExpected "sceneExistsExpected"
    Assert-BooleanFalse $status.sceneMutationRequired "sceneMutationRequired"
    Assert-BooleanFalse $status.sceneMutationPerformed "sceneMutationPerformed"
    Assert-BooleanTrue $status.protectedSceneMustNotBeCommittedDirty "protectedSceneMustNotBeCommittedDirty"
    Assert-BooleanTrue $status.sceneSafeRuntimeQaPrepared "sceneSafeRuntimeQaPrepared"
    Assert-BooleanTrue $status.manualQaChecklistRequired "manualQaChecklistRequired"
    Assert-BooleanTrue $status.manualQaDeferredToP10BOrP10D "manualQaDeferredToP10BOrP10D"
    Assert-BooleanTrue $status.p10bReady "high-detail p10bReady"

    $scenePath = Join-Path $repoRoot ($status.scenePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $scenePath -PathType Leaf)) {
        throw "Expected high-detail scene does not exist locally: $($status.scenePath)"
    }
}

function Test-AnchorFinalQaStatus {
    $status = Read-JsonFile "Assets/Data/P10/p10a_anchor_final_qa_status.json"
    if ([int]$status.totalAnchorTargets -ne 116) {
        throw "P10-A anchor QA must preserve 116 total anchor targets."
    }
    if ([int]$status.humanitarianCandidateTotal -ne 110) {
        throw "P10-A anchor QA must preserve 110 humanitarian candidates."
    }
    if ([int]$status.namedHumanitarianCandidateCount -ne 28 -or [int]$status.idOnlyHumanitarianCandidateCount -ne 82) {
        throw "P10-A anchor QA must preserve 28 named and 82 ID-only humanitarian candidates."
    }
    Assert-BooleanTrue $status.allHumanitarianCandidatesRemainNonOfficial "allHumanitarianCandidatesRemainNonOfficial"
    Assert-BooleanTrue $status.allHumanitarianCandidatesRequireWarning "allHumanitarianCandidatesRequireWarning"
    Assert-BooleanFalse $status.safeApprovedByDefault "safeApprovedByDefault"
    Assert-BooleanFalse $status.routeOfficialClaimed "routeOfficialClaimed"
    Assert-BooleanFalse $status.gisGradeProofClaimed "gisGradeProofClaimed"
    Assert-BooleanFalse $status.exactPlateauObjectIdentityClaimed "exactPlateauObjectIdentityClaimed"
    if ($status.coordinatePolicy -notlike "*proxy*" -or $status.coordinatePolicy -notlike "*nearest*") {
        throw "P10-A coordinatePolicy must preserve proxy/nearest-match wording."
    }
    if ($status.routeWording -notlike "*not official evacuation route*" -or $status.routeWording -notlike "*not fully road-geometry validated*") {
        throw "P10-A route wording must preserve non-official and not-validated route limitations."
    }
}

function Test-P10BReadinessChecklist {
    $checklist = Read-JsonFile "Assets/Data/P10/p10a_p10b_readiness_checklist.json"
    Assert-BooleanFalse $checklist.p10bBuildStarted "p10bBuildStarted"
    Assert-BooleanFalse $checklist.p10cReleasePackagingStarted "p10cReleasePackagingStarted"
    Assert-BooleanFalse $checklist.p10cArchiveCompleted "p10cArchiveCompleted"
    Assert-BooleanTrue $checklist.noLargeBuildArtifactsCommitted "noLargeBuildArtifactsCommitted"
    Assert-BooleanTrue $checklist.beforeAfterMetricsRequiredForOptimization "beforeAfterMetricsRequiredForOptimization"
    Assert-BooleanTrue $checklist.p10bReady "p10bReady"
    Assert-ArrayContains @($checklist.qualityPresets) "Low" "qualityPresets"
    Assert-ArrayContains @($checklist.qualityPresets) "Medium" "qualityPresets"
    Assert-ArrayContains @($checklist.qualityPresets) "High" "qualityPresets"

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
        Assert-ArrayContains @($checklist.metricsToCollect) $metric "metricsToCollect"
    }

    foreach ($candidate in @(
        "marker_density_cap",
        "NPC_cap",
        "debug_visual_toggles",
        "log_throttling",
        "object_pooling_opportunities",
        "Update_loop_hot_spots",
        "UI_text_layout_risks",
        "light_curtain_visual_cost"
    )) {
        Assert-ArrayContains @($checklist.optimizationCandidateChecklist) $candidate "optimizationCandidateChecklist"
    }
}

Write-Host "P10-A JSON validation: starting"
Test-GapClosureMatrix
Test-HighDetailSceneQaStatus
Test-AnchorFinalQaStatus
Test-P10BReadinessChecklist
Write-Host "P10-A JSON validation: PASS" -ForegroundColor Green
exit 0
