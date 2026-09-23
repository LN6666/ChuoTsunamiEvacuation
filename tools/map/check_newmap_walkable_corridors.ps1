param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Require-Condition {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        Write-Host "[FAIL] $Message"
        exit 1
    }
}

$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$reportPath = Join-Path $root "Assets\Data\P10\newmap_walkable_corridor_collision_report.json"
$whitelistPath = Join-Path $root "Assets\Data\P10\newmap_collision_whitelist_final_report.json"

Require-Condition (Test-Path -LiteralPath $bootstrapPath -PathType Leaf) "Runtime bootstrap missing."
Require-Condition (Test-Path -LiteralPath $reportPath -PathType Leaf) "Walkable corridor report missing."
Require-Condition (Test-Path -LiteralPath $whitelistPath -PathType Leaf) "Final whitelist report missing."

$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$report = Get-Content -Encoding UTF8 -LiteralPath $reportPath -Raw | ConvertFrom-Json
$whitelist = Get-Content -Encoding UTF8 -LiteralPath $whitelistPath -Raw | ConvertFrom-Json

Require-Condition ($bootstrap -match "RunBuildingCollisionPrecisionCorridorDiagnostics") "Walkable corridor diagnostics are not implemented."
Require-Condition ($bootstrap -match "CarveBuildingPrecisionTargetClearances") "Active target clearance carve is not implemented."
Require-Condition ($bootstrap -match "LastBuildingPrecisionUnexpectedCorridorBlockers") "Unexpected corridor blocker counter missing."
Require-Condition ($bootstrap -match "LastBuildingPrecisionActiveTargetApproachBlocked") "Active target approach counter missing."
Require-Condition ($bootstrap -match "building_collision_precision_corridors") "Corridor self-audit smoke scenario missing."
Require-Condition ($report.sampleScope -contains "active official shelter approach rings") "Official shelter approach sampling missing."
Require-Condition ($report.sampleScope -contains "active non-official candidate approach rings") "Non-official candidate approach sampling missing."
Require-Condition ($whitelist.nonBlockingCategories -contains "route_line_visual") "Route-line visual nonblocking category missing."
Require-Condition ($whitelist.nonBlockingCategories -contains "green_frame_visual") "Green-frame visual nonblocking category missing."
Require-Condition ($whitelist.nonBlockingCategories -contains "label_visual") "Label visual nonblocking category missing."

Write-Host "[PASS] NewMap walkable corridor checks passed."
exit 0
