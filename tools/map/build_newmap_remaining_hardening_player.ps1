param(
    [int]$TimeoutSeconds = 1800
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$VersionLine = Get-Content -LiteralPath (Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt") | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
$Version = ($VersionLine -replace "m_EditorVersion:\s*", "").Trim()
$Unity = "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe"
$LogPath = Join-Path $ProjectRoot "Logs\newmap_remaining_hardening_build.log"
$BuildPath = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapRemainingHardeningPre\ChuoTsunamiEvacuation_NewMapRemainingHardeningPre.exe"
$OutputJson = Join-Path $ProjectRoot "Assets\Data\P10\newmap_remaining_hardening_build_report.json"
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogPath) | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputJson) | Out-Null

if (-not (Test-Path -LiteralPath $Unity -PathType Leaf)) {
    Write-Host "[FAIL] Unity executable not found: $Unity"
    exit 1
}

$arguments = @(
    "-batchmode",
    "-quit",
    "-projectPath", $ProjectRoot,
    "-executeMethod", "NewMapSceneSetupUtility.BuildNewMapRemainingHardeningPlayerCommandLine",
    "-logFile", $LogPath
)

Write-Host "Starting remaining-hardening temporary NewMap player build. Log: $LogPath"
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $process.Id -Force
    Write-Host "[FAIL] Unity build timed out after $TimeoutSeconds seconds. Check for modal dialogs or package/import blockers."
    exit 1
}

$exists = Test-Path -LiteralPath $BuildPath -PathType Leaf
$result = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    activeScene = "Assets/Scenes/Chuo_BaseMap.unity"
    buildPath = $BuildPath
    buildExists = $exists
    unityExitCode = $process.ExitCode
    logPath = $LogPath
    finalStatus = if ($process.ExitCode -eq 0 -and $exists) { "completed_on_new_chuo_basemap" } else { "failed" }
    launchSmoke = "pending"
    performanceSampling = "pending"
    playerLogSummary = "pending"
    note = "Temporary remaining-hardening build only. This is not a final release package."
}
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $OutputJson -Encoding UTF8

if ($process.ExitCode -ne 0 -or -not $exists) {
    Write-Host "[FAIL] Unity build failed or EXE was not created. JSON: $OutputJson"
    exit 1
}

Write-Host "[PASS] Remaining-hardening temporary NewMap player build completed: $BuildPath"
exit 0
