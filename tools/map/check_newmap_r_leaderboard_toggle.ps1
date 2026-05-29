param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Require-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

$config = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_leaderboard_toggle_config.json") -Raw | ConvertFrom-Json
$controller = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapGameController.cs") -Raw
$ui = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeUI.cs") -Raw
$lines = Get-Content -Encoding UTF8 -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapShelterDirectLineController.cs") -Raw

Require-Condition ([bool]$config.enabled) "Leaderboard toggle disabled."
Require-Condition ([string]$config.toggleKey -eq "R") "Leaderboard toggle key must be R."
Require-Condition ([bool]$config.hideWhenMenuOpen) "Leaderboard toggle must be gated while menu is open."
Require-Condition ([bool]$config.preserveWarnings) "Official/non-official warning preservation disabled."
Require-Condition ($controller -match "Input\.GetKeyDown\(KeyCode\.R\)" -and $controller -match "ToggleShelterRankingPanelFromInput") "R key toggle handler missing."
Require-Condition ($controller -match "ui\.HideShelterRanking" -and $controller -match "ui\.ShowShelterRanking") "Show/hide behavior missing."
Require-Condition ($controller -match "IsPauseVisible" -and $controller -match "IsRulesVisible" -and $controller -match "IsResultVisible") "R toggle must be gated by modal UI."
Require-Condition ($ui -match "IsShelterRankingVisible" -and $ui -match "HideShelterRanking") "Ranking UI visibility APIs missing."
Require-Condition ($lines -match "No active targets" -and $lines -match "estimated prototype guidance only") "Ranking panel wording missing."

Write-Host "[PASS] NewMap R leaderboard toggle checks passed."
exit 0
