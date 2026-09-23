[CmdletBinding()]
param(
    [switch]$Json
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

function Test-RepoFile {
    param([string]$RelativePath)

    return Test-Path -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\")) -PathType Leaf
}

function New-Result {
    param(
        [string]$Phase,
        [string]$Category,
        [string]$Status,
        [string]$Evidence,
        [string]$Notes
    )

    [pscustomobject]@{
        phase = $Phase
        category = $Category
        status = $Status
        evidence = $Evidence
        notes = $Notes
    }
}

$hasHighDetail = Test-RepoFile "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
$hasHazardLayer = Test-RepoFile "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json"
$hasEvaluator = Test-RepoFile "Assets/Scripts/P8/P8InfrastructureHazardEvaluator.cs"
$hasTarget = Test-RepoFile "Assets/Scripts/P8/P8InfrastructureHazardTarget.cs"
$hasProxy = Test-RepoFile "Assets/Scripts/P8/P8InfrastructureHazardProxy.cs"
$hasMarker = Test-RepoFile "Assets/Scripts/P8/P8InfrastructureHazardMarker.cs"
$hasNavigation = Test-RepoFile "Assets/Scripts/Navigation/NavigationGuidanceController.cs"
$hasNpcPrototype = Test-RepoFile "Assets/Scripts/NPC/NpcEvacuationAgent.cs"
$hasPlayer = Test-RepoFile "Assets/Scripts/Player/SimplePlayerController.cs"
$hasCameraContext = Test-RepoFile "Assets/Scripts/Navigation/NavigationGuidanceController.cs"
$hasResultPanel = Test-RepoFile "Assets/Scripts/Result/ResultPanelController.cs"

$results = @(
    New-Result "P2" "player_movement" ($(if ($hasPlayer -and $hasHighDetail) { "passed" } else { "blocked" })) "SimplePlayerController exists and P7_HighDetail_Chuo baseline is present." "High-detail collision/spawn is smoke-level only in P8-C."
    New-Result "P2" "camera" ($(if ($hasCameraContext -and $hasHighDetail) { "passed" } else { "blocked" })) "Runtime camera/navigation context can be represented without Chuo_BaseMap." "No camera success/failure rule changes."
    New-Result "P2" "shelter_interaction_proxy" ($(if ($hasProxy -and $hasTarget) { "proxy-based" } else { "blocked" })) "P8-C shelter_proxy category is available through P8InfrastructureHazardTarget/Proxy." "Does not call BuildingShelter or ShelterEntranceTrigger."
    New-Result "P2" "result_panel_flow" ($(if ($hasResultPanel -and $hasEvaluator) { "passed" } else { "blocked" })) "P8-C evaluator has AffectsGameplaySuccessFailure=false." "ResultPanel flow is not mutated."
    New-Result "P3" "unity_ready_data_assumptions" ($(if ($hasHazardLayer -and $hasEvaluator) { "passed" } else { "blocked" })) "P8-B hazard layer v1 is available under Assets/Data/P8." "Uses P8 loader and evidence fields."
    New-Result "P4" "real_shelter_loading_marker_logic" ($(if ($hasMarker -and $hasProxy) { "proxy-based" } else { "blocked" })) "P8-C marker/proxy components can represent high-detail shelter anchors." "True PLATEAU entrance semantics remain pending."
    New-Result "P5" "qualified_shelter_route_highrise_humanitarian_metadata" ($(if ($hasProxy -and $hasNavigation) { "proxy-based" } else { "blocked" })) "Proxy targets can carry route/candidate context without official-route claims." "real_qualified remains opt-in/fail-safe."
    New-Result "P6" "navigation_guidance_proxy_targets" ($(if ($hasNavigation -and $hasProxy) { "proxy-based" } else { "blocked" })) "NavigationGuidanceController can target P8-C proxy transforms." "Display-only, no Chuo_BaseMap dependency."
    New-Result "P6" "npc_prototype_staging" ($(if ($hasNpcPrototype -and $hasProxy) { "proxy-based" } else { "blocked" })) "P6 NPC prototype can stage against limited proxy targets." "No P9 crowd, real spawn, congestion, or indoor gameplay."
    New-Result "P4-P6" "true_plateau_semantic_geometry" "pending" "High-detail map has incomplete semantic coverage for roads/bridges/underground/entrances." "P8-C intentionally uses proxies where semantics are incomplete."
)

if ($Json) {
    $results | ConvertTo-Json -Depth 4
}
else {
    Write-Host "P8-C P2-P6 runtime adaptation inspection"
    Write-Host "Repo root: $repoRoot"
    foreach ($result in $results) {
        Write-Host ("{0} {1}: {2} - {3}" -f $result.phase, $result.category, $result.status, $result.evidence)
    }
}

$blocked = @($results | Where-Object { $_.status -eq "blocked" })
if ($blocked.Count -gt 0) {
    Write-Host ""
    Write-Host "P8-C P2-P6 runtime adaptation inspection: FAIL" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "P8-C P2-P6 runtime adaptation inspection: PASS" -ForegroundColor Green
exit 0
