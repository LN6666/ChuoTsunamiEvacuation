[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$baselineScene = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
$minimumBaselineBytes = 1000000000

function Normalize-RepoPath {
    param([string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path)) { return "" }
    return ($Path -replace "\\", "/").TrimStart("./".ToCharArray()).TrimStart("/")
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

    if ($null -eq $output) { return @() }
    return @($output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-ChangedFiles {
    $diffFiles = Get-GitLines @("-C", $repoRoot, "diff", "--name-only", "HEAD", "--")
    $untrackedFiles = Get-GitLines @("-C", $repoRoot, "ls-files", "--others", "--exclude-standard")
    return @(@($diffFiles) + @($untrackedFiles) | ForEach-Object { Normalize-RepoPath $_ } | Where-Object { $_ } | Sort-Object -Unique)
}

function Assert-FileExists {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Assert-FileContains {
    param([string]$RelativePath, [string[]]$Fragments)
    Assert-FileExists $RelativePath
    $text = Get-Content -Raw -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\"))
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
    }
}

function Assert-P8StageCount {
    Assert-FileExists "docs/P8_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath (Join-Path $repoRoot "docs\P8_STAGE_PLAN.md") |
            Where-Object { $_ -match "^##\s+(P8-[A-Z0-9]+)\s*$" } |
            ForEach-Object { $Matches[1] }
    )
    $expected = @("P8-A", "P8-B", "P8-C", "P8-D")
    $unexpected = @($stages | Where-Object { $expected -notcontains $_ })
    if ($stages.Count -ne 4 -or $unexpected.Count -gt 0) {
        throw "P8 must have exactly four stages: P8-A, P8-B, P8-C, P8-D. Found: $($stages -join ', ')"
    }
}

function Assert-ProtectedPaths {
    $protectedStatus = @(Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", "ProjectSettings", "Packages", "Assets/Scenes/Chuo_BaseMap.unity", "Assets/PLATEAU"))
    if ($protectedStatus.Count -gt 0) {
        throw "Protected path dirty state detected: $($protectedStatus -join '; ')"
    }

    $stagedBaseline = @(Get-GitLines @("-C", $repoRoot, "diff", "--cached", "--name-only", "--", $baselineScene))
    if ($stagedBaseline.Count -gt 0) {
        throw "P7 high-detail baseline scene is staged; do not commit scene mutation in this P8-B pass."
    }

    $baselinePath = Join-Path $repoRoot ($baselineScene -replace "/", "\")
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw "Missing P7 high-detail baseline scene: $baselineScene"
    }

    $baselineItem = Get-Item -LiteralPath $baselinePath
    if ($baselineItem.Length -lt $minimumBaselineBytes) {
        throw "P7 high-detail baseline scene is unexpectedly small; baseline may be lost."
    }
}

function Assert-P8BArtifacts {
    foreach ($file in @(
        "Assets/Scripts/P8/P8RiskFrontController.cs",
        "Assets/Scripts/P8/P8RiskFrontVisualConfig.cs",
        "Assets/Scripts/P8/P8RiskFrontCurveGenerator.cs",
        "Assets/Scripts/P8/P8RiskFrontLightCurtainRenderer.cs",
        "Assets/Scripts/P8/P8RiskFrontDebugStatus.cs",
        "Assets/Scripts/P8/P8RiskFrontTimeDriver.cs",
        "Assets/Tests/EditMode/P8/P8RiskFrontVisualizationTests.cs",
        "Assets/Tests/PlayMode/P8/P8RiskFrontPlayModeTests.cs",
        "Assets/Data/P8/risk_front_visualization_config.json",
        "Assets/Data/P8/tsunami_hazard_sample_chuo.json",
        "docs/P8B_DYNAMIC_RISK_FRONT_DESIGN.md",
        "docs/P8B_LIGHT_CURTAIN_VISUAL_RULES.md",
        "docs/P8B_SCIENCE_VS_CINEMATIC_VISUAL.md",
        "docs/P8B_SCENE_ANCHOR_REPORT.md",
        "docs/P8B_TEST_RESULTS.md",
        "docs/P8B_KNOWN_LIMITATIONS.md",
        "docs/P8B_NEXT_TO_P8C_HANDOFF.md",
        "codex_prompts/p8b_dynamic_risk_front.md",
        "deepseek_review_prompt_p8b_riskfront.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-DocsAndDisclaimers {
    foreach ($file in @(
        "docs/P8B_DYNAMIC_RISK_FRONT_DESIGN.md",
        "docs/P8B_LIGHT_CURTAIN_VISUAL_RULES.md",
        "docs/P8B_SCIENCE_VS_CINEMATIC_VISUAL.md",
        "docs/P8B_SCENE_ANCHOR_REPORT.md"
    )) {
        Assert-FileContains $file @(
            "not physical tsunami height",
            "no real-time fluid simulation",
            "no official hazard"
        )
    }

    Assert-FileContains "Assets/Scripts/P8/P8RiskFrontVisualConfig.cs" @(
        "Cinematic risk-front visualization, not physical tsunami height",
        "AffectsGameplaySuccessFailure = false"
    )
}

function Assert-HazardConfig {
    $configPath = Join-Path $repoRoot "Assets\Data\P8\risk_front_visualization_config.json"
    $config = Get-Content -Raw -LiteralPath $configPath | ConvertFrom-Json
    if ([bool]$config.riskFrontEnabledInP8A -ne $false) {
        throw "riskFrontEnabledInP8A must remain false."
    }
    if ([bool]$config.riskFrontEnabledInP8B -ne $true) {
        throw "riskFrontEnabledInP8B must be true for P8-B visualization."
    }
    if ([double]$config.visualHeightMeters -gt 10.0 -and [bool]$config.visualHeightIsCinematicOnly -ne $true) {
        throw "Large visualHeightMeters requires visualHeightIsCinematicOnly=true."
    }
    if ([string]$config.p8bDisclaimer -notlike "*not physical tsunami height*") {
        throw "risk-front config missing cinematic-not-physical disclaimer."
    }
}

function Assert-NoOutOfScopeSystems {
    $changedFiles = @(Get-ChangedFiles)
    foreach ($file in $changedFiles) {
        if ($file -match "^(Assets/(Scripts|Tests)/(P9|P10)|tools/(p9|p10)/|docs/P(9|10))") {
            throw "P8-B must not add P9/P10 systems: $file"
        }
        if ($file -match "^(Assets/(Scripts|Tests)/P8/.*CollapseProxy|tools/p8/.*p8d|docs/P8D_)" ) {
            throw "P8-B regression guard must not include P8-D collapse systems: $file"
        }
    }
}

Write-Host "P8-B risk-front preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-P8StageCount
    Assert-ProtectedPaths
    Assert-P8BArtifacts
    Assert-DocsAndDisclaimers
    Assert-HazardConfig
    Assert-NoOutOfScopeSystems
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-B risk-front preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-B risk-front preflight: PASS" -ForegroundColor Green
exit 0
