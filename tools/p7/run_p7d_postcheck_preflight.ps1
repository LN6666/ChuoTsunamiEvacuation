[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

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
        $output = & git @GitArgs 2>$null
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($null -eq $output) {
        return @()
    }

    return @($output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Test-PathStartsWith {
    param(
        [string]$Path,
        [string]$Prefix
    )

    return $Path.StartsWith($Prefix, [System.StringComparison]::OrdinalIgnoreCase)
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

function Assert-FileContains {
    param(
        [string]$RelativePath,
        [string[]]$Fragments
    )

    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }

    $text = Get-Content -Raw -LiteralPath $path
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
    }
}

function Assert-ProtectedPathsClean {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]

    foreach ($file in $changedFiles) {
        if ($file -eq "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity") {
            continue
        }

        foreach ($prefix in @("ProjectSettings/", "Packages/", "Assets/Data/", "Assets/PLATEAU/", "Library/", "Temp/", "Obj/", "Builds/", "logs/", "review_reports/", "test-results/")) {
            if (Test-PathStartsWith -Path $file -Prefix $prefix) {
                $violations.Add("$file matches protected/generated prefix $prefix") | Out-Null
            }
        }

        if (Test-PathStartsWith -Path $file -Prefix "Assets/Scenes/Chuo_BaseMap.unity") {
            $violations.Add("$file matches protected Chuo_BaseMap path") | Out-Null
        }

        if (Test-PathStartsWith -Path $file -Prefix "Assets/Scripts/" -and -not (Test-PathStartsWith -Path $file -Prefix "Assets/Scripts/P7Benchmark/")) {
            $violations.Add("$file changes gameplay script outside P7Benchmark") | Out-Null
        }
    }

    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }

        throw "Protected/generated path check failed."
    }

    Write-Host "Protected/generated path check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-PostcheckDocs {
    Assert-FileContains "docs/P7D_NEW_MAP_BASELINE_DECISION.md" @(
        "USER-APPROVED PRACTICAL HIGH-DETAIL BASELINE",
        "known limitations",
        "not claim average LOD3"
    )

    Assert-FileContains "docs/P7D_LOD_COVERAGE_FINAL_REPORT.md" @(
        "Average LOD3 achieved: FALSE",
        "practical high-detail baseline",
        "known limitation"
    )

    Assert-FileContains "docs/P7_FINAL_CLOSEOUT.md" @(
        "P7 final status: PASS",
        "P7 has exactly five stages",
        "No false average LOD3"
    )

    Assert-FileContains "docs/P7D_P8_P9_HANDOFF.md" @(
        "user-approved practical high-detail baseline",
        "proxy colliders",
        "representative interior templates"
    )

    Assert-FileContains "docs/P7D_WINDOWS_EXE_PROFILING_REPORT.md" @(
        "PREPARED, NOT RUN",
        "No EXE metrics are fabricated",
        "P10 release"
    )

    Assert-FileContains "docs/P7D_ASSET_ARCHIVE_AND_RECOVERY_PLAN.md" @(
        "cloud PC may be deleted",
        "cloud drive",
        "do not blindly commit"
    )

    Assert-FileContains "docs/P7_DECISION_LOG.md" @(
        "P7-D Practical Baseline Approval",
        "limitations, not blockers",
        "No false average LOD3"
    )
}

function Assert-P7StageCount {
    Assert-FileContains "docs/TASKS.md" @(
        "P7 has exactly five stages",
        "P7-0, P7-A, P7-B, P7-C, and P7-D"
    )
}

function Assert-SceneState {
    $p7Scene = Join-Path $repoRoot "Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity"
    if (-not (Test-Path -LiteralPath $p7Scene -PathType Leaf)) {
        throw "Missing accepted baseline scene: Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
    }

    $chuoStatus = Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", "Assets/Scenes/Chuo_BaseMap.unity")
    if ($chuoStatus.Count -gt 0) {
        throw "Chuo_BaseMap has git status changes: $($chuoStatus -join '; ')"
    }

    Write-Host "Scene state passed: P7_HighDetail_Chuo exists; Chuo_BaseMap has no git changes."
}

Write-Host "P7-D final postcheck preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-P7StageCount
    Assert-SceneState
    Assert-ProtectedPathsClean
    Assert-PostcheckDocs
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P7-D final postcheck preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P7-D final postcheck preflight: PASS" -ForegroundColor Green
exit 0
