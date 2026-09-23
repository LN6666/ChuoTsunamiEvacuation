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
    while ($normalized.StartsWith("./", [System.StringComparison]::Ordinal)) { $normalized = $normalized.Substring(2) }
    while ($normalized.StartsWith("/", [System.StringComparison]::Ordinal)) { $normalized = $normalized.Substring(1) }
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

function Assert-FileExists {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Assert-Branch {
    $branch = (& git -C $repoRoot branch --show-current).Trim()
    if ($branch -ne "p10c-pre-performance-gate-optimization") {
        throw "Expected branch p10c-pre-performance-gate-optimization, found $branch"
    }
}

function Assert-P10StageCount {
    Assert-FileExists "docs/P10_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath (Join-Path $repoRoot "docs\P10_STAGE_PLAN.md") |
            Where-Object { $_ -match "^##\s+(P10-[A-D])\s*$" } |
            ForEach-Object { $Matches[1] }
    )
    $expected = @("P10-A", "P10-B", "P10-C", "P10-D")
    foreach ($stage in $expected) {
        if ($stages -notcontains $stage) { throw "Missing official P10 stage: $stage" }
    }
    if ($stages.Count -ne 4) {
        throw "P10 must have exactly four official stages. Found: $($stages -join ', ')"
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
        "Assets/Settings",
        "Assets/Scenes/Chuo_BaseMap.unity",
        "Assets/Scenes/Chuo_BaseMap.unity.meta",
        "Assets/PLATEAU",
        $baselineScene
    ))
    if ($protectedStatus.Count -gt 0) {
        throw "Protected path dirty state detected: $($protectedStatus -join '; ')"
    }
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot ($baselineScene -replace "/", "\")) -PathType Leaf)) {
        throw "Protected high-detail scene is missing: $baselineScene"
    }
}

function Test-AllowedP10CPrePath {
    param([string]$Path)
    if ($Path -like "Assets/Scripts/P10/P10CPre*.cs") { return $true }
    if ($Path -like "Assets/Scripts/P10/P10CPre*.cs.meta") { return $true }
    if ($Path -eq "Assets/Scripts/P10/P10BGreenGroundFrameRuntime.cs") { return $true }
    if ($Path -like "Assets/Scripts/Editor/P10CPre*.cs") { return $true }
    if ($Path -like "Assets/Scripts/Editor/P10CPre*.cs.meta") { return $true }
    if ($Path -like "Assets/Tests/EditMode/P10/P10CPre*.cs") { return $true }
    if ($Path -like "Assets/Tests/EditMode/P10/P10CPre*.cs.meta") { return $true }
    if ($Path -like "Assets/Tests/PlayMode/P10/P10CPre*.cs") { return $true }
    if ($Path -like "Assets/Tests/PlayMode/P10/P10CPre*.cs.meta") { return $true }
    if ($Path -like "Assets/Data/P10/p10c_pre_*.json") { return $true }
    if ($Path -like "Assets/Data/P10/p10c_pre_*.json.meta") { return $true }
    if ($Path -like "docs/P10C_PRE_*.md") { return $true }
    if ($Path -eq "docs/P10B_PLUS_PLUS_NEXT_STEPS_TO_P10C.md") { return $true }
    if ($Path -eq "docs/P10_STAGE_PLAN.md") { return $true }
    if ($Path -eq "docs/REVIEW_BACKLOG.md") { return $true }
    if ($Path -eq "docs/TASKS.md") { return $true }
    if ($Path -like "tools/p10/*p10c_pre*.ps1") { return $true }
    if ($Path -eq "codex_prompts/p10c_pre_performance_gate_optimization.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p10c_pre.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-AllowedP10CPrePath $file) { continue }
        $violations.Add("$file is outside P10-C-Pre allowed paths") | Out-Null
    }
    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) { Write-Host "FAIL: $violation" }
        throw "P10-C-Pre changed-file scope check failed."
    }
    Write-Host "P10-C-Pre changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "Assets/Scripts/P10/P10CPrePerformanceGateConfig.cs",
        "Assets/Scripts/P10/P10CPreBuildProfileSummary.cs",
        "Assets/Scripts/P10/P10CPreRuntimePerformanceGate.cs",
        "Assets/Scripts/P10/P10CPreQualityProfileApplier.cs",
        "Assets/Scripts/P10/P10CPreStagedActivationConfig.cs",
        "Assets/Scripts/Editor/P10CPreTestBuildBuilder.cs",
        "Assets/Data/P10/p10c_pre_performance_gate_config.json",
        "Assets/Data/P10/p10c_pre_quality_profiles.json",
        "Assets/Data/P10/p10c_pre_built_player_profile_summary.json",
        "Assets/Data/P10/p10c_pre_optimization_decision.json",
        "Assets/Data/P10/p10c_pre_manual_playtest_checklist.json",
        "docs/P10C_PRE_TEST_BUILD_NOTES.md",
        "docs/P10C_PRE_BUILT_PLAYER_PROFILING_REPORT.md",
        "docs/P10C_PRE_QUALITY_PROFILE_RECOMMENDATIONS.md",
        "docs/P10C_PRE_CPU_OPTIMIZATION_GATE.md",
        "docs/P10C_PRE_MEMORY_GC_PAGING_GATE.md",
        "docs/P10C_PRE_STREAMING_DECISION.md",
        "docs/P10C_PRE_AA_VISUAL_QUALITY_GATE.md",
        "docs/P10C_PRE_USER_PERFORMANCE_PLAYTEST_GUIDE.md",
        "docs/P10C_PRE_READINESS_DECISION.md",
        "tools/p10/run_p10c_pre_preflight.ps1",
        "tools/p10/validate_p10c_pre_json.ps1",
        "tools/p10/build_p10c_pre_test_build.ps1",
        "tools/p10/run_p10c_pre_built_player_profile.ps1",
        "codex_prompts/p10c_pre_performance_gate_optimization.md",
        "deepseek_review_prompt_p10c_pre.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-NoBuildReleaseArchiveArtifacts {
    $changedFiles = @(Get-ChangedFiles)
    $forbidden = @(
        $changedFiles |
            Where-Object {
                ($_ -match "^(Build|Builds|Release|release_package|archives)/") -or
                ($_ -match "\.(exe|zip|7z|msi|rar)$")
            } |
            Sort-Object -Unique
    )
    if ($forbidden.Count -gt 0) {
        throw "P10-C-Pre must not stage build/release/archive artifacts: $($forbidden -join '; ')"
    }
}

function Assert-NoAddressablesOrPackageAdditions {
    $changedFiles = @(Get-ChangedFiles)
    $addressables = @(
        $changedFiles |
            Where-Object {
                ($_ -match "Addressable") -or
                ($_ -match "Addressables") -or
                ($_ -like "Packages/*")
            } |
            Sort-Object -Unique
    )
    if ($addressables.Count -gt 0) {
        throw "P10-C-Pre must not add Addressables or package changes: $($addressables -join '; ')"
    }
}

function Assert-ClaimBoundaries {
    $docs = @(
        "docs/P10C_PRE_TEST_BUILD_NOTES.md",
        "docs/P10C_PRE_BUILT_PLAYER_PROFILING_REPORT.md",
        "docs/P10C_PRE_STREAMING_DECISION.md",
        "docs/P10C_PRE_AA_VISUAL_QUALITY_GATE.md",
        "docs/P10C_PRE_READINESS_DECISION.md"
    )
    $text = ""
    foreach ($file in $docs) {
        $text += "`n" + (Get-Content -Raw -LiteralPath (Join-Path $repoRoot ($file -replace "/", "\")))
    }
    foreach ($required in @(
        "performance gate",
        "temporary",
        "not the final release",
        "No true production chunk streaming",
        "AA is not confirmed",
        "not official route",
        "P10-C"
    )) {
        if ($text.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "P10-C-Pre docs must clearly document '$required'."
        }
    }
}

Write-Host "P10-C-Pre preflight: starting"
Write-Host "Repo root: $repoRoot"
Assert-Branch
Assert-P10StageCount
Assert-NoExtraP10StageArtifacts
Assert-RequiredArtifacts
Assert-ChangedFilesAllowed
Assert-NoBuildReleaseArchiveArtifacts
Assert-NoAddressablesOrPackageAdditions
Assert-ProtectedPathsClean
Assert-ClaimBoundaries
& (Join-Path $scriptRoot "validate_p10c_pre_json.ps1")
Write-Host "P10-C-Pre preflight: PASS" -ForegroundColor Green
exit 0
