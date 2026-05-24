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
    $text = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\"))
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
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
        throw "P8 must have exactly five stages: P8-A, P8-B, P8-C, P8-D, P8-E. Found: $($stages -join ', ')"
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
            throw "P8-B evidence work must not add P9/P10 systems: $file"
        }

        if ($file -match "^(Assets/(Scripts|Tests)/P8/.*(InfrastructureInteraction|CollapseProxy)|tools/p8/.*p8d)" ) {
            throw "P8-B evidence work must not implement P8-C/P8-D systems: $file"
        }

        if (Test-PathStartsWith $file "Assets/Scenes/") {
            throw "P8-B evidence work must not change scenes: $file"
        }
    }
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "Assets/Data/P8/tsunami_hazard_evidence_registry.json",
        "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json",
        "Assets/Data/P8/risk_front_visualization_config.json",
        "docs/P8B_TOKYO_TSUNAMI_EVIDENCE_REVIEW.md",
        "docs/P8B_CHUO_TSUNAMI_EXTRACTION_PLAN.md",
        "docs/P8B_HAZARD_LAYER_V1_STATUS.md",
        "docs/P8B_TO_P8C_GATE_DECISION.md",
        "tools/p8/run_p8b_evidence_preflight.ps1"
    )) {
        Assert-FileExists $file
    }
}

function Assert-Docs {
    Assert-FileContains "docs/P8B_TOKYO_TSUNAMI_EVIDENCE_REVIEW.md" @(
        "Chuo City does not appear to provide a standalone tsunami hazard map",
        "Tokyo Metropolitan Government",
        "maximum tsunami height",
        "maximum inundation depth",
        "2.12m to 2.46m",
        "not a full spatial inundation-depth grid"
    )

    Assert-FileContains "docs/P8B_CHUO_TSUNAMI_EXTRACTION_PLAN.md" @(
        "Tokyo Open Data",
        "maxTsunamiHeightMeters",
        "inundationDepthMeters",
        "boundaryIsEvidenceBasedOrPrototype=evidence_based",
        "PASS"
    )

    Assert-FileContains "docs/P8B_HAZARD_LAYER_V1_STATUS.md" @(
        "official_tsunami_metropolitan",
        "Tokyo Metropolitan Government",
        "extracted",
        "completeOfficialSpatialLayerExtracted",
        "maxTsunamiHeightMeters",
        "P8-C"
    )

    Assert-FileContains "docs/P8B_TO_P8C_GATE_DECISION.md" @(
        "PASS",
        "CONDITIONAL PASS",
        "BLOCKED",
        "Do not mark `BLOCKED` solely because Chuo lacks a standalone tsunami hazard map",
        "maximum tsunami height",
        "inundation-depth grid"
    )
}

function Assert-Registry {
    $registry = Read-Json "Assets/Data/P8/tsunami_hazard_evidence_registry.json"
    $requiredCategories = @(
        "official_tsunami_metropolitan",
        "official_tsunami_report_reference",
        "official_flood_proxy",
        "academic_model_candidate",
        "manual_extraction_required",
        "evidence_planned"
    )
    foreach ($category in $requiredCategories) {
        if ($registry.evidenceCategories -notcontains $category) {
            throw "Evidence registry missing category: $category"
        }
    }

    $requiredSources = @(
        "tokyo_damage_estimation_map_tsunami",
        "tokyo_damage_estimation_report_tsunami",
        "chuo_city_tsunami_liquefaction_page",
        "supplementary_pdf_tokyo_bay_tsunami_height_chuo",
        "flood_proxy_chuo_hazard_map"
    )
    $sourceIds = @($registry.sources | ForEach-Object { [string]$_.evidenceSourceId })
    foreach ($sourceId in $requiredSources) {
        if ($sourceIds -notcontains $sourceId) {
            throw "Evidence registry missing source: $sourceId"
        }
    }

    $primary = @($registry.sources | Where-Object { $_.primaryCandidate -eq $true -and $_.sourceCategory -eq "official_tsunami_metropolitan" })
    if ($primary.Count -lt 1) {
        throw "Evidence registry must prioritize at least one official_tsunami_metropolitan source."
    }

    $floodProxy = @($registry.sources | Where-Object { $_.evidenceSourceId -eq "flood_proxy_chuo_hazard_map" })
    if ($floodProxy.Count -ne 1 -or [bool]$floodProxy[0].nonTsunamiProxy -ne $true -or [bool]$floodProxy[0].fallbackOnly -ne $true) {
        throw "flood_proxy_chuo_hazard_map must be non-tsunami proxy fallback only."
    }

    if ([string]$registry.currentGateDecision -ne "PASS") {
        throw "Current P8-C gate decision must be PASS after official spatial extraction."
    }
}

function Assert-HazardLayer {
    $registry = Read-Json "Assets/Data/P8/tsunami_hazard_evidence_registry.json"
    $layer = Read-Json "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json"

    if ([string]$layer.sourceMode -ne "evidence_planned" -and [string]$layer.sourceMode -ne "official_tsunami_metropolitan") {
        throw "P8-B evidence layer sourceMode must be evidence_planned or official_tsunami_metropolitan."
    }

    if ([string]$layer.p8cGateDecision -ne "PASS") {
        throw "P8-B evidence layer must expose P8-C gate decision PASS after extraction."
    }

    if ([bool]$layer.officialMetropolitanEvidenceIdentified -ne $true) {
        throw "P8-B evidence layer must identify Tokyo metropolitan tsunami evidence."
    }

    if ([bool]$layer.completeOfficialSpatialLayerExtracted -ne $true) {
        throw "P8-B evidence layer must claim completeOfficialSpatialLayerExtracted=true only after official extraction."
    }

    if ([string]$layer.spatialExtractionStatus -ne "extracted") {
        throw "P8-B evidence layer must mark spatial extraction as extracted."
    }

    if ($layer.features.Count -lt 2) {
        throw "P8-B evidence layer should keep multiple front records for front driving tests."
    }

    $registryIds = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($source in $registry.sources) {
        $registryIds.Add([string]$source.evidenceSourceId) | Out-Null
    }

    foreach ($feature in $layer.features) {
        foreach ($field in @(
            "scenarioName",
            "arrivalTimeSeconds",
            "inundationDepthMeters",
            "maxTsunamiHeightMeters",
            "hazardIntensity",
            "sourceMode",
            "evidenceSourceId",
            "confidence",
            "geometryType",
            "boundaryIsEvidenceBasedOrPrototype",
            "inundationDepthStatus",
            "boundaryStatus",
            "spatialExtractionStatus"
        )) {
            if (-not ($feature.PSObject.Properties.Name -contains $field)) {
                throw "Hazard feature $($feature.featureId) missing $field."
            }
        }

        if (-not $registryIds.Contains([string]$feature.evidenceSourceId)) {
            throw "Hazard feature $($feature.featureId) references unregistered evidenceSourceId: $($feature.evidenceSourceId)"
        }

        if ([double]$feature.maxTsunamiHeightMeters -le 0) {
            throw "Hazard feature $($feature.featureId) must include a source maxTsunamiHeightMeters value."
        }

        if ([string]$feature.inundationDepthStatus -notmatch "extracted_spatial") {
            throw "Hazard feature $($feature.featureId) must use an extracted spatial inundation-depth value."
        }

        if ([string]$feature.boundaryIsEvidenceBasedOrPrototype -eq "evidence_based" -and [string]$feature.spatialExtractionStatus -ne "extracted") {
            throw "Hazard feature $($feature.featureId) must mark spatialExtractionStatus=extracted for evidence-based boundary use."
        }

        if ($feature.PSObject.Properties.Name -contains "spatialSampleCount" -and [int]$feature.spatialSampleCount -le 0) {
            throw "Hazard feature $($feature.featureId) must expose extracted spatialSampleCount."
        }

        if ([double]$feature.visualHeightMeters -gt 10.0 -and [bool]$feature.visualHeightIsCinematicOnly -ne $true) {
            throw "Hazard feature $($feature.featureId) has large visualHeightMeters without visualHeightIsCinematicOnly=true."
        }
    }
}

function Assert-RiskFrontConfig {
    $config = Read-Json "Assets/Data/P8/risk_front_visualization_config.json"
    foreach ($fragment in @(
        "Tokyo Metropolitan Government",
        "Official metropolitan source identified",
        "spatial depth/boundary extracted: yes",
        "official_spatial",
        "not a full real-time fluid simulation",
        "cinematic-only"
    )) {
        if ([string]$config.notes -notmatch [regex]::Escape($fragment)) {
            throw "risk-front config notes must document: $fragment"
        }
    }

    if ([string]$config.evidenceSourceId -ne "tokyo_damage_estimation_map_tsunami") {
        throw "risk-front config should point to the Tokyo damage estimation map evidence candidate."
    }

    if ([double]$config.visualHeightMeters -gt 10.0 -and [bool]$config.visualHeightIsCinematicOnly -ne $true) {
        throw "Large visualHeightMeters requires visualHeightIsCinematicOnly=true."
    }
}

function Assert-CodeDrivers {
    Assert-FileContains "Assets/Scripts/P8/P8HazardLayerLoader.cs" @(
        "tsunami_hazard_layer_v1_chuo.json",
        "tsunami_hazard_evidence_registry.json"
    )

    Assert-FileContains "Assets/Scripts/P8/P8RiskFrontCurveGenerator.cs" @(
        "maxTsunamiHeightMeters",
        "inundationDepthStatus",
        "spatialExtractionStatus",
        "hazard_layer_arrival_depth_boundary_v1"
    )
}

Write-Host "P8-B evidence preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-P8StageCount
    Assert-ProtectedPaths
    Assert-NoOutOfScopeSystems
    Assert-RequiredArtifacts
    Assert-Docs
    Assert-Registry
    Assert-HazardLayer
    Assert-RiskFrontConfig
    Assert-CodeDrivers
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-B evidence preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-B evidence preflight: PASS" -ForegroundColor Green
exit 0
