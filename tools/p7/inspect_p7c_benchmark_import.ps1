[CmdletBinding()]
param(
    [string]$ImportRootRelative = "Assets/P7Benchmark/Imported/53393690",
    [string]$OutputMarkdownPath = ""
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$importRoot = Join-Path $repoRoot ($ImportRootRelative -replace "/", "\")

function Format-Bytes {
    param([int64]$Bytes)

    $mb = [Math]::Round($Bytes / 1MB, 2)
    return "$Bytes bytes ($mb MB)"
}

function Get-RepoRelativePath {
    param([string]$FullPath)

    $rootWithSeparator = $repoRoot.TrimEnd("\") + "\"
    if ($FullPath.StartsWith($rootWithSeparator, [System.StringComparison]::OrdinalIgnoreCase)) {
        return ($FullPath.Substring($rootWithSeparator.Length) -replace "\\", "/")
    }

    return ($FullPath -replace "\\", "/")
}

function Get-P7CGroupName {
    param([System.IO.FileInfo]$File)

    $relative = Get-RepoRelativePath -FullPath $File.FullName
    $underImport = $relative.Substring($ImportRootRelative.Length).TrimStart("/")
    $parts = @($underImport -split "/")

    if ($parts.Count -ge 2 -and $parts[0].Equals("udx", [System.StringComparison]::OrdinalIgnoreCase)) {
        return $parts[1].ToLowerInvariant()
    }

    if ($parts.Count -gt 0 -and -not [string]::IsNullOrWhiteSpace($parts[0])) {
        return $parts[0].ToLowerInvariant()
    }

    return "root"
}

function New-TableLines {
    param(
        [string[]]$Headers,
        [object[]]$Rows
    )

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("| " + ($Headers -join " | ") + " |") | Out-Null
    $lines.Add("| " + (($Headers | ForEach-Object { "---" }) -join " | ") + " |") | Out-Null

    foreach ($row in $Rows) {
        $values = foreach ($header in $Headers) {
            $value = $row.$header
            if ($null -eq $value) {
                ""
            }
            else {
                ([string]$value).Replace("|", "\|")
            }
        }

        $lines.Add("| " + ($values -join " | ") + " |") | Out-Null
    }

    return @($lines)
}

if (-not (Test-Path -LiteralPath $importRoot -PathType Container)) {
    Write-Host "P7-C import inspection: FAIL"
    Write-Host "Missing import root: $ImportRootRelative"
    exit 1
}

$allFiles = @(
    Get-ChildItem -LiteralPath $importRoot -Recurse -File -Force -ErrorAction SilentlyContinue |
        Where-Object { -not $_.Extension.Equals(".meta", [System.StringComparison]::OrdinalIgnoreCase) }
)

$totalBytes = [int64]0
foreach ($file in $allFiles) {
    $totalBytes += [int64]$file.Length
}

$renderableExtensions = @(".fbx", ".obj", ".dae", ".blend", ".gltf", ".glb", ".prefab", ".mesh", ".asset")
$textureExtensions = @(".jpg", ".jpeg", ".png", ".tga", ".tif", ".tiff", ".psd", ".exr")
$gmlFiles = @($allFiles | Where-Object { $_.Extension.Equals(".gml", [System.StringComparison]::OrdinalIgnoreCase) })
$textureFiles = @($allFiles | Where-Object { $textureExtensions -contains $_.Extension.ToLowerInvariant() })
$renderableFiles = @($allFiles | Where-Object { $renderableExtensions -contains $_.Extension.ToLowerInvariant() })
$rawCityGmlUnconverted = $gmlFiles.Count -gt 0 -and $renderableFiles.Count -eq 0

$extensionRows = @(
    $allFiles |
        Group-Object { if ([string]::IsNullOrWhiteSpace($_.Extension)) { "<none>" } else { $_.Extension.ToLowerInvariant() } } |
        Sort-Object Name |
        ForEach-Object {
            $bytes = [int64]0
            foreach ($file in $_.Group) {
                $bytes += [int64]$file.Length
            }

            [pscustomobject]@{
                Extension = $_.Name
                Count = $_.Count
                Bytes = $bytes
                SizeMB = [Math]::Round($bytes / 1MB, 2)
            }
        }
)

$groupRows = @(
    $allFiles |
        Group-Object { Get-P7CGroupName -File $_ } |
        Sort-Object Name |
        ForEach-Object {
            $bytes = [int64]0
            foreach ($file in $_.Group) {
                $bytes += [int64]$file.Length
            }

            [pscustomobject]@{
                Group = $_.Name
                Count = $_.Count
                Bytes = $bytes
                SizeMB = [Math]::Round($bytes / 1MB, 2)
            }
        }
)

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# P7-C 53393690 Benchmark Import Inspection") | Out-Null
$lines.Add("") | Out-Null
$lines.Add("- Import root: $ImportRootRelative") | Out-Null
$lines.Add("- Non-meta file count: $($allFiles.Count)") | Out-Null
$lines.Add("- Non-meta bytes: $(Format-Bytes -Bytes $totalBytes)") | Out-Null
$lines.Add("- CityGML file count: $($gmlFiles.Count)") | Out-Null
$lines.Add("- Texture file count: $($textureFiles.Count)") | Out-Null
$lines.Add("- Renderable Unity/model asset count: $($renderableFiles.Count)") | Out-Null
$lines.Add("- Raw CityGML remains unconverted: $rawCityGmlUnconverted") | Out-Null
$lines.Add("") | Out-Null
$lines.Add("## Extension Summary") | Out-Null
foreach ($line in (New-TableLines -Headers @("Extension", "Count", "Bytes", "SizeMB") -Rows $extensionRows)) {
    $lines.Add([string]$line) | Out-Null
}
$lines.Add("") | Out-Null
$lines.Add("## Candidate Logical Groups") | Out-Null
foreach ($line in (New-TableLines -Headers @("Group", "Count", "Bytes", "SizeMB") -Rows $groupRows)) {
    $lines.Add([string]$line) | Out-Null
}
$lines.Add("") | Out-Null

if ($rawCityGmlUnconverted) {
    $lines.Add("Conclusion: candidate `53393690` is present as raw CityGML plus texture source files only. No renderable Unity mesh/model/prefab assets were detected by extension scan.") | Out-Null
}
else {
    $lines.Add("Conclusion: renderable Unity/model assets were detected by extension scan. Visual usability still requires Unity scene and profiler validation.") | Out-Null
}

foreach ($line in $lines) {
    Write-Host $line
}

if (-not [string]::IsNullOrWhiteSpace($OutputMarkdownPath)) {
    $outputPath = $OutputMarkdownPath
    if (-not [System.IO.Path]::IsPathRooted($outputPath)) {
        $outputPath = Join-Path $repoRoot $outputPath
    }

    $outputDirectory = Split-Path -Parent $outputPath
    if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
        New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
    }

    Set-Content -LiteralPath $outputPath -Value $lines -Encoding UTF8
}

exit 0
