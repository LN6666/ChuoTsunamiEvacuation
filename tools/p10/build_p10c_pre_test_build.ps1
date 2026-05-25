[CmdletBinding()]
param(
    [ValidateSet("Batch", "Gui")]
    [string]$LaunchMode = "Batch",

    [string]$OutputDirectory = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre",

    [string]$SceneList = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity",

    [int]$TimeoutSeconds = 1800
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$projectVersionFile = Join-Path $repoRoot "ProjectSettings\ProjectVersion.txt"
$logsDir = Join-Path $repoRoot "logs"
$logPath = Join-Path $logsDir "p10c-pre-test-build-unity.log"
$exePath = Join-Path $OutputDirectory "ChuoTsunamiEvacuation_P10CPre.exe"
$summaryPath = Join-Path $OutputDirectory "p10c_pre_build_summary.json"

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
        & git -C $repoRoot status --short -- ProjectSettings Packages Assets/Scenes/Chuo_BaseMap.unity Assets/Scenes/Chuo_BaseMap.unity.meta Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity Assets/PLATEAU 2>$null
        & git -C $repoRoot status --short -- Assets/Settings Assets/AddressableAssetsData/link.xml Assets/AddressableAssetsData/link.xml.meta Assets/AddressableAssetsData/ProfileDataSourceSettings.asset Assets/AddressableAssetsData/ProfileDataSourceSettings.asset.meta 2>$null
    )
    if ($protectedChanges.Count -gt 0) {
        Write-Host "P10-C-Pre build detected protected path changes:" -ForegroundColor Red
        $protectedChanges | ForEach-Object { Write-Host $_ }
        exit 1
    }
}

function Restore-UnityGeneratedBuildChurn {
    $trackedChurnPaths = @(
        "ProjectSettings/GraphicsSettings.asset",
        "ProjectSettings/ProjectSettings.asset",
        "ProjectSettings/UnityConnectSettings.asset",
        "Assets/Settings/DefaultVolumeProfile.asset",
        "Assets/Settings/PC_RPAsset.asset",
        "Assets/Settings/UniversalRenderPipelineGlobalSettings.asset"
    )
    $trackedChurn = @(
        & git -C $repoRoot status --short -- @trackedChurnPaths 2>$null
    )
    if ($trackedChurn.Count -gt 0) {
        Write-Host "Reverting Unity-generated tracked settings churn from temporary build:" -ForegroundColor Yellow
        $trackedChurn | ForEach-Object { Write-Host $_ }
        & git -C $repoRoot checkout -- @trackedChurnPaths
    }

    $generatedAddressables = @(
        "Assets/AddressableAssetsData/link.xml",
        "Assets/AddressableAssetsData/link.xml.meta",
        "Assets/AddressableAssetsData/ProfileDataSourceSettings.asset",
        "Assets/AddressableAssetsData/ProfileDataSourceSettings.asset.meta"
    )
    foreach ($relativePath in $generatedAddressables) {
        $fullPath = Join-Path $repoRoot ($relativePath -replace "/", "\")
        if (Test-Path -LiteralPath $fullPath -PathType Leaf) {
            $isTracked = (& git -C $repoRoot ls-files -- $relativePath)
            if (-not $isTracked) {
                Write-Host "Removing Unity-generated untracked file: $relativePath" -ForegroundColor Yellow
                Remove-Item -LiteralPath $fullPath -Force
            }
        }
    }
}

Write-Host "P10-C-Pre temporary Windows x64 profiling/test build: starting"
Write-Host "Project: $repoRoot"
Write-Host "Output: $exePath"
Write-Host "Scene list: $SceneList"
Write-Host "This is not the final P10-C release build or release package."

Assert-ProtectedPathsUnchanged

$unity = Find-UnityExecutable
if (-not $unity) {
    Write-Host "Unity executable was not found. Set UNITY_EXE or install the Unity version from ProjectSettings/ProjectVersion.txt." -ForegroundColor Red
    exit 1
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
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
    "-executeMethod", "P10CPreTestBuildBuilder.BuildFromCommandLine",
    "-p10cPreBuildOutputPath", $exePath,
    "-p10cPreBuildSummaryPath", $summaryPath,
    "-p10cPreBuildScenes", $SceneList,
    "-logFile", $logPath
)

$windowStyle = "Hidden"
if ($LaunchMode -eq "Gui") {
    $windowStyle = "Normal"
}

Write-Host "Unity: $unity"
Write-Host "Launch mode: $LaunchMode"
Write-Host "Log: $logPath"
Write-Host "Build summary: $summaryPath"

$process = Start-Process -FilePath $unity -ArgumentList $arguments -WindowStyle $windowStyle -PassThru
$deadline = (Get-Date).AddSeconds([Math]::Max($TimeoutSeconds, 1))
while (-not $process.WaitForExit(2000)) {
    if ((Get-Date) -ge $deadline) {
        Stop-Process -Id $process.Id -Force
        Write-Host "P10-C-Pre temporary build timed out after $TimeoutSeconds seconds." -ForegroundColor Red
        Write-Host "Log: $logPath"
        exit 1
    }
}

Restore-UnityGeneratedBuildChurn
Assert-ProtectedPathsUnchanged

if ($process.ExitCode -ne 0) {
    Write-Host "P10-C-Pre temporary build failed with Unity exit code $($process.ExitCode)." -ForegroundColor Red
    Write-Host "Log: $logPath"
    if (Test-Path -LiteralPath $summaryPath) {
        Write-Host "Build summary: $summaryPath"
    }
    exit $process.ExitCode
}

if (-not (Test-Path -LiteralPath $exePath -PathType Leaf)) {
    Write-Host "Unity reported success but the expected EXE was not found: $exePath" -ForegroundColor Red
    exit 1
}

Write-Host "P10-C-Pre temporary build: PASS" -ForegroundColor Green
Write-Host "Temporary build output: $exePath"
Write-Host "Build summary: $summaryPath"
exit 0
