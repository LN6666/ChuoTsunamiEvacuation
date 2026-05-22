[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$sceneRelativePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
$scenePath = Join-Path $repoRoot ($sceneRelativePath -replace "/", "\")

$expectedRootNames = @(
    "Buildings",
    "Roads",
    "Bridges",
    "Underground",
    "CityFurniture",
    "Water",
    "Vegetation",
    "Relief",
    "DisasterRisk",
    "LandUse",
    "UrbanPlanningDecision",
    "P2P6Compatibility"
)

function Test-SceneName {
    param(
        [string]$SceneText,
        [string]$Name
    )

    return $SceneText.IndexOf("m_Name: $Name", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}

Write-Host "P7-D high-detail scene inspection"
Write-Host "Scene: $sceneRelativePath"

if (-not (Test-Path -LiteralPath $scenePath -PathType Leaf)) {
    Write-Host "FAIL: missing high-detail scene: $sceneRelativePath" -ForegroundColor Red
    exit 1
}

$sceneText = Get-Content -Raw -LiteralPath $scenePath
$failed = $false

foreach ($fragment in @("Chuo_BaseMap", "Assets/PLATEAU", "Assets/Data", "EvacuationGameManager", "MovingTsunamiWall", "LightCurtain", "RiskFront")) {
    if ($sceneText.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Write-Host "FAIL: scene contains forbidden fragment: $fragment"
        $failed = $true
    }
}

Write-Host ""
Write-Host "Expected roots:"
foreach ($rootName in $expectedRootNames) {
    $status = if (Test-SceneName -SceneText $sceneText -Name $rootName) { "PASS" } else { "FAIL" }
    if ($status -eq "FAIL") {
        $failed = $true
    }

    Write-Host ("{0,-28} {1}" -f $rootName, $status)
}

$meshRendererCount = ([regex]::Matches($sceneText, "MeshRenderer:", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
$meshFilterCount = ([regex]::Matches($sceneText, "MeshFilter:", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
$plateauComponentEvidence = $sceneText.IndexOf("PLATEAUInstancedCityModel", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
$pendingMarker = $sceneText.IndexOf("ActualLoadedStatus_NotImported_RenderableEvidencePending", [System.StringComparison]::OrdinalIgnoreCase) -ge 0

Write-Host ""
Write-Host "Renderable evidence:"
Write-Host "MeshRenderer count: $meshRendererCount"
Write-Host "MeshFilter count: $meshFilterCount"
Write-Host "PLATEAU component evidence: $plateauComponentEvidence"
Write-Host "Pending import marker: $pendingMarker"

if ($meshRendererCount -eq 0 -and $meshFilterCount -eq 0 -and -not $plateauComponentEvidence) {
    Write-Host "BLOCKED: high-detail scene is still a shell/import target, not a populated profiling scene." -ForegroundColor Yellow
}

if ($failed) {
    Write-Host ""
    Write-Host "P7-D high-detail scene inspection: FAIL" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "P7-D high-detail scene inspection: PASS_WITH_IMPORT_BLOCKER_STATUS" -ForegroundColor Green
exit 0
