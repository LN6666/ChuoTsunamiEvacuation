param(
    [ValidateSet("EditMode", "PlayMode", "All")]
    [string]$Mode = "EditMode"
)

$ErrorActionPreference = "Stop"

# Optional local override. Leave empty to auto-detect.
$UnityExecutablePath = ""

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$ResultsDir = Join-Path $ProjectRoot "test-results"
$ProjectVersionFile = Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt"

function Get-UnityEditorVersion {
    if (-not (Test-Path $ProjectVersionFile)) {
        return $null
    }

    $versionLine = Get-Content $ProjectVersionFile | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
    if (-not $versionLine) {
        return $null
    }

    return ($versionLine -replace "m_EditorVersion:\s*", "").Trim()
}

function Find-UnityExecutable {
    if ($UnityExecutablePath -and (Test-Path $UnityExecutablePath)) {
        return (Resolve-Path $UnityExecutablePath).Path
    }

    if ($env:UNITY_EXE -and (Test-Path $env:UNITY_EXE)) {
        return (Resolve-Path $env:UNITY_EXE).Path
    }

    $editorVersion = Get-UnityEditorVersion
    $candidatePaths = @()

    if ($editorVersion) {
        $candidatePaths += "C:\Program Files\Unity\Hub\Editor\$editorVersion\Editor\Unity.exe"
        $candidatePaths += "C:\Program Files (x86)\Unity\Hub\Editor\$editorVersion\Editor\Unity.exe"
    }

    $hubRoot = "C:\Program Files\Unity\Hub\Editor"
    if (Test-Path $hubRoot) {
        $candidatePaths += Get-ChildItem -Path $hubRoot -Directory |
            Sort-Object Name -Descending |
            ForEach-Object { Join-Path $_.FullName "Editor\Unity.exe" }
    }

    foreach ($candidatePath in $candidatePaths) {
        if ($candidatePath -and (Test-Path $candidatePath)) {
            return (Resolve-Path $candidatePath).Path
        }
    }

    return $null
}

function Invoke-UnityTestRun {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("EditMode", "PlayMode")]
        [string]$TestPlatform,

        [Parameter(Mandatory = $true)]
        [string]$ResultsPath
    )

    $unity = Find-UnityExecutable
    if (-not $unity) {
        Write-Host "Unity executable was not found." -ForegroundColor Red
        Write-Host "Set `$UnityExecutablePath near the top of tools/run_unity_tests.ps1, or set UNITY_EXE to the full Unity.exe path."
        Write-Host "Expected Unity version is read from ProjectSettings/ProjectVersion.txt when possible."
        exit 1
    }

    New-Item -ItemType Directory -Force -Path $ResultsDir | Out-Null

    Write-Host "Running Unity $TestPlatform tests..."
    Write-Host "Unity: $unity"
    Write-Host "Project: $ProjectRoot"
    Write-Host "Results: $ResultsPath"

    & $unity `
        -batchmode `
        -quit `
        -projectPath $ProjectRoot `
        -runTests `
        -testPlatform $TestPlatform `
        -testResults $ResultsPath `
        -logFile -

    if ($LASTEXITCODE -ne 0) {
        throw "Unity $TestPlatform tests failed with exit code $LASTEXITCODE. Results: $ResultsPath"
    }

    Write-Host "Unity $TestPlatform tests completed. Results: $ResultsPath" -ForegroundColor Green
}

$editModeResults = Join-Path $ResultsDir "editmode-results.xml"
$playModeResults = Join-Path $ResultsDir "playmode-results.xml"

switch ($Mode) {
    "EditMode" {
        Invoke-UnityTestRun -TestPlatform "EditMode" -ResultsPath $editModeResults
    }
    "PlayMode" {
        Invoke-UnityTestRun -TestPlatform "PlayMode" -ResultsPath $playModeResults
    }
    "All" {
        Invoke-UnityTestRun -TestPlatform "EditMode" -ResultsPath $editModeResults
        Invoke-UnityTestRun -TestPlatform "PlayMode" -ResultsPath $playModeResults
    }
}
