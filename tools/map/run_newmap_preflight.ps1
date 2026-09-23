param(
    [string]$ExpectedProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"

function Resolve-FullPath {
    param([string]$Path)

    if (Test-Path -LiteralPath $Path) {
        return (Resolve-Path -LiteralPath $Path).Path
    }

    return [System.IO.Path]::GetFullPath($Path)
}

function Normalize-PathForCompare {
    param([string]$Path)

    $trimChars = [char[]]@("\", "/")
    return ([System.IO.Path]::GetFullPath($Path)).TrimEnd($trimChars)
}

function Add-PreflightCheck {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Detail
    )

    $script:Checks += [pscustomobject]@{
        Name = $Name
        Passed = $Passed
        Detail = $Detail
    }
}

$script:Checks = @()
$expectedRootPath = Resolve-FullPath $ExpectedProjectRoot
$currentPath = (Resolve-Path -LiteralPath (Get-Location).Path).Path
$expectedCompare = Normalize-PathForCompare $expectedRootPath
$currentCompare = Normalize-PathForCompare $currentPath

Add-PreflightCheck "Current directory" ($currentCompare -ieq $expectedCompare) "Current=$currentPath Expected=$expectedRootPath"

$assetsPath = Join-Path $expectedRootPath "Assets"
$packagesPath = Join-Path $expectedRootPath "Packages"
$projectSettingsPath = Join-Path $expectedRootPath "ProjectSettings"
$projectVersionPath = Join-Path $projectSettingsPath "ProjectVersion.txt"
$readinessScript = Join-Path $expectedRootPath "tools\map\check_plateau_sdk_readiness.ps1"

Add-PreflightCheck "Assets folder" (Test-Path -LiteralPath $assetsPath -PathType Container) $assetsPath
Add-PreflightCheck "Packages folder" (Test-Path -LiteralPath $packagesPath -PathType Container) $packagesPath
Add-PreflightCheck "ProjectSettings folder" (Test-Path -LiteralPath $projectSettingsPath -PathType Container) $projectSettingsPath
Add-PreflightCheck "Unity version file" (Test-Path -LiteralPath $projectVersionPath -PathType Leaf) $projectVersionPath
Add-PreflightCheck "SDK readiness script" (Test-Path -LiteralPath $readinessScript -PathType Leaf) $readinessScript

$sdkStatus = $null
if (Test-Path -LiteralPath $readinessScript -PathType Leaf) {
    $sdkJson = & $readinessScript -ProjectRoot $expectedRootPath -AsJson -NoExitCode
    $sdkStatus = $sdkJson | ConvertFrom-Json

    Add-PreflightCheck "Unity project valid" ([bool]$sdkStatus.UnityProjectValid) $expectedRootPath
    Add-PreflightCheck "PLATEAU SDK ready" ([bool]$sdkStatus.Ready) $sdkStatus.PlateauManifestValue
    Add-PreflightCheck "Chuo_BaseMap scene exists" ([bool]$sdkStatus.ChuoBaseMapSceneExists) $sdkStatus.ChuoBaseMapScenePath
    Add-PreflightCheck "Chuo_BaseMap scene roots" (@($sdkStatus.MissingSceneRoots).Count -eq 0) "Missing roots: $(@($sdkStatus.MissingSceneRoots) -join ', ')"
}

$docsToCheck = @(
    "docs\NEWMAP_PROJECT_STATE_AUDIT.md",
    "docs\NEWMAP_PLATEAU_SDK_SETUP.md",
    "docs\NEWMAP_CHUO_BASEMAP_BASELINE.md"
)

foreach ($relativePath in $docsToCheck) {
    $fullPath = Join-Path $expectedRootPath $relativePath
    Add-PreflightCheck "Required doc $relativePath" (Test-Path -LiteralPath $fullPath -PathType Leaf) $fullPath
}

Write-Host "New map preflight"
Write-Host "Workspace: $expectedRootPath"
if ($sdkStatus) {
    Write-Host "Unity version: $($sdkStatus.UnityVersion)"
    Write-Host "PLATEAU SDK: $($sdkStatus.PlateauManifestValue)"
    Write-Host "Chuo_BaseMap: $($sdkStatus.ChuoBaseMapScenePath)"
}

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
