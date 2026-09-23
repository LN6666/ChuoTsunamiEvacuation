[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$baselineScene = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"

function Normalize-RepoPath {
    param([string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path)) { return "" }
    $normalized = $Path -replace "\\", "/"
    while ($normalized.StartsWith("./", [System.StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(2)
    }
    return $normalized
}

function Get-GitLines {
    param([string[]]$GitArgs)
    $previousErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = "Continue"
        $output = & git -c filter.lfs.clean= -c filter.lfs.smudge= -c filter.lfs.required=false @GitArgs 2>$null
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }
    if ($null -eq $output) { return @() }
    return @($output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Assert-FileExists {
    param([string]$RelativePath)
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\")) -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Assert-FileContains {
    param([string]$RelativePath, [string[]]$Fragments)
    Assert-FileExists $RelativePath
    $text = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\"))
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
    }
}

function Read-Json {
    param([string]$RelativePath)
    Assert-FileExists $RelativePath
    return Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\")) | ConvertFrom-Json
}

function Assert-Branch {
    $branch = (& git -C $repoRoot branch --show-current).Trim()
    if ($branch -ne "p8-tsunami-hazard-risk-front-foundation") {
        throw "Expected branch p8-tsunami-hazard-risk-front-foundation, found $branch"
    }
}

function Assert-P8Stages {
    $stagePlan = Join-Path $repoRoot "docs\P8_STAGE_PLAN.md"
    Assert-FileExists "docs/P8_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath $stagePlan |
            Where-Object { $_ -match "^##\s+(P8-[A-Z0-9]+)\s*$" } |
            ForEach-Object { $Matches[1] }
    )
    $expected = @("P8-A", "P8-B", "P8-C", "P8-D", "P8-E")
    $unexpected = @($stages | Where-Object { $expected -notcontains $_ })
    if ($stages.Count -ne 5 -or $unexpected.Count -gt 0) {
        throw "P8 stages must be exactly P8-A through P8-E. Found: $($stages -join ', ')"
    }

    $forbidden = @(
        Get-GitLines @("-C", $repoRoot, "ls-files") |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { $_ -match "(^|[\\/_.-])p8[-_]?(0|f|g)($|[\\/_.-])" }
    )
    if ($forbidden.Count -gt 0) {
        throw "Forbidden P8-0/F/G artifact found: $($forbidden -join '; ')"
    }
}

function Assert-ProtectedPaths {
    $protected = @(Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", "ProjectSettings", "Packages", "Assets/Scenes/Chuo_BaseMap.unity", "Assets/PLATEAU"))
    if ($protected.Count -gt 0) {
        throw "Protected path dirty state detected: $($protected -join '; ')"
    }

    $stagedBaseline = @(Get-GitLines @("-C", $repoRoot, "diff", "--cached", "--name-only", "--", $baselineScene))
    if ($stagedBaseline.Count -gt 0) {
        throw "P7 high-detail baseline scene is staged."
    }

    $baselinePath = Join-Path $repoRoot ($baselineScene -replace "/", "\")
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw "Missing P7 high-detail baseline scene."
    }
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P8E_PLATEAU_SEMANTIC_BINDING_V1.md",
        "docs/P8E_REAL_OBJECT_BINDING_STATUS.md",
        "docs/P8E_ROAD_BUILDING_BRIDGE_UNDERGROUND_ENTRANCE_BINDING.md",
        "docs/P8E_HUMANITARIAN_CANDIDATE_PERSISTENT_MARKER_READINESS.md",
        "docs/P8E_HUMANITARIAN_CANDIDATE_MARKER_ANCHORING_STATUS.md",
        "docs/P8E_HUMANITARIAN_CANDIDATE_HAZARD_STATUS_READY.md",
        "docs/P8E_LIGHT_CURTAIN_PROGRESSION_MODEL.md",
        "docs/P8E_TSUNAMI_ONSHORE_SPEED_MODEL.md",
        "docs/P8E_SHALLOW_DEPTH_SLOWDOWN_RULES.md",
        "docs/P8E_RISK_FRONT_SWEEP_COMPLETION_STATUS.md",
        "docs/P8E_P2_P6_NEW_MAP_ADAPTATION_FINAL_CHECK.md",
        "docs/P8E_P2_P6_PASSED_PROXY_BLOCKED_MATRIX.md",
        "docs/P8E_P2_P6_TO_P9_BOUNDARY.md",
        "docs/P8E_P5_ROUTE_CANDIDATE_GEOMETRY_HANDOFF.md",
        "docs/P8E_ROUTE_COORDINATE_TRANSFORM_STATUS.md",
        "docs/P8E_CANDIDATE_GEOMETRY_BINDING_STATUS.md",
        "Assets/Data/P8/p8e_semantic_binding_v1.json",
        "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json",
        "Assets/Data/P8/risk_front_progression_model_config.json",
        "Assets/Data/P8/p8e_p2_p6_new_map_adaptation_matrix.json",
        "Assets/Data/P8/p8e_route_candidate_geometry_handoff.json",
        "Assets/Scripts/P8/P8RiskFrontProgressionModel.cs",
        "Assets/Scripts/P8/P8RiskFrontSpeedModel.cs",
        "Assets/Scripts/P8/P8HumanitarianCandidateMarkerData.cs",
        "Assets/Scripts/P8/P8HumanitarianCandidateMarkerStatus.cs",
        "Assets/Tests/EditMode/P8/P8EHardeningTests.cs",
        "Assets/Tests/PlayMode/P8/P8EHardeningPlayModeTests.cs",
        "tools/p8/build_p8e_semantic_binding_report.ps1",
        "tools/p8/inspect_p8e_plateau_semantic_binding.ps1",
        "tools/p8/inspect_p8e_p2_p6_new_map_adaptation.ps1",
        "tools/p8/run_p8e_hardening_preflight.ps1",
        "codex_prompts/p8e_pre_p9_baseline_completion.md",
        "deepseek_review_prompt_p8e_hardening.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-SemanticBinding {
    $data = Read-Json "Assets/Data/P8/p8e_semantic_binding_v1.json"
    if ([bool]$data.completeSceneSemanticBinding -ne $false -or [bool]$data.sceneMutationPerformed -ne $false) {
        throw "Semantic binding must not claim complete scene binding or scene mutation."
    }
    foreach ($category in @("road", "building", "bridge", "underground", "entrance", "humanitarian_candidate_proxy", "highrise_candidate_marker")) {
        if (@($data.bindings | Where-Object { $_.category -eq $category }).Count -ne 1) {
            throw "Missing semantic binding category: $category"
        }
    }
}

function Assert-MarkerReadiness {
    $data = Read-Json "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json"
    $records = @($data.records)
    if ($records.Count -ne 110 -or [int]$data.totalCandidates -ne 110) {
        throw "Expected 110 humanitarian marker records, found $($records.Count)."
    }
    foreach ($record in $records) {
        if ([bool]$record.isOfficialShelter -ne $false) {
            throw "Candidate $($record.candidateId) claims official shelter status."
        }
        if ([bool]$record.nonOfficialWarningRequired -ne $true) {
            throw "Candidate $($record.candidateId) lacks non-official warning requirement."
        }
        if ([bool]$record.selectableGameplayEnabled -ne $false -or [bool]$record.affectsGameplaySuccessFailure -ne $false) {
            throw "Candidate $($record.candidateId) enables gameplay/selectable behavior."
        }
    }
}

function Assert-LightCurtainConfig {
    $config = Read-Json "Assets/Data/P8/risk_front_progression_model_config.json"
    if ([bool]$config.useArrivalTimeWhenAvailable -ne $true -or [bool]$config.arrivalTimeOverridePriority -ne $true) {
        throw "Risk-front progression must prioritize arrivalTimeSeconds."
    }
    if ([bool]$config.shallowDepthSlowdownEnabled -ne $true) {
        throw "Risk-front progression must include shallow-depth slowdown."
    }
    if ([bool]$config.visualHeightIsCinematicOnlyRequired -ne $true) {
        throw "visualHeightMeters must remain cinematic-only."
    }
    if ([bool]$config.affectsGameplaySuccessFailure -ne $false -or [bool]$config.implementsP9Gameplay -ne $false) {
        throw "Risk-front progression must not implement gameplay rules."
    }
}

function Assert-RouteHandoff {
    $handoff = Read-Json "Assets/Data/P8/p8e_route_candidate_geometry_handoff.json"
    if ([bool]$handoff.routesAreOfficial -ne $false -or [bool]$handoff.routeRoadGeometryValidated -ne $false) {
        throw "Route handoff must not claim official or road-geometry-validated routes."
    }
    if ([bool]$handoff.wgs84PlateauTransformLimitationRemains -ne $true) {
        throw "Route handoff must document WGS84/PLATEAU transform limitation."
    }
}

function Assert-NoFalseClaims {
    Assert-FileContains "docs/P8E_P5_ROUTE_CANDIDATE_GEOMETRY_HANDOFF.md" @(
        "not official evacuation routes",
        "not road-geometry-validated",
        "not official evacuation shelters"
    )
    Assert-FileContains "docs/P8E_LIGHT_CURTAIN_PROGRESSION_MODEL.md" @(
        "visualHeightMeters remains cinematic only",
        "maxTsunamiHeightMeters remains reference metadata",
        "Derived boundaries"
    )
    Assert-FileContains "docs/P8E_PLATEAU_SEMANTIC_BINDING_V1.md" @(
        "not a full Unity scene-object binding",
        "completeSceneSemanticBinding=false",
        "sceneMutationPerformed=false"
    )
}

Write-Host "P8-E hardening preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-Branch
    Assert-P8Stages
    Assert-ProtectedPaths
    Assert-RequiredArtifacts
    Assert-SemanticBinding
    Assert-MarkerReadiness
    Assert-LightCurtainConfig
    Assert-RouteHandoff
    Assert-NoFalseClaims

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "inspect_p8e_plateau_semantic_binding.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8-E semantic binding inspection failed."
    }

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "inspect_p8e_p2_p6_new_map_adaptation.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8-E P2-P6 adaptation inspection failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-E hardening preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-E hardening preflight: PASS" -ForegroundColor Green
exit 0
