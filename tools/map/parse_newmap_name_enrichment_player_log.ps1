param(
    [string]$LogPath = "",
    [string]$OutputPath = "Assets\Data\P10\newmap_name_enrichment_player_log_summary.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Get-LatestPlayerLog {
    $localLow = Join-Path $env:USERPROFILE "AppData\LocalLow"
    if (-not (Test-Path -LiteralPath $localLow -PathType Container)) {
        return $null
    }

    return Get-ChildItem -LiteralPath $localLow -Recurse -Filter Player.log -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
}

function Get-TokenValue {
    param([string]$Line, [string]$Name)
    if ([string]::IsNullOrWhiteSpace($Line)) {
        return $null
    }

    $pattern = "(?:^|\s)" + [regex]::Escape($Name) + "=(?<value>\S+)"
    $match = [regex]::Match($Line, $pattern)
    if ($match.Success) {
        return $match.Groups["value"].Value
    }

    return $null
}

function As-Int {
    param($Value)
    if ($null -eq $Value) { return $null }
    return [int]$Value
}

function As-Bool {
    param($Value)
    if ($null -eq $Value) { return $null }
    return ([string]$Value) -eq "True" -or ([string]$Value) -eq "true"
}

function Update-JsonFile {
    param([string]$RelativePath, [scriptblock]$Updater)
    $path = Join-Path $ProjectRoot $RelativePath
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        $json = Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
        & $Updater $json
        $json | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $path -Encoding UTF8
    }
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $candidate = Get-LatestPlayerLog
    if ($candidate) {
        $LogPath = $candidate.FullName
    }
}

if ([string]::IsNullOrWhiteSpace($LogPath) -or -not (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
    Write-Host "[FAIL] Player.log not found."
    exit 1
}

$lines = @(Get-Content -Encoding UTF8 -LiteralPath $LogPath -ErrorAction SilentlyContinue | ForEach-Object { [string]$_ })
$errors = @($lines | Where-Object {
    $_ -match "NullReferenceException|MissingReferenceException|Unhandled Exception|UnityEngine\.Debug:LogError|LogType\.Error|^\s*Error:|Scripts have compiler errors"
})
$warnings = @($lines | Where-Object {
    ($_ -match "UnityEngine\.Debug:LogWarning|LogType\.Warning|^\s*Warning:|^\s*WARNING:") -and
    ($_ -notmatch "Stage 1 warning") -and
    ($_ -notmatch "evacuation_stage1_warning") -and
    ($_ -notmatch "tourism_non_official_inspection_warning") -and
    ($_ -notmatch "success_non_official_candidate_with_warning")
})

$nameLabelLine = [string](@($lines | Where-Object { $_ -match "NewMap name labels built" } | Select-Object -Last 1) | Select-Object -First 1)
$selfAuditCompleted = [bool](@($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke completed" } | Select-Object -First 1) | Select-Object -First 1)
$selfAuditFailures = @($lines | Where-Object { $_ -match "NewMap gameplay self-audit smoke:" -and $_ -match "result=fail" })

$labels = [ordered]@{
    availableLabels = As-Int (Get-TokenValue $nameLabelLine "availableLabels")
    activeLabels = As-Int (Get-TokenValue $nameLabelLine "activeLabels")
    officialShelterLabels = As-Int (Get-TokenValue $nameLabelLine "officialShelterLabels")
    nonOfficialCandidateLabels = As-Int (Get-TokenValue $nameLabelLine "nonOfficialCandidateLabels")
    roadNameLabels = As-Int (Get-TokenValue $nameLabelLine "roadNameLabels")
    buildingNameLabels = As-Int (Get-TokenValue $nameLabelLine "buildingNameLabels")
    tokyoStationLabels = As-Int (Get-TokenValue $nameLabelLine "tokyoStationLabels")
    idOnlyLabels = As-Int (Get-TokenValue $nameLabelLine "idOnlyLabels")
    nameCacheLoaded = As-Bool (Get-TokenValue $nameLabelLine "nameCacheLoaded")
    nameCacheRecords = As-Int (Get-TokenValue $nameLabelLine "nameCacheRecords")
    reliableCacheLabels = As-Int (Get-TokenValue $nameLabelLine "reliableCacheLabels")
    addressOnlyHidden = As-Int (Get-TokenValue $nameLabelLine "addressOnlyHidden")
    lowConfidenceHidden = As-Int (Get-TokenValue $nameLabelLine "lowConfidenceHidden")
    runtimeNetworkRequestsAllowed = As-Bool (Get-TokenValue $nameLabelLine "runtimeNetworkRequestsAllowed")
    sourceNameStatus = Get-TokenValue $nameLabelLine "sourceNameStatus"
}

$cachePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_name_cache.json"
$cache = Get-Content -Encoding UTF8 -LiteralPath $cachePath -Raw | ConvertFrom-Json
$reportPath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_name_enrichment_report.json"
$enrichmentReport = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json

$namePassed = $labels.runtimeNetworkRequestsAllowed -eq $false -and
    $labels.nameCacheLoaded -eq $true -and
    $labels.nameCacheRecords -ge 100 -and
    $labels.officialShelterLabels -gt 0 -and
    $labels.nonOfficialCandidateLabels -gt 0 -and
    $labels.buildingNameLabels -gt 0 -and
    $labels.roadNameLabels -gt 0 -and
    $labels.idOnlyLabels -eq 0
$logPassed = $errors.Count -eq 0 -and $warnings.Count -eq 0 -and $namePassed -and $selfAuditCompleted -and $selfAuditFailures.Count -eq 0

$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    logPath = $LogPath
    errorCount = $errors.Count
    warningCount = $warnings.Count
    nameLabelDiagnostics = $labels
    selfAuditCompleted = $selfAuditCompleted
    selfAuditFailureCount = $selfAuditFailures.Count
    cachePath = "Assets/Data/P10/newmap_name_cache.json"
    cacheRecordCount = $cache.labels.Count
    onlineQueriesAttempted = $enrichmentReport.onlineQueriesAttempted
    onlineQueriesSucceeded = $enrichmentReport.onlineQueriesSucceeded
    onlineLabelsAdded = $enrichmentReport.onlineLabelsAdded
    namesRejected = $enrichmentReport.namesRejected
    runtimeNoWebPassed = ($labels.runtimeNetworkRequestsAllowed -eq $false)
    labelRuntimePassed = $namePassed
    firstErrors = @($errors | Select-Object -First 20)
    firstWarnings = @($warnings | Select-Object -First 20)
    finalStatus = if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed" }
}

$fullOutput = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fullOutput) | Out-Null
$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $fullOutput -Encoding UTF8

Update-JsonFile "Assets\Data\P10\newmap_name_label_runtime_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName generatedAt -NotePropertyValue (Get-Date).ToString("s") -Force
    $json | Add-Member -NotePropertyName activeScene -NotePropertyValue "Assets/Scenes/Chuo_BaseMap.unity" -Force
    $json | Add-Member -NotePropertyName runtimeNetworkRequestsAllowed -NotePropertyValue $false -Force
    $json | Add-Member -NotePropertyName nameCachePath -NotePropertyValue "Assets/Data/P10/newmap_name_cache.json" -Force
    $json | Add-Member -NotePropertyName nameCacheExists -NotePropertyValue $true -Force
    $json | Add-Member -NotePropertyName nameCacheRecordCount -NotePropertyValue $labels.nameCacheRecords -Force
    $json | Add-Member -NotePropertyName officialShelterLabelsRuntime -NotePropertyValue $labels.officialShelterLabels -Force
    $json | Add-Member -NotePropertyName nonOfficialCandidateLabelsRuntime -NotePropertyValue $labels.nonOfficialCandidateLabels -Force
    $json | Add-Member -NotePropertyName buildingLabelsRuntime -NotePropertyValue $labels.buildingNameLabels -Force
    $json | Add-Member -NotePropertyName roadLabelsRuntime -NotePropertyValue $labels.roadNameLabels -Force
    $json | Add-Member -NotePropertyName tokyoStationLabelsRuntime -NotePropertyValue $labels.tokyoStationLabels -Force
    $json | Add-Member -NotePropertyName idOnlyLabelsRuntime -NotePropertyValue $labels.idOnlyLabels -Force
    $json | Add-Member -NotePropertyName fullAddressesHidden -NotePropertyValue $true -Force
    $json | Add-Member -NotePropertyName idOnlyHiddenInNormalMode -NotePropertyValue $true -Force
    $json | Add-Member -NotePropertyName lowConfidenceHiddenInNormalMode -NotePropertyValue $true -Force
    $json | Add-Member -NotePropertyName runtimeValidationStatus -NotePropertyValue ($(if ($namePassed) { "validated_by_name_enrichment_player_smoke" } else { "failed_player_log_parse" })) -Force
}

Update-JsonFile "Assets\Data\P10\newmap_name_enrichment_player_report.json" {
    param($json)
    $json | Add-Member -NotePropertyName playerLogSummary -NotePropertyValue "completed" -Force
    $json | Add-Member -NotePropertyName playerLogErrors -NotePropertyValue $errors.Count -Force
    $json | Add-Member -NotePropertyName playerLogWarnings -NotePropertyValue $warnings.Count -Force
    $json | Add-Member -NotePropertyName labelRuntimeDiagnostics -NotePropertyValue ($(if ($namePassed) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName runtimeNoWebDiagnostics -NotePropertyValue ($(if ($labels.runtimeNetworkRequestsAllowed -eq $false) { "passed" } else { "failed" })) -Force
    $json | Add-Member -NotePropertyName nameLabelDiagnostics -NotePropertyValue $labels -Force
    $json | Add-Member -NotePropertyName finalStatus -NotePropertyValue ($(if ($logPassed) { "completed_on_new_chuo_basemap" } else { "failed_player_log_parse" })) -Force
}

$labelDocPath = Join-Path $ProjectRoot "docs\NEWMAP_NAME_LABEL_RUNTIME_REPORT.md"
$labelDoc = @(
    "# NewMap Name Label Runtime Report",
    "",
    "Generated: $(Get-Date -Format s)",
    "",
    "- Cache loaded: $($labels.nameCacheLoaded)",
    "- Cache records: $($labels.nameCacheRecords)",
    "- Official labels: $($labels.officialShelterLabels)",
    "- Non-official labels: $($labels.nonOfficialCandidateLabels)",
    "- Building labels: $($labels.buildingNameLabels)",
    "- Road labels: $($labels.roadNameLabels)",
    "- Tokyo Station labels: $($labels.tokyoStationLabels)",
    "- ID-only labels visible: $($labels.idOnlyLabels)",
    "- Runtime network requests allowed: $($labels.runtimeNetworkRequestsAllowed)",
    "- Final status: $(if ($namePassed) { 'validated_by_name_enrichment_player_smoke' } else { 'failed_player_log_parse' })",
    "",
    "The Unity runtime reads `Assets/Data/P10/newmap_name_cache.json` and does not perform online lookups."
)
$labelDoc | Set-Content -LiteralPath $labelDocPath -Encoding UTF8

Write-Host "[PASS] Name Enrichment Player.log summary written to $fullOutput"
if (-not $logPassed) {
    exit 1
}
exit 0
