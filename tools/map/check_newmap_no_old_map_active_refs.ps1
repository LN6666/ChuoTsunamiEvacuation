param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$oldPatterns = @("P7_HighDetail", "P7HighDetail", "P10RealMap", "LowSpec")
$activeFiles = @(
    "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs",
    "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs",
    "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json",
    "Assets\Data\P10\newmap_active_target_final_report.json",
    "Assets\Data\P10\newmap_disabled_targets_final.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json"
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

$sceneConstantPath = Join-Path $ProjectRoot "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs"
$sceneConstantText = Get-Content -LiteralPath $sceneConstantPath -Raw
if ($sceneConstantText -notmatch "Assets/Scenes/Chuo_BaseMap\.unity") {
    $failures += "NewMap runtime scene constant does not point to Assets/Scenes/Chuo_BaseMap.unity"
}

$buildUtilityPath = Join-Path $ProjectRoot "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"
$buildUtilityText = Get-Content -LiteralPath $buildUtilityPath -Raw
if ($buildUtilityText -notmatch "BuildNewMapFinalTempPlayerCommandLine" -or $buildUtilityText -notmatch "NewMapFinalPre") {
    $failures += "Final temp player build utility is not configured for NewMapFinalPre."
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Active NewMap files do not reference deprecated old-map targets."
exit 0
