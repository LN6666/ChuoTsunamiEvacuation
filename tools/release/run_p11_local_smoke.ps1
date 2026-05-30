param(
    [int]$SmokeDurationSeconds = 200
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$ReleaseFolder = "D:\UnityProjects\ChuoTsunamiEvacuation-Releases\ChuoTsunamiEvacuation_v1.0"
$ExePath = Join-Path $ReleaseFolder "ChuoTsunamiEvacuation.exe"
$LogPath = Join-Path $ProjectRoot "Logs\p11_final_player.log"

if (-not (Test-Path -LiteralPath $ExePath -PathType Leaf)) {
    Write-Host "[FAIL] Final EXE missing: $ExePath"
    exit 1
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogPath) | Out-Null
if (Test-Path -LiteralPath $LogPath -PathType Leaf) {
    Remove-Item -LiteralPath $LogPath -Force
}

Write-Host "Launching P11 local smoke for $SmokeDurationSeconds seconds: $ExePath"
$process = Start-Process -FilePath $ExePath -ArgumentList @("-newmapSelfAuditSmoke", "-logFile", $LogPath) -PassThru -WindowStyle Hidden
$samples = @()
$started = Get-Date
Start-Sleep -Seconds 8
while (((Get-Date) - $started).TotalSeconds -lt $SmokeDurationSeconds) {
    if ($process.HasExited) { break }
    $process.Refresh()
    $samples += [pscustomobject][ordered]@{
        time = (Get-Date).ToString("s")
        id = $process.Id
        workingSetBytes = $process.WorkingSet64
        privateMemoryBytes = $process.PrivateMemorySize64
        cpuSeconds = $process.TotalProcessorTime.TotalSeconds
    }
    Start-Sleep -Seconds 1
}

if (-not $process.HasExited) {
    $process.CloseMainWindow() | Out-Null
    Start-Sleep -Seconds 5
    $process.Refresh()
    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}

if ($samples.Count -eq 0) {
    Write-Host "[FAIL] P11 local smoke produced no process samples."
    exit 1
}

& (Join-Path $PSScriptRoot "parse_p11_player_log.ps1") -LogPath $LogPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$summaryPath = Join-Path $ProjectRoot "Assets\Data\P10\p11_local_final_smoke_report.json"
$summary = Get-Content -LiteralPath $summaryPath -Raw -Encoding UTF8 | ConvertFrom-Json
$summary | Add-Member -NotePropertyName exePath -NotePropertyValue $ExePath -Force
$summary | Add-Member -NotePropertyName releaseFolder -NotePropertyValue $ReleaseFolder -Force
$summary | Add-Member -NotePropertyName smokeDurationSecondsRequested -NotePropertyValue $SmokeDurationSeconds -Force
$summary | Add-Member -NotePropertyName memorySampleCount -NotePropertyValue $samples.Count -Force
$summary | Add-Member -NotePropertyName maxPrivateMemoryBytes -NotePropertyValue (($samples | Measure-Object -Property privateMemoryBytes -Maximum).Maximum) -Force
$summary | Add-Member -NotePropertyName maxWorkingSetBytes -NotePropertyValue (($samples | Measure-Object -Property workingSetBytes -Maximum).Maximum) -Force
$summary | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $summaryPath -Encoding UTF8

Write-Host "[PASS] P11 local smoke passed."
exit 0
