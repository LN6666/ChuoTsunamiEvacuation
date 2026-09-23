param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$playerPath = Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs"
$npcPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"
$configPath = Join-Path $root "Assets\Data\P10\newmap_player_npc_collision_config.json"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_player_npc_collision_report.json"

if (-not (Test-Path -LiteralPath $playerPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $npcPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $configPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $reportPath -PathType Leaf)) {
    Write-Host "[FAIL] Player/NPC collision files are missing."
    exit 1
}

$player = Get-Content -Encoding UTF8 -LiteralPath $playerPath -Raw
$npc = Get-Content -Encoding UTF8 -LiteralPath $npcPath -Raw
$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json

$passed =
    [bool]$config.enabled -and
    [bool]$config.preventDirectOverlap -and
    [bool]$config.softSlowdownEnabled -and
    [bool]$report.playerCannotPassStraightThroughNearNpc -and
    (-not [bool]$report.physicsExplosionRisk) -and
    ($player -match "ConfigurePlayerNpcCollision") -and
    ($player -match "ApplyPlayerNpcCollisionCorrection") -and
    ($npc -match "ResolvePlayerPositionAgainstNpcs") -and
    ($npc -match "CapsuleCollider") -and
    ($npc -match "isTrigger = true")

if (-not $passed) {
    Write-Host "[FAIL] Player/NPC soft collision validation failed."
    exit 1
}

Write-Host "[PASS] Player/NPC soft collision is configured and implemented."
exit 0
