[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$dataRoot = Join-Path $repoRoot "Assets\Data\P8"
$allowedSourceModes = @("test", "evidence_planned", "manual_sample")
$allowedGeometryTypes = @("grid", "polygon", "polyline", "point", "synthetic")

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

function Assert-AllowedSourceMode {
    param(
        [string]$SourceMode,
        [string]$Label
    )

    if ($allowedSourceModes -notcontains $SourceMode) {
        throw "$Label sourceMode must be one of: $($allowedSourceModes -join ', ')"
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
    "arrivalTimeSeconds",
    "inundationDepthMeters",
    "waterLevelMeters",
    "tsunamiHeightMeters",
    "visualHeightIsCinematicOnly",
    "hazardDrivenCollapse"
)

$sample = Read-Json "Assets/Data/P8/tsunami_hazard_sample_chuo.json"
Assert-HasProperty $sample "scenarioId" "sample hazard"
Assert-HasProperty $sample "sourceMode" "sample hazard"
Assert-HasProperty $sample "hazardLayerVersion" "sample hazard"
Assert-HasProperty $sample "timeOriginSeconds" "sample hazard"
Assert-HasProperty $sample "features" "sample hazard"
Assert-AllowedSourceMode $sample.sourceMode "sample hazard"

if ($sample.features.Count -lt 1) {
    throw "sample hazard must contain at least one feature."
}

foreach ($feature in $sample.features) {
    foreach ($field in @(
        "featureId",
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

    if ($allowedGeometryTypes -notcontains $feature.geometryType) {
        throw "sample feature geometryType is not allowed: $($feature.geometryType)"
    }

    Assert-VisualHeightGuard $feature "sample feature $($feature.featureId)"
}

$riskFront = Read-Json "Assets/Data/P8/risk_front_visualization_config.json"
Assert-AllowedSourceMode $riskFront.sourceMode "risk front config"
Assert-VisualHeightGuard $riskFront "risk front config"
if ([bool]$riskFront.riskFrontEnabledInP8A -ne $false) {
    throw "risk front config must keep riskFrontEnabledInP8A=false."
}

$infrastructure = Read-Json "Assets/Data/P8/infrastructure_hazard_interaction_config.json"
Assert-AllowedSourceMode $infrastructure.sourceMode "infrastructure config"
if ([bool]$infrastructure.interactionEnabledInP8A -ne $false) {
    throw "infrastructure config must keep interactionEnabledInP8A=false."
}
if ([bool]$infrastructure.collapseProxyEnabledInP8A -ne $false -or [bool]$infrastructure.hazardDrivenCollapse -ne $false) {
    throw "infrastructure config must keep collapse proxy behavior disabled in P8-A."
}

Write-Host "P8-A hazard JSON validation: PASS"
exit 0
