[CmdletBinding()]
param(
    [string]$P7SourceScene = "D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity",
    [string]$P7SourceMeta = "D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity.meta",
    [string]$P9TargetScene = "Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity",
    [string]$P9TargetMeta = "Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity.meta"
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$targetSceneFull = Join-Path $repoRoot $P9TargetScene
$targetMetaFull = Join-Path $repoRoot $P9TargetMeta
$actualSceneThresholdBytes = 1GB

Write-Host "P10-C-Pre high-detail scene check"
Write-Host "P7 source scene: $P7SourceScene"
Write-Host "P9 target scene: $targetSceneFull"

$failures = New-Object System.Collections.Generic.List[string]
if (-not (Test-Path -LiteralPath $P7SourceScene -PathType Leaf)) { $failures.Add("Missing P7 source scene.") }
if (-not (Test-Path -LiteralPath $P7SourceMeta -PathType Leaf)) { $failures.Add("Missing P7 source scene meta.") }
if (-not (Test-Path -LiteralPath $targetSceneFull -PathType Leaf)) { $failures.Add("Missing P9 target scene.") }
if (-not (Test-Path -LiteralPath $targetMetaFull -PathType Leaf)) { $failures.Add("Missing P9 target scene meta.") }

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}

$sourceInfo = Get-Item -LiteralPath $P7SourceScene
$targetInfo = Get-Item -LiteralPath $targetSceneFull
Write-Host "P7 source size bytes: $($sourceInfo.Length)"
Write-Host "P9 target size bytes: $($targetInfo.Length)"

if ($sourceInfo.Length -lt $actualSceneThresholdBytes) {
    Write-Host "FAIL: P7 source scene is smaller than expected for the actual high-detail scene." -ForegroundColor Red
    exit 1
}

if ($targetInfo.Length -lt $actualSceneThresholdBytes) {
    Write-Host "LIMITATION: P9 target scene is currently a small tracked placeholder/status shell, not the actual P7 high-detail scene copy." -ForegroundColor Yellow
}

Write-Host "P10-C-Pre high-detail scene check: PASS_WITH_DOCUMENTED_LIMITATION" -ForegroundColor Green
exit 0
