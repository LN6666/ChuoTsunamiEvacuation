[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
. (Join-Path $scriptRoot "get_p7d_scene_evidence.ps1")

$evidence = Get-P7DSceneEvidence -RepoRoot $repoRoot
$failed = $false

Write-Host "P7-D high-detail scene inspection"
Write-Host "Scene: $($evidence.SceneRelativePath)"

if (-not $evidence.SceneExists) {
    Write-Host "FAIL: missing high-detail scene: $($evidence.SceneRelativePath)" -ForegroundColor Red
    exit 1
}

Write-Host "sceneBytes=$($evidence.SceneBytes)"
Write-Host "sceneLastWriteTime=$($evidence.SceneLastWriteTime)"
Write-Host "linesScanned=$($evidence.LinesScanned)"
Write-Host "meshRendererCount=$($evidence.MeshRendererCount)"
Write-Host "meshFilterCount=$($evidence.MeshFilterCount)"
Write-Host "meshColliderCount=$($evidence.MeshColliderCount)"
Write-Host "lodGroupCount=$($evidence.LodGroupCount)"
Write-Host "plateauCityObjectGroupCount=$($evidence.PlateauCityObjectGroupCount)"
Write-Host "averageLod3Achieved=$($evidence.AverageLod3Achieved)"
Write-Host "chuoBaseMapExists=$($evidence.ChuoBaseMapExists)"
Write-Host "chuoBaseMapTracked=$($evidence.ChuoBaseMapTracked)"

Write-Host ""
Write-Host "Unity output roots:"
foreach ($root in $evidence.UnityOutputRoots.Keys) {
    Write-Host ("{0,-34} {1}" -f $root, $evidence.UnityOutputRoots[$root])
}

Write-Host ""
Write-Host "Detected categories:"
Write-P7DCategoryTable -Evidence $evidence

if ($evidence.MeshRendererCount -eq 0 -or $evidence.MeshFilterCount -eq 0 -or $evidence.PlateauCityObjectGroupCount -eq 0) {
    Write-Host "FAIL: high-detail scene has no complete renderable PLATEAU evidence." -ForegroundColor Red
    $failed = $true
}

if ($evidence.AverageLod3Achieved) {
    Write-Host "Average LOD3 claim: supported by detected scene LOD range."
}
else {
    Write-Host "Average LOD3 claim: NOT SUPPORTED. Detected scene LOD includes levels below LOD3." -ForegroundColor Yellow
}

if ($evidence.ForbiddenHits.Count -gt 0) {
    foreach ($hit in $evidence.ForbiddenHits) {
        Write-Host "FAIL: forbidden/protected fragment detected in scene text: $hit"
    }
    $failed = $true
}

if ($failed) {
    Write-Host ""
    Write-Host "P7-D high-detail scene inspection: FAIL" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "P7-D high-detail scene inspection: CONDITIONAL PASS - renderable import exists, but LOD/category coverage is limited" -ForegroundColor Yellow
exit 0
