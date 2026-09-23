param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"
$oldPatterns = @("P7_HighDetail", "P10RealMap", "LowSpec", "P7HighDetail")
$activeFiles = @(
    "ProjectSettings\EditorBuildSettings.asset",
    "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json",
    "Assets\Data\P10\newmap_target_remap_status.json"
)

$failures = @()
foreach ($relativePath in $activeFiles) {
    $path = Join-Path $ProjectRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        continue
    }

    $text = Get-Content -LiteralPath $path -Raw
    foreach ($pattern in $oldPatterns) {
        if ($text -match [regex]::Escape($pattern)) {
            $failures += "$relativePath contains active old-map reference: $pattern"
        }
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] No old P7/P10RealMap/LowSpec references found in active scene target files."
exit 0
