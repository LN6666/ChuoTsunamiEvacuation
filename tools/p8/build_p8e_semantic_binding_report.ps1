[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$outputPath = Join-Path $repoRoot "Assets\Data\P8\p8e_semantic_binding_v1.json"

function Test-RepoPath {
    param([string]$RelativePath)
    return Test-Path -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\")) -PathType Leaf
}

function New-Binding {
    param(
        [string]$Category,
        [string]$BindingMode,
        [string[]]$SourceEvidence,
        [string]$Confidence,
        [bool]$IsProxy,
        [bool]$P9Usable,
        [string]$P9Limitations,
        [string]$Notes,
        [bool]$PlateauEvidence,
        [bool]$SceneEvidence,
        [string]$ObjectId = "",
        [string]$Blocker = ""
    )

    [ordered]@{
        category = $Category
        bindingMode = $BindingMode
        sourceEvidence = @($SourceEvidence)
        confidence = $Confidence
        isProxy = $IsProxy
        p9Usable = $P9Usable
        p9Limitations = $P9Limitations
        notes = $Notes
        actualPlateauSemanticEvidenceFound = $PlateauEvidence
        sceneObjectEvidenceFound = $SceneEvidence
        objectId = $ObjectId
        blocker = $Blocker
    }
}

$roadGml = "Assets/P7Benchmark/Imported/53393690/udx/tran/53393690_tran_6697_op.gml"
$buildingGml = "Assets/P7Benchmark/Imported/53393690/udx/bldg/53393690_bldg_6697_op.gml"
$bridgeGml = "Assets/P7Benchmark/Imported/53393690/udx/brid/53393690_brid_6697_op.gml"
$waterfrontGml = "Assets/P7Benchmark/Imported/53393690/udx/fld/pref/53393690_fld_pref_6697_op.gml"
$candidateAudit = "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json"
$p8cConfig = "Assets/Data/P8/infrastructure_hazard_interaction_config.json"
$p8dConfig = "Assets/Data/P8/infrastructure_damage_proxy_config.json"

$bindings = @(
    New-Binding "road" "plateau_metadata" @($roadGml, $p8cConfig) "medium" $true $true `
        "Usable as road-category hazard/status metadata only; individual Unity road scene objects are not proven bound." `
        "PLATEAU transport metadata is present locally, but this hardening pass does not mutate scene objects or claim road-geometry-validated routing." `
        (Test-RepoPath $roadGml) $false "53393690_tran_6697_op"

    New-Binding "building" "plateau_metadata" @($buildingGml, $candidateAudit, $p8dConfig) "medium" $true $true `
        "Usable for building/hazard proxy status and candidate marker readiness; not an official safety or damage assessment." `
        "PLATEAU building metadata and P8 candidate audit are present, but direct high-detail scene object IDs are not proven." `
        (Test-RepoPath $buildingGml) $false "53393690_bldg_6697_op"

    New-Binding "bridge" "plateau_metadata" @($bridgeGml, $p8cConfig, $p8dConfig) "medium" $true $true `
        "Usable for bridge restricted-proxy status; not validated as navigable route geometry." `
        "PLATEAU bridge metadata is present. P8 keeps bridge effects as visual/status proxy only." `
        (Test-RepoPath $bridgeGml) $false "53393690_brid_6697_op"

    New-Binding "underground" "data_only" @($p8cConfig, $p8dConfig) "low" $true $true `
        "Usable only as category-level underground avoid proxy. P9 must not assume real subway entrance geometry." `
        "No explicit local underground PLATEAU/scene semantic object binding was found in the inspected P8/P7 high-detail evidence." `
        $false $false "" "missing_explicit_underground_semantic_metadata"

    New-Binding "entrance" "proxy_marker" @($p8cConfig, $p8dConfig, $candidateAudit) "low" $true $true `
        "Usable as entrance blocked proxy and future entrance/safe-floor gameplay placeholder. P9 must not assume real entrance placement." `
        "No reliable per-building entrance geometry was found; P8-E preserves a proxy marker boundary." `
        $false $false "" "missing_building_entrance_geometry"

    New-Binding "waterfront" "plateau_metadata" @($waterfrontGml, $p8cConfig) "low" $true $true `
        "Usable for waterfront hazard/status proxy. P9 must verify scene placement before gameplay collision or route blocking." `
        "Local flood/waterfront-adjacent PLATEAU evidence exists in P7Benchmark, but it is not treated as tsunami data and not a full scene binding." `
        (Test-RepoPath $waterfrontGml) $false "53393690_fld_pref_6697_op"

    New-Binding "shelter_proxy" "proxy_marker" @("Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json", $p8cConfig) "medium" $true $true `
        "Usable for P8 shelter-marker compatibility only. P9 must keep official shelters separate from non-official candidates." `
        "P8 keeps shelter interaction at smoke/proxy level and does not change success/failure rules." `
        $false $false "p8_shelter_proxy"

    New-Binding "navigation_target_proxy" "proxy_marker" @($p8cConfig) "medium" $true $true `
        "Usable for navigation guidance proxy. P9 must implement final route/crowd gameplay separately." `
        "P8 does not claim final navigability or official route approval." `
        $false $false "p8_navigation_target_proxy"

    New-Binding "humanitarian_candidate_proxy" "data_only" @($candidateAudit, "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json") "high" $true $true `
        "Usable for non-official hazard status and future marker generation. P9 must not present candidates as official shelters." `
        "The expanded 110-candidate audit is accepted by the user, remains non-official, and requires warnings." `
        $true $false "p8_humanitarian_candidate_audit_v1"

    New-Binding "highrise_candidate_marker" "proxy_marker" @($candidateAudit, "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json") "high" $true $true `
        "Usable for persistent marker readiness with explicit non-official labels. P9 decides any selectable life-first behavior." `
        "Marker readiness is coordinate/data based; final scene anchoring remains a P9/P8-E follow-up boundary unless safely added later." `
        $true $false "p8_highrise_candidate_marker_v1"
)

$report = [ordered]@{
    datasetId = "p8e_semantic_binding_v1"
    generatedAt = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssK")
    stage = "P8-E hardening"
    completeSceneSemanticBinding = $false
    sceneMutationPerformed = $false
    baselineScene = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
    legacyFallbackScene = "Assets/Scenes/Chuo_BaseMap.unity"
    p9AssumptionBoundary = "P9 may use these bindings as proxy/metadata foundations, but must not assume full scene semantic object binding, official routes, official shelter status, or engineering damage predictions."
    bindings = $bindings
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $outputPath -Encoding UTF8
Write-Host "Wrote $outputPath"
Write-Host "P8-E semantic binding categories: $($bindings.Count)"
