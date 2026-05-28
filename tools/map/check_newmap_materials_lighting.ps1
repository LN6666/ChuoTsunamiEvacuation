param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$lightingCode = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapLightingController.cs") -Raw
$factoryCode = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapVisualFactory.cs") -Raw
$lighting = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_lighting_visual_status.json") -Raw | ConvertFrom-Json
$materials = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_material_visual_quality_status.json") -Raw | ConvertFrom-Json

$checks = @(
    ($lightingCode -match "NewMap_ClearDay_DirectionalLight"),
    ($lightingCode -match "AmbientMode\.Trilight"),
    ($lightingCode -match "NightRain"),
    ($factoryCode -match "Universal Render Pipeline/Lit"),
    ($factoryCode -match "Standard"),
    ([bool]$lighting.clearDayLightingConfigExists),
    ([bool]$lighting.rainNightReversible),
    ([bool]$materials.runtimeGeneratedMaterialShaderValid),
    (-not [bool]$materials.magentaMissingShaderAllowed),
    ([string]$materials.lod2VisualQualityClaim -eq "not_claimed")
)

if ($checks -contains $false) {
    Write-Host "[FAIL] Materials/lighting check failed."
    exit 1
}

Write-Host "[PASS] Materials/lighting check passed."
exit 0
