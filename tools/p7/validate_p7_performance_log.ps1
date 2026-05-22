[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Path
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptRoot "..\..")

function Normalize-RepoPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ""
    }

    $normalized = $Path -replace "\\", "/"
    while ($normalized.StartsWith("./", [System.StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(2)
    }
    while ($normalized.StartsWith("/", [System.StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(1)
    }

    return $normalized
}

function Resolve-InputPath {
    param([string]$InputPath)

    if ([System.IO.Path]::IsPathRooted($InputPath)) {
        return $InputPath
    }

    return Join-Path $repoRoot $InputPath
}

function Test-RequiredText {
    param(
        [string]$Content,
        [string]$RequiredText
    )

    return $Content.IndexOf($RequiredText, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}

function Test-RecordFile {
    param([string]$RecordPath)

    $relativePath = Normalize-RepoPath ($RecordPath.Substring($repoRoot.Path.Length).TrimStart([char[]]@("\", "/")))
    $content = Get-Content -LiteralPath $RecordPath -Raw

    $requiredTexts = @(
        "Timestamp",
        "Branch",
        "Machine specs",
        "Scene/setup",
        "Asset scope",
        "LOD level",
        "Graphics settings",
        "Editor Metrics",
        "Windows EXE Metrics",
        "Average FPS",
        "1% low FPS",
        "RAM",
        "VRAM / texture memory",
        "Draw calls",
        "Batches",
        "Triangles",
        "Loading time",
        "Build size",
        "Bottlenecks",
        "Decision",
        "Notes"
    )

    $missing = New-Object System.Collections.Generic.List[string]
    foreach ($requiredText in $requiredTexts) {
        if (-not (Test-RequiredText -Content $content -RequiredText $requiredText)) {
            $missing.Add($requiredText)
        }
    }

    if ($missing.Count -gt 0) {
        Write-Host "FAIL: $relativePath is missing required fields:"
        foreach ($item in $missing) {
            Write-Host "FAIL: - $item"
        }
        return $false
    }

    Write-Host "PASS: $relativePath contains required benchmark fields."
    return $true
}

Push-Location $repoRoot
try {
    $resolvedPath = Resolve-InputPath -InputPath $Path
    if (-not (Test-Path -LiteralPath $resolvedPath)) {
        throw "Benchmark record path not found: $Path"
    }

    $recordFiles = @()
    if (Test-Path -LiteralPath $resolvedPath -PathType Container) {
        $recordFiles = @(Get-ChildItem -LiteralPath $resolvedPath -Filter "*.md" -File | Sort-Object FullName)
        if ($recordFiles.Count -eq 0) {
            throw "No Markdown benchmark records found under: $Path"
        }
    }
    else {
        $recordFiles = @(Get-Item -LiteralPath $resolvedPath)
    }

    $allPassed = $true
    foreach ($recordFile in $recordFiles) {
        if (-not (Test-RecordFile -RecordPath $recordFile.FullName)) {
            $allPassed = $false
        }
    }

    if ($allPassed) {
        Write-Host "P7 performance log validation result: PASS"
        exit 0
    }

    Write-Host "P7 performance log validation result: FAIL"
    exit 1
}
finally {
    Pop-Location
}
