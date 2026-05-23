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
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    $text = Get-Content -Raw -LiteralPath $path
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
    }
}

function Test-AllowedP8Path {
    param([string]$Path)

    if ($Path -eq "Assets/Data/P8.meta") { return $true }
    if ($Path -eq "Assets/Scripts/P8.meta") { return $true }
    if ($Path -eq "Assets/Tests/EditMode/P8.meta") { return $true }
    if ($Path -eq "Assets/Tests/PlayMode/P8.meta") { return $true }
    if (Test-PathStartsWith $Path "docs/P8") { return $true }
    if (Test-PathStartsWith $Path "docs/P8A") { return $true }
    if (Test-PathStartsWith $Path "tools/p8/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Data/P8/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Scripts/P8/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/EditMode/P8/") { return $true }
    if (Test-PathStartsWith $Path "Assets/Tests/PlayMode/P8/") { return $true }
    if ($Path -eq "codex_prompts/p8a_baseline_hazard_data_layer.md") { return $true }
    if ($Path -eq "codex_prompts/p8a_scene_compatibility_gate.md") { return $true }
    if ($Path -eq "codex_prompts/p8a_hazard_evidence_hardening.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p8a.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p8a_compat.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p8a_evidence.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p8a_integration.md") { return $true }
    return $false
}

function Assert-ProtectedPathsClean {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    $baselineScene = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"

    foreach ($file in $changedFiles) {
        if ($file -eq $baselineScene) {
            $stagedBaseline = @(Get-GitLines @("-C", $repoRoot, "diff", "--cached", "--name-only", "--", $baselineScene))
            if ($stagedBaseline.Count -gt 0) {
                $violations.Add("$file is staged; protected P7 high-detail baseline scene must not be committed") | Out-Null
            }
            else {
                Write-Host "WARN: $file has local unstaged baseline changes and is intentionally preserved."
            }

            continue
        }

        if (Test-AllowedP8Path $file) {
            continue
        }

        foreach ($prefix in @(
            "ProjectSettings/",
            "Packages/",
            "Assets/PLATEAU/",
            "Library/",
            "Temp/",
            "Obj/",
            "Builds/",
            "logs/",
            "review_reports/",
            "test-results/"
        )) {
            if (Test-PathStartsWith $file $prefix) {
                $violations.Add("$file matches protected/generated prefix $prefix") | Out-Null
            }
        }

        if (Test-PathStartsWith $file "Assets/Data/") {
            $violations.Add("$file changes Assets/Data outside isolated P8 path") | Out-Null
        }

        if (Test-PathStartsWith $file "Assets/Scenes/Chuo_BaseMap.unity") {
            $violations.Add("$file modifies legacy fallback scene") | Out-Null
        }
        if (Test-PathStartsWith $file "Assets/Scripts/" -and -not (Test-PathStartsWith $file "Assets/Scripts/P8/")) {
            $violations.Add("$file changes gameplay script outside P8 foundation path") | Out-Null
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

function Assert-BaselinePreserved {
    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "inspect_p8a_baseline_scene.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "Baseline scene inspection failed."
    }
}

function Assert-P8Docs {
    Assert-FileContains "docs/P8A_BASELINE_HANDOFF_FROM_P7.md" @(
        "user-approved practical high-detail baseline",
        "Average LOD3 is not achieved",
        "local-only imported map state"
    )

    Assert-FileContains "docs/P8A_BASELINE_PRESERVATION_PLAN.md" @(
        'Do not run `git reset --hard`',
        "Do not checkout or restore",
        "archive"
    )

    Assert-FileContains "docs/P8_BOUNDARIES.md" @(
        "P8-A is foundation only",
        "Full real-time fluid simulation",
        "Gameplay success/failure rule changes"
    )

    Assert-FileContains "docs/P8A_SCIENCE_VS_VISUAL_LAYER.md" @(
        "cinematic-only",
        "not a physical tsunami height",
        "visualHeightIsCinematicOnly",
        "kilometer-scale",
        "P8-A does not implement"
    )

    Assert-FileContains "docs/P8A_EVIDENCE_SOURCE_REGISTRY.md" @(
        "official tsunami",
        "Tokyo/Chuo hazard maps",
        "Cabinet Office",
        "MLIT",
        "academic tsunami simulation papers",
        "PLATEAU",
        "OSM",
        "manual sample"
    )

    Assert-FileContains "docs/P8A_OFFICIAL_SOURCE_REVIEW_PROTOCOL.md" @(
        "must not fetch",
        "reviewed_for_values",
        "P8-A does not introduce an official runtime source mode"
    )

    Assert-FileContains "docs/P8A_HAZARD_VARIABLE_DEFINITIONS.md" @(
        "arrivalTimeSeconds",
        "inundationDepthMeters",
        "tsunamiHeightMeters",
        "visualHeightMeters",
        "sourceMode"
    )

    Assert-FileContains "docs/P8A_ARRIVAL_DEPTH_BOUNDARY_MODEL.md" @(
        "arrival front",
        "visual risk front",
        "data boundary",
        "visual curtain boundary"
    )

    Assert-FileContains "docs/P8A_CINEMATIC_LIGHT_CURTAIN_RULES.md" @(
        "kilometer-scale",
        "visualHeightIsCinematicOnly=true",
        "P8-A does not implement"
    )

    Assert-FileContains "docs/P8A_P2_P6_COMPATIBILITY_GATE.md" @(
        "does not change gameplay success/failure rules",
        "remains fail-safe and opt-in",
        'not hard-bound to `Chuo_BaseMap`'
    )
}

function Assert-P8Artifacts {
    foreach ($file in @(
        "Assets/Data/P8/tsunami_hazard_layer_schema.json",
        "Assets/Data/P8/tsunami_hazard_sample_chuo.json",
        "Assets/Data/P8/risk_front_visualization_config.json",
        "Assets/Data/P8/infrastructure_hazard_interaction_config.json",
        "Assets/Scripts/P8/P8HazardLayerData.cs",
        "Assets/Scripts/P8/P8HazardLayerLoader.cs",
        "Assets/Scripts/P8/P8RiskFrontConfig.cs",
        "Assets/Scripts/P8/P8HazardDataValidator.cs",
        "Assets/Scripts/P8/P8SceneCompatibilityReport.cs",
        "Assets/Tests/EditMode/P8/P8HazardLayerFoundationTests.cs",
        "Assets/Tests/EditMode/P8/P8SceneCompatibilityGateTests.cs",
        "Assets/Tests/PlayMode/P8/P8SceneCompatibilityPlayModeTests.cs",
        "tools/p8/inspect_p8a_p2_p6_compatibility.ps1",
        "tools/p8/inspect_p8a_baseline_scene.ps1",
        "tools/p8/inspect_p8a_scene_compatibility.ps1",
        "tools/p8/run_p8a_compat_preflight.ps1",
        "tools/p8/validate_p8_hazard_json.ps1",
        "codex_prompts/p8a_baseline_hazard_data_layer.md",
        "codex_prompts/p8a_scene_compatibility_gate.md",
        "codex_prompts/p8a_hazard_evidence_hardening.md",
        "deepseek_review_prompt_p8a.md",
        "deepseek_review_prompt_p8a_compat.md",
        "deepseek_review_prompt_p8a_evidence.md",
        "deepseek_review_prompt_p8a_integration.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-NoP8RuntimeImplementation {
    $scriptPath = Join-Path $repoRoot "Assets\Scripts\P8"
    if (Test-Path -LiteralPath $scriptPath -PathType Container) {
        $matches = @(
            Get-ChildItem -LiteralPath $scriptPath -Recurse -Filter *.cs |
                Select-String -Pattern "MonoBehaviour|void Update\s*\("
        )

        if ($matches.Count -gt 0) {
            foreach ($match in $matches) {
                Write-Host "FAIL: P8-A runtime implementation marker at $($match.Path):$($match.LineNumber)"
            }
            throw "P8-A must remain loader/validator foundation only."
        }
    }
}

function Assert-NoP9P10Systems {
    $changedFiles = @(Get-ChangedFiles)
    foreach ($file in $changedFiles) {
        if ($file -match "Assets/(Scripts|Tests)/(P9|P10)") {
            throw "P8-A must not add P9/P10 systems: $file"
        }
    }
}

Write-Host "P8-A preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-P8StageCount
    Assert-BaselinePreserved
    Assert-ProtectedPathsClean
    Assert-P8Artifacts
    Assert-P8Docs
    Assert-NoP8RuntimeImplementation
    Assert-NoP9P10Systems

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p8_hazard_json.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8 hazard JSON validation failed."
    }

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "inspect_p8a_p2_p6_compatibility.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P2-P6 compatibility inspection failed."
    }

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "inspect_p8a_scene_compatibility.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "Scene compatibility inspection failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-A preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-A preflight: PASS" -ForegroundColor Green
exit 0
