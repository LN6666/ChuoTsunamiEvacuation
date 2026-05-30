param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Read-Json {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing $RelativePath"
        exit 1
    }

    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw | ConvertFrom-Json
}

$config = Read-Json "Assets\Data\P10\newmap_ground_raise_30_config.json"
$report = Read-Json "Assets\Data\P10\newmap_ground_raise_30_report.json"
$resnap = Read-Json "Assets\Data\P10\newmap_ground_raise_30_resnap_report.json"
$floating = Read-Json "Assets\Data\P10\newmap_building_floating_after_30_raise.json"

$errors = @()
if (-not [bool]$config.enabled) { $errors += "30 percent ground raise config is disabled." }
if ([math]::Abs([double]$config.raisePercent - 0.30) -gt 0.001) { $errors += "raisePercent must be 0.30." }
if ([string]$report.baselineModeUsed -notin @("absolute_ground_y", "existing_raise_offset", "fallback_delta")) { $errors += "baseline mode is invalid or missing." }
if ($null -eq $report.oldGroundY -or $null -eq $report.oldRaiseOffset) { $errors += "old ground Y / raise offset missing." }
if ($null -eq $report.newGroundY -or $null -eq $report.newRaiseOffset) { $errors += "new ground Y / raise offset missing." }
if ([math]::Abs([double]$report.newGroundY - [double]$report.oldGroundY) -le 0.001) { $errors += "new ground Y is unchanged." }
if ([math]::Abs([double]$report.newRaiseOffset - [double]$report.oldRaiseOffset) -le 0.001) { $errors += "new raise offset is unchanged." }
if ([math]::Abs([double]$report.actualRaisePercent - 0.30) -gt 0.035) { $errors += "actual raise percent is not about 30 percent." }
if (-not [bool]$resnap.groundCoverColliderVisualAligned) { $errors += "ground cover collider/visual alignment not confirmed." }
if (-not [bool]$resnap.playerCannotFallThroughRaisedGround) { $errors += "fall-through prevention not confirmed." }
if ([bool]$resnap.blueGroundRegression) { $errors += "blue ground regression is marked true." }
if ([bool]$floating.fullFixClaimed) { $errors += "Building floating report must not claim a full fix without manual evidence." }

if ($errors.Count -gt 0) {
    foreach ($errorMessage in $errors) { Write-Host "[FAIL] $errorMessage" }
    exit 1
}

Write-Host "[PASS] NewMap 30 percent ground raise JSON check passed."
exit 0
