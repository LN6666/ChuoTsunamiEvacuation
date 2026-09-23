[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

function Read-JsonFile {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing JSON file: $RelativePath"
    }
    try {
        return Get-Content -Raw -Encoding UTF8 -LiteralPath $path | ConvertFrom-Json
    }
    catch {
        throw "Invalid JSON in $RelativePath. $($_.Exception.Message)"
    }
}

function Assert-BooleanTrue {
    param([object]$Value, [string]$Context)
    if (-not [bool]$Value) { throw "$Context must be true." }
}

function Assert-BooleanFalse {
    param([object]$Value, [string]$Context)
    if ([bool]$Value) { throw "$Context must be false." }
}

function Assert-RequiredText {
    param([string]$Value, [string]$Context)
    if ([string]::IsNullOrWhiteSpace($Value)) { throw "$Context is required." }
}

function Get-LocalizationValue {
    param([object]$Table, [string]$Key)
    foreach ($entry in @($Table.entries)) {
        if ($entry.key -eq $Key) { return [string]$entry.value }
    }
    return ""
}

function Test-Localization {
    $en = Read-JsonFile "Assets/Data/P10/p10b_plus_localization_en.json"
    $ja = Read-JsonFile "Assets/Data/P10/p10b_plus_localization_ja.json"
    if ($en.language -ne "en") { throw "English localization language must be en." }
    if ($ja.language -ne "ja") { throw "Japanese localization language must be ja." }
    foreach ($key in @(
        "game.title",
        "menu.start",
        "menu.rules",
        "pause.title",
        "rules.body",
        "warning.non_official"
    )) {
        Assert-RequiredText (Get-LocalizationValue $en $key) "English localization $key"
        Assert-RequiredText (Get-LocalizationValue $ja $key) "Japanese localization $key"
    }
    if ((Get-LocalizationValue $en "warning.non_official") -notlike "*Not an official evacuation shelter*") {
        throw "English non-official warning must preserve shelter warning."
    }
}

function Test-UiConfig {
    $config = Read-JsonFile "Assets/Data/P10/p10b_plus_ui_config.json"
    Assert-BooleanTrue $config.startMenuEnabled "startMenuEnabled"
    Assert-BooleanTrue $config.pauseMenuEnabled "pauseMenuEnabled"
    Assert-BooleanTrue $config.rulesPanelScrollable "rulesPanelScrollable"
    Assert-BooleanTrue $config.wrapDynamicText "wrapDynamicText"
    Assert-BooleanFalse $config.unlicensedInternetImageCommitted "unlicensedInternetImageCommitted"
    if ([double]$config.backgroundOpacity -lt 0.3 -or [double]$config.backgroundOpacity -gt 0.4) {
        throw "backgroundOpacity should stay around 0.3 to 0.4."
    }
    if (-not [string]::IsNullOrWhiteSpace([string]$config.backgroundImagePath)) {
        Assert-RequiredText $config.backgroundSourceUrl "backgroundSourceUrl"
        Assert-RequiredText $config.backgroundLicense "backgroundLicense"
        Assert-RequiredText $config.backgroundAuthorOrProvider "backgroundAuthorOrProvider"
    }
}

function Test-Weather {
    $config = Read-JsonFile "Assets/Data/P10/p10b_plus_weather_config.json"
    $map = @{}
    foreach ($mode in @($config.modes)) { $map[$mode.modeId] = [double]$mode.movementSpeedMultiplier }
    if ($map["clear_day"] -ne 1.0) { throw "clear_day multiplier must be 1.0." }
    if ($map["rainy_day"] -ne 0.75) { throw "rainy_day multiplier must be 0.75." }
    if ($map["night_clear"] -ne 0.85) { throw "night_clear multiplier must be 0.85." }
    if ($map["night_rain"] -ne 0.65) { throw "night_rain multiplier must be 0.65." }
}

function Test-MovementStamina {
    $config = Read-JsonFile "Assets/Data/P10/p10b_plus_movement_stamina_config.json"
    if ([double]$config.normalWalkSpeedMetersPerSecond -lt 0.45 -or [double]$config.normalWalkSpeedMetersPerSecond -gt 0.55) {
        throw "normal walk speed should be about 0.5 m/s."
    }
    if ([double]$config.sprintSpeedMetersPerSecond -lt 2.0 -or [double]$config.sprintSpeedMetersPerSecond -gt 3.0) {
        throw "sprint speed should be 2.0-3.0 m/s."
    }
    if ([double]$config.exhaustionLockSeconds -ne 15.0) { throw "exhaustion lock must be 15 seconds." }
    if ([double]$config.recovery30Seconds -ne 15.0) { throw "30 percent recovery must be at 15 seconds." }
    if ([double]$config.recovery50Seconds -ne 45.0) { throw "50 percent recovery must be at 45 seconds." }
    if ([double]$config.recovery100Seconds -ne 90.0) { throw "100 percent recovery must be at 90 seconds." }
}

function Test-AvatarMobility {
    $config = Read-JsonFile "Assets/Data/P10/p10b_plus_avatar_mobility_config.json"
    Assert-BooleanTrue $config.avatarPresentationRandomizedFiftyFifty "avatarPresentationRandomizedFiftyFifty"
    Assert-BooleanTrue $config.mobilityProfileSeparateFromAvatarPresentation "mobilityProfileSeparateFromAvatarPresentation"
    Assert-BooleanFalse $config.enableGenderSpeedModifier "enableGenderSpeedModifier"
    if ([double]$config.femalePresentationSpeedMultiplier -ne 0.85) {
        throw "femalePresentationSpeedMultiplier should remain optional scenario value 0.85."
    }
    if (($config.genderSpeedModifierPolicy -as [string]) -notlike "*not_real_world_claim*") {
        throw "genderSpeedModifierPolicy must document not_real_world_claim."
    }
    $profileIds = @($config.mobilityProfiles | ForEach-Object { $_.profileId })
    foreach ($required in @("standard", "cautious", "carrying_load", "injured", "elderly", "custom_slow")) {
        if ($profileIds -notcontains $required) { throw "Missing mobility profile $required." }
    }
}

function Test-ManualChecklist {
    $checklist = Read-JsonFile "Assets/Data/P10/p10b_plus_manual_playtest_checklist.json"
    Assert-BooleanTrue $checklist.finalWindowsExeBuildDeferredToP10C "finalWindowsExeBuildDeferredToP10C"
    if (@($checklist.checklist).Count -lt 10) {
        throw "Manual checklist must contain detailed P10-B+ checks."
    }
}

Write-Host "P10-B+ JSON validation: starting"
Test-Localization
Test-UiConfig
Test-Weather
Test-MovementStamina
Test-AvatarMobility
Test-ManualChecklist
Write-Host "P10-B+ JSON validation: PASS" -ForegroundColor Green
exit 0
