[CmdletBinding()]
param(
    [int]$SceneCreationTimeoutSeconds = 900
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).Path
$checkScript = Join-Path $scriptRoot "check_p7_scope.ps1"
$createSceneScript = Join-Path $scriptRoot "create_p7b_benchmark_scene.ps1"
$sceneRelativePath = "Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity"
$scenePath = Join-Path $repoRoot ($sceneRelativePath -replace "/", "\")

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

function Test-Wave2AAllowedUnityPath {
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

function Invoke-Step {
    param(
        [string]$Name,
        [scriptblock]$ScriptBlock
    )

    Write-Host ""
    Write-Host "P7-B Wave 2-A preflight: $Name"
    & $ScriptBlock
    Write-Host "P7-B Wave 2-A preflight: $Name PASS"
}

function Assert-ProtectedPaths {
    $diffFiles = Get-GitLines @("-C", $repoRoot, "diff", "--name-only", "HEAD", "--")
    $untrackedFiles = Get-GitLines @("-C", $repoRoot, "ls-files", "--others", "--exclude-standard")
    $changedFiles = @(
        @($diffFiles) + @($untrackedFiles) |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
    )

    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-Wave2AAllowedUnityPath -Path $file) {
            continue
        }

        $alwaysForbiddenPrefixes = @(
            "ProjectSettings/",
            "Packages/",
            "Assets/PLATEAU/",
            "Assets/Data/"
        )

        foreach ($prefix in $alwaysForbiddenPrefixes) {
            if (Test-PathStartsWith -Path $file -Prefix $prefix) {
                $violations.Add("$file matches forbidden prefix $prefix")
            }
        }

        if (Test-PathStartsWith -Path $file -Prefix "Assets/Scenes/Chuo_BaseMap.unity") {
            $violations.Add("$file matches forbidden Chuo_BaseMap path")
        }

        $restrictedUnityPrefixes = @(
            "Assets/Scripts/",
            "Assets/Editor/",
            "Assets/Tests/",
            "Assets/Scenes/"
        )

        foreach ($prefix in $restrictedUnityPrefixes) {
            if (Test-PathStartsWith -Path $file -Prefix $prefix) {
                $violations.Add("$file is outside the Wave 2-A P7Benchmark allowlist")
            }
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

function Assert-BenchmarkScene {
    if (-not (Test-Path -LiteralPath $scenePath -PathType Leaf)) {
        Write-Host "Benchmark scene is missing. Attempting Unity command-line creation."
        & powershell -ExecutionPolicy Bypass -File $createSceneScript -TimeoutSeconds $SceneCreationTimeoutSeconds
        if ($LASTEXITCODE -ne 0) {
            throw "Benchmark scene creation failed."
        }
    }

    if (-not (Test-Path -LiteralPath $scenePath -PathType Leaf)) {
        throw "Benchmark scene was not found at $sceneRelativePath."
    }

    $sceneText = Get-Content -Raw -LiteralPath $scenePath
    if ($sceneText -notmatch [regex]::Escape("P7BenchmarkRoot")) {
        throw "Benchmark scene does not contain P7BenchmarkRoot."
    }

    if ($sceneText -match "Chuo_BaseMap" -or $sceneText -match "Assets/PLATEAU" -or $sceneText -match "Assets/Data") {
        throw "Benchmark scene contains a protected path reference."
    }

    Write-Host "Benchmark scene validated: $sceneRelativePath"
}

Write-Host "P7-B Wave 2-A preflight: starting"

$failed = $false
try {
    Invoke-Step "benchmark scene validation" { Assert-BenchmarkScene }

    Invoke-Step "scope guard" {
        & powershell -ExecutionPolicy Bypass -File $checkScript -Mode P7BWave2A
        if ($LASTEXITCODE -ne 0) {
            throw "Wave 2-A scope guard failed."
        }
    }

    Invoke-Step "protected path check" { Assert-ProtectedPaths }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P7-B Wave 2-A preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P7-B Wave 2-A preflight: PASS" -ForegroundColor Green
exit 0
