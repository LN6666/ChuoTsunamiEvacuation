[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

$scopeGuardScript = Join-Path $scriptRoot "check_p7_scope.ps1"
$sceneScript = Join-Path $scriptRoot "inspect_p7d_high_detail_scene.ps1"
$categoryScript = Join-Path $scriptRoot "inspect_p7d_plateau_loaded_categories.ps1"
$compatibilityScript = Join-Path $scriptRoot "inspect_p7d_p2_p6_compatibility.ps1"
$snapshotScript = Join-Path $scriptRoot "collect_p7d_performance_snapshot.ps1"
$perfValidateScript = Join-Path $scriptRoot "validate_p7d_performance_record.ps1"

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

function Invoke-Step {
    param(
        [string]$Name,
        [scriptblock]$ScriptBlock
    )

    Write-Host ""
    Write-Host "P7-D preflight: $Name"
    & $ScriptBlock
    Write-Host "P7-D preflight: $Name PASS"
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
    }

    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }

        throw "Protected path check failed."
    }

    Write-Host "Protected path check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-FinalClaimsBlocked {
    $closeoutPath = Join-Path $repoRoot "docs\P7_FINAL_CLOSEOUT.md"
    $baselinePath = Join-Path $repoRoot "docs\P7D_NEW_MAP_BASELINE_DECISION.md"

    if (-not (Test-Path -LiteralPath $closeoutPath -PathType Leaf)) {
        throw "Missing docs/P7_FINAL_CLOSEOUT.md"
    }
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw "Missing docs/P7D_NEW_MAP_BASELINE_DECISION.md"
    }

    $closeout = Get-Content -Raw -LiteralPath $closeoutPath
    $baseline = Get-Content -Raw -LiteralPath $baselinePath

    if ($closeout.IndexOf("P7 final status: BLOCKED", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "P7 final closeout must remain BLOCKED while manual import/profiling are pending."
    }
    if ($baseline.IndexOf("Decision: BLOCKED", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "P7-D baseline decision must be BLOCKED until actual import/profiling pass."
    }

    Write-Host "Final-claim guard passed: P7-D is blocked, not falsely closed."
}

Write-Host "P7-D preflight: starting"
Write-Host "P7-D preflight: repo root $repoRoot"

$failed = $false
try {
    Invoke-Step "P7-D scope guard" {
        & powershell -ExecutionPolicy Bypass -File $scopeGuardScript -Mode P7D
        if ($LASTEXITCODE -ne 0) {
            throw "P7-D scope guard failed."
        }
    }

    Invoke-Step "protected path check" { Assert-ProtectedPathsClean }

    Invoke-Step "high-detail scene validator" {
        & powershell -ExecutionPolicy Bypass -File $sceneScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7-D high-detail scene validator failed."
        }
    }

    Invoke-Step "PLATEAU loaded category validator" {
        & powershell -ExecutionPolicy Bypass -File $categoryScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7-D PLATEAU loaded category validator failed."
        }
    }

    Invoke-Step "P2-P6 compatibility validator" {
        & powershell -ExecutionPolicy Bypass -File $compatibilityScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7-D P2-P6 compatibility validator failed."
        }
    }

    Invoke-Step "performance snapshot" {
        & powershell -ExecutionPolicy Bypass -File $snapshotScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7-D performance snapshot failed."
        }
    }

    Invoke-Step "performance record validator" {
        & powershell -ExecutionPolicy Bypass -File $perfValidateScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7-D performance record validator failed."
        }
    }

    Invoke-Step "final closeout claim guard" { Assert-FinalClaimsBlocked }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P7-D preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P7-D preflight: CONDITIONAL PASS - BLOCKED ON MANUAL PLATEAU IMPORT" -ForegroundColor Yellow
exit 0
