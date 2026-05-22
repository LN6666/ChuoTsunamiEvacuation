[CmdletBinding()]
param(
    [ValidateSet("Default", "P7BWave2A", "P7BWave2C")]
    [string]$Mode = "Default",

    [int64]$LargeFileThresholdBytes = 5242880,
    [string]$WarningSummaryPath = ""
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
    param(
        [string]$Path,
        [string]$Mode
    )

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

    if (
        $Mode -eq "P7BWave2C" -and
        $(Test-PathStartsWith -Path $Path -Prefix "Assets/P7Benchmark/Imported/53393690/")
    ) {
        return $true
    }

    return $false
}

function Test-P7BWave2AAllowedUnityPath {
    param([string]$Path)

    $allowedPrefixes = @(
        "Assets/Scripts/P7Benchmark/",
        "Assets/Editor/P7Benchmark/",
        "Assets/Tests/EditMode/P7Benchmark/",
        "Assets/Tests/PlayMode/P7Benchmark/",
        "Assets/Scenes/P7Benchmark/"
    )

    $allowedExactPaths = @(
        "Assets/Scripts/P7Benchmark.meta",
        "Assets/Editor/P7Benchmark.meta",
        "Assets/Tests/EditMode/P7Benchmark.meta",
        "Assets/Tests/PlayMode/P7Benchmark.meta",
        "Assets/Scenes/P7Benchmark.meta"
    )

    foreach ($allowedExactPath in $allowedExactPaths) {
        if ($Path.Equals($allowedExactPath, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    foreach ($prefix in $allowedPrefixes) {
        if (Test-PathStartsWith -Path $Path -Prefix $prefix) {
            return $true
        }
    }

    return $false
}

function Get-P7BWave2AProtectedViolation {
    param([string]$Path)

    if (Test-P7BWave2AAllowedUnityPath -Path $Path) {
        return $null
    }

    $alwaysForbiddenPrefixes = @(
        "ProjectSettings/",
        "Packages/",
        "Assets/PLATEAU/",
        "Assets/Data/"
    )

    foreach ($prefix in $alwaysForbiddenPrefixes) {
        if (Test-PathStartsWith -Path $Path -Prefix $prefix) {
            return "$Path matches forbidden Wave 2-A prefix $prefix"
        }
    }

    if (Test-PathStartsWith -Path $Path -Prefix "Assets/Scenes/Chuo_BaseMap.unity") {
        return "$Path matches forbidden Wave 2-A base-map scene path"
    }

    $wave2AUnityPrefixes = @(
        "Assets/Scripts/",
        "Assets/Editor/",
        "Assets/Tests/EditMode/",
        "Assets/Tests/PlayMode/",
        "Assets/Scenes/"
    )

    foreach ($prefix in $wave2AUnityPrefixes) {
        if (Test-PathStartsWith -Path $Path -Prefix $prefix) {
            return "$Path is outside the P7-B Wave 2-A allowlist for $prefix"
        }
    }

    return $null
}

function Test-P7BWave2CAllowedPath {
    param([string]$Path)

    $allowedPrefixes = @(
        "Assets/P7Benchmark/Imported/53393690/",
        "Assets/Scenes/P7Benchmark/",
        "Assets/Scripts/P7Benchmark/",
        "Assets/Editor/P7Benchmark/",
        "Assets/Tests/EditMode/P7Benchmark/",
        "Assets/Tests/PlayMode/P7Benchmark/",
        "docs/P7B_WAVE2C_",
        "tools/p7/"
    )

    $allowedExactPaths = @(
        ".gitattributes",
        "Assets/P7Benchmark.meta",
        "Assets/P7Benchmark/Imported.meta",
        "Assets/P7Benchmark/Imported/53393690.meta",
        "Assets/Scenes/P7Benchmark.meta",
        "Assets/Scripts/P7Benchmark.meta",
        "Assets/Editor/P7Benchmark.meta",
        "Assets/Tests/EditMode/P7Benchmark.meta",
        "Assets/Tests/PlayMode/P7Benchmark.meta",
        "tools/p7/import_p7b_53393690_sandbox.ps1",
        "tools/p7/run_p7b_wave2c_preflight.ps1",
        "tools/p7/check_p7_scope.ps1",
        "codex_prompts/p7b_wave2c_53393690_sandbox_import.md",
        "deepseek_review_prompt_p7b_wave2c.md",
        "docs/TASKS.md",
        "docs/REVIEW_BACKLOG.md",
        "docs/P7_DECISION_LOG.md"
    )

    foreach ($allowedExactPath in $allowedExactPaths) {
        if ($Path.Equals($allowedExactPath, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    foreach ($prefix in $allowedPrefixes) {
        if (Test-PathStartsWith -Path $Path -Prefix $prefix) {
            if ($prefix -eq "tools/p7/") {
                $fileName = [System.IO.Path]::GetFileName($Path)
                return $fileName.IndexOf("wave2c", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
                    $fileName.Equals("check_p7_scope.ps1", [System.StringComparison]::OrdinalIgnoreCase)
            }

            return $true
        }
    }

    return $false
}

function Get-P7BWave2CProtectedViolation {
    param([string]$Path)

    if (Test-P7BWave2CAllowedPath -Path $Path) {
        return $null
    }

    $alwaysForbiddenPrefixes = @(
        "ProjectSettings/",
        "Packages/",
        "Assets/PLATEAU/",
        "Assets/Data/"
    )

    foreach ($prefix in $alwaysForbiddenPrefixes) {
        if (Test-PathStartsWith -Path $Path -Prefix $prefix) {
            return "$Path matches forbidden Wave 2-C prefix $prefix"
        }
    }

    if (Test-PathStartsWith -Path $Path -Prefix "Assets/Scenes/Chuo_BaseMap.unity") {
        return "$Path matches forbidden Wave 2-C base-map scene path"
    }

    $restrictedUnityPrefixes = @(
        "Assets/Scenes/",
        "Assets/Scripts/",
        "Assets/Editor/",
        "Assets/Tests/"
    )

    foreach ($prefix in $restrictedUnityPrefixes) {
        if (Test-PathStartsWith -Path $Path -Prefix $prefix) {
            return "$Path is outside the P7-B Wave 2-C allowlist for $prefix"
        }
    }

    if (
        $(Test-PathStartsWith -Path $Path -Prefix "Assets/P7Benchmark/Imported/") -and
        -not $(Test-PathStartsWith -Path $Path -Prefix "Assets/P7Benchmark/Imported/53393690/") -and
        -not $Path.Equals("Assets/P7Benchmark/Imported.meta", [System.StringComparison]::OrdinalIgnoreCase) -and
        -not $Path.Equals("Assets/P7Benchmark/Imported/53393690.meta", [System.StringComparison]::OrdinalIgnoreCase)
    ) {
        return "$Path is outside the approved 53393690 sandbox import path"
    }

    if (Test-PathStartsWith -Path $Path -Prefix "Assets/") {
        return "$Path is outside the P7-B Wave 2-C Unity allowlist"
    }

    return "$Path is outside the P7-B Wave 2-C file allowlist"
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

function Test-ExpectedKeywordPath {
    param([string]$Path)

    $normalized = Normalize-RepoPath $Path
    $lowerPath = $normalized.ToLowerInvariant()
    $fileName = [System.IO.Path]::GetFileName($lowerPath)

    if (Test-PathStartsWith -Path $lowerPath -Prefix "codex_prompts/") {
        return $true
    }

    if ($fileName.StartsWith("deepseek_review_prompt", [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    $expectedBoundaryFiles = @(
        "docs/p7_boundaries.md",
        "docs/p7_stage_plan.md",
        "docs/p7_decision_log.md",
        "docs/review_backlog.md"
    )

    if ($expectedBoundaryFiles -contains $lowerPath) {
        return $true
    }

    if (Test-PathStartsWith -Path $lowerPath -Prefix "docs/") {
        if ($fileName.Contains("boundary") -or
            $fileName.Contains("boundaries") -or
            $fileName.Contains("prompt") -or
            $fileName.Contains("review")) {
            return $true
        }
    }

    return $false
}

function Test-ExpectedKeywordLine {
    param(
        [string]$Path,
        [string]$Line
    )

    if (Test-ExpectedKeywordPath -Path $Path) {
        return $true
    }

    $lowerLine = $Line.ToLowerInvariant()
    $expectedLineFragments = @(
        "do not create",
        "must not create",
        "must not implement",
        "must not add",
        "reserved for",
        "scope guard",
        "warns on",
        "warning",
        "no dependency import",
        "not allowed",
        "did not create",
        "exactly five stages",
        "has exactly five stages",
        "only five stages"
    )

    foreach ($fragment in $expectedLineFragments) {
        if ($lowerLine.Contains($fragment)) {
            return $true
        }
    }

    return $false
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

    Write-Host "P7 scope guard: mode $Mode"
    Write-Host "P7 scope guard: checking $($changedFiles.Count) changed/untracked files against HEAD."

    $protectedViolations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if ($Mode -eq "P7BWave2A") {
            $wave2AViolation = Get-P7BWave2AProtectedViolation -Path $file
            if (-not [string]::IsNullOrWhiteSpace($wave2AViolation)) {
                $protectedViolations.Add($wave2AViolation)
            }
        }
        elseif ($Mode -eq "P7BWave2C") {
            $wave2CViolation = Get-P7BWave2CProtectedViolation -Path $file
            if (-not [string]::IsNullOrWhiteSpace($wave2CViolation)) {
                $protectedViolations.Add($wave2CViolation)
            }
        }
        else {
            foreach ($prefix in $protectedPrefixes) {
                if (Test-PathStartsWith -Path $file -Prefix $prefix) {
                    $protectedViolations.Add("$file matches protected prefix $prefix")
                }
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
        if ($length -gt $LargeFileThresholdBytes -and -not (Test-AllowedLargePath -Path $file -Mode $Mode)) {
            $sizeMb = [Math]::Round($length / 1MB, 2)
            $thresholdMb = [Math]::Round($LargeFileThresholdBytes / 1MB, 2)
            $largeFileViolations.Add("$file is $sizeMb MB, above $thresholdMb MB")
        }
    }

    $keywordWarnings = New-Object System.Collections.Generic.List[string]
    $expectedKeywordWarnings = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-PathStartsWith -Path $file -Prefix "docs/p7_status/") {
            continue
        }

        if (-not (Test-TextFilePath -Path $file)) {
            continue
        }

        $fullPath = Join-Path $repoRoot ($file -replace "/", "\")
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            continue
        }

        $contentLines = @(Get-Content -LiteralPath $fullPath -ErrorAction SilentlyContinue)
        if ($null -eq $contentLines) {
            continue
        }

        $insideForbiddenKeywordArray = $false
        for ($lineIndex = 0; $lineIndex -lt $contentLines.Count; $lineIndex++) {
            $line = [string]$contentLines[$lineIndex]
            if ($file -eq "tools/p7/check_p7_scope.ps1" -and $line -match '^\s*\$forbiddenKeywords\s*=\s*@\(') {
                $insideForbiddenKeywordArray = $true
            }

            foreach ($keyword in $forbiddenKeywords) {
                if ($line.IndexOf($keyword, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
                    $warning = "$file line $($lineIndex + 1) contains keyword '$keyword'"
                    if ($insideForbiddenKeywordArray -or (Test-ExpectedKeywordLine -Path $file -Line $line)) {
                        $expectedKeywordWarnings.Add($warning)
                    }
                    else {
                        $keywordWarnings.Add($warning)
                    }
                }
            }

            if ($insideForbiddenKeywordArray -and $line -match '^\s*\)\s*$') {
                $insideForbiddenKeywordArray = $false
            }
        }
    }

    $warningSummaryLines = New-Object System.Collections.Generic.List[string]
    foreach ($warning in $keywordWarnings) {
        $warningSummaryLines.Add("WARN: $warning")
    }
    foreach ($warning in $expectedKeywordWarnings) {
        $warningSummaryLines.Add("INFO: expected-context keyword: $warning")
    }

    if ($keywordWarnings.Count -gt 0) {
        Write-Host ""
        Write-Host "P7 scope guard warnings:"
        foreach ($warning in $keywordWarnings) {
            Write-Host "WARN: $warning"
        }
    }

    if ($expectedKeywordWarnings.Count -gt 0) {
        Write-Host ""
        Write-Host "P7 scope guard expected-context keyword notices:"
        foreach ($warning in $expectedKeywordWarnings) {
            Write-Host "INFO: expected-context keyword: $warning"
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($WarningSummaryPath)) {
        $summaryFullPath = $WarningSummaryPath
        if (-not [System.IO.Path]::IsPathRooted($summaryFullPath)) {
            $summaryFullPath = Join-Path $repoRoot $summaryFullPath
        }

        $summaryDir = Split-Path -Parent $summaryFullPath
        if (-not [string]::IsNullOrWhiteSpace($summaryDir)) {
            New-Item -ItemType Directory -Force -Path $summaryDir | Out-Null
        }

        if ($warningSummaryLines.Count -gt 0) {
            Set-Content -LiteralPath $summaryFullPath -Value $warningSummaryLines -Encoding UTF8
        }
        else {
            Set-Content -LiteralPath $summaryFullPath -Value "No P7 scope guard keyword warnings." -Encoding UTF8
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
