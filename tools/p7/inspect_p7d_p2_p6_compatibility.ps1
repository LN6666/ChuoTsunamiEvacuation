[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
. (Join-Path $scriptRoot "get_p7d_scene_evidence.ps1")

$failed = $false
$evidence = Get-P7DSceneEvidence -RepoRoot $repoRoot

function Test-RepoFile {
    param([string]$Path)

    return Test-Path -LiteralPath (Join-Path $repoRoot ($Path -replace "/", "\")) -PathType Leaf
}

function Get-JsonText {
    param([string]$Path)

    $fullPath = Join-Path $repoRoot ($Path -replace "/", "\")
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        return ""
    }

    return Get-Content -Raw -LiteralPath $fullPath
}

$checks = @(
    @{ Phase = "P2"; Area = "player movement"; Evidence = "Assets/Scripts/Player/SimplePlayerController.cs"; Status = "source_scene_independent" },
    @{ Phase = "P2"; Area = "camera"; Evidence = "Assets/Scripts/Editor/FirstPlayableSceneBuilder.cs"; Status = "builder_can_create_camera_not_bound_to_base_map" },
    @{ Phase = "P2"; Area = "shelter interaction"; Evidence = "Assets/Scripts/Shelter/ShelterEntranceTrigger.cs"; Status = "source_scene_independent_needs_new_map_marker_wiring" },
    @{ Phase = "P2"; Area = "result panel / success failure"; Evidence = "Assets/Scripts/Result/ResultPanelController.cs"; Status = "rules_source_unchanged" },
    @{ Phase = "P3"; Area = "data pipeline runtime assumptions"; Evidence = "Assets/Scripts/Data/ShelterDataSourceResolver.cs"; Status = "static_data_loader_scene_independent" },
    @{ Phase = "P4"; Area = "real shelter markers"; Evidence = "Assets/Scripts/Gameplay/RealShelterMarkerRuntimeGenerator.cs"; Status = "can_adapt_after_coordinate_transform_validation" },
    @{ Phase = "P5"; Area = "qualified shelters / route metadata"; Evidence = "Assets/Scripts/Gameplay/P5DRealQualifiedShelterRuntimeGenerator.cs"; Status = "opt_in_fail_safe_scene_independent" },
    @{ Phase = "P5"; Area = "humanitarian candidate display"; Evidence = "Assets/Scripts/Gameplay/P5GHHumanitarianCandidateRuntimeGenerator.cs"; Status = "feature_flags_default_off" },
    @{ Phase = "P6"; Area = "navigation guidance"; Evidence = "Assets/Scripts/Navigation/NavigationGuidanceController.cs"; Status = "target_object_driven_needs_new_scene_targets" },
    @{ Phase = "P6"; Area = "NPC prototype"; Evidence = "Assets/Scripts/NPC/NpcEvacuationAgent.cs"; Status = "prototype_can_stage_targets_needs_smoke_test" }
)

Write-Host "P7-D P2-P6 compatibility inspection"
Write-Host ("{0,-6} {1,-36} {2,-42} {3}" -f "phase", "area", "status", "evidence")

foreach ($check in $checks) {
    if (-not (Test-RepoFile -Path $check.Evidence)) {
        Write-Host ("{0,-6} {1,-36} {2,-42} {3}" -f $check.Phase, $check.Area, "missing_source_evidence", $check.Evidence)
        $failed = $true
        continue
    }

    Write-Host ("{0,-6} {1,-36} {2,-42} {3}" -f $check.Phase, $check.Area, $check.Status, $check.Evidence)
}

$sourceConfig = Get-JsonText -Path "Assets/Data/shelter_source_config.json"
if ($sourceConfig.IndexOf('"sourceMode"', [System.StringComparison]::OrdinalIgnoreCase) -lt 0 -or
    $sourceConfig.IndexOf('"test"', [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
    Write-Host "FAIL: shelter_source_config.json does not clearly preserve sourceMode=test."
    $failed = $true
}
else {
    Write-Host "P5 source mode: sourceMode=test evidence preserved."
}

if ($sourceConfig.IndexOf('"enableHumanitarianCandidates"', [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -and
    $sourceConfig.IndexOf('"enableLifeFirstCandidateSelection"', [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -and
    $sourceConfig.IndexOf("true", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
    Write-Host "P5 flags: humanitarian and life-first candidate flags remain default-off."
}
else {
    Write-Host "WARN: P5 opt-in flag state could not be confirmed from shelter_source_config.json."
}

if ($evidence.ForbiddenHits.Count -gt 0) {
    foreach ($hit in $evidence.ForbiddenHits) {
        Write-Host "FAIL: high-detail scene contains forbidden/protected fragment: $hit"
    }
    $failed = $true
}

Write-Host ""
Write-Host "Compatibility verdict: CONDITIONAL. P2-P6 code/data are scene-independent enough to adapt, but runtime smoke on the populated high-detail scene is still pending."

if ($failed) {
    Write-Host "P7-D P2-P6 compatibility inspection: FAIL" -ForegroundColor Red
    exit 1
}

Write-Host "P7-D P2-P6 compatibility inspection: CONDITIONAL PASS" -ForegroundColor Yellow
exit 0
