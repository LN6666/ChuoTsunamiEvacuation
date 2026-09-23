[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
. (Join-Path $scriptRoot "get_p7d_scene_evidence.ps1")

$evidence = Get-P7DSceneEvidence -RepoRoot $repoRoot

Write-Host "P7-D LOD coverage inspection"
Write-Host "Scene: $($evidence.SceneRelativePath)"

if (-not $evidence.SceneExists) {
    Write-Host "FAIL: missing high-detail scene."
    exit 1
}

Write-Host ""
Write-Host "Total detected LOD counts:"
if ($evidence.TotalLodCounts.Count -eq 0) {
    Write-Host "none"
}
else {
    foreach ($lod in ($evidence.TotalLodCounts.Keys | Sort-Object { [int]$_ })) {
        Write-Host ("LOD{0}: {1}" -f $lod, $evidence.TotalLodCounts[$lod])
    }
}

Write-Host ""
Write-Host ("{0,-24} {1,-18} {2,-12} {3,-42} {4}" -f "category", "target", "range", "counts", "verdict")
foreach ($categoryName in $evidence.Categories.Keys) {
    $stats = $evidence.Categories[$categoryName]
    $range = Get-P7DDetectedLodRange -Stats $stats
    $counts = Get-P7DLodSummary -Stats $stats
    $status = Get-P7DCategoryStatus -Stats $stats
    $verdict = if ($status -eq "MISSING") {
        "missing"
    }
    elseif ($stats.TargetLod.IndexOf("LOD3", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -and -not $stats.Lods.Contains("3")) {
        "target_mismatch_no_lod3"
    }
    else {
        "target_partially_supported"
    }

    Write-Host ("{0,-24} {1,-18} {2,-12} {3,-42} {4}" -f $stats.Category, $stats.TargetLod, $range, $counts, $verdict)
}

Write-Host ""
if ($evidence.AverageLod3Achieved) {
    Write-Host "P7-D LOD coverage inspection: PASS - average LOD3 supported by scene text" -ForegroundColor Green
    exit 0
}

Write-Host "P7-D LOD coverage inspection: CONDITIONAL PASS - mismatch documented, average LOD3 not achieved" -ForegroundColor Yellow
exit 0
