param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Require-Text {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -notmatch $Pattern) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

function Require-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$playerPath = Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs"
$npcPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"
$configPath = Join-Path $root "Assets\Data\P10\newmap_building_collision_precision_config.json"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_building_collision_precision_report.json"

Require-Condition (Test-Path -LiteralPath $bootstrapPath -PathType Leaf) "Runtime bootstrap missing."
Require-Condition (Test-Path -LiteralPath $playerPath -PathType Leaf) "Player controller missing."
Require-Condition (Test-Path -LiteralPath $npcPath -PathType Leaf) "NPC prototype missing."
Require-Condition (Test-Path -LiteralPath $configPath -PathType Leaf) "Precision config missing."
Require-Condition (Test-Path -LiteralPath $reportPath -PathType Leaf) "Precision report missing."

$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$player = Get-Content -Encoding UTF8 -LiteralPath $playerPath -Raw
$npc = Get-Content -Encoding UTF8 -LiteralPath $npcPath -Raw
$config = Get-Content -Encoding UTF8 -LiteralPath $configPath -Raw | ConvertFrom-Json
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json

Require-Text $bootstrap "NewMapBuildingCollisionPrecisionConfig" "Precision config class/load path missing."
Require-Text $bootstrap "TryBuildProjectedMeshFootprintBounds" "Mesh-projected footprint builder missing."
Require-Text $bootstrap "BuildTightBuildingFootprintBounds" "Tight footprint proxy builder missing."
Require-Text $bootstrap "NormalizeBuildingPrecisionBoundsToGroundY" "Ground-Y normalization missing."
Require-Text $bootstrap "CarveBuildingPrecisionTargetClearances" "Active target interaction clearance carving missing."
Require-Text $bootstrap "AddSplitBuildingBoundsAroundClearance" "Clearance splitting helper missing."
Require-Text $bootstrap "RunBuildingCollisionPrecisionCorridorDiagnostics" "Corridor diagnostics missing."
Require-Text $bootstrap "buildingPrecisionMaxWidth" "Precision max-width runtime logging missing."
Require-Text $bootstrap "building_collision_precision_tight_proxies" "Precision self-audit scenario missing."
Require-Text $bootstrap "building_collision_precision_corridors" "Corridor self-audit scenario missing."
Require-Text $player "GetBuildingCollisionBoundsForDiagnostics" "Player building bounds diagnostic missing."
Require-Text $player "IsInsideBuildingForDiagnostics" "Player inside-building diagnostic missing."
Require-Text $npc "BuildingAvoidanceBoundsCount" "NPC building avoidance diagnostic missing."

Require-Condition ([bool]$config.enabled) "Precision config disabled."
Require-Condition ([double]$config.maxColliderWidthMeters -le 80.0) "Precision width cap exceeds 80m."
Require-Condition ([double]$config.maxColliderDepthMeters -le 80.0) "Precision depth cap exceeds 80m."
Require-Condition ([double]$config.colliderHeightMeters -le 8.0) "Precision height cap exceeds player-relevant range."
Require-Condition ([bool]$config.disableClusterRootColliders) "Cluster/root bounds are not disabled/skipped."
Require-Condition ([bool]$config.carveActiveTargetInteractionClearance) "Active target clearance carving disabled."
Require-Condition ([string]$report.implementation -match "capped at 80m") "Precision report does not document 80m cap."
Require-Condition ([bool]$report.notGisGradeFootprintAccuracyClaim) "Precision report must not claim GIS-grade accuracy."

Write-Host "[PASS] NewMap building collision precision source/config checks passed."
exit 0
