param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing $RelativePath"
        exit 1
    }
    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
}

function Require-Close {
    param([double]$Actual, [double]$Expected, [double]$Tolerance, [string]$Message)
    if ([math]::Abs($Actual - $Expected) -gt $Tolerance) {
        Write-Host "[FAIL] $Message actual=$Actual expected=$Expected"
        exit 1
    }
}

function Require-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

$stamina = Read-Json "Assets\Data\P10\newmap_player_stamina_config.json"
$tsunami = Read-Json "Assets\Data\P10\newmap_tsunami_mode_hotfix_config.json"
$staminaReport = Read-Json "Assets\Data\P10\newmap_stamina_final_tuning_report.json"
$sprintReport = Read-Json "Assets\Data\P10\newmap_sprint_speed_final_tuning_report.json"
$warningReport = Read-Json "Assets\Data\P10\newmap_tsunami_warning_final_tuning_report.json"

$finalStamina = [double]$stamina.baselineMaxStamina * [double]$stamina.staminaMultiplier
$finalSprint = 5.0 * [double]$stamina.sprintSpeedMultiplierAdditional

Require-Close $finalStamina 13000.0 0.001 "Evacuation max stamina must be reduced 35% to 13000."
Require-Close ([double]$stamina.staminaMultiplier) 130.0 0.001 "Stamina multiplier must be 130."
Require-Close $finalSprint 5.7375 0.0001 "Evacuation sprint speed must be reduced 15% to 5.7375."
Require-Close ([double]$stamina.sprintSpeedMultiplierAdditional) 1.1475 0.0001 "Sprint additional multiplier must be 1.1475."
Require-Close ([double]$tsunami.tsunamiWarningDurationSeconds) 180.0 0.001 "Tsunami warning duration must be 180 seconds."
Require-Close ([double]$tsunami.warningPhaseSeconds) 180.0 0.001 "Warning phase seconds must be 180."
Require-Close ([double]$staminaReport.newFinalStamina) 13000.0 0.001 "Stamina report new final stamina mismatch."
Require-Close ([double]$sprintReport.newSprintSpeedMetersPerSecond) 5.7375 0.0001 "Sprint report new speed mismatch."
Require-Close ([double]$warningReport.newWarningDurationSeconds) 180.0 0.001 "Warning report new duration mismatch."
Require-True ([bool]$staminaReport.tourismModeStaminaDisabled) "Tourism Mode stamina disabled must remain true."
Require-True ([bool]$staminaReport.evacuationModeStaminaEnabled) "Evacuation Mode stamina enabled must remain true."
Require-True ([bool]$warningReport.tourismModeDisablesTsunami) "Tourism Mode tsunami disabled must remain true."
Require-True ([bool]$warningReport.evacuationModeUsesThreeMinuteWarning) "Evacuation Mode must use the 3-minute warning."

Write-Host "[PASS] NewMap stamina/sprint/warning tuning validated."
exit 0
