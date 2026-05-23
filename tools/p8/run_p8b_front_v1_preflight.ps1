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
        throw "P7 high-detail baseline scene is staged; do not commit scene mutation."
    }

    $baselinePath = Join-Path $repoRoot ($baselineScene -replace "/", "\")
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw "Missing P7 high-detail baseline scene: $baselineScene"
    }

    $baselineItem = Get-Item -LiteralPath $baselinePath
    if ($baselineItem.Length -lt 1000000000) {
        throw "P7 high-detail baseline scene is unexpectedly small; baseline may be lost."
    }
}

function Assert-NoOutOfScopeSystems {
    $changedFiles = @(Get-ChangedFiles)
    foreach ($file in $changedFiles) {
        if ($file -eq $baselineScene) {
            continue
        }

        if ($file -match "^(Assets/(Scripts|Tests)/(P9|P10)|tools/(p9|p10)/|docs/P(9|10))") {
            throw "P8-B front v1 must not add P9/P10 systems: $file"
        }

        if ($file -match "^(Assets/(Scripts|Tests)/P8/.*CollapseProxy|tools/p8/.*p8d|docs/P8D_)" ) {
            throw "P8-B front v1 regression guard must not include P8-D collapse systems: $file"
        }
    }
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P8B_HAZARD_LAYER_V1_STATUS.md",
        "docs/P8B_EVIDENCE_BACKED_FRONT_MODEL.md",
        "docs/P8B_TOKYO_TSUNAMI_EVIDENCE_REVIEW.md",
        "docs/P8B_CHUO_TSUNAMI_EXTRACTION_PLAN.md",
        "docs/P8B_TO_P8C_GATE_DECISION.md",
        "docs/P8B_RISK_FRONT_DATA_DRIVEN_BEHAVIOR.md",
        "docs/P8B_ARRIVAL_DEPTH_BOUNDARY_DRIVEN_FRONT.md",
        "docs/P8B_TO_P8C_HANDOFF_CHECKLIST.md",
        "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json",
        "Assets/Data/P8/tsunami_hazard_evidence_registry.json",
        "Assets/Data/P8/risk_front_visualization_config.json",
        "Assets/Scripts/P8/P8RiskFrontCurveGenerator.cs",
        "Assets/Scripts/P8/P8RiskFrontController.cs",
        "Assets/Scripts/P8/P8RiskFrontLightCurtainRenderer.cs",
        "Assets/Tests/EditMode/P8/P8RiskFrontVisualizationTests.cs",
        "Assets/Tests/PlayMode/P8/P8RiskFrontPlayModeTests.cs",
        "tools/p8/run_p8b_front_v1_preflight.ps1"
    )) {
        Assert-FileExists $file
    }
}

function Assert-Docs {
    Assert-FileContains "docs/P8B_HAZARD_LAYER_V1_STATUS.md" @(
        "official_tsunami_metropolitan",
        "Tokyo Metropolitan Government",
        "extracted",
        "arrivalTimeSeconds",
        "inundationBoundary",
        "inundationDepthMeters",
        "hazardIntensity",
        "maxTsunamiHeightMeters",
        "evidenceSourceId",
        "PASS",
        "P8-C"
    )

    Assert-FileContains "docs/P8B_EVIDENCE_BACKED_FRONT_MODEL.md" @(
        "hazard-layer v1",
        "Tokyo Metropolitan Government",
        "official_spatial",
        "visualHeightMeters",
        "P8-C"
    )

    Assert-FileContains "docs/P8B_RISK_FRONT_DATA_DRIVEN_BEHAVIOR.md" @(
        "hazard_layer_arrival_depth_boundary_v1",
        "no full fluid simulation",
        "does not change gameplay success/failure"
    )

    Assert-FileContains "docs/P8B_ARRIVAL_DEPTH_BOUNDARY_DRIVEN_FRONT.md" @(
        "arrivalTimeSeconds",
        "fallback_procedural_boundary",
        "inundationDepthMeters",
        "hazardIntensity"
    )
}

function Assert-HazardLayerV1Data {
    $hazardPath = Join-Path $repoRoot "Assets\Data\P8\tsunami_hazard_layer_v1_chuo.json"
    $hazard = Get-Content -Raw -LiteralPath $hazardPath | ConvertFrom-Json

    if ($hazard.sourceMode -ne "manual_sample" -and $hazard.sourceMode -ne "evidence_planned" -and $hazard.sourceMode -ne "official_tsunami_metropolitan") {
        throw "P8-B hazard layer v1 must remain manual_sample, evidence_planned, or official_tsunami_metropolitan."
    }

    if ([bool]$hazard.completeOfficialSpatialLayerExtracted -eq $true -and [string]$hazard.spatialExtractionStatus -ne "extracted") {
        throw "P8-B hazard layer must not claim complete official spatial extraction unless spatialExtractionStatus=extracted."
    }

    if ($hazard.features.Count -lt 2) {
        throw "P8-B hazard layer v1 must include multiple front/boundary records; expected at least 2."
    }

    $registeredEvidenceSourceIds = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($source in $hazard.evidenceSources) {
        if ([string]::IsNullOrWhiteSpace([string]$source.evidenceSourceId)) {
            throw "Hazard evidence source is missing evidenceSourceId."
        }
        $registeredEvidenceSourceIds.Add([string]$source.evidenceSourceId) | Out-Null
    }

    foreach ($feature in $hazard.features) {
        foreach ($field in @(
            "arrivalTimeSeconds",
            "inundationDepthMeters",
            "hazardIntensity",
            "sourceMode",
            "evidenceSourceId",
            "confidence",
            "geometryType",
            "boundaryIsEvidenceBasedOrPrototype",
            "maxTsunamiHeightMeters",
            "inundationDepthStatus",
            "boundaryStatus",
            "spatialExtractionStatus"
        )) {
            if (-not ($feature.PSObject.Properties.Name -contains $field)) {
                throw "Hazard feature $($feature.featureId) missing $field."
            }
        }

        if (-not $registeredEvidenceSourceIds.Contains([string]$feature.evidenceSourceId)) {
            throw "Hazard feature $($feature.featureId) references unregistered evidenceSourceId: $($feature.evidenceSourceId)"
        }
        if ([double]$feature.maxTsunamiHeightMeters -le 0) {
            throw "Hazard feature $($feature.featureId) must include maxTsunamiHeightMeters from the official source."
        }
        if ([string]$feature.inundationDepthStatus -notmatch "extracted_spatial") {
            throw "Hazard feature $($feature.featureId) must use an extracted spatial inundation-depth field."
        }
        if ($feature.PSObject.Properties.Name -contains "spatialSampleCount" -and $feature.PSObject.Properties.Name -contains "spatialSamples") {
            if ([int]$feature.spatialSampleCount -le 0 -or [int]$feature.spatialSampleCount -ne $feature.spatialSamples.Count) {
                throw "Hazard feature $($feature.featureId) spatialSampleCount must match non-empty spatialSamples."
            }
        }
        if ([double]$feature.visualHeightMeters -gt 10.0 -and [bool]$feature.visualHeightIsCinematicOnly -ne $true) {
            throw "Hazard feature $($feature.featureId) has large visualHeightMeters without visualHeightIsCinematicOnly=true."
        }
    }
}

function Assert-RiskFrontConfig {
    $configPath = Join-Path $repoRoot "Assets\Data\P8\risk_front_visualization_config.json"
    $config = Get-Content -Raw -LiteralPath $configPath | ConvertFrom-Json

    if ([bool]$config.riskFrontEnabledInP8B -ne $true) {
        throw "riskFrontEnabledInP8B must be true for P8-B front v1."
    }
    if ([double]$config.visualHeightMeters -gt 10.0 -and [bool]$config.visualHeightIsCinematicOnly -ne $true) {
        throw "Large visualHeightMeters requires visualHeightIsCinematicOnly=true."
    }
    if ($config.scienceLayerFields -contains "visualHeightMeters") {
        throw "scienceLayerFields must not contain visualHeightMeters."
    }
    if ($config.visualLayerFields -contains "tsunamiHeightMeters" -or $config.visualLayerFields -contains "waterLevelMeters") {
        throw "visualLayerFields must not contain physical tsunami/water fields."
    }
    foreach ($fragment in @("arrival", "depth", "intensity", "evidenceSourceId", "official_spatial", "cinematic-only")) {
        if ([string]$config.notes -notmatch [regex]::Escape($fragment)) {
            throw "risk-front config notes must document $fragment."
        }
    }
}

function Assert-CodeDrivers {
    Assert-FileContains "Assets/Scripts/P8/P8RiskFrontCurveGenerator.cs" @(
        "hazard_layer_arrival_depth_boundary_v1",
        "arrivalTimeSeconds",
        "inundationDepthMeters",
        "hazardIntensity",
        "fallback_procedural_boundary",
        "evidenceSourceId"
    )

    Assert-FileContains "Assets/Scripts/P8/P8RiskFrontLightCurtainRenderer.cs" @(
        "LastVisualIntensity01",
        "LastAppliedAlpha"
    )
}

Write-Host "P8-B front v1 preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-P8StageCount
    Assert-ProtectedPaths
    Assert-NoOutOfScopeSystems
    Assert-RequiredArtifacts
    Assert-Docs
    Assert-HazardLayerV1Data
    Assert-RiskFrontConfig
    Assert-CodeDrivers
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-B front v1 preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-B front v1 preflight: PASS" -ForegroundColor Green
exit 0
