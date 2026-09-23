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
        throw "P8 stage headings must be exactly P8-A through P8-E. Found: $($stages -join ', ')"
    }

    $forbidden = @(
        Get-GitLines @("-C", $repoRoot, "ls-files") |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { $_ -match "(^|[\\/_.-])p8[-_]?(0|f|g)($|[\\/_.-])" }
    )
    if ($forbidden.Count -gt 0) {
        throw "Forbidden P8-0/F/G artifact path found: $($forbidden -join '; ')"
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
    if ((Get-Item -LiteralPath $baselinePath).Length -lt 1000000000) {
        throw "P7_HighDetail baseline scene is unexpectedly small; baseline may be lost."
    }
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P8E_FINAL_CLOSEOUT_SUMMARY.md",
        "docs/P8_FINAL_STAGE_STATUS.md",
        "docs/P8_FINAL_KNOWN_LIMITATIONS.md",
        "docs/P8_FINAL_REVIEW_CHECKLIST.md",
        "docs/P8E_HUMANITARIAN_CANDIDATE_PERSISTENT_VISIBILITY_PLAN.md",
        "docs/P8E_HUMANITARIAN_CANDIDATE_NON_OFFICIAL_DISCLAIMER_RULES.md",
        "docs/P8E_HUMANITARIAN_CANDIDATE_P9_HANDOFF.md",
        "docs/P8E_P9_HANDOFF_PACKAGE.md",
        "docs/P8E_P9_REQUIRED_INPUTS.md",
        "docs/P8E_P9_DO_NOT_REDO_P8_SCOPE.md",
        "docs/P8E_BASELINE_DECISION_FOR_P9.md",
        "docs/P8E_FINAL_LIMITATIONS_AND_B_LEVEL_FOLLOWUPS.md",
        "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json",
        "Assets/Tests/EditMode/P8/P8EFinalHandoffTests.cs",
        "tools/p8/validate_p8e_handoff_package.ps1",
        "tools/p8/run_p8e_preflight.ps1",
        "tools/p8/run_p8_final_preflight.ps1",
        "codex_prompts/p8e_final_closeout_p9_handoff.md",
        "deepseek_review_prompt_p8e_final.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-P8FoundationArtifacts {
    foreach ($file in @(
        "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json",
        "Assets/Data/P8/tsunami_hazard_evidence_registry.json",
        "Assets/Data/P8/risk_front_visualization_config.json",
        "Assets/Scripts/P8/P8RiskFrontController.cs",
        "Assets/Scripts/P8/P8InfrastructureHazardEvaluator.cs",
        "Assets/Data/P8/infrastructure_hazard_interaction_config.json",
        "Assets/Scripts/P8/P8InfrastructureDamageEvaluator.cs",
        "Assets/Data/P8/infrastructure_damage_proxy_config.json",
        "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json",
        "docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-HumanitarianCandidates {
    $audit = Read-Json "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json"
    $handoff = Read-Json "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json"
    $records = @($audit.records)
    if ($records.Count -ne 110) {
        throw "Expected 110 humanitarian candidates, found $($records.Count)."
    }
    foreach ($record in $records) {
        if ([bool]$record.isOfficialShelter -ne $false) {
            throw "Candidate $($record.candidateId) claims official shelter status."
        }
        if ([bool]$record.nonOfficialWarningRequired -ne $true) {
            throw "Candidate $($record.candidateId) lacks non-official warning requirement."
        }
        if ([bool]$record.implementsP9SelectableGameplay -ne $false) {
            throw "Candidate $($record.candidateId) enables P9 selectable gameplay."
        }
    }
    if ([bool]$handoff.isOfficialShelterDataset -ne $false -or [bool]$handoff.isOfficialShelter -ne $false -or [bool]$handoff.nonOfficialWarningRequired -ne $true) {
        throw "P8-E handoff must keep candidates non-official and warning-required."
    }
    if ([bool]$handoff.selectableGameplayEnabledInP8E -ne $false -or [bool]$handoff.implementsP9Gameplay -ne $false) {
        throw "P8-E handoff must not enable P9 gameplay."
    }
}

function Assert-NoOutOfScopeImplementation {
    $changedFiles = @(Get-ChangedFiles)
    foreach ($file in $changedFiles) {
        if ($file -eq $baselineScene) { continue }
        if ($file -match "^(Assets/(Scripts|Tests)/(P9|P10)|tools/(p9|p10)/|docs/P(9|10))") {
            throw "P8-E must not add P9/P10 systems: $file"
        }
    }
}

function Assert-NoFalsePhysicalClaims {
    Assert-FileContains "docs/P8E_FINAL_CLOSEOUT_SUMMARY.md" @(
        "cinematic visual layer",
        "no real structural collapse",
        "no physics collapse",
        "no gameplay success/failure rule changes"
    )
    Assert-FileContains "docs/P8_FINAL_KNOWN_LIMITATIONS.md" @(
        "not full official inundation contours",
        "not engineering predictions",
        "No real indoor scene"
    )
    Assert-FileContains "docs/P8E_P9_HANDOFF_PACKAGE.md" @(
        "visualHeightMeters is not physical tsunami height",
        "not real structural collapse",
        "not physical tsunami height",
        "not official contours"
    )
}

Write-Host "P8-E preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-Branch
    Assert-P8StageCount
    Assert-ProtectedPaths
    Assert-RequiredArtifacts
    Assert-P8FoundationArtifacts
    Assert-HumanitarianCandidates
    Assert-NoOutOfScopeImplementation
    Assert-NoFalsePhysicalClaims

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p8e_handoff_package.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8-E handoff package validation failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-E preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-E preflight: PASS" -ForegroundColor Green
exit 0
