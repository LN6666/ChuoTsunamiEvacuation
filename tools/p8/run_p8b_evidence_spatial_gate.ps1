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
    $diffFiles = Get-GitLines @("-C", $repoRoot, "diff", "--name-only", "HEAD", "--")
    $untrackedFiles = Get-GitLines @("-C", $repoRoot, "ls-files", "--others", "--exclude-standard")
    $changedFiles = @($diffFiles + $untrackedFiles | ForEach-Object { Normalize-RepoPath $_ } | Sort-Object -Unique)

    foreach ($file in $changedFiles) {
        if ($file -eq $baselineScene) {
            continue
        }

        if ($file -match "^(Assets/(Scripts|Tests)/(P9|P10)|tools/(p9|p10)/|docs/P(9|10))") {
            throw "P8-B spatial gate must not add P9/P10 systems: $file"
        }

        if ($file -match "^(Assets/(Scripts|Tests)/P8/.*CollapseProxy|tools/p8/.*p8d)" ) {
            throw "P8-B spatial gate regression guard must not include P8-D collapse systems: $file"
        }

        if ($file.StartsWith("Assets/Scenes/", [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "P8-B spatial gate must not change scenes: $file"
        }
    }
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "docs/P8B_TOKYO_TSUNAMI_SPATIAL_EXTRACTION.md",
        "docs/P8B_CHUO_INUNDATION_DEPTH_LAYER_CONSTRUCTION.md",
        "docs/P8B_OFFICIAL_TSUNAMI_SOURCE_DECISION.md",
        "docs/P8B_TO_P8C_GATE_DECISION.md",
        "Assets/Data/P8/tsunami_hazard_evidence_registry.json",
        "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json",
        "Assets/Data/P8/risk_front_visualization_config.json",
        "tools/p8/extract_p8b_chuo_tsunami_layer.py",
        "tools/p8/run_p8b_evidence_spatial_gate.ps1"
    )) {
        Assert-FileExists $file
    }
}

function Assert-Docs {
    Assert-FileContains "docs/P8B_TOKYO_TSUNAMI_SPATIAL_EXTRACTION.md" @(
        "Tokyo metropolitan evidence is the primary source candidate",
        "Chuo standalone tsunami-map absence is not evidence absence",
        "maxTsunamiHeightMeters is not the same as full inundationDepthMeters grid",
        "direct HTTPS CSV",
        "no token"
    )

    Assert-FileContains "docs/P8B_CHUO_INUNDATION_DEPTH_LAYER_CONSTRUCTION.md" @(
        "Tokyo Open Data",
        "MLIT N03",
        "clipped to Chuo",
        "spatialSampleCount",
        "not an official inundation contour"
    )

    Assert-FileContains "docs/P8B_OFFICIAL_TSUNAMI_SOURCE_DECISION.md" @(
        "PASS",
        "Tokyo Metropolitan Government",
        "official_tsunami_metropolitan",
        "maximum tsunami height is not the inundation-depth grid"
    )

    Assert-FileContains "docs/P8B_TO_P8C_GATE_DECISION.md" @(
        "PASS",
        "CONDITIONAL PASS",
        "BLOCKED",
        "P8-C may proceed",
        "no full fluid simulation"
    )
}

function Assert-Registry {
    $registry = Read-Json "Assets/Data/P8/tsunami_hazard_evidence_registry.json"
    if ([string]$registry.currentGateDecision -ne "PASS") {
        throw "Evidence registry currentGateDecision must be PASS after official spatial extraction."
    }

    $requiredSources = @(
        "tokyo_damage_estimation_map_tsunami",
        "tokyo_damage_estimation_report_tsunami",
        "chuo_city_tsunami_liquefaction_page",
        "supplementary_pdf_tokyo_bay_tsunami_height_chuo",
        "flood_proxy_chuo_hazard_map",
        "mlit_n03_chuo_admin_boundary"
    )

    foreach ($sourceId in $requiredSources) {
        $source = @($registry.sources | Where-Object { $_.evidenceSourceId -eq $sourceId })
        if ($source.Count -ne 1) {
            throw "Evidence registry missing source: $sourceId"
        }
    }

    $tokyo = @($registry.sources | Where-Object { $_.evidenceSourceId -eq "tokyo_damage_estimation_map_tsunami" })[0]
    if ([string]$tokyo.sourceCategory -ne "official_tsunami_metropolitan" -or [string]$tokyo.extractionStatus -ne "extracted") {
        throw "Tokyo tsunami source must be official_tsunami_metropolitan with extractionStatus=extracted."
    }
    if ([bool]$tokyo.requiresTokenOrLogin -ne $false) {
        throw "Tokyo tsunami source should be recorded as no token/login required."
    }
    if ([string]$tokyo.accessMethod -notmatch "direct_https_csv") {
        throw "Tokyo tsunami source accessMethod must record direct HTTPS CSV access."
    }

    $boundary = @($registry.sources | Where-Object { $_.evidenceSourceId -eq "mlit_n03_chuo_admin_boundary" })[0]
    if ([string]$boundary.sourceCategory -ne "official_admin_boundary" -or [string]$boundary.extractionStatus -ne "extracted") {
        throw "MLIT N03 boundary source must be official_admin_boundary with extractionStatus=extracted."
    }
}

function Assert-HazardLayer {
    $registry = Read-Json "Assets/Data/P8/tsunami_hazard_evidence_registry.json"
    $layer = Read-Json "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json"

    if ([string]$layer.sourceMode -ne "official_tsunami_metropolitan") {
        throw "Spatial layer sourceMode must be official_tsunami_metropolitan."
    }
    if ([string]$layer.p8cGateDecision -ne "PASS") {
        throw "P8-C gate must be PASS only after extracted official spatial layer exists."
    }
    if ([string]$layer.extractionStatus -ne "extracted" -or [string]$layer.spatialExtractionStatus -ne "extracted") {
        throw "Spatial layer extractionStatus and spatialExtractionStatus must be extracted."
    }
    if ([bool]$layer.completeOfficialSpatialLayerExtracted -ne $true) {
        throw "completeOfficialSpatialLayerExtracted must be true for PASS."
    }
    if ($layer.features.Count -lt 2) {
        throw "Spatial layer must include multiple scenario records."
    }

    $registryIds = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($source in $registry.sources) {
        $registryIds.Add([string]$source.evidenceSourceId) | Out-Null
    }

    foreach ($feature in $layer.features) {
        foreach ($field in @(
            "scenarioName",
            "sourceMode",
            "sourceCategory",
            "evidenceSourceId",
            "geometryType",
            "extractionStatus",
            "spatialExtractionStatus",
            "arrivalTimeSeconds",
            "inundationDepthMeters",
            "maxInundationDepthMeters",
            "maxTsunamiHeightMeters",
            "inundationDepthStatus",
            "spatialSampleCount",
            "spatialSamples",
            "boundaryIsEvidenceBasedOrPrototype"
        )) {
            if (-not ($feature.PSObject.Properties.Name -contains $field)) {
                throw "Hazard feature $($feature.featureId) missing $field."
            }
        }

        if (-not $registryIds.Contains([string]$feature.evidenceSourceId)) {
            throw "Hazard feature $($feature.featureId) references unregistered evidenceSourceId."
        }
        if ([string]$feature.geometryType -ne "grid") {
            throw "Hazard feature $($feature.featureId) must be geometryType=grid for extracted mesh samples."
        }
        if ([string]$feature.extractionStatus -ne "extracted" -or [string]$feature.spatialExtractionStatus -ne "extracted") {
            throw "Hazard feature $($feature.featureId) extraction statuses must be extracted."
        }
        if ([string]$feature.boundaryIsEvidenceBasedOrPrototype -ne "evidence_based") {
            throw "Hazard feature $($feature.featureId) must mark boundaryIsEvidenceBasedOrPrototype=evidence_based after extraction."
        }
        if ([double]$feature.maxTsunamiHeightMeters -le 0 -or [double]$feature.maxInundationDepthMeters -le 0) {
            throw "Hazard feature $($feature.featureId) must include positive max tsunami height and max inundation depth values."
        }
        if ([string]$feature.inundationDepthStatus -notmatch "extracted_spatial") {
            throw "Hazard feature $($feature.featureId) must record extracted spatial inundation-depth status."
        }
        if ([int]$feature.spatialSampleCount -le 0 -or [int]$feature.spatialSampleCount -ne $feature.spatialSamples.Count) {
            throw "Hazard feature $($feature.featureId) spatialSampleCount must match non-empty spatialSamples."
        }
        if ([string]$feature.notes -notmatch "not used as the inundation-depth grid") {
            throw "Hazard feature $($feature.featureId) notes must keep maxTsunamiHeightMeters separate from the inundation-depth grid."
        }
    }
}

function Assert-RiskFrontConfig {
    $config = Read-Json "Assets/Data/P8/risk_front_visualization_config.json"
    if ([string]$config.riskFrontDriverMode -ne "official_spatial") {
        throw "Risk front config must report current risk front driver official_spatial."
    }
    if ([bool]$config.officialMetropolitanSourceIdentified -ne $true -or [bool]$config.spatialDepthBoundaryExtracted -ne $true) {
        throw "Risk front config must report official source identified and spatial depth/boundary extracted."
    }
    if ([bool]$config.completeOfficialSpatialLayerClaim -ne $true) {
        throw "Risk front config must report completeOfficialSpatialLayerClaim=true only because extraction is complete."
    }
    if ([double]$config.visualHeightMeters -gt 10.0 -and [bool]$config.visualHeightIsCinematicOnly -ne $true) {
        throw "Large visualHeightMeters requires visualHeightIsCinematicOnly=true."
    }
    foreach ($fragment in @("official_spatial", "not a full real-time fluid simulation", "not physical tsunami height", "maxTsunamiHeightMeters is separate")) {
        if ([string]$config.notes -notmatch [regex]::Escape($fragment)) {
            throw "risk-front config notes must document: $fragment"
        }
    }
}

Write-Host "P8-B evidence spatial gate: starting"
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
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-B evidence spatial gate: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-B evidence spatial gate: PASS" -ForegroundColor Green
exit 0
