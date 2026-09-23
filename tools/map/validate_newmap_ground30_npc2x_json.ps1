param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Read-RequiredText {
    param([string]$RelativePath)
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Write-Host "[FAIL] Missing $RelativePath"
        exit 1
    }

    return Get-Content -Encoding UTF8 -LiteralPath $path -Raw
}

& (Join-Path $PSScriptRoot "check_newmap_ground_raise_30.ps1") -ProjectRoot $root
& (Join-Path $PSScriptRoot "check_newmap_npc_boundary_distribution_2270.ps1") -ProjectRoot $root
& (Join-Path $PSScriptRoot "check_newmap_npc_double_count.ps1") -ProjectRoot $root

$bootstrap = Read-RequiredText "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$npc = Read-RequiredText "Assets\Scripts\NewMap\NewMapNpcCrowdPrototype.cs"
$tests = Read-RequiredText "Assets\Tests\PlayMode\NewMapRuntimePlayModeTests.cs"

if ($bootstrap -notmatch "ResolveGroundRaise30Y" -or $bootstrap -notmatch "NewMapGroundRaise30Config") {
    Write-Host "[FAIL] Runtime ground raise 30 integration is missing."
    exit 1
}
if ($npc -notmatch "distributionRadiusMeters = 2270f" -or $npc -notmatch "npcCountMultiplier = 200" -or $npc -notmatch "maxNpcCount = 1600") {
    Write-Host "[FAIL] NPC runtime defaults are not updated to 2270m/1600."
    exit 1
}
if ($tests -notmatch "1600" -or $tests -notmatch "2270\.5f" -or $tests -notmatch "LastGroundRaise30ActualRaisePercent") {
    Write-Host "[FAIL] PlayMode tests do not cover 30 percent raise and 2x NPC distribution."
    exit 1
}

Write-Host "[PASS] NewMap ground30/NPC2x JSON and runtime source validation passed."
exit 0
