[CmdletBinding()]
param(
    [string]$PlateauDataRoot = "D:\PLATEAU_DATA\Chuo_2025_CityGML"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$manifestPath = Join-Path $repoRoot "Packages\manifest.json"
$unityImportRoots = @(
    "Assets/P7HighDetail/Imported",
    "Assets/P7HighDetail/PLATEAU"
)

$categories = @(
    @{ Category = "Buildings"; TargetLod = "LOD3"; SourceFolder = "bldg"; Notes = "Building import target is LOD3." },
    @{ Category = "Roads"; TargetLod = "LOD3"; SourceFolder = "tran"; Notes = "Road/transport import target is LOD3." },
    @{ Category = "Bridges"; TargetLod = "LOD3"; SourceFolder = "brid"; Notes = "Bridge import target is LOD3." },
    @{ Category = "Underground"; TargetLod = "LOD3 if available"; SourceFolder = "ubld"; Notes = "Underground city/street/space target is LOD3 if available." },
    @{ Category = "City furniture"; TargetLod = "LOD2 or LOD3"; SourceFolder = "frn"; Notes = "City furniture target depends on availability and performance." },
    @{ Category = "Water"; TargetLod = "LOD1"; SourceFolder = "wtr"; Notes = "Water body import target is LOD1." },
    @{ Category = "Vegetation"; TargetLod = "LOD3 if available"; SourceFolder = "veg"; Notes = "Vegetation target is LOD3 if available." },
    @{ Category = "Relief"; TargetLod = "import terrain relief"; SourceFolder = "dem"; Notes = "Relief/terrain should be imported as terrain evidence." },
    @{ Category = "Disaster risk"; TargetLod = "import"; SourceFolder = "fld"; Notes = "Disaster risk import is data evidence, not P8 hazard simulation." },
    @{ Category = "Land use"; TargetLod = "import"; SourceFolder = "luse"; Notes = "Land use category should be imported when feasible." },
    @{ Category = "Urban planning decision"; TargetLod = "LOD1"; SourceFolder = "urf"; Notes = "Urban planning decision target is LOD1." }
)

$renderableExtensions = @(".prefab", ".fbx", ".obj", ".dae", ".gltf", ".glb", ".asset", ".mesh")

function Get-UnityEvidence {
    param(
        [string]$Category,
        [string]$SourceFolder
    )

    $matches = New-Object System.Collections.Generic.List[string]

    foreach ($unityImportRoot in $unityImportRoots) {
        $fullRoot = Join-Path $repoRoot ($unityImportRoot -replace "/", "\")
        if (-not (Test-Path -LiteralPath $fullRoot -PathType Container)) {
            continue
        }

        $files = Get-ChildItem -LiteralPath $fullRoot -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object {
                $extension = $_.Extension.ToLowerInvariant()
                $renderableExtensions -contains $extension -and
                (
                    $_.FullName.IndexOf($SourceFolder, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
                    $_.FullName.IndexOf($Category, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
                )
            } |
            Select-Object -First 5

        foreach ($file in $files) {
            $relative = $file.FullName.Substring($repoRoot.Length).TrimStart("\", "/") -replace "\\", "/"
            $matches.Add($relative) | Out-Null
        }
    }

    if ($matches.Count -eq 0) {
        return "none"
    }

    return ($matches -join "; ")
}

Write-Host "P7-C PLATEAU import readiness inspection"
Write-Host "Project: $repoRoot"
Write-Host "PLATEAU source root: $PlateauDataRoot"

if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
    $manifest = Get-Content -Raw -LiteralPath $manifestPath
    if ($manifest.IndexOf("com.synesthesias.plateau-unity-sdk", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Write-Host "PLATEAU SDK package reference: detected in Packages/manifest.json"
    }
    else {
        Write-Host "WARN: PLATEAU SDK package reference was not detected in Packages/manifest.json"
    }
}
else {
    Write-Host "WARN: Packages/manifest.json was not found."
}

$udxRoot = Join-Path $PlateauDataRoot "udx"
Write-Host ""
Write-Host ("{0,-28} {1,-18} {2,-28} {3,-42} {4}" -f "category", "target LOD", "source evidence", "actual Unity evidence", "status")

$allRenderableVerified = $true
foreach ($category in $categories) {
    $sourcePath = Join-Path $udxRoot $category.SourceFolder
    $sourceEvidence = if (Test-Path -LiteralPath $sourcePath -PathType Container) { "source:$($category.SourceFolder)" } else { "missing source:$($category.SourceFolder)" }
    $unityEvidence = Get-UnityEvidence -Category $category.Category -SourceFolder $category.SourceFolder
    $status = "target_only_pending_unity_import"

    if ($unityEvidence -ne "none") {
        $status = "unity_renderable_candidate_detected_needs_visual_check"
    }
    else {
        $allRenderableVerified = $false
    }

    Write-Host ("{0,-28} {1,-18} {2,-28} {3,-42} {4}" -f $category.Category, $category.TargetLod, $sourceEvidence, $unityEvidence, $status)
    Write-Host ("  notes: {0}" -f $category.Notes)
}

Write-Host ""
if ($allRenderableVerified) {
    Write-Host "Average LOD3 achieved: NOT CLAIMED. Renderable candidates require scene evidence and visual/profiler verification."
}
else {
    Write-Host "Average LOD3 achieved: FALSE. Current evidence is target settings plus local source folders; high-detail Unity renderable scene import remains pending."
}

Write-Host "Screenshots/settings are treated as target settings, not completion evidence."
Write-Host "P7-C PLATEAU import readiness inspection: PASS_WITH_PENDING_IMPORT_EVIDENCE" -ForegroundColor Green
exit 0
