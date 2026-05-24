[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$baselineScene = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"

function Normalize-RepoPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ""
    }

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

    if ($null -eq $output) {
        return @()
    }

    return @($output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-ChangedFiles {
    $diffFiles = Get-GitLines @("-C", $repoRoot, "diff", "--name-only", "HEAD", "--")
    $untrackedFiles = Get-GitLines @("-C", $repoRoot, "ls-files", "--others", "--exclude-standard")

    return @(
        @($diffFiles) + @($untrackedFiles) |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
    )
}

function Test-PathStartsWith {
    param(
        [string]$Path,
        [string]$Prefix
    )

    return $Path.StartsWith($Prefix, [System.StringComparison]::OrdinalIgnoreCase)
}

function Assert-FileExists {
    param([string]$RelativePath)

    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Assert-FileContains {
    param(
        [string]$RelativePath,
        [string[]]$Fragments
    )

    Assert-FileExists $RelativePath
    $text = Get-Content -Raw -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\"))
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
    }
}

function Assert-Branch {
    $branch = (& git -C $repoRoot branch --show-current).Trim()
    if ($branch -ne "p8-tsunami-hazard-risk-front-foundation") {
        throw "Expected branch p8-tsunami-hazard-risk-front-foundation, found $branch"
    }
}

function Assert-P8StageCount {
    Assert-FileExists "docs/P8_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath (Join-Path $repoRoot "docs\P8_STAGE_PLAN.md") |
            Where-Object { $_ -match "^##\s+(P8-[A-Z0-9]+)\s*$" } |
            ForEach-Object { $Matches[1] }
    )

    $expected = @("P8-A", "P8-B", "P8-C", "P8-D", "P8-E")
    $unexpected = @($stages | Where-Object { $expected -notcontains $_ })
    if ($stages.Count -ne 5 -or $unexpected.Count -gt 0) {
        throw "P8 must have exactly five stages: P8-A, P8-B, P8-C, P8-D, P8-E. Found: $($stages -join ', ')"
    }
}

function Assert-NoExtraP8Stages {
    $files = @(
        Get-GitLines @("-C", $repoRoot, "ls-files") |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { $_ -match "(^|[\\/_.-])p8[-_]?(0|f|g)($|[\\/_.-])" }
    )

    if ($files.Count -gt 0) {
        throw "Unexpected P8-0/F/G artifact path detected: $($files -join '; ')"
    }
}

function Assert-ProtectedPaths {
    $protectedStatus = @(Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", "ProjectSettings", "Packages", "Assets/Scenes/Chuo_BaseMap.unity", "Assets/PLATEAU"))
    if ($protectedStatus.Count -gt 0) {
        throw "Protected path dirty state detected: $($protectedStatus -join '; ')"
    }

    $stagedBaseline = @(Get-GitLines @("-C", $repoRoot, "diff", "--cached", "--name-only", "--", $baselineScene))
    if ($stagedBaseline.Count -gt 0) {
        throw "P7 high-detail baseline scene is staged; do not commit scene mutation."
    }

    $baselinePath = Join-Path $repoRoot ($baselineScene -replace "/", "\")
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw "Missing P7 high-detail baseline scene: $baselineScene"
    }

    $baselineItem = Get-Item -LiteralPath $baselinePath
    if ($baselineItem.Length -lt 1000000000) {
        throw "P7 high-detail baseline scene is unexpectedly small; baseline may be lost."
    }
}

function Test-AllowedP8CPath {
    param([string]$Path)

    if ($Path -eq $baselineScene) { return $true }
    if ($Path -like "docs/P8*.md") { return $true }
    if ($Path -eq "docs/TASKS.md") { return $true }
    if ($Path -eq "docs/REVIEW_BACKLOG.md") { return $true }
    if ($Path -eq "docs/P8_STAGE_PLAN.md") { return $true }
    if ($Path -like "tools/p8/*p8bc*.ps1") { return $true }
    if ($Path -like "tools/p8/*p8c*.ps1") { return $true }
    if ($Path -like "tools/p8/*p8d*.ps1") { return $true }
    if ($Path -eq "tools/p8/inspect_p8a_scene_compatibility.ps1") { return $true }
    if ($Path -eq "tools/p8/run_p8a_preflight.ps1") { return $true }
    if ($Path -eq "tools/p8/run_p8a_compat_preflight.ps1") { return $true }
    if ($Path -eq "tools/p8/run_p8b_evidence_preflight.ps1") { return $true }
    if ($Path -eq "tools/p8/run_p8b_guard_preflight.ps1") { return $true }
    if ($Path -eq "tools/p8/run_p8b_riskfront_preflight.ps1") { return $true }
    if ($Path -eq "tools/p8/run_p8b_front_v1_preflight.ps1") { return $true }
    if ($Path -eq "tools/p8/run_p8b_evidence_spatial_gate.ps1") { return $true }
    if ($Path -eq "tools/p8/build_p8_humanitarian_candidate_audit.py") { return $true }
    if ($Path -eq "tools/p8/run_p8_humanitarian_candidate_audit_preflight.ps1") { return $true }
    if (Test-PathStartsWith $Path "Assets/Scripts/P8/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/EditMode/P8/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/PlayMode/P8/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Data/P8/") { return $true }
    if ($Path -eq "codex_prompts/p8c_infrastructure_hazard_interaction.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p8c.md") { return $true }
    if ($Path -like "codex_prompts/p8*.md") { return $true }
    if ($Path -like "deepseek_review_prompt_p8*.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]

    foreach ($file in $changedFiles) {
        if (Test-AllowedP8CPath $file) {
            continue
        }

        $violations.Add("$file is outside P8-C allowed paths") | Out-Null
    }

    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }

        throw "P8-C changed-file scope check failed."
    }

    Write-Host "P8-C changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "Assets/Scripts/P8/P8InfrastructureHazardData.cs",
        "Assets/Scripts/P8/P8InfrastructureHazardState.cs",
        "Assets/Scripts/P8/P8InfrastructureHazardEvaluator.cs",
        "Assets/Scripts/P8/P8InfrastructureHazardTarget.cs",
        "Assets/Scripts/P8/P8InfrastructureHazardProxy.cs",
        "Assets/Scripts/P8/P8InfrastructureHazardMarker.cs",
        "Assets/Scripts/P8/P8InfrastructureHazardDebugSummary.cs",
        "Assets/Scripts/P8/P8P2P6RuntimeCompatibilityInspector.cs",
        "Assets/Tests/EditMode/P8/P8InfrastructureHazardEvaluatorTests.cs",
        "Assets/Tests/PlayMode/P8/P8InfrastructureHazardPlayModeTests.cs",
        "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json",
        "Assets/Data/P8/tsunami_hazard_evidence_registry.json",
        "Assets/Data/P8/infrastructure_hazard_interaction_config.json",
        "Assets/Data/P8/risk_front_visualization_config.json",
        "Assets/Scripts/P8/P8RiskFrontController.cs",
        "tools/p8/inspect_p8c_p2_p6_runtime_adaptation.ps1",
        "tools/p8/run_p8c_preflight.ps1",
        "docs/P8C_INFRASTRUCTURE_HAZARD_INTERACTION_DESIGN.md",
        "docs/P8C_HAZARD_TO_INFRASTRUCTURE_MAPPING.md",
        "docs/P8C_ROAD_BUILDING_BRIDGE_UNDERGROUND_STATUS.md",
        "docs/P8C_RISK_FRONT_INFRASTRUCTURE_LINK.md",
        "docs/P8C_P2_P6_RUNTIME_ADAPTATION_REPORT.md",
        "docs/P8C_NEW_MAP_GAMEPLAY_SMOKE_RESULTS.md",
        "docs/P8C_TEST_RESULTS.md",
        "docs/P8C_KNOWN_LIMITATIONS.md",
        "docs/P8C_TO_P8D_HANDOFF.md",
        "codex_prompts/p8c_infrastructure_hazard_interaction.md",
        "deepseek_review_prompt_p8c.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-CodeAndDataRules {
    Assert-FileContains "Assets/Scripts/P8/P8InfrastructureHazardEvaluator.cs" @(
        "arrivalTimeSeconds",
        "inundationDepthMeters",
        "hazardIntensity",
        "confidence",
        "sourceMode",
        "evidenceSourceId",
        "maxTsunamiHeightMeters is reference metadata only",
        "visualHeightMeters is cinematic only",
        "AffectsGameplaySuccessFailure = false",
        "ImplementsCollapseProxy = false",
        "RequiresP9Systems = false"
    )

    Assert-FileContains "Assets/Scripts/P8/P8InfrastructureHazardData.cs" @(
        "Road",
        "Building",
        "Bridge",
        "Underground",
        "Entrance",
        "Waterfront",
        "OpenSpace",
        "ShelterProxy",
        "NavigationTargetProxy",
        "HumanitarianCandidateProxy",
        "HighriseCandidateMarker"
    )

    Assert-FileContains "Assets/Scripts/P8/P8InfrastructureHazardState.cs" @(
        "Safe",
        "Watch",
        "Warning",
        "InundatedProxy",
        "RestrictedProxy",
        "AvoidProxy"
    )

    $infrastructure = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "Assets\Data\P8\infrastructure_hazard_interaction_config.json") | ConvertFrom-Json
    if ([bool]$infrastructure.interactionEnabledInP8A -ne $false -or [bool]$infrastructure.interactionEnabledInP8C -ne $true) {
        throw "Infrastructure config must keep P8-A disabled and enable only P8-C proxy interaction."
    }
    if ([bool]$infrastructure.collapseProxyEnabledInP8C -ne $false -or [bool]$infrastructure.collapseGameplayEnabledInP8C -ne $false -or [bool]$infrastructure.hazardDrivenCollapse -ne $false) {
        throw "P8-C must not enable collapse proxy/gameplay."
    }
}

function Assert-NoOutOfScopeSystems {
    $changedFiles = @(Get-ChangedFiles)
    foreach ($file in $changedFiles) {
        if ($file -eq $baselineScene) {
            continue
        }

        if ($file -match "^(Assets/(Scripts|Tests)/(P9|P10)|tools/(p9|p10)/|docs/P(9|10))") {
            throw "P8-C must not add P9/P10 systems: $file"
        }
    }

    $p8cSource = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "Assets\Scripts\P8\P8InfrastructureHazardEvaluator.cs")
    if ($p8cSource -match "EvacuationGameManager|ResultPanelController|ShelterEntranceTrigger|BuildingShelter") {
        throw "P8-C evaluator must not couple to gameplay success/failure or shelter controllers."
    }
}

Write-Host "P8-C preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-Branch
    Assert-P8StageCount
    Assert-NoExtraP8Stages
    Assert-ProtectedPaths
    Assert-ChangedFilesAllowed
    Assert-RequiredArtifacts
    Assert-CodeAndDataRules
    Assert-NoOutOfScopeSystems

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "inspect_p8c_p2_p6_runtime_adaptation.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8-C P2-P6 runtime adaptation inspection failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-C preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-C preflight: PASS" -ForegroundColor Green
exit 0
