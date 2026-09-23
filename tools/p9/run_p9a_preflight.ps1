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
        $output = & git -c filter.lfs.clean= -c filter.lfs.smudge= -c filter.lfs.process= -c filter.lfs.required=false @GitArgs 2>$null
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
    $text = Get-Content -Raw -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\"))
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
    }
}

function Assert-Branch {
    $branch = (& git -C $repoRoot branch --show-current).Trim()
    if ($branch -ne "p9-crowd-spawn-evacuation-proxy-foundation") {
        throw "Expected branch p9-crowd-spawn-evacuation-proxy-foundation, found $branch"
    }
}

function Assert-P9StageCount {
    Assert-FileExists "docs/P9_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath (Join-Path $repoRoot "docs\P9_STAGE_PLAN.md") |
            Where-Object { $_ -match "^##\s+(P9-[A-Z0-9]+)\s*$" } |
            ForEach-Object { $Matches[1] }
    )

    $expected = @("P9-A", "P9-B", "P9-C", "P9-D")
    $unexpected = @($stages | Where-Object { $expected -notcontains $_ })
    foreach ($stage in $expected) {
        if ($stages -notcontains $stage) {
            throw "Missing P9 stage: $stage"
        }
    }

    if ($stages.Count -ne 4 -or $unexpected.Count -gt 0) {
        throw "P9 must have exactly four stages: P9-A, P9-B, P9-C, P9-D. Found: $($stages -join ', ')"
    }
}

function Assert-NoExtraP9StageArtifacts {
    $trackedAndChanged = @(
        Get-GitLines @("-C", $repoRoot, "ls-files") |
            ForEach-Object { Normalize-RepoPath $_ }
    ) + @(Get-ChangedFiles)

    $files = @(
        $trackedAndChanged |
            Where-Object { $_ -match "(^|[\\/_.-])p9[-_]?[efg]($|[\\/_.-])" } |
            Sort-Object -Unique
    )

    if ($files.Count -gt 0) {
        throw "Unexpected P9 stage artifact path detected: $($files -join '; ')"
    }
}

function Assert-ProtectedPaths {
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

function Test-AllowedP9APath {
    param([string]$Path)

    if ($Path -like "docs/P9_*.md") { return $true }
    if ($Path -like "docs/P9A_*.md") { return $true }
    if (Test-PathStartsWith $Path "tools/p9/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Data/P9/") { return $true }
    if ($Path -eq "Assets/Data/P9.meta") { return $true }
    if (Test-PathStartsWith $Path "Assets/Scripts/P9/") { return $true }
    if ($Path -eq "Assets/Scripts/P9.meta") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/EditMode/P9/") { return $true }
    if ($Path -eq "Assets/Tests/EditMode/P9.meta") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/PlayMode/P9/") { return $true }
    if ($Path -eq "Assets/Tests/PlayMode/P9.meta") { return $true }
    if ($Path -eq "codex_prompts/p9a_crowd_spawn_evacuation_proxy_foundation.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p9a.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]

    foreach ($file in $changedFiles) {
        if (Test-AllowedP9APath $file) {
            continue
        }

        $violations.Add("$file is outside P9-A allowed paths") | Out-Null
    }

    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }

        throw "P9-A changed-file scope check failed."
    }

    Write-Host "P9-A changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P9_STAGE_PLAN.md",
        "docs/P9_BOUNDARIES.md",
        "docs/P9_RELATION_TO_P8.md",
        "docs/P9_NO_INDOOR_SCENE_DECISION.md",
        "docs/P9A_P8_HANDOFF_CONTRACT.md",
        "docs/P9A_METRICS_AND_LOGGING_PLAN.md",
        "docs/P9A_FOUNDATION_SUMMARY.md",
        "docs/P9A_TEST_RESULTS.md",
        "docs/P9A_KNOWN_LIMITATIONS.md",
        "docs/P9A_REVIEW_BACKLOG.md",
        "docs/P9A_NEXT_STEPS.md",
        "Assets/Data/P9/p9_spawn_point_schema.json",
        "Assets/Data/P9/p9_spawn_points_sample.json",
        "Assets/Data/P9/p9_crowd_agent_schema.json",
        "Assets/Data/P9/p9_crowd_agents_sample.json",
        "Assets/Data/P9/p9_entrance_safe_floor_proxy_schema.json",
        "Assets/Data/P9/p9_entrance_safe_floor_proxy_sample.json",
        "Assets/Scripts/P9/P9SpawnPointData.cs",
        "Assets/Scripts/P9/P9SpawnPointLoader.cs",
        "Assets/Scripts/P9/P9CrowdAgentProfile.cs",
        "Assets/Scripts/P9/P9EntranceSafeFloorProxy.cs",
        "Assets/Scripts/P9/P9CrowdSimulationConfig.cs",
        "Assets/Scripts/P9/P9EvacuationProxyState.cs",
        "Assets/Scripts/P9/P9P8HandoffAdapter.cs",
        "Assets/Scripts/P9/P9ScenarioDebugSummary.cs",
        "Assets/Tests/EditMode/P9/P9FoundationEditModeTests.cs",
        "Assets/Tests/PlayMode/P9/P9FoundationPlayModeTests.cs",
        "tools/p9/run_p9a_preflight.ps1",
        "tools/p9/validate_p9_json.ps1",
        "codex_prompts/p9a_crowd_spawn_evacuation_proxy_foundation.md",
        "deepseek_review_prompt_p9a.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-DocumentationRules {
    Assert-FileContains "docs/P9_STAGE_PLAN.md" @(
        "P9 has exactly four stages",
        "P9-A",
        "P9-B",
        "P9-C",
        "P9-D",
        "P7_HighDetail_Chuo practical baseline",
        "P8 finalizes",
        "Do not implement P10 release packaging in P9"
    )

    Assert-FileContains "docs/P9_BOUNDARIES.md" @(
        "P9-A is scaffold only",
        'No modification to `Assets/Scenes/Chuo_BaseMap.unity`',
        'No modification to `ProjectSettings` or `Packages`',
        "No real indoor scene gameplay",
        "No NPC/crowd-caused player failure in P9-A",
        "Humanitarian high-rise candidates can exist as non-official candidates only"
    )

    Assert-FileContains "docs/P9_RELATION_TO_P8.md" @(
        "P8-D/E continue in parallel",
        "P9 does not solve the P8 hazard/front/infrastructure foundation",
        "P9-A uses null/fallback/no-effect behavior",
        "Final gameplay failure waits until P9-C"
    )

    Assert-FileContains "docs/P9_NO_INDOOR_SCENE_DECISION.md" @(
        "Real indoor shelter scenes are cancelled",
        "no LOD4/BIM interior scene",
        "external/abstracted proxy flow",
        "non-official candidates"
    )

    Assert-FileContains "docs/P9A_P8_HANDOFF_CONTRACT.md" @(
        "infrastructure hazard state",
        "road/bridge/underground state",
        "building warning state",
        "entrance blocked state",
        "low-floor inundation warning",
        "collapse/damage proxy state",
        "sourceMode",
        "evidenceSourceId",
        "depth status",
        "intensity status",
        "arrival timing status",
        "does not require those outputs to be final",
        "null/fallback/no-effect"
    )
}

function Assert-CodeRules {
    Assert-FileContains "Assets/Scripts/P9/P9EvacuationProxyState.cs" @(
        "AffectsGameplaySuccessFailure = false",
        "CanCausePlayerFailureInP9A = false",
        "ImplementsFinalFailureGameplay = false",
        "ImplementsIndoorSceneGameplay = false",
        "RequiresChuoBaseMap = false",
        "RequiresP8DEFinalHandoff = false"
    )

    Assert-FileContains "Assets/Scripts/P9/P9P8HandoffAdapter.cs" @(
        "RequiresP8DEFinalHandoff",
        "unknown_no_effect",
        "P9-A uses no-effect fallback state"
    )

    $sourceDir = Join-Path $repoRoot "Assets\Scripts\P9"
    $sourceText = @(
        Get-ChildItem -LiteralPath $sourceDir -Filter "*.cs" |
            ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }
    ) -join "`n"

    foreach ($forbidden in @(
        "EvacuationGameManager",
        "ResultPanelController",
        "ShelterEntranceTrigger",
        "BuildingShelter",
        "Chuo_BaseMap",
        "P7_HighDetail_Chuo"
    )) {
        if ($sourceText.Contains($forbidden)) {
            throw "P9-A runtime source must not reference $forbidden."
        }
    }

    foreach ($forbiddenIndoor in @(
        "fire-route",
        "interior-template",
        "stair gameplay"
    )) {
        if ($sourceText.IndexOf($forbiddenIndoor, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw "P9-A runtime source contains indoor gameplay wording: $forbiddenIndoor"
        }
    }
}

function Assert-HumanitarianCandidatesRemainNonOfficial {
    $json = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "Assets\Data\P9\p9_entrance_safe_floor_proxy_sample.json") | ConvertFrom-Json
    foreach ($proxy in @($json.proxies)) {
        if ([bool]$proxy.humanitarianCandidateFlag) {
            if ([bool]$proxy.isOfficialShelter) {
                throw "Humanitarian candidate must remain non-official: $($proxy.proxyId)"
            }

            if (-not [bool]$proxy.nonOfficialWarningRequired) {
                throw "Humanitarian candidate requires non-official warning: $($proxy.proxyId)"
            }
        }
    }
}

Write-Host "P9-A preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-Branch
    Assert-P9StageCount
    Assert-NoExtraP9StageArtifacts
    Assert-ProtectedPaths
    Assert-ChangedFilesAllowed
    Assert-RequiredArtifacts
    Assert-DocumentationRules
    Assert-CodeRules
    Assert-HumanitarianCandidatesRemainNonOfficial

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p9_json.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P9 JSON validation failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P9-A preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P9-A preflight: PASS" -ForegroundColor Green
exit 0
