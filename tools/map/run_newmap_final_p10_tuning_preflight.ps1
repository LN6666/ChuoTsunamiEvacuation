param()

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

function Add-Check {
    param([string]$Name, [bool]$Passed, [string]$Detail = "")
    $suffix = if ($Detail) { ": $Detail" } else { "" }
    if ($Passed) {
        Write-Host "[PASS] $Name$suffix"
    }
    else {
        Write-Host "[FAIL] $Name$suffix"
        $script:Failed = $true
    }
}

$Failed = $false

powershell -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot "validate_newmap_final_p10_tuning_json.ps1")
powershell -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot "check_newmap_stamina_3500_sprint20.ps1")

$scenePath = Join-Path $ProjectRoot "Assets\Scenes\Chuo_BaseMap.unity"
$buildTool = Join-Path $ProjectRoot "tools\map\build_newmap_final_p10_tuning_player.ps1"
$parseTool = Join-Path $ProjectRoot "tools\map\parse_newmap_final_p10_tuning_player_log.ps1"
$editorUtility = Join-Path $ProjectRoot "Assets\Scripts\Editor\NewMapSceneSetupUtility.cs"
$runtimeBootstrap = Join-Path $ProjectRoot "Assets\Scripts\NewMap\NewMapRuntimeBootstrap.cs"
$uiSource = Join-Path $ProjectRoot "Assets\Scripts\NewMap\NewMapRuntimeUI.cs"

Add-Check "Chuo_BaseMap scene exists" (Test-Path -LiteralPath $scenePath -PathType Leaf) "Assets/Scenes/Chuo_BaseMap.unity"
Add-Check "Build tool exists" (Test-Path -LiteralPath $buildTool -PathType Leaf) "tools/map/build_newmap_final_p10_tuning_player.ps1"
Add-Check "Parser tool exists" (Test-Path -LiteralPath $parseTool -PathType Leaf) "tools/map/parse_newmap_final_p10_tuning_player_log.ps1"
Add-Check "Editor build method exists" ((Get-Content -LiteralPath $editorUtility -Raw) -match "BuildNewMapFinalP10TuningPlayerCommandLine") "NewMapSceneSetupUtility"
Add-Check "Runtime self-audit logs final stamina/sprint" ((Get-Content -LiteralPath $runtimeBootstrap -Raw) -match "mode_speed_stamina_rules") "NewMapRuntimeBootstrap"
Add-Check "Runtime UI rules mention 3500" ((Get-Content -LiteralPath $uiSource -Raw) -match "3500") "NewMapRuntimeUI"

$forbidden = @(
    "P10-E",
    "P10-F",
    "P10-G",
    "P10E",
    "P10F",
    "P10G",
    "FinalRelease",
    "ReleaseArchive"
)
$forbiddenHits = @()
foreach ($token in $forbidden) {
    $hits = @(Get-ChildItem -LiteralPath $ProjectRoot -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object {
            $_.FullName -notmatch "\\Library\\" -and
            $_.FullName -notmatch "\\Temp\\" -and
            $_.FullName -notmatch "\\Logs\\" -and
            $_.FullName -notmatch "\\Builds\\" -and
            $_.Name -match [regex]::Escape($token)
        } |
        Select-Object -First 3)
    if ($hits.Count -gt 0) {
        $forbiddenHits += $hits.FullName
    }
}
Add-Check "No final release/archive/P10-E/F/G artifacts" ($forbiddenHits.Count -eq 0) ($forbiddenHits -join "; ")

if ($Failed) {
    Write-Host "Preflight result: FAIL"
    exit 1
}

Write-Host "Preflight result: PASS"
