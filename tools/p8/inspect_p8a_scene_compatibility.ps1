[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$baselineRelative = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
$legacyBaseMapRelative = "Assets/Scenes/Chuo_BaseMap.unity"
$minimumBaselineBytes = 1000000000

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

function Get-GitStatusFor {
    param([string[]]$RelativePaths)

    $gitArgs = @("-C", $repoRoot, "status", "--porcelain=v1", "--") + $RelativePaths
    return @(Get-GitLines $gitArgs)
}

function Get-BaselineCandidates {
    $baselinePath = Join-Path $repoRoot ($baselineRelative -replace "/", "\")
    $candidates = New-Object 'System.Collections.Generic.List[string]'
    $candidates.Add($baselinePath) | Out-Null

    if (-not [string]::IsNullOrWhiteSpace($env:P8A_BASELINE_SCENE_PATH)) {
        $candidates.Add($env:P8A_BASELINE_SCENE_PATH) | Out-Null
    }

    $defaultSiblingP7 = "D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity"
    if ($defaultSiblingP7 -ne $baselinePath) {
        $candidates.Add($defaultSiblingP7) | Out-Null
    }

    return @($candidates | Select-Object -Unique)
}

function Select-LargeBaselineScene {
    param([string[]]$CandidatePaths)

    foreach ($candidate in $CandidatePaths) {
        if ([string]::IsNullOrWhiteSpace($candidate)) {
            continue
        }

        if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            continue
        }

        $item = Get-Item -LiteralPath $candidate
        Write-Host "INFO: P7 high-detail baseline candidate: $candidate"
        Write-Host "INFO: Candidate bytes: $($item.Length)"
        if ($item.Length -ge $minimumBaselineBytes) {
            return $item
        }
    }

    return $null
}

function Assert-FileExists {
    param(
        [string]$RelativePath,
        [string]$Label
    )

    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "$Label missing required file: $RelativePath"
    }

    Write-Host "PASS: $Label -> $RelativePath"
}

function Assert-P8StageCount {
    $stagePlanPath = Join-Path $repoRoot "docs\P8_STAGE_PLAN.md"
    if (-not (Test-Path -LiteralPath $stagePlanPath -PathType Leaf)) {
        throw "Missing docs/P8_STAGE_PLAN.md"
    }

    $stageLines = @(
        Get-Content -LiteralPath $stagePlanPath |
            Where-Object { $_ -match "^##\s+(P8-[A-Z0-9]+)\s*$" } |
            ForEach-Object { $Matches[1] }
    )

    $expected = @("P8-A", "P8-B", "P8-C", "P8-D")
    $unexpected = @($stageLines | Where-Object { $expected -notcontains $_ })

    if ($stageLines.Count -ne 4 -or $unexpected.Count -gt 0) {
        throw "P8 stage plan must contain exactly P8-A, P8-B, P8-C, and P8-D. Found: $($stageLines -join ', ')"
    }

    foreach ($stage in $expected) {
        if ($stageLines -notcontains $stage) {
            throw "P8 stage plan missing required stage: $stage"
        }
    }

    Write-Host "PASS: P8 has exactly four stages: $($expected -join ', ')"
}

function Assert-BaselineScene {
    $baselinePath = Join-Path $repoRoot ($baselineRelative -replace "/", "\")
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw "FAIL: Missing P7 high-detail baseline scene: $baselineRelative"
    }

    Write-Host "PASS: P7 high-detail baseline shell exists in current worktree: $baselineRelative"

    $sceneItem = Select-LargeBaselineScene @(Get-BaselineCandidates)
    if ($null -eq $sceneItem) {
        throw "FAIL: No large P7 high-detail baseline scene found. Set P8A_BASELINE_SCENE_PATH or restore the local P7 high-detail baseline."
    }

    Write-Host "INFO: Selected P7 high-detail baseline: $($sceneItem.FullName)"
    Write-Host "INFO: P7 high-detail baseline bytes: $($sceneItem.Length)"
    Write-Host "INFO: P7 high-detail baseline last write: $($sceneItem.LastWriteTime)"

    if ($sceneItem.FullName -ne $baselinePath) {
        Write-Host "INFO: Using external local P7 baseline; current worktree scene shell is not modified."
    }

    $tracked = @(Get-GitLines @("-C", $repoRoot, "ls-files", "--stage", "--", $baselineRelative))
    if ($tracked.Count -eq 0) {
        Write-Warning "P7 high-detail baseline is not tracked by Git; treat it as local-only and protected."
    }
    else {
        Write-Host "INFO: P7 high-detail baseline is tracked by Git."
    }

    $status = @(Get-GitStatusFor @($baselineRelative))
    if ($status.Count -gt 0) {
        Write-Warning "P7 high-detail baseline has local git status: $($status -join '; ')"
    }
    else {
        Write-Host "INFO: P7 high-detail baseline git status is clean."
    }
}

function Assert-LegacyBaseMapUntouched {
    $legacyPath = Join-Path $repoRoot ($legacyBaseMapRelative -replace "/", "\")
    if (Test-Path -LiteralPath $legacyPath -PathType Leaf) {
        Write-Host "INFO: Legacy base map exists locally: $legacyBaseMapRelative"
    }
    else {
        Write-Host "INFO: Legacy base map is not present in this checkout: $legacyBaseMapRelative"
    }

    $status = @(Get-GitStatusFor @($legacyBaseMapRelative))
    if ($status.Count -gt 0) {
        throw "FAIL: Chuo_BaseMap has git status changes: $($status -join '; ')"
    }

    Write-Host "PASS: Chuo_BaseMap has no git status changes."
}

function Assert-P8HazardDataExists {
    foreach ($file in @(
        "Assets/Data/P8/tsunami_hazard_layer_schema.json",
        "Assets/Data/P8/tsunami_hazard_sample_chuo.json",
        "Assets/Data/P8/risk_front_visualization_config.json",
        "Assets/Data/P8/infrastructure_hazard_interaction_config.json"
    )) {
        Assert-FileExists $file "P8-A hazard data"
    }
}

function Assert-CompatibilityDocsExist {
    foreach ($file in @(
        "docs/P8A_SCENE_COMPATIBILITY_GATE.md",
        "docs/P8A_P7_HIGHDETAIL_BASELINE_STATUS.md",
        "docs/P8A_P2_P6_COMPATIBILITY_SMOKE_REPORT.md",
        "docs/P8A_P8B_SCENE_ANCHOR_PLAN.md"
    )) {
        Assert-FileExists $file "P8-A compatibility document"
    }
}

function Assert-NoP9P10SystemsAdded {
    $branch = (& git -C $repoRoot branch --show-current).Trim()
    if ($branch -match "^p9" -or $branch -match "^p10") {
        Write-Host "INFO: Later-stage branch '$branch' detected; skipping P8-A changed-file prohibition for P9/P10 paths."
        return
    }

    $changedFiles = @(Get-ChangedFiles)
    foreach ($file in $changedFiles) {
        if ($file -match "^(Assets/(Scripts|Tests)/(P9|P10)|tools/(p9|p10)/|docs/P(9|10))") {
            throw "FAIL: P8-A must not add P9/P10 systems or docs in this gate: $file"
        }
    }

    Write-Host "PASS: No changed P9/P10 system paths detected."
}

function Assert-ProjectSettingsAndPackagesClean {
    $status = @(Get-GitStatusFor @("ProjectSettings", "Packages"))
    if ($status.Count -gt 0) {
        throw "FAIL: ProjectSettings/Packages dirty state detected: $($status -join '; ')"
    }

    Write-Host "PASS: ProjectSettings and Packages have no git status changes."
}

function Test-AllowedP8RuntimeScript {
    param([string]$Path)

    $name = [System.IO.Path]::GetFileName($Path)
    return $name -eq "P8RiskFrontController.cs" -or
           $name -eq "P8RiskFrontLightCurtainRenderer.cs" -or
           $name -eq "P8RiskFrontTimeDriver.cs" -or
           $name -eq "P8RiskFrontDebugStatus.cs" -or
           $name -eq "P8InfrastructureHazardTarget.cs" -or
           $name -eq "P8InfrastructureHazardMarker.cs" -or
           $name -eq "P8InfrastructureHazardDebugSummary.cs"
}

function Assert-NoUnauthorizedP8RuntimeBehavior {
    $scriptPath = Join-Path $repoRoot "Assets\Scripts\P8"
    if (-not (Test-Path -LiteralPath $scriptPath -PathType Container)) {
        throw "Missing P8 script folder: Assets/Scripts/P8"
    }

    $matches = @(
        Get-ChildItem -LiteralPath $scriptPath -Recurse -Filter *.cs |
            Select-String -Pattern "MonoBehaviour|void Update\s*\("
    )

    $unauthorized = @($matches | Where-Object { -not (Test-AllowedP8RuntimeScript $_.Path) })
    if ($unauthorized.Count -gt 0) {
        foreach ($match in $unauthorized) {
            Write-Host "FAIL: unauthorized later-stage/runtime marker at $($match.Path):$($match.LineNumber)"
        }

        throw "Only approved P8-B risk-front runtime scripts may contain MonoBehaviour/Update in P8."
    }

    Write-Host "PASS: no unauthorized P8 runtime behavior detected."
}

Write-Host "P8-A scene compatibility inspection"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-P8StageCount
    Assert-BaselineScene
    Assert-LegacyBaseMapUntouched
    Assert-P8HazardDataExists
    Assert-CompatibilityDocsExist
    Assert-NoP9P10SystemsAdded
    Assert-ProjectSettingsAndPackagesClean
    Assert-NoUnauthorizedP8RuntimeBehavior
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-A scene compatibility inspection: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-A scene compatibility inspection: PASS" -ForegroundColor Green
exit 0
