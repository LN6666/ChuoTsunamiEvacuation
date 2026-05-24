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
    if ($branch -ne "p10a-gap-closure-high-detail-qa") {
        throw "Expected branch p10a-gap-closure-high-detail-qa, found $branch"
    }
}

function Assert-P10StageCount {
    Assert-FileExists "docs/P10_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath (Join-Path $repoRoot "docs\P10_STAGE_PLAN.md") |
            Where-Object { $_ -match "^##\s+(P10-[A-Z0-9]+)\s*$" } |
            ForEach-Object { $Matches[1] }
    )
    $expected = @("P10-A", "P10-B", "P10-C", "P10-D")
    $unexpected = @($stages | Where-Object { $expected -notcontains $_ })
    foreach ($stage in $expected) {
        if ($stages -notcontains $stage) {
            throw "Missing P10 stage: $stage"
        }
    }
    if ($stages.Count -ne 4 -or $unexpected.Count -gt 0) {
        throw "P10 must have exactly four stages: P10-A, P10-B, P10-C, P10-D. Found: $($stages -join ', ')"
    }
}

function Assert-NoExtraP10StageArtifacts {
    $trackedAndChanged = @(
        Get-GitLines @("-C", $repoRoot, "ls-files") |
            ForEach-Object { Normalize-RepoPath $_ }
    ) + @(Get-ChangedFiles)
    $files = @(
        $trackedAndChanged |
            Where-Object { $_ -match "(^|[\\/_.-])p10[-_]?[efg]($|[\\/_.-])" } |
            Sort-Object -Unique
    )
    if ($files.Count -gt 0) {
        throw "Unexpected P10 stage artifact path detected: $($files -join '; ')"
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

function Test-AllowedP10APath {
    param([string]$Path)
    if ($Path -eq "Assets/Data/P10.meta") { return $true }
    if ($Path -like "Assets/Data/P10/p10a_*.json") { return $true }
    if ($Path -like "Assets/Data/P10/p10a_*.json.meta") { return $true }
    if ($Path -like "docs/P10A_*.md") { return $true }
    if ($Path -eq "docs/P10_STAGE_PLAN.md") { return $true }
    if ($Path -eq "docs/P9_FINAL_CLOSEOUT.md") { return $true }
    if ($Path -eq "docs/P9D_P10_HANDOFF.md") { return $true }
    if ($Path -eq "docs/REVIEW_BACKLOG.md") { return $true }
    if ($Path -eq "docs/TASKS.md") { return $true }
    if ($Path -eq "tools/p10/run_p10a_preflight.ps1") { return $true }
    if ($Path -eq "tools/p10/validate_p10a_json.ps1") { return $true }
    if ($Path -eq "codex_prompts/p10a_gap_closure_high_detail_qa.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p10a.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-AllowedP10APath $file) { continue }
        $violations.Add("$file is outside P10-A allowed paths") | Out-Null
    }
    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }
        throw "P10-A changed-file scope check failed."
    }
    Write-Host "P10-A changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P10_STAGE_PLAN.md",
        "docs/P10A_REMAINING_GAP_CLOSURE.md",
        "docs/P10A_HIGH_DETAIL_SCENE_QA.md",
        "docs/P10A_COORDINATE_ANCHORING_FINAL_QA.md",
        "docs/P10A_HUMANITARIAN_CANDIDATE_FINAL_QA.md",
        "docs/P10A_ROUTE_AND_GEOMETRY_QA.md",
        "docs/P10A_HAZARD_FRONT_LIGHT_CURTAIN_QA.md",
        "docs/P10A_RESULT_PANEL_QA.md",
        "docs/P10A_FULL_GAMEPLAY_SMOKE_QA.md",
        "docs/P10A_P10B_READINESS.md",
        "docs/P10A_TEST_RESULTS.md",
        "docs/P10A_KNOWN_LIMITATIONS.md",
        "docs/P10A_REVIEW_BACKLOG.md",
        "docs/P10A_NEXT_STEPS_TO_P10B.md",
        "Assets/Data/P10/p10a_gap_closure_matrix.json",
        "Assets/Data/P10/p10a_high_detail_scene_qa_status.json",
        "Assets/Data/P10/p10a_anchor_final_qa_status.json",
        "Assets/Data/P10/p10a_p10b_readiness_checklist.json",
        "tools/p10/run_p10a_preflight.ps1",
        "tools/p10/validate_p10a_json.ps1",
        "codex_prompts/p10a_gap_closure_high_detail_qa.md",
        "deepseek_review_prompt_p10a.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-HighDetailSceneAvailable {
    Assert-FileExists $baselineScene
}

function Assert-NoBuildOrReleaseArtifacts {
    $changedFiles = @(Get-ChangedFiles)
    $forbidden = @(
        $changedFiles |
            Where-Object {
                (Test-PathStartsWith -Path $_ -Prefix "Builds/") -or
                (Test-PathStartsWith -Path $_ -Prefix "Release/") -or
                (Test-PathStartsWith -Path $_ -Prefix "release_package/") -or
                (Test-PathStartsWith -Path $_ -Prefix "archives/") -or
                (Test-PathStartsWith -Path $_ -Prefix "run_logs/")
            } |
            Sort-Object -Unique
    )
    if ($forbidden.Count -gt 0) {
        throw "P10-A must not add build/release/archive artifacts: $($forbidden -join '; ')"
    }
}

function Assert-ClaimBoundaries {
    $p10JsonText = @(
        Get-ChildItem -LiteralPath (Join-Path $repoRoot "Assets\Data\P10") -Filter "p10a_*.json" |
            ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }
    ) -join "`n"

    foreach ($forbiddenClaim in @(
        '"noP10EfgCreated": false',
        '"newGameplaySystemAdded": true',
        '"p8P9Reimplemented": true',
        '"sceneMutationRequired": true',
        '"sceneMutationPerformed": true',
        '"routeOfficialClaimed": true',
        '"gisGradeProofClaimed": true',
        '"exactPlateauObjectIdentityClaimed": true',
        '"p10bBuildStarted": true',
        '"p10cReleasePackagingStarted": true',
        '"p10cArchiveCompleted": true'
    )) {
        if ($p10JsonText.Contains($forbiddenClaim)) {
            throw "P10-A data contains forbidden claim/state: $forbiddenClaim"
        }
    }

    foreach ($file in @(
        "docs/P10A_COORDINATE_ANCHORING_FINAL_QA.md",
        "docs/P10A_ROUTE_AND_GEOMETRY_QA.md",
        "docs/P10A_P10B_READINESS.md"
    )) {
        $text = Get-Content -Raw -LiteralPath (Join-Path $repoRoot ($file -replace "/", "\"))
        foreach ($required in @(
            "proxy",
            "not official",
            "not fully road-geometry validated"
        )) {
            if ($text.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
                throw "$file must clearly document '$required'."
            }
        }
    }
}

function Assert-ReviewBacklogUpdated {
    Assert-FileExists "docs/P10A_REVIEW_BACKLOG.md"
    $text = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "docs\P10A_REVIEW_BACKLOG.md")
    foreach ($required in @(
        "P10-B",
        "performance",
        "high-detail",
        "B-level"
    )) {
        if ($text.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "P10A_REVIEW_BACKLOG must mention '$required'."
        }
    }
}

Write-Host "P10-A preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-Branch
    Assert-P10StageCount
    Assert-NoExtraP10StageArtifacts
    Assert-ProtectedPathsClean
    Assert-ChangedFilesAllowed
    Assert-RequiredArtifacts
    Assert-HighDetailSceneAvailable
    Assert-NoBuildOrReleaseArtifacts
    Assert-ClaimBoundaries
    Assert-ReviewBacklogUpdated

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p10a_json.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P10-A JSON validation failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P10-A preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P10-A preflight: PASS" -ForegroundColor Green
exit 0
