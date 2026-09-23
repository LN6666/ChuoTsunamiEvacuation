[CmdletBinding()]
param(
    [string]$PlateauRoot = "D:\PLATEAU_DATA\Chuo_2025_CityGML"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$scopeGuardScript = Join-Path $scriptRoot "check_p7_scope.ps1"
$inspectScript = Join-Path $scriptRoot "inspect_p7b_lod3_candidate.ps1"

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

function Invoke-Step {
    param(
        [string]$Name,
        [scriptblock]$ScriptBlock
    )

    Write-Host ""
    Write-Host "P7-B Wave 2-B preflight: $Name"
    & $ScriptBlock
    Write-Host "P7-B Wave 2-B preflight: $Name PASS"
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

function Assert-AllowedWave2BFiles {
    $allowedPaths = @(
        "docs/P7B_WAVE2B_LOD3_CANDIDATE_DRYRUN.md",
        "docs/P7B_WAVE2B_IMPORT_DECISION.md",
        "docs/P7B_WAVE2B_VISUAL_FEASIBILITY_PLAN.md",
        "docs/P7B_WAVE2B_TEST_RESULTS.md",
        "docs/P7B_WAVE2B_KNOWN_LIMITATIONS.md",
        "tools/p7/inspect_p7b_lod3_candidate.ps1",
        "tools/p7/run_p7b_wave2b_preflight.ps1",
        "codex_prompts/p7b_wave2b_lod3_candidate_dryrun.md",
        "deepseek_review_prompt_p7b_wave2b.md",
        "docs/TASKS.md",
        "docs/REVIEW_BACKLOG.md",
        "docs/P7_DECISION_LOG.md",
        "tools/p7/check_p7_scope.ps1"
    )

    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]

    foreach ($file in $changedFiles) {
        if ($allowedPaths -notcontains $file) {
            $violations.Add("$file is outside the Wave 2-B dry-run allowlist") | Out-Null
        }
    }

    if ($violations.Count -gt 0) {
        foreach ($violation in $violations) {
            Write-Host "FAIL: $violation"
        }

        throw "Wave 2-B allowlist check failed."
    }

    Write-Host "Wave 2-B allowlist check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-ProtectedPathsClean {
    $changedFiles = @(Get-ChangedFiles)
    $protectedPrefixes = @(
        "ProjectSettings/",
        "Packages/",
        "Assets/Data/",
        "Assets/PLATEAU/",
        "Assets/Scripts/",
        "Assets/Editor/",
        "Assets/Tests/",
        "Assets/Scenes/"
    )

    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        foreach ($prefix in $protectedPrefixes) {
            if (Test-PathStartsWith -Path $file -Prefix $prefix) {
                $violations.Add("$file matches protected prefix $prefix") | Out-Null
            }
        }

        if ($file.Equals("Assets/Scenes/Chuo_BaseMap.unity", [System.StringComparison]::OrdinalIgnoreCase)) {
            $violations.Add("$file matches protected Chuo_BaseMap path") | Out-Null
        }
    }

    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }

        throw "Protected path check failed."
    }

    Write-Host "Protected path check passed: Chuo_BaseMap, ProjectSettings, Packages, Assets/Data, Assets/PLATEAU, Unity scripts/tests/editor files, and scenes are untouched."
}

function Invoke-CandidateInspection {
    $jsonLines = @(& powershell -ExecutionPolicy Bypass -File $inspectScript -OutputFormat Json -PlateauRoot $PlateauRoot)
    if ($LASTEXITCODE -ne 0) {
        throw "LOD3 candidate inspection failed."
    }

    $inspection = ($jsonLines -join [Environment]::NewLine) | ConvertFrom-Json

    if (-not $inspection.plateauRootExists) {
        throw "PLATEAU root was not found: $PlateauRoot"
    }

    if (-not $inspection.udxRootExists) {
        throw "PLATEAU udx root was not found under: $PlateauRoot"
    }

    if (-not $inspection.dryRunOnly -or -not $inspection.noAssetImportPerformed -or -not $inspection.noAssetCopyPerformed) {
        throw "Inspection result did not preserve dry-run/no-import/no-copy flags."
    }

    $requiredCandidates = @("53393690", "53393672", "53394611")
    foreach ($meshCode in $requiredCandidates) {
        $candidate = @($inspection.candidates | Where-Object { $_.meshCode -eq $meshCode } | Select-Object -First 1)
        if ($candidate.Count -eq 0) {
            throw "Candidate $meshCode is missing from inspection output."
        }

        if ([int]$candidate[0].files -le 0) {
            throw "Candidate $meshCode has no discovered files."
        }
    }

    if ([int]$inspection.lod4PathHitsFoundUnderUdx -ne 0) {
        throw "LOD4 path/name hits were found under udx. Update Wave 2-B docs before continuing."
    }

    foreach ($candidate in $inspection.candidates) {
        Write-Host ("Candidate {0}: {1} files, {2}, LOD3 path hits {3}, LOD4 path hits {4}, estimate {5}" -f `
            $candidate.meshCode,
            $candidate.files,
            $candidate.size,
            $candidate.lod3PathHits,
            $candidate.lod4PathHits,
            $candidate.futureImportEstimate.level)
    }

    Write-Host "Candidate inspection confirms dry-run only: no import and no copy."
}

Write-Host "P7-B Wave 2-B preflight: starting"
Write-Host "P7-B Wave 2-B preflight: repo root $repoRoot"
Write-Host "P7-B Wave 2-B preflight: PLATEAU root $PlateauRoot"

$failed = $false
try {
    Invoke-Step "dry-run candidate inspection" { Invoke-CandidateInspection }

    Invoke-Step "P7 scope guard" {
        & powershell -ExecutionPolicy Bypass -File $scopeGuardScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7 scope guard failed."
        }
    }

    Invoke-Step "Wave 2-B dry-run allowlist" { Assert-AllowedWave2BFiles }

    Invoke-Step "protected path check" { Assert-ProtectedPathsClean }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P7-B Wave 2-B preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P7-B Wave 2-B preflight: PASS" -ForegroundColor Green
exit 0
