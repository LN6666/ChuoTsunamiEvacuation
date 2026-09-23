param(
    [int]$TimeoutSeconds = 1800
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$VersionLine = Get-Content -LiteralPath (Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt") | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
$Version = ($VersionLine -replace "m_EditorVersion:\s*", "").Trim()
$Unity = "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe"
$LogPath = Join-Path $ProjectRoot "Logs\newmap_visual_fix_build.log"
$BuildPath = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapVisualFixPre\ChuoTsunamiEvacuation_NewMapVisualFixPre.exe"
$OutputJson = Join-Path $ProjectRoot "Assets\Data\P10\newmap_visual_fix_player_report.json"
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
    "-executeMethod", "NewMapSceneSetupUtility.BuildNewMapVisualFixPlayerCommandLine",
    "-logFile", $LogPath
)

Write-Host "Starting visual-fix temporary NewMap player build. Log: $LogPath"
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $process.Id -Force
    Write-Host "[FAIL] Unity build timed out after $TimeoutSeconds seconds."
    exit 1
}

$exists = Test-Path -LiteralPath $BuildPath -PathType Leaf
$configCopied = $false
if ($process.ExitCode -eq 0 -and $exists) {
    $sourceNpcConfig = Join-Path $ProjectRoot "Assets\Data\P10\newmap_npc_distribution_config.json"
    $playerDataConfigDir = Join-Path (Join-Path (Split-Path -Parent $BuildPath) "ChuoTsunamiEvacuation_NewMapVisualFixPre_Data") "Data\P10"
    if (Test-Path -LiteralPath $sourceNpcConfig -PathType Leaf) {
        New-Item -ItemType Directory -Force -Path $playerDataConfigDir | Out-Null
        Copy-Item -LiteralPath $sourceNpcConfig -Destination (Join-Path $playerDataConfigDir "newmap_npc_distribution_config.json") -Force
        $configCopied = $true
    }
}
$result = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    activeScene = "Assets/Scenes/Chuo_BaseMap.unity"
    buildPath = $BuildPath
    buildExists = $exists
    npcConfigCopiedToPlayerData = $configCopied
    unityExitCode = $process.ExitCode
    logPath = $LogPath
    finalStatus = if ($process.ExitCode -eq 0 -and $exists) { "completed_on_new_chuo_basemap" } else { "failed" }
    launchSmoke = "pending"
    playerLogSummary = "pending"
    mouseLookTest = "pending_manual_confirmation"
    fpsStutterSample = "pending"
    clearDayVisualSanity = "pending"
    nightModeVisualSanity = "pending"
    note = "Temporary visual-fix build only. This is not a final release package."
}
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $OutputJson -Encoding UTF8

if ($process.ExitCode -ne 0 -or -not $exists) {
    Write-Host "[FAIL] Unity build failed or EXE was not created. JSON: $OutputJson"
    exit 1
}

Write-Host "[PASS] Visual-fix temporary NewMap player build completed: $BuildPath"
exit 0
