param(
    [int]$TimeoutSeconds = 1800
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$VersionLine = Get-Content -LiteralPath (Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt") | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
$Version = ($VersionLine -replace "m_EditorVersion:\s*", "").Trim()
$Unity = "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe"
$LogPath = Join-Path $ProjectRoot "Logs\newmap_spawn_mouse_fix_build.log"
$BuildPath = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapSpawnMouseFixPre\ChuoTsunamiEvacuation_NewMapSpawnMouseFixPre.exe"
$OutputJson = Join-Path $ProjectRoot "Assets\Data\P10\newmap_spawn_mouse_fix_player_report.json"
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
    "-executeMethod", "NewMapSceneSetupUtility.BuildNewMapSpawnMouseFixPlayerCommandLine",
    "-logFile", $LogPath
)

Write-Host "Starting spawn/mouse-fix temporary NewMap player build. Log: $LogPath"
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $process.Id -Force
    Write-Host "[FAIL] Unity build timed out after $TimeoutSeconds seconds."
    exit 1
}

$exists = Test-Path -LiteralPath $BuildPath -PathType Leaf
$copiedConfigs = @()
if ($process.ExitCode -eq 0 -and $exists) {
    $playerDataConfigDir = Join-Path (Join-Path (Split-Path -Parent $BuildPath) "ChuoTsunamiEvacuation_NewMapSpawnMouseFixPre_Data") "Data\P10"
    New-Item -ItemType Directory -Force -Path $playerDataConfigDir | Out-Null
    $configNames = @(
        "newmap_npc_distribution_config.json",
        "newmap_mouse_drag_look_config.json",
        "newmap_lighting_profiles.json",
        "newmap_spawn_config.json",
        "newmap_safe_spawn_points.json"
    )

    foreach ($configName in $configNames) {
        $source = Join-Path $ProjectRoot ("Assets\Data\P10\" + $configName)
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            Copy-Item -LiteralPath $source -Destination (Join-Path $playerDataConfigDir $configName) -Force
            $copiedConfigs += $configName
        }
    }
}

$result = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    activeScene = "Assets/Scenes/Chuo_BaseMap.unity"
    buildPath = $BuildPath
    buildExists = $exists
    copiedRuntimeConfigs = $copiedConfigs
    unityExitCode = $process.ExitCode
    logPath = $LogPath
    finalStatus = if ($process.ExitCode -eq 0 -and $exists) { "completed_on_new_chuo_basemap" } else { "failed" }
    launchSmoke = "pending"
    playerLogSummary = "pending"
    spawnValidationSmoke = "pending"
    mouseLeftRightDragSmoke = "pending"
    lightingNightRegression = "not_reworked_this_task"
    note = "Temporary spawn/mouse-fix player only. This is not a final release package."
}
$result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $OutputJson -Encoding UTF8

if ($process.ExitCode -ne 0 -or -not $exists) {
    Write-Host "[FAIL] Unity build failed or EXE was not created. JSON: $OutputJson"
    exit 1
}

Write-Host "[PASS] Spawn/mouse-fix temporary NewMap player build completed: $BuildPath"
exit 0
