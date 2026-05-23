[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

function Get-P7DCategoryDefinitions {
    return @(
        [pscustomobject]@{ Key = "Buildings"; Prefixes = @("bldg"); SourceFolder = "bldg"; TargetLod = "LOD3"; RootName = "Buildings" },
        [pscustomobject]@{ Key = "Roads"; Prefixes = @("tran"); SourceFolder = "tran"; TargetLod = "LOD3"; RootName = "Roads" },
        [pscustomobject]@{ Key = "Bridges"; Prefixes = @("brid"); SourceFolder = "brid"; TargetLod = "LOD3"; RootName = "Bridges" },
        [pscustomobject]@{ Key = "Underground"; Prefixes = @("ubld", "tun"); SourceFolder = "ubld"; TargetLod = "LOD3 if available"; RootName = "Underground" },
        [pscustomobject]@{ Key = "CityFurniture"; Prefixes = @("frn"); SourceFolder = "frn"; TargetLod = "LOD2/LOD3"; RootName = "CityFurniture" },
        [pscustomobject]@{ Key = "Water"; Prefixes = @("wtr"); SourceFolder = "wtr"; TargetLod = "LOD1"; RootName = "Water" },
        [pscustomobject]@{ Key = "Vegetation"; Prefixes = @("veg"); SourceFolder = "veg"; TargetLod = "LOD3 if available"; RootName = "Vegetation" },
        [pscustomobject]@{ Key = "Relief"; Prefixes = @("dem"); SourceFolder = "dem"; TargetLod = "import terrain"; RootName = "Relief" },
        [pscustomobject]@{ Key = "DisasterRisk"; Prefixes = @("fld"); SourceFolder = "fld"; TargetLod = "import"; RootName = "DisasterRisk" },
        [pscustomobject]@{ Key = "LandUse"; Prefixes = @("luse"); SourceFolder = "luse"; TargetLod = "import"; RootName = "LandUse" },
        [pscustomobject]@{ Key = "UrbanPlanningDecision"; Prefixes = @("urf"); SourceFolder = "urf"; TargetLod = "LOD1"; RootName = "UrbanPlanningDecision" }
    )
}

function New-P7DCategoryStats {
    param([pscustomobject]$Definition)

    return [pscustomobject]@{
        Category = $Definition.Key
        Prefixes = $Definition.Prefixes
        SourceFolder = $Definition.SourceFolder
        TargetLod = $Definition.TargetLod
        RootName = $Definition.RootName
        ExplicitRootSeen = $false
        GameObjectNameHits = 0
        MeshAssetNameHits = 0
        GmlRootNameHits = 0
        CityObjectGroupHits = 0
        MeshRendererHits = 0
        MeshFilterHits = 0
        MeshColliderHits = 0
        LodGroupHits = 0
        Lods = [ordered]@{}
        SampleNames = New-Object System.Collections.Generic.List[string]
        SampleGmlRoots = New-Object System.Collections.Generic.List[string]
    }
}

function Add-P7DSample {
    param(
        [System.Collections.Generic.List[string]]$List,
        [string]$Value,
        [int]$Limit = 4
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return
    }

    if ($List.Count -ge $Limit) {
        return
    }

    if (-not $List.Contains($Value)) {
        $List.Add($Value) | Out-Null
    }
}

function Get-P7DCategoryStatus {
    param([pscustomobject]$Stats)

    if ($Stats.CityObjectGroupHits -gt 0 -and $Stats.MeshRendererHits -gt 0 -and $Stats.MeshFilterHits -gt 0) {
        $hasLod3 = $Stats.Lods.Contains("3")
        if ($Stats.TargetLod.IndexOf("LOD3", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -and -not $hasLod3) {
            return "PARTIAL_RENDERABLE_LOD_MISMATCH"
        }

        return "RENDERABLE_PRESENT"
    }

    if ($Stats.CityObjectGroupHits -gt 0) {
        return "METADATA_ONLY_OR_NON_RENDERABLE"
    }

    if ($Stats.GameObjectNameHits -gt 0 -or $Stats.GmlRootNameHits -gt 0) {
        return "PARTIAL_RAW_OR_UNCONVERTED"
    }

    return "MISSING"
}

function Get-P7DDetectedLodRange {
    param([pscustomobject]$Stats)

    if ($Stats.Lods.Count -eq 0) {
        return "none"
    }

    $lodKeys = @($Stats.Lods.Keys | Sort-Object { [int]$_ })
    $min = $lodKeys[0]
    $max = $lodKeys[$lodKeys.Count - 1]

    if ($min -eq $max) {
        return "LOD$min"
    }

    return "LOD$min-LOD$max"
}

function Get-P7DLodSummary {
    param([pscustomobject]$Stats)

    if ($Stats.Lods.Count -eq 0) {
        return "none"
    }

    return (@($Stats.Lods.Keys | Sort-Object { [int]$_ } | ForEach-Object { "LOD$_=$($Stats.Lods[$_])" }) -join ", ")
}

function Get-P7DRenderableEvidence {
    param([pscustomobject]$Stats)

    if ($Stats.MeshRendererHits -gt 0 -and $Stats.MeshFilterHits -gt 0) {
        return "MeshRenderer=$($Stats.MeshRendererHits); MeshFilter=$($Stats.MeshFilterHits)"
    }

    if ($Stats.MeshRendererHits -gt 0 -or $Stats.MeshFilterHits -gt 0) {
        return "partial MeshRenderer=$($Stats.MeshRendererHits); MeshFilter=$($Stats.MeshFilterHits)"
    }

    return "none"
}

function Get-P7DFlatOrAttributeEvidence {
    param([pscustomobject]$Stats)

    if ($Stats.CityObjectGroupHits -gt 0 -and $Stats.MeshRendererHits -eq 0 -and $Stats.MeshFilterHits -eq 0) {
        return "PLATEAU metadata present without renderable mesh evidence"
    }

    if ($Stats.Category -eq "Roads" -and $Stats.CityObjectGroupHits -gt 0 -and -not $Stats.Lods.Contains("2") -and -not $Stats.Lods.Contains("3")) {
        return "low-detail transport mesh only; no LOD2/LOD3 road geometry evidence"
    }

    if ($Stats.Category -eq "Bridges" -and $Stats.CityObjectGroupHits -gt 0 -and -not $Stats.Lods.Contains("2") -and -not $Stats.Lods.Contains("3")) {
        return "low-detail bridge mesh only; no LOD2/LOD3 bridge geometry evidence"
    }

    return "none detected from scene text"
}

function Get-P7DCategoryNote {
    param([pscustomobject]$Stats)

    $status = Get-P7DCategoryStatus -Stats $Stats
    if ($status -eq "MISSING") {
        return "No converted scene object evidence detected for this category."
    }

    if ($status -eq "PARTIAL_RENDERABLE_LOD_MISMATCH") {
        return "Renderable Unity objects exist, but detected LOD is below the original target."
    }

    if ($status -eq "METADATA_ONLY_OR_NON_RENDERABLE") {
        return "PLATEAU metadata exists, but mesh renderer/filter evidence is absent."
    }

    if ($status -eq "PARTIAL_RAW_OR_UNCONVERTED") {
        return "Name/root evidence exists without full PLATEAU component and renderer evidence."
    }

    return "Renderable Unity evidence detected."
}

function Get-P7DSceneEvidence {
    param(
        [string]$RepoRoot,
        [string]$SceneRelativePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity",
        [string]$PlateauDataRoot = "D:\PLATEAU_DATA\Chuo_2025_CityGML"
    )

    if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
        $scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
        $RepoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
    }

    $scenePath = Join-Path $RepoRoot ($SceneRelativePath -replace "/", "\")
    $definitions = @(Get-P7DCategoryDefinitions)
    $statsByCategory = [ordered]@{}
    $prefixToCategory = @{}

    foreach ($definition in $definitions) {
        $stats = New-P7DCategoryStats -Definition $definition
        $statsByCategory[$definition.Key] = $stats
        foreach ($prefix in $definition.Prefixes) {
            $prefixToCategory[$prefix.ToLowerInvariant()] = $definition.Key
        }
    }

    $evidence = [pscustomobject]@{
        SceneRelativePath = $SceneRelativePath
        ScenePath = $scenePath
        SceneExists = Test-Path -LiteralPath $scenePath -PathType Leaf
        SceneBytes = 0
        SceneLastWriteTime = $null
        LinesScanned = 0
        MeshRendererCount = 0
        MeshFilterCount = 0
        MeshColliderCount = 0
        LodGroupCount = 0
        PlateauCityObjectGroupCount = 0
        TotalLodCounts = [ordered]@{}
        ForbiddenHits = New-Object System.Collections.Generic.List[string]
        Categories = $statsByCategory
        UnityOutputRoots = [ordered]@{}
        ChuoBaseMapTracked = $false
        ChuoBaseMapExists = $false
        AverageLod3Achieved = $false
    }

    $unityRoots = @(
        "Assets/P7HighDetail",
        "Assets/P7HighDetail/PLATEAU",
        "Assets/P7HighDetail/Imported",
        "Assets/PLATEAU",
        "Assets/Data"
    )

    foreach ($unityRoot in $unityRoots) {
        $fullUnityRoot = Join-Path $RepoRoot ($unityRoot -replace "/", "\")
        $evidence.UnityOutputRoots[$unityRoot] = Test-Path -LiteralPath $fullUnityRoot
    }

    $chuoBaseMapPath = Join-Path $RepoRoot "Assets\Scenes\Chuo_BaseMap.unity"
    $evidence.ChuoBaseMapExists = Test-Path -LiteralPath $chuoBaseMapPath -PathType Leaf

    try {
        $tracked = & git -C $RepoRoot ls-files "Assets/Scenes/Chuo_BaseMap.unity" 2>$null
        $evidence.ChuoBaseMapTracked = -not [string]::IsNullOrWhiteSpace(($tracked | Out-String))
    }
    catch {
        $evidence.ChuoBaseMapTracked = $false
    }

    if (-not $evidence.SceneExists) {
        return $evidence
    }

    $sceneItem = Get-Item -LiteralPath $scenePath
    $evidence.SceneBytes = $sceneItem.Length
    $evidence.SceneLastWriteTime = $sceneItem.LastWriteTime

    $currentDocumentKind = ""
    $currentCategoryKey = $null
    $rgPattern = '^(GameObject:|Mesh:|MeshRenderer:|MeshFilter:|MeshCollider:|LODGroup:|  m_Name: |  lod: )|PLATEAUCityObjectGroup|Assets/PLATEAU|Assets/Data|Chuo_BaseMap|LightCurtain|CrowdFailure|RealSpawn|IndoorEvacuation|P7-E|P7-F|P7-G'

    & rg --no-heading -n $rgPattern $scenePath | ForEach-Object {
        $rawLine = [string]$_
        $separatorIndex = $rawLine.IndexOf(":", [System.StringComparison]::Ordinal)
        if ($separatorIndex -lt 0) {
            return
        }

        $line = $rawLine.Substring($separatorIndex + 1)
        $evidence.LinesScanned++

        if ($line -eq "GameObject:") {
            $currentDocumentKind = "GameObject"
            $currentCategoryKey = $null
            return
        }

        if ($line -eq "Mesh:") {
            $currentDocumentKind = "Mesh"
            return
        }

        if ($line -eq "MeshRenderer:") {
            $evidence.MeshRendererCount++
            if ($null -ne $currentCategoryKey) {
                $statsByCategory[$currentCategoryKey].MeshRendererHits++
            }
            return
        }

        if ($line -eq "MeshFilter:") {
            $evidence.MeshFilterCount++
            if ($null -ne $currentCategoryKey) {
                $statsByCategory[$currentCategoryKey].MeshFilterHits++
            }
            return
        }

        if ($line -eq "MeshCollider:") {
            $evidence.MeshColliderCount++
            if ($null -ne $currentCategoryKey) {
                $statsByCategory[$currentCategoryKey].MeshColliderHits++
            }
            return
        }

        if ($line -eq "LODGroup:") {
            $evidence.LodGroupCount++
            if ($null -ne $currentCategoryKey) {
                $statsByCategory[$currentCategoryKey].LodGroupHits++
            }
            return
        }

        if ($line.StartsWith("  m_Name: ", [System.StringComparison]::Ordinal)) {
            $name = $line.Substring(10).Trim()

            foreach ($definition in $definitions) {
                if ($name.Equals($definition.RootName, [System.StringComparison]::OrdinalIgnoreCase)) {
                    $statsByCategory[$definition.Key].ExplicitRootSeen = $true
                }
            }

            if ($name.EndsWith(".gml", [System.StringComparison]::OrdinalIgnoreCase)) {
                $parts = $name.Split("_")
                if ($parts.Count -gt 1) {
                    $gmlPrefix = $parts[1].ToLowerInvariant()
                    if ($prefixToCategory.ContainsKey($gmlPrefix)) {
                        $gmlCategory = $prefixToCategory[$gmlPrefix]
                        $statsByCategory[$gmlCategory].GmlRootNameHits++
                        Add-P7DSample -List $statsByCategory[$gmlCategory].SampleGmlRoots -Value $name
                    }
                }
            }

            $underscore = $name.IndexOf("_", [System.StringComparison]::Ordinal)
            if ($underscore -gt 0) {
                $prefix = $name.Substring(0, $underscore).ToLowerInvariant()
                if ($prefixToCategory.ContainsKey($prefix)) {
                    $categoryKey = $prefixToCategory[$prefix]
                    $stats = $statsByCategory[$categoryKey]

                    if ($currentDocumentKind -eq "GameObject") {
                        $currentCategoryKey = $categoryKey
                        $stats.GameObjectNameHits++
                        Add-P7DSample -List $stats.SampleNames -Value $name
                    }
                    elseif ($currentDocumentKind -eq "Mesh") {
                        $stats.MeshAssetNameHits++
                    }
                }
            }

            return
        }

        if ($line.IndexOf("PLATEAUCityObjectGroup", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $evidence.PlateauCityObjectGroupCount++
            if ($null -ne $currentCategoryKey) {
                $statsByCategory[$currentCategoryKey].CityObjectGroupHits++
            }
            return
        }

        if ($line.StartsWith("  lod: ", [System.StringComparison]::Ordinal)) {
            $lodValue = $line.Substring(7).Trim()
            if (-not $evidence.TotalLodCounts.Contains($lodValue)) {
                $evidence.TotalLodCounts[$lodValue] = 0
            }
            $evidence.TotalLodCounts[$lodValue]++

            if ($null -ne $currentCategoryKey) {
                $stats = $statsByCategory[$currentCategoryKey]
                if (-not $stats.Lods.Contains($lodValue)) {
                    $stats.Lods[$lodValue] = 0
                }
                $stats.Lods[$lodValue]++
            }

            return
        }

        foreach ($fragment in @("Assets/PLATEAU", "Assets/Data", "Chuo_BaseMap", "LightCurtain", "CrowdFailure", "RealSpawn", "IndoorEvacuation", "P7-E", "P7-F", "P7-G")) {
            if ($line.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
                Add-P7DSample -List $evidence.ForbiddenHits -Value $fragment -Limit 20
            }
        }
    }

    $lodKeys = @($evidence.TotalLodCounts.Keys | Sort-Object { [int]$_ })
    $evidence.AverageLod3Achieved = $lodKeys.Count -gt 0 -and ([int]$lodKeys[0]) -ge 3

    foreach ($definition in $definitions) {
        $sourcePath = Join-Path (Join-Path $PlateauDataRoot "udx") $definition.SourceFolder
        $statsByCategory[$definition.Key] | Add-Member -NotePropertyName SourceFolderExists -NotePropertyValue (Test-Path -LiteralPath $sourcePath -PathType Container) -Force
    }

    return $evidence
}

function Write-P7DCategoryTable {
    param([pscustomobject]$Evidence)

    Write-Host ("{0,-24} {1,-18} {2,-12} {3,8} {4,8} {5,8} {6,8} {7,-32} {8}" -f "category", "target", "actual", "objects", "renderer", "filter", "plateau", "status", "notes")
    foreach ($categoryName in $Evidence.Categories.Keys) {
        $stats = $Evidence.Categories[$categoryName]
        $range = Get-P7DDetectedLodRange -Stats $stats
        $status = Get-P7DCategoryStatus -Stats $stats
        $note = Get-P7DCategoryNote -Stats $stats
        Write-Host ("{0,-24} {1,-18} {2,-12} {3,8} {4,8} {5,8} {6,8} {7,-32} {8}" -f $stats.Category, $stats.TargetLod, $range, $stats.GameObjectNameHits, $stats.MeshRendererHits, $stats.MeshFilterHits, $stats.CityObjectGroupHits, $status, $note)
    }
}
