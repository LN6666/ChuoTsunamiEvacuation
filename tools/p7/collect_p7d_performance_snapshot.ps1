[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$scenePath = Join-Path $repoRoot "Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity"

Write-Host "P7-D performance snapshot"

if (-not (Test-Path -LiteralPath $scenePath -PathType Leaf)) {
    Write-Host "FAIL: P7-D scene missing."
    exit 1
}

$sceneText = Get-Content -Raw -LiteralPath $scenePath
$meshRendererCount = ([regex]::Matches($sceneText, "MeshRenderer:", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
$meshFilterCount = ([regex]::Matches($sceneText, "MeshFilter:", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
$pendingMarker = $sceneText.IndexOf("ActualLoadedStatus_NotImported_RenderableEvidencePending", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
$sceneBytes = (Get-Item -LiteralPath $scenePath).Length

Write-Host "scene=Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
Write-Host "sceneBytes=$sceneBytes"
Write-Host "meshRendererCount=$meshRendererCount"
Write-Host "meshFilterCount=$meshFilterCount"
Write-Host "pendingImportMarker=$pendingMarker"
Write-Host "averageFps=not_collected"
Write-Host "onePercentLowFps=not_collected"
Write-Host "windowsExeProfiling=blocked_pending_manual_plateau_import"

if ($meshRendererCount -eq 0 -and $meshFilterCount -eq 0) {
    Write-Host "P7-D performance snapshot: BLOCKED_NO_RENDERABLE_HIGH_DETAIL_ASSETS" -ForegroundColor Yellow
    exit 0
}

Write-Host "P7-D performance snapshot: READY_FOR_EXE_PROFILING_AFTER_BUILD" -ForegroundColor Green
exit 0
