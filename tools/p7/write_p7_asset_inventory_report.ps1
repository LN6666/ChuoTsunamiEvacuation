[CmdletBinding()]
param(
    [string]$ScanJson = "",
    [string]$ProjectRoot = "",
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
}
else {
    $ProjectRoot = (Resolve-Path $ProjectRoot).ProviderPath
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $ProjectRoot "docs"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory = Join-Path $ProjectRoot $OutputDirectory
}

function ConvertTo-ForwardSlashPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ""
    }

    return ($Path -replace "\\", "/")
}

function ConvertTo-RepoPath {
    param([string]$FullPath)

    $fullProject = [System.IO.Path]::GetFullPath($ProjectRoot).TrimEnd([char[]]@("\", "/"))
    $fullTarget = [System.IO.Path]::GetFullPath($FullPath)
    if (-not $fullTarget.StartsWith($fullProject, [System.StringComparison]::OrdinalIgnoreCase)) {
        return ConvertTo-ForwardSlashPath $FullPath
    }

    $baseUri = New-Object System.Uri (($fullProject + [System.IO.Path]::DirectorySeparatorChar))
    $targetUri = New-Object System.Uri $fullTarget
    return ConvertTo-ForwardSlashPath ([System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()))
}

function Format-MarkdownText {
    param([object]$Value)

    if ($null -eq $Value) {
        return ""
    }

    $text = [string]$Value
    $text = $text -replace "`r", " "
    $text = $text -replace "`n", " "
    $text = $text -replace "\|", "\|"
    return $text.Trim()
}

function Format-Bytes {
    param([object]$Bytes)

    if ($null -eq $Bytes) {
        return "0 B"
    }

    $value = [double]$Bytes
    if ($value -ge 1GB) {
        return ("{0:N2} GB" -f ($value / 1GB))
    }
    if ($value -ge 1MB) {
        return ("{0:N2} MB" -f ($value / 1MB))
    }
    if ($value -ge 1KB) {
        return ("{0:N2} KB" -f ($value / 1KB))
    }

    return ("{0:N0} B" -f $value)
}

function New-MarkdownTable {
    param(
        [string[]]$Headers,
        [object[]]$Rows
    )

    if ($null -eq $Rows -or $Rows.Count -eq 0) {
        return "_None found._"
    }

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("| $($Headers -join " | ") |") | Out-Null
    $lines.Add("| $(($Headers | ForEach-Object { "---" }) -join " | ") |") | Out-Null

    foreach ($row in $Rows) {
        $cells = @($row | ForEach-Object { Format-MarkdownText $_ })
        $lines.Add("| $($cells -join " | ") |") | Out-Null
    }

    return ($lines -join [Environment]::NewLine)
}

function New-MarkdownList {
    param([object[]]$Values)

    if ($null -eq $Values -or $Values.Count -eq 0) {
        return "- None."
    }

    return (($Values | ForEach-Object { "- $(Format-MarkdownText $_)" }) -join [Environment]::NewLine)
}

function Get-Array {
    param([object]$Value)

    if ($null -eq $Value) {
        return @()
    }

    return @($Value)
}

function Select-Top {
    param(
        [object]$Value,
        [int]$Count = 10
    )

    return @(Get-Array $Value | Select-Object -First $Count)
}

function Get-ControlLevel {
    param(
        [int]$FileCount,
        [int64]$TotalBytes
    )

    if ($FileCount -le 10 -and $TotalBytes -le 250MB) {
        return "High"
    }
    if ($FileCount -le 100 -and $TotalBytes -le 1GB) {
        return "Medium"
    }

    return "Low"
}

function Get-ImportRisk {
    param(
        [int]$FileCount,
        [int64]$TotalBytes,
        [string]$Categories
    )

    if ($TotalBytes -ge 1GB -or $FileCount -ge 1000) {
        return "High"
    }
    if ($TotalBytes -ge 250MB -or $Categories.IndexOf("Buildings", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        return "Medium"
    }

    return "Low"
}

function Get-WindowsBenchmarkSuitability {
    param(
        [object]$Candidate,
        [string]$ControlLevel,
        [string]$ImportRisk
    )

    $categories = [string]$Candidate.likelyCategories
    $keywords = [string]$Candidate.keywordTags
    $hasRelevantCategory = (
        $categories.IndexOf("Buildings", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $categories.IndexOf("Roads", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $categories.IndexOf("Bridges", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $categories.IndexOf("Underground", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $keywords.IndexOf("riverfront", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    )

    if ($hasRelevantCategory -and $ControlLevel -ne "Low" -and $ImportRisk -ne "High") {
        return "Good candidate after small-area bounds are confirmed"
    }

    if ($hasRelevantCategory) {
        return "Useful but needs smaller mesh-code or area slice"
    }

    return "Secondary reference only"
}

if ([string]::IsNullOrWhiteSpace($ScanJson)) {
    $stdinLines = @($input)
    if ($stdinLines.Count -gt 0) {
        $ScanJson = ($stdinLines -join [Environment]::NewLine)
    }
}

if ([string]::IsNullOrWhiteSpace($ScanJson)) {
    $scanScript = Join-Path $scriptRoot "scan_p7_assets.ps1"
    $scanOutput = & $scanScript -ProjectRoot $ProjectRoot
    if ($LASTEXITCODE -ne 0) {
        throw "scan_p7_assets.ps1 failed with exit code $LASTEXITCODE"
    }

    $ScanJson = ($scanOutput -join [Environment]::NewLine)
}

$scan = $ScanJson | ConvertFrom-Json

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

$inventoryReportPath = Join-Path $OutputDirectory "P7_ASSET_INVENTORY_REPORT.md"
$lodReportPath = Join-Path $OutputDirectory "P7_LOD_AVAILABILITY_REPORT.md"
$candidateReportPath = Join-Path $OutputDirectory "P7_BENCHMARK_AREA_CANDIDATES.md"

$rootRows = @(
    Get-Array $scan.roots | ForEach-Object {
        , @($_.label, $_.path, $_.fileCount, (Format-Bytes $_.totalBytes))
    }
)

$specialRows = @(
    Get-Array $scan.specialFolderSummary | ForEach-Object {
        , @($_.folder, $_.exists, $_.fileCount, (Format-Bytes $_.totalBytes), $_.note)
    }
)

$extensionRows = @(
    Select-Top $scan.extensionSummary 20 | ForEach-Object {
        , @($_.name, $_.fileCount, (Format-Bytes $_.totalBytes))
    }
)

$categoryRows = @(
    Select-Top $scan.likelyPlateauCategorySummary 20 | ForEach-Object {
        , @($_.name, $_.fileCount, (Format-Bytes $_.totalBytes))
    }
)

$lodRows = @(
    Get-Array $scan.candidateLodSummary | ForEach-Object {
        , @($_.name, $_.fileCount, (Format-Bytes $_.totalBytes))
    }
)

$largeFileRows = @(
    Select-Top $scan.largeFiles 20 | ForEach-Object {
        , @($_.sourceRoot, $_.relativePath, $_.extension, (Format-Bytes $_.fileSizeBytes), $_.likelyPlateauCategory, $_.candidateLodLevel)
    }
)

$plateauRows = @(
    Select-Top $scan.likelyPlateauRelatedPaths 20 | ForEach-Object {
        , @($_.sourceRoot, $_.relativePath, $_.extension, (Format-Bytes $_.fileSizeBytes), $_.likelyPlateauCategory, $_.candidateLodLevel)
    }
)

$lodPathRows = @(
    Select-Top $scan.lodIndicatorPaths 30 | ForEach-Object {
        , @($_.sourceRoot, $_.relativePath, $_.extension, $_.likelyPlateauCategory, $_.candidateLodLevel)
    }
)

$candidateFolderRows = @(
    Select-Top $scan.candidateFolders 15 | ForEach-Object {
        , @($_.candidateFolder, $_.fileCount, (Format-Bytes $_.totalBytes), $_.likelyCategories, $_.candidateLodLevels, $_.keywordTags, $_.meshCodes)
    }
)

$bridgeRows = @(
    Select-Top $scan.keywordHits.bridge 10 | ForEach-Object {
        , @($_.sourceRoot, $_.relativePath, $_.extension, (Format-Bytes $_.fileSizeBytes), $_.likelyPlateauCategory)
    }
)

$undergroundRows = @(
    Select-Top $scan.keywordHits.underground 10 | ForEach-Object {
        , @($_.sourceRoot, $_.relativePath, $_.extension, (Format-Bytes $_.fileSizeBytes), $_.likelyPlateauCategory)
    }
)

$roadRows = @(
    Select-Top $scan.keywordHits.road 10 | ForEach-Object {
        , @($_.sourceRoot, $_.relativePath, $_.extension, (Format-Bytes $_.fileSizeBytes), $_.likelyPlateauCategory)
    }
)

$riverfrontRows = @(
    Select-Top $scan.keywordHits.riverfront 10 | ForEach-Object {
        , @($_.sourceRoot, $_.relativePath, $_.extension, (Format-Bytes $_.fileSizeBytes), $_.likelyPlateauCategory)
    }
)

$keywordRows = @(
    Get-Array $scan.keywordSummary | ForEach-Object {
        , @($_.name, $_.fileCount, (Format-Bytes $_.totalBytes))
    }
)

$nextScanItems = Get-Array $scan.recommendedNextScanImprovements
$riskItems = Get-Array $scan.risks

$inventoryContent = @"
# P7 Asset Inventory Report

Generated: $($scan.scanTimestamp)

Generated by: tools/p7/run_p7_asset_inventory.ps1

Inventory run ID: $($scan.inventoryRunId)

Branch: $($scan.branch)

Project path: $($scan.projectPath)

Inference policy: $($scan.inferencePolicy)

## Summary

| Metric | Value |
|---|---|
| Total files scanned | $($scan.totals.fileCount) |
| Total bytes scanned | $(Format-Bytes $scan.totals.totalBytes) |
| Large file threshold | $(Format-Bytes $scan.largeFileThresholdBytes) |

## Scanned Roots

$(New-MarkdownTable -Headers @("Root", "Path", "Files", "Bytes") -Rows $rootRows)

## Protected Project Folders Listed Only

$(New-MarkdownTable -Headers @("Folder", "Exists", "Files", "Bytes", "Note") -Rows $specialRows)

## Extension Summary

$(New-MarkdownTable -Headers @("Extension", "Files", "Bytes") -Rows $extensionRows)

## Likely PLATEAU Category Summary

$(New-MarkdownTable -Headers @("Inferred category", "Files", "Bytes") -Rows $categoryRows)

## Large File Summary

$(New-MarkdownTable -Headers @("Root", "Path", "Ext", "Size", "Category", "LOD") -Rows $largeFileRows)

## Candidate LOD Path Summary

This section is path/name-based only. It does not verify geometry contents or visual quality.

$(New-MarkdownTable -Headers @("LOD token", "Files", "Bytes") -Rows $lodRows)

$(New-MarkdownTable -Headers @("Root", "Path", "Ext", "Category", "LOD") -Rows $lodPathRows)

## PLATEAU-Related Path Summary

$(New-MarkdownTable -Headers @("Root", "Path", "Ext", "Size", "Category", "LOD") -Rows $plateauRows)

## Keyword Hit Summary

$(New-MarkdownTable -Headers @("Keyword family", "Files", "Bytes") -Rows $keywordRows)

### Bridge Keyword Samples

$(New-MarkdownTable -Headers @("Root", "Path", "Ext", "Size", "Category") -Rows $bridgeRows)

### Underground Keyword Samples

$(New-MarkdownTable -Headers @("Root", "Path", "Ext", "Size", "Category") -Rows $undergroundRows)

### Road Keyword Samples

$(New-MarkdownTable -Headers @("Root", "Path", "Ext", "Size", "Category") -Rows $roadRows)

### Riverfront Keyword Samples

$(New-MarkdownTable -Headers @("Root", "Path", "Ext", "Size", "Category") -Rows $riverfrontRows)

## Candidate Folders For P7 Benchmark

$(New-MarkdownTable -Headers @("Folder", "Files", "Bytes", "Categories", "LOD tokens", "Keyword tags", "Mesh codes") -Rows $candidateFolderRows)

## Risks

$(New-MarkdownList -Values $riskItems)

## Recommended Next Scan Improvements

$(New-MarkdownList -Values $nextScanItems)
"@

Set-Content -LiteralPath $inventoryReportPath -Value $inventoryContent -Encoding UTF8

$categoryLodRows = @(
    Select-Top $scan.categoryLodSummary 30 | ForEach-Object {
        , @($_.name, $_.fileCount, (Format-Bytes $_.totalBytes))
    }
)

$meshCodeRows = @(
    Select-Top $scan.meshCodeSummary 25 | ForEach-Object {
        , @($_.name, $_.fileCount, (Format-Bytes $_.totalBytes))
    }
)

$lodAvailabilityNotes = New-Object System.Collections.Generic.List[string]
$nonUnknownLodCount = @(Get-Array $scan.candidateLodSummary | Where-Object { $_.name -ne "unknown" }).Count
if ($nonUnknownLodCount -eq 0) {
    $lodAvailabilityNotes.Add("No LOD1/LOD2/LOD3/LOD4 tokens were found in scanned paths or filenames.") | Out-Null
    $lodAvailabilityNotes.Add("This does not prove high-detail data is unavailable because this P7-A scanner does not parse CityGML geometry tags.") | Out-Null
}
else {
    $lodAvailabilityNotes.Add("LOD tokens were found in paths or filenames and should be verified with geometry-aware checks before import decisions.") | Out-Null
}
$lodAvailabilityNotes.Add("LOD availability below is inference only, not verified geometry quality.") | Out-Null

$lodContent = @"
# P7 LOD Availability Report

Generated: $($scan.scanTimestamp)

Inventory run ID: $($scan.inventoryRunId)

Branch: $($scan.branch)

## Scope

This report is populated from path, filename, extension, and file-size metadata only. It does not parse CityGML contents, open Unity, inspect meshes, or verify geometry quality.

## Availability Notes

$(New-MarkdownList -Values @($lodAvailabilityNotes))

## LOD Token Summary

$(New-MarkdownTable -Headers @("LOD token", "Files", "Bytes") -Rows $lodRows)

## Category And LOD Cross-Check

$(New-MarkdownTable -Headers @("Category and LOD", "Files", "Bytes") -Rows $categoryLodRows)

## Candidate LOD Paths

$(New-MarkdownTable -Headers @("Root", "Path", "Ext", "Category", "LOD") -Rows $lodPathRows)

## Mesh-Code Hints

Mesh-code hints are filename-based and useful only as a first pass for narrowing later P7-B benchmark areas.

$(New-MarkdownTable -Headers @("Mesh code", "Files", "Bytes") -Rows $meshCodeRows)

## Follow-Up Required Before Import

- Confirm actual CityGML LOD tags or Unity imported mesh details with an approved geometry-aware check.
- Confirm material and texture counts in Unity only during the benchmark stage.
- Treat missing path/name LOD tokens as unknown, not as proof of missing LOD data.
"@

Set-Content -LiteralPath $lodReportPath -Value $lodContent -Encoding UTF8

$candidateSource = @(
    Get-Array $scan.candidateFolders |
        Where-Object {
            $categories = [string]$_.likelyCategories
            $keywords = [string]$_.keywordTags
            $folder = [string]$_.candidateFolder
            $isAssetCandidateFolder = (
                $folder.StartsWith("Local PLATEAU source/udx/", [System.StringComparison]::OrdinalIgnoreCase) -or
                $folder.StartsWith("Assets/PLATEAU", [System.StringComparison]::OrdinalIgnoreCase)
            )
            if (-not $isAssetCandidateFolder) {
                return $false
            }

            $categories.IndexOf("Buildings", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $categories.IndexOf("Roads", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $categories.IndexOf("Bridges", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $categories.IndexOf("Underground", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $categories.IndexOf("Water", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $keywords.IndexOf("bridge", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $keywords.IndexOf("road", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $keywords.IndexOf("underground", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $keywords.IndexOf("riverfront", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
        } |
        Select-Object -First 5
)

if ($candidateSource.Count -eq 0) {
    $candidateSource = @(Select-Top $scan.candidateFolders 5)
}

$benchmarkRows = @(
    $candidateSource | ForEach-Object {
        $controlLevel = Get-ControlLevel -FileCount ([int]$_.fileCount) -TotalBytes ([int64]$_.totalBytes)
        $importRisk = Get-ImportRisk -FileCount ([int]$_.fileCount) -TotalBytes ([int64]$_.totalBytes) -Categories ([string]$_.likelyCategories)
        $lodEvidence = [string]$_.candidateLodLevels
        if ([string]::IsNullOrWhiteSpace($lodEvidence) -or $lodEvidence -eq "unknown") {
            $lodEvidence = "Unknown by path/name"
        }
        $categoryParts = @(([string]$_.likelyCategories) -split "," | ForEach-Object { $_.Trim() })
        $shelterRelevance = if($categoryParts -contains "Buildings") { "High-rise/shelter context candidate" } else { "Indirect context only" }
        $bridgeRiverfront = if(([string]$_.keywordTags).IndexOf("bridge", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or ([string]$_.keywordTags).IndexOf("riverfront", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or ([string]$_.likelyCategories).IndexOf("Water", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) { "Relevant" } else { "Not evident" }
        $undergroundRoad = if(([string]$_.keywordTags).IndexOf("underground", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or ([string]$_.keywordTags).IndexOf("road", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or ([string]$_.likelyCategories).IndexOf("Roads", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or ([string]$_.likelyCategories).IndexOf("Underground", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) { "Relevant" } else { "Not evident" }

        , @(
            $_.candidateFolder,
            $_.fileCount,
            (Format-Bytes $_.totalBytes),
            $_.likelyCategories,
            $lodEvidence,
            $shelterRelevance,
            $bridgeRiverfront,
            $undergroundRoad,
            $controlLevel,
            $importRisk,
            (Get-WindowsBenchmarkSuitability -Candidate $_ -ControlLevel $controlLevel -ImportRisk $importRisk)
        )
    }
)

$candidateContent = @"
# P7 Benchmark Area Candidates

Generated: $($scan.scanTimestamp)

Inventory run ID: $($scan.inventoryRunId)

Branch: $($scan.branch)

## Selection Position

No final benchmark area is selected by this report. The candidates below are path/name and file-metadata hints only, intended to narrow P7-B planning after human review.

## Criteria

| Criterion | Use |
|---|---|
| LOD3/LOD4 availability | Prefer candidates with explicit high-detail path/name evidence, then verify geometry before import. |
| Shelter/high-rise relevance | Prefer building-heavy areas that can support evacuation-building presentation needs. |
| Bridge/riverfront relevance | Prefer areas that show waterfront or bridge continuity for Chuo context. |
| Underground/road relevance | Prefer transportation or underground-related categories only when local assets provide evidence. |
| Asset size controllability | Prefer small, bounded folders or mesh-code slices before broad imports. |
| Risk of scene/import instability | Treat very large source sets or high file counts as risk until benchmarked. |
| Windows EXE benchmark suitability | Prefer candidates representative enough to measure but small enough to build and rerun. |

## Candidate Areas Or Categories

$(New-MarkdownTable -Headers @("Candidate", "Files", "Bytes", "Categories", "LOD evidence", "Shelter/high-rise", "Bridge/riverfront", "Underground/road", "Size control", "Import risk", "Windows EXE suitability") -Rows $benchmarkRows)

## Recommendation

- Carry 2-5 of the candidates above into P7-B planning only after confirming exact area bounds.
- If all LOD evidence remains unknown, start P7-B with the smallest representative category or mesh-code slice instead of a broad import.
- Keep generated Unity scenes, imported PLATEAU assets, and build outputs local-only unless a later prompt explicitly approves a tracked artifact.
"@

Set-Content -LiteralPath $candidateReportPath -Value $candidateContent -Encoding UTF8

Write-Host "P7 asset inventory report written: $(ConvertTo-RepoPath $inventoryReportPath)"
Write-Host "P7 LOD availability report written: $(ConvertTo-RepoPath $lodReportPath)"
Write-Host "P7 benchmark candidates report written: $(ConvertTo-RepoPath $candidateReportPath)"
