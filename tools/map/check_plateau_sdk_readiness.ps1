param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path,
    [switch]$AsJson,
    [switch]$NoExitCode
)

$ErrorActionPreference = "Stop"

function Resolve-FullPath {
    param([string]$Path)

    if (Test-Path -LiteralPath $Path) {
        return (Resolve-Path -LiteralPath $Path).Path
    }

    return [System.IO.Path]::GetFullPath($Path)
}

function Add-ReadinessCheck {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Detail,
        [string]$Severity = "Error"
    )

    $script:Checks += [pscustomobject]@{
        Name = $Name
        Passed = $Passed
        Severity = $Severity
        Detail = $Detail
    }
}

$script:Checks = @()
$plateauPackageName = "com.synesthesias.plateau-unity-sdk"
$requiredSceneRoots = @(
    "MapRoot",
    "RuntimeSystemsRoot",
    "PlayerSpawnRoot",
    "ShelterMarkerRoot",
    "CandidateMarkerRoot",
    "HazardVisualRoot",
    "NavigationRoot",
    "CrowdRoot",
    "CollapseDebrisRoot",
    "GreenFrameRoot",
    "UIAnchorRoot",
    "DebugDiagnosticsRoot",
    "PerformanceMetricsRoot"
)

$projectRootPath = Resolve-FullPath $ProjectRoot
$assetsPath = Join-Path $projectRootPath "Assets"
$packagesPath = Join-Path $projectRootPath "Packages"
$projectSettingsPath = Join-Path $projectRootPath "ProjectSettings"
$projectVersionPath = Join-Path $projectSettingsPath "ProjectVersion.txt"
$manifestPath = Join-Path $packagesPath "manifest.json"
$lockPath = Join-Path $packagesPath "packages-lock.json"
$scenePath = Join-Path $assetsPath "Scenes\Chuo_BaseMap.unity"
$sceneMetaPath = Join-Path $assetsPath "Scenes\Chuo_BaseMap.unity.meta"

Add-ReadinessCheck "Assets folder" (Test-Path -LiteralPath $assetsPath -PathType Container) $assetsPath
Add-ReadinessCheck "Packages folder" (Test-Path -LiteralPath $packagesPath -PathType Container) $packagesPath
Add-ReadinessCheck "ProjectSettings folder" (Test-Path -LiteralPath $projectSettingsPath -PathType Container) $projectSettingsPath

$unityVersion = $null
if (Test-Path -LiteralPath $projectVersionPath -PathType Leaf) {
    $versionLine = Select-String -LiteralPath $projectVersionPath -Pattern "^m_EditorVersion:" | Select-Object -First 1
    if ($versionLine) {
        $unityVersion = $versionLine.Line -replace "^m_EditorVersion:\s*", ""
    }
    Add-ReadinessCheck "Unity version file" $true $projectVersionPath
} else {
    Add-ReadinessCheck "Unity version file" $false "Missing ProjectSettings/ProjectVersion.txt"
}

$manifestValue = $null
$manifest = $null
if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $manifestProperty = $manifest.dependencies.PSObject.Properties[$plateauPackageName]
    if ($manifestProperty) {
        $manifestValue = [string]$manifestProperty.Value
        Add-ReadinessCheck "PLATEAU SDK manifest dependency" $true $manifestValue
    } else {
        Add-ReadinessCheck "PLATEAU SDK manifest dependency" $false "Missing $plateauPackageName in Packages/manifest.json"
    }
} else {
    Add-ReadinessCheck "Package manifest" $false "Missing Packages/manifest.json"
}

$localPackagePath = $null
$localPackageExists = $false
if ($manifestValue) {
    if ($manifestValue.StartsWith("file:", [System.StringComparison]::OrdinalIgnoreCase)) {
        $rawPackagePath = $manifestValue.Substring(5)
        if (-not [System.IO.Path]::IsPathRooted($rawPackagePath)) {
            $rawPackagePath = Join-Path (Split-Path -Parent $manifestPath) $rawPackagePath
        }

        $localPackagePath = [System.IO.Path]::GetFullPath($rawPackagePath)
        $localPackageExists = Test-Path -LiteralPath $localPackagePath -PathType Leaf
        Add-ReadinessCheck "PLATEAU SDK local tarball" $localPackageExists $localPackagePath
    } else {
        Add-ReadinessCheck "PLATEAU SDK local tarball" $true "Package source is not a local file path: $manifestValue" "Warning"
    }
}

$lockVersion = $null
$lockSource = $null
if (Test-Path -LiteralPath $lockPath -PathType Leaf) {
    $lock = Get-Content -LiteralPath $lockPath -Raw | ConvertFrom-Json
    $lockProperty = $lock.dependencies.PSObject.Properties[$plateauPackageName]
    if ($lockProperty) {
        $lockVersion = [string]$lockProperty.Value.version
        $lockSource = [string]$lockProperty.Value.source
        Add-ReadinessCheck "PLATEAU SDK package lock" $true "$lockSource $lockVersion"
    } else {
        Add-ReadinessCheck "PLATEAU SDK package lock" $false "Missing $plateauPackageName in Packages/packages-lock.json"
    }
} else {
    Add-ReadinessCheck "Package lock" $false "Missing Packages/packages-lock.json"
}

$sceneExists = Test-Path -LiteralPath $scenePath -PathType Leaf
$sceneMetaExists = Test-Path -LiteralPath $sceneMetaPath -PathType Leaf
Add-ReadinessCheck "Chuo_BaseMap scene file" $sceneExists $scenePath
Add-ReadinessCheck "Chuo_BaseMap scene meta" $sceneMetaExists $sceneMetaPath "Warning"

$presentSceneRoots = @()
$missingSceneRoots = @()
if ($sceneExists) {
    $rootHits = @{}
    foreach ($root in $requiredSceneRoots) {
        $rootHits[$root] = $false
    }

    $reader = [System.IO.StreamReader]::new($scenePath)
    try {
        while (($line = $reader.ReadLine()) -ne $null) {
            if (-not $line.StartsWith("  m_Name: ")) {
                continue
            }

            $name = $line.Substring(10).Trim()
            if ($rootHits.ContainsKey($name)) {
                $rootHits[$name] = $true
            }
        }
    }
    finally {
        $reader.Close()
    }

    foreach ($root in $requiredSceneRoots) {
        if ($rootHits[$root]) {
            $presentSceneRoots += $root
        } else {
            $missingSceneRoots += $root
        }
    }

    Add-ReadinessCheck "Chuo_BaseMap baseline roots" ($missingSceneRoots.Count -eq 0) "Missing roots: $($missingSceneRoots -join ', ')"
}

$errorChecks = @($script:Checks | Where-Object { $_.Severity -eq "Error" -and -not $_.Passed })
$ready = ($errorChecks.Count -eq 0)
$unityProjectValid = (
    (Test-Path -LiteralPath $assetsPath -PathType Container) -and
    (Test-Path -LiteralPath $packagesPath -PathType Container) -and
    (Test-Path -LiteralPath $projectSettingsPath -PathType Container)
)

$result = [pscustomobject]@{
    ProjectRoot = $projectRootPath
    UnityProjectValid = $unityProjectValid
    UnityVersion = $unityVersion
    PlateauPackageName = $plateauPackageName
    PlateauManifestValue = $manifestValue
    PlateauLocalPackagePath = $localPackagePath
    PlateauLocalPackageExists = $localPackageExists
    PlateauLockVersion = $lockVersion
    PlateauLockSource = $lockSource
    ChuoBaseMapScenePath = $scenePath
    ChuoBaseMapSceneExists = $sceneExists
    ChuoBaseMapMetaExists = $sceneMetaExists
    RequiredSceneRoots = $requiredSceneRoots
    PresentSceneRoots = $presentSceneRoots
    MissingSceneRoots = $missingSceneRoots
    Ready = $ready
    Checks = $script:Checks
}

if ($AsJson) {
    $result | ConvertTo-Json -Depth 6
} else {
    Write-Host "PLATEAU SDK readiness"
    Write-Host "Project root: $projectRootPath"
    Write-Host "Unity version: $unityVersion"
    Write-Host "PLATEAU package: $manifestValue"
    foreach ($check in $script:Checks) {
        $label = if ($check.Passed) { "PASS" } else { "FAIL" }
        if ($check.Severity -eq "Warning" -and -not $check.Passed) {
            $label = "WARN"
        }
        Write-Host ("[{0}] {1}: {2}" -f $label, $check.Name, $check.Detail)
    }
    Write-Host "Ready: $ready"
}

if (-not $NoExitCode) {
    if ($ready) {
        exit 0
    }

    exit 1
}
