param(
    [int]$TimeoutSeconds = 1800
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$ReleaseRoot = "D:\UnityProjects\ChuoTsunamiEvacuation-Releases"
$ReleaseFolder = Join-Path $ReleaseRoot "ChuoTsunamiEvacuation_v1.0"
$ExePath = Join-Path $ReleaseFolder "ChuoTsunamiEvacuation.exe"
$BuildLogPath = Join-Path $ProjectRoot "Logs\p11_final_build.log"
$OutputJson = Join-Path $ProjectRoot "Assets\Data\P10\p11_final_build_status.json"
$ManifestJson = Join-Path $ProjectRoot "Assets\Data\P10\p11_release_package_manifest.json"

function Get-DirectorySizeBytes {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) { return 0 }
    return [long]((Get-ChildItem -LiteralPath $Path -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum)
}

function Write-Json {
    param($Object, [string]$Path)
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    $Object | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding UTF8
}

$versionLine = Get-Content -LiteralPath (Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt") | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
$unityVersion = ($versionLine -replace "m_EditorVersion:\s*", "").Trim()
$unity = "C:\Program Files\Unity\Hub\Editor\$unityVersion\Editor\Unity.exe"
if (-not (Test-Path -LiteralPath $unity -PathType Leaf)) {
    throw "Unity executable not found: $unity"
}

$releaseRootFull = [IO.Path]::GetFullPath($ReleaseRoot)
$releaseFolderFull = [IO.Path]::GetFullPath($ReleaseFolder)
if (-not $releaseFolderFull.StartsWith($releaseRootFull, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to clean unexpected release folder: $releaseFolderFull"
}

if (Test-Path -LiteralPath $ReleaseFolder) {
    Remove-Item -LiteralPath $ReleaseFolder -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $ReleaseFolder | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $BuildLogPath) | Out-Null

$arguments = @(
    "-batchmode",
    "-quit",
    "-projectPath", $ProjectRoot,
    "-executeMethod", "NewMapSceneSetupUtility.BuildP11FinalPlayerCommandLine",
    "-logFile", $BuildLogPath
)

Write-Host "Starting P11 final Windows x64 build. Log: $BuildLogPath"
$process = Start-Process -FilePath $unity -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $process.Id -Force
    throw "Unity final build timed out after $TimeoutSeconds seconds."
}

$dataFolder = Join-Path $ReleaseFolder "ChuoTsunamiEvacuation_Data"
$complete = (Test-Path -LiteralPath $ExePath -PathType Leaf) -and (Test-Path -LiteralPath $dataFolder -PathType Container)
$copyErrors = @()

if ($complete) {
    $playerDataP10 = Join-Path $dataFolder "Data\P10"
    New-Item -ItemType Directory -Force -Path $playerDataP10 | Out-Null
    Get-ChildItem -LiteralPath (Join-Path $ProjectRoot "Assets\Data\P10") -Filter "*.json" -File | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $playerDataP10 $_.Name) -Force
    }

    $docs = @(
        "README_RUN.md",
        "INSTALL_AND_RUN.md",
        "CONTROLS.md",
        "GAME_RULES_EN.md",
        "GAME_RULES_JA.md",
        "KNOWN_LIMITATIONS.md",
        "DATA_SOURCES.md",
        "ATTRIBUTION.md",
        "RELEASE_NOTES.md",
        "SECOND_PC_INSTALL_TEST_GUIDE.md",
        "SECOND_PC_TEST_REPORT_TEMPLATE.md",
        "PLAYER_LOG_LOCATION.md"
    )

    foreach ($docName in $docs) {
        $source = Join-Path $ProjectRoot ("docs\" + $docName)
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            Copy-Item -LiteralPath $source -Destination (Join-Path $ReleaseFolder $docName) -Force
        }
        else {
            $copyErrors += "Missing doc: $docName"
        }
    }

    $branch = (git -C $ProjectRoot branch --show-current).Trim()
    $commit = (git -C $ProjectRoot rev-parse HEAD).Trim()
    $versionText = @"
ChuoTsunamiEvacuation v1.0
Build date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")
Git branch: $branch
Git commit: $commit
Unity: $unityVersion
Executable: ChuoTsunamiEvacuation.exe
"@
    $versionText | Set-Content -LiteralPath (Join-Path $ReleaseFolder "VERSION.txt") -Encoding UTF8

    $manifest = [ordered]@{
        version = "1.0"
        buildDate = (Get-Date).ToString("s")
        gitBranch = $branch
        gitCommitHash = $commit
        unityVersion = $unityVersion
        executableName = "ChuoTsunamiEvacuation.exe"
        includedDocs = $docs + @("VERSION.txt", "PACKAGE_MANIFEST.json")
        buildFolder = $ReleaseFolder
        buildFolderSizeBytes = Get-DirectorySizeBytes $ReleaseFolder
        playerLogPath = "%USERPROFILE%\AppData\LocalLow\DefaultCompany\ChuoTsunamiEvacuation\Player.log"
        knownLimitationsFile = "KNOWN_LIMITATIONS.md"
        installMethod = "Copy the entire ChuoTsunamiEvacuation_v1.0 folder and keep the EXE beside ChuoTsunamiEvacuation_Data."
        completePortableFolder = $true
    }
    Write-Json $manifest (Join-Path $ReleaseFolder "PACKAGE_MANIFEST.json")
    Write-Json $manifest $ManifestJson
}

$buildErrors = @()
$buildWarnings = @()
if (Test-Path -LiteralPath $BuildLogPath -PathType Leaf) {
    $buildLines = @(Get-Content -LiteralPath $BuildLogPath -Encoding UTF8 -ErrorAction SilentlyContinue)
    $buildErrors = @($buildLines | Where-Object { $_ -match "BuildFailedException|Scripts have compiler errors|error CS|NullReferenceException|^\s*Error:" } | Select-Object -First 30)
    $buildWarnings = @($buildLines | Where-Object { $_ -match "^\s*Warning:|LogType\.Warning" } | Select-Object -First 30)
}

$folderSize = Get-DirectorySizeBytes $ReleaseFolder
$result = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    buildResult = if ($process.ExitCode -eq 0 -and $complete -and $copyErrors.Count -eq 0) { "passed" } else { "failed" }
    unityExitCode = $process.ExitCode
    unityVersion = $unityVersion
    buildPath = $ReleaseFolder
    exePath = $ExePath
    dataFolder = $dataFolder
    includedScenes = @("Assets/Scenes/Chuo_BaseMap.unity")
    buildSizeBytes = $folderSize
    warnings = $buildWarnings
    errors = $buildErrors + $copyErrors
    folderComplete = $complete
    releaseDocsCopied = $copyErrors.Count -eq 0
}
Write-Json $result $OutputJson

$statusDoc = @"
# P11 Final Build Status

Generated: $($result.generatedAt)

- Build result: $($result.buildResult)
- Build path: $ReleaseFolder
- EXE path: $ExePath
- Included scene: Assets/Scenes/Chuo_BaseMap.unity
- Build size bytes: $folderSize
- Complete portable folder: $complete
- Docs copied: $($copyErrors.Count -eq 0)

Errors:

$($result.errors -join [Environment]::NewLine)
"@
$statusDoc | Set-Content -LiteralPath (Join-Path $ProjectRoot "docs\P11_FINAL_BUILD_STATUS.md") -Encoding UTF8

if ($result.buildResult -ne "passed") {
    Write-Host "[FAIL] P11 final build failed. JSON: $OutputJson"
    exit 1
}

Write-Host "[PASS] P11 final build completed: $ExePath"
exit 0
