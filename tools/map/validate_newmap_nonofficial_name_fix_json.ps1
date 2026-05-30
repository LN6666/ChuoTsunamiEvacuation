param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$script:Failures = @()

function Add-Failure {
    param([string]$Message)
    $script:Failures += $Message
}

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "Missing JSON: $RelativePath"
        return $null
    }
    try {
        return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
    }
    catch {
        Add-Failure "Invalid JSON: $RelativePath ($($_.Exception.Message))"
        return $null
    }
}

function Require {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Add-Failure $Message
    }
}

function Get-Name {
    param($Label)
    if ($Label.finalDisplayName) { return [string]$Label.finalDisplayName }
    if ($Label.normalizedName) { return [string]$Label.normalizedName }
    return [string]$Label.name
}

function Has-Japanese {
    param([string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name)) { return $false }
    return [regex]::IsMatch($Name, "[\u3040-\u30ff\u3400-\u9fff]")
}

function Is-BadVisibleName {
    param([string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name)) { return $true }
    $lower = $Name.Trim().ToLowerInvariant()
    return $lower.StartsWith("bldg_") -or
        $lower.StartsWith("gml_") -or
        $lower.StartsWith("13102-bldg-") -or
        $lower.StartsWith("sample_plateau") -or
        $lower.StartsWith("p8_plateau_highrise_candidate_") -or
        $lower -eq "unknown" -or
        $lower -eq "unnamed" -or
        ($Name -match '^\s*[-+]?\d+(\.\d+)?\s*,\s*[-+]?\d+(\.\d+)?\s*$') -or
        (($Name -match '\u6771\u4eac\u90fd|\u4e2d\u592e\u533a') -and ($Name -match '\u4e01\u76ee|\u756a|\u53f7|\u3012')) -or
        -not (Has-Japanese $Name)
}

$config = Read-Json "Assets\Data\P10\newmap_non_official_name_enrichment_config.json"
$audit = Read-Json "Assets\Data\P10\newmap_non_official_candidate_name_audit.json"
$query = Read-Json "Assets\Data\P10\newmap_non_official_candidate_name_query_list.json"
$report = Read-Json "Assets\Data\P10\newmap_non_official_name_enrichment_report.json"
$normalization = Read-Json "Assets\Data\P10\newmap_non_official_name_normalization_report.json"
$writeback = Read-Json "Assets\Data\P10\newmap_non_official_name_cache_writeback_report.json"
$cache = Read-Json "Assets\Data\P10\newmap_name_cache.json"
$runtime = Read-Json "Assets\Data\P10\newmap_name_label_runtime_report.json"
$readiness = Read-Json "Assets\Data\P10\newmap_manual_playtest_readiness.json"
$resource = Read-Json "Assets\Resources\NewMap\newmap_runtime_non_official_candidates.json"

if ($config) {
    Require ([bool]$config.enabled) "Non-official enrichment config disabled."
    Require ([string]$config.targetType -eq "active_non_official_candidates") "Wrong enrichment targetType."
    Require (-not [bool]$config.runtimeNetworkRequestsAllowed) "Config allows runtime network requests."
    Require ([bool]$config.allowOnlineLookup) "Online preprocessing lookup is disabled."
    Require ([bool]$config.onlyQueryMissingNames) "Config must only query missing names."
    Require ([bool]$config.preferJapaneseNames) "Config must prefer Japanese names."
    Require ([bool]$config.mainNameOnly) "Config must keep main names only."
    Require ([bool]$config.hideAddressLikeNames) "Config must hide address-like names."
    Require ([bool]$config.hideIdOnlyInNormalMode) "Config must hide ID-only names."
    Require ([double]$config.rateLimitSeconds -ge 1.1) "Rate limit must be >= 1.1 seconds."
    Require ([int]$config.maxQueriesPerRun -le 120) "maxQueriesPerRun must be <= 120."
    Require ([string]$config.userAgent -eq "ChuoTsunamiEvacuation-PBL10-NonOfficialNameEnrichment/1.0") "Unexpected User-Agent."
}

if ($audit) {
    Require ([int]$audit.activeResourceNonOfficialCandidates -ge [int]$audit.activePlayableNonOfficialCandidates) "Audit active counts inconsistent."
    Require ([int]$audit.activePlayableNonOfficialCandidates -gt 0) "No active playable non-official candidates audited."
    Require ([int]$audit.activePlayableNeedsEnrichmentCount -gt 0) "Audit did not find any name-enrichment candidates."
    Require ([int]$audit.coordinateAvailableCount -gt 0) "Audit found no coordinates."
    Require (($audit.PSObject.Properties.Name -contains "playableBoundaryRadiusMeters") -and [double]$audit.playableBoundaryRadiusMeters -gt 0.0) "Audit boundary radius is missing."
}

if ($query) {
    Require (-not [bool]$query.runtimeNetworkRequestsAllowed) "Query list allows runtime network."
    Require ([int]$query.queryCandidateCount -gt 0) "Query list is empty."
    Require ([int]$query.enabledForOnlineLookupCount -gt 0) "No missing-name candidates enabled for online lookup."
    foreach ($item in @($query.items)) {
        Require ([bool]$item.nonOfficialWarningRequired) "Query item lost non-official warning: $($item.candidateId)"
        Require (-not [bool]$item.isOfficialShelter) "Query item promoted to official shelter: $($item.candidateId)"
    }
}

if ($report) {
    Require (-not [bool]$report.runtimeNetworkRequestsAllowed) "Enrichment report allows runtime network."
    Require ([int]$report.queryCandidates -gt 0) "Enrichment report has no query candidates."
    if ([int]$report.onlineQueriesAttempted -eq 0) {
        Require (-not [string]::IsNullOrWhiteSpace([string]$report.onlineQueriesAttemptedExplanation)) "0 online attempts without explanation."
    }
    else {
        Require ([int]$report.onlineQueriesAttempted -eq ([int]$report.onlineQueriesSucceeded + [int]$report.onlineQueriesFailed)) "Online attempt/success/failure counts inconsistent."
    }
    Require ([int]$report.namesNewlyAdded -gt 0) "No names were added or updated."
    Require ([int]$report.localSourceNamesUsed -gt 0) "No local/source names were used."
    Require ([bool]$report.normalization.japaneseKanjiMainNameOnly) "Japanese/Kanji main-name normalization missing."
    Require (-not [bool]$report.normalization.fabricatedNamesAllowed) "Fabricated names allowed."
    Require ([bool]$report.attributionRequired) "Attribution not marked required despite OSM use."
}

if ($normalization) {
    Require ([bool]$normalization.preferJapaneseNames) "Normalization does not prefer Japanese names."
    Require ([bool]$normalization.mainNameOnly) "Normalization is not main-name only."
    Require (-not [bool]$normalization.machineTranslationUsed) "Machine translation used."
    Require (-not [bool]$normalization.fabricatedNamesAllowed) "Fabricated names allowed."
    Require ([int]$normalization.visibleAddressLikeLabelCount -eq 0) "Visible address-like labels reported."
    Require ([int]$normalization.visibleIdOnlyLabelCount -eq 0) "Visible ID-only labels reported."
    Require ([string]$normalization.finalStatus -eq "passed") "Normalization report did not pass."
}

if ($writeback) {
    Require ([string]$writeback.cachePath -eq "Assets/Data/P10/newmap_name_cache.json") "Unexpected cache writeback path."
    Require (([int]$writeback.cacheEntriesAdded + [int]$writeback.cacheEntriesUpdated) -gt 0) "No cache entries added/updated."
    Require ([int]$writeback.runtimeResourceDisplayNamesUpdated -gt 0) "Runtime resource display names were not updated."
    Require ([bool]$writeback.nonOfficialWarningRequired) "Writeback dropped non-official warning flag."
    Require (-not [bool]$writeback.isOfficialShelter) "Writeback promoted candidate to official shelter."
}

if ($cache) {
    Require (-not [bool]$cache.runtimeNetworkRequestsAllowed) "Name cache allows runtime network."
    $candidateLabels = @($cache.labels | Where-Object { $_.objectType -eq "candidate" -and -not [bool]$_.hiddenInNormalMode -and -not [bool]$_.disabled })
    $enriched = @($candidateLabels | Where-Object { [string]$_.source -match "local_osm_cache|online_osm|online_gsi|source_metadata" })
    Require ($candidateLabels.Count -gt 0) "No visible candidate labels in cache."
    Require ($enriched.Count -gt 0) "No enriched candidate labels in cache."
    foreach ($label in $candidateLabels) {
        $name = Get-Name $label
        Require (-not (Is-BadVisibleName $name)) "Bad visible candidate cache name: $($label.id) = $name"
        Require ([bool]$label.nonOfficialWarningRequired) "Candidate cache label missing non-official warning: $($label.id)"
        Require (-not [bool]$label.isOfficialShelter) "Candidate cache label marked official: $($label.id)"
        Require ([double]$label.confidence -ge 0.6) "Candidate cache label below runtime confidence: $($label.id)"
    }
}

if ($runtime) {
    Require (-not [bool]$runtime.runtimeNetworkRequestsAllowed) "Runtime label report allows network."
    Require ([int]$runtime.runtimeWebRequestsObserved -eq 0) "Runtime web requests observed."
    Require ([bool]$runtime.nonOfficialWarningPreserved) "Runtime report did not preserve warning."
    Require ([bool]$runtime.isOfficialShelterPreservedFalse) "Runtime report did not preserve non-official status."
    Require ([bool]$runtime.hideDisabledOutOfMapCandidates) "Runtime report did not hide disabled/out-of-map candidates."
}

if ($resource) {
    $badActive = @($resource.records | Where-Object {
        [bool]$_.activeInGame -and [bool]$_.nonOfficialWarningRequired -and -not [bool]$_.isOfficialShelter -and (Is-BadVisibleName ([string]$_.displayName))
    })
    Require ($badActive.Count -le 40) "Too many active resource records still have ID/address-like names after writeback: $($badActive.Count)"
}

if ($readiness) {
    Require (-not [bool]$readiness.finalReleaseArchiveCreated) "Readiness says final archive created."
    Require (-not [bool]$readiness.p10EFGCreated) "Readiness says P10-E/F/G created."
    Require (-not [string]::IsNullOrWhiteSpace([string]$readiness.manualReadinessDecision)) "Manual readiness decision missing."
}

if ($script:Failures.Count -gt 0) {
    foreach ($failure in $script:Failures) { Write-Host "[FAIL] $failure" }
    exit 1
}

Write-Host "[PASS] Non-official candidate name enrichment JSON/cache validation passed."
exit 0
