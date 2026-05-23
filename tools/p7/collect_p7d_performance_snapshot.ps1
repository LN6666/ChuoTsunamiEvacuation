[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
. (Join-Path $scriptRoot "get_p7d_scene_evidence.ps1")

$evidence = Get-P7DSceneEvidence -RepoRoot $repoRoot

Write-Host "P7-D performance snapshot"

if (-not $evidence.SceneExists) {
    Write-Host "FAIL: P7-D scene missing."
    exit 1
}

Write-Host "scene=$($evidence.SceneRelativePath)"
Write-Host "sceneBytes=$($evidence.SceneBytes)"
Write-Host "meshRendererCount=$($evidence.MeshRendererCount)"
Write-Host "meshFilterCount=$($evidence.MeshFilterCount)"
Write-Host "meshColliderCount=$($evidence.MeshColliderCount)"
Write-Host "lodGroupCount=$($evidence.LodGroupCount)"
Write-Host "plateauCityObjectGroupCount=$($evidence.PlateauCityObjectGroupCount)"
Write-Host "averageLod3Achieved=$($evidence.AverageLod3Achieved)"
Write-Host "averageFps=not_collected"
Write-Host "onePercentLowFps=not_collected"
Write-Host "windowsExeProfiling=prepared_not_run"

if ($evidence.MeshRendererCount -eq 0 -and $evidence.MeshFilterCount -eq 0) {
    Write-Host "P7-D performance snapshot: BLOCKED_NO_RENDERABLE_HIGH_DETAIL_ASSETS" -ForegroundColor Yellow
    exit 0
}

Write-Host "P7-D performance snapshot: RENDERABLE_SCENE_READY_FOR_MANUAL_EXE_PROFILING" -ForegroundColor Yellow
exit 0
