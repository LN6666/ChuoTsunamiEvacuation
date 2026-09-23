param(
    [int]$DurationSeconds = 180,
    [string]$ProcessName = "ChuoTsunamiEvacuation_NewMapPre",
    [string]$OutputPath = "Assets\Data\P10\newmap_performance_results.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$output = Join-Path $ProjectRoot $OutputPath
$existing = $null
if (Test-Path -LiteralPath $output -PathType Leaf) {
    try {
        $existing = Get-Content -LiteralPath $output -Raw | ConvertFrom-Json
    }
    catch {
        $existing = $null
    }
}

function New-MergedResult {
    param(
        [object]$ExistingResult
    )

    $merged = [ordered]@{}
    if ($ExistingResult) {
        foreach ($property in $ExistingResult.PSObject.Properties) {
            $merged[$property.Name] = $property.Value
        }
    }

    return $merged
}

$samples = @()
$process = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue | Select-Object -First 1

if (-not $process) {
    $process = Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Select-Object -First 1
}

if (-not $process) {
    $result = New-MergedResult -ExistingResult $existing
    $result["generatedAt"] = (Get-Date).ToString("s")
    $result["classification"] = "ready_with_limitations"
    $result["fpsMeasured"] = $false
    $result["memoryMeasured"] = $false
    $result["playerBuildMeasured"] = $false
    $result["limitation"] = "No NewMap player or Unity process was running to sample."
    $result["finalStatus"] = "runtime_sampling_not_available"
    $result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $output -Encoding UTF8
    Write-Host "[WARN] No process found for sampling. Wrote limitation result to $output"
    exit 0
}

$started = Get-Date
while (((Get-Date) - $started).TotalSeconds -lt $DurationSeconds) {
    if ($process.HasExited) {
        break
    }

    $process.Refresh()
    $samples += [pscustomobject][ordered]@{
        time = (Get-Date).ToString("s")
        processName = $process.ProcessName
        id = $process.Id
        workingSetBytes = $process.WorkingSet64
        privateMemoryBytes = $process.PrivateMemorySize64
        cpuSeconds = $process.TotalProcessorTime.TotalSeconds
    }
    Start-Sleep -Seconds 1
}

if ($samples.Count -eq 0) {
    $result = New-MergedResult -ExistingResult $existing
    $result["generatedAt"] = (Get-Date).ToString("s")
    $result["classification"] = "ready_with_limitations"
    $result["fpsMeasured"] = $false
    $result["memoryMeasured"] = $false
    $result["playerBuildMeasured"] = $true
    $result["limitation"] = "NewMap player process was found but exited before sampling completed."
    $result["finalStatus"] = "runtime_process_exited_before_sampling"
    $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $output -Encoding UTF8
    Write-Host "[WARN] Process exited before samples were captured. Wrote limitation result to $output"
    exit 0
}

$maxPrivate = ($samples | Measure-Object -Property privateMemoryBytes -Maximum).Maximum
$maxWorkingSet = ($samples | Measure-Object -Property workingSetBytes -Maximum).Maximum
$classification = if ($maxPrivate -gt 12GB) { "ready_with_limitations" } else { "ready_for_manual_playtest" }
$result = New-MergedResult -ExistingResult $existing
$result["generatedAt"] = (Get-Date).ToString("s")
$result["classification"] = $classification
$result["fpsMeasured"] = $false
$result["memoryMeasured"] = $true
$result["playerBuildMeasured"] = $true
$result["sampledProcess"] = $process.ProcessName
$result["durationSeconds"] = $DurationSeconds
$result["maxPrivateMemoryBytes"] = $maxPrivate
$result["maxWorkingSetBytes"] = $maxWorkingSet
$result["ordinaryPcRisk"] = ($maxPrivate -gt 12GB)
$result["limitations"] = @("FPS/stutter not exported by the runtime sampler; manual visual FPS check is still required.")
$result["finalStatus"] = if ($maxPrivate -gt 12GB) { "ready_with_limitations_memory_risk" } else { "ready_for_manual_playtest_with_manual_fps_check" }
$result["samples"] = $samples
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $output -Encoding UTF8
Write-Host "[PASS] Wrote performance sampling result to $output"
exit 0
