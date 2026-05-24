[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$auditPath = Join-Path $repoRoot "Assets\Data\P8\humanitarian_highrise_candidate_audit_v1.json"
$statusConfigPath = Join-Path $repoRoot "Assets\Data\P8\humanitarian_candidate_hazard_status_config.json"

if (-not (Test-Path -LiteralPath $auditPath -PathType Leaf)) {
    throw "Missing humanitarian candidate audit JSON."
}
if (-not (Test-Path -LiteralPath $statusConfigPath -PathType Leaf)) {
    throw "Missing humanitarian candidate status config JSON."
}

$audit = Get-Content -Raw -Encoding UTF8 -LiteralPath $auditPath | ConvertFrom-Json
$statusConfig = Get-Content -Raw -Encoding UTF8 -LiteralPath $statusConfigPath | ConvertFrom-Json
$records = @($audit.records)

if ($records.Count -le 5) {
    throw "Humanitarian candidate audit must remain expanded beyond sample-only data."
}
if ([bool]$audit.isOfficialShelterDataset -ne $false -or [bool]$statusConfig.isOfficialShelterDataset -ne $false) {
    throw "Humanitarian candidate data must not be official shelter data."
}
if ([bool]$audit.nonOfficialWarningRequired -ne $true -or [bool]$statusConfig.requireNonOfficialWarningForHumanitarianCandidates -ne $true) {
    throw "Humanitarian candidates must require non-official warnings."
}
if ([bool]$statusConfig.selectableGameplayEnabledInP8D -ne $false) {
    throw "P8-D must not enable selectable humanitarian candidate gameplay."
}

$named = 0
$unknown = 0
foreach ($record in $records) {
    if ([bool]$record.isOfficialShelter -ne $false) {
        throw "Candidate $($record.candidateId) claims official shelter status."
    }
    if ([bool]$record.nonOfficialWarningRequired -ne $true) {
        throw "Candidate $($record.candidateId) lacks non-official warning requirement."
    }
    if ([bool]$record.p8dDamageStatusEligible -ne $true) {
        throw "Candidate $($record.candidateId) is not marked P8-D damage-status eligible."
    }
    if ([bool]$record.implementsP9SelectableGameplay -ne $false) {
        throw "Candidate $($record.candidateId) enables P9 selectable gameplay."
    }
    if ([string]::IsNullOrWhiteSpace([string]$record.buildingName)) {
        $unknown += 1
        if ([bool]$record.manualReviewNeeded -ne $true) {
            throw "Unknown-name candidate $($record.candidateId) must require manual review."
        }
    }
    else {
        $named += 1
    }
}

Write-Host "P8-D humanitarian candidate status inspection: PASS" -ForegroundColor Green
Write-Host "Candidates: $($records.Count), named: $named, id-only/unknown: $unknown"
exit 0
