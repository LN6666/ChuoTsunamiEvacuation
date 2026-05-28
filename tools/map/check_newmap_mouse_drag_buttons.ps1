param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$configPath = Join-Path $root "Assets\Data\P10\newmap_mouse_drag_look_config.json"
$playerPath = Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs"
$testsPath = Join-Path $root "Assets\Tests\PlayMode\NewMapRuntimePlayModeTests.cs"

$config = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
$buttons = @($config.allowedButtons)
$playerCode = Get-Content -LiteralPath $playerPath -Raw
$tests = Get-Content -LiteralPath $testsPath -Raw

$ok =
    [bool]$config.enabled -and
    [bool]$config.lookRequiresMouseButton -and
    ($buttons -contains "LeftMouse") -and
    ($buttons -contains "RightMouse") -and
    [bool]$config.cursorVisibleWhenNotDragging -and
    $playerCode -match "IsAnyLookMouseButtonHeld" -and
    $playerCode -match "allowedLookMouseButtons" -and
    $playerCode -match "ApplyLookInputForDiagnostics\(float mouseX, float mouseY, string mouseButtonName\)" -and
    $tests -match '"LeftMouse"' -and
    $tests -match '"RightMouse"' -and
    $tests -match "ApplyLookInputForDiagnostics\(4f, -3f, false\)"

if (-not $ok) {
    Write-Host "[FAIL] Left/right mouse drag-look config/code/test check failed."
    exit 1
}

Write-Host "[PASS] Left/right mouse drag-look config/code/test check passed."
exit 0
