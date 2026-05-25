[CmdletBinding()]
param(
    [string]$LogPath = "",

    [string]$BuildDirectory = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre",

    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot "Assets\Data\P10\p10c_mm_player_log_summary.json"
}

function Resolve-DefaultLogPath {
    if (-not [string]::IsNullOrWhiteSpace($LogPath)) {
        return $LogPath
    }

    if (-not (Test-Path -LiteralPath $BuildDirectory -PathType Container)) {
        return Join-Path $BuildDirectory "P10CMM_Player.log"
    }

    $candidate = Get-ChildItem -LiteralPath $BuildDirectory -Filter "P10CMM_Player*.log" -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($candidate) {
        return $candidate.FullName
    }

    return Join-Path $BuildDirectory "P10CMM_Player.log"
}

function Add-SampleLine {
    param(
        [System.Collections.Generic.List[string]]$List,
        [string]$Line
    )
    if ($List.Count -ge 20) {
        return
    }
    $trimmed = if ($Line.Length -gt 220) { $Line.Substring(0, 220) } else { $Line }
    $List.Add($trimmed) | Out-Null
}

function Write-Json {
    param([object]$Value, [string]$Path)
    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }
    $Value | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $Path -Encoding UTF8
}

$resolvedLogPath = Resolve-DefaultLogPath
$warningSamples = New-Object System.Collections.Generic.List[string]
$errorSamples = New-Object System.Collections.Generic.List[string]
$exceptionSamples = New-Object System.Collections.Generic.List[string]
$missingAssetSamples = New-Object System.Collections.Generic.List[string]
$shaderMaterialSamples = New-Object System.Collections.Generic.List[string]
$performanceSamples = New-Object System.Collections.Generic.List[string]
$lineCounts = @{}

$summary = [ordered]@{
    schemaVersion = "p10c_mm.player_log_summary.v1"
    status = "log_missing"
    p10cMinusMinusIsExtendedPerformanceGate = $true
    officialP10CReleaseDeferred = $true
    finalReleasePackageCreated = $false
    finalArchiveCreated = $false
    noP10EFG = $true
    logPath = $resolvedLogPath
    parsedAtLocal = (Get-Date).ToString("s")
    lineCount = 0
    warningsCount = 0
    errorsCount = 0
    exceptionsCount = 0
    missingAssetReferenceCount = 0
    shaderMaterialWarningCount = 0
    performanceWarningCount = 0
    repeatedLogSpamCount = 0
    topRepeatedLines = @()
    warningSamples = @()
    errorSamples = @()
    exceptionSamples = @()
    missingAssetSamples = @()
    shaderMaterialSamples = @()
    performanceSamples = @()
    summary = "Player.log was not found."
}

if (Test-Path -LiteralPath $resolvedLogPath -PathType Leaf) {
    $summary.status = "parsed"
    Get-Content -LiteralPath $resolvedLogPath -ErrorAction Stop | ForEach-Object {
        $line = [string]$_
        $summary.lineCount++
        $normalized = $line.Trim()
        if (-not [string]::IsNullOrWhiteSpace($normalized)) {
            if (-not $lineCounts.ContainsKey($normalized)) {
                $lineCounts[$normalized] = 0
            }
            $lineCounts[$normalized]++
        }

        if ($line -match "\bWarning\b" -or $line -match "LogWarning") {
            $summary.warningsCount++
            Add-SampleLine -List $warningSamples -Line $line
        }
        if ($line -match "\bError\b" -or $line -match "LogError" -or $line -match "Exception" -or $line -match "NullReferenceException") {
            $summary.errorsCount++
            Add-SampleLine -List $errorSamples -Line $line
        }
        if ($line -match "Exception" -or $line -match "NullReferenceException" -or $line -match "MissingReferenceException") {
            $summary.exceptionsCount++
            Add-SampleLine -List $exceptionSamples -Line $line
        }
        if ($line -match "missing" -and ($line -match "asset" -or $line -match "reference" -or $line -match "file")) {
            $summary.missingAssetReferenceCount++
            Add-SampleLine -List $missingAssetSamples -Line $line
        }
        if ($line -match "shader" -or $line -match "material" -or $line -match "pink") {
            if ($line -match "warning" -or $line -match "error" -or $line -match "unsupported" -or $line -match "missing") {
                $summary.shaderMaterialWarningCount++
                Add-SampleLine -List $shaderMaterialSamples -Line $line
            }
        }
        if ($line -match "performance" -or $line -match "stall" -or $line -match "timeout" -or $line -match "took .*ms" -or $line -match "allocation") {
            $summary.performanceWarningCount++
            Add-SampleLine -List $performanceSamples -Line $line
        }
    }

    $topRepeated = @(
        $lineCounts.GetEnumerator() |
            Where-Object { $_.Value -ge 5 } |
            Sort-Object Value -Descending |
            Select-Object -First 10 |
            ForEach-Object {
                [ordered]@{
                    count = $_.Value
                    line = if ($_.Key.Length -gt 220) { $_.Key.Substring(0, 220) } else { $_.Key }
                }
            }
    )
    $summary.repeatedLogSpamCount = @($topRepeated).Count
    $summary.topRepeatedLines = @($topRepeated)
    $summary.warningSamples = @($warningSamples)
    $summary.errorSamples = @($errorSamples)
    $summary.exceptionSamples = @($exceptionSamples)
    $summary.missingAssetSamples = @($missingAssetSamples)
    $summary.shaderMaterialSamples = @($shaderMaterialSamples)
    $summary.performanceSamples = @($performanceSamples)
    $summary.summary = "Parsed Player.log warnings=$($summary.warningsCount), errors=$($summary.errorsCount), exceptions=$($summary.exceptionsCount), repeated=$($summary.repeatedLogSpamCount)."
}

Write-Json -Value $summary -Path $OutputPath
Write-Host $summary.summary
Write-Host "P10-C-- Player.log summary: $OutputPath"
exit 0
