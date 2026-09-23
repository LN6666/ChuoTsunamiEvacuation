param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$audit = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_building_material_texture_audit_round2.json") -Raw | ConvertFrom-Json
$visualFactory = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapVisualFactory.cs") -Raw

$ok =
    [string]$audit.lod2VisualQualityClaim -eq "not_claimed" -and
    -not [bool]$audit.materialAudit.magentaMissingShaderAllowed -and
    -not [bool]$audit.materialAudit.importedPlateauMaterialsMutated -and
    -not [bool]$audit.materialAudit.texturesRemoved -and
    $visualFactory -match "Universal Render Pipeline/Lit" -and
    $visualFactory -match "Standard" -and
    $visualFactory -match "Unlit/Color"

if (-not $ok) {
    Write-Host "[FAIL] Material texture audit round-2 check failed."
    exit 1
}

Write-Host "[PASS] Material texture audit round-2 check passed."
exit 0
