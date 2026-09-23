param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$spawnPath = Join-Path $root "Assets\Data\P10\newmap_ground_cover_spawn_npc_target_status.json"
$raisePath = Join-Path $root "Assets\Data\P10\newmap_ground_raise_alignment_report.json"
$bootstrapPath = Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$npcPath = Join-Path $root "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"

if (-not (Test-Path -LiteralPath $spawnPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $raisePath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $bootstrapPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $npcPath -PathType Leaf)) {
    Write-Host "[FAIL] Spawn/NPC ground cover files are missing."
    exit 1
}

$spawn = Get-Content -Encoding UTF8 -LiteralPath $spawnPath -Raw | ConvertFrom-Json
$raise = Get-Content -Encoding UTF8 -LiteralPath $raisePath -Raw | ConvertFrom-Json
$bootstrap = Get-Content -Encoding UTF8 -LiteralPath $bootstrapPath -Raw
$npc = Get-Content -Encoding UTF8 -LiteralPath $npcPath -Raw

$passed = [bool]$spawn.playerSpawnUsesGroundCoverY -and
    [bool]$spawn.randomSpawnCandidatesUseGroundCoverY -and
    [bool]$spawn.spawnAvoidsBuildingBounds -and
    [bool]$spawn.npcDistributionUsesGroundCoverY -and
    [bool]$spawn.officialShelterTargetsUseGroundCoverY -and
    [bool]$spawn.nonOfficialCandidateTargetsUseGroundCoverY -and
    [bool]$raise.playerStandsOnVisibleCover -and
    [bool]$raise.npcUsesSameGroundReference -and
    [bool]$raise.targetUsesSameGroundReference -and
    $bootstrap -match "ResolveSupportSurfaceY" -and
    $bootstrap -match "NewMapGameplayGroundCoverConfig" -and
    $bootstrap -match "position\.y = ResolveLocalSupportSurfaceY" -and
    $npc -match "snapped = new Vector3\(candidate\.x, Mathf\.Clamp\(fallbackY"

if (-not $passed) {
    Write-Host "[FAIL] Spawn/NPC/target ground cover alignment validation failed."
    exit 1
}

Write-Host "[PASS] Player, NPCs, targets, and green frames are configured for ground-cover Y."
exit 0
