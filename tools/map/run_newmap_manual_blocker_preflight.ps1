param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"

function Add-Check {
    param([string]$Name, [bool]$Passed, [string]$Detail)
    $script:Checks += [pscustomobject]@{ Name = $Name; Passed = $Passed; Detail = $Detail }
}

$script:Checks = @()
$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$scenePath = Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity"

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Chuo_BaseMap scene exists" (Test-Path -LiteralPath $scenePath -PathType Leaf) $scenePath
Add-Check "PlayerSpawnRoot exists in scene" ([bool](Select-String -LiteralPath $scenePath -Pattern "m_Name: PlayerSpawnRoot" -SimpleMatch -Quiet)) "PlayerSpawnRoot"

$playerCode = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapPlayerController.cs") -Raw
$bootstrapCode = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs") -Raw
$buildUtility = Get-Content -LiteralPath (Join-Path $root "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs") -Raw
Add-Check "Active camera created at runtime" ($playerCode -match "Main Camera" -and $playerCode -match "Camera\.main") "NewMapPlayerController"
Add-Check "Mouse look config present" ($playerCode -match "MouseLookEnabled" -and $playerCode -match "CursorLockMode\.Locked") "MouseLookEnabled/CursorLockMode"
Add-Check "Debug diagnostics inactive by default" ($bootstrapCode -match "DebugDiagnosticsRoot" -and $bootstrapCode -match "SetActive\(false\)") "DebugDiagnosticsRoot"
Add-Check "Gameplay support root present" ($bootstrapCode -match "GameplaySupportRoot") "GameplaySupportRoot"
Add-Check "Visual-fix build method present" ($buildUtility -match "BuildNewMapVisualFixPlayerCommandLine" -and $buildUtility -match "NewMapVisualFixPre") "BuildNewMapVisualFixPlayerCommandLine"

& (Join-Path $root "tools\map\validate_newmap_manual_blocker_json.ps1") -ProjectRoot $root
Add-Check "Manual blocker JSON validation" ($LASTEXITCODE -eq 0) "validate_newmap_manual_blocker_json.ps1"

& (Join-Path $root "tools\map\check_newmap_mouse_look_config.ps1") -ProjectRoot $root
Add-Check "Mouse look preflight" ($LASTEXITCODE -eq 0) "check_newmap_mouse_look_config.ps1"

& (Join-Path $root "tools\map\check_newmap_debug_objects_hidden.ps1") -ProjectRoot $root
Add-Check "Debug object cleanup preflight" ($LASTEXITCODE -eq 0) "check_newmap_debug_objects_hidden.ps1"

& (Join-Path $root "tools\map\check_newmap_ground_visual_alignment.ps1") -ProjectRoot $root
Add-Check "Ground alignment preflight" ($LASTEXITCODE -eq 0) "check_newmap_ground_visual_alignment.ps1"

& (Join-Path $root "tools\map\check_newmap_materials_lighting.ps1") -ProjectRoot $root
Add-Check "Materials/lighting preflight" ($LASTEXITCODE -eq 0) "check_newmap_materials_lighting.ps1"

$npcConfig = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_npc_distribution_config.json") -Raw | ConvertFrom-Json
$npcReport = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_npc_distribution_report.json") -Raw | ConvertFrom-Json
Add-Check "NPC 20x config present" ([int]$npcConfig.npcCountMultiplier -eq 20 -and [int]$npcReport.requestedCount -eq 160) "requested=$($npcReport.requestedCount)"
Add-Check "NPC cap safe" ([int]$npcReport.cappedCount -le [int]$npcReport.maxNpcCount) "capped=$($npcReport.cappedCount) max=$($npcReport.maxNpcCount)"
Add-Check "NPC wide distribution config" ([int]$npcConfig.distributionRadiusMeters -eq 1000 -and [int]$npcConfig.sectorCount -ge 12 -and [int]$npcConfig.ringCount -ge 4) "radius=$($npcConfig.distributionRadiusMeters)"

$readiness = Get-Content -LiteralPath (Join-Path $root "Assets\Data\P10\newmap_manual_playtest_readiness.json") -Raw | ConvertFrom-Json
Add-Check "Final readiness decision exists" (-not [string]::IsNullOrWhiteSpace([string]$readiness.manualReadinessDecision)) $readiness.manualReadinessDecision

$forbiddenDocs = @(Get-ChildItem -LiteralPath (Join-Path $root "docs") -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match "P10[-_](E|F|G)([-_.]|$)" })
Add-Check "No P10-E/F/G docs" ($forbiddenDocs.Count -eq 0) ($forbiddenDocs.Name -join ", ")

$archives = @(Get-ChildItem -LiteralPath $root -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match "\.(zip|7z|tar|gz)$" })
Add-Check "No root release archive" ($archives.Count -eq 0) ($archives.Name -join ", ")

$logSummaryPath = Join-Path $root "Assets\Data\P10\newmap_visual_fix_player_log_summary.json"
if (Test-Path -LiteralPath $logSummaryPath -PathType Leaf) {
    $logSummary = Get-Content -LiteralPath $logSummaryPath -Raw | ConvertFrom-Json
    Add-Check "Latest visual-fix Player.log has no errors" ([int]$logSummary.errorCount -eq 0) "errors=$($logSummary.errorCount)"
}
else {
    Add-Check "Latest visual-fix Player.log summary absent before build" $true "not yet generated"
}

Write-Host "NewMap manual blocker preflight"
foreach ($check in $script:Checks) {
    $label = if ($check.Passed) { "PASS" } else { "FAIL" }
    Write-Host ("[{0}] {1}: {2}" -f $label, $check.Name, $check.Detail)
}

$failures = @($script:Checks | Where-Object { -not $_.Passed })
if ($failures.Count -gt 0) {
    Write-Host "Preflight result: FAIL"
    exit 1
}

Write-Host "Preflight result: PASS"
exit 0
