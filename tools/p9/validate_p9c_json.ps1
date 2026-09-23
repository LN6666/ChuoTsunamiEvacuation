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
        return Get-Content -Raw -LiteralPath $path | ConvertFrom-Json
    }
    catch {
        throw "Invalid JSON in $RelativePath. $($_.Exception.Message)"
    }
}

function Assert-BooleanTrue {
    param([object]$Value, [string]$Context)
    if (-not [bool]$Value) {
        throw "$Context must be true."
    }
}

function Assert-BooleanFalse {
    param([object]$Value, [string]$Context)
    if ([bool]$Value) {
        throw "$Context must be false."
    }
}

function Assert-RequiredText {
    param([string]$Value, [string]$Context)
    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "$Context is required."
    }
}

function Assert-UniqueId {
    param([hashtable]$Seen, [string]$Id, [string]$Context)
    Assert-RequiredText $Id $Context
    if ($Seen.ContainsKey($Id)) {
        throw "Duplicate $Context '$Id'."
    }
    $Seen[$Id] = $true
}

function Test-OutcomeRules {
    $config = Read-JsonFile "Assets/Data/P9/p9c_outcome_rules_config.json"
    Assert-RequiredText $config.scenarioPresetId "outcome scenarioPresetId"
    Assert-BooleanTrue $config.enableOutcomeMutationProxy "outcome mutation proxy"
    Assert-BooleanTrue $config.enableEntranceBlockedFailure "entrance blocked failure"
    Assert-BooleanTrue $config.enableCrowdDelayFailure "crowd delay failure"
    Assert-BooleanTrue $config.enableHazardTimingFailure "hazard timing failure"
    Assert-BooleanFalse $config.claimsOfficialRouteStatus "claimsOfficialRouteStatus"
    Assert-BooleanFalse $config.importsExternalCrowdPackage "importsExternalCrowdPackage"
    Assert-BooleanFalse $config.reimplementsP8HazardModel "reimplementsP8HazardModel"
    if ([double]$config.maxTotalDelaySeconds -le 0) {
        throw "maxTotalDelaySeconds must be positive."
    }
}

function Test-TargetRules {
    $rules = Read-JsonFile "Assets/Data/P9/p9c_vertical_evacuation_target_rules.json"
    Assert-BooleanTrue $rules.allowLifeFirstCandidates "allowLifeFirstCandidates"
    Assert-BooleanTrue $rules.requireNonOfficialWarning "requireNonOfficialWarning"
    if (-not ($rules.nonOfficialWarningText -like "*Not an official evacuation shelter*")) {
        throw "nonOfficialWarningText must state not official."
    }
    if (-not ($rules.lifeFirstUseText -like "*Use only when official shelter access is unsafe or unavailable*")) {
        throw "lifeFirstUseText must contain life-first usage boundary."
    }

    $records = @($rules.targets)
    if ($records.Count -lt 5) {
        throw "P9-C target rules require at least five fixture targets."
    }

    $seen = @{}
    $hasOfficial = $false
    $hasEligibleLifeFirst = $false
    $hasBlocked = $false
    $hasLowFloor = $false
    $hasMissingSafeFloor = $false
    foreach ($record in $records) {
        Assert-UniqueId $seen $record.targetId "target id"
        Assert-BooleanFalse $record.safeApprovedByDefault "target $($record.targetId) safeApprovedByDefault"
        Assert-BooleanFalse $record.routeIsOfficial "target $($record.targetId) routeIsOfficial"
        Assert-BooleanTrue $record.routeIsEstimatedPrototypeGuidance "target $($record.targetId) routeIsEstimatedPrototypeGuidance"

        if ([bool]$record.isOfficialShelter) {
            $hasOfficial = $true
        }
        if ([bool]$record.isHumanitarianCandidate) {
            Assert-BooleanFalse $record.isOfficialShelter "humanitarian target $($record.targetId) isOfficialShelter"
            Assert-BooleanTrue $record.nonOfficialWarningRequired "humanitarian target $($record.targetId) nonOfficialWarningRequired"
            if ($record.safeFloorStatus -eq "available" -and $record.entranceStatus -eq "open") {
                $hasEligibleLifeFirst = $true
            }
        }
        if ($record.entranceStatus -eq "blocked") { $hasBlocked = $true }
        if ([bool]$record.lowFloorInundationWarning) { $hasLowFloor = $true }
        if (-not [bool]$record.hasSafeFloorProxy) { $hasMissingSafeFloor = $true }
    }

    Assert-BooleanTrue $hasOfficial "official target fixture"
    Assert-BooleanTrue $hasEligibleLifeFirst "eligible life-first fixture"
    Assert-BooleanTrue $hasBlocked "blocked target fixture"
    Assert-BooleanTrue $hasLowFloor "low-floor target fixture"
    Assert-BooleanTrue $hasMissingSafeFloor "missing safe-floor fixture"
}

function Test-EntranceRules {
    $rules = Read-JsonFile "Assets/Data/P9/p9c_entrance_congestion_rules.json"
    if ([double]$rules.queueDelaySecondsPerPerson -le 0) {
        throw "queueDelaySecondsPerPerson must be positive."
    }
    if ([double]$rules.maxDelayCapSeconds -le 0) {
        throw "maxDelayCapSeconds must be positive."
    }
    Assert-BooleanTrue $rules.failWhenBlocked "failWhenBlocked"
    Assert-BooleanTrue $rules.failWhenDelayExceedsSafetyWindow "failWhenDelayExceedsSafetyWindow"

    $records = @($rules.entrances)
    foreach ($required in @("open", "crowded", "blocked", "hazard_affected")) {
        if (-not ($records | Where-Object { $_.entranceStatus -eq $required })) {
            throw "Missing entrance fixture status: $required"
        }
    }
}

function Test-SafeFloorRules {
    $rules = Read-JsonFile "Assets/Data/P9/p9c_safe_floor_proxy_rules.json"
    Assert-BooleanTrue $rules.failWhenUnavailable "failWhenUnavailable"
    Assert-BooleanTrue $rules.failWhenBelowRequiredHeight "failWhenBelowRequiredHeight"
    Assert-BooleanTrue $rules.failWhenCrowdOverCapacity "failWhenCrowdOverCapacity"
    foreach ($status in @("available", "unavailable", "unknown", "below_required_height", "hazard_warning", "crowd_over_capacity")) {
        if (@($rules.supportedStatuses) -notcontains $status) {
            throw "Missing safe-floor status: $status"
        }
    }
}

function Test-CollapseConfig {
    $config = Read-JsonFile "Assets/Data/P9/p9c_collapse_debris_fatality_config.json"
    Assert-BooleanTrue $config.enableCollapseDebrisFatalityProxy "enableCollapseDebrisFatalityProxy"
    Assert-BooleanTrue $config.scenarioConfigurable "scenarioConfigurable"
    Assert-BooleanFalse $config.frameLevelRandomDeath "frameLevelRandomDeath"
    if ([Math]::Abs([double]$config.collapseDebrisExposureFatalityProbability - 0.35) -gt 0.0001) {
        throw "collapseDebrisExposureFatalityProbability must default to 0.35."
    }
    if ([int]$config.maxCollapseDebrisFatalEventsPerRun -ne 1) {
        throw "maxCollapseDebrisFatalEventsPerRun must default to 1."
    }
}

function Test-PresetsAndReasonCodes {
    $presets = Read-JsonFile "Assets/Data/P9/p9c_scenario_failure_presets.json"
    if (@($presets.presets).Count -lt 4) {
        throw "P9-C scenario presets require at least four records."
    }

    $catalog = Read-JsonFile "Assets/Data/P9/p9c_reason_code_catalog.json"
    $codes = @($catalog.reasonCodes | ForEach-Object { $_.reasonCode })
    foreach ($code in @(
        "selected_official_shelter",
        "selected_life_first_vertical_candidate",
        "entrance_blocked_failure",
        "failed_due_to_crowd_delay",
        "vertical_evacuation_complete_proxy",
        "safe_floor_below_required_height_failure",
        "collapse_debris_exposure_event",
        "collapse_debris_fatality_proxy",
        "killed_by_building_collapse_proxy",
        "failed_due_to_tsunami_arrival",
        "vertical_evacuation_failed_after_arrival"
    )) {
        if ($codes -notcontains $code) {
            throw "Missing reason code: $code"
        }
    }
}

Write-Host "P9-C JSON validation: starting"
Test-OutcomeRules
Test-TargetRules
Test-EntranceRules
Test-SafeFloorRules
Test-CollapseConfig
Test-PresetsAndReasonCodes
Write-Host "P9-C JSON validation: PASS" -ForegroundColor Green
exit 0
