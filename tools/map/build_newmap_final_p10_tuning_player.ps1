param(
    [int]$TimeoutSeconds = 1800,
    [int]$SmokeDurationSeconds = 200
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$VersionLine = Get-Content -LiteralPath (Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt") | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
$Version = ($VersionLine -replace "m_EditorVersion:\s*", "").Trim()
$Unity = "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe"
$LogPath = Join-Path $ProjectRoot "Logs\newmap_final_p10_tuning_build.log"
$PlayerLogPath = Join-Path $ProjectRoot "Logs\newmap_final_p10_tuning_player.log"
$BuildPath = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapFinalP10TuningPre\ChuoTsunamiEvacuation_NewMapFinalP10TuningPre.exe"
$OutputJson = Join-Path $ProjectRoot "Assets\Data\P10\newmap_final_p10_tuning_player_report.json"

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogPath) | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputJson) | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $BuildPath) | Out-Null

if (-not (Test-Path -LiteralPath $Unity -PathType Leaf)) {
    Write-Host "[FAIL] Unity executable not found: $Unity"
    exit 1
}

$arguments = @(
    "-batchmode",
    "-quit",
    "-projectPath", $ProjectRoot,
    "-executeMethod", "NewMapSceneSetupUtility.BuildNewMapFinalP10TuningPlayerCommandLine",
    "-logFile", $LogPath
)

Write-Host "Starting NewMap final P10 tuning temporary player build. Log: $LogPath"
$buildProcess = Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (-not $buildProcess.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $buildProcess.Id -Force
    Write-Host "[FAIL] Unity build timed out after $TimeoutSeconds seconds."
    exit 1
}

$exists = Test-Path -LiteralPath $BuildPath -PathType Leaf
$copiedConfigs = @()
if ($buildProcess.ExitCode -eq 0 -and $exists) {
    $playerDataConfigDir = Join-Path (Join-Path (Split-Path -Parent $BuildPath) "ChuoTsunamiEvacuation_NewMapFinalP10TuningPre_Data") "Data\P10"
    New-Item -ItemType Directory -Force -Path $playerDataConfigDir | Out-Null
    $configNames = @(
        "newmap_airwall_hard_cleanup_config.json",
        "newmap_circular_boundary_config.json",
        "newmap_leaderboard_toggle_config.json",
        "newmap_player_npc_collision_config.json",
        "newmap_player_stamina_config.json",
        "newmap_tsunami_mode_hotfix_config.json",
        "newmap_ground_cover_raise_config.json",
        "newmap_final_ground_micro_raise_config.json",
        "newmap_ground_raise_30_config.json",
        "newmap_npc_movement_config.json",
        "newmap_npc_lifecycle_config.json",
        "newmap_npc_distribution_config.json",
        "newmap_npc_boundary_wide_distribution_config.json",
        "newmap_npc_double_count_config.json",
        "newmap_mouse_drag_look_config.json",
        "newmap_lighting_profiles.json",
        "newmap_spawn_config.json",
        "newmap_safe_spawn_points.json",
        "newmap_playable_bounds_config.json",
        "newmap_name_label_config.json",
        "newmap_name_cache.json",
        "newmap_name_normalization_rules.json",
        "newmap_non_official_name_enrichment_config.json",
        "newmap_adaptive_support_grid_config.json",
        "newmap_safe_ground_config.json",
        "newmap_gameplay_ground_cover_config.json"
    )

    foreach ($configName in $configNames) {
        $source = Join-Path $ProjectRoot ("Assets\Data\P10\" + $configName)
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            Copy-Item -LiteralPath $source -Destination (Join-Path $playerDataConfigDir $configName) -Force
            $copiedConfigs += $configName
        }
    }
}

$launchSmoke = "pending"
$memorySamples = @()
$maxPrivate = 0
$maxWorkingSet = 0
$smokeStartedAt = $null

if ($buildProcess.ExitCode -eq 0 -and $exists) {
    if (Test-Path -LiteralPath $PlayerLogPath -PathType Leaf) {
        Remove-Item -LiteralPath $PlayerLogPath -Force
    }

    Write-Host "Launching NewMap final P10 tuning smoke for $SmokeDurationSeconds seconds. Player log: $PlayerLogPath"
    $smokeStartedAt = Get-Date
    $playerArguments = @("-newmapSelfAuditSmoke", "-logFile", $PlayerLogPath)
    $playerProcess = Start-Process -FilePath $BuildPath -ArgumentList $playerArguments -PassThru -WindowStyle Hidden
    Start-Sleep -Seconds 8
    $playerProcess.Refresh()

    $started = Get-Date
    while (((Get-Date) - $started).TotalSeconds -lt $SmokeDurationSeconds) {
        if ($playerProcess.HasExited) {
            break
        }

        $playerProcess.Refresh()
        $memorySamples += [pscustomobject][ordered]@{
            time = (Get-Date).ToString("s")
            processName = $playerProcess.ProcessName
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

$result = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    activeScene = "Assets/Scenes/Chuo_BaseMap.unity"
    buildPath = $BuildPath
    buildExists = $exists
    copiedRuntimeConfigs = $copiedConfigs
    unityExitCode = $buildProcess.ExitCode
    buildLogPath = $LogPath
    playerLogPath = $PlayerLogPath
    launchSmoke = $launchSmoke
    commandLineSmokeArg = "-newmapSelfAuditSmoke"
    smokeStartedAt = if ($smokeStartedAt) { $smokeStartedAt.ToString("s") } else { $null }
    smokeDurationSeconds = $SmokeDurationSeconds
    memoryMeasured = ($memorySamples.Count -gt 0)
    maxPrivateMemoryBytes = $maxPrivate
    maxWorkingSetBytes = $maxWorkingSet
    note = "Temporary NewMap final P10 stamina/sprint tuning player only. This is not a final release package."
    playerLogSummary = "pending"
    finalStatus = if ($buildProcess.ExitCode -eq 0 -and $exists -and $memorySamples.Count -gt 0) { "built_and_smoke_launched" } elseif ($buildProcess.ExitCode -eq 0 -and $exists) { "built_smoke_failed" } else { "failed" }
}
$result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $OutputJson -Encoding UTF8

if ($buildProcess.ExitCode -ne 0 -or -not $exists) {
    Write-Host "[FAIL] Unity build failed or EXE was not created. JSON: $OutputJson"
    exit 1
}

if ($memorySamples.Count -eq 0) {
    Write-Host "[FAIL] Temporary player build completed, but launch smoke did not produce process samples. JSON: $OutputJson"
    exit 1
}

Write-Host "[PASS] NewMap final P10 tuning temporary player build and launch smoke completed: $BuildPath"
exit 0
