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
        $output = & git -c filter.lfs.clean= -c filter.lfs.smudge= -c filter.lfs.required=false @GitArgs 2>$null
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }
    if ($null -eq $output) { return @() }
    return @($output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Assert-FileExists {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Read-Json {
    param([string]$RelativePath)
    Assert-FileExists $RelativePath
    return Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\")) | ConvertFrom-Json
}

function Assert-P8StageCount {
    Assert-FileExists "docs/P8_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath (Join-Path $repoRoot "docs\P8_STAGE_PLAN.md") |
            Where-Object { $_ -match "^##\s+(P8-[A-Z0-9]+)\s*$" } |
            ForEach-Object { $Matches[1] }
    )
    $expected = @("P8-A", "P8-B", "P8-C", "P8-D", "P8-E")
    $unexpected = @($stages | Where-Object { $expected -notcontains $_ })
    if ($stages.Count -ne 5 -or $unexpected.Count -gt 0) {
        throw "P8 stage headings must be exactly P8-A through P8-E. Found: $($stages -join ', ')"
    }

    $forbidden = @(
        Get-GitLines @("-C", $repoRoot, "ls-files") |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { $_ -match "(^|[\\/_.-])p8[-_]?(0|f|g)($|[\\/_.-])" }
    )
    if ($forbidden.Count -gt 0) {
        throw "Forbidden P8-0/F/G artifact path found: $($forbidden -join '; ')"
    }
}

function Assert-ProtectedPaths {
    $protectedStatus = @(Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", "ProjectSettings", "Packages", "Assets/Scenes/Chuo_BaseMap.unity", "Assets/PLATEAU"))
    if ($protectedStatus.Count -gt 0) {
        throw "Protected path dirty state detected: $($protectedStatus -join '; ')"
    }

    $stagedBaseline = @(Get-GitLines @("-C", $repoRoot, "diff", "--cached", "--name-only", "--", $baselineScene))
    if ($stagedBaseline.Count -gt 0) {
        throw "P7 high-detail baseline scene is staged."
    }

    $baselinePath = Join-Path $repoRoot ($baselineScene -replace "/", "\")
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw "Missing P7 high-detail baseline scene: $baselineScene"
    }
}

function Assert-NoOutOfScopeChangedFiles {
    $changed = @(
        @(Get-GitLines @("-C", $repoRoot, "diff", "--name-only", "HEAD", "--")) +
        @(Get-GitLines @("-C", $repoRoot, "ls-files", "--others", "--exclude-standard")) |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { $_ }
    )
    foreach ($file in $changed) {
        if ($file -eq $baselineScene) { continue }
        if ($file -match "^(Assets/(Scripts|Tests)/(P9|P10)|tools/(p9|p10)/|docs/P(9|10))") {
            throw "Audit must not add P9/P10 systems: $file"
        }
        if ($file -match "^(Assets/Scripts/P8/.*(Collapse|Damage).*|Assets/Tests/.*/P8/.*Collapse.*)") {
            throw "Audit must not implement P8-D collapse/damage systems: $file"
        }
    }
}

function Assert-CandidateAudit {
    Assert-FileExists "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json"
    Assert-FileExists "docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md"
    Assert-FileExists "docs/P8C_HUMANITARIAN_CANDIDATE_HANDOFF.md"
    Assert-FileExists "docs/P8D_HUMANITARIAN_CANDIDATE_DAMAGE_STATUS_SCOPE.md"
    Assert-FileExists "docs/P8E_HUMANITARIAN_CANDIDATE_PERSISTENT_VISIBILITY_SCOPE.md"

    $audit = Read-Json "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json"
    $records = @($audit.records)
    if ($records.Count -le 5) {
        throw "Candidate audit must be expanded beyond the original five samples."
    }
    if ([bool]$audit.isOfficialShelterDataset -ne $false -or [bool]$audit.nonOfficialWarningRequired -ne $true) {
        throw "Candidate audit dataset must be explicitly non-official and require warnings."
    }
    if ([bool]$audit.affectsGameplaySuccessFailure -ne $false -or [bool]$audit.implementsP8D -ne $false -or [bool]$audit.implementsP9SelectableGameplay -ne $false) {
        throw "Candidate audit must remain data-only and gameplay-neutral."
    }
    if ([int]$audit.screeningSummary.plateauIncludedCount -le 0) {
        throw "Candidate audit must include PLATEAU-derived candidates when local evidence exists."
    }

    foreach ($record in $records) {
        if ([string]$record.candidateLayer -ne "humanitarian_candidate") {
            throw "Record $($record.candidateId) has unexpected candidateLayer=$($record.candidateLayer)"
        }
        if ([bool]$record.isOfficialShelter -ne $false) {
            throw "Record $($record.candidateId) claims official shelter status."
        }
        if ([bool]$record.nonOfficialWarningRequired -ne $true) {
            throw "Record $($record.candidateId) does not require non-official warning."
        }
        if ([bool]$record.manualReviewNeeded -ne $true -and [string]::IsNullOrWhiteSpace([string]$record.evidenceStatus)) {
            throw "Record $($record.candidateId) lacks manual review or evidence status."
        }
        if ([string]$record.officialDesignationStatus -match "^official") {
            throw "Record $($record.candidateId) has an official designation status."
        }
        if ([bool]$record.affectsGameplaySuccessFailure -ne $false -or [bool]$record.implementsP8D -ne $false -or [bool]$record.implementsP9SelectableGameplay -ne $false) {
            throw "Record $($record.candidateId) enables out-of-scope gameplay implementation."
        }
        if ([bool]$record.hazardStatusEligible -ne $true -or [bool]$record.p8cProxyEligible -ne $true) {
            throw "Record $($record.candidateId) is missing P8-C hazard/proxy eligibility."
        }
    }

    $doc = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repoRoot "docs\P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md")
    foreach ($fragment in @(
        "These are not official evacuation shelters",
        "They must not be displayed as official shelters",
        "Total candidates found:",
        "Total PLATEAU-derived candidates:",
        "Candidate list expanded beyond original 5 sample candidates: yes",
        "Table A: Named High-Rise / Office / Tower Candidates",
        "Table B: Existing P5 Sample Candidates",
        "Table C: PLATEAU ID-Only Candidates Requiring Manual Name Review"
    )) {
        if ($doc.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "Candidate name list missing required wording: $fragment"
        }
    }
}

Write-Host "P8 humanitarian candidate audit preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-P8StageCount
    Assert-ProtectedPaths
    Assert-NoOutOfScopeChangedFiles
    Assert-CandidateAudit
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8 humanitarian candidate audit preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8 humanitarian candidate audit preflight: PASS" -ForegroundColor Green
exit 0
