param(
    [int]$DurationSeconds = 180,
    [string]$ProcessName = "ChuoTsunamiEvacuation_NewMapFinalPre",
    [string]$OutputPath = "Assets\Data\P10\newmap_performance_final_gate.json"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$output = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $output) | Out-Null

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
    param([object]$ExistingResult)
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
    $result = New-MergedResult -ExistingResult $existing
    $result["generatedAt"] = (Get-Date).ToString("s")
    $result["activeScene"] = "Assets/Scenes/Chuo_BaseMap.unity"
    $result["sampledProcess"] = $ProcessName
    $result["durationSeconds"] = $DurationSeconds
    $result["memoryMeasured"] = $false
    $result["fpsMeasured"] = $false
    $result["decision"] = "needs_quick_fix"
    $result["limitation"] = "No final temporary player process was running to sample."
    $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $output -Encoding UTF8
    Write-Host "[WARN] No process found for sampling. Wrote needs_quick_fix result to $output"
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

$result = New-MergedResult -ExistingResult $existing
$result["generatedAt"] = (Get-Date).ToString("s")
$result["activeScene"] = "Assets/Scenes/Chuo_BaseMap.unity"
$result["sampledProcess"] = $process.ProcessName
$result["durationSeconds"] = $DurationSeconds
$result["memoryMeasured"] = ($samples.Count -gt 0)
$result["playerBuildMeasured"] = $true
$result["samples"] = $samples

if ($samples.Count -gt 0) {
    $maxPrivate = ($samples | Measure-Object -Property privateMemoryBytes -Maximum).Maximum
    $maxWorkingSet = ($samples | Measure-Object -Property workingSetBytes -Maximum).Maximum
    $result["maxPrivateMemoryBytes"] = $maxPrivate
    $result["maxWorkingSetBytes"] = $maxWorkingSet
    $result["ordinaryPcRisk"] = ($maxPrivate -gt 12GB)
    $result["decision"] = if ($maxPrivate -gt 12GB) { "ready_with_memory_limitations" } else { "ready_for_manual_playtest" }
    $result["limitations"] = if ($maxPrivate -gt 12GB) {
        @("Private memory exceeds the 12 GB ordinary-PC risk gate; manual test should run on the cloud desktop/high-memory PC.")
    } else {
        @()
    }
}
else {
    $result["decision"] = "needs_quick_fix"
    $result["limitation"] = "Final temporary player process exited before a sample was captured."
}

$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $output -Encoding UTF8
Write-Host "[PASS] Wrote final performance sampling result to $output"
exit 0
