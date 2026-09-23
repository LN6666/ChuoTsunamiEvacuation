param(
    [string]$ProjectRoot = "D:\UnityProjects\ChuoTsunamiEvacuation"
)

$ErrorActionPreference = "Stop"

function Add-Check {
    param([string]$Name, [bool]$Passed, [string]$Detail)
    $script:Checks += [pscustomobject]@{ Name = $Name; Passed = $Passed; Detail = $Detail }
}

function Get-RelativeNameMatches {
    param(
        [string[]]$Roots,
        [string]$Pattern
    )

    $matches = @()
    foreach ($scanRoot in $Roots) {
        if (-not (Test-Path -LiteralPath $scanRoot)) {
            continue
        }

        $items = @(Get-ChildItem -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue)
        foreach ($item in $items) {
            if ($item.Name -match $Pattern) {
                $matches += $item.FullName
            }
        }
    }

    return $matches
}

$script:Checks = @()

$root = (Resolve-Path -LiteralPath $ProjectRoot).Path
$sourceScene = Join-Path $root "Assets\Scenes\Chuo_GroundRoad_Import_Source.unity"
$baseScene = Join-Path $root "Assets\Scenes\Chuo_BaseMap.unity"
$requiredRoots = @(
    "GroundRoadImportRoot",
    "ImportedRoadRoot",
    "ImportedTerrainRoot",
    "ImportedReliefRoot",
    "ImportedBridgeRoot",
    "ImportedWaterRoot",
    "ImportDiagnosticsRoot"
)

Add-Check "Workspace exists" (Test-Path -LiteralPath $root -PathType Container) $root
Add-Check "Ground/road import source scene exists" (Test-Path -LiteralPath $sourceScene -PathType Leaf) $sourceScene
Add-Check "Chuo_BaseMap scene still exists" (Test-Path -LiteralPath $baseScene -PathType Leaf) $baseScene

if ((Test-Path -LiteralPath $sourceScene -PathType Leaf) -and (Test-Path -LiteralPath $baseScene -PathType Leaf)) {
    $sourceInfo = Get-Item -LiteralPath $sourceScene
    $baseInfo = Get-Item -LiteralPath $baseScene

    Add-Check "Chuo_BaseMap not overwritten by source scene" ($baseInfo.Length -gt 1048576 -and $baseInfo.Length -gt $sourceInfo.Length) ("baseBytes={0} sourceBytes={1}" -f $baseInfo.Length, $sourceInfo.Length)

    $sceneText = Get-Content -LiteralPath $sourceScene -Raw
    foreach ($requiredRoot in $requiredRoots) {
        Add-Check "Root exists: $requiredRoot" ($sceneText -match "(?m)^\s*m_Name:\s*$([regex]::Escape($requiredRoot))\s*$") $requiredRoot
    }

    $actualRootNames = @([regex]::Matches($sceneText, "(?m)^\s*m_Name:\s*(.+?)\s*$") | ForEach-Object { $_.Groups[1].Value } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $extraRoots = @($actualRootNames | Where-Object { $requiredRoots -notcontains $_ })
    $missingRoots = @($requiredRoots | Where-Object { $actualRootNames -notcontains $_ })
    Add-Check "Source scene contains only initial import roots" ($extraRoots.Count -eq 0 -and $missingRoots.Count -eq 0 -and $actualRootNames.Count -eq $requiredRoots.Count) ("roots={0}" -f ($actualRootNames -join ", "))
}

$rootArchivePatterns = @("*.zip", "*.7z", "*.tar", "*.gz", "*.tgz")
$archiveFiles = @()
foreach ($pattern in $rootArchivePatterns) {
    $archiveFiles += @(Get-ChildItem -LiteralPath $root -File -Filter $pattern -Force -ErrorAction SilentlyContinue)
}

$releaseLikeDirectories = @(Get-ChildItem -LiteralPath $root -Directory -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)(^|[-_])(release|archive)([-_]|$)"
})

Add-Check "No final release/archive artifacts" ($archiveFiles.Count -eq 0 -and $releaseLikeDirectories.Count -eq 0) (($archiveFiles.Name + $releaseLikeDirectories.Name) -join ", ")

$scanRoots = @(
    (Join-Path $root "docs"),
    (Join-Path $root "tools"),
    (Join-Path $root "Assets\Data"),
    (Join-Path $root "Assets\Scripts")
)
$rootFileMatches = @(Get-ChildItem -LiteralPath $root -File -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "(?i)\bP10[-_](E|F|G)([-_.]|$)"
} | ForEach-Object { $_.FullName })
$forbiddenP10 = @($rootFileMatches + @(Get-RelativeNameMatches -Roots $scanRoots -Pattern "(?i)\bP10[-_](E|F|G)([-_.]|$)"))
Add-Check "No P10-E/F/G artifacts" ($forbiddenP10.Count -eq 0) ($forbiddenP10 -join ", ")

Write-Host "Ground/road import source scene check"
foreach ($check in $script:Checks) {
    $label = if ($check.Passed) { "PASS" } else { "FAIL" }
    Write-Host ("[{0}] {1}: {2}" -f $label, $check.Name, $check.Detail)
}

$failures = @($script:Checks | Where-Object { -not $_.Passed })
if ($failures.Count -gt 0) {
    Write-Host "Check result: FAIL"
    exit 1
}

Write-Host "Check result: PASS"
exit 0
