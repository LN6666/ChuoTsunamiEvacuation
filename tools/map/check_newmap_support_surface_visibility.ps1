param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$source = Get-Content -LiteralPath $bootstrapPath -Raw

if ($source -notmatch 'new GameObject\("NewMap_RuntimeGroundSupport_DocumentedProxy"\)' -or
    $source -notmatch 'AddComponent<BoxCollider>\(\)' -or
    $source -notmatch 'EnforceSupportSurfaceVisibility' -or
    $source -notmatch 'renderer\.enabled = false') {
    Write-Host "[FAIL] Runtime support surface must be collider-only and enforce hidden renderers."
    exit 1
}

if ($source -match 'GameObject\.CreatePrimitive\(PrimitiveType\.Cube\).*NewMap_RuntimeGroundSupport_DocumentedProxy') {
    Write-Host "[FAIL] Runtime support proxy must not be created as a rendered primitive."
    exit 1
}

$statusPath = Join-Path $root "Assets\Data\P10\newmap_support_surface_visibility_status.json"
$status = Get-Content -LiteralPath $statusPath -Raw | ConvertFrom-Json
if (-not [bool]$status.supportColliderRequired -or [bool]$status.supportRendererAllowedInNormalMode) {
    Write-Host "[FAIL] Support visibility status JSON does not require collider-only hidden support."
    exit 1
}

Write-Host "[PASS] Runtime support surface renderer visibility is guarded."
exit 0
