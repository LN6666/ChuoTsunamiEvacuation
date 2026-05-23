[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$allowedSourceModes = @("test", "evidence_planned", "manual_sample")
$evidenceRequiredSourceModes = @("evidence_planned", "manual_sample")
$allowedGeometryTypes = @("grid", "polygon", "polyline", "point", "synthetic")
$allowedBoundaryKinds = @("evidence_based", "prototype")
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
    if ($SourceMode -like "*official*" -or $SourceMode -like "*authoritative*") {
        throw "$Label sourceMode must not claim official or authoritative hazard values: $SourceMode"
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

function Assert-AllowedGeometryType {
    param(
        [object]$Value,
        [string]$Label
    )

    Assert-NonEmptyString $Value "$Label geometryType"
    if ($allowedGeometryTypes -notcontains ([string]$Value)) {
        throw "$Label geometryType must be one of: $($allowedGeometryTypes -join ', ')"
    }
}

function Assert-AllowedBoundaryKind {
    param(
        [object]$Value,
        [string]$Label
    )

    Assert-NonEmptyString $Value "$Label boundaryIsEvidenceBasedOrPrototype"
    if ($allowedBoundaryKinds -notcontains ([string]$Value)) {
        throw "$Label boundaryIsEvidenceBasedOrPrototype must be evidence_based or prototype."
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

    foreach ($scienceField in @("tsunamiHeightMeters", "waterLevelMeters", "inundationDepthMeters")) {
        if ($Object.PSObject.Properties.Name -contains $scienceField) {
            $scienceValue = [double]$Object.$scienceField
            if ([double]$Object.visualHeightMeters -gt [Math]::Max(10.0, $scienceValue) -and
                [bool]$Object.visualHeightIsCinematicOnly -ne $true) {
                throw "$Label visualHeightMeters exceeds $scienceField and must be cinematic-only."
            }
        }
    }
}

function Assert-ManualSampleNotOfficial {
    param(
        [object]$Object,
        [string]$Label
    )

    if ($Object.PSObject.Properties.Name -contains "manualSampleIsOfficial" -and
        [bool]$Object.manualSampleIsOfficial -ne $false) {
        throw "$Label manualSampleIsOfficial must be false."
    }

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

function Assert-RiskFrontConfig {
    param([object]$Config)

    foreach ($field in @(
        "scenarioId",
        "sourceMode",
        "configVersion",
        "hazardLayerVersion",
        "geometryType",
        "visualHeightMeters",
        "visualHeightIsCinematicOnly",
        "boundaryIsEvidenceBasedOrPrototype",
        "visualLayerPurpose",
        "p8bVisualSceneObjectsImplemented",
        "p8cInfrastructureInteractionImplemented",
        "p8dCollapseProxyGameplayImplemented",
        "performanceSettings"
    )) {
        Assert-HasProperty $Config $field "risk front config"
    }

    Assert-AllowedSourceMode $Config.sourceMode "risk front config"
    Assert-EvidenceSourceId $Config "risk front config"
    Assert-AllowedGeometryType $Config.geometryType "risk front config"
    Assert-AllowedBoundaryKind $Config.boundaryIsEvidenceBasedOrPrototype "risk front config"
    Assert-FieldSeparation $Config "risk front config"
    Assert-VisualHeightGuard $Config "risk front config"
    Assert-ManualSampleNotOfficial $Config "risk front config"

    if ([string]$Config.visualLayerPurpose -notmatch "cinematic") {
        throw "risk front config visualLayerPurpose must explicitly say the visual layer is cinematic."
    }
    if ([bool]$Config.riskFrontEnabledInP8A -ne $false) {
        throw "risk front config must keep riskFrontEnabledInP8A=false."
    }
    if ([bool]$Config.p8bVisualSceneObjectsImplemented -ne $false) {
        throw "P8-B guard task must not implement visual scene objects."
    }
    if ([bool]$Config.p8cInfrastructureInteractionImplemented -ne $false -or
        [bool]$Config.p8dCollapseProxyGameplayImplemented -ne $false) {
        throw "P8-B guard config must not include P8-C infrastructure or P8-D collapse gameplay behavior."
    }

    $notes = [string]$Config.notes
    foreach ($fragment in @("cinematic", "not physical tsunami height", "not official")) {
        if ($notes.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "risk front config notes must include: $fragment"
        }
    }
}

function Assert-HazardLayerStillSeparated {
    param([object]$Hazard)

    Assert-AllowedSourceMode $Hazard.sourceMode "hazard layer"
    Assert-FieldSeparation $Hazard "hazard layer"
    if ($Hazard.features.Count -lt 1) {
        throw "hazard layer must contain at least one feature for P8-B validation."
    }

    foreach ($feature in $Hazard.features) {
        Assert-AllowedSourceMode $feature.sourceMode "hazard feature"
        Assert-AllowedGeometryType $feature.geometryType "hazard feature $($feature.featureId)"
        Assert-AllowedBoundaryKind $feature.boundaryIsEvidenceBasedOrPrototype "hazard feature $($feature.featureId)"
        Assert-EvidenceSourceId $feature "hazard feature $($feature.featureId)"
        Assert-VisualHeightGuard $feature "hazard feature $($feature.featureId)"
        Assert-NumberRange $feature.hazardIntensity 0 1 "hazard feature hazardIntensity"
        Assert-NumberRange $feature.confidence 0 1 "hazard feature confidence"
        Assert-NumberRange $feature.collapseProbability 0 1 "hazard feature collapseProbability"

        if ([bool]$feature.hazardDrivenCollapse -ne $false) {
            throw "hazard feature $($feature.featureId) must keep hazardDrivenCollapse=false until P8-D."
        }
        if ([string]$feature.collapseProxyState -notin @("disabled", "data_only")) {
            throw "hazard feature $($feature.featureId) collapseProxyState must remain disabled or data_only."
        }
    }
}

function Assert-InfrastructureRemainsDataOnly {
    param([object]$Config)

    Assert-AllowedSourceMode $Config.sourceMode "infrastructure config"
    Assert-EvidenceSourceId $Config "infrastructure config"
    Assert-ManualSampleNotOfficial $Config "infrastructure config"

    foreach ($field in @(
        "roadInteractionMode",
        "buildingInteractionMode",
        "bridgeInteractionMode",
        "undergroundInteractionMode",
        "entranceInteractionMode"
    )) {
        Assert-HasProperty $Config $field "infrastructure config"
        if ([string]$Config.$field -ne "data_only") {
            throw "P8-B must keep $field=data_only until P8-C."
        }
    }

    if ([bool]$Config.interactionEnabledInP8A -ne $false) {
        throw "infrastructure interaction must remain disabled in this guard package."
    }
    if ([bool]$Config.collapseProxyEnabledInP8A -ne $false -or
        [bool]$Config.collapseGameplayEnabledInP8A -ne $false -or
        [bool]$Config.hazardDrivenCollapse -ne $false) {
        throw "collapse proxy/gameplay must remain data-only until P8-D."
    }
    if ([string]$Config.collapseProxyState -ne "data_only") {
        throw "infrastructure collapseProxyState must remain data_only."
    }
    Assert-NumberRange $Config.collapseProbability 0 1 "infrastructure config collapseProbability"
}

Write-Host "P8-B risk-front config validation"

$riskFront = Read-Json "Assets/Data/P8/risk_front_visualization_config.json"
$hazard = Read-Json "Assets/Data/P8/tsunami_hazard_sample_chuo.json"
$infrastructure = Read-Json "Assets/Data/P8/infrastructure_hazard_interaction_config.json"

Assert-RiskFrontConfig $riskFront
Assert-HazardLayerStillSeparated $hazard
Assert-InfrastructureRemainsDataOnly $infrastructure

Write-Host "P8-B risk-front config validation: PASS"
exit 0
