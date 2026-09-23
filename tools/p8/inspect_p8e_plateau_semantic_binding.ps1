[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$dataPath = Join-Path $repoRoot "Assets\Data\P8\p8e_semantic_binding_v1.json"

if (-not (Test-Path -LiteralPath $dataPath -PathType Leaf)) {
    throw "Missing P8-E semantic binding data: Assets/Data/P8/p8e_semantic_binding_v1.json"
}

$data = Get-Content -Raw -Encoding UTF8 -LiteralPath $dataPath | ConvertFrom-Json
if ($data.datasetId -ne "p8e_semantic_binding_v1") {
    throw "Unexpected semantic binding datasetId: $($data.datasetId)"
}

$required = @(
    "road",
    "building",
    "bridge",
    "underground",
    "entrance",
    "waterfront",
    "shelter_proxy",
    "navigation_target_proxy",
    "humanitarian_candidate_proxy",
    "highrise_candidate_marker"
)
$allowedModes = @("actual_scene_object", "plateau_metadata", "proxy_marker", "data_only", "blocked")
$bindings = @($data.bindings)

foreach ($category in $required) {
    $match = @($bindings | Where-Object { $_.category -eq $category })
    if ($match.Count -ne 1) {
        throw "Expected exactly one semantic binding for category '$category'. Found $($match.Count)."
    }
}

foreach ($binding in $bindings) {
    if ($allowedModes -notcontains $binding.bindingMode) {
        throw "Invalid bindingMode '$($binding.bindingMode)' for $($binding.category)."
    }
    if ([string]::IsNullOrWhiteSpace($binding.p9Limitations)) {
        throw "Missing P9 limitation for $($binding.category)."
    }
    if ($binding.category -in @("underground", "entrance") -and [string]::IsNullOrWhiteSpace($binding.blocker)) {
        throw "$($binding.category) must document the semantic-binding blocker."
    }
}

if ([bool]$data.completeSceneSemanticBinding -ne $false -or [bool]$data.sceneMutationPerformed -ne $false) {
    throw "P8-E semantic binding must not claim complete scene binding or scene mutation."
}

Write-Host "P8-E semantic binding inspection: PASS" -ForegroundColor Green
