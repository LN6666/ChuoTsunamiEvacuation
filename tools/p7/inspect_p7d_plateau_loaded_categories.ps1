[CmdletBinding()]
param(
    [string]$PlateauDataRoot = "D:\PLATEAU_DATA\Chuo_2025_CityGML"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$sceneRelativePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
$scenePath = Join-Path $repoRoot ($sceneRelativePath -replace "/", "\")
$unityRoots = @(
    "Assets/P7HighDetail/Imported",
    "Assets/P7HighDetail/PLATEAU",
    "Assets/Scenes/P7HighDetail"
)

$categories = @(
    @{ Category = "Buildings"; TargetLod = "LOD3"; SourceFolder = "bldg"; SceneRoot = "Buildings" },
    @{ Category = "Roads"; TargetLod = "LOD3"; SourceFolder = "tran"; SceneRoot = "Roads" },
    @{ Category = "Bridges"; TargetLod = "LOD3"; SourceFolder = "brid"; SceneRoot = "Bridges" },
    @{ Category = "Underground"; TargetLod = "LOD3 if available"; SourceFolder = "ubld"; SceneRoot = "Underground" },
    @{ Category = "CityFurniture"; TargetLod = "LOD2/LOD3"; SourceFolder = "frn"; SceneRoot = "CityFurniture" },
    @{ Category = "Water"; TargetLod = "LOD1"; SourceFolder = "wtr"; SceneRoot = "Water" },
    @{ Category = "Vegetation"; TargetLod = "LOD3 if available"; SourceFolder = "veg"; SceneRoot = "Vegetation" },
    @{ Category = "Relief"; TargetLod = "import terrain"; SourceFolder = "dem"; SceneRoot = "Relief" },
    @{ Category = "DisasterRisk"; TargetLod = "import"; SourceFolder = "fld"; SceneRoot = "DisasterRisk" },
    @{ Category = "LandUse"; TargetLod = "import"; SourceFolder = "luse"; SceneRoot = "LandUse" },
    @{ Category = "UrbanPlanningDecision"; TargetLod = "LOD1"; SourceFolder = "urf"; SceneRoot = "UrbanPlanningDecision" }
)

$renderableExtensions = @(".prefab", ".fbx", ".obj", ".dae", ".gltf", ".glb", ".asset", ".mesh")
$sceneText = if (Test-Path -LiteralPath $scenePath -PathType Leaf) { Get-Content -Raw -LiteralPath $scenePath } else { "" }
$udxRoot = Join-Path $PlateauDataRoot "udx"
$allLoaded = $true

function Get-UnityEvidence {
    param(
        [string]$Category,
        [string]$SourceFolder
    )

    $matches = New-Object System.Collections.Generic.List[string]
    foreach ($unityRoot in $unityRoots) {
        $fullRoot = Join-Path $repoRoot ($unityRoot -replace "/", "\")
        if (-not (Test-Path -LiteralPath $fullRoot)) {
            continue
        }

        $files = Get-ChildItem -LiteralPath $fullRoot -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object {
                $renderableExtensions -contains $_.Extension.ToLowerInvariant() -and
                ($_.FullName.IndexOf($SourceFolder, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
                 $_.FullName.IndexOf($Category, [System.StringComparison]::OrdinalIgnoreCase) -ge 0)
            } |
            Select-Object -First 4

        foreach ($file in $files) {
            $matches.Add(($file.FullName.Substring($repoRoot.Length).TrimStart("\", "/") -replace "\\", "/")) | Out-Null
        }
    }

    if ($matches.Count -eq 0) {
        return "none"
    }

    return $matches -join "; "
}

Write-Host "P7-D PLATEAU loaded category inspection"
Write-Host ("{0,-26} {1,-18} {2,-18} {3,-34} {4}" -f "category", "target", "source", "unity evidence", "status")

foreach ($category in $categories) {
    $sourcePath = Join-Path $udxRoot $category.SourceFolder
    $sourceEvidence = if (Test-Path -LiteralPath $sourcePath -PathType Container) { "source:$($category.SourceFolder)" } else { "missing:$($category.SourceFolder)" }
    $rootExists = $sceneText.IndexOf("m_Name: $($category.SceneRoot)", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    $unityEvidence = Get-UnityEvidence -Category $category.Category -SourceFolder $category.SourceFolder
    $status = "BLOCKED_pending_manual_import"

    if ($unityEvidence -ne "none" -and $rootExists) {
        $status = "loaded_candidate_needs_visual_profiler_check"
    }
    else {
        $allLoaded = $false
    }

    Write-Host ("{0,-26} {1,-18} {2,-18} {3,-34} {4}" -f $category.Category, $category.TargetLod, $sourceEvidence, $unityEvidence, $status)
}

Write-Host ""
if ($allLoaded) {
    Write-Host "Average LOD3 achieved: NOT CLAIMED. Scene evidence still requires LOD/object inspection."
}
else {
    Write-Host "Average LOD3 achieved: FALSE. Current scene has target roots only; renderable category evidence is missing."
}

Write-Host "P7-D PLATEAU loaded category inspection: PASS_WITH_IMPORT_BLOCKER_STATUS" -ForegroundColor Green
exit 0
