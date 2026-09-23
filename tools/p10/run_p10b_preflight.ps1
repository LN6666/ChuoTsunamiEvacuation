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
    if ($branch -ne "p10b-runtime-smoke-profiling-optimization") {
        throw "Expected branch p10b-runtime-smoke-profiling-optimization, found $branch"
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
        if ($stages -notcontains $stage) { throw "Missing P10 stage: $stage" }
    }
    if ($stages.Count -ne 4 -or $unexpected.Count -gt 0) {
        throw "P10 must have exactly four official stages: P10-A, P10-B, P10-C, P10-D. Found: $($stages -join ', ')"
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

function Test-AllowedP10BPath {
    param([string]$Path)
    if ($Path -eq "Assets/Scripts/P10.meta") { return $true }
    if ($Path -like "Assets/Scripts/P10/*") { return $true }
    if ($Path -eq "Assets/Tests/EditMode/P10.meta") { return $true }
    if ($Path -eq "Assets/Tests/PlayMode/P10.meta") { return $true }
    if ($Path -like "Assets/Tests/EditMode/P10/*") { return $true }
    if ($Path -like "Assets/Tests/PlayMode/P10/*") { return $true }
    if ($Path -like "Assets/Data/P10/p10b_*.json") { return $true }
    if ($Path -like "Assets/Data/P10/p10b_*.json.meta") { return $true }
    if ($Path -like "docs/P10B_*.md") { return $true }
    if ($Path -eq "docs/P10_STAGE_PLAN.md") { return $true }
    if ($Path -eq "docs/P10A_PLUS_NEXT_STEPS_TO_P10B.md") { return $true }
    if ($Path -eq "docs/REVIEW_BACKLOG.md") { return $true }
    if ($Path -eq "docs/TASKS.md") { return $true }
    if ($Path -eq "tools/p10/run_p10b_preflight.ps1") { return $true }
    if ($Path -eq "tools/p10/validate_p10b_json.ps1") { return $true }
    if ($Path -eq "codex_prompts/p10b_runtime_smoke_profiling_optimization.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p10b.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-AllowedP10BPath $file) { continue }
        $violations.Add("$file is outside P10-B allowed paths") | Out-Null
    }
    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }
        throw "P10-B changed-file scope check failed."
    }
    Write-Host "P10-B changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "Assets/Scripts/P10/P10BGreenGroundFrameConfig.cs",
        "Assets/Scripts/P10/P10BGreenGroundFramePool.cs",
        "Assets/Scripts/P10/P10BGreenGroundFrameGenerator.cs",
        "Assets/Scripts/P10/P10BGreenGroundFrameRuntime.cs",
        "Assets/Scripts/P10/P10BPerformanceMetricsRecorder.cs",
        "Assets/Scripts/P10/P10BStressScenarioConfig.cs",
        "Assets/Data/P10/p10b_green_ground_frame_config.json",
        "Assets/Data/P10/p10b_green_ground_frame_targets_sample.json",
        "Assets/Data/P10/p10b_performance_metrics_config.json",
        "Assets/Data/P10/p10b_stress_scenarios.json",
        "Assets/Data/P10/p10b_quality_presets.json",
        "Assets/Data/P10/p10b_profiling_report_sample.json",
        "Assets/Data/P10/p10b_manual_playtest_checklist.json",
        "docs/P10B_RUNTIME_SMOKE_AND_MANUAL_PLAYTEST.md",
        "docs/P10B_GREEN_GROUND_FRAME_MARKERS.md",
        "docs/P10B_PERFORMANCE_PROFILING.md",
        "docs/P10B_STRESS_TEST_SCENARIOS.md",
        "docs/P10B_OPTIMIZATION_PASS.md",
        "docs/P10B_RESULT_PANEL_VISUAL_SMOKE.md",
        "docs/P10B_LIGHT_CURTAIN_VISUAL_SMOKE.md",
        "docs/P10B_MANUAL_PLAYTEST_CHECKLIST.md",
        "docs/P10B_TEST_RESULTS.md",
        "docs/P10B_KNOWN_ISSUES_FOR_USER_PLAYTEST.md",
        "docs/P10B_NEXT_STEPS_TO_P10C.md",
        "tools/p10/run_p10b_preflight.ps1",
        "tools/p10/validate_p10b_json.ps1",
        "codex_prompts/p10b_runtime_smoke_profiling_optimization.md",
        "deepseek_review_prompt_p10b.md"
    )) {
        Assert-FileExists $file
    }
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
                (Test-PathStartsWith -Path $_ -Prefix "run_logs/") -or
                ($_ -match "\.(exe|zip|7z|msi)$")
            } |
            Sort-Object -Unique
    )
    if ($forbidden.Count -gt 0) {
        throw "P10-B must not add final build/release/archive artifacts: $($forbidden -join '; ')"
    }
}

function Assert-ClaimBoundaries {
    $docs = @(
        "docs/P10B_GREEN_GROUND_FRAME_MARKERS.md",
        "docs/P10B_RUNTIME_SMOKE_AND_MANUAL_PLAYTEST.md",
        "docs/P10B_NEXT_STEPS_TO_P10C.md",
        "docs/P10_STAGE_PLAN.md"
    )
    $text = ""
    foreach ($file in $docs) {
        $text += "`n" + (Get-Content -Raw -LiteralPath (Join-Path $repoRoot ($file -replace "/", "\")))
    }
    foreach ($required in @(
        "Windows EXE",
        "deferred to P10-C",
        "non-official",
        "warning",
        "not official",
        "coordinate-derived",
        "proxy"
    )) {
        if ($text.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "P10-B docs must clearly document '$required'."
        }
    }

    $routeDocs = (Get-Content -Raw -LiteralPath (Join-Path $repoRoot "docs\P10B_KNOWN_ISSUES_FOR_USER_PLAYTEST.md"))
    if ($routeDocs.IndexOf("not official evacuation routes", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "P10-B docs must preserve P5 route non-official wording."
    }
}

function Assert-NoP7P8P9RuntimeSourceChanges {
    $changedFiles = @(Get-ChangedFiles)
    $sourceChanges = @(
        $changedFiles |
            Where-Object {
                (Test-PathStartsWith -Path $_ -Prefix "Assets/Scripts/P7") -or
                (Test-PathStartsWith -Path $_ -Prefix "Assets/Scripts/P8") -or
                (Test-PathStartsWith -Path $_ -Prefix "Assets/Scripts/P9")
            }
    )
    if ($sourceChanges.Count -gt 0) {
        throw "P10-B must not reimplement P7/P8/P9 runtime systems: $($sourceChanges -join '; ')"
    }
}

Write-Host "P10-B preflight: starting"
Write-Host "Repo root: $repoRoot"
Assert-Branch
Assert-P10StageCount
Assert-NoExtraP10StageArtifacts
Assert-RequiredArtifacts
Assert-ChangedFilesAllowed
Assert-NoBuildOrReleaseArtifacts
Assert-ProtectedPathsClean
Assert-ClaimBoundaries
Assert-NoP7P8P9RuntimeSourceChanges
& (Join-Path $scriptRoot "validate_p10b_json.ps1")
Write-Host "P10-B preflight: PASS" -ForegroundColor Green
exit 0
