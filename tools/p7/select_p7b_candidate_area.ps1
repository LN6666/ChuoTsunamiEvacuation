[CmdletBinding()]
param(
    [ValidateSet("Markdown", "Json")]
    [string]$OutputFormat = "Markdown",
    [string]$PlateauRoot = "D:\PLATEAU_DATA\Chuo_2025_CityGML"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

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

function Get-ReportInfo {
    param([string]$RelativePath)

    $fullPath = Join-Path $repoRoot $RelativePath
    $exists = Test-Path -LiteralPath $fullPath -PathType Leaf
    $bytes = [int64]0
    if ($exists) {
        $bytes = (Get-Item -LiteralPath $fullPath).Length
    }

    [PSCustomObject]@{
        relativePath = ConvertTo-ForwardSlashPath $RelativePath
        exists = $exists
        bytes = $bytes
        size = ConvertTo-DisplaySize $bytes
    }
}

function Get-FilesForCandidate {
    param([hashtable]$Definition)

    $files = New-Object System.Collections.Generic.List[System.IO.FileInfo]

    if (Test-Path -LiteralPath $PlateauRoot -PathType Container) {
        foreach ($category in @($Definition.categories)) {
            $categoryPath = Join-Path $PlateauRoot "udx\$category"
            if (-not (Test-Path -LiteralPath $categoryPath -PathType Container)) {
                continue
            }

            foreach ($file in Get-ChildItem -LiteralPath $categoryPath -File -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "$($Definition.meshCode)*" }) {
                $files.Add($file) | Out-Null
            }
        }

        if ($Definition.ContainsKey("appearanceRelativePath") -and -not [string]::IsNullOrWhiteSpace($Definition.appearanceRelativePath)) {
            $appearancePath = Join-Path $PlateauRoot $Definition.appearanceRelativePath
            if (Test-Path -LiteralPath $appearancePath -PathType Container) {
                foreach ($file in Get-ChildItem -LiteralPath $appearancePath -File -Force -ErrorAction SilentlyContinue) {
                    $files.Add($file) | Out-Null
                }
            }
        }
    }

    return @($files.ToArray())
}

function Get-RelativePlateauPath {
    param([System.IO.FileInfo]$File)

    $root = [System.IO.Path]::GetFullPath($PlateauRoot).TrimEnd([char[]]@("\", "/"))
    $target = [System.IO.Path]::GetFullPath($File.FullName)
    $rootUri = New-Object System.Uri (($root + [System.IO.Path]::DirectorySeparatorChar))
    $targetUri = New-Object System.Uri $target
    return ConvertTo-ForwardSlashPath ([System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($targetUri).ToString()))
}

function Get-SupportingFolderSummary {
    param([string]$RelativePath)

    $fullPath = Join-Path $PlateauRoot $RelativePath
    $files = @()
    if (Test-Path -LiteralPath $fullPath -PathType Container) {
        $files = @(Get-ChildItem -LiteralPath $fullPath -File -Force -ErrorAction SilentlyContinue)
    }

    $bytes = [int64]0
    foreach ($file in $files) {
        $bytes += [int64]$file.Length
    }

    [PSCustomObject]@{
        path = ConvertTo-ForwardSlashPath ("Local PLATEAU source/$RelativePath")
        exists = (Test-Path -LiteralPath $fullPath -PathType Container)
        files = $files.Count
        bytes = $bytes
        size = ConvertTo-DisplaySize $bytes
    }
}

$reportInfos = @(
    Get-ReportInfo -RelativePath "docs\P7_ASSET_INVENTORY_REPORT.md"
    Get-ReportInfo -RelativePath "docs\P7_LOD_AVAILABILITY_REPORT.md"
    Get-ReportInfo -RelativePath "docs\P7_BENCHMARK_AREA_CANDIDATES.md"
)

$candidateDefinitions = @(
    @{
        id = "C1"
        label = "53393690 LOD3 building / bridge / road cluster"
        meshCode = "53393690"
        categories = @("bldg", "brid", "tran")
        appearanceRelativePath = "udx\bldg\53393690_bldg_6697_appearance"
        rationale = "Strongest current LOD3 path/name signal; includes building, bridge, and road source files in one mesh-code cluster."
        risk = "Large building GML and many texture files; LOD3 signal is texture-path based and geometry is unverified."
        decisionUse = "Primary candidate for path/name-based LOD3 feasibility planning; not a final Unity import approval."
        lod3Score = 5
        sizeScore = 2
        goalScore = 4
        riskScore = 2
        importScore = 2
        exeScore = 4
    },
    @{
        id = "C2"
        label = "53393672 compact bridge / road / building cluster"
        meshCode = "53393672"
        categories = @("bldg", "brid", "tran")
        appearanceRelativePath = ""
        rationale = "Small, bounded bridge and road candidate with building context; useful if import-size control is the first priority."
        risk = "No LOD3/LOD4 path token evidence in current inventory; geometry and visual usefulness are unverified."
        decisionUse = "Fallback benchmark slice for bridge/road controllability."
        lod3Score = 0
        sizeScore = 5
        goalScore = 4
        riskScore = 4
        importScore = 4
        exeScore = 3
    },
    @{
        id = "C3"
        label = "53394611 underground / road / building cluster"
        meshCode = "53394611"
        categories = @("bldg", "ubld", "tran", "brid")
        appearanceRelativePath = "udx\bldg\53394611_bldg_6697_appearance"
        rationale = "Only explicit underground-building mesh-code file found by the P7-A path/name inventory, with nearby road, bridge, and building files."
        risk = "No LOD3/LOD4 path token evidence in this cluster; total footprint is large if all context files are included."
        decisionUse = "Fallback candidate for underground feasibility after the first LOD3 candidate is assessed."
        lod3Score = 0
        sizeScore = 2
        goalScore = 5
        riskScore = 2
        importScore = 2
        exeScore = 4
    }
)

$candidateSummaries = New-Object System.Collections.Generic.List[object]
foreach ($definition in $candidateDefinitions) {
    $files = @(Get-FilesForCandidate -Definition $definition)
    $bytes = [int64]0
    $lod3Hits = 0
    $lod4Hits = 0
    $samples = New-Object System.Collections.Generic.List[string]

    foreach ($file in $files) {
        $bytes += [int64]$file.Length
        $relativePath = Get-RelativePlateauPath -File $file
        if ($relativePath -match "(?i)lod[\s_-]*3") {
            $lod3Hits++
        }
        if ($relativePath -match "(?i)lod[\s_-]*4") {
            $lod4Hits++
        }
        if ($samples.Count -lt 6) {
            $samples.Add($relativePath) | Out-Null
        }
    }

    $scoreTotal = [int]$definition.lod3Score + [int]$definition.sizeScore + [int]$definition.goalScore + [int]$definition.riskScore + [int]$definition.importScore + [int]$definition.exeScore
    $candidateSummaries.Add([PSCustomObject]@{
        id = [string]$definition.id
        label = [string]$definition.label
        meshCode = [string]$definition.meshCode
        files = $files.Count
        bytes = $bytes
        size = ConvertTo-DisplaySize $bytes
        lod3PathHits = $lod3Hits
        lod4PathHits = $lod4Hits
        categories = (@($definition.categories) -join ", ")
        rationale = [string]$definition.rationale
        risk = [string]$definition.risk
        decisionUse = [string]$definition.decisionUse
        scoreTotal = $scoreTotal
        lod3Score = [int]$definition.lod3Score
        sizeScore = [int]$definition.sizeScore
        goalScore = [int]$definition.goalScore
        riskScore = [int]$definition.riskScore
        importScore = [int]$definition.importScore
        exeScore = [int]$definition.exeScore
        samplePaths = @($samples.ToArray())
    }) | Out-Null
}

$supportingFolders = @(
    Get-SupportingFolderSummary -RelativePath "udx\wtr"
    Get-SupportingFolderSummary -RelativePath "udx\ubld"
    Get-SupportingFolderSummary -RelativePath "udx\brid"
    Get-SupportingFolderSummary -RelativePath "udx\tran"
)

$lod4PathHits = 0
if (Test-Path -LiteralPath $PlateauRoot -PathType Container) {
    $lod4PathHits = @(
        Get-ChildItem -LiteralPath $PlateauRoot -Recurse -File -Force -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match "(?i)lod[\s_-]*4" } |
            Select-Object -First 1
    ).Count
}

$result = [PSCustomObject]@{
    schemaVersion = "p7b.areaFeasibility.selection.v1"
    generatedAt = (Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")
    inferencePolicy = "Path/name/file-metadata inference only. CityGML geometry, Unity import quality, visual correctness, and gameplay suitability are not verified."
    plateauRoot = $PlateauRoot
    plateauRootExists = (Test-Path -LiteralPath $PlateauRoot -PathType Container)
    p7Reports = $reportInfos
    lod4PathHitsFound = $lod4PathHits
    recommendation = "Carry C1, C2, and C3 into P7-B planning. Treat C1 as the primary LOD3 path/name candidate, but keep final Unity import selection pending until geometry-aware inspection and human approval."
    candidates = @($candidateSummaries.ToArray())
    supportingFolders = $supportingFolders
}

if ($OutputFormat -eq "Json") {
    $result | ConvertTo-Json -Depth 8
    exit 0
}

$reportRows = New-Object System.Collections.Generic.List[object]
foreach ($reportInfo in $reportInfos) {
    $reportRows.Add(@($reportInfo.relativePath, $reportInfo.exists, $reportInfo.size)) | Out-Null
}

$candidateRows = New-Object System.Collections.Generic.List[object]
foreach ($candidate in $candidateSummaries) {
    $candidateRows.Add(@($candidate.id, $candidate.label, $candidate.files, $candidate.size, $candidate.categories, $candidate.lod3PathHits, $candidate.lod4PathHits, $candidate.scoreTotal, $candidate.decisionUse)) | Out-Null
}

$scoreRows = New-Object System.Collections.Generic.List[object]
foreach ($candidate in $candidateSummaries) {
    $scoreRows.Add(@($candidate.id, $candidate.lod3Score, $candidate.sizeScore, $candidate.goalScore, $candidate.riskScore, $candidate.importScore, $candidate.exeScore, $candidate.scoreTotal)) | Out-Null
}

$supportingRows = New-Object System.Collections.Generic.List[object]
foreach ($folder in $supportingFolders) {
    $supportingRows.Add(@($folder.path, $folder.exists, $folder.files, $folder.size)) | Out-Null
}

$sampleLines = New-Object System.Collections.Generic.List[string]
foreach ($candidate in $candidateSummaries) {
    $sampleLines.Add("### $($candidate.id) Samples") | Out-Null
    if ($candidate.samplePaths.Count -eq 0) {
        $sampleLines.Add("- No local files found for this candidate definition.") | Out-Null
    }
    else {
        foreach ($samplePath in $candidate.samplePaths) {
            $sampleLines.Add("- ``Local PLATEAU source/$samplePath``") | Out-Null
        }
    }
    $sampleLines.Add("") | Out-Null
}

@"
# P7-B Candidate Area Selection Summary

Generated: $($result.generatedAt)

Inference policy: $($result.inferencePolicy)

PLATEAU root: ``$PlateauRoot``

PLATEAU root exists: ``$($result.plateauRootExists)``

## P7-A Report Inputs

$(New-MarkdownTable -Headers @("Report", "Exists", "Size") -Rows $reportRows.ToArray())

## Candidate Summary

$(New-MarkdownTable -Headers @("ID", "Candidate", "Files", "Size", "Categories", "LOD3 path hits", "LOD4 path hits", "Score", "Use") -Rows $candidateRows.ToArray())

## Score Detail

Scores are conservative planning scores from 0 to 5. They are not quality measurements.

$(New-MarkdownTable -Headers @("ID", "LOD3 availability", "Size control", "Goal relation", "Risk", "Unity import ease", "EXE benchmark usefulness", "Total") -Rows $scoreRows.ToArray())

## Supporting Category Folders

$(New-MarkdownTable -Headers @("Folder", "Exists", "Files", "Size") -Rows $supportingRows.ToArray())

## LOD4 Path/Name Check

LOD4 path/name hits found in the local PLATEAU source: ``$lod4PathHits``.

Do not treat this as geometry proof. A zero result means no ``lod4`` path/name token was found by this helper, not that CityGML contents were parsed.

## Recommendation

$($result.recommendation)

## Candidate Samples

$($sampleLines.ToArray() -join [Environment]::NewLine)
"@

exit 0
