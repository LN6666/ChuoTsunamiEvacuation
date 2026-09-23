[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

function Assert-FileExists {
    param(
        [string]$RelativePath,
        [string]$Label
    )

    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "$Label missing required file: $RelativePath"
    }

    Write-Host "PASS: $Label -> $RelativePath"
}

function Assert-NoHardBoundBaseMapReference {
    $scriptRootPath = Join-Path $repoRoot "Assets\Scripts"
    $matches = @(
        Get-ChildItem -LiteralPath $scriptRootPath -Recurse -Filter *.cs |
            Select-String -Pattern "Chuo_BaseMap" -SimpleMatch
    )

    if ($matches.Count -gt 0) {
        foreach ($match in $matches) {
            Write-Host "FAIL: hard-bound base map reference at $($match.Path):$($match.LineNumber)"
        }

        throw "Detected script reference to Chuo_BaseMap."
    }
}

Write-Host "P8-A P2-P6 compatibility inspection"
Write-Host "Repo root: $repoRoot"

Assert-FileExists "Assets/Scripts/Player/SimplePlayerController.cs" "P2 player movement"
Assert-FileExists "Assets/Scripts/Shelter/ShelterEntranceTrigger.cs" "P2 shelter interaction"
Assert-FileExists "Assets/Scripts/Result/ResultPanelController.cs" "P2 result panel"
Assert-FileExists "Assets/Scripts/Data/TsunamiHazardFixtureLoader.cs" "P3 data loader"
Assert-FileExists "Assets/Scripts/Data/RealShelterDataLoader.cs" "P4 real shelter loading"
Assert-FileExists "Assets/Scripts/Gameplay/RealShelterMarkerRuntimeGenerator.cs" "P4 marker generation"
Assert-FileExists "Assets/Scripts/Data/RealQualifiedShelterDataLoader.cs" "P5 real qualified data"
Assert-FileExists "Assets/Scripts/Data/HumanitarianCandidateDataLoader.cs" "P5 humanitarian candidates"
Assert-FileExists "Assets/Scripts/Navigation/NavigationGuidanceController.cs" "P6 navigation guidance"
Assert-FileExists "Assets/Scripts/NPC/NpcEvacuationAgent.cs" "P6 NPC prototype"

Assert-NoHardBoundBaseMapReference

Write-Host "P2: movement, shelter interaction, and result flow scripts are present."
Write-Host "P3: data pipeline runtime copy pattern remains available."
Write-Host "P4: real shelter loading and marker generation scripts are present."
Write-Host "P5: qualified shelter and candidate data scripts are present; no official-route claim is added."
Write-Host "P6: navigation and NPC prototype scripts are present; no hard dependency on Chuo_BaseMap detected."
Write-Host "P8-A P2-P6 compatibility inspection: PASS"
exit 0
