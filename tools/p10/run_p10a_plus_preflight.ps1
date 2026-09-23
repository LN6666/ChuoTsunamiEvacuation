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
    if ($branch -ne "p10a-plus-final-gap-hardening") {
        throw "Expected branch p10a-plus-final-gap-hardening, found $branch"
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

function Test-AllowedP10APlusPath {
    param([string]$Path)
    if ($Path -like "Assets/Data/P10/p10a_plus_*.json") { return $true }
    if ($Path -like "Assets/Data/P10/p10a_plus_*.json.meta") { return $true }
    if ($Path -eq "Assets/Data/P10/p10a_p10b_readiness_checklist.json") { return $true }
    if ($Path -like "docs/P10A_PLUS_*.md") { return $true }
    if ($Path -eq "docs/P10A_REMAINING_GAP_CLOSURE.md") { return $true }
    if ($Path -eq "docs/P10A_P10B_READINESS.md") { return $true }
    if ($Path -eq "docs/P10A_KNOWN_LIMITATIONS.md") { return $true }
    if ($Path -eq "docs/REVIEW_BACKLOG.md") { return $true }
    if ($Path -eq "docs/TASKS.md") { return $true }
    if ($Path -eq "tools/p10/run_p10a_plus_preflight.ps1") { return $true }
    if ($Path -eq "tools/p10/validate_p10a_plus_json.ps1") { return $true }
    if ($Path -eq "tools/p10/write_p10a_plus_hardening_report.ps1") { return $true }
    if ($Path -eq "codex_prompts/p10a_plus_final_gap_hardening.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p10a_plus.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-AllowedP10APlusPath $file) { continue }
        $violations.Add("$file is outside P10-A+ allowed paths") | Out-Null
    }
    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }
        throw "P10-A+ changed-file scope check failed."
    }
    Write-Host "P10-A+ changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P10A_PLUS_FINAL_GAP_HARDENING.md",
        "docs/P10A_PLUS_CANDIDATE_ANCHORING_HARDENING.md",
        "docs/P10A_PLUS_CANDIDATE_TO_BUILDING_NEAREST_MATCH.md",
        "docs/P10A_PLUS_ENTRANCE_PROXY_HARDENING.md",
        "docs/P10A_PLUS_ROUTE_PROXY_VALIDATION.md",
        "docs/P10A_PLUS_PLATEAU_SEMANTIC_BINDING_AUDIT.md",
        "docs/P10A_PLUS_LIGHT_CURTAIN_VISUAL_QA.md",
        "docs/P10A_PLUS_RESULT_PANEL_VISUAL_QA.md",
        "docs/P10A_PLUS_HIGH_DETAIL_SMOKE_CHECKLIST.md",
        "docs/P10A_PLUS_HARDENING_MATRIX.md",
        "docs/P10A_PLUS_TEST_RESULTS.md",
        "docs/P10A_PLUS_KNOWN_LIMITATIONS.md",
        "docs/P10A_PLUS_NEXT_STEPS_TO_P10B.md",
        "Assets/Data/P10/p10a_plus_hardening_matrix.json",
        "Assets/Data/P10/p10a_plus_candidate_anchor_hardening_report.json",
        "Assets/Data/P10/p10a_plus_candidate_to_building_nearest_match_report.json",
        "Assets/Data/P10/p10a_plus_entrance_proxy_hardening_report.json",
        "Assets/Data/P10/p10a_plus_route_proxy_validation_report.json",
        "Assets/Data/P10/p10a_plus_plateau_semantic_binding_audit.json",
        "Assets/Data/P10/p10a_plus_high_detail_smoke_status.json",
        "tools/p10/run_p10a_plus_preflight.ps1",
        "tools/p10/validate_p10a_plus_json.ps1",
        "tools/p10/write_p10a_plus_hardening_report.ps1",
        "codex_prompts/p10a_plus_final_gap_hardening.md",
        "deepseek_review_prompt_p10a_plus.md"
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
                (Test-PathStartsWith -Path $_ -Prefix "run_logs/")
            } |
            Sort-Object -Unique
    )
    if ($forbidden.Count -gt 0) {
        throw "P10-A+ must not add build/release/archive artifacts: $($forbidden -join '; ')"
    }
}

function Assert-ClaimBoundaries {
    $p10JsonText = @(
        Get-ChildItem -LiteralPath (Join-Path $repoRoot "Assets\Data\P10") -Filter "p10a_plus_*.json" |
            ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }
    ) -join "`n"
    $p10JsonText += "`n" + (Get-Content -Raw -LiteralPath (Join-Path $repoRoot "Assets\Data\P10\p10a_p10b_readiness_checklist.json"))

    foreach ($forbiddenClaim in @(
        '"noP10EfgCreated": false',
        '"noNewLargeGameplaySystem": false',
        '"noP7P8P9Reimplementation": false',
        '"noP10CArchiveOrReleaseWork": false',
        '"isOfficialShelter": true',
        '"safeApprovedByDefault": true',
        '"isOfficialEvacuationRoute": true',
        '"routeRoadGeometryValidated": true',
        '"exactPlateauUnityObjectIdentityProven": true',
        '"fullSceneSemanticCoverageProven": true',
        '"sceneMutationPerformed": true',
        '"p10bBuildStarted": true',
        '"p10cReleasePackagingStarted": true',
        '"p10cArchiveCompleted": true'
    )) {
        if ($p10JsonText.Contains($forbiddenClaim)) {
            throw "P10-A+ data contains forbidden claim/state: $forbiddenClaim"
        }
    }

    foreach ($file in @(
        "docs/P10A_PLUS_CANDIDATE_TO_BUILDING_NEAREST_MATCH.md",
        "docs/P10A_PLUS_ROUTE_PROXY_VALIDATION.md",
        "docs/P10A_PLUS_PLATEAU_SEMANTIC_BINDING_AUDIT.md",
        "docs/P10A_PLUS_FINAL_GAP_HARDENING.md"
    )) {
        $text = Get-Content -Raw -LiteralPath (Join-Path $repoRoot ($file -replace "/", "\"))
        foreach ($required in @(
            "proxy",
            "nearest-match",
            "not official",
            "exact PLATEAU"
        )) {
            if ($text.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
                throw "$file must clearly document '$required'."
            }
        }
    }
}

function Assert-NoP7P8P9RuntimeSourceChanges {
    $changedFiles = @(Get-ChangedFiles)
    $sourceChanges = @(
        $changedFiles |
            Where-Object {
                (Test-PathStartsWith -Path $_ -Prefix "Assets/Scripts/P7") -or
                (Test-PathStartsWith -Path $_ -Prefix "Assets/Scripts/P8") -or
                (Test-PathStartsWith -Path $_ -Prefix "Assets/Scripts/P9") -or
                (Test-PathStartsWith -Path $_ -Prefix "Assets/Tests/")
            }
    )
    if ($sourceChanges.Count -gt 0) {
        throw "P10-A+ must not add/rewrite P7/P8/P9 runtime/test systems: $($sourceChanges -join '; ')"
    }
}

Write-Host "P10-A+ preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-Branch
    Assert-P10StageCount
    Assert-NoExtraP10StageArtifacts
    Assert-ProtectedPathsClean
    Assert-ChangedFilesAllowed
    Assert-RequiredArtifacts
    Assert-NoBuildOrReleaseArtifacts
    Assert-NoP7P8P9RuntimeSourceChanges
    Assert-ClaimBoundaries

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p10a_plus_json.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P10-A+ JSON validation failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P10-A+ preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P10-A+ preflight: PASS" -ForegroundColor Green
exit 0
