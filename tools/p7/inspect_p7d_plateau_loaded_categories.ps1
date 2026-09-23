[CmdletBinding()]
param(
    [string]$PlateauDataRoot = "D:\PLATEAU_DATA\Chuo_2025_CityGML"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
. (Join-Path $scriptRoot "get_p7d_scene_evidence.ps1")

$evidence = Get-P7DSceneEvidence -RepoRoot $repoRoot -PlateauDataRoot $PlateauDataRoot
$failed = $false

Write-Host "P7-D PLATEAU loaded category inspection"
Write-Host "Scene: $($evidence.SceneRelativePath)"

if (-not $evidence.SceneExists) {
    Write-Host "FAIL: missing high-detail scene."
    exit 1
}

Write-Host ("{0,-24} {1,-18} {2,-12} {3,-10} {4,8} {5,8} {6,8} {7,8} {8,-32} {9}" -f "category", "target", "actual", "source", "objects", "renderer", "filter", "gml", "status", "flat/attribute evidence")
foreach ($categoryName in $evidence.Categories.Keys) {
    $stats = $evidence.Categories[$categoryName]
    $range = Get-P7DDetectedLodRange -Stats $stats
    $status = Get-P7DCategoryStatus -Stats $stats
    $flatEvidence = Get-P7DFlatOrAttributeEvidence -Stats $stats
    $sourceStatus = if ($stats.SourceFolderExists) { "source" } else { "noSource" }

    Write-Host ("{0,-24} {1,-18} {2,-12} {3,-10} {4,8} {5,8} {6,8} {7,8} {8,-32} {9}" -f $stats.Category, $stats.TargetLod, $range, $sourceStatus, $stats.GameObjectNameHits, $stats.MeshRendererHits, $stats.MeshFilterHits, $stats.GmlRootNameHits, $status, $flatEvidence)

    if ($stats.Category -in @("Buildings", "Roads") -and $stats.CityObjectGroupHits -eq 0) {
        $failed = $true
    }
}

Write-Host ""
Write-Host "Sample imported names:"
foreach ($categoryName in $evidence.Categories.Keys) {
    $stats = $evidence.Categories[$categoryName]
    $samples = if ($stats.SampleNames.Count -gt 0) { $stats.SampleNames -join "; " } else { "none" }
    Write-Host ("{0,-24} {1}" -f $stats.Category, $samples)
}

Write-Host ""
if ($evidence.AverageLod3Achieved) {
    Write-Host "Average LOD3 achieved: TRUE"
}
else {
    Write-Host "Average LOD3 achieved: FALSE. Actual imported scene evidence includes LOD0-LOD2 and no verified LOD3 average." -ForegroundColor Yellow
}

if ($failed) {
    Write-Host "P7-D PLATEAU loaded category inspection: FAIL" -ForegroundColor Red
    exit 1
}

Write-Host "P7-D PLATEAU loaded category inspection: CONDITIONAL PASS - partial renderable import with LOD/category limitations" -ForegroundColor Yellow
exit 0
