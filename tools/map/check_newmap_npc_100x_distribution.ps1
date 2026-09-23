param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$configPath = Join-Path $root "Assets\Data\P10\newmap_npc_distribution_config.json"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_npc_100x_distribution_report.json"
$npcPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"

if (-not (Test-Path -LiteralPath $configPath -PathType Leaf)) { Write-Host "[FAIL] Missing NPC config."; exit 1 }
if (-not (Test-Path -LiteralPath $reportPath -PathType Leaf)) { Write-Host "[FAIL] Missing NPC 100x report."; exit 1 }
if (-not (Test-Path -LiteralPath $npcPath -PathType Leaf)) { Write-Host "[FAIL] Missing NPC runtime script."; exit 1 }

$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json
$npc = Get-Content -Encoding UTF8 -LiteralPath $npcPath -Raw

$passed = [int]$config.npcCountMultiplier -eq 100 -and
    [int]$config.maxNpcCount -eq 800 -and
    [bool]$config.useSectorDistribution -and
    [int]$config.sectorCount -ge 32 -and
    [int]$config.ringCount -ge 6 -and
    [bool]$config.avoidBuildings -and
    [bool]$config.usePooling -and
    [bool]$config.farNpcUpdateThrottle -and
    [int]$report.requestedNpcCount -eq 800 -and
    [int]$report.maxNpcCount -eq 800 -and
    $npc -match "RejectedInsideBuildingCount" -and
    $npc -match "farNpcStaticProxyMode" -and
    $npc -match "IsInsideBuildingBounds"

if (-not $passed) {
    Write-Host "[FAIL] NPC 100x distribution config/implementation is incomplete."
    exit 1
}

Write-Host "[PASS] NPC 100x distribution is capped and guarded."
exit 0
