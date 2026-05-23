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

function Test-AllowedP8BPath {
    param([string]$Path)

    if ($Path -like "docs/P8B_*.md") { return $true }
    if ($Path -like "tools/p8/*p8b*.ps1") { return $true }
    if (Test-PathStartsWith $Path "Assets/Scripts/P8/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/EditMode/P8/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/PlayMode/P8/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Data/P8/") { return $true }
    if ($Path -eq "codex_prompts/p8b_riskfront_validation_hardening.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p8b_guard.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p8b_integration.md") { return $true }
    return $false
}

function Assert-ProtectedPathsClean {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    $stagedBaseline = @(Get-GitLines @("-C", $repoRoot, "diff", "--cached", "--name-only", "--", $baselineScene))

    foreach ($file in $changedFiles) {
        if ($file -eq $baselineScene) {
            if ($stagedBaseline.Count -gt 0) {
                $violations.Add("$file is staged; protected P7 high-detail baseline scene must not be committed") | Out-Null
            }
            else {
                Write-Host "WARN: $file has local unstaged baseline changes and is intentionally preserved."
            }

            continue
        }

        if (Test-AllowedP8BPath $file) {
            continue
        }

        $violations.Add("$file is outside the P8-B guard allowed paths") | Out-Null
    }

    foreach ($file in $changedFiles) {
        foreach ($protectedPath in @(
            "Assets/Scenes/Chuo_BaseMap.unity"
        )) {
            if ($file -eq $protectedPath) {
                $violations.Add("$file is a protected scene and must not be changed") | Out-Null
            }
        }

        foreach ($prefix in @(
            "ProjectSettings/",
            "Packages/",
            "Assets/PLATEAU/",
            "Library/",
            "Temp/",
            "Logs/",
            "Builds/",
            "review_reports/",
            "test-results/"
        )) {
            if (Test-PathStartsWith $file $prefix) {
                $violations.Add("$file matches protected/generated prefix $prefix") | Out-Null
            }
        }
    }

    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }

        throw "P8-B protected path check failed."
    }

    Write-Host "P8-B protected path check passed for $($changedFiles.Count) changed/untracked files."
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
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    $text = Get-Content -Raw -LiteralPath $path
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
    }
}

function Assert-P8StageCount {
    Assert-FileExists "docs/P8_STAGE_PLAN.md"
    $stageText = Get-Content -LiteralPath (Join-Path $repoRoot "docs\P8_STAGE_PLAN.md")
    $stages = @(
        $stageText |
            Where-Object { $_ -match "^##\s+(P8-[A-D])\s*$" } |
            ForEach-Object { $Matches[1] }
    )

    $expected = @("P8-A", "P8-B", "P8-C", "P8-D")
    if ($stages.Count -ne 4) {
        throw "P8_STAGE_PLAN must define exactly four P8 stage headings; found $($stages.Count)."
    }

    foreach ($stage in $expected) {
        if ($stages -notcontains $stage) {
            throw "P8_STAGE_PLAN missing stage heading: $stage"
        }
    }
}

function Assert-NoSceneMutation {
    $changedFiles = @(Get-ChangedFiles)
    foreach ($file in $changedFiles) {
        if ($file -eq $baselineScene) {
            continue
        }

        if (Test-PathStartsWith $file "Assets/Scenes/") {
            throw "P8-B integration must not change scene files: $file"
        }
    }
}

function Assert-NoP9P10Systems {
    $changedFiles = @(Get-ChangedFiles)
    foreach ($file in $changedFiles) {
        if ($file -match "^(Assets/(Scripts|Tests)/(P9|P10)|tools/(p9|p10)/|docs/P(9|10))") {
            throw "P8-B guard must not add P9/P10 systems: $file"
        }
    }
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P8B_PERFORMANCE_GUARD.md",
        "docs/P8B_TO_P8C_HANDOFF_CHECKLIST.md",
        "docs/P8B_VALIDATION_HARDENING.md",
        "docs/P8B_SAFETY_GUARDRAILS.md",
        "docs/P8B_REVIEW_BACKLOG.md",
        "tools/p8/validate_p8b_riskfront_config.ps1",
        "tools/p8/run_p8b_guard_preflight.ps1",
        "tools/p8/inspect_p8b_visual_performance_risk.ps1",
        "Assets/Scripts/P8/P8RiskFrontPerformanceGuard.cs",
        "Assets/Tests/EditMode/P8/P8BGuardTests.cs",
        "Assets/Tests/PlayMode/P8/P8BRiskFrontGuardPlayModeTests.cs",
        "codex_prompts/p8b_riskfront_validation_hardening.md",
        "deepseek_review_prompt_p8b_guard.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-Docs {
    Assert-FileContains "docs/P8B_VALIDATION_HARDENING.md" @(
        "visual curtain is cinematic only",
        "tsunamiHeightMeters",
        "waterLevelMeters",
        "visualHeightMeters",
        "boundaryIsEvidenceBasedOrPrototype",
        "no P9 systems"
    )

    Assert-FileContains "docs/P8B_SAFETY_GUARDRAILS.md" @(
        "no full fluid simulation",
        "no gameplay success/failure changes",
        "no P9 systems",
        "no P10 systems"
    )

    Assert-FileContains "docs/P8B_PERFORMANCE_GUARD.md" @(
        "excessive segment counts",
        "per-frame mesh rebuild",
        "transparency overdraw",
        "light curtain objects",
        "unbounded particle usage",
        "material/shader risk",
        "culling"
    )

    Assert-FileContains "docs/P8B_TO_P8C_HANDOFF_CHECKLIST.md" @(
        "hazard layer loaded",
        "risk front visual exists or is ready",
        "affected infrastructure types are defined",
        "road/building/bridge/underground interactions are still not implemented",
        "must not assume official hazard values"
    )
}

Write-Host "P8-B guard preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-P8StageCount
    Assert-ProtectedPathsClean
    Assert-NoSceneMutation
    Assert-NoP9P10Systems
    Assert-RequiredArtifacts
    Assert-Docs

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p8b_riskfront_config.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8-B risk-front config validation failed."
    }

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "inspect_p8b_visual_performance_risk.ps1") -TreatWarningsAsErrors
    if ($LASTEXITCODE -ne 0) {
        throw "P8-B visual performance risk inspection failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-B guard preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-B guard preflight: PASS" -ForegroundColor Green
exit 0
