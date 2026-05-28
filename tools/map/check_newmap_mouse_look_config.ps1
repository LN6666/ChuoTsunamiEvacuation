param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$playerPath = Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs"
$statusPath = Join-Path $root "Assets\Data\P10\newmap_mouse_look_camera_status.json"
$player = Get-Content -LiteralPath $playerPath -Raw
$status = Get-Content -LiteralPath $statusPath -Raw | ConvertFrom-Json

$checks = @(
    ($player -match "MouseLookEnabled"),
    ($player -match "CursorLockMode\.Locked"),
    ($player -match "CursorLockMode\.None"),
    ($player -match "ApplyLookDelta"),
    ([bool]$status.mouseLookRestored),
    ([bool]$status.gameplayCursorLocks),
    ([bool]$status.menuPauseResultCursorUnlocks),
    ([bool]$status.tourismModeMouseLook),
    ([bool]$status.evacuationModeMouseLook)
)

if ($checks -contains $false) {
    Write-Host "[FAIL] Mouse-look config/code check failed."
    exit 1
}

Write-Host "[PASS] Mouse-look config/code check passed."
exit 0
