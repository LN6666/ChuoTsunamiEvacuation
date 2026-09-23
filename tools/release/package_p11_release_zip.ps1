param()

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$ReleaseRoot = "D:\UnityProjects\ChuoTsunamiEvacuation-Releases"
$ReleaseFolder = Join-Path $ReleaseRoot "ChuoTsunamiEvacuation_v1.0"
$ZipPath = Join-Path $ReleaseRoot "ChuoTsunamiEvacuation_v1.0.zip"
$OutputJson = Join-Path $ProjectRoot "Assets\Data\P10\p11_release_zip_status.json"

function Get-DirectorySizeBytes {
    param([string]$Path)
    return [long]((Get-ChildItem -LiteralPath $Path -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum)
}

function Write-Json {
    param($Object, [string]$Path)
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    $Object | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $Path -Encoding UTF8
}

$exe = Join-Path $ReleaseFolder "ChuoTsunamiEvacuation.exe"
$data = Join-Path $ReleaseFolder "ChuoTsunamiEvacuation_Data"
$manifest = Join-Path $ReleaseFolder "PACKAGE_MANIFEST.json"
$complete = (Test-Path -LiteralPath $exe -PathType Leaf) -and (Test-Path -LiteralPath $data -PathType Container) -and (Test-Path -LiteralPath $manifest -PathType Leaf)
if (-not $complete) {
    Write-Host "[FAIL] Release folder is incomplete: $ReleaseFolder"
    exit 1
}

if (Test-Path -LiteralPath $ZipPath -PathType Leaf) {
    Remove-Item -LiteralPath $ZipPath -Force
}

$tar = Join-Path $env:SystemRoot "System32\tar.exe"
if (-not (Test-Path -LiteralPath $tar -PathType Leaf)) {
    Write-Host "[FAIL] tar.exe not found and Compress-Archive cannot be used for this large Unity package."
    exit 1
}

$archiveProcess = Start-Process -FilePath $tar -ArgumentList @(
    "-a",
    "-cf",
    $ZipPath,
    "-C",
    $ReleaseRoot,
    "ChuoTsunamiEvacuation_v1.0"
) -PassThru -NoNewWindow -Wait

if ($archiveProcess.ExitCode -ne 0) {
    Write-Host "[FAIL] tar.exe failed to create ZIP. Exit code: $($archiveProcess.ExitCode)"
    exit 1
}

$zipExists = Test-Path -LiteralPath $ZipPath -PathType Leaf
$zipSize = if ($zipExists) { (Get-Item -LiteralPath $ZipPath).Length } else { 0 }
$result = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    releaseFolder = $ReleaseFolder
    zipPath = $ZipPath
    zipExists = $zipExists
    zipSizeBytes = $zipSize
    releaseFolderSizeBytes = Get-DirectorySizeBytes $ReleaseFolder
    topLevelFolder = "ChuoTsunamiEvacuation_v1.0"
    finalStatus = if ($zipExists -and $zipSize -gt 0) { "passed" } else { "failed" }
}
Write-Json $result $OutputJson

$doc = @"
# P11 Release ZIP Status

Generated: $($result.generatedAt)

- Release folder: $ReleaseFolder
- ZIP path: $ZipPath
- ZIP exists: $zipExists
- ZIP size bytes: $zipSize
- Top-level ZIP folder: ChuoTsunamiEvacuation_v1.0
- Result: $($result.finalStatus)
"@
$doc | Set-Content -LiteralPath (Join-Path $ProjectRoot "docs\P11_RELEASE_ZIP_STATUS.md") -Encoding UTF8

if ($result.finalStatus -ne "passed") {
    Write-Host "[FAIL] P11 release ZIP failed. JSON: $OutputJson"
    exit 1
}

Write-Host "[PASS] P11 release ZIP created: $ZipPath"
exit 0
