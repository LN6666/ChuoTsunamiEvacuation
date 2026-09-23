param()

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing JSON: $RelativePath"
    }
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Require-Close {
    param([double]$Actual, [double]$Expected, [double]$Tolerance, [string]$Message)
    if ([math]::Abs($Actual - $Expected) -gt $Tolerance) {
        throw "$Message actual=$Actual expected=$Expected"
    }
}

function Require-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw $Message
    }
}

$config = Read-Json "Assets\Data\P10\newmap_player_stamina_config.json"
$report = Read-Json "Assets\Data\P10\newmap_final_stamina_sprint_tuning_report.json"

$finalStamina = [double]$config.baselineMaxStamina * [double]$config.staminaMultiplier
$finalSprint = 5.0 * [double]$config.sprintSpeedMultiplierAdditional
$actualReduction = (1.0 - ($finalSprint / 5.7375)) * 100.0

Require-Close $finalStamina 3500.0 0.001 "Evacuation max stamina must be exactly 3500."
Require-Close ([double]$config.staminaMultiplier) 35.0 0.001 "Stamina multiplier must be 35."
Require-Close $finalSprint 4.59 0.0001 "Evacuation sprint speed must be 4.59 m/s."
Require-Close ([double]$config.sprintSpeedMultiplierAdditional) 0.918 0.0001 "Sprint multiplier must be 0.918."
Require-Close $actualReduction 20.0 0.001 "Sprint reduction from previous build must be 20%."

Require-Close ([double]$report.newMaxStamina) 3500.0 0.001 "Final tuning report stamina mismatch."
Require-Close ([double]$report.newSprintSpeedMetersPerSecond) 4.59 0.0001 "Final tuning report sprint mismatch."
Require-Close ([double]$report.actualSprintReductionPercent) 20.0 0.001 "Final tuning report sprint reduction mismatch."
Require-True ([bool]$report.tourismModeStaminaDisabled) "Tourism Mode stamina disabled must remain true."
Require-True ([bool]$report.evacuationModeStaminaEnabled) "Evacuation Mode stamina enabled must remain true."

$playerSource = Get-Content -LiteralPath (Join-Path $ProjectRoot "Assets\Scripts\NewMap\NewMapPlayerController.cs") -Raw
$runtimeTypes = Get-Content -LiteralPath (Join-Path $ProjectRoot "Assets\Scripts\NewMap\NewMapRuntimeTypes.cs") -Raw
Require-True ($playerSource -match "staminaMultiplier\s*=\s*35f") "Runtime fallback stamina multiplier still points to an old value."
Require-True ($playerSource -match "sprintSpeedMultiplierAdditional\s*=\s*0\.918f") "Runtime fallback sprint multiplier still points to an old value."
Require-True ($playerSource -match "StaminaEnabled\s*=>\s*currentMode\s*==\s*NewMapGameMode\.Evacuation") "Tourism no-stamina mode split regressed."
Require-True ($runtimeTypes -match "TourismSprintSpeed\s*=\s*10\.0f") "Tourism sprint speed must remain 10.0 m/s."

Write-Host "[PASS] NewMap stamina=3500 and sprint -20% validation passed."
