[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

function Assert-FileExists {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Assert-FileContains {
    param([string]$RelativePath, [string[]]$Fragments)
    Assert-FileExists $RelativePath
    $text = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\"))
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
    }
}

function Read-Json {
    param([string]$RelativePath)
    Assert-FileExists $RelativePath
    return Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\")) | ConvertFrom-Json
}

foreach ($file in @(
    "docs/P8E_P9_HANDOFF_PACKAGE.md",
    "docs/P8E_P9_REQUIRED_INPUTS.md",
    "docs/P8E_P9_DO_NOT_REDO_P8_SCOPE.md",
    "docs/P8E_HUMANITARIAN_CANDIDATE_PERSISTENT_VISIBILITY_PLAN.md",
    "docs/P8E_HUMANITARIAN_CANDIDATE_NON_OFFICIAL_DISCLAIMER_RULES.md",
    "docs/P8E_HUMANITARIAN_CANDIDATE_P9_HANDOFF.md",
    "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json"
)) {
    Assert-FileExists $file
}

Assert-FileContains "docs/P8E_P9_HANDOFF_PACKAGE.md" @(
    "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity",
    "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json",
    "Assets/Data/P8/infrastructure_hazard_interaction_config.json",
    "Assets/Data/P8/infrastructure_damage_proxy_config.json",
    "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json",
    "P9 should not redo",
    "final real gameplay landing"
)

Assert-FileContains "docs/P8E_HUMANITARIAN_CANDIDATE_NON_OFFICIAL_DISCLAIMER_RULES.md" @(
    "Non-official humanitarian high-rise candidate",
    "Not an official evacuation shelter",
    "isOfficialShelter=false",
    "nonOfficialWarningRequired=true",
    "selectableGameplayEnabled=false"
)

Assert-FileContains "docs/P8E_P9_DO_NOT_REDO_P8_SCOPE.md" @(
    "tsunami hazard data extraction",
    "dynamic risk-front foundation",
    "infrastructure hazard state foundation",
    "damage/blockage/collapse proxy foundation",
    "expanded humanitarian high-rise candidate audit"
)

$handoff = Read-Json "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json"
if ([string]$handoff.datasetId -ne "p8e_humanitarian_candidate_persistent_visibility_handoff_v1") {
    throw "Unexpected P8-E handoff datasetId: $($handoff.datasetId)"
}
if ([int]$handoff.candidateTotals.totalCandidates -ne 110 -or [int]$handoff.candidateTotals.namedCandidates -ne 28) {
    throw "P8-E handoff candidate totals do not match the accepted audit."
}
if ([int]$handoff.candidateTotals.idOnlyOrUnknownNamePlateauCandidates -ne 81) {
    throw "P8-E handoff must preserve the accepted 81 PLATEAU ID-only/unknown-name candidate count."
}
if ([bool]$handoff.dataOnly -ne $true -or [bool]$handoff.persistentSceneObjectsCreatedInP8E -ne $false) {
    throw "P8-E handoff must remain data-only with no scene object creation."
}
if ([bool]$handoff.isOfficialShelterDataset -ne $false -or [bool]$handoff.isOfficialShelter -ne $false -or [bool]$handoff.nonOfficialWarningRequired -ne $true) {
    throw "P8-E handoff must keep candidates non-official and warning-required."
}
if ([bool]$handoff.selectableGameplayEnabledInP8E -ne $false -or [bool]$handoff.implementsP9Gameplay -ne $false -or [bool]$handoff.affectsGameplaySuccessFailure -ne $false) {
    throw "P8-E handoff must not enable P9 gameplay or success/failure rules."
}
if ([string]$handoff.requiredVisibleLabel -notmatch "Not an official evacuation shelter") {
    throw "P8-E handoff missing required non-official label."
}

Write-Host "P8-E handoff package validation: PASS" -ForegroundColor Green
exit 0
