[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$configPath = Join-Path $repoRoot "Assets\Data\P8\infrastructure_damage_proxy_config.json"

if (-not (Test-Path -LiteralPath $configPath -PathType Leaf)) {
    throw "Missing P8-D damage config: Assets/Data/P8/infrastructure_damage_proxy_config.json"
}

$config = Get-Content -Raw -Encoding UTF8 -LiteralPath $configPath | ConvertFrom-Json

if ([string]$config.configId -ne "p8_infrastructure_damage_proxy_config_v1") {
    throw "Unexpected P8-D configId: $($config.configId)"
}
if ([double]$config.warningDepthMeters -lt 0 -or [double]$config.lowFloorInundationDepthMeters -le 0) {
    throw "P8-D warning and low-floor thresholds must be non-negative/positive."
}
if ([double]$config.buildingDamageDepthMeters -lt [double]$config.lowFloorInundationDepthMeters) {
    throw "Building damage threshold must be greater than or equal to low-floor threshold."
}
if ([double]$config.collapseProxyProbability -lt 0 -or [double]$config.collapseProxyProbability -gt 0.1) {
    throw "Collapse proxy probability must be low and config-controlled (0.0 to 0.1)."
}
if ([int]$config.maxCollapseProxySampleCount -lt 0 -or [int]$config.maxCollapseProxySampleCount -gt 20) {
    throw "Collapse proxy sample count must remain small."
}
if ([bool]$config.enableCollapseProxyVisual -ne $true) {
    throw "P8-D collapse proxy visual flag must be explicit."
}
if ([bool]$config.noPhysicsCollapse -ne $true -or [bool]$config.noDebrisSimulation -ne $true) {
    throw "P8-D config must explicitly disable physics collapse and debris simulation."
}
if ([bool]$config.noGameplaySuccessFailureChange -ne $true) {
    throw "P8-D config must explicitly preserve gameplay success/failure rules."
}

Write-Host "P8-D damage config validation: PASS" -ForegroundColor Green
exit 0
