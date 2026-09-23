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

$npcConfig = Read-Json "Assets\Data\P10\newmap_npc_distribution_config.json"
$doubleConfig = Read-Json "Assets\Data\P10\newmap_npc_double_count_config.json"
$doubleReport = Read-Json "Assets\Data\P10\newmap_npc_double_count_report.json"
$movement = Read-Json "Assets\Data\P10\newmap_npc_movement_config.json"

$errors = @()
if ([int]$npcConfig.npcCountMultiplier -ne 200) { $errors += "NPC multiplier must be 200 to double the prior 100x/800 count." }
if ([int]$npcConfig.maxNpcCount -ne 1600) { $errors += "NPC max cap must be 1600." }
if ([int]$doubleConfig.previousRequestedCount -ne 800 -or [int]$doubleConfig.requestedNpcCount -ne 1600) { $errors += "Double-count config must record 800 -> 1600." }
if ([int]$doubleReport.previousRequestedCount -ne 800 -or [int]$doubleReport.newRequestedCount -ne 1600) { $errors += "Double-count report must record 800 -> 1600." }
if ([int]$doubleReport.newMaxCap -ne 1600) { $errors += "Double-count report must cap at 1600." }
if (-not [bool]$doubleConfig.usePooling -or -not [bool]$npcConfig.usePooling) { $errors += "NPC pooling must remain enabled." }
if (-not [bool]$doubleConfig.farNpcUpdateThrottle -or -not [bool]$movement.farNpcUpdateThrottle) { $errors += "Far NPC throttling must remain enabled." }
if (-not [bool]$doubleConfig.farNpcStaticProxyEnabled -or -not [bool]$movement.farNpcStaticProxyMode) { $errors += "Far NPC static proxy must be enabled for 1600 NPCs." }
if ([bool]$doubleReport.capped -and [string]::IsNullOrWhiteSpace([string]$doubleReport.capReason)) { $errors += "Capped NPC count needs a cap reason." }

if ($errors.Count -gt 0) {
    foreach ($errorMessage in $errors) { Write-Host "[FAIL] $errorMessage" }
    exit 1
}

Write-Host "[PASS] NewMap NPC double-count check passed."
exit 0
