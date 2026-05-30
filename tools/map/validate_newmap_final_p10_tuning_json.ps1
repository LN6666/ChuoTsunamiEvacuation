param()

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Require-Path {
    param([string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

$required = @(
    "Assets\Data\P10\newmap_player_stamina_config.json",
    "Assets\Data\P10\newmap_final_stamina_sprint_tuning_report.json",
    "Assets\Data\P10\newmap_final_p10_tuning_player_report.json",
    "Assets\Data\P10\p10_to_p11_handoff_readiness.json",
    "docs\NEWMAP_FINAL_STAMINA_SPRINT_TUNING.md",
    "docs\NEWMAP_FINAL_P10_TUNING_PLAYER_REPORT.md",
    "docs\P10_TO_P11_HANDOFF_READINESS.md",
    "docs\GAME_RULES_EN.md",
    "docs\GAME_RULES_JA.md",
    "docs\NEWMAP_MANUAL_PLAYTEST_CHECKLIST.md",
    "docs\NEWMAP_MANUAL_PLAYTEST_READINESS.md"
)

foreach ($relativePath in $required) {
    Require-Path $relativePath
}

$report = Read-Json "Assets\Data\P10\newmap_final_stamina_sprint_tuning_report.json"
if ([double]$report.newMaxStamina -ne 3500.0) { throw "Final tuning report newMaxStamina must be 3500." }
if ([double]$report.newSprintSpeedMetersPerSecond -ne 4.59) { throw "Final tuning report sprint speed must be 4.59." }
if ([double]$report.actualSprintReductionPercent -ne 20.0) { throw "Sprint reduction must be 20%." }
if (-not [bool]$report.tourismModeStaminaDisabled) { throw "Tourism Mode stamina disabled was not recorded." }

$playerReport = Read-Json "Assets\Data\P10\newmap_final_p10_tuning_player_report.json"
if ([string]::IsNullOrWhiteSpace([string]$playerReport.buildPath)) { throw "Final P10 player report buildPath is missing." }

$handoff = Read-Json "Assets\Data\P10\p10_to_p11_handoff_readiness.json"
if ([double]$handoff.stamina -ne 3500.0) { throw "P10 to P11 handoff stamina must be 3500." }
if ([double]$handoff.sprintSpeedReductionPercent -ne 20.0) { throw "P10 to P11 handoff sprint reduction must be 20%." }

Write-Host "[PASS] NewMap final P10 tuning JSON validation passed."
