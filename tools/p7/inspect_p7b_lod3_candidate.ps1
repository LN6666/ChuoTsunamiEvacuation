[CmdletBinding()]
param(
    [ValidateSet("Markdown", "Json")]
    [string]$OutputFormat = "Markdown",

    [string]$PlateauRoot = "D:\PLATEAU_DATA\Chuo_2025_CityGML",

    [int]$SamplePathLimit = 10,

    [int]$LargestFileLimit = 8,

    [int64]$SmallImportThresholdBytes = 104857600,

    [int64]$HeavyImportThresholdBytes = 786432000
)

$ErrorActionPreference = "Stop"

function ConvertTo-DisplaySize {
    param([int64]$Bytes)

    if ($Bytes -ge 1GB) {
        return ("{0:N2} GB" -f ($Bytes / 1GB))
    }

    if ($Bytes -ge 1MB) {
        return ("{0:N2} MB" -f ($Bytes / 1MB))
    }

    if ($Bytes -ge 1KB) {
        return ("{0:N2} KB" -f ($Bytes / 1KB))
    }

    return "$Bytes B"
}

function ConvertTo-ForwardSlashPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ""
    }

    return ($Path -replace "\\", "/")
}

function ConvertTo-CellText {
    param([object]$Value)

    if ($null -eq $Value) {
        return ""
    }

    $text = [string]$Value
    return (($text -replace "\r?\n", " ") -replace "\|", "/")
}

function New-MarkdownTable {
    param(
        [string[]]$Headers,
        [object[]]$Rows
    )

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("| $($Headers -join " | ") |") | Out-Null
    $lines.Add("| $(($Headers | ForEach-Object { "---" }) -join " | ") |") | Out-Null

    foreach ($row in $Rows) {
        $cells = @($row | ForEach-Object { ConvertTo-CellText $_ })
        $lines.Add("| $($cells -join " | ") |") | Out-Null
    }

    return ($lines.ToArray() -join [Environment]::NewLine)
}

function Get-RelativePlateauPath {
    param([System.IO.FileInfo]$File)

    $root = [System.IO.Path]::GetFullPath($PlateauRoot).TrimEnd([char[]]@("\", "/"))
    $target = [System.IO.Path]::GetFullPath($File.FullName)
    $rootUri = New-Object System.Uri (($root + [System.IO.Path]::DirectorySeparatorChar))
    $targetUri = New-Object System.Uri $target
    return ConvertTo-ForwardSlashPath ([System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($targetUri).ToString()))
}

function Get-UdxRelativePath {
    param([string]$RelativePlateauPath)

    if ($RelativePlateauPath.StartsWith("udx/", [System.StringComparison]::OrdinalIgnoreCase)) {
        return $RelativePlateauPath.Substring(4)
    }

    return $RelativePlateauPath
}

function Get-CategoryHint {
    param([string]$RelativePlateauPath)

    $udxRelativePath = Get-UdxRelativePath -RelativePlateauPath $RelativePlateauPath
    $firstSlash = $udxRelativePath.IndexOf("/")
    if ($firstSlash -le 0) {
        return "unknown"
    }

    return $udxRelativePath.Substring(0, $firstSlash)
}

function Get-LodIndicators {
    param([string]$Path)

    $matches = [regex]::Matches($Path, "(?i)lod[\s_-]*([0-9])")
    $values = New-Object System.Collections.Generic.List[string]
    foreach ($match in $matches) {
        $values.Add("LOD$($match.Groups[1].Value)") | Out-Null
    }

    return @($values.ToArray() | Sort-Object -Unique)
}

function Get-ImportEstimate {
    param(
        [int64]$Bytes,
        [int]$FileCount,
        [int]$Lod3PathHits
    )

    if ($FileCount -eq 0) {
        return [PSCustomObject]@{
            level = "missing"
            appearsSmallEnough = $false
            summary = "No files were found for this candidate in the local PLATEAU source."
        }
    }

    if ($Bytes -le $SmallImportThresholdBytes) {
        return [PSCustomObject]@{
            level = "bounded-small"
            appearsSmallEnough = $true
            summary = "Small enough for a future isolated import experiment, but still requires explicit human approval and rollback criteria."
        }
    }

    if ($Bytes -le $HeavyImportThresholdBytes) {
        return [PSCustomObject]@{
            level = "bounded-heavy"
            appearsSmallEnough = $false
            summary = "Bounded but not small. A future experiment should narrow scope before import and must receive explicit human approval."
        }
    }

    return [PSCustomObject]@{
        level = "too-heavy-wholesale"
        appearsSmallEnough = $false
        summary = "Too heavy for a first wholesale import experiment. Narrow the candidate before any approved Unity import."
    }
}

$candidateDefinitions = @(
    [PSCustomObject]@{
        meshCode = "53393690"
        role = "preferred planning candidate"
        coreCategories = @("bldg", "brid", "tran")
        rationale = "Strongest current LOD3 path/name signal from P7-B Wave 1. Still not geometry-quality verified."
    },
    [PSCustomObject]@{
        meshCode = "53393672"
        role = "fallback planning candidate"
        coreCategories = @("bldg", "brid", "tran")
        rationale = "Compact fallback for bridge, road, and building context if size control is the first priority."
    },
    [PSCustomObject]@{
        meshCode = "53394611"
        role = "fallback planning candidate"
        coreCategories = @("bldg", "ubld", "tran", "brid")
        rationale = "Fallback with underground-building metadata signal from P7-A/P7-B findings."
    }
)

$plateauRootExists = Test-Path -LiteralPath $PlateauRoot -PathType Container
$udxRoot = Join-Path $PlateauRoot "udx"
$udxRootExists = Test-Path -LiteralPath $udxRoot -PathType Container

$allSourceFiles = @()
if ($udxRootExists) {
    $allSourceFiles = @(Get-ChildItem -LiteralPath $udxRoot -Recurse -File -Force -ErrorAction SilentlyContinue)
}

$candidateSummaries = New-Object System.Collections.Generic.List[object]

foreach ($definition in $candidateDefinitions) {
    $meshCode = [string]$definition.meshCode
    $files = @($allSourceFiles | Where-Object {
        $_.FullName.IndexOf($meshCode, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    })

    $fileRecords = New-Object System.Collections.Generic.List[object]
    foreach ($file in $files) {
        $relativePath = Get-RelativePlateauPath -File $file
        $category = Get-CategoryHint -RelativePlateauPath $relativePath
        $lodIndicators = @(Get-LodIndicators -Path $relativePath)

        $fileRecords.Add([PSCustomObject]@{
            path = $relativePath
            bytes = [int64]$file.Length
            size = ConvertTo-DisplaySize ([int64]$file.Length)
            extension = $file.Extension.ToLowerInvariant()
            category = $category
            lodIndicators = $lodIndicators
            lastWriteTime = $file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
        }) | Out-Null
    }

    $totalBytes = [int64]0
    foreach ($record in $fileRecords) {
        $totalBytes += [int64]$record.bytes
    }

    $lod3PathHits = @($fileRecords | Where-Object { @($_.lodIndicators) -contains "LOD3" }).Count
    $lod4PathHits = @($fileRecords | Where-Object { @($_.lodIndicators) -contains "LOD4" }).Count
    $observedCategories = @($fileRecords | ForEach-Object { $_.category } | Sort-Object -Unique)
    $extraCategories = @($observedCategories | Where-Object { @($definition.coreCategories) -notcontains $_ })

    $extensionSummary = @(
        $fileRecords |
            Group-Object extension |
            Sort-Object @{ Expression = "Count"; Descending = $true }, Name |
            ForEach-Object {
                $bytes = [int64]0
                foreach ($item in $_.Group) {
                    $bytes += [int64]$item.bytes
                }

                [PSCustomObject]@{
                    extension = if ([string]::IsNullOrWhiteSpace($_.Name)) { "(none)" } else { $_.Name }
                    files = $_.Count
                    bytes = $bytes
                    size = ConvertTo-DisplaySize $bytes
                }
            }
    )

    $categorySummary = @(
        $fileRecords |
            Group-Object category |
            Sort-Object @{ Expression = "Count"; Descending = $true }, Name |
            ForEach-Object {
                $bytes = [int64]0
                foreach ($item in $_.Group) {
                    $bytes += [int64]$item.bytes
                }

                [PSCustomObject]@{
                    category = $_.Name
                    files = $_.Count
                    bytes = $bytes
                    size = ConvertTo-DisplaySize $bytes
                }
            }
    )

    $largestFiles = @(
        $fileRecords |
            Sort-Object bytes -Descending |
            Select-Object -First $LargestFileLimit
    )

    $sampleFiles = @(
        $fileRecords |
            Sort-Object path |
            Select-Object -First $SamplePathLimit
    )

    $lod3Samples = @(
        $fileRecords |
            Where-Object { @($_.lodIndicators) -contains "LOD3" } |
            Sort-Object path |
            Select-Object -First $SamplePathLimit
    )

    $candidateSummaries.Add([PSCustomObject]@{
        meshCode = $meshCode
        role = [string]$definition.role
        rationale = [string]$definition.rationale
        plannedCoreCategories = @($definition.coreCategories)
        observedCategories = $observedCategories
        observedExtraCategories = $extraCategories
        files = $fileRecords.Count
        bytes = $totalBytes
        size = ConvertTo-DisplaySize $totalBytes
        lod3PathHits = $lod3PathHits
        lod4PathHits = $lod4PathHits
        extensionSummary = $extensionSummary
        categorySummary = $categorySummary
        largestFiles = $largestFiles
        sampleFiles = $sampleFiles
        lod3Samples = $lod3Samples
        futureImportEstimate = Get-ImportEstimate -Bytes $totalBytes -FileCount $fileRecords.Count -Lod3PathHits $lod3PathHits
    }) | Out-Null
}

$globalLod4PathHits = @($allSourceFiles | Where-Object {
    $_.FullName -match "(?i)lod[\s_-]*4"
}).Count

$result = [PSCustomObject]@{
    schemaVersion = "p7b.wave2b.lod3CandidateDryRun.v1"
    generatedAt = (Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")
    inferencePolicy = "File/path metadata only. This script uses Get-ChildItem and FileInfo metadata; it does not parse CityGML geometry, import assets, copy files, delete files, move files, or modify Unity content."
    plateauRoot = $PlateauRoot
    plateauRootExists = $plateauRootExists
    udxRootExists = $udxRootExists
    primaryCandidate = "53393690"
    fallbackCandidates = @("53393672", "53394611")
    lod4PathHitsFoundUnderUdx = $globalLod4PathHits
    dryRunOnly = $true
    noAssetImportPerformed = $true
    noAssetCopyPerformed = $true
    candidates = @($candidateSummaries.ToArray())
}

if ($OutputFormat -eq "Json") {
    $result | ConvertTo-Json -Depth 10
    exit 0
}

$overviewRows = New-Object System.Collections.Generic.List[object]
foreach ($candidate in $candidateSummaries) {
    $overviewRows.Add(@(
        $candidate.meshCode,
        $candidate.role,
        $candidate.files,
        $candidate.size,
        ($candidate.observedCategories -join ", "),
        $candidate.lod3PathHits,
        $candidate.lod4PathHits,
        $candidate.futureImportEstimate.level
    )) | Out-Null
}

$detailSections = New-Object System.Collections.Generic.List[string]
foreach ($candidate in $candidateSummaries) {
    $categoryRows = New-Object System.Collections.Generic.List[object]
    foreach ($category in $candidate.categorySummary) {
        $categoryRows.Add(@($category.category, $category.files, $category.size)) | Out-Null
    }

    $extensionRows = New-Object System.Collections.Generic.List[object]
    foreach ($extension in $candidate.extensionSummary) {
        $extensionRows.Add(@($extension.extension, $extension.files, $extension.size)) | Out-Null
    }

    $largestRows = New-Object System.Collections.Generic.List[object]
    foreach ($file in $candidate.largestFiles) {
        $largestRows.Add(@($file.path, $file.size, $file.category, (@($file.lodIndicators) -join ", "))) | Out-Null
    }

    $sampleRows = New-Object System.Collections.Generic.List[object]
    foreach ($file in $candidate.sampleFiles) {
        $sampleRows.Add(@($file.path, $file.size, $file.category, (@($file.lodIndicators) -join ", "))) | Out-Null
    }

    $lod3Rows = New-Object System.Collections.Generic.List[object]
    foreach ($file in $candidate.lod3Samples) {
        $lod3Rows.Add(@($file.path, $file.size, $file.category, (@($file.lodIndicators) -join ", "))) | Out-Null
    }

    $lod3Section = "No LOD3 path/name samples found for this candidate."
    if ($lod3Rows.Count -gt 0) {
        $lod3Section = New-MarkdownTable -Headers @("Path", "Size", "Category", "LOD indicators") -Rows $lod3Rows.ToArray()
    }

    $detailSections.Add(@"
## Candidate $($candidate.meshCode)

Role: $($candidate.role)

Rationale: $($candidate.rationale)

Future import estimate: $($candidate.futureImportEstimate.summary)

Planned core categories from P7-B findings: ``$($candidate.plannedCoreCategories -join ", ")``

Observed extra same-mesh-code categories: ``$($candidate.observedExtraCategories -join ", ")``

### Category Summary

$(New-MarkdownTable -Headers @("Category", "Files", "Size") -Rows $categoryRows.ToArray())

### Extension Summary

$(New-MarkdownTable -Headers @("Extension", "Files", "Size") -Rows $extensionRows.ToArray())

### Largest File Samples

$(New-MarkdownTable -Headers @("Path", "Size", "Category", "LOD indicators") -Rows $largestRows.ToArray())

### Path Samples

$(New-MarkdownTable -Headers @("Path", "Size", "Category", "LOD indicators") -Rows $sampleRows.ToArray())

### LOD3 Path/Name Samples

$lod3Section
"@) | Out-Null
}

@"
# P7-B Wave 2-B LOD3 Candidate Dry-Run Inspection

Generated: $($result.generatedAt)

Inference policy: $($result.inferencePolicy)

PLATEAU root: ``$PlateauRoot``

PLATEAU root exists: ``$plateauRootExists``

UDX root exists: ``$udxRootExists``

Primary candidate: ``53393690``

Fallback candidates: ``53393672``, ``53394611``

Dry-run only: ``true``

No asset import performed: ``true``

No asset copy performed: ``true``

## Candidate Overview

$(New-MarkdownTable -Headers @("Mesh code", "Role", "Files", "Size", "Observed categories", "LOD3 path hits", "LOD4 path hits", "Import estimate") -Rows $overviewRows.ToArray())

## LOD4 Path/Name Check

LOD4 path/name hits found under the local ``udx`` root: ``$globalLod4PathHits``.

A zero result is path/name evidence only. It is not geometry parsing proof.

$($detailSections.ToArray() -join ([Environment]::NewLine + [Environment]::NewLine))
"@

exit 0
