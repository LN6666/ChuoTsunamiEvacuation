param(
    [int]$TimeoutSeconds = 1800
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$VersionLine = Get-Content -LiteralPath (Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt") | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
$Version = ($VersionLine -replace "m_EditorVersion:\s*", "").Trim()
$Unity = "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe"
$LogPath = Join-Path $ProjectRoot "Logs\newmap_latest_hotfix_pre_build.log"
$BuildPath = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapLatestHotfixPre\ChuoTsunamiEvacuation_NewMapLatestHotfixPre.exe"

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogPath) | Out-Null

if (-not (Test-Path -LiteralPath $Unity -PathType Leaf)) {
    Write-Host "[FAIL] Unity executable not found: $Unity"
    exit 1
}

$arguments = @(
    "-batchmode",
    "-quit",
    "-projectPath", $ProjectRoot,
    "-executeMethod", "NewMapSceneSetupUtility.BuildNewMapLatestHotfixPrePlayerCommandLine",
    "-logFile", $LogPath
)

Write-Host "Starting latest NewMap hotfix temporary player build. Log: $LogPath"
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $process.Id -Force
    Write-Host "[FAIL] Unity build timed out after $TimeoutSeconds seconds."
    exit 1
}

$exists = Test-Path -LiteralPath $BuildPath -PathType Leaf
if ($process.ExitCode -eq 0 -and $exists) {
    $playerDataConfigDir = Join-Path (Join-Path (Split-Path -Parent $BuildPath) "ChuoTsunamiEvacuation_NewMapLatestHotfixPre_Data") "Data\P10"
    New-Item -ItemType Directory -Force -Path $playerDataConfigDir | Out-Null

    $configNames = @(
        "newmap_airwall_hard_cleanup_config.json",
        "newmap_player_npc_collision_config.json",
        "newmap_ground_cover_raise_config.json",
        "newmap_npc_lifecycle_config.json",
        "newmap_npc_movement_config.json",
        "newmap_npc_distribution_config.json",
        "newmap_mouse_drag_look_config.json",
        "newmap_lighting_profiles.json",
        "newmap_spawn_config.json",
        "newmap_safe_spawn_points.json",
        "newmap_playable_bounds_config.json",
        "newmap_name_label_config.json",
        "newmap_name_cache.json",
        "newmap_name_normalization_rules.json",
        "newmap_adaptive_support_grid_config.json",
        "newmap_safe_ground_config.json",
        "newmap_gameplay_ground_cover_config.json",
        "newmap_player_stamina_config.json",
        "newmap_tsunami_mode_hotfix_config.json"
    )

    foreach ($configName in $configNames) {
        $source = Join-Path $ProjectRoot ("Assets\Data\P10\" + $configName)
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            Copy-Item -LiteralPath $source -Destination (Join-Path $playerDataConfigDir $configName) -Force
        }
    }
}

if ($process.ExitCode -ne 0 -or -not $exists) {
    Write-Host "[FAIL] Latest NewMap hotfix temporary player build failed. exitCode=$($process.ExitCode) exists=$exists"
    exit 1
}

Write-Host "[PASS] Latest NewMap hotfix temporary player build completed: $BuildPath"
