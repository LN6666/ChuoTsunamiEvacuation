param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$profiles = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_lighting_profiles.json") -Raw | ConvertFrom-Json
$status = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_night_lighting_correction.json") -Raw | ConvertFrom-Json
$lightingCode = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapLightingController.cs") -Raw

$daySky = ([double]$profiles.clear_day.skyR + [double]$profiles.clear_day.skyG + [double]$profiles.clear_day.skyB) / 3.0
$nightSky = ([double]$profiles.night_clear.skyR + [double]$profiles.night_clear.skyG + [double]$profiles.night_clear.skyB) / 3.0
$ok =
    $daySky -gt 0.6 -and
    $nightSky -lt 0.12 -and
    [double]$profiles.night_clear.fillLightIntensity -gt 0.1 -and
    [double]$profiles.night_clear.ambientIntensity -ge 0.8 -and
    [bool]$status.dayLightingPreserved -and
    $lightingCode -match "NewMap_NightReadable_FillLight" -and
    $lightingCode -match "CameraClearFlags\.SolidColor"

if (-not $ok) {
    Write-Host "[FAIL] Night lighting round-2 check failed."
    exit 1
}

Write-Host "[PASS] Night lighting round-2 check passed."
exit 0
