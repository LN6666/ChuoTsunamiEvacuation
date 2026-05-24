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
    while ($normalized.StartsWith("/", [System.StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(1)
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

function Get-ChangedFiles {
    $diffFiles = Get-GitLines @("-C", $repoRoot, "diff", "--name-only", "HEAD", "--")
    $untrackedFiles = Get-GitLines @("-C", $repoRoot, "ls-files", "--others", "--exclude-standard")
    return @(@($diffFiles) + @($untrackedFiles) | ForEach-Object { Normalize-RepoPath $_ } | Where-Object { $_ } | Sort-Object -Unique)
}

function Assert-FileExists {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
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

function Assert-P8StagePlan {
    Assert-FileExists "docs/P8_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath (Join-Path $repoRoot "docs\P8_STAGE_PLAN.md") |
            Where-Object { $_ -match "^##\s+(P8-[A-Z0-9]+)\s*$" } |
            ForEach-Object { $Matches[1] }
    )
    $expected = @("P8-A", "P8-B", "P8-C", "P8-D", "P8-E")
    $unexpected = @($stages | Where-Object { $expected -notcontains $_ })
    if ($stages.Count -ne 5 -or $unexpected.Count -gt 0) {
        throw "P8 stage headings must be exactly P8-A through P8-E. Found: $($stages -join ', ')"
    }

    $forbiddenHeadings = @($stages | Where-Object { $_ -eq "P8-0" -or $_ -eq "P8-F" -or $_ -eq "P8-G" })
    if ($forbiddenHeadings.Count -gt 0) {
        throw "Forbidden P8 stage heading found: $($forbiddenHeadings -join ', ')"
    }

    $trackedForbidden = @(
        Get-GitLines @("-C", $repoRoot, "ls-files") |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { $_ -match "(^|[\\/_.-])p8[-_]?(0|f|g)($|[\\/_.-])" }
    )
    if ($trackedForbidden.Count -gt 0) {
        throw "Forbidden P8-0/F/G artifact path found: $($trackedForbidden -join '; ')"
    }
}

function Assert-ProtectedPaths {
    $protectedStatus = @(Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", "ProjectSettings", "Packages", "Assets/Scenes/Chuo_BaseMap.unity", "Assets/PLATEAU"))
    if ($protectedStatus.Count -gt 0) {
        throw "Protected path dirty state detected: $($protectedStatus -join '; ')"
    }

    $stagedBaseline = @(Get-GitLines @("-C", $repoRoot, "diff", "--cached", "--name-only", "--", $baselineScene))
    if ($stagedBaseline.Count -gt 0) {
        throw "P7_HighDetail baseline scene is staged; do not commit scene mutation."
    }

    $baselinePath = Join-Path $repoRoot ($baselineScene -replace "/", "\")
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw "Missing P7_HighDetail baseline scene: $baselineScene"
    }
}

function Assert-RequiredDocs {
    foreach ($file in @(
        "docs/P8B_PROBLEM1_OFFICIAL_HAZARD_LAYER_RESOLUTION.md",
        "docs/P8B_HAZARD_LAYER_V1_FINAL_STATUS.md",
        "docs/P8B_TO_P8C_GATE_DECISION.md",
        "docs/P8C_PROBLEM2_P2_P6_NEW_MAP_ADAPTATION_STATUS.md",
        "docs/P8C_PROBLEM3_RISK_FRONT_INFRASTRUCTURE_CLOSURE_STATUS.md",
        "docs/P8C_TO_P8D_E_HANDOFF.md",
        "docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md",
        "docs/P8C_HUMANITARIAN_CANDIDATE_HANDOFF.md",
        "docs/P8D_HUMANITARIAN_CANDIDATE_DAMAGE_STATUS_SCOPE.md",
        "docs/P8E_HUMANITARIAN_CANDIDATE_PERSISTENT_VISIBILITY_SCOPE.md"
    )) {
        Assert-FileExists $file
    }

    Assert-FileContains "docs/P8B_PROBLEM1_OFFICIAL_HAZARD_LAYER_RESOLUTION.md" @(
        "broader Tokyo damage-estimation source family",
        "Taisho Kanto earthquake",
        "Nankai Trough megathrust earthquake case 1",
        "not a tsunami scenario in current extracted layers",
        "case 5",
        "case 8",
        "not official contour",
        "not used by the P8-B hazard layer v1"
    )

    Assert-FileContains "docs/P8C_PROBLEM2_P2_P6_NEW_MAP_ADAPTATION_STATUS.md" @(
        "smoke/proxy level",
        "No gameplay success/failure rule changes",
        "P9 owns final real gameplay landing"
    )

    Assert-FileContains "docs/P8C_PROBLEM3_RISK_FRONT_INFRASTRUCTURE_CLOSURE_STATUS.md" @(
        "humanitarian_candidate_proxy",
        "highrise_candidate_marker",
        "reference metadata only",
        "cinematic only"
    )

    Assert-FileContains "docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md" @(
        "Total candidates found:",
        "Total PLATEAU-derived candidates:",
        "Candidate list expanded beyond original 5 sample candidates: yes",
        "These are not official evacuation shelters",
        "must not be displayed as official shelters",
        "Table A: Named High-Rise / Office / Tower Candidates",
        "Table C: PLATEAU ID-Only Candidates Requiring Manual Name Review",
        "p5f_sample_humanitarian_strong_003",
        "p5f_sample_not_recommended_007"
    )
}

function Assert-HazardOfficialSource {
    $registry = Read-Json "Assets/Data/P8/tsunami_hazard_evidence_registry.json"
    $layer = Read-Json "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json"

    if ([string]$registry.currentGateDecision -ne "PASS" -or [string]$layer.p8cGateDecision -ne "PASS") {
        throw "P8-B/P8-C gate decision must be PASS."
    }
    if ([string]$layer.sourceMode -ne "official_tsunami_metropolitan") {
        throw "Hazard layer must use official_tsunami_metropolitan sourceMode."
    }
    if ([bool]$layer.officialTokyoTsunamiDatasetCheck.usesTokyoMetropolitanGovernmentDamageEstimationTsunamiDatasets -ne $true) {
        throw "Hazard layer must explicitly reference Tokyo Metropolitan Government tsunami damage-estimation datasets."
    }
    if ([bool]$layer.officialTokyoTsunamiDatasetCheck.usesChuoFloodHazardMapAsTsunamiData -ne $false) {
        throw "Hazard layer must not use Chuo flood hazard map as tsunami data."
    }

    $features = @($layer.features)
    if ($features.Count -ne 2) {
        throw "Current generated Chuo layer should contain exactly two extracted scenario features."
    }

    foreach ($feature in $features) {
        if ([string]$feature.evidenceSourceId -ne "tokyo_damage_estimation_map_tsunami") {
            throw "Feature $($feature.featureId) must reference tokyo_damage_estimation_map_tsunami."
        }
        if ([double]$feature.inundationDepthMeters -le 0 -or [double]$feature.maxTsunamiHeightMeters -le 0) {
            throw "Feature $($feature.featureId) must include positive depth and height values."
        }
        if ([double]$feature.arrivalTimeSeconds -le 0) {
            throw "Feature $($feature.featureId) must include arrivalTimeSeconds."
        }
        if ([string]$feature.boundaryStatus -notmatch "not_official_inundation_contour") {
            throw "Feature $($feature.featureId) must label boundary as not official contour."
        }
        if ([bool]$feature.visualHeightIsCinematicOnly -ne $true) {
            throw "Feature $($feature.featureId) must keep visualHeightMeters cinematic only."
        }
    }
}

function Assert-NoOutOfScopeImplementation {
    $changedFiles = @(Get-ChangedFiles)
    foreach ($file in $changedFiles) {
        if ($file -eq $baselineScene) { continue }
        if ($file -match "^(Assets/(Scripts|Tests)/(P9|P10)|tools/(p9|p10)/|docs/P(9|10))") {
            throw "P8-B/C consolidation must not add P9/P10 systems: $file"
        }
        if ($file -match "^(Assets/(Scripts|Tests)/P8/.*CollapseProxy|tools/p8/.*p8d)") {
            throw "P8-B/C consolidation must not implement P8-D collapse systems: $file"
        }
    }

    $evaluator = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "Assets\Scripts\P8\P8InfrastructureHazardEvaluator.cs")
    if ($evaluator -notmatch "ImplementsCollapseProxy = false" -or $evaluator -notmatch "RequiresP9Systems = false") {
        throw "P8-C evaluator must keep P8-D/P9 implementation flags false."
    }

    $infrastructure = Read-Json "Assets/Data/P8/infrastructure_hazard_interaction_config.json"
    if ([bool]$infrastructure.collapseProxyEnabledInP8C -ne $false -or [bool]$infrastructure.hazardDrivenCollapse -ne $false) {
        throw "Infrastructure config must not enable P8-D collapse in this consolidation."
    }
}

function Assert-NoFalseClaims {
    Assert-FileContains "docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md" @(
        "isOfficialShelter=false",
        "nonOfficialWarningRequired=true",
        "manualReviewNeeded=true",
        "Future persistent visibility requires explicit non-official labeling"
    )

    Assert-FileContains "docs/P8B_HAZARD_LAYER_V1_FINAL_STATUS.md" @(
        "not an official inundation contour",
        "must not be used as",
        "not based on Chuo flood hazard map proxy data"
    )
}

Write-Host "P8-B/C consolidation preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-P8StagePlan
    Assert-ProtectedPaths
    Assert-RequiredDocs
    Assert-HazardOfficialSource
    Assert-NoOutOfScopeImplementation
    Assert-NoFalseClaims
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-B/C consolidation preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-B/C consolidation preflight: PASS" -ForegroundColor Green
exit 0
