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

function Read-RequiredJson {
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

function Require-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Add-Failure $Message
    }
}

function Get-DisplayName {
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

function Is-AddressOrIdName {
    param([string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name)) { return $true }
    $lower = $Name.ToLowerInvariant()
    return $lower.StartsWith("bldg_") -or
        $lower.StartsWith("gml_") -or
        $lower.StartsWith("13102-bldg-") -or
        $lower.Contains("_unknown_") -or
        $lower -eq "unknown" -or
        $lower -eq "unnamed" -or
        ($Name -match "^\s*[-+]?\d+(\.\d+)?\s*,\s*[-+]?\d+(\.\d+)?\s*$") -or
        ($Name -match "東京都.*中央区.*(丁目|番|号)")
}

$config = Read-RequiredJson "Assets\Data\P10\newmap_name_enrichment_config.json"
$cache = Read-RequiredJson "Assets\Data\P10\newmap_name_cache.json"
$report = Read-RequiredJson "Assets\Data\P10\newmap_name_enrichment_report.json"
$audit = Read-RequiredJson "Assets\Data\P10\newmap_name_cache_coverage_audit.json"
$queryList = Read-RequiredJson "Assets\Data\P10\newmap_name_enrichment_query_list.json"
$rules = Read-RequiredJson "Assets\Data\P10\newmap_name_normalization_rules.json"
$normalization = Read-RequiredJson "Assets\Data\P10\newmap_name_normalization_report.json"
$labelConfig = Read-RequiredJson "Assets\Data\P10\newmap_name_label_config.json"
$runtimeReport = Read-RequiredJson "Assets\Data\P10\newmap_name_label_runtime_report.json"
$readiness = Read-RequiredJson "Assets\Data\P10\newmap_manual_playtest_readiness.json"

if ($config) {
    Require-Condition ([bool]$config.preprocessingOnly) "Name enrichment config must be preprocessing-only."
    Require-Condition (-not [bool]$config.runtimeNetworkRequestsAllowed) "Name enrichment config allows runtime network."
    Require-Condition ([bool]$config.allowOnlineLookup) "Online lookup is disabled despite this completion pass."
    Require-Condition ([bool]$config.onlyQueryMissingNames) "Enrichment must only query missing names."
    Require-Condition ([int]$config.maxQueriesPerRun -le 200) "Online query cap must remain bounded at <= 200."
    Require-Condition ([double]$config.rateLimitSeconds -ge 1.1) "Online rate limit must be >= 1.1 seconds."
    Require-Condition ([string]$config.userAgent -match "ChuoTsunamiEvacuation-PBL10-NameEnrichment") "Nominatim User-Agent must identify this project."
}

if ($cache) {
    Require-Condition (-not [bool]$cache.runtimeNetworkRequestsAllowed) "Name cache allows runtime network."
    Require-Condition ($cache.labels.Count -ge 100) "Name cache has too few records after enrichment."
    $official = @($cache.labels | Where-Object { $_.objectType -eq "official_shelter" -and -not [bool]$_.hiddenInNormalMode })
    $candidate = @($cache.labels | Where-Object { $_.objectType -eq "candidate" -and -not [bool]$_.hiddenInNormalMode })
    $building = @($cache.labels | Where-Object { $_.objectType -eq "building" -and -not [bool]$_.hiddenInNormalMode })
    $road = @($cache.labels | Where-Object { $_.objectType -eq "road" -and -not [bool]$_.hiddenInNormalMode })
    Require-Condition ($official.Count -ge 15) "Official shelter labels missing from final cache."
    Require-Condition ($candidate.Count -gt 0) "Non-official candidate labels missing from final cache."
    Require-Condition ($building.Count -gt 0) "Building labels missing from final cache."
    Require-Condition ($road.Count -gt 0) "Road labels missing from final cache."

    foreach ($label in $cache.labels) {
        $hidden = [bool]$label.hiddenInNormalMode -or [bool]$label.disabled
        $displayName = Get-DisplayName $label
        if (-not $hidden) {
            Require-Condition (Has-Japanese $displayName) "Visible cache label is not Japanese/Kanji/Kana: $($label.id) = $displayName"
            Require-Condition (-not (Is-AddressOrIdName $displayName)) "Address/ID-like label visible in cache: $($label.id) = $displayName"
            Require-Condition ([double]$label.confidence -ge 0.6) "Low-confidence label visible in cache: $($label.id)"
        }
    }
}

if ($report) {
    Require-Condition (-not [bool]$report.runtimeNetworkRequestsAllowed) "Enrichment report allows runtime network."
    Require-Condition ([string]$report.provider -eq "OpenStreetMap Nominatim") "Enrichment report must record Nominatim provider when online queries ran."
    Require-Condition ([int]$report.onlineQueriesAttempted -gt 0) "No online queries were attempted; enrichment cannot be claimed complete."
    Require-Condition ([int]$report.onlineQueriesSucceeded -gt 0) "No online queries succeeded."
    Require-Condition ([int]$report.onlineQueriesAttempted -eq [int]$report.onlineQueriesSucceeded + [int]$report.onlineQueriesFailed) "Online attempt/success/failure counts are inconsistent."
    Require-Condition ([int]$report.namesNewlyAdded -gt 0) "Online lookup did not add any names."
    Require-Condition ([bool]$report.normalization.japaneseKanjiMainNameOnly) "Japanese/Kanji main-name normalization not reported."
    Require-Condition (-not [bool]$report.normalization.fabricatedNamesAllowed) "Fabricated names must not be allowed."
    Require-Condition ([bool]$report.attributionRequired) "Attribution must be required when OSM/Nominatim data is used."
}

if ($audit) {
    Require-Condition ([string]$audit.hardRuleStatus -eq "online_queries_executed") "Audit hard rule did not record executed online queries."
    Require-Condition ([int]$audit.totalCachedNameRecords -ge 100) "Audit total cache records too low."
    Require-Condition ([int]$audit.visibleIdOnlyLabels -eq 0) "Audit found visible ID-only labels."
    Require-Condition ([int]$audit.visibleAddressLikeLabels -eq 0) "Audit found visible address-like labels."
    Require-Condition ([int]$audit.visibleLowConfidenceLabels -eq 0) "Audit found visible low-confidence labels."
}

if ($queryList) {
    Require-Condition (-not [bool]$queryList.runtimeNetworkRequestsAllowed) "Query list allows runtime network."
    Require-Condition ([int]$queryList.queryCount -gt 0) "Missing-name query list is empty."
    Require-Condition ([int]$queryList.enabledForOnlineLookupCount -gt 0) "No query list items were enabled for online lookup."
    Require-Condition ([int]$queryList.maxQueriesPerRun -le 200) "Query list cap exceeds approved maximum."
}

if ($rules) {
    Require-Condition ([bool]$rules.preferJapaneseKanji) "Normalization rules must prefer Japanese/Kanji."
    Require-Condition ([bool]$rules.showOnlyMainName) "Normalization rules must keep only main names."
    Require-Condition ([bool]$rules.hideFullAddress) "Normalization rules must hide full addresses."
    Require-Condition ([bool]$rules.hideIdOnly) "Normalization rules must hide ID-only labels."
    Require-Condition (-not [bool]$rules.machineTranslationAllowed) "Machine translation must be disabled."
    Require-Condition (-not [bool]$rules.fabricatedNamesAllowed) "Fabricated names must be disabled."
}

if ($normalization) {
    Require-Condition ([int]$normalization.visibleAddressLikeLabelCount -eq 0) "Normalization report has visible address-like labels."
    Require-Condition ([int]$normalization.visibleIdOnlyLabelCount -eq 0) "Normalization report has visible ID-only labels."
    Require-Condition ([int]$normalization.visibleLowConfidenceLabelCount -eq 0) "Normalization report has visible low-confidence labels."
    Require-Condition ([string]$normalization.finalStatus -eq "passed") "Normalization report did not pass."
}

if ($labelConfig) {
    Require-Condition (-not [bool]$labelConfig.runtimeNetworkRequestsAllowed) "Runtime label config allows network."
    Require-Condition ([bool]$labelConfig.showOfficialShelterNames) "Official shelter labels disabled."
    Require-Condition ([bool]$labelConfig.showNonOfficialCandidateNames) "Non-official candidate labels disabled."
    Require-Condition ([bool]$labelConfig.showBuildingNames) "Building labels disabled."
    Require-Condition ([bool]$labelConfig.showRoadNames) "Road labels disabled."
    Require-Condition ([int]$labelConfig.maxVisibleLabels -le 300) "Runtime label cap too high."
}

if ($runtimeReport) {
    Require-Condition (-not [bool]$runtimeReport.runtimeNetworkRequestsAllowed) "Runtime label report allows network."
    Require-Condition ([bool]$runtimeReport.nameCacheExists) "Runtime label report says name cache is missing."
    Require-Condition ([int]$runtimeReport.nameCacheRecordCount -ge 100) "Runtime label report cache count too low."
    Require-Condition ([bool]$runtimeReport.fullAddressesHidden) "Runtime report must hide full addresses."
    Require-Condition ([bool]$runtimeReport.idOnlyHiddenInNormalMode) "Runtime report must hide ID-only names."
    Require-Condition ([bool]$runtimeReport.lowConfidenceHiddenInNormalMode) "Runtime report must hide low-confidence names."
}

$attributionDoc = Join-Path $root "docs\NEWMAP_NAME_LABEL_ATTRIBUTION.md"
if ($report -and [bool]$report.attributionRequired) {
    Require-Condition (Test-Path -LiteralPath $attributionDoc -PathType Leaf) "Attribution doc missing despite OSM/Nominatim use."
    if (Test-Path -LiteralPath $attributionDoc -PathType Leaf) {
        $attribution = Get-Content -Encoding UTF8 -LiteralPath $attributionDoc -Raw
        Require-Condition ($attribution -match "OpenStreetMap" -and $attribution -match "Open Database License") "Attribution doc does not mention OSM/ODbL."
    }
}

if ($readiness) {
    $allowed = @("ready_for_manual_playtest", "ready_with_documented_building_visual_limitations", "ready_with_documented_ground_cover_limitations", "needs_quick_fix_before_manual_test", "blocked")
    Require-Condition ($allowed -contains [string]$readiness.manualReadinessDecision) "Manual readiness decision missing/invalid."
}

if ($script:Failures.Count -gt 0) {
    foreach ($failure in $script:Failures) {
        Write-Host "[FAIL] $failure"
    }
    exit 1
}

Write-Host "[PASS] NewMap name enrichment JSON/cache files validated."
exit 0
