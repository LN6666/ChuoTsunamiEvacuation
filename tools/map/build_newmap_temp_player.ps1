param(
    [int]$TimeoutSeconds = 1800
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$VersionLine = Get-Content -LiteralPath (Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt") | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
$Version = ($VersionLine -replace "m_EditorVersion:\s*", "").Trim()
$Unity = "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe"
$LogPath = Join-Path $ProjectRoot "Logs\newmap_temp_build.log"
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogPath) | Out-Null

if (-not (Test-Path -LiteralPath $Unity -PathType Leaf)) {
    Write-Host "[FAIL] Unity executable not found: $Unity"
    exit 1
}

$arguments = @(
    "-batchmode",
    "-quit",
    "-projectPath", $ProjectRoot,
    "-executeMethod", "NewMapSceneSetupUtility.BuildNewMapTempPlayerCommandLine",
    "-logFile", $LogPath
)

Write-Host "Starting temporary NewMap player build. Log: $LogPath"
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $process.Id -Force
    Write-Host "[FAIL] Unity build timed out after $TimeoutSeconds seconds. Check for modal dialogs or package/import blockers."
    exit 1
}

if ($process.ExitCode -ne 0) {
    Write-Host "[FAIL] Unity build exited with code $($process.ExitCode). Log: $LogPath"
    exit $process.ExitCode
}

Write-Host "[PASS] Temporary NewMap player build completed."
exit 0
