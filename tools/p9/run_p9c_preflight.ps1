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
    if ($branch -ne "p9c-evacuation-failure-congestion-proxy-gameplay") {
        throw "Expected branch p9c-evacuation-failure-congestion-proxy-gameplay, found $branch"
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

function Test-AllowedP9CPath {
    param([string]$Path)
    if ($Path -like "docs/P9C_*.md") { return $true }
    if ($Path -eq "docs/P9_STAGE_PLAN.md") { return $true }
    if ($Path -eq "docs/P9B_NEXT_STEPS_TO_P9C.md") { return $true }
    if ($Path -like "Assets/Data/P9/p9c_*.json") { return $true }
    if ($Path -like "Assets/Data/P9/p9c_*.json.meta") { return $true }
    if (Test-PathStartsWith $Path "Assets/Scripts/P9/") { return $true }
    if ($Path -eq "Assets/Scripts/Result/ResultMetrics.cs") { return $true }
    if ($Path -eq "Assets/Scripts/Result/ResultMetrics.cs.meta") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/EditMode/P9/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/PlayMode/P9/") { return $true }
    if ($Path -eq "tools/p9/run_p9c_preflight.ps1") { return $true }
    if ($Path -eq "tools/p9/validate_p9c_json.ps1") { return $true }
    if ($Path -eq "codex_prompts/p9c_evacuation_failure_congestion_proxy_gameplay.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p9c.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-AllowedP9CPath $file) { continue }
        $violations.Add("$file is outside P9-C allowed paths") | Out-Null
    }
    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }
        throw "P9-C changed-file scope check failed."
    }
    Write-Host "P9-C changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P9C_EVACUATION_FAILURE_GAMEPLAY.md",
        "docs/P9C_LIFE_FIRST_VERTICAL_TARGET_RULES.md",
        "docs/P9C_ENTRANCE_QUEUE_CONGESTION_RULES.md",
        "docs/P9C_SAFE_FLOOR_VERTICAL_EVACUATION_PROXY.md",
        "docs/P9C_COLLAPSE_DEBRIS_FATALITY_PROXY.md",
        "docs/P9C_RESULT_PANEL_AND_REASON_CODES.md",
        "docs/P9C_REFERENCE_MODEL_REVIEW.md",
        "docs/P9C_TEST_RESULTS.md",
        "docs/P9C_KNOWN_LIMITATIONS.md",
        "docs/P9C_REVIEW_BACKLOG.md",
        "docs/P9C_NEXT_STEPS_TO_P9D.md",
        "Assets/Data/P9/p9c_outcome_rules_config.json",
        "Assets/Data/P9/p9c_vertical_evacuation_target_rules.json",
        "Assets/Data/P9/p9c_entrance_congestion_rules.json",
        "Assets/Data/P9/p9c_safe_floor_proxy_rules.json",
        "Assets/Data/P9/p9c_collapse_debris_fatality_config.json",
        "Assets/Data/P9/p9c_scenario_failure_presets.json",
        "Assets/Data/P9/p9c_reason_code_catalog.json",
        "Assets/Scripts/P9/P9COutcomeReasonCode.cs",
        "Assets/Scripts/P9/P9CLifeFirstTargetSelector.cs",
        "Assets/Scripts/P9/P9CEntranceCongestionEvaluator.cs",
        "Assets/Scripts/P9/P9CSafeFloorEvaluator.cs",
        "Assets/Scripts/P9/P9CCollapseDebrisFatalityEvaluator.cs",
        "Assets/Scripts/P9/P9CHazardTimingOutcomeEvaluator.cs",
        "Assets/Scripts/P9/P9CVerticalEvacuationProxyResolver.cs",
        "Assets/Scripts/P9/P9CResultPanelFeedbackFormatter.cs",
        "Assets/Scripts/P9/P9CRunLogRecord.cs",
        "Assets/Tests/EditMode/P9/P9CDataAndBoundaryEditModeTests.cs",
        "Assets/Tests/EditMode/P9/P9CLifeFirstEntranceSafeFloorEditModeTests.cs",
        "Assets/Tests/EditMode/P9/P9CCollapseResultLogEditModeTests.cs",
        "Assets/Tests/PlayMode/P9/P9CRuntimePlayModeTests.cs",
        "tools/p9/run_p9c_preflight.ps1",
        "tools/p9/validate_p9c_json.ps1",
        "codex_prompts/p9c_evacuation_failure_congestion_proxy_gameplay.md",
        "deepseek_review_prompt_p9c.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-P8HandoffFiles {
    foreach ($file in @(
        "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json",
        "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json",
        "Assets/Data/P8/p8e_semantic_binding_v1.json",
        "Assets/Data/P8/p8e_p2_p6_new_map_adaptation_matrix.json",
        "Assets/Data/P8/p8e_route_candidate_geometry_handoff.json",
        "Assets/Data/P8/risk_front_progression_model_config.json"
    )) {
        Assert-FileExists $file
    }
}

function Assert-CodeBoundaries {
    $sourceDir = Join-Path $repoRoot "Assets\Scripts\P9"
    $p9Source = @(
        Get-ChildItem -LiteralPath $sourceDir -Filter "*.cs" |
            ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }
    ) -join "`n"
    $p9cSource = @(
        Get-ChildItem -LiteralPath $sourceDir -Filter "P9C*.cs" |
            ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }
    ) -join "`n"

    foreach ($forbidden in @(
        "EvacuationGameManager",
        "ResultPanelController",
        "ShelterEntranceTrigger",
        "BuildingShelter"
    )) {
        if ($p9Source.Contains($forbidden)) {
            throw "P9-C source must not directly reference legacy outcome/controller class $forbidden."
        }
    }

    foreach ($forbidden in @(
        "ClaimsOfficialRoutes = true",
        "ClaimsOfficialHumanitarianCandidateShelters = true",
        "routeIsOfficial = true"
    )) {
        if ($p9Source.Contains($forbidden)) {
            throw "P9-C source contains forbidden official claim: $forbidden"
        }
    }

    foreach ($forbiddenIndoor in @(
        "representative indoor template",
        "bim indoor",
        "indoor staircase",
        "fire escape route interior"
    )) {
        if ($p9Source.IndexOf($forbiddenIndoor, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw "P9-C runtime source contains forbidden building-interior gameplay wording: $forbiddenIndoor"
        }
    }

    foreach ($forbiddenRandom in @(
        "UnityEngine.Random",
        "Random.Range"
    )) {
        if ($p9cSource.Contains($forbiddenRandom)) {
            throw "P9-C source contains frame/random-style API: $forbiddenRandom"
        }
    }

    $collapseSource = Get-Content -Raw -LiteralPath (Join-Path $sourceDir "P9CCollapseDebrisFatalityEvaluator.cs")
    if ($collapseSource.Contains("Update(")) {
        throw "P9-C collapse/debris fatality must not run from Update."
    }
    if (-not $collapseSource.Contains("ExposureEventLevelProbability = true")) {
        throw "P9-C collapse/debris fatality must advertise exposure-event probability."
    }
    if (-not $collapseSource.Contains("FrameLevelRandomDeath = false")) {
        throw "P9-C collapse/debris fatality must reject frame-level random death."
    }
}

function Assert-ReferenceReview {
    $path = Join-Path $repoRoot "docs\P9C_REFERENCE_MODEL_REVIEW.md"
    $text = Get-Content -Raw -LiteralPath $path
    foreach ($required in @(
        "reference_only",
        "JR-Morgan/Crowd-Evacuation-Simulation",
        "TUNAMI-EVAC",
        "armostafizi/EvacuationModel",
        "fabhiansan/tsunami_simulation",
        "Project-PLATEAU/evacuation-simulation-tools",
        "Social Force Model",
        "RVO/ORCA"
    )) {
        if (-not $text.Contains($required)) {
            throw "P9-C reference review missing required text: $required"
        }
    }
}

Write-Host "P9-C preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-Branch
    Assert-P9StageCount
    Assert-NoExtraP9StageArtifacts
    Assert-ProtectedPathsClean
    Assert-ChangedFilesAllowed
    Assert-RequiredArtifacts
    Assert-P8HandoffFiles
    Assert-CodeBoundaries
    Assert-ReferenceReview

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p9c_json.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P9-C JSON validation failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P9-C preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P9-C preflight: PASS" -ForegroundColor Green
exit 0
