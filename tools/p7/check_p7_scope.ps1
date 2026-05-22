[CmdletBinding()]
param(
    [int64]$LargeFileThresholdBytes = 5242880
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

function Test-PathStartsWith {
    param(
        [string]$Path,
        [string]$Prefix
    )

    return $Path.StartsWith($Prefix, [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-AllowedLargePath {
    param([string]$Path)

    $allowedPrefixes = @(
        "docs/",
        "codex_prompts/",
        "logs/",
        "review_reports/",
        "run_logs/",
        "test-results/"
    )

    foreach ($prefix in $allowedPrefixes) {
        if (Test-PathStartsWith -Path $Path -Prefix $prefix) {
            return $true
        }
    }

    return $false
}

function Test-TextFilePath {
    param([string]$Path)

    $fileName = [System.IO.Path]::GetFileName($Path)
    if ($fileName -eq ".gitignore") {
        return $true
    }

    $extension = [System.IO.Path]::GetExtension($Path).ToLowerInvariant()
    $textExtensions = @(
        ".asmdef",
        ".cs",
        ".csv",
        ".json",
        ".md",
        ".ps1",
        ".rsp",
        ".txt",
        ".xml",
        ".yaml",
        ".yml"
    )

    return $textExtensions -contains $extension
}

Push-Location $repoRoot
try {
    $gitRoot = (& git rev-parse --show-toplevel).Trim()
    if ([string]::IsNullOrWhiteSpace($gitRoot)) {
        throw "Unable to resolve git repository root."
    }

    $protectedPrefixes = @(
        "ProjectSettings/",
        "Packages/",
        "Assets/Scenes/",
        "Assets/PLATEAU/",
        "Assets/Scripts/",
        "Assets/Data/"
    )

    $forbiddenKeywords = @(
        "crowd failure",
        "road block gameplay",
        "building collapse",
        "tsunami height",
        "inundation depth",
        "light curtain",
        "real spawn",
        "indoor evacuation",
        "P6-F",
        "P7-E",
        "P7-F",
        "P7-G"
    )

    $diffFiles = Get-GitLines @("diff", "--name-only", "HEAD", "--")
    $untrackedFiles = Get-GitLines @("ls-files", "--others", "--exclude-standard")
    $allChangedFiles = @($diffFiles) + @($untrackedFiles)
    $changedFiles = @(
        $allChangedFiles |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
    )

    Write-Host "P7 scope guard: checking $($changedFiles.Count) changed/untracked files against HEAD."

    $protectedViolations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        foreach ($prefix in $protectedPrefixes) {
            if (Test-PathStartsWith -Path $file -Prefix $prefix) {
                $protectedViolations.Add("$file matches protected prefix $prefix")
            }
        }
    }

    $addedDiffFiles = Get-GitLines @("diff", "--name-only", "--diff-filter=A", "HEAD", "--")
    $allAddedFiles = @($addedDiffFiles) + @($untrackedFiles)
    $addedFiles = @(
        $allAddedFiles |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
    )

    $largeFileViolations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $addedFiles) {
        $fullPath = Join-Path $repoRoot ($file -replace "/", "\")
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            continue
        }

        $length = (Get-Item -LiteralPath $fullPath).Length
        if ($length -gt $LargeFileThresholdBytes -and -not (Test-AllowedLargePath -Path $file)) {
            $sizeMb = [Math]::Round($length / 1MB, 2)
            $thresholdMb = [Math]::Round($LargeFileThresholdBytes / 1MB, 2)
            $largeFileViolations.Add("$file is $sizeMb MB, above $thresholdMb MB")
        }
    }

    $keywordWarnings = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (-not (Test-TextFilePath -Path $file)) {
            continue
        }

        $fullPath = Join-Path $repoRoot ($file -replace "/", "\")
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            continue
        }

        $content = Get-Content -LiteralPath $fullPath -Raw -ErrorAction SilentlyContinue
        if ($null -eq $content) {
            continue
        }

        foreach ($keyword in $forbiddenKeywords) {
            if ($content.IndexOf($keyword, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
                $keywordWarnings.Add("$file contains keyword '$keyword'")
            }
        }
    }

    if ($keywordWarnings.Count -gt 0) {
        Write-Host ""
        Write-Host "P7 scope guard warnings:"
        foreach ($warning in $keywordWarnings) {
            Write-Host "WARN: $warning"
        }
    }

    $hasFailure = $false

    if ($protectedViolations.Count -gt 0) {
        $hasFailure = $true
        Write-Host ""
        Write-Host "P7 scope guard protected-path failures:"
        foreach ($violation in $protectedViolations) {
            Write-Host "FAIL: $violation"
        }
    }

    if ($largeFileViolations.Count -gt 0) {
        $hasFailure = $true
        Write-Host ""
        Write-Host "P7 scope guard large-file failures:"
        foreach ($violation in $largeFileViolations) {
            Write-Host "FAIL: $violation"
        }
    }

    Write-Host ""
    if ($hasFailure) {
        Write-Host "P7 scope guard result: FAIL"
        exit 1
    }

    Write-Host "P7 scope guard result: PASS"
    exit 0
}
finally {
    Pop-Location
}
