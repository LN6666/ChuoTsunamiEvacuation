[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$scopeGuardScript = Join-Path $scriptRoot "check_p7_scope.ps1"
$importRootRelative = "Assets/P7Benchmark/Imported/53393690"
$importRoot = Join-Path $repoRoot ($importRootRelative -replace "/", "\")

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

function Invoke-Step {
    param(
        [string]$Name,
        [scriptblock]$ScriptBlock
    )

    Write-Host ""
    Write-Host "P7-B Wave 2-C preflight: $Name"
    & $ScriptBlock
    Write-Host "P7-B Wave 2-C preflight: $Name PASS"
}

function Get-ChangedFiles {
    $diffFiles = Get-GitLines @("-C", $repoRoot, "diff", "--name-only", "HEAD", "--")
    $untrackedFiles = Get-GitLines @("-C", $repoRoot, "ls-files", "--others", "--exclude-standard")

    return @(
        @($diffFiles) + @($untrackedFiles) |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
    )
}

function Assert-ProtectedPathsClean {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]

    foreach ($file in $changedFiles) {
        if (Test-PathStartsWith -Path $file -Prefix "ProjectSettings/") {
            $violations.Add("$file matches forbidden ProjectSettings prefix") | Out-Null
        }
        if (Test-PathStartsWith -Path $file -Prefix "Packages/") {
            $violations.Add("$file matches forbidden Packages prefix") | Out-Null
        }
        if (Test-PathStartsWith -Path $file -Prefix "Assets/PLATEAU/") {
            $violations.Add("$file matches forbidden Assets/PLATEAU prefix") | Out-Null
        }
        if (Test-PathStartsWith -Path $file -Prefix "Assets/Data/") {
            $violations.Add("$file matches forbidden Assets/Data prefix") | Out-Null
        }
        if (Test-PathStartsWith -Path $file -Prefix "Assets/Scenes/Chuo_BaseMap.unity") {
            $violations.Add("$file matches forbidden Chuo_BaseMap path") | Out-Null
        }
        if (
            $(Test-PathStartsWith -Path $file -Prefix "Assets/Scenes/") -and
            -not $(Test-PathStartsWith -Path $file -Prefix "Assets/Scenes/P7Benchmark/") -and
            -not $file.Equals("Assets/Scenes/P7Benchmark.meta", [System.StringComparison]::OrdinalIgnoreCase)
        ) {
            $violations.Add("$file is a production scene change outside P7Benchmark") | Out-Null
        }
        if (
            $(Test-PathStartsWith -Path $file -Prefix "Assets/Scripts/") -and
            -not $(Test-PathStartsWith -Path $file -Prefix "Assets/Scripts/P7Benchmark/") -and
            -not $file.Equals("Assets/Scripts/P7Benchmark.meta", [System.StringComparison]::OrdinalIgnoreCase)
        ) {
            $violations.Add("$file is an existing gameplay script change outside P7Benchmark") | Out-Null
        }
        if (
            $(Test-PathStartsWith -Path $file -Prefix "Assets/P7Benchmark/Imported/") -and
            -not $(Test-PathStartsWith -Path $file -Prefix "$importRootRelative/") -and
            -not $file.Equals("Assets/P7Benchmark/Imported.meta", [System.StringComparison]::OrdinalIgnoreCase) -and
            -not $file.Equals("Assets/P7Benchmark/Imported/53393690.meta", [System.StringComparison]::OrdinalIgnoreCase)
        ) {
            $violations.Add("$file is outside the approved 53393690 import path") | Out-Null
        }
    }

    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }

        throw "Protected path check failed."
    }

    Write-Host "Protected path check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-ImportedCandidate {
    if (-not (Test-Path -LiteralPath $importRoot -PathType Container)) {
        throw "Import root is missing: $importRootRelative"
    }

    $importedFiles = @(Get-ChildItem -LiteralPath $importRoot -Recurse -File -Force -ErrorAction SilentlyContinue | Where-Object {
        $_.Name -notlike "*.meta"
    })

    if ($importedFiles.Count -ne 5843) {
        throw "Expected 5843 imported non-meta files for candidate 53393690, found $($importedFiles.Count)."
    }

    $bytes = [int64]0
    foreach ($file in $importedFiles) {
        $bytes += [int64]$file.Length
        if (
            $file.FullName.IndexOf("53393672", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $file.FullName.IndexOf("53394611", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
        ) {
            throw "Fallback candidate path was found inside the 53393690 sandbox import: $($file.FullName)"
        }
    }

    if ($bytes -ne 634782243) {
        throw "Expected 634782243 imported bytes for candidate 53393690, found $bytes."
    }

    $requiredFiles = @(
        "udx\bldg\53393690_bldg_6697_op.gml",
        "udx\brid\53393690_brid_6697_op.gml",
        "udx\tran\53393690_tran_6697_op.gml"
    )

    foreach ($requiredFile in $requiredFiles) {
        $requiredPath = Join-Path $importRoot $requiredFile
        if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
            throw "Required imported file is missing: $requiredFile"
        }
    }

    Write-Host "Imported candidate check passed: 5843 files, 634782243 bytes."
}

function Assert-LfsTracking {
    $samplePath = "$importRootRelative/udx/bldg/53393690_bldg_6697_op.gml"
    $attrLines = @(Get-GitLines @("-C", $repoRoot, "check-attr", "filter", "--", $samplePath))
    foreach ($line in $attrLines) {
        if ($line -match "filter:\s*lfs") {
            Write-Host "Git LFS tracking check passed for $samplePath"
            return
        }
    }

    throw "Git LFS tracking is not active for $samplePath. Large imported files would not be push-safe."
}

Write-Host "P7-B Wave 2-C preflight: starting"
Write-Host "P7-B Wave 2-C preflight: repo root $repoRoot"

$failed = $false
try {
    Invoke-Step "P7 scope guard Wave 2-C mode" {
        & powershell -ExecutionPolicy Bypass -File $scopeGuardScript -Mode P7BWave2C
        if ($LASTEXITCODE -ne 0) {
            throw "P7 Wave 2-C scope guard failed."
        }
    }

    Invoke-Step "protected path check" { Assert-ProtectedPathsClean }

    Invoke-Step "imported candidate check" { Assert-ImportedCandidate }

    Invoke-Step "Git LFS tracking check" { Assert-LfsTracking }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P7-B Wave 2-C preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P7-B Wave 2-C preflight: PASS" -ForegroundColor Green
exit 0
