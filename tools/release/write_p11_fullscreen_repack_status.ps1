param(
    [string]$GitPushStatus = "not_run",
    [string]$GitHubReleaseUpdateStatus = "not_run",
    [string]$GitHubReleaseNote = "",
    [string]$OutputJson = "Assets\Data\P10\p11_fullscreen_repack_status.json"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$ReleaseRoot = "D:\UnityProjects\ChuoTsunamiEvacuation-Releases"
$ReleaseFolder = Join-Path $ReleaseRoot "ChuoTsunamiEvacuation_v1.0"
$ExePath = Join-Path $ReleaseFolder "ChuoTsunamiEvacuation.exe"
$ZipPath = Join-Path $ReleaseRoot "ChuoTsunamiEvacuation_v1.0.zip"
$OutputPath = Join-Path $ProjectRoot $OutputJson
$OutputDoc = Join-Path $ProjectRoot "docs\P11_FULLSCREEN_REPACK_STATUS.md"

function Read-JsonIfExists {
    param([string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return $null
    }

    return Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Get-TestSummary {
    param([string]$RelativePath)
    $path = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return [ordered]@{
            result = "missing"
            path = $path
        }
    }

    [xml]$xml = Get-Content -LiteralPath $path -Raw -Encoding UTF8
    $root = $xml.DocumentElement
    return [ordered]@{
        result = if ([int]$root.GetAttribute("failed") -eq 0 -and $root.GetAttribute("result") -ne "Failed") { "passed" } else { "failed" }
        path = $path
        total = [int]$root.GetAttribute("total")
        passed = [int]$root.GetAttribute("passed")
        failed = [int]$root.GetAttribute("failed")
        skipped = [int]$root.GetAttribute("skipped")
        inconclusive = [int]$root.GetAttribute("inconclusive")
    }
}

function Test-Contains {
    param([string]$RelativePath, [string]$Pattern)
    $path = Join-Path $ProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return $false
    }

    return (Get-Content -LiteralPath $path -Raw -Encoding UTF8) -match $Pattern
}

$requiredReleaseItems = @(
    "ChuoTsunamiEvacuation.exe",
    "ChuoTsunamiEvacuation_Data",
    "UnityPlayer.dll",
    "MonoBleedingEdge",
    "README_RUN.md",
    "INSTALL_AND_RUN.md",
    "CONTROLS.md",
    "GAME_RULES_EN.md",
    "GAME_RULES_JA.md",
    "KNOWN_LIMITATIONS.md",
    "ATTRIBUTION.md",
    "RELEASE_NOTES.md",
    "SECOND_PC_INSTALL_TEST_GUIDE.md",
    "SECOND_PC_TEST_REPORT_TEMPLATE.md"
)

$releaseItemChecks = [ordered]@{}
foreach ($item in $requiredReleaseItems) {
    $releaseItemChecks[$item] = Test-Path -LiteralPath (Join-Path $ReleaseFolder $item)
}

$projectSettings = Get-Content -LiteralPath (Join-Path $ProjectRoot "ProjectSettings\ProjectSettings.asset") -Raw -Encoding UTF8
$playerSettings = [ordered]@{
    fullscreenModeFullScreenWindow = $projectSettings -match "fullscreenMode:\s*1"
    resizableWindowEnabled = $projectSettings -match "resizableWindow:\s*1"
    nativeFullscreenSwitchDisabledForRuntimeHandler = $projectSettings -match "allowFullscreenSwitch:\s*0"
}

$docs = [ordered]@{
    controlsMentionsF11 = Test-Contains "docs\CONTROLS.md" "F11"
    controlsMentionsAltEnter = Test-Contains "docs\CONTROLS.md" "Alt\+Enter"
    installMentionsRecovery = Test-Contains "docs\INSTALL_AND_RUN.md" "screen-fullscreen 0"
    readmeMentionsRecovery = Test-Contains "docs\README_RUN.md" "screen-fullscreen 0"
    secondPcMentionsRecovery = Test-Contains "docs\SECOND_PC_INSTALL_TEST_GUIDE.md" "screen-fullscreen 0"
}

$source = [ordered]@{
    fullscreenControllerExists = Test-Path -LiteralPath (Join-Path $ProjectRoot "Assets\Scripts\NewMap\NewMapFullscreenModeController.cs") -PathType Leaf
    f11Implemented = Test-Contains "Assets\Scripts\NewMap\NewMapFullscreenModeController.cs" "KeyCode\.F11"
    altEnterImplemented = Test-Contains "Assets\Scripts\NewMap\NewMapFullscreenModeController.cs" "KeyCode\.Return"
    preferenceSaved = Test-Contains "Assets\Scripts\NewMap\NewMapFullscreenModeController.cs" "PlayerPrefs\.SetInt\(FullscreenPreferenceKey"
}

$build = Read-JsonIfExists "Assets\Data\P10\p11_final_build_status.json"
$smoke = Read-JsonIfExists "Assets\Data\P10\p11_local_final_smoke_report.json"
$zip = Read-JsonIfExists "Assets\Data\P10\p11_release_zip_status.json"
$editMode = Get-TestSummary "test-results\editmode-results.xml"
$playMode = Get-TestSummary "test-results\playmode-results.xml"
$zipExists = Test-Path -LiteralPath $ZipPath -PathType Leaf
$zipSize = if ($zipExists) { (Get-Item -LiteralPath $ZipPath).Length } else { 0 }
$exeExists = Test-Path -LiteralPath $ExePath -PathType Leaf
$releaseFolderExists = Test-Path -LiteralPath $ReleaseFolder -PathType Container
$allReleaseItemsPresent = -not ($releaseItemChecks.Values -contains $false)

$gitCommit = ""
try {
    $gitCommit = (git -C $ProjectRoot rev-parse HEAD).Trim()
}
catch {
    $gitCommit = "unknown"
}

$passed =
    $releaseFolderExists -and
    $exeExists -and
    $allReleaseItemsPresent -and
    $zipExists -and
    $zipSize -gt 0 -and
    [bool]$source.fullscreenControllerExists -and
    [bool]$source.f11Implemented -and
    [bool]$source.altEnterImplemented -and
    [bool]$source.preferenceSaved -and
    [bool]$playerSettings.fullscreenModeFullScreenWindow -and
    [bool]$playerSettings.resizableWindowEnabled -and
    [bool]$playerSettings.nativeFullscreenSwitchDisabledForRuntimeHandler -and
    [bool]$docs.controlsMentionsF11 -and
    [bool]$docs.controlsMentionsAltEnter -and
    [bool]$docs.installMentionsRecovery -and
    [string]$build.buildResult -eq "passed" -and
    [string]$smoke.playerLogResult -eq "passed_clean" -and
    [string]$editMode.result -eq "passed" -and
    [string]$playMode.result -eq "passed"

$result = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    workspace = $ProjectRoot
    fullscreenFixStatus = if ([bool]$source.fullscreenControllerExists -and [bool]$source.f11Implemented -and [bool]$source.altEnterImplemented) { "implemented" } else { "incomplete" }
    toggleKeys = @("F11", "Alt+Enter")
    playerSettingsChanged = $true
    playerSettings = $playerSettings
    releaseFolder = $ReleaseFolder
    exePath = $ExePath
    zipPath = $ZipPath
    releaseFolderExists = $releaseFolderExists
    exeExists = $exeExists
    zipExists = $zipExists
    zipSizeBytes = $zipSize
    requiredReleaseItems = $releaseItemChecks
    allRequiredReleaseItemsPresent = $allReleaseItemsPresent
    source = $source
    docs = $docs
    build = $build
    playerLog = [ordered]@{
        result = $smoke.playerLogResult
        logPath = $smoke.logPath
        errorCount = $smoke.errorCount
        warningCount = $smoke.warningCount
        exceptionCount = $smoke.exceptionCount
    }
    tests = [ordered]@{
        editMode = $editMode
        playMode = $playMode
    }
    zipStatus = $zip
    git = [ordered]@{
        commit = $gitCommit
        pushStatus = $GitPushStatus
    }
    githubRelease = [ordered]@{
        updateStatus = $GitHubReleaseUpdateStatus
        note = $GitHubReleaseNote
    }
    finalStatus = if ($passed) { "passed" } else { "needs_review" }
    nextUserAction = "Copy the updated release folder or ChuoTsunamiEvacuation_v1.0.zip to the other Windows computer."
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
$result | ConvertTo-Json -Depth 18 | Set-Content -LiteralPath $OutputPath -Encoding UTF8

$doc = @"
# P11 Fullscreen Repack Status

Generated: $($result.generatedAt)

- Fullscreen fix status: $($result.fullscreenFixStatus)
- Toggle keys: F11, Alt+Enter
- Player Settings changed: $($result.playerSettingsChanged)
- Rebuilt EXE: $ExePath
- Rebuilt release folder: $ReleaseFolder
- Rebuilt ZIP: $ZipPath
- Required release items present: $allReleaseItemsPresent
- Player.log result: $($result.playerLog.result), errors=$($result.playerLog.errorCount), warnings=$($result.playerLog.warningCount), exceptions=$($result.playerLog.exceptionCount)
- EditMode result: $($editMode.result), total=$($editMode.total), failed=$($editMode.failed)
- PlayMode result: $($playMode.result), total=$($playMode.total), failed=$($playMode.failed)
- Git push status: $GitPushStatus
- GitHub Release update status: $GitHubReleaseUpdateStatus
- Final status: $($result.finalStatus)

Player Settings:

- ``fullscreenMode: 1`` FullScreenWindow/borderless fullscreen: $($playerSettings.fullscreenModeFullScreenWindow)
- ``resizableWindow: 1``: $($playerSettings.resizableWindowEnabled)
- ``allowFullscreenSwitch: 0``: $($playerSettings.nativeFullscreenSwitchDisabledForRuntimeHandler)

Next user action:

Copy the updated release folder or ``ChuoTsunamiEvacuation_v1.0.zip`` to the other Windows computer.
"@
$doc | Set-Content -LiteralPath $OutputDoc -Encoding UTF8

Write-Host "[INFO] P11 fullscreen repack status written: $OutputPath"
Write-Host "[INFO] P11 fullscreen repack doc written: $OutputDoc"
if ($result.finalStatus -ne "passed") {
    exit 1
}

exit 0
