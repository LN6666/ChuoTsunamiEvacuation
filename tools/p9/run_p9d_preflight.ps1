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
        $output = & git -c filter.lfs.clean= -c filter.lfs.smudge= -c filter.lfs.process= -c filter.lfs.required=false @GitArgs 2>$null
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
    return @(
        @($diffFiles) + @($untrackedFiles) |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
    )
}

function Test-PathStartsWith {
    param([string]$Path, [string]$Prefix)
    return $Path.StartsWith($Prefix, [System.StringComparison]::OrdinalIgnoreCase)
}

function Assert-FileExists {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Assert-Branch {
    $branch = (& git -C $repoRoot branch --show-current).Trim()
    if ($branch -ne "p9d-final-integration-p10-handoff") {
        throw "Expected branch p9d-final-integration-p10-handoff, found $branch"
    }
}

function Assert-P9StageCount {
    Assert-FileExists "docs/P9_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath (Join-Path $repoRoot "docs\P9_STAGE_PLAN.md") |
            Where-Object { $_ -match "^##\s+(P9-[A-Z0-9]+)\s*$" } |
            ForEach-Object { $Matches[1] }
    )
    $expected = @("P9-A", "P9-B", "P9-C", "P9-D")
    $unexpected = @($stages | Where-Object { $expected -notcontains $_ })
    foreach ($stage in $expected) {
        if ($stages -notcontains $stage) {
            throw "Missing P9 stage: $stage"
        }
    }
    if ($stages.Count -ne 4 -or $unexpected.Count -gt 0) {
        throw "P9 must have exactly four stages: P9-A, P9-B, P9-C, P9-D. Found: $($stages -join ', ')"
    }
}

function Assert-NoExtraP9StageArtifacts {
    $trackedAndChanged = @(
        Get-GitLines @("-C", $repoRoot, "ls-files") |
            ForEach-Object { Normalize-RepoPath $_ }
    ) + @(Get-ChangedFiles)
    $files = @(
        $trackedAndChanged |
            Where-Object { $_ -match "(^|[\\/_.-])p9[-_]?[efg]($|[\\/_.-])" } |
            Sort-Object -Unique
    )
    if ($files.Count -gt 0) {
        throw "Unexpected P9 stage artifact path detected: $($files -join '; ')"
    }
}

function Assert-ProtectedPathsClean {
    $protectedStatus = @(Get-GitLines @(
        "-C", $repoRoot,
        "status", "--porcelain=v1", "--",
        "ProjectSettings",
        "Packages",
        "Assets/Scenes/Chuo_BaseMap.unity",
        "Assets/PLATEAU",
        $baselineScene
    ))
    if ($protectedStatus.Count -gt 0) {
        throw "Protected path dirty state detected: $($protectedStatus -join '; ')"
    }
}

function Test-AllowedP9DPath {
    param([string]$Path)
    if ($Path -like "docs/P9D_*.md") { return $true }
    if ($Path -eq "docs/P9_FINAL_CLOSEOUT.md") { return $true }
    if ($Path -eq "docs/P9_STAGE_PLAN.md") { return $true }
    if ($Path -eq "docs/P9C_NEXT_STEPS_TO_P9D.md") { return $true }
    if ($Path -like "Assets/Data/P9/p9d_*.json") { return $true }
    if ($Path -like "Assets/Data/P9/p9d_*.json.meta") { return $true }
    if (Test-PathStartsWith $Path "Assets/Scripts/P9/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/EditMode/P9/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/PlayMode/P9/") { return $true }
    if ($Path -eq "tools/p9/run_p9d_preflight.ps1") { return $true }
    if ($Path -eq "tools/p9/validate_p9d_json.ps1") { return $true }
    if ($Path -eq "codex_prompts/p9d_final_integration_p10_handoff.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p9d.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-AllowedP9DPath $file) { continue }
        $violations.Add("$file is outside P9-D allowed paths") | Out-Null
    }
    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }
        throw "P9-D changed-file scope check failed."
    }
    Write-Host "P9-D changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P9D_FINAL_INTEGRATION.md",
        "docs/P9D_COORDINATE_BASED_ANCHORING.md",
        "docs/P9D_ANCHORING_REPORT.md",
        "docs/P9D_FULL_GAMEPLAY_FLOW.md",
        "docs/P9D_P2_P6_FINAL_COMPATIBILITY.md",
        "docs/P9D_HUMANITARIAN_CANDIDATE_FINAL_STATUS.md",
        "docs/P9D_ROUTE_AND_GEOMETRY_LIMITATIONS.md",
        "docs/P9D_RESULT_PANEL_AND_REASON_CODE_FINAL_CHECK.md",
        "docs/P9D_TEST_RESULTS.md",
        "docs/P9D_KNOWN_LIMITATIONS.md",
        "docs/P9D_P10_HANDOFF.md",
        "docs/P9D_REVIEW_BACKLOG.md",
        "docs/P9_FINAL_CLOSEOUT.md",
        "Assets/Data/P9/p9d_coordinate_anchoring_config.json",
        "Assets/Data/P9/p9d_anchoring_report_sample.json",
        "Assets/Data/P9/p9d_final_gameplay_scenario_sample.json",
        "Assets/Data/P9/p9d_p10_handoff_status.json",
        "Assets/Scripts/P9/P9DCoordinateAnchoringConfig.cs",
        "Assets/Scripts/P9/P9DCoordinateAnchor.cs",
        "Assets/Scripts/P9/P9DCoordinateAnchoringResult.cs",
        "Assets/Scripts/P9/P9DNearestAnchorMatcher.cs",
        "Assets/Scripts/P9/P9DAnchoringConfidence.cs",
        "Assets/Scripts/P9/P9DFinalGameplayFlowValidator.cs",
        "Assets/Scripts/P9/P9DP10HandoffSummary.cs",
        "Assets/Tests/EditMode/P9/P9DCoordinateAnchoringEditModeTests.cs",
        "Assets/Tests/EditMode/P9/P9DFinalIntegrationEditModeTests.cs",
        "Assets/Tests/PlayMode/P9/P9DFinalRuntimePlayModeTests.cs",
        "tools/p9/run_p9d_preflight.ps1",
        "tools/p9/validate_p9d_json.ps1",
        "codex_prompts/p9d_final_integration_p10_handoff.md",
        "deepseek_review_prompt_p9d.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-P8HandoffFiles {
    foreach ($file in @(
        "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json",
        "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json",
        "Assets/Data/P8/p8e_semantic_binding_v1.json",
        "Assets/Data/P8/p8e_p2_p6_new_map_adaptation_matrix.json",
        "Assets/Data/P8/p8e_route_candidate_geometry_handoff.json",
        "Assets/Data/P8/risk_front_progression_model_config.json"
    )) {
        Assert-FileExists $file
    }
}

function Assert-CodeBoundaries {
    $sourceDir = Join-Path $repoRoot "Assets\Scripts\P9"
    $sourceText = @(
        Get-ChildItem -LiteralPath $sourceDir -Filter "*.cs" |
            ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }
    ) -join "`n"
    $p9dDataText = @(
        Get-ChildItem -LiteralPath (Join-Path $repoRoot "Assets\Data\P9") -Filter "p9d_*.json" |
            ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }
    ) -join "`n"

    foreach ($forbidden in @(
        "EvacuationGameManager",
        "ResultPanelController",
        "ShelterEntranceTrigger",
        "BuildingShelter"
    )) {
        if ($sourceText.Contains($forbidden)) {
            throw "P9-D source must not directly reference legacy outcome/controller class $forbidden."
        }
    }

    foreach ($forbiddenIndoor in @(
        "representative indoor template",
        "bim indoor",
        "indoor staircase",
        "fire escape route interior"
    )) {
        if ($sourceText.IndexOf($forbiddenIndoor, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw "P9-D runtime source contains forbidden building-interior gameplay wording: $forbiddenIndoor"
        }
    }

    foreach ($forbiddenClaim in @(
        '"claimsOfficialRouteStatus": true',
        '"claimsExactPlateauObjectBinding": true',
        '"routeIsOfficial": true',
        '"exactPlateauObjectIdentityClaimed": true',
        '"p9EfgCreated": true',
        '"p10ReleasePackagingStarted": true'
    )) {
        if ($p9dDataText.Contains($forbiddenClaim)) {
            throw "P9-D data contains forbidden claim/state: $forbiddenClaim"
        }
    }
}

function Assert-DocsBoundaries {
    foreach ($file in @(
        "docs/P9D_COORDINATE_BASED_ANCHORING.md",
        "docs/P9D_ROUTE_AND_GEOMETRY_LIMITATIONS.md",
        "docs/P9D_P10_HANDOFF.md"
    )) {
        $text = Get-Content -Raw -LiteralPath (Join-Path $repoRoot ($file -replace "/", "\"))
        foreach ($required in @(
            "proxy",
            "not official",
            "exact PLATEAU"
        )) {
            if ($text.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
                throw "$file must clearly document '$required'."
            }
        }
    }
}

Write-Host "P9-D preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-Branch
    Assert-P9StageCount
    Assert-NoExtraP9StageArtifacts
    Assert-ProtectedPathsClean
    Assert-ChangedFilesAllowed
    Assert-RequiredArtifacts
    Assert-P8HandoffFiles
    Assert-CodeBoundaries
    Assert-DocsBoundaries

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p9d_json.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P9-D JSON validation failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P9-D preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P9-D preflight: PASS" -ForegroundColor Green
exit 0
