$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Require-File {
    param([string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing required file: $RelativePath"
        exit 1
    }
}

function Require-Contains {
    param([string]$RelativePath, [string]$Needle)
    $path = Join-Path $ProjectRoot $RelativePath
    $text = Get-Content -Encoding UTF8 -LiteralPath $path -Raw
    if ($text -notlike "*$Needle*") {
        Write-Host "[FAIL] $RelativePath did not contain required text: $Needle"
        exit 1
    }
}

$required = @(
    "Assets\Data\P10\newmap_spawn_config.json",
    "Assets\Data\P10\newmap_player_stamina_config.json",
    "Assets\Data\P10\newmap_tsunami_mode_hotfix_config.json",
    "Assets\Scripts\NewMap\NewMapGameController.cs",
    "Assets\Scripts\NewMap\NewMapHazardController.cs",
    "Assets\Scripts\NewMap\NewMapPlayerController.cs",
    "Assets\Scripts\NewMap\NewMapBuildingEntryTrigger.cs",
    "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs",
    "docs\P10_NEWMAP_TSUNAMI_MODE_HOTFIX_REPORT.md",
    "review_prompts\deepseek_review_p10_newmap_tsunami_mode_hotfix.md"
)

foreach ($path in $required) {
    Require-File $path
}

Require-Contains "Assets\Data\P10\newmap_player_stamina_config.json" '"staminaMultiplier": 100.0'
Require-Contains "Assets\Data\P10\newmap_tsunami_mode_hotfix_config.json" '"tsunamiWarningDurationSeconds": 300.0'
Require-Contains "Assets\Data\P10\newmap_tsunami_mode_hotfix_config.json" '"warningPhaseSeconds": 300.0'
Require-Contains "Assets\Data\P10\newmap_tsunami_mode_hotfix_config.json" '"tsunamiStartSide": "south"'
Require-Contains "Assets\Data\P10\newmap_tsunami_mode_hotfix_config.json" '"curtainHeightMeters": 1000.0'
Require-Contains "Assets\Data\P10\newmap_spawn_config.json" '"deterministicSeedEnabled": false'
Require-Contains "Assets\Scripts\NewMap\NewMapGameController.cs" "NotifyBuildingEntryTouch"
Require-Contains "Assets\Scripts\NewMap\NewMapGameController.cs" "warning_duration_seconds"
Require-Contains "Assets\Scripts\NewMap\NewMapRuntimeUI.cs" "Press E to enter building"
Require-Contains "Assets\Scripts\NewMap\NewMapRuntimeUI.cs" "ResultRetryButton"
Require-Contains "Assets\Scripts\NewMap\NewMapHazardController.cs" "GetFloodedSideSamplePointForDiagnostics"
Require-Contains "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs" "P10_BoundaryAirWall_North"

$forbiddenDirs = @(
    "Assets\Data\P10-E",
    "Assets\Data\P10-F",
    "Assets\Data\P10-G",
    "docs\P10-E",
    "docs\P10-F",
    "docs\P10-G"
)

foreach ($relative in $forbiddenDirs) {
    if (Test-Path -LiteralPath (Join-Path $ProjectRoot $relative)) {
        Write-Host "[FAIL] Forbidden P10 subphase path exists: $relative"
        exit 1
    }
}

Write-Host "[PASS] P10 NewMap tsunami-mode hotfix preflight passed."
