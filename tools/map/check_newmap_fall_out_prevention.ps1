param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$jsonPath = Join-Path $root "Assets\Data\P10\newmap_fall_out_prevention_report.json"
$playerPath = Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs"

if (-not (Test-Path -LiteralPath $jsonPath -PathType Leaf) -or -not (Test-Path -LiteralPath $playerPath -PathType Leaf)) {
    Write-Host "[FAIL] Fall-out prevention files are missing."
    exit 1
}

$report = Get-Content -Encoding UTF8 -LiteralPath $jsonPath -Raw | ConvertFrom-Json
$player = Get-Content -Encoding UTF8 -LiteralPath $playerPath -Raw

$passed = [bool]$report.fallRecoveryEnabled -and
    [bool]$report.playerCannotFallOutOfMap -and
    [bool]$report.playerCannotLeaveAirWallBounds -and
    -not [bool]$report.playerLogWarningSpamExpected -and
    $player -match "ConfigureGroundSafety" -and
    $player -match "fallRecoveryThresholdY" -and
    $player -match "LastFallRecoveryReason" -and
    $player -notmatch "NewMap player fall recovery returned the player"

if (-not $passed) {
    Write-Host "[FAIL] Fall-out prevention check failed."
    exit 1
}

Write-Host "[PASS] Fall-out prevention check passed."
exit 0
