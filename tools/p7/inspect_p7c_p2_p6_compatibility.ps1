[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$sceneRelativePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
$scenePath = Join-Path $repoRoot ($sceneRelativePath -replace "/", "\")

$checks = @(
    @{ Area = "player movement"; Evidence = "Assets/Scripts/Player/SimplePlayerController.cs"; Marker = "PlayerMovement_runtime_smoke_pending"; Notes = "Controller can be staged in the new scene after spawn placement." },
    @{ Area = "camera"; Evidence = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"; Marker = "P7HighDetail_OverviewCamera"; Notes = "Scene shell includes a non-gameplay overview camera." },
    @{ Area = "GameManager"; Evidence = "Assets/Scripts/Core/EvacuationGameManager.cs"; Marker = "GameManager_old_scene_hard_binding_to_verify"; Notes = "Source exists; runtime smoke must verify no old-scene hard binding." },
    @{ Area = "result panel"; Evidence = "Assets/Scripts/Result/ResultPanelController.cs"; Marker = "ResultPanel_success_failure_rules_unchanged"; Notes = "P7-C must not change success/failure flow." },
    @{ Area = "shelter interaction"; Evidence = "Assets/Scripts/Shelter/ShelterEntranceTrigger.cs"; Marker = "ShelterInteraction_placeholder_targets_required"; Notes = "New scene needs representative shelter targets before runtime validation." },
    @{ Area = "P5 data loaders"; Evidence = "Assets/Scripts/Data/P5CStaticDataLoader.cs"; Marker = "P5DataLoaders_scene_independent_check_required"; Notes = "Data loaders should run without requiring Chuo_BaseMap." },
    @{ Area = "P6 navigation guidance"; Evidence = "Assets/Scripts/Navigation/NavigationGuidanceDisplay.cs"; Marker = "P6NavigationGuidance_target_objects_required"; Notes = "Guidance needs target objects in the new high-detail scene." },
    @{ Area = "P6 NPC prototype"; Evidence = "Assets/Scripts/NPC/NpcEvacuationAgent.cs"; Marker = "P6NpcPrototype_test_staging_required"; Notes = "NPC prototype can be staged later without P9 crowd systems." }
)

Write-Host "P7-C P2-P6 compatibility inspection"
Write-Host "Scene: $sceneRelativePath"

$sceneText = ""
if (Test-Path -LiteralPath $scenePath -PathType Leaf) {
    $sceneText = Get-Content -Raw -LiteralPath $scenePath
}

$failed = $false

Write-Host ""
Write-Host ("{0,-28} {1,-12} {2,-32} {3}" -f "area", "status", "evidence", "notes")
foreach ($check in $checks) {
    $evidencePath = Join-Path $repoRoot ($check.Evidence -replace "/", "\")
    $hasSourceEvidence = Test-Path -LiteralPath $evidencePath -PathType Leaf
    $hasSceneMarker = -not [string]::IsNullOrEmpty($sceneText) -and $sceneText.IndexOf($check.Marker, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    $status = "prepared_pending_runtime_smoke"

    if (-not $hasSourceEvidence) {
        $status = "missing_source_evidence"
        $failed = $true
    }
    elseif (-not $hasSceneMarker) {
        $status = "missing_scene_marker"
        $failed = $true
    }

    Write-Host ("{0,-28} {1,-12} {2,-32} {3}" -f $check.Area, $status, $check.Evidence, $check.Notes)
}

$requiredDocs = @(
    "docs/P7C_P2_P6_COMPATIBILITY_PLAN.md",
    "docs/P7C_NEW_MAP_GAMEPLAY_SMOKE_TEST.md",
    "docs/P7C_P8_P9_BASELINE_PREPARATION.md"
)

foreach ($requiredDoc in $requiredDocs) {
    $docPath = Join-Path $repoRoot ($requiredDoc -replace "/", "\")
    if (-not (Test-Path -LiteralPath $docPath -PathType Leaf)) {
        Write-Host "FAIL: missing compatibility documentation: $requiredDoc"
        $failed = $true
    }
}

if (-not [string]::IsNullOrEmpty($sceneText)) {
    $forbiddenFragments = @(
        "TriggerFailure",
        "CompleteFailure",
        "CompleteSuccess",
        "LightCurtain",
        "MovingTsunamiWall",
        "CrowdFailure",
        "RealSpawn"
    )

    foreach ($fragment in $forbiddenFragments) {
        if ($sceneText.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Write-Host "FAIL: high-detail scene contains forbidden gameplay or P8/P9 fragment: $fragment"
            $failed = $true
        }
    }
}

Write-Host ""
Write-Host "Runtime validation status: prepared_pending_runtime_smoke. P7-C documents the checks; P7-D must run them against the populated high-detail scene before final baseline confirmation."

if ($failed) {
    Write-Host "P7-C P2-P6 compatibility inspection: FAIL" -ForegroundColor Red
    exit 1
}

Write-Host "P7-C P2-P6 compatibility inspection: PASS_WITH_PENDING_RUNTIME_SMOKE" -ForegroundColor Green
exit 0
