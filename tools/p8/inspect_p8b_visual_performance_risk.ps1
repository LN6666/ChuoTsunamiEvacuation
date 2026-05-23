[CmdletBinding()]
param(
    [string]$ConfigPath = "",
    [switch]$TreatWarningsAsErrors
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

$recommendedMaxSegmentCount = 256
$hardMaxSegmentCount = 1024
$recommendedMaxTransparentLayerCount = 2
$recommendedMaxLightCurtainObjectCount = 16
$recommendedMaxParticleCount = 1000

function Resolve-ConfigPath {
    if (-not [string]::IsNullOrWhiteSpace($ConfigPath)) {
        if ([System.IO.Path]::IsPathRooted($ConfigPath)) {
            return $ConfigPath
        }

        return (Join-Path $repoRoot ($ConfigPath -replace "/", "\"))
    }

    return Join-Path $repoRoot "Assets\Data\P8\risk_front_visualization_config.json"
}

function Add-Warning {
    param(
        [System.Collections.Generic.List[string]]$Warnings,
        [string]$Message
    )

    $Warnings.Add($Message) | Out-Null
}

function Test-HighRiskMaterial {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $false
    }

    return $Value.IndexOf("expensive", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $Value.IndexOf("complex", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $Value.IndexOf("multi_pass", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $Value.IndexOf("unbounded", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $Value.IndexOf("high", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}

function Inspect-Settings {
    param([object]$Settings)

    $warnings = New-Object 'System.Collections.Generic.List[string]'

    if ($null -eq $Settings) {
        Add-Warning $warnings "Performance settings are missing; P8-B visual implementation must define bounded defaults."
        return $warnings
    }

    if ([int]$Settings.segmentCount -gt $recommendedMaxSegmentCount) {
        Add-Warning $warnings "excessive segment counts: segmentCount=$($Settings.segmentCount) exceeds recommended $recommendedMaxSegmentCount."
    }

    if ([int]$Settings.segmentCount -gt $hardMaxSegmentCount) {
        Add-Warning $warnings "excessive segment counts: segmentCount=$($Settings.segmentCount) exceeds hard guard $hardMaxSegmentCount."
    }

    if ([bool]$Settings.rebuildsMeshEveryFrame -eq $true) {
        Add-Warning $warnings "expensive per-frame mesh rebuild: rebuildsMeshEveryFrame=true."
    }

    if ([int]$Settings.transparentLayerCount -gt $recommendedMaxTransparentLayerCount) {
        Add-Warning $warnings "high transparency overdraw: transparentLayerCount=$($Settings.transparentLayerCount)."
    }

    if ([int]$Settings.lightCurtainObjectCount -gt $recommendedMaxLightCurtainObjectCount) {
        Add-Warning $warnings "too many light curtain objects: lightCurtainObjectCount=$($Settings.lightCurtainObjectCount)."
    }

    if ([bool]$Settings.particlesAreBounded -ne $true) {
        Add-Warning $warnings "unbounded particle usage: particlesAreBounded must be true."
    }

    if ([int]$Settings.maxParticleCount -gt $recommendedMaxParticleCount) {
        Add-Warning $warnings "unbounded particle usage risk: maxParticleCount=$($Settings.maxParticleCount) exceeds $recommendedMaxParticleCount."
    }

    if ((Test-HighRiskMaterial ([string]$Settings.materialMode)) -or
        (Test-HighRiskMaterial ([string]$Settings.shaderRisk))) {
        Add-Warning $warnings "material/shader risk: materialMode=$($Settings.materialMode), shaderRisk=$($Settings.shaderRisk)."
    }

    if ([bool]$Settings.hasCullingStrategy -ne $true) {
        Add-Warning $warnings "lack of culling strategy: hasCullingStrategy must be true."
    }

    if ([bool]$Settings.hasEnableDisableStrategy -ne $true) {
        Add-Warning $warnings "lack of enable/disable strategy: hasEnableDisableStrategy must be true."
    }

    if ([bool]$Settings.usesSharedMeshOrInstancePool -ne $true) {
        Add-Warning $warnings "too many unique allocations risk: usesSharedMeshOrInstancePool should be true."
    }

    return $warnings
}

Write-Host "P8-B visual performance risk inspection"

$resolvedConfigPath = Resolve-ConfigPath
if (-not (Test-Path -LiteralPath $resolvedConfigPath -PathType Leaf)) {
    throw "Missing P8-B risk-front config: $resolvedConfigPath"
}

$config = Get-Content -Raw -LiteralPath $resolvedConfigPath | ConvertFrom-Json
$warnings = Inspect-Settings $config.performanceSettings

foreach ($warning in $warnings) {
    Write-Host "WARN: $warning"
}

if ($warnings.Count -gt 0) {
    Write-Host "P8-B visual performance risk inspection: WARN ($($warnings.Count) warning(s))"
    if ($TreatWarningsAsErrors) {
        exit 1
    }

    exit 0
}

Write-Host "P8-B visual performance risk inspection: PASS"
exit 0
