param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$configPath = Join-Path $root "Assets\Data\P10\newmap_mouse_drag_look_config.json"
$playerPath = Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs"

$config = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
$playerCode = Get-Content -LiteralPath $playerPath -Raw

$ok =
    [bool]$config.enabled -and
    [bool]$config.lookRequiresMouseButton -and
    [string]$config.lookMouseButton -eq "RightMouse" -and
    [bool]$config.cursorVisibleWhenNotDragging -and
    $playerCode -match "Input\.GetMouseButton" -and
    $playerCode -match "ApplyLookInputForDiagnostics" -and
    $playerCode -match "SetMouseLookDragging\(false\)" -and
    $playerCode -notmatch "ApplyLookDelta\(Input\.GetAxisRaw\(\""Mouse X\""\), Input\.GetAxisRaw\(\""Mouse Y\""\)\);\s*return;"

if (-not $ok) {
    Write-Host "[FAIL] Mouse drag-look config/code check failed."
    exit 1
}

Write-Host "[PASS] Mouse drag-look config/code check passed."
exit 0
