[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$scopeGuardScript = Join-Path $scriptRoot "check_p7_scope.ps1"
$inspectionScript = Join-Path $scriptRoot "inspect_p7c_benchmark_import.ps1"
$highDetailSceneInspectionScript = Join-Path $scriptRoot "inspect_p7c_high_detail_scene.ps1"
$plateauReadinessInspectionScript = Join-Path $scriptRoot "inspect_p7c_plateau_import_readiness.ps1"
$p2P6CompatibilityInspectionScript = Join-Path $scriptRoot "inspect_p7c_p2_p6_compatibility.ps1"
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

function Invoke-Step {
    param(
        [string]$Name,
        [scriptblock]$ScriptBlock
    )

    Write-Host ""
    Write-Host "P7-C preflight: $Name"
    & $ScriptBlock
    Write-Host "P7-C preflight: $Name PASS"
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
            -not $(Test-PathStartsWith -Path $file -Prefix "Assets/Scenes/P7HighDetail/") -and
            -not $file.Equals("Assets/Scenes/P7Benchmark.meta", [System.StringComparison]::OrdinalIgnoreCase) -and
            -not $file.Equals("Assets/Scenes/P7HighDetail.meta", [System.StringComparison]::OrdinalIgnoreCase)
        ) {
            $violations.Add("$file is a production scene change outside P7Benchmark/P7HighDetail") | Out-Null
        }
        if (
            $(Test-PathStartsWith -Path $file -Prefix "Assets/Scripts/") -and
            -not $(Test-PathStartsWith -Path $file -Prefix "Assets/Scripts/P7Benchmark/") -and
            -not $file.Equals("Assets/Scripts/P7Benchmark.meta", [System.StringComparison]::OrdinalIgnoreCase)
        ) {
            $violations.Add("$file is an existing gameplay script change outside P7Benchmark") | Out-Null
        }
        if (
            $(Test-PathStartsWith -Path $file -Prefix "Assets/Editor/") -and
            -not $(Test-PathStartsWith -Path $file -Prefix "Assets/Editor/P7Benchmark/") -and
            -not $file.Equals("Assets/Editor/P7Benchmark.meta", [System.StringComparison]::OrdinalIgnoreCase)
        ) {
            $violations.Add("$file is an editor script change outside P7Benchmark") | Out-Null
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

function Assert-BenchmarkSceneSandboxed {
    if (-not (Test-Path -LiteralPath $scenePath -PathType Leaf)) {
        throw "Missing benchmark scene: $sceneRelativePath"
    }

    $sceneText = Get-Content -Raw -LiteralPath $scenePath
    $forbiddenFragments = @(
        "Chuo_BaseMap",
        "Assets/PLATEAU",
        "Assets/Data",
        "EvacuationGameManager"
    )

    foreach ($fragment in $forbiddenFragments) {
        if ($sceneText.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw "Benchmark scene contains forbidden fragment: $fragment"
        }
    }

    if ($sceneText.IndexOf("P7BenchmarkRoot", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Benchmark scene does not contain P7BenchmarkRoot."
    }

    if ($sceneText.IndexOf("P7C_ChunkLoadingRoot", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Benchmark scene does not contain P7C_ChunkLoadingRoot."
    }

    Write-Host "Benchmark scene sandbox check passed: $sceneRelativePath"
}

function Assert-FiveStageAndNoP8P9RuntimeScope {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]

    foreach ($file in $changedFiles) {
        $fileName = [System.IO.Path]::GetFileName($file)
        if ($fileName -match "P7[-_ ]?[EFG]" -or $fileName -match "p7[-_ ]?[efg]") {
            $violations.Add("$file appears to create a stage outside P7-0/P7-A/P7-B/P7-C/P7-D") | Out-Null
        }

        $isRuntimeUnityFile = (Test-PathStartsWith -Path $file -Prefix "Assets/Scripts/") -or
            (Test-PathStartsWith -Path $file -Prefix "Assets/Scenes/")
        if (-not $isRuntimeUnityFile) {
            continue
        }

        $fullPath = Join-Path $repoRoot ($file -replace "/", "\")
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            continue
        }

        $text = Get-Content -Raw -LiteralPath $fullPath -ErrorAction SilentlyContinue
        if ([string]::IsNullOrWhiteSpace($text)) {
            continue
        }

        $forbiddenRuntimeFragments = @(
            "LightCurtain",
            "InundationDepth",
            "MovingTsunamiWall",
            "RiskFront",
            "IndoorEvacuation",
            "CrowdFailure",
            "RealSpawn"
        )

        foreach ($fragment in $forbiddenRuntimeFragments) {
            if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
                $violations.Add("$file contains forbidden P8/P9 runtime fragment: $fragment") | Out-Null
            }
        }
    }

    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }

        throw "Five-stage or P8/P9 runtime scope check failed."
    }

    Write-Host "Five-stage and P8/P9 runtime scope check passed for $($changedFiles.Count) changed/untracked files."
}

Write-Host "P7-C preflight: starting"
Write-Host "P7-C preflight: repo root $repoRoot"

$failed = $false
try {
    Invoke-Step "P7-C scope guard" {
        & powershell -ExecutionPolicy Bypass -File $scopeGuardScript -Mode P7C
        if ($LASTEXITCODE -ne 0) {
            throw "P7-C scope guard failed."
        }
    }

    Invoke-Step "benchmark import inspection" {
        & powershell -ExecutionPolicy Bypass -File $inspectionScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7-C import inspection failed."
        }
    }

    Invoke-Step "high-detail scene readiness inspection" {
        & powershell -ExecutionPolicy Bypass -File $highDetailSceneInspectionScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7-C high-detail scene readiness inspection failed."
        }
    }

    Invoke-Step "PLATEAU and LOD coverage readiness inspection" {
        & powershell -ExecutionPolicy Bypass -File $plateauReadinessInspectionScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7-C PLATEAU import readiness inspection failed."
        }
    }

    Invoke-Step "P2-P6 compatibility inspection" {
        & powershell -ExecutionPolicy Bypass -File $p2P6CompatibilityInspectionScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7-C P2-P6 compatibility inspection failed."
        }
    }

    Invoke-Step "five-stage and P8/P9 scope guard" { Assert-FiveStageAndNoP8P9RuntimeScope }

    Invoke-Step "protected path check" { Assert-ProtectedPathsClean }

    Invoke-Step "benchmark scene sandbox check" { Assert-BenchmarkSceneSandboxed }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P7-C preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P7-C preflight: PASS" -ForegroundColor Green
exit 0
