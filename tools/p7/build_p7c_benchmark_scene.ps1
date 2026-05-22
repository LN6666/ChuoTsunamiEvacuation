[CmdletBinding()]
param(
    [ValidateSet("Batch", "Gui")]
    [string]$LaunchMode = "Batch",

    [int]$TimeoutSeconds = 900
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$projectVersionFile = Join-Path $repoRoot "ProjectSettings\ProjectVersion.txt"
$logsDir = Join-Path $repoRoot "logs"
$logPath = Join-Path $logsDir "p7c-build-benchmark-scene-unity.log"

function Get-UnityEditorVersion {
    if (-not (Test-Path -LiteralPath $projectVersionFile)) {
        return $null
    }

    $versionLine = Get-Content -LiteralPath $projectVersionFile | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
    if (-not $versionLine) {
        return $null
    }

    return ($versionLine -replace "m_EditorVersion:\s*", "").Trim()
}

function Find-UnityExecutable {
    if ($env:UNITY_EXE -and (Test-Path -LiteralPath $env:UNITY_EXE)) {
        return (Resolve-Path -LiteralPath $env:UNITY_EXE).Path
    }

    $editorVersion = Get-UnityEditorVersion
    $candidatePaths = @()

    if ($editorVersion) {
        $candidatePaths += "C:\Program Files\Unity\Hub\Editor\$editorVersion\Editor\Unity.exe"
        $candidatePaths += "C:\Program Files (x86)\Unity\Hub\Editor\$editorVersion\Editor\Unity.exe"
    }

    $hubRoot = "C:\Program Files\Unity\Hub\Editor"
    if (Test-Path -LiteralPath $hubRoot) {
        $candidatePaths += Get-ChildItem -LiteralPath $hubRoot -Directory |
            Sort-Object Name -Descending |
            ForEach-Object { Join-Path $_.FullName "Editor\Unity.exe" }
    }

    foreach ($candidatePath in $candidatePaths) {
        if ($candidatePath -and (Test-Path -LiteralPath $candidatePath)) {
            return (Resolve-Path -LiteralPath $candidatePath).Path
        }
    }

    return $null
}

function Assert-ProtectedPathsUnchanged {
    $protectedChanges = @(
        & git -C $repoRoot status --short -- ProjectSettings Packages Assets/Scenes/Chuo_BaseMap.unity Assets/Scenes/Chuo_BaseMap.unity.meta Assets/PLATEAU Assets/Data 2>$null
    )

    if ($protectedChanges.Count -gt 0) {
        Write-Host "P7-C scene build detected protected path changes:" -ForegroundColor Red
        $protectedChanges | ForEach-Object { Write-Host $_ }
        exit 1
    }
}

Write-Host "P7-C benchmark scene build: starting"
Write-Host "Project: $repoRoot"

$unity = Find-UnityExecutable
if (-not $unity) {
    Write-Host "Unity executable was not found. P7-C scene build requires Unity Editor invocation." -ForegroundColor Red
    Write-Host "Set UNITY_EXE to the full Unity.exe path or install the Unity version from ProjectSettings/ProjectVersion.txt."
    exit 1
}

New-Item -ItemType Directory -Force -Path $logsDir | Out-Null
if (Test-Path -LiteralPath $logPath) {
    Remove-Item -LiteralPath $logPath -Force
}

$arguments = @()
if ($LaunchMode -eq "Batch") {
    $arguments += "-batchmode"
}

$arguments += @(
    "-quit",
    "-projectPath", $repoRoot,
    "-executeMethod", "P7CChunkRegistryBuilder.BuildFromCommandLine",
    "-p7cBuildSceneQuit",
    "-logFile", $logPath
)

Write-Host "Unity: $unity"
Write-Host "Launch mode: $LaunchMode"
Write-Host "Log: $logPath"

$windowStyle = "Hidden"
if ($LaunchMode -eq "Gui") {
    $windowStyle = "Normal"
}

$process = Start-Process -FilePath $unity -ArgumentList $arguments -WindowStyle $windowStyle -PassThru
$deadline = (Get-Date).AddSeconds([Math]::Max($TimeoutSeconds, 1))

while (-not $process.WaitForExit(2000)) {
    if ((Get-Date) -ge $deadline) {
        Stop-Process -Id $process.Id -Force
        Write-Host "P7-C benchmark scene build timed out after $TimeoutSeconds seconds." -ForegroundColor Red
        Write-Host "Log: $logPath"
        exit 1
    }
}

if ($process.ExitCode -ne 0) {
    Write-Host "P7-C benchmark scene build failed with Unity exit code $($process.ExitCode)." -ForegroundColor Red
    Write-Host "Log: $logPath"
    exit $process.ExitCode
}

Assert-ProtectedPathsUnchanged

Write-Host "P7-C benchmark scene build: PASS"
exit 0
