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
    if ($branch -ne "p9b-new-map-spawn-crowd-runtime-prototype") {
        throw "Expected branch p9b-new-map-spawn-crowd-runtime-prototype, found $branch"
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

function Test-AllowedP9BPath {
    param([string]$Path)
    if ($Path -like "docs/P9B_*.md") { return $true }
    if ($Path -eq "docs/P9_STAGE_PLAN.md") { return $true }
    if ($Path -eq "docs/P9A_NEXT_STEPS.md") { return $true }
    if ($Path -eq "tools/p9/run_p9b_preflight.ps1") { return $true }
    if ($Path -eq "tools/p9/validate_p9b_json.ps1") { return $true }
    if ($Path -eq "tools/p8/inspect_p8a_scene_compatibility.ps1") { return $true }
    if ($Path -like "Assets/Data/P9/p9b_*.json") { return $true }
    if ($Path -like "Assets/Data/P9/p9b_*.json.meta") { return $true }
    if (Test-PathStartsWith $Path "Assets/Scripts/P9/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/EditMode/P9/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/PlayMode/P9/") { return $true }
    if ($Path -eq "codex_prompts/p9b_new_map_spawn_crowd_runtime_prototype.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p9b.md") { return $true }

    $p8Allowed = @(
        "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json",
        "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json.meta",
        "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json",
        "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json.meta",
        "Assets/Data/P8/p8e_semantic_binding_v1.json",
        "Assets/Data/P8/p8e_semantic_binding_v1.json.meta",
        "Assets/Data/P8/p8e_p2_p6_new_map_adaptation_matrix.json",
        "Assets/Data/P8/p8e_p2_p6_new_map_adaptation_matrix.json.meta",
        "Assets/Data/P8/p8e_route_candidate_geometry_handoff.json",
        "Assets/Data/P8/p8e_route_candidate_geometry_handoff.json.meta",
        "Assets/Data/P8/risk_front_progression_model_config.json",
        "Assets/Data/P8/risk_front_progression_model_config.json.meta"
    )
    return $p8Allowed -contains $Path
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-AllowedP9BPath $file) { continue }
        $violations.Add("$file is outside P9-B allowed paths") | Out-Null
    }
    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }
        throw "P9-B changed-file scope check failed."
    }
    Write-Host "P9-B changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P9B_NEW_MAP_SCENE_INTEGRATION.md",
        "docs/P9B_WEIGHTED_SPAWN_SYSTEM.md",
        "docs/P9B_HUMANITARIAN_CANDIDATE_MARKERS.md",
        "docs/P9B_CROWD_RUNTIME_PROTOTYPE.md",
        "docs/P9B_ENTRANCE_SAFE_FLOOR_RUNTIME_PROTOTYPE.md",
        "docs/P9B_COLLAPSE_RISK_ZONE_PROTOTYPE.md",
        "docs/P9B_TEST_RESULTS.md",
        "docs/P9B_KNOWN_LIMITATIONS.md",
        "docs/P9B_REVIEW_BACKLOG.md",
        "docs/P9B_NEXT_STEPS_TO_P9C.md",
        "Assets/Data/P9/p9b_weighted_spawn_config.json",
        "Assets/Data/P9/p9b_spawn_zones_sample.json",
        "Assets/Data/P9/p9b_runtime_crowd_scenario_sample.json",
        "Assets/Data/P9/p9b_entrance_marker_assignments_sample.json",
        "Assets/Data/P9/p9b_collapse_debris_risk_zones_sample.json",
        "Assets/Data/P9/p9b_humanitarian_marker_runtime_config.json",
        "Assets/Scripts/P9/P9WeightedSpawnSelector.cs",
        "Assets/Scripts/P9/P9SceneRuntimeBootstrap.cs",
        "Assets/Scripts/P9/P9HumanitarianCandidateMarkerRuntime.cs",
        "Assets/Scripts/P9/P9EntranceSafeFloorMarkerRuntime.cs",
        "Assets/Scripts/P9/P9CrowdRuntimeSpawner.cs",
        "Assets/Scripts/P9/P9CollapseDebrisRiskZone.cs",
        "Assets/Tests/EditMode/P9/P9BWeightedSpawnEditModeTests.cs",
        "Assets/Tests/EditMode/P9/P9BHumanitarianCandidateEditModeTests.cs",
        "Assets/Tests/EditMode/P9/P9BProxyBoundaryEditModeTests.cs",
        "Assets/Tests/PlayMode/P9/P9BRuntimePlayModeTests.cs",
        "tools/p9/run_p9b_preflight.ps1",
        "tools/p9/validate_p9b_json.ps1",
        "codex_prompts/p9b_new_map_spawn_crowd_runtime_prototype.md",
        "deepseek_review_prompt_p9b.md"
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
    $sourceText = @(
        Get-ChildItem -LiteralPath $sourceDir -Filter "*.cs" |
            ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }
    ) -join "`n"

    foreach ($forbidden in @(
        "EvacuationGameManager",
        "ResultPanelController",
        "ShelterEntranceTrigger",
        "BuildingShelter"
    )) {
        if ($sourceText.Contains($forbidden)) {
            throw "P9-B source must not directly reference outcome/controller class $forbidden."
        }
    }

    foreach ($forbidden in @(
        "AffectsGameplaySuccessFailure = true",
        "CanCausePlayerFailureInP9B = true",
        "ImplementsFinalFailureGameplay = true",
        "ClaimsOfficialRoutes = true",
        "ClaimsOfficialHumanitarianCandidateShelters = true"
    )) {
        if ($sourceText.Contains($forbidden)) {
            throw "P9-B source contains forbidden enabled policy: $forbidden"
        }
    }

    foreach ($forbiddenIndoor in @(
        "interior template",
        "bim indoor",
        "indoor staircase",
        "fire escape route interior"
    )) {
        if ($sourceText.IndexOf($forbiddenIndoor, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw "P9-B runtime source contains indoor gameplay wording: $forbiddenIndoor"
        }
    }
}

Write-Host "P9-B preflight: starting"
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

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p9b_json.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P9-B JSON validation failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P9-B preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P9-B preflight: PASS" -ForegroundColor Green
exit 0
