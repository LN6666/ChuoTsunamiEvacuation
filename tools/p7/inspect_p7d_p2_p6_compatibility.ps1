[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$sceneRelativePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
$scenePath = Join-Path $repoRoot ($sceneRelativePath -replace "/", "\")

$sceneText = if (Test-Path -LiteralPath $scenePath -PathType Leaf) { Get-Content -Raw -LiteralPath $scenePath } else { "" }
$failed = $false

$checks = @(
    @{ Phase = "P2"; Area = "player movement"; Evidence = "Assets/Scripts/Player/SimplePlayerController.cs"; Marker = "PlayerMovement_runtime_smoke_pending" },
    @{ Phase = "P2"; Area = "camera"; Evidence = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"; Marker = "P7HighDetail_OverviewCamera" },
    @{ Phase = "P2"; Area = "result panel"; Evidence = "Assets/Scripts/Result/ResultPanelController.cs"; Marker = "ResultPanel_success_failure_rules_unchanged" },
    @{ Phase = "P4"; Area = "shelter interaction"; Evidence = "Assets/Scripts/Shelter/ShelterEntranceTrigger.cs"; Marker = "ShelterInteraction_placeholder_targets_required" },
    @{ Phase = "P5"; Area = "static data loaders"; Evidence = "Assets/Scripts/Data/P5CStaticDataLoader.cs"; Marker = "P5DataLoaders_scene_independent_check_required" },
    @{ Phase = "P6"; Area = "navigation guidance"; Evidence = "Assets/Scripts/Navigation/NavigationGuidanceDisplay.cs"; Marker = "P6NavigationGuidance_target_objects_required" },
    @{ Phase = "P6"; Area = "NPC prototype"; Evidence = "Assets/Scripts/NPC/NpcEvacuationAgent.cs"; Marker = "P6NpcPrototype_test_staging_required" }
)

Write-Host "P7-D P2-P6 compatibility inspection"
Write-Host ("{0,-6} {1,-26} {2,-12} {3}" -f "phase", "area", "status", "evidence")

foreach ($check in $checks) {
    $evidencePath = Join-Path $repoRoot ($check.Evidence -replace "/", "\")
    $hasEvidence = Test-Path -LiteralPath $evidencePath -PathType Leaf
    $hasMarker = $sceneText.IndexOf($check.Marker, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    $status = "prepared_pending_runtime_smoke"

    if (-not $hasEvidence) {
        $status = "missing_source_evidence"
        $failed = $true
    }
    elseif (-not $hasMarker) {
        $status = "missing_scene_marker"
        $failed = $true
    }

    Write-Host ("{0,-6} {1,-26} {2,-12} {3}" -f $check.Phase, $check.Area, $status, $check.Evidence)
}

foreach ($fragment in @("TriggerFailure", "CompleteFailure", "CompleteSuccess", "MovingTsunamiWall", "LightCurtain", "RiskFront", "CrowdFailure", "RealSpawn")) {
    if ($sceneText.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Write-Host "FAIL: high-detail scene contains forbidden gameplay/P8/P9 fragment: $fragment"
        $failed = $true
    }
}

Write-Host ""
Write-Host "P2-P6 runtime compatibility remains BLOCKED until manual PLATEAU import populates the high-detail scene."

if ($failed) {
    Write-Host "P7-D P2-P6 compatibility inspection: FAIL" -ForegroundColor Red
    exit 1
}

Write-Host "P7-D P2-P6 compatibility inspection: PASS_WITH_RUNTIME_IMPORT_BLOCKER_STATUS" -ForegroundColor Green
exit 0
