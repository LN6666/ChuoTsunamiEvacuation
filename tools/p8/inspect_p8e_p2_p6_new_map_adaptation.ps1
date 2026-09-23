[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$matrixPath = Join-Path $repoRoot "Assets\Data\P8\p8e_p2_p6_new_map_adaptation_matrix.json"

if (-not (Test-Path -LiteralPath $matrixPath -PathType Leaf)) {
    throw "Missing P8-E P2-P6 matrix: Assets/Data/P8/p8e_p2_p6_new_map_adaptation_matrix.json"
}

$matrix = Get-Content -Raw -Encoding UTF8 -LiteralPath $matrixPath | ConvertFrom-Json
if ($matrix.datasetId -ne "p8e_p2_p6_new_map_adaptation_matrix_v1") {
    throw "Unexpected P2-P6 matrix datasetId: $($matrix.datasetId)"
}

$allowedStatuses = @("passed", "proxy_ready", "blocked", "p9_final_gameplay_required")
$items = @($matrix.items)
if ($items.Count -lt 12) {
    throw "P2-P6 matrix is too small: $($items.Count) items."
}

foreach ($item in $items) {
    if ($allowedStatuses -notcontains $item.status) {
        throw "Invalid P2-P6 status '$($item.status)' for $($item.system) / $($item.item)."
    }
    if ([string]::IsNullOrWhiteSpace($item.evidence) -or [string]::IsNullOrWhiteSpace($item.p9Boundary)) {
        throw "P2-P6 item $($item.system) / $($item.item) must include evidence and P9 boundary."
    }
}

if ([bool]$matrix.successFailureRulesChangedInP8 -ne $false -or [bool]$matrix.implementsP9Gameplay -ne $false) {
    throw "P8-E P2-P6 matrix must not change success/failure rules or implement P9 gameplay."
}

Write-Host "P8-E P2-P6 new-map adaptation inspection: PASS" -ForegroundColor Green
