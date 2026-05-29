param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$scenePath = Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$auditPath = Join-Path $root "Assets\Data\P10\newmap_concave_mesh_trigger_audit.json"
$fixPath = Join-Path $root "Assets\Data\P10\newmap_concave_mesh_trigger_fix.json"

foreach ($path in @($scenePath, $bootstrapPath, $auditPath, $fixPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing concave trigger validation input: $path"
        exit 1
    }
}

$meshText = (& rg -c "^MeshCollider:" $scenePath 2>$null)
$meshCount = if ($LASTEXITCODE -eq 0 -and $meshText) { [int]$meshText } else { 0 }
$triggerText = (& rg -c "m_IsTrigger: 1" $scenePath 2>$null)
$triggerCount = if ($LASTEXITCODE -eq 0 -and $triggerText) { [int]$triggerText } else { 0 }
$offenders = 0
if ($triggerCount -gt 0) {
    $scene = Get-Content -Encoding UTF8 -LiteralPath $scenePath -Raw
    $blocks = [regex]::Matches($scene, "(?ms)^MeshCollider:.*?(?=^---|^MeshCollider:|\z)")
    foreach ($block in $blocks) {
        if ($block.Value -match "m_IsTrigger:\s*1" -and $block.Value -match "m_Convex:\s*0") {
            $offenders++
        }
    }
}

$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$audit = Get-Content -Encoding UTF8 -LiteralPath $auditPath -Raw | ConvertFrom-Json
$fix = Get-Content -Encoding UTF8 -LiteralPath $fixPath -Raw | ConvertFrom-Json

$codeSafe =
    $bootstrap -match "NeutralizeExistingConcaveMeshTrigger" -and
    $bootstrap -match "ConvertOrReplaceBlockingColliderAsTrigger" -and
    $bootstrap -match "CreatePrimitiveTriggerProxy" -and
    $bootstrap -match "meshCollider != null && !meshCollider.convex"

$passed =
    $offenders -eq 0 -and
    [int]$audit.sceneConcaveMeshTriggerOffenders -eq 0 -and
    [bool]$fix.runtimeFixImplemented -and
    $codeSafe

if (-not $passed) {
    Write-Host "[FAIL] Concave MeshCollider trigger validation failed. sceneMesh=$meshCount offenders=$offenders codeSafe=$codeSafe"
    exit 1
}

Write-Host "[PASS] No scene concave MeshCollider triggers and runtime cleanup excludes concave MeshCollider triggers."
exit 0
