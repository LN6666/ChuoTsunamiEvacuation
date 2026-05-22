[CmdletBinding()]
param(
    [string]$ProjectRoot = "",
    [string[]]$AdditionalRootPaths = @(),
    [switch]$SkipExternalPlateauSource,
    [int64]$LargeFileThresholdBytes = 52428800,
    [int]$TopCount = 30
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
}
else {
    $ProjectRoot = (Resolve-Path $ProjectRoot).ProviderPath
}

function ConvertTo-ForwardSlashPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ""
    }

    return ($Path -replace "\\", "/")
}

function ConvertTo-RelativePath {
    param(
        [string]$BasePath,
        [string]$FullPath
    )

    $baseFullPath = [System.IO.Path]::GetFullPath($BasePath).TrimEnd([char[]]@("\", "/"))
    $targetFullPath = [System.IO.Path]::GetFullPath($FullPath)
    $baseUri = New-Object System.Uri (($baseFullPath + [System.IO.Path]::DirectorySeparatorChar))
    $targetUri = New-Object System.Uri $targetFullPath
    $relativeUri = $baseUri.MakeRelativeUri($targetUri)
    $relativePath = [System.Uri]::UnescapeDataString($relativeUri.ToString())
    return ConvertTo-ForwardSlashPath $relativePath
}

function Get-GitText {
    param([string[]]$GitArgs)

    $previousErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = "Continue"
        $output = & git @GitArgs 2>$null
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($null -eq $output) {
        return ""
    }

    return (($output | Select-Object -First 1) -as [string]).Trim()
}

function Add-CountBytes {
    param(
        [hashtable]$Map,
        [string]$Key,
        [int64]$Bytes
    )

    if ([string]::IsNullOrWhiteSpace($Key)) {
        $Key = "(unknown)"
    }

    if (-not $Map.ContainsKey($Key)) {
        $Map[$Key] = @{
            name = $Key
            fileCount = 0
            totalBytes = [int64]0
        }
    }

    $Map[$Key]["fileCount"] = [int]$Map[$Key]["fileCount"] + 1
    $Map[$Key]["totalBytes"] = [int64]$Map[$Key]["totalBytes"] + $Bytes
}

function Convert-MapToSummary {
    param([hashtable]$Map)

    return @(
        $Map.Keys |
            ForEach-Object {
                $entry = $Map[$_]
                [PSCustomObject]@{
                    name = [string]$entry["name"]
                    fileCount = [int]$entry["fileCount"]
                    totalBytes = [int64]$entry["totalBytes"]
                }
            } |
            Sort-Object -Property @{ Expression = { $_.fileCount }; Descending = $true }, @{ Expression = { $_.totalBytes }; Descending = $true }, "name"
    )
}

function Add-Sample {
    param(
        [System.Collections.Generic.List[object]]$List,
        [object]$Value,
        [int]$Limit
    )

    if ($List.Count -lt $Limit) {
        $List.Add($Value) | Out-Null
    }
}

function Add-SetValue {
    param(
        [hashtable]$Map,
        [string]$Key
    )

    if ([string]::IsNullOrWhiteSpace($Key)) {
        return
    }

    $Map[$Key] = $true
}

function Get-LodIndicators {
    param([string]$PathText)

    $lods = New-Object System.Collections.Generic.List[string]
    $matches = [System.Text.RegularExpressions.Regex]::Matches($PathText, "(?i)lod[\s_-]*([1-4])")
    foreach ($match in $matches) {
        $lod = "LOD$($match.Groups[1].Value)"
        if (-not $lods.Contains($lod)) {
            $lods.Add($lod) | Out-Null
        }
    }

    if ($lods.Count -eq 0) {
        $lods.Add("unknown") | Out-Null
    }

    return @($lods)
}

function Get-PlateauCategory {
    param([string]$PathText)

    $normalized = (ConvertTo-ForwardSlashPath $PathText).ToLowerInvariant()

    $categoryRules = @(
        @{ token = "/bldg/"; category = "Buildings" },
        @{ token = "_bldg_"; category = "Buildings" },
        @{ token = "/ubld/"; category = "Underground buildings" },
        @{ token = "_ubld_"; category = "Underground buildings" },
        @{ token = "/tran/"; category = "Roads/transportation" },
        @{ token = "_tran_"; category = "Roads/transportation" },
        @{ token = "/brid/"; category = "Bridges" },
        @{ token = "_brid_"; category = "Bridges" },
        @{ token = "/tun/"; category = "Tunnels" },
        @{ token = "_tun_"; category = "Tunnels" },
        @{ token = "/urf/"; category = "Urban facilities" },
        @{ token = "_urf_"; category = "Urban facilities" },
        @{ token = "/fld/"; category = "Flood or risk layers" },
        @{ token = "_fld_"; category = "Flood or risk layers" },
        @{ token = "/veg/"; category = "Vegetation" },
        @{ token = "_veg_"; category = "Vegetation" },
        @{ token = "/luse/"; category = "Land use" },
        @{ token = "_luse_"; category = "Land use" },
        @{ token = "/dem/"; category = "Terrain/elevation" },
        @{ token = "_dem_"; category = "Terrain/elevation" },
        @{ token = "/frn/"; category = "City furniture" },
        @{ token = "_frn_"; category = "City furniture" },
        @{ token = "/wtr/"; category = "Water" },
        @{ token = "_wtr_"; category = "Water" },
        @{ token = "/htd/"; category = "Hazard/tide data" },
        @{ token = "_htd_"; category = "Hazard/tide data" },
        @{ token = "/area/"; category = "Administrative area" },
        @{ token = "_area_"; category = "Administrative area" }
    )

    foreach ($rule in $categoryRules) {
        if ($normalized.Contains($rule.token)) {
            return $rule.category
        }
    }

    if ($normalized.Contains("plateau") -or $normalized.Contains("citygml")) {
        return "PLATEAU related"
    }

    return "unknown"
}

function Get-KeywordTags {
    param([string]$PathText)

    $normalized = (ConvertTo-ForwardSlashPath $PathText).ToLowerInvariant()
    $tags = New-Object System.Collections.Generic.List[string]

    if ($normalized -match "(^|[/_\\.-])(brid|bridge)([/_\\.-]|$)") {
        $tags.Add("bridge") | Out-Null
    }

    if ($normalized -match "(^|[/_\\.-])(ubld|underground|subway|metro|station|tun|tunnel|basement|chika)([/_\\.-]|$)") {
        $tags.Add("underground") | Out-Null
    }

    if ($normalized -match "(^|[/_\\.-])(tran|road|street|sidewalk|walkway|highway)([/_\\.-]|$)") {
        $tags.Add("road") | Out-Null
    }

    if ($normalized -match "(^|[/_\\.-])(wtr|water|river|canal|waterfront|riverfront|sumida|harumi|kachidoki|tsukishima|tsukuda)([/_\\.-]|$)") {
        $tags.Add("riverfront") | Out-Null
    }

    return @($tags)
}

function Get-CandidateFolderKey {
    param([string]$RelativePath)

    $directory = ConvertTo-ForwardSlashPath (Split-Path -Parent $RelativePath)
    if ([string]::IsNullOrWhiteSpace($directory)) {
        return "(root)"
    }

    $parts = @($directory -split "/" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($parts.Count -eq 0) {
        return "(root)"
    }

    if ($parts[0].Equals("udx", [System.StringComparison]::OrdinalIgnoreCase) -and $parts.Count -ge 2) {
        return "udx/$($parts[1])"
    }

    if ($parts.Count -ge 2) {
        return "$($parts[0])/$($parts[1])"
    }

    return $parts[0]
}

function Get-MeshCode {
    param([string]$PathText)

    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($PathText)
    $match = [System.Text.RegularExpressions.Regex]::Match($fileName, "^(?<mesh>\d{6,8})(?:_|$)")
    if ($match.Success) {
        return $match.Groups["mesh"].Value
    }

    return ""
}

function Get-RootLabel {
    param(
        [string]$RootPath,
        [string]$ProjectRootPath
    )

    $fullRoot = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([char[]]@("\", "/"))
    $fullProject = [System.IO.Path]::GetFullPath($ProjectRootPath).TrimEnd([char[]]@("\", "/"))
    $assetsPath = [System.IO.Path]::GetFullPath((Join-Path $ProjectRootPath "Assets")).TrimEnd([char[]]@("\", "/"))
    $externalPlateauPath = [System.IO.Path]::GetFullPath("D:\PLATEAU_DATA\Chuo_2025_CityGML").TrimEnd([char[]]@("\", "/"))

    if ($fullRoot.Equals($assetsPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        return "Assets"
    }

    if ($fullRoot.Equals($externalPlateauPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        return "Local PLATEAU source"
    }

    if ($fullRoot.StartsWith($fullProject, [System.StringComparison]::OrdinalIgnoreCase)) {
        return ConvertTo-ForwardSlashPath (ConvertTo-RelativePath -BasePath $ProjectRootPath -FullPath $RootPath)
    }

    return Split-Path -Leaf $RootPath
}

function New-FileRecord {
    param(
        [string]$SourceRootLabel,
        [string]$RelativePath,
        [string]$Extension,
        [int64]$FileSizeBytes,
        [string]$Category,
        [string[]]$Lods
    )

    [PSCustomObject]@{
        sourceRoot = $SourceRootLabel
        relativePath = $RelativePath
        extension = $Extension
        fileSizeBytes = $FileSizeBytes
        likelyPlateauCategory = $Category
        candidateLodLevel = (($Lods | Sort-Object -Unique) -join ", ")
    }
}

$projectAssetsRoot = Join-Path $ProjectRoot "Assets"
$rootsToScan = New-Object System.Collections.Generic.List[string]
if (Test-Path -LiteralPath $projectAssetsRoot -PathType Container) {
    $rootsToScan.Add((Resolve-Path $projectAssetsRoot).ProviderPath) | Out-Null
}

$externalPlateauRoot = "D:\PLATEAU_DATA\Chuo_2025_CityGML"
if (-not $SkipExternalPlateauSource -and (Test-Path -LiteralPath $externalPlateauRoot -PathType Container)) {
    $rootsToScan.Add((Resolve-Path $externalPlateauRoot).ProviderPath) | Out-Null
}

foreach ($additionalRootPath in $AdditionalRootPaths) {
    if ([string]::IsNullOrWhiteSpace($additionalRootPath)) {
        continue
    }

    if (Test-Path -LiteralPath $additionalRootPath -PathType Container) {
        $rootsToScan.Add((Resolve-Path $additionalRootPath).ProviderPath) | Out-Null
    }
}

$uniqueRoots = @(
    $rootsToScan |
        ForEach-Object { [System.IO.Path]::GetFullPath($_).TrimEnd([char[]]@("\", "/")) } |
        Sort-Object -Unique
)

$extensionMap = @{}
$categoryMap = @{}
$lodMap = @{}
$categoryLodMap = @{}
$meshCodeMap = @{}
$keywordMap = @{}
$candidateFolderMap = @{}

$largeFiles = New-Object System.Collections.Generic.List[object]
$modelFiles = New-Object System.Collections.Generic.List[object]
$textureFiles = New-Object System.Collections.Generic.List[object]
$materialPrefabFiles = New-Object System.Collections.Generic.List[object]
$plateauRelatedPaths = New-Object System.Collections.Generic.List[object]
$lodIndicatorPaths = New-Object System.Collections.Generic.List[object]
$bridgePaths = New-Object System.Collections.Generic.List[object]
$undergroundPaths = New-Object System.Collections.Generic.List[object]
$roadPaths = New-Object System.Collections.Generic.List[object]
$riverfrontPaths = New-Object System.Collections.Generic.List[object]
$rootSummaries = New-Object System.Collections.Generic.List[object]

$totalFileCount = 0
$totalBytes = [int64]0

$modelExtensions = @(".fbx", ".obj", ".dae", ".3ds", ".blend", ".gltf", ".glb", ".gml", ".citygml", ".mesh")
$textureExtensions = @(".png", ".jpg", ".jpeg", ".tga", ".tif", ".tiff", ".psd", ".bmp", ".exr", ".hdr")
$materialPrefabExtensions = @(".mat", ".prefab")

foreach ($rootPath in $uniqueRoots) {
    $rootLabel = Get-RootLabel -RootPath $rootPath -ProjectRootPath $ProjectRoot
    $rootFileCount = 0
    $rootBytes = [int64]0

    $files = Get-ChildItem -LiteralPath $rootPath -Recurse -File -Force -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        $rootFileCount++
        $totalFileCount++
        $rootBytes += [int64]$file.Length
        $totalBytes += [int64]$file.Length

        $relativePath = ConvertTo-RelativePath -BasePath $rootPath -FullPath $file.FullName
        $displayPath = if ($rootLabel -eq "Assets") { "Assets/$relativePath" } else { $relativePath }
        $extension = $file.Extension.ToLowerInvariant()
        if ([string]::IsNullOrWhiteSpace($extension)) {
            $extension = "(none)"
        }

        $pathForInference = "$rootLabel/$relativePath"
        $category = Get-PlateauCategory -PathText $pathForInference
        $lods = @(Get-LodIndicators -PathText $pathForInference)
        $keywordTags = @(Get-KeywordTags -PathText $pathForInference)
        $primaryLod = (($lods | Where-Object { $_ -ne "unknown" } | Select-Object -First 1) -as [string])
        if ([string]::IsNullOrWhiteSpace($primaryLod)) {
            $primaryLod = "unknown"
        }

        Add-CountBytes -Map $extensionMap -Key $extension -Bytes $file.Length
        Add-CountBytes -Map $categoryMap -Key $category -Bytes $file.Length
        foreach ($lod in $lods) {
            Add-CountBytes -Map $lodMap -Key $lod -Bytes $file.Length
            Add-CountBytes -Map $categoryLodMap -Key "$category | $lod" -Bytes $file.Length
        }
        foreach ($tag in $keywordTags) {
            Add-CountBytes -Map $keywordMap -Key $tag -Bytes $file.Length
        }

        $meshCode = Get-MeshCode -PathText $relativePath
        if (-not [string]::IsNullOrWhiteSpace($meshCode)) {
            Add-CountBytes -Map $meshCodeMap -Key $meshCode -Bytes $file.Length
        }

        $record = New-FileRecord -SourceRootLabel $rootLabel -RelativePath $displayPath -Extension $extension -FileSizeBytes $file.Length -Category $category -Lods $lods

        if ($file.Length -ge $LargeFileThresholdBytes) {
            $largeFiles.Add($record) | Out-Null
        }

        if ($modelExtensions -contains $extension) {
            Add-Sample -List $modelFiles -Value $record -Limit $TopCount
        }

        if ($textureExtensions -contains $extension) {
            Add-Sample -List $textureFiles -Value $record -Limit $TopCount
        }

        if ($materialPrefabExtensions -contains $extension) {
            Add-Sample -List $materialPrefabFiles -Value $record -Limit $TopCount
        }

        if ($category -ne "unknown" -or $pathForInference.IndexOf("plateau", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or $pathForInference.IndexOf("citygml", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Add-Sample -List $plateauRelatedPaths -Value $record -Limit ($TopCount * 2)
        }

        if (($lods | Where-Object { $_ -ne "unknown" }).Count -gt 0) {
            Add-Sample -List $lodIndicatorPaths -Value $record -Limit ($TopCount * 2)
        }

        if ($keywordTags -contains "bridge") {
            Add-Sample -List $bridgePaths -Value $record -Limit $TopCount
        }

        if ($keywordTags -contains "underground") {
            Add-Sample -List $undergroundPaths -Value $record -Limit $TopCount
        }

        if ($keywordTags -contains "road") {
            Add-Sample -List $roadPaths -Value $record -Limit $TopCount
        }

        if ($keywordTags -contains "riverfront") {
            Add-Sample -List $riverfrontPaths -Value $record -Limit $TopCount
        }

        $folderKey = Get-CandidateFolderKey -RelativePath $relativePath
        $candidateKey = "$rootLabel/$folderKey"
        if (-not $candidateFolderMap.ContainsKey($candidateKey)) {
            $candidateFolderMap[$candidateKey] = @{
                candidateFolder = $candidateKey
                sourceRoot = $rootLabel
                fileCount = 0
                totalBytes = [int64]0
                categories = @{}
                lods = @{}
                keywords = @{}
                meshCodes = @{}
                samplePaths = New-Object System.Collections.Generic.List[object]
                score = 0
            }
        }

        $candidate = $candidateFolderMap[$candidateKey]
        $candidate["fileCount"] = [int]$candidate["fileCount"] + 1
        $candidate["totalBytes"] = [int64]$candidate["totalBytes"] + [int64]$file.Length
        Add-SetValue -Map $candidate["categories"] -Key $category
        foreach ($lod in $lods) {
            if ($lod -ne "unknown") {
                Add-SetValue -Map $candidate["lods"] -Key $lod
            }
        }
        foreach ($tag in $keywordTags) {
            Add-SetValue -Map $candidate["keywords"] -Key $tag
        }
        if (-not [string]::IsNullOrWhiteSpace($meshCode)) {
            Add-SetValue -Map $candidate["meshCodes"] -Key $meshCode
        }
        Add-Sample -List $candidate["samplePaths"] -Value $displayPath -Limit 5

        $score = 0
        if (@("Buildings", "Roads/transportation", "Bridges", "Underground buildings", "Water") -contains $category) {
            $score += 3
        }
        if ($primaryLod -ne "unknown") {
            $score += 3
        }
        $score += ($keywordTags.Count * 2)
        if ($modelExtensions -contains $extension) {
            $score += 1
        }
        if ($file.Length -le 250MB) {
            $score += 1
        }
        $candidate["score"] = [int]$candidate["score"] + $score
    }

    $rootSummaries.Add([PSCustomObject]@{
        label = $rootLabel
        path = $rootPath
        exists = $true
        fileCount = $rootFileCount
        totalBytes = $rootBytes
    }) | Out-Null
}

$specialFolders = @(
    "Assets",
    "Assets/PLATEAU",
    "Assets/Scenes",
    "Assets/Data",
    "Assets/Materials",
    "Assets/Prefabs",
    "Assets/AddressableAssetsData",
    "Assets/Settings"
)

$specialFolderSummary = @(
    foreach ($folder in $specialFolders) {
        $fullPath = Join-Path $ProjectRoot ($folder -replace "/", "\")
        $exists = Test-Path -LiteralPath $fullPath -PathType Container
        $fileCount = 0
        $totalFolderBytes = [int64]0
        if ($exists) {
            $folderFiles = Get-ChildItem -LiteralPath $fullPath -Recurse -File -Force -ErrorAction SilentlyContinue
            foreach ($folderFile in $folderFiles) {
                $fileCount++
                $totalFolderBytes += [int64]$folderFile.Length
            }
        }

        $note = ""
        if ($folder -eq "Assets/Scenes") {
            $note = "Listing only; scenes are protected from modification."
        }
        elseif ($folder -eq "Assets/Data") {
            $note = "Listing only; existing data is protected from modification."
        }
        elseif ($folder -eq "Assets/PLATEAU") {
            $note = "Project PLATEAU import folder, if present; protected from modification."
        }

        [PSCustomObject]@{
            folder = $folder
            exists = $exists
            fileCount = $fileCount
            totalBytes = $totalFolderBytes
            note = $note
        }
    }
)

$candidateFolderList = New-Object System.Collections.Generic.List[object]
foreach ($candidateKey in $candidateFolderMap.Keys) {
    $candidate = $candidateFolderMap[$candidateKey]
    $categoryKeys = @($candidate["categories"].Keys | Sort-Object)
    $lodKeys = @($candidate["lods"].Keys | Sort-Object)
    $keywordKeys = @($candidate["keywords"].Keys | Sort-Object)
    $meshCodeKeys = @($candidate["meshCodes"].Keys | Sort-Object | Select-Object -First 8)
    $candidateLodLevelsText = "unknown"
    if ($lodKeys.Count -gt 0) {
        $candidateLodLevelsText = ($lodKeys -join ", ")
    }
    $keywordTagsText = ""
    if ($keywordKeys.Count -gt 0) {
        $keywordTagsText = ($keywordKeys -join ", ")
    }
    $meshCodesText = ""
    if ($meshCodeKeys.Count -gt 0) {
        $meshCodesText = ($meshCodeKeys -join ", ")
    }

    $candidateFolderList.Add([PSCustomObject]@{
        candidateFolder = [string]$candidate["candidateFolder"]
        sourceRoot = [string]$candidate["sourceRoot"]
        fileCount = [int]$candidate["fileCount"]
        totalBytes = [int64]$candidate["totalBytes"]
        likelyCategories = ($categoryKeys -join ", ")
        candidateLodLevels = $candidateLodLevelsText
        keywordTags = $keywordTagsText
        meshCodes = $meshCodesText
        samplePaths = @($candidate["samplePaths"].ToArray())
        score = [int]$candidate["score"]
    }) | Out-Null
}

$candidateFolders = @(
    $candidateFolderList |
        Where-Object { $_.score -gt 0 -or $_.likelyCategories -ne "unknown" } |
        Sort-Object -Property @{ Expression = { $_.score }; Descending = $true }, @{ Expression = { $_.totalBytes }; Descending = $true }, "candidateFolder" |
        Select-Object -First ($TopCount * 2)
)

$riskNotes = New-Object System.Collections.Generic.List[string]
if (-not (Test-Path -LiteralPath (Join-Path $ProjectRoot "Assets\PLATEAU") -PathType Container)) {
    $riskNotes.Add("Assets/PLATEAU is not present in the project tree; existing imported PLATEAU Unity assets were not found under the expected folder.") | Out-Null
}
if (-not (Test-Path -LiteralPath (Join-Path $ProjectRoot "Assets\Scenes\Chuo_BaseMap.unity") -PathType Leaf)) {
    $riskNotes.Add("Assets/Scenes/Chuo_BaseMap.unity is not present in this project checkout; the generated base map remains local-only or absent from this branch.") | Out-Null
}
if (($lodIndicatorPaths.Count) -eq 0) {
    $riskNotes.Add("No LOD1/LOD2/LOD3/LOD4 indicators were found in paths or filenames; this does not prove LOD absence because geometry contents were not parsed.") | Out-Null
}
if (($largeFiles.Count) -gt 0) {
    $riskNotes.Add("Large local source files exist and should be benchmarked in small areas before any broad import attempt.") | Out-Null
}

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz"
$runId = Get-Date -Format "yyyyMMdd_HHmmss"
$branch = Get-GitText @("branch", "--show-current")
if ([string]::IsNullOrWhiteSpace($branch)) {
    $branch = "(detached or unknown)"
}

$extensionSummary = @(Convert-MapToSummary -Map $extensionMap)
$categorySummary = @(Convert-MapToSummary -Map $categoryMap)
$lodSummary = @(Convert-MapToSummary -Map $lodMap)
$categoryLodSummary = @(Convert-MapToSummary -Map $categoryLodMap)
$meshCodeSummary = @(Convert-MapToSummary -Map $meshCodeMap | Select-Object -First ($TopCount * 2))
$keywordSummary = @(Convert-MapToSummary -Map $keywordMap)
$largeFileSummary = @($largeFiles.ToArray() | Sort-Object -Property @{ Expression = { $_.fileSizeBytes }; Descending = $true }, "relativePath" | Select-Object -First $TopCount)
$modelFileSummary = @($modelFiles.ToArray())
$textureFileSummary = @($textureFiles.ToArray())
$materialPrefabFileSummary = @($materialPrefabFiles.ToArray())
$plateauRelatedPathSummary = @($plateauRelatedPaths.ToArray())
$lodIndicatorPathSummary = @($lodIndicatorPaths.ToArray())
$bridgePathSummary = @($bridgePaths.ToArray())
$undergroundPathSummary = @($undergroundPaths.ToArray())
$roadPathSummary = @($roadPaths.ToArray())
$riverfrontPathSummary = @($riverfrontPaths.ToArray())
$rootSummaryArray = @($rootSummaries.ToArray())
$riskArray = @($riskNotes.ToArray())
$nextScanImprovementArray = @(
    "Add optional CityGML schema-aware LOD tag counting only after a separate approved plan.",
    "Add an optional CSV export only if a later prompt approves a generated inventory artifact path.",
    "Add Unity Editor import/object/material counts during P7-B benchmark work, not during P7-A read-only inventory.",
    "Add coordinate or mesh-code map visualization after a benchmark area is selected."
)

$scanResult = [PSCustomObject]@{
    schemaVersion = "p7a.assetInventory.scan.v1"
    inventoryRunId = $runId
    scanTimestamp = $timestamp
    branch = $branch
    projectPath = $ProjectRoot
    largeFileThresholdBytes = $LargeFileThresholdBytes
    inferencePolicy = "Path/name/file-metadata inference only. CityGML geometry and Unity import quality are not parsed or verified."
    roots = $rootSummaryArray
    specialFolderSummary = @($specialFolderSummary)
    totals = [PSCustomObject]@{
        fileCount = $totalFileCount
        totalBytes = $totalBytes
    }
    extensionSummary = $extensionSummary
    likelyPlateauCategorySummary = $categorySummary
    candidateLodSummary = $lodSummary
    categoryLodSummary = $categoryLodSummary
    meshCodeSummary = $meshCodeSummary
    keywordSummary = $keywordSummary
    largeFiles = $largeFileSummary
    likelyMeshModelFiles = $modelFileSummary
    likelyTextureFiles = $textureFileSummary
    likelyMaterialPrefabFiles = $materialPrefabFileSummary
    likelyPlateauRelatedPaths = $plateauRelatedPathSummary
    lodIndicatorPaths = $lodIndicatorPathSummary
    keywordHits = [PSCustomObject]@{
        bridge = $bridgePathSummary
        underground = $undergroundPathSummary
        road = $roadPathSummary
        riverfront = $riverfrontPathSummary
    }
    candidateFolders = @($candidateFolders)
    risks = $riskArray
    recommendedNextScanImprovements = $nextScanImprovementArray
}

$scanResult | ConvertTo-Json -Depth 12
