[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$dataRoot = Join-Path $repoRoot "Assets\Data\P8"
$allowedSourceModes = @("test", "evidence_planned", "manual_sample")
$evidenceRequiredSourceModes = @("evidence_planned", "manual_sample")
$allowedGeometryTypes = @("grid", "polygon", "polyline", "point", "synthetic")
$allowedBoundaryKinds = @("evidence_based", "prototype")
$allowedSourceCategories = @(
    "official_tsunami_inundation_map",
    "tokyo_chuo_hazard_map",
    "cabinet_office_mlit_local_government",
    "academic_tsunami_simulation_paper",
    "plateau_citygml_category",
    "osm_route_context",
    "manual_sample"
)
$allowedReviewedStatuses = @(
    "not_attached",
    "attached_unreviewed",
    "reviewed_for_planning",
    "reviewed_for_values"
)
$requiredScienceFields = @(
    "arrivalTimeSeconds",
    "inundationDepthMeters",
    "waterLevelMeters",
    "tsunamiHeightMeters",
    "inundationBoundary",
    "hazardIntensity",
    "confidence",
    "evidenceSourceId"
)
$requiredVisualFields = @(
    "visualHeightMeters",
    "visualHeightIsCinematicOnly"
)

function Read-Json {
    param([string]$RelativePath)

    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing JSON file: $RelativePath"
    }

    return Get-Content -Raw -LiteralPath $path | ConvertFrom-Json
}

function Assert-HasProperty {
    param(
        [object]$Object,
        [string]$Property,
        [string]$Label
    )

    if ($null -eq $Object -or -not ($Object.PSObject.Properties.Name -contains $Property)) {
        throw "$Label missing required property: $Property"
    }
}

function Assert-NonEmptyString {
    param(
        [object]$Value,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace([string]$Value)) {
        throw "$Label must be a non-empty string."
    }
}

function Assert-AllowedSourceMode {
    param(
        [string]$SourceMode,
        [string]$Label
    )

    Assert-NonEmptyString $SourceMode "$Label sourceMode"
    if ($SourceMode -like "*official*") {
        throw "$Label uses an official-looking sourceMode that is not allowed in P8-A: $SourceMode"
    }
    if ($allowedSourceModes -notcontains $SourceMode) {
        throw "$Label sourceMode must be one of: $($allowedSourceModes -join ', ')"
    }
}

function Test-RequiresEvidenceSource {
    param([string]$SourceMode)
    return $evidenceRequiredSourceModes -contains $SourceMode
}

function Assert-EvidenceSourceId {
    param(
        [object]$Object,
        [string]$Label
    )

    Assert-HasProperty $Object "sourceMode" $Label
    Assert-AllowedSourceMode $Object.sourceMode $Label
    if (Test-RequiresEvidenceSource $Object.sourceMode) {
        Assert-HasProperty $Object "evidenceSourceId" $Label
        Assert-NonEmptyString $Object.evidenceSourceId "$Label evidenceSourceId"
    }
}

function Assert-NumberRange {
    param(
        [object]$Value,
        [double]$Minimum,
        [double]$Maximum,
        [string]$Label
    )

    $number = [double]$Value
    if ($number -lt $Minimum -or $number -gt $Maximum) {
        throw "$Label must be between $Minimum and $Maximum."
    }
}

function Assert-FieldSeparation {
    param(
        [object]$Object,
        [string]$Label
    )

    Assert-HasProperty $Object "scienceLayerFields" $Label
    Assert-HasProperty $Object "visualLayerFields" $Label

    foreach ($field in $requiredScienceFields) {
        if ($Object.scienceLayerFields -notcontains $field) {
            throw "$Label scienceLayerFields missing $field."
        }
    }

    foreach ($field in $requiredVisualFields) {
        if ($Object.visualLayerFields -notcontains $field) {
            throw "$Label visualLayerFields missing $field."
        }
    }

    foreach ($field in $requiredVisualFields) {
        if ($Object.scienceLayerFields -contains $field) {
            throw "$Label scienceLayerFields must not contain visual field $field."
        }
    }

    foreach ($field in $requiredScienceFields) {
        if ($Object.visualLayerFields -contains $field) {
            throw "$Label visualLayerFields must not contain science field $field."
        }
    }
}

function Assert-VisualHeightGuard {
    param(
        [object]$Object,
        [string]$Label
    )

    Assert-HasProperty $Object "visualHeightMeters" $Label
    Assert-HasProperty $Object "visualHeightIsCinematicOnly" $Label

    if ([double]$Object.visualHeightMeters -gt 10.0 -and [bool]$Object.visualHeightIsCinematicOnly -ne $true) {
        throw "$Label has large visualHeightMeters but visualHeightIsCinematicOnly is not true."
    }
}

function Assert-ManualSampleMarked {
    param(
        [object]$Object,
        [string]$Label
    )

    if ($Object.sourceMode -eq "manual_sample") {
        Assert-HasProperty $Object "notes" $Label
        $notes = [string]$Object.notes
        if ($notes.IndexOf("not official", [System.StringComparison]::OrdinalIgnoreCase) -lt 0 -and
            $notes.IndexOf("non-authoritative", [System.StringComparison]::OrdinalIgnoreCase) -lt 0 -and
            $notes.IndexOf("placeholder", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$Label manual_sample data must be clearly marked not official/non-authoritative/placeholder."
        }
    }
}

function Assert-SchemaContains {
    param([string[]]$Fragments)

    $schemaPath = Join-Path $dataRoot "tsunami_hazard_layer_schema.json"
    if (-not (Test-Path -LiteralPath $schemaPath -PathType Leaf)) {
        throw "Missing hazard schema: Assets/Data/P8/tsunami_hazard_layer_schema.json"
    }

    $schemaText = Get-Content -Raw -LiteralPath $schemaPath
    foreach ($fragment in $Fragments) {
        if ($schemaText.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "Hazard schema missing required fragment: $fragment"
        }
    }
}

Write-Host "P8-A hazard JSON validation"

Assert-SchemaContains @(
    "scenarioId",
    "sourceMode",
    "scienceLayerFields",
    "visualLayerFields",
    "arrivalTimeSeconds",
    "inundationDepthMeters",
    "waterLevelMeters",
    "tsunamiHeightMeters",
    "visualHeightIsCinematicOnly",
    "sourceCategory",
    "reviewedStatus",
    "hazardDrivenCollapse"
)

$sample = Read-Json "Assets/Data/P8/tsunami_hazard_sample_chuo.json"
Assert-HasProperty $sample "scenarioId" "sample hazard"
Assert-HasProperty $sample "sourceMode" "sample hazard"
Assert-HasProperty $sample "hazardLayerVersion" "sample hazard"
Assert-HasProperty $sample "timeOriginSeconds" "sample hazard"
Assert-HasProperty $sample "features" "sample hazard"
Assert-HasProperty $sample "evidenceSources" "sample hazard"
Assert-AllowedSourceMode $sample.sourceMode "sample hazard"
Assert-FieldSeparation $sample "sample hazard"
Assert-ManualSampleMarked $sample "sample hazard"

if ($sample.features.Count -lt 1) {
    throw "sample hazard must contain at least one feature."
}

if ((Test-RequiresEvidenceSource $sample.sourceMode) -and $sample.evidenceSources.Count -lt 1) {
    throw "sample hazard manual/evidence-planned data requires evidenceSources."
}

$registeredEvidenceSourceIds = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
foreach ($source in $sample.evidenceSources) {
    foreach ($field in @("evidenceSourceId", "sourceMode", "sourceCategory", "title", "reviewedStatus")) {
        Assert-HasProperty $source $field "sample evidence source"
    }

    Assert-AllowedSourceMode $source.sourceMode "sample evidence source"
    if ($allowedSourceCategories -notcontains $source.sourceCategory) {
        throw "sample evidence source has invalid sourceCategory: $($source.sourceCategory)"
    }
    if ($allowedReviewedStatuses -notcontains $source.reviewedStatus) {
        throw "sample evidence source has invalid reviewedStatus: $($source.reviewedStatus)"
    }
    if ($source.reviewedStatus -eq "reviewed_for_values" -and $source.sourceCategory -ne "manual_sample") {
        throw "P8-A must not introduce reviewed official hazard values."
    }
    if (-not $registeredEvidenceSourceIds.Add([string]$source.evidenceSourceId)) {
        throw "Duplicate evidenceSourceId: $($source.evidenceSourceId)"
    }
}

foreach ($feature in $sample.features) {
    foreach ($field in @(
        "featureId",
        "sourceMode",
        "geometryType",
        "arrivalTimeSeconds",
        "inundationDepthMeters",
        "waterLevelMeters",
        "tsunamiHeightMeters",
        "inundationBoundary",
        "hazardIntensity",
        "confidence",
        "evidenceSourceId",
        "boundaryIsEvidenceBasedOrPrototype",
        "affectedInfrastructureTypes",
        "buildingDamageState",
        "collapseProxyState",
        "collapseProbability",
        "collapseRandomSeed",
        "hazardDrivenCollapse"
    )) {
        Assert-HasProperty $feature $field "sample feature"
    }

    Assert-AllowedSourceMode $feature.sourceMode "sample feature"
    if ($feature.sourceMode -ne $sample.sourceMode) {
        throw "sample feature sourceMode must match sample hazard sourceMode."
    }
    if ($allowedGeometryTypes -notcontains $feature.geometryType) {
        throw "sample feature geometryType is not allowed: $($feature.geometryType)"
    }
    if ($allowedBoundaryKinds -notcontains $feature.boundaryIsEvidenceBasedOrPrototype) {
        throw "sample feature boundaryIsEvidenceBasedOrPrototype is invalid: $($feature.boundaryIsEvidenceBasedOrPrototype)"
    }
    Assert-NumberRange $feature.hazardIntensity 0 1 "sample feature hazardIntensity"
    Assert-NumberRange $feature.confidence 0 1 "sample feature confidence"
    Assert-EvidenceSourceId $feature "sample feature"
    if (-not $registeredEvidenceSourceIds.Contains([string]$feature.evidenceSourceId)) {
        throw "sample feature evidenceSourceId is not registered: $($feature.evidenceSourceId)"
    }
    Assert-VisualHeightGuard $feature "sample feature $($feature.featureId)"
    if ([bool]$feature.hazardDrivenCollapse -ne $false) {
        throw "sample feature must keep hazardDrivenCollapse=false in P8-A."
    }
}

$riskFront = Read-Json "Assets/Data/P8/risk_front_visualization_config.json"
Assert-AllowedSourceMode $riskFront.sourceMode "risk front config"
Assert-EvidenceSourceId $riskFront "risk front config"
Assert-FieldSeparation $riskFront "risk front config"
Assert-VisualHeightGuard $riskFront "risk front config"
if ([bool]$riskFront.riskFrontEnabledInP8A -ne $false) {
    throw "risk front config must keep riskFrontEnabledInP8A=false."
}
if ([bool]$riskFront.manualSampleIsOfficial -ne $false) {
    throw "risk front config manualSampleIsOfficial must be false."
}
Assert-ManualSampleMarked $riskFront "risk front config"

$infrastructure = Read-Json "Assets/Data/P8/infrastructure_hazard_interaction_config.json"
Assert-AllowedSourceMode $infrastructure.sourceMode "infrastructure config"
Assert-EvidenceSourceId $infrastructure "infrastructure config"
if ([bool]$infrastructure.interactionEnabledInP8A -ne $false) {
    throw "infrastructure config must keep interactionEnabledInP8A=false."
}
if ([bool]$infrastructure.collapseProxyEnabledInP8A -ne $false -or
    [bool]$infrastructure.collapseGameplayEnabledInP8A -ne $false -or
    [bool]$infrastructure.hazardDrivenCollapse -ne $false) {
    throw "infrastructure config must keep collapse proxy behavior disabled in P8-A."
}
if ([bool]$infrastructure.manualSampleIsOfficial -ne $false) {
    throw "infrastructure config manualSampleIsOfficial must be false."
}
Assert-NumberRange $infrastructure.collapseProbability 0 1 "infrastructure config collapseProbability"
Assert-ManualSampleMarked $infrastructure "infrastructure config"

Write-Host "P8-A hazard JSON validation: PASS"
exit 0
