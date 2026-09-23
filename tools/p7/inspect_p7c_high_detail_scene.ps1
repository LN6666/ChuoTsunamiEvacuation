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

$requiredDocs = @(
    "docs/P7C_HIGH_DETAIL_CHUO_SCENE_PLAN.md",
    "docs/P7C_PLATEAU_IMPORT_EXECUTION_OR_CHECKLIST.md",
    "docs/P7C_P7D_PROFILING_TARGET.md"
)

function Test-SceneObjectName {
    param(
        [string]$SceneText,
        [string]$Name
    )

    return $SceneText.IndexOf("m_Name: $Name", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}

Write-Host "P7-C high-detail scene inspection"
Write-Host "Scene: $sceneRelativePath"

$failed = $false
foreach ($requiredDoc in $requiredDocs) {
    $docPath = Join-Path $repoRoot ($requiredDoc -replace "/", "\")
    if (-not (Test-Path -LiteralPath $docPath -PathType Leaf)) {
        Write-Host "FAIL: missing required high-detail documentation: $requiredDoc"
        $failed = $true
    }
}

if (-not (Test-Path -LiteralPath $scenePath -PathType Leaf)) {
    Write-Host "FAIL: missing P7-D high-detail profiling target scene: $sceneRelativePath"
    Write-Host "A pending-import checklist alone is not enough for final P7-D profiling; create the scene shell or document a hard blocker."
    exit 1
}

$sceneText = Get-Content -Raw -LiteralPath $scenePath

$forbiddenFragments = @(
    "Chuo_BaseMap",
    "Assets/PLATEAU",
    "Assets/Data",
    "EvacuationGameManager",
    "MovingTsunamiWall",
    "Inundation",
    "LightCurtain",
    "CrowdFailure"
)

foreach ($fragment in $forbiddenFragments) {
    if ($sceneText.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Write-Host "FAIL: high-detail scene contains forbidden fragment: $fragment"
        $failed = $true
    }
}

$requiredSceneObjects = @(
    "P7HighDetailRoot",
    "P7D_ProfilingTarget_Metadata",
    "PLATEAUImportStatus_PendingManualSdkImport",
    "P8P9P10_BaselineIntent_PendingP7DConfirmation",
    "NoP8P9Systems_NoGameplayRuleChanges"
)

foreach ($requiredObject in $requiredSceneObjects) {
    if (-not (Test-SceneObjectName -SceneText $sceneText -Name $requiredObject)) {
        Write-Host "FAIL: high-detail scene missing metadata object: $requiredObject"
        $failed = $true
    }
}

Write-Host ""
Write-Host "Layer root readiness:"
foreach ($rootName in $expectedRootNames) {
    $status = "PASS"
    if (-not (Test-SceneObjectName -SceneText $sceneText -Name $rootName)) {
        $status = "FAIL"
        $failed = $true
    }

    Write-Host ("{0,-24} {1}" -f $rootName, $status)
}

if ($sceneText.IndexOf("ActualLoadedStatus_NotImported_RenderableEvidencePending", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
    Write-Host ""
    Write-Host "INFO: Scene explicitly marks PLATEAU renderable asset import as pending. P7-D cannot treat profiling as final until actual assets are loaded."
}

if ($failed) {
    Write-Host ""
    Write-Host "P7-C high-detail scene inspection: FAIL" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "P7-C high-detail scene inspection: PASS" -ForegroundColor Green
exit 0
