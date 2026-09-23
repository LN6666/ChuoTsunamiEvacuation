param(
    [int]$TimeoutSeconds = 1800,
    [int]$SmokeDurationSeconds = 210
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$VersionLine = Get-Content -LiteralPath (Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt") | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
$Version = ($VersionLine -replace "m_EditorVersion:\s*", "").Trim()
$Unity = "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe"
$LogPath = Join-Path $ProjectRoot "Logs\p10_newmap_tsunami_mode_hotfix_build.log"
$BuildPath = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10NewMapTsunamiModeHotfixPre\ChuoTsunamiEvacuation_P10NewMapTsunamiModeHotfixPre.exe"
$OutputJson = Join-Path $ProjectRoot "Assets\Data\P10\p10_newmap_tsunami_mode_hotfix_player_report.json"

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
    "-executeMethod", "NewMapSceneSetupUtility.BuildP10NewMapTsunamiModeHotfixPlayerCommandLine",
    "-logFile", $LogPath
)

Write-Host "Starting P10 NewMap tsunami-mode hotfix build. Log: $LogPath"
$buildProcess = Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (-not $buildProcess.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $buildProcess.Id -Force
    Write-Host "[FAIL] Unity build timed out after $TimeoutSeconds seconds."
    exit 1
}

$exists = Test-Path -LiteralPath $BuildPath -PathType Leaf
$copiedConfigs = @()
if ($buildProcess.ExitCode -eq 0 -and $exists) {
    $playerDataConfigDir = Join-Path (Join-Path (Split-Path -Parent $BuildPath) "ChuoTsunamiEvacuation_P10NewMapTsunamiModeHotfixPre_Data") "Data\P10"
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
            $copiedConfigs += $configName
        }
    }
}

$launchSmoke = "not_run"
$memorySamples = @()
$maxPrivate = 0
$maxWorkingSet = 0
$smokeStartedAt = $null

if ($buildProcess.ExitCode -eq 0 -and $exists) {
    Write-Host "Launching P10 NewMap tsunami-mode hotfix smoke with -newmapSelfAuditSmoke for $SmokeDurationSeconds seconds."
    $smokeStartedAt = Get-Date
    $playerProcess = Start-Process -FilePath $BuildPath -ArgumentList "-newmapSelfAuditSmoke" -PassThru -WindowStyle Hidden
    Start-Sleep -Seconds 8
    $started = Get-Date
    while (((Get-Date) - $started).TotalSeconds -lt $SmokeDurationSeconds) {
        if ($playerProcess.HasExited) {
            break
        }

        $playerProcess.Refresh()
        $memorySamples += [pscustomobject][ordered]@{
            time = (Get-Date).ToString("s")
            id = $playerProcess.Id
            workingSetBytes = $playerProcess.WorkingSet64
            privateMemoryBytes = $playerProcess.PrivateMemorySize64
            cpuSeconds = $playerProcess.TotalProcessorTime.TotalSeconds
        }
        Start-Sleep -Seconds 1
    }

    if ($memorySamples.Count -gt 0) {
        $maxPrivate = ($memorySamples | Measure-Object -Property privateMemoryBytes -Maximum).Maximum
        $maxWorkingSet = ($memorySamples | Measure-Object -Property workingSetBytes -Maximum).Maximum
        $launchSmoke = if ($playerProcess.HasExited) { "process_exited_after_samples" } else { "completed_smoke_window" }
    }
    else {
        $launchSmoke = "failed_no_process_samples"
    }

    if (-not $playerProcess.HasExited) {
        $playerProcess.CloseMainWindow() | Out-Null
        Start-Sleep -Seconds 5
        $playerProcess.Refresh()
        if (-not $playerProcess.HasExited) {
            Stop-Process -Id $playerProcess.Id -Force
        }
    }
}

$report = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    task = "P10 NewMap tsunami mode hotfix"
    buildPath = $BuildPath
    buildExists = $exists
    unityExitCode = $buildProcess.ExitCode
    buildLogPath = $LogPath
    launchSmoke = $launchSmoke
    smokeStartedAt = if ($smokeStartedAt) { $smokeStartedAt.ToString("s") } else { $null }
    smokeDurationSeconds = $SmokeDurationSeconds
    copiedConfigs = $copiedConfigs
    maxPrivateMemoryBytes = [int64]$maxPrivate
    maxWorkingSetBytes = [int64]$maxWorkingSet
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputJson -Encoding UTF8

if ($buildProcess.ExitCode -ne 0 -or -not $exists) {
    Write-Host "[FAIL] P10 NewMap tsunami-mode hotfix build failed."
    exit 1
}

if ($launchSmoke -like "failed*") {
    Write-Host "[FAIL] P10 NewMap tsunami-mode hotfix smoke failed: $launchSmoke"
    exit 1
}

Write-Host "[PASS] P10 NewMap tsunami-mode hotfix build and smoke complete: $BuildPath"
