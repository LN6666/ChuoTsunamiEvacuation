[CmdletBinding()]
param(
    [string]$PlateauRoot = "D:\PLATEAU_DATA\Chuo_2025_CityGML",
    [string]$CandidateId = "53393690"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$targetRelativeRoot = "Assets/P7Benchmark/Imported/53393690"
$targetRoot = Join-Path $repoRoot ($targetRelativeRoot -replace "/", "\")
$importLogRelativePath = "docs/P7B_WAVE2C_IMPORT_LOG.md"
$importLogPath = Join-Path $repoRoot ($importLogRelativePath -replace "/", "\")

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

function Assert-PathInside {
    param(
        [string]$Path,
        [string]$AllowedRoot,
        [string]$Message
    )

    $resolvedPath = [System.IO.Path]::GetFullPath($Path).TrimEnd([char[]]@("\", "/"))
    $resolvedAllowedRoot = [System.IO.Path]::GetFullPath($AllowedRoot).TrimEnd([char[]]@("\", "/"))

    if (
        -not $resolvedPath.Equals($resolvedAllowedRoot, [System.StringComparison]::OrdinalIgnoreCase) -and
        -not $resolvedPath.StartsWith($resolvedAllowedRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)
    ) {
        throw $Message
    }
}

function Get-RelativePath {
    param(
        [string]$Root,
        [string]$Path
    )

    $rootFull = [System.IO.Path]::GetFullPath($Root).TrimEnd([char[]]@("\", "/"))
    $pathFull = [System.IO.Path]::GetFullPath($Path)
    $rootUri = New-Object System.Uri (($rootFull + [System.IO.Path]::DirectorySeparatorChar))
    $pathUri = New-Object System.Uri $pathFull
    return ConvertTo-ForwardSlashPath ([System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString()))
}

function Get-GitLines {
    param([string[]]$GitArgs)

    $previousErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = "Continue"
        $output = & git @GitArgs 2>$null
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($null -eq $output) {
        return @()
    }

    return @($output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

if ($CandidateId -ne "53393690") {
    throw "Wave 2-C is approved only for candidate 53393690. Refusing candidate $CandidateId."
}

$plateauRootPath = (Resolve-Path -LiteralPath $PlateauRoot).ProviderPath
$udxRoot = Join-Path $plateauRootPath "udx"
if (-not (Test-Path -LiteralPath $udxRoot -PathType Container)) {
    throw "PLATEAU udx root not found: $udxRoot"
}

$repoRootFull = [System.IO.Path]::GetFullPath($repoRoot).TrimEnd([char[]]@("\", "/"))
$targetRootFull = [System.IO.Path]::GetFullPath($targetRoot).TrimEnd([char[]]@("\", "/"))
$allowedTargetRoot = Join-Path $repoRoot "Assets\P7Benchmark\Imported\53393690"
Assert-PathInside -Path $targetRootFull -AllowedRoot $allowedTargetRoot -Message "Target path is outside Assets/P7Benchmark/Imported/53393690."

$forbiddenRepoTargets = @(
    "Assets\PLATEAU",
    "Assets\Data",
    "ProjectSettings",
    "Packages",
    "Assets\Scenes\Chuo_BaseMap.unity"
)

foreach ($forbidden in $forbiddenRepoTargets) {
    $forbiddenFull = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $forbidden)).TrimEnd([char[]]@("\", "/"))
    if (
        $targetRootFull.Equals($forbiddenFull, [System.StringComparison]::OrdinalIgnoreCase) -or
        $targetRootFull.StartsWith($forbiddenFull + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)
    ) {
        throw "Target path would touch forbidden path: $forbidden"
    }
}

$sourceFiles = @(Get-ChildItem -LiteralPath $udxRoot -Recurse -File -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.FullName.IndexOf($CandidateId, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
})

if ($sourceFiles.Count -eq 0) {
    throw "No source files found for candidate $CandidateId under $udxRoot."
}

$requiredRelativeFiles = @(
    "udx/bldg/53393690_bldg_6697_op.gml",
    "udx/brid/53393690_brid_6697_op.gml",
    "udx/tran/53393690_tran_6697_op.gml"
)

$sourceRelativePaths = @($sourceFiles | ForEach-Object { Get-RelativePath -Root $plateauRootPath -Path $_.FullName })
foreach ($requiredRelativeFile in $requiredRelativeFiles) {
    if ($sourceRelativePaths -notcontains $requiredRelativeFile) {
        throw "Required source file for unique candidate identification is missing: $requiredRelativeFile"
    }
}

$otherCandidateHits = @($sourceRelativePaths | Where-Object {
    $_ -match "53393672|53394611"
})
if ($otherCandidateHits.Count -gt 0) {
    throw "Candidate source selection unexpectedly includes fallback candidate paths."
}

$sourceBytes = [int64]0
foreach ($file in $sourceFiles) {
    $sourceBytes += [int64]$file.Length
}

Write-Host "P7-B Wave 2-C import: source $udxRoot"
Write-Host "P7-B Wave 2-C import: target $targetRelativeRoot"
Write-Host "P7-B Wave 2-C import: copying $($sourceFiles.Count) files, $(ConvertTo-DisplaySize $sourceBytes)."

New-Item -ItemType Directory -Force -Path $targetRoot | Out-Null

$copiedFiles = 0
$copiedBytes = [int64]0
$largestFiles = New-Object System.Collections.Generic.List[object]

foreach ($sourceFile in $sourceFiles) {
    $relativePath = Get-RelativePath -Root $plateauRootPath -Path $sourceFile.FullName
    $destinationPath = Join-Path $targetRoot ($relativePath -replace "/", "\")
    Assert-PathInside -Path $destinationPath -AllowedRoot $targetRoot -Message "Destination path escaped approved import root: $destinationPath"

    $destinationDirectory = Split-Path -Parent $destinationPath
    if (-not [string]::IsNullOrWhiteSpace($destinationDirectory)) {
        New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null
    }

    Copy-Item -LiteralPath $sourceFile.FullName -Destination $destinationPath -Force
    $copiedFiles++
    $copiedBytes += [int64]$sourceFile.Length

    $largestFiles.Add([PSCustomObject]@{
        path = $relativePath
        bytes = [int64]$sourceFile.Length
        size = ConvertTo-DisplaySize ([int64]$sourceFile.Length)
    }) | Out-Null
}

if ($copiedFiles -ne $sourceFiles.Count -or $copiedBytes -ne $sourceBytes) {
    throw "Copied file count or byte count does not match source candidate metadata."
}

$extensionRows = @(
    $sourceFiles |
        Group-Object Extension |
        Sort-Object @{ Expression = "Count"; Descending = $true }, Name |
        ForEach-Object {
            $bytes = [int64]0
            foreach ($item in $_.Group) {
                $bytes += [int64]$item.Length
            }

            "| $($(if ([string]::IsNullOrWhiteSpace($_.Name)) { "(none)" } else { $_.Name.ToLowerInvariant() })) | $($_.Count) | $(ConvertTo-DisplaySize $bytes) |"
        }
)

$categoryRows = @(
    $sourceRelativePaths |
        ForEach-Object {
            if ($_ -match "^udx/([^/]+)/") { $Matches[1] } else { "unknown" }
        } |
        Group-Object |
        Sort-Object @{ Expression = "Count"; Descending = $true }, Name |
        ForEach-Object {
            $category = $_.Name
            $categoryFiles = @($sourceFiles | Where-Object {
                $relativePath = Get-RelativePath -Root $plateauRootPath -Path $_.FullName
                $relativePath -like "udx/$category/*"
            })
            $bytes = [int64]0
            foreach ($item in $categoryFiles) {
                $bytes += [int64]$item.Length
            }
            "| $category | $($_.Count) | $(ConvertTo-DisplaySize $bytes) |"
        }
)

$largestRows = @(
    $largestFiles |
        Sort-Object bytes -Descending |
        Select-Object -First 10 |
        ForEach-Object { "| $($_.path) | $($_.size) |" }
)

$gitLfsAttributes = @(Get-GitLines @("-C", $repoRoot, "check-attr", "filter", "--", "$targetRelativeRoot/udx/bldg/53393690_bldg_6697_op.gml"))
$lfsConfigured = $false
foreach ($line in $gitLfsAttributes) {
    if ($line -match "filter:\s*lfs") {
        $lfsConfigured = $true
    }
}

$logContent = @"
# P7-B Wave 2-C Import Log

Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")

Candidate: 53393690

Source root: $plateauRootPath

Target root: $targetRelativeRoot

Imported file count: $copiedFiles

Imported bytes: $copiedBytes

Imported size: $(ConvertTo-DisplaySize $copiedBytes)

Git LFS rule active for target sample: $lfsConfigured

## Scope Statement

This import copies only candidate `53393690` into the isolated P7Benchmark sandbox path.

This is not a full Chuo import, not production scene integration, and not a visual-quality claim.

`Chuo_BaseMap.unity`, `Assets/PLATEAU`, `Assets/Data`, `ProjectSettings`, and `Packages` are not touched by this import script.

## Extension Summary

| Extension | Files | Size |
|---|---:|---:|
$($extensionRows -join [Environment]::NewLine)

## Category Summary

| Category | Files | Size |
|---|---:|---:|
$($categoryRows -join [Environment]::NewLine)

## Largest Files

| Source-relative path | Size |
|---|---:|
$($largestRows -join [Environment]::NewLine)

## Visual Verification

Unity visual usability remains pending. The imported source package contains CityGML and texture files, but this script does not convert CityGML into renderable Unity meshes.
"@

Set-Content -LiteralPath $importLogPath -Value $logContent -Encoding UTF8

Write-Host "P7-B Wave 2-C import: copied $copiedFiles files, $(ConvertTo-DisplaySize $copiedBytes)."
Write-Host "P7-B Wave 2-C import: log written to $importLogRelativePath"
exit 0
