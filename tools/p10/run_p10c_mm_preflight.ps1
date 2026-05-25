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
    if ($branch -ne "p10c-minus-minus-extended-performance-sampling") {
        throw "Expected branch p10c-minus-minus-extended-performance-sampling, found $branch"
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

function Test-AllowedP10CMMPath {
    param([string]$Path)
    if ($Path -like "Assets/Scripts/P10/P10CMM*.cs") { return $true }
    if ($Path -like "Assets/Scripts/P10/P10CMM*.cs.meta") { return $true }
    if ($Path -like "Assets/Tests/EditMode/P10/P10CMM*.cs") { return $true }
    if ($Path -like "Assets/Tests/EditMode/P10/P10CMM*.cs.meta") { return $true }
    if ($Path -like "Assets/Tests/PlayMode/P10/P10CMM*.cs") { return $true }
    if ($Path -like "Assets/Tests/PlayMode/P10/P10CMM*.cs.meta") { return $true }
    if ($Path -like "Assets/Data/P10/p10c_mm_*.json") { return $true }
    if ($Path -like "Assets/Data/P10/p10c_mm_*.json.meta") { return $true }
    if ($Path -like "docs/P10C_MM_*.md") { return $true }
    if ($Path -eq "docs/P10_STAGE_PLAN.md") { return $true }
    if ($Path -eq "docs/P10C_PRE_READINESS_DECISION.md") { return $true }
    if ($Path -eq "docs/P10B_PLUS_PLUS_NEXT_STEPS_TO_P10C.md") { return $true }
    if ($Path -eq "docs/REVIEW_BACKLOG.md") { return $true }
    if ($Path -eq "docs/TASKS.md") { return $true }
    if ($Path -like "tools/p10/*p10c_mm*.ps1") { return $true }
    if ($Path -eq "codex_prompts/p10c_mm_extended_performance_sampling.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p10c_mm.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-AllowedP10CMMPath $file) { continue }
        $violations.Add("$file is outside P10-C-- allowed paths") | Out-Null
    }
    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) { Write-Host "FAIL: $violation" }
        throw "P10-C-- changed-file scope check failed."
    }
    Write-Host "P10-C-- changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "Assets/Scripts/P10/P10CMMFpsStutterExporter.cs",
        "Assets/Tests/EditMode/P10/P10CMMFpsStutterExporterEditModeTests.cs",
        "Assets/Tests/PlayMode/P10/P10CMMFpsStutterExporterPlayModeTests.cs",
        "Assets/Data/P10/p10c_mm_process_sampling_3min.json",
        "Assets/Data/P10/p10c_mm_process_sampling_5min.json",
        "Assets/Data/P10/p10c_mm_process_sampling_10min.json",
        "Assets/Data/P10/p10c_mm_fps_stutter_summary.json",
        "Assets/Data/P10/p10c_mm_high_detail_full_load_status.json",
        "Assets/Data/P10/p10c_mm_scenario_performance_summary.json",
        "Assets/Data/P10/p10c_mm_player_log_summary.json",
        "Assets/Data/P10/p10c_mm_paging_risk_summary.json",
        "Assets/Data/P10/p10c_mm_readiness_decision.json",
        "docs/P10C_MM_EXTENDED_PERFORMANCE_SAMPLING.md",
        "docs/P10C_MM_EXTENDED_PROCESS_SAMPLING_REPORT.md",
        "docs/P10C_MM_FPS_FRAME_STUTTER_REPORT.md",
        "docs/P10C_MM_HIGH_DETAIL_FULL_LOAD_VALIDATION.md",
        "docs/P10C_MM_SCENARIO_PERFORMANCE_REPORT.md",
        "docs/P10C_MM_PLAYER_LOG_REPORT.md",
        "docs/P10C_MM_DISK_PAGING_MEMORY_SWAP_REPORT.md",
        "docs/P10C_MM_READINESS_DECISION.md",
        "docs/P10C_MM_TEST_RESULTS.md",
        "docs/P10C_MM_NEXT_STEPS_TO_P10C.md",
        "tools/p10/run_p10c_mm_preflight.ps1",
        "tools/p10/validate_p10c_mm_json.ps1",
        "tools/p10/run_p10c_mm_extended_process_sampling.ps1",
        "tools/p10/run_p10c_mm_fps_stutter_capture.ps1",
        "tools/p10/parse_p10c_mm_player_log.ps1",
        "tools/p10/write_p10c_mm_performance_report.ps1",
        "codex_prompts/p10c_mm_extended_performance_sampling.md",
        "deepseek_review_prompt_p10c_mm.md"
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
        throw "P10-C-- must not stage build/release/archive artifacts: $($forbidden -join '; ')"
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
        throw "P10-C-- must not add Addressables or package changes: $($addressables -join '; ')"
    }
}

function Assert-ClaimBoundaries {
    $docs = @(
        "docs/P10C_MM_EXTENDED_PERFORMANCE_SAMPLING.md",
        "docs/P10C_MM_READINESS_DECISION.md",
        "docs/P10C_MM_NEXT_STEPS_TO_P10C.md"
    )
    $text = ""
    foreach ($file in $docs) {
        $text += "`n" + (Get-Content -Raw -LiteralPath (Join-Path $repoRoot ($file -replace "/", "\")))
    }
    foreach ($required in @(
        "not official P10-C",
        "not the final release",
        "No final release package",
        "No P10-E",
        "P10-C remains"
    )) {
        if ($text.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "P10-C-- docs must clearly document '$required'."
        }
    }
}

Write-Host "P10-C-- preflight: starting"
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
& (Join-Path $scriptRoot "validate_p10c_mm_json.ps1")
Write-Host "P10-C-- preflight: PASS" -ForegroundColor Green
exit 0
