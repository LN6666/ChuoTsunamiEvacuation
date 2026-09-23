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
    if ($branch -ne "p10b-plus-plus-final-optimization") {
        throw "Expected branch p10b-plus-plus-final-optimization, found $branch"
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
        "Assets/PLATEAU",
        $baselineScene
    ))
    if ($protectedStatus.Count -gt 0) {
        throw "Protected path dirty state detected: $($protectedStatus -join '; ')"
    }
}

function Test-AllowedP10BPlusPlusPath {
    param([string]$Path)
    if ($Path -like "Assets/Scripts/P10/P10B*") { return $true }
    if ($Path -like "Assets/Scripts/P10/P10B*.meta") { return $true }
    if ($Path -like "Assets/Tests/EditMode/P10/P10B*") { return $true }
    if ($Path -like "Assets/Tests/PlayMode/P10/P10B*") { return $true }
    if ($Path -like "Assets/Data/P10/p10b_*.json") { return $true }
    if ($Path -like "Assets/Data/P10/p10b_*.json.meta") { return $true }
    if ($Path -like "docs/P10B_*.md") { return $true }
    if ($Path -eq "docs/P10_STAGE_PLAN.md") { return $true }
    if ($Path -eq "docs/P10B_NEXT_STEPS_TO_P10C.md") { return $true }
    if ($Path -eq "docs/P10B_PLUS_NEXT_STEPS_TO_P10C.md") { return $true }
    if ($Path -eq "docs/P10B_MANUAL_PLAYTEST_CHECKLIST.md") { return $true }
    if ($Path -eq "docs/REVIEW_BACKLOG.md") { return $true }
    if ($Path -eq "docs/TASKS.md") { return $true }
    if ($Path -eq "tools/p10/run_p10b_plus_plus_preflight.ps1") { return $true }
    if ($Path -eq "tools/p10/validate_p10b_plus_plus_json.ps1") { return $true }
    if ($Path -like "tools/p10/audit_p10b_plus_plus_*.ps1") { return $true }
    if ($Path -eq "tools/p10/write_p10b_plus_plus_optimization_report.ps1") { return $true }
    if ($Path -eq "codex_prompts/p10b_plus_plus_final_optimization.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p10b_plus_plus.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-AllowedP10BPlusPlusPath $file) { continue }
        $violations.Add("$file is outside P10-B++ allowed paths") | Out-Null
    }
    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) { Write-Host "FAIL: $violation" }
        throw "P10-B++ changed-file scope check failed."
    }
    Write-Host "P10-B++ changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "Assets/Scripts/P10/P10BPlusPlusOptimizationConfig.cs",
        "Assets/Scripts/P10/P10BPlusPlusFrameSpikeDetector.cs",
        "Assets/Scripts/P10/P10BPlusPlusMetricsRingBuffer.cs",
        "Assets/Scripts/P10/P10BPlusPlusRuntimeOptimizer.cs",
        "Assets/Scripts/P10/P10BPlusPlusQualityPresetAdvisor.cs",
        "Assets/Data/P10/p10b_plus_plus_optimization_config.json",
        "Assets/Data/P10/p10b_plus_plus_performance_audit_sample.json",
        "Assets/Data/P10/p10b_plus_plus_quality_recommendations.json",
        "docs/P10B_PLUS_PLUS_FINAL_OPTIMIZATION_ATTEMPT.md",
        "docs/P10B_PLUS_PLUS_STREAMING_AND_LOADING_AUDIT.md",
        "docs/P10B_PLUS_PLUS_ANTI_ALIASING_AND_QUALITY_AUDIT.md",
        "docs/P10B_PLUS_PLUS_CPU_OPTIMIZATION_AUDIT.md",
        "docs/P10B_PLUS_PLUS_MEMORY_GC_OPTIMIZATION_AUDIT.md",
        "docs/P10B_PLUS_PLUS_STUTTER_REDUCTION_PLAN.md",
        "docs/P10B_PLUS_PLUS_DISK_PAGING_RISK_CHECKLIST.md",
        "docs/P10B_PLUS_PLUS_LOD_CULLING_BATCHING_QA.md",
        "docs/P10B_PLUS_PLUS_MANUAL_PROFILER_CHECKLIST.md",
        "docs/P10B_PLUS_PLUS_OPTIMIZATION_REPORT.md",
        "docs/P10B_PLUS_PLUS_TEST_RESULTS.md",
        "docs/P10B_PLUS_PLUS_NEXT_STEPS_TO_P10C.md",
        "tools/p10/run_p10b_plus_plus_preflight.ps1",
        "tools/p10/validate_p10b_plus_plus_json.ps1",
        "codex_prompts/p10b_plus_plus_final_optimization.md",
        "deepseek_review_prompt_p10b_plus_plus.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-NoBuildReleaseArchiveArtifacts {
    $changedFiles = @(Get-ChangedFiles)
    $forbidden = @(
        $changedFiles |
            Where-Object {
                (Test-PathStartsWith -Path $_ -Prefix "Builds/") -or
                (Test-PathStartsWith -Path $_ -Prefix "Release/") -or
                (Test-PathStartsWith -Path $_ -Prefix "release_package/") -or
                (Test-PathStartsWith -Path $_ -Prefix "archives/") -or
                (Test-PathStartsWith -Path $_ -Prefix "run_logs/") -or
                ($_ -match "\.(exe|zip|7z|msi)$")
            } |
            Sort-Object -Unique
    )
    if ($forbidden.Count -gt 0) {
        throw "P10-B++ must not add build/release/archive artifacts: $($forbidden -join '; ')"
    }
}

function Assert-NoAddressablesOrPackageAdditions {
    $changedFiles = @(Get-ChangedFiles)
    $addressables = @(
        $changedFiles |
            Where-Object {
                ($_ -match "Addressable") -or
                ($_ -match "Addressables") -or
                (Test-PathStartsWith -Path $_ -Prefix "Packages/")
            } |
            Sort-Object -Unique
    )
    if ($addressables.Count -gt 0) {
        throw "P10-B++ must not add Addressables or package changes: $($addressables -join '; ')"
    }
}

function Assert-ClaimBoundaries {
    $requiredDocs = @(
        "docs/P10B_PLUS_PLUS_STREAMING_AND_LOADING_AUDIT.md",
        "docs/P10B_PLUS_PLUS_ANTI_ALIASING_AND_QUALITY_AUDIT.md",
        "docs/P10B_PLUS_PLUS_NEXT_STEPS_TO_P10C.md",
        "docs/P10B_PLUS_PLUS_FINAL_OPTIMIZATION_ATTEMPT.md"
    )
    $text = ""
    foreach ($file in $requiredDocs) {
        $text += "`n" + (Get-Content -Raw -LiteralPath (Join-Path $repoRoot ($file -replace "/", "\")))
    }
    foreach ($required in @(
        "not a new official",
        "No Addressables",
        "not implemented",
        "does not claim",
        "P10-C",
        "disk paging"
    )) {
        if ($text.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "P10-B++ docs must clearly document '$required'."
        }
    }
    $routeText = (Get-Content -Raw -LiteralPath (Join-Path $repoRoot "docs\P10B_MANUAL_PLAYTEST_CHECKLIST.md"))
    if ($routeText.IndexOf("not displayed as official shelters", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "P10-B++ must preserve non-official humanitarian candidate wording."
    }
}

Write-Host "P10-B++ preflight: starting"
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
& (Join-Path $scriptRoot "validate_p10b_plus_plus_json.ps1")
Write-Host "P10-B++ preflight: PASS" -ForegroundColor Green
exit 0
