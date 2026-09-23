param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$failures = @()
$oldPatterns = @("P7_HighDetail", "P7HighDetail", "P10RealMap", "LowSpec")
$activeFiles = @(
    "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs",
    "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs",
    "Assets\Data\P10\newmap_p2_p10_full_completion_matrix.json",
    "Assets\Data\P10\newmap_active_target_final_report.json",
    "Assets\Data\P10\newmap_disabled_targets_final.json",
    "Assets\Data\P10\newmap_manual_playtest_readiness.json",
    "Assets\Data\P10\newmap_hardening_player_build_report.json"
)

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

$typesPath = Join-Path $ProjectRoot "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs"
$typesText = Get-Content -LiteralPath $typesPath -Raw
if ($typesText -notmatch "Assets/Scenes/Chuo_BaseMap\.unity") {
    $failures += "Runtime scene constant does not point to Assets/Scenes/Chuo_BaseMap.unity"
}

$buildUtilityPath = Join-Path $ProjectRoot "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"
$buildUtilityText = Get-Content -LiteralPath $buildUtilityPath -Raw
if ($buildUtilityText -notmatch "BuildNewMapHardeningTempPlayerCommandLine" -or $buildUtilityText -notmatch "NewMapHardeningPre") {
    $failures += "Hardening temp player build utility is not configured for NewMapHardeningPre."
}

$p10Forbidden = @(Get-ChildItem -LiteralPath (Join-Path $ProjectRoot "docs") -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "P10[-_](E|F|G)([-_.]|$)" })
if ($p10Forbidden.Count -gt 0) {
    $failures += "Forbidden P10-E/F/G docs exist: $($p10Forbidden.Name -join ', ')"
}

$archives = @(Get-ChildItem -LiteralPath $ProjectRoot -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object {
        $_.Name -match "\.(zip|7z|tar|tar\.gz)$" -and
        $_.FullName -notmatch "\\data_pipeline\\downloads\\" -and
        $_.FullName -notmatch "\\\.venv\\" -and
        $_.FullName -notmatch "\\Library\\" -and
        $_.FullName -notmatch "\\\.git\\"
    })
if ($archives.Count -gt 0) {
    $failures += "Release/archive artifact found in workspace: $($archives[0].FullName)"
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Active hardening files avoid deprecated scene refs, P10-E/F/G, and release/archive artifacts."
exit 0
