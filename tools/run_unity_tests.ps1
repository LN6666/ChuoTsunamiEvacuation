param(
    [ValidateSet("EditMode", "PlayMode", "All")]
    [string]$Mode = "EditMode",

    [ValidateSet("Batch", "Gui")]
    [string]$LaunchMode = "Batch",

    [int]$TimeoutSeconds = 900
)

$ErrorActionPreference = "Stop"

# Optional local override. Leave empty to auto-detect.
$UnityExecutablePath = ""

$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
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

function Get-UnityEnvironmentBlocker {
    param(
        [Parameter(Mandatory = $true)]
        [string]$LogPath
    )

    if (-not (Test-Path $LogPath)) {
        return $null
    }

    $lines = @(Get-Content -LiteralPath $LogPath -ErrorAction SilentlyContinue)
    $licensingLines = @($lines | Where-Object {
        $_ -match "Licensing Client" -or
        $_ -match "licensing client" -or
        $_ -match "connection.*refused" -or
        $_ -match "connection.*failed"
    })
    $packageManagerLines = @($lines | Where-Object {
        $_ -match "Registered 0 packages" -or
        $_ -match "Package Manager.*0 packages"
    })

    if ($licensingLines.Count -gt 0 -and $packageManagerLines.Count -gt 0) {
        $blockerLines = @()
        $blockerLines += $licensingLines | Select-Object -First 4
        $blockerLines += $packageManagerLines | Select-Object -First 4

        return [pscustomobject]@{
            Message = "Unity Package Manager registered 0 packages after Unity Licensing Client connection failures; batchmode cannot compile Unity modules in this environment."
            Lines = @($blockerLines | Select-Object -Unique)
        }
    }

    return $null
}

function Format-UnityBlockerLines {
    param(
        [AllowNull()]
        $Blocker
    )

    if (-not $Blocker -or -not $Blocker.Lines -or $Blocker.Lines.Count -eq 0) {
        return ""
    }

    return "Blocker log lines:`n" + (($Blocker.Lines | ForEach-Object { "  $_" }) -join "`n")
}

function Get-UnityBlockingReason {
    param(
        [Parameter(Mandatory = $true)]
        [string]$LogPath
    )

    if (-not (Test-Path $LogPath)) {
        return "Unity log was not created."
    }

    $environmentBlocker = Get-UnityEnvironmentBlocker -LogPath $LogPath
    if ($environmentBlocker) {
        $blockerLines = Format-UnityBlockerLines -Blocker $environmentBlocker
        return "$($environmentBlocker.Message) $blockerLines"
    }

    $logText = Get-Content -Raw -LiteralPath $LogPath
    if ($logText -match "Scripts have compiler errors") {
        return "Unity script compilation failed before tests ran."
    }

    if ($logText -match "Unity command-line test XML written") {
        return "Unity reported XML output in the log, but the expected XML file was not found."
    }

    return "Unity did not produce XML results. See log for details."
}

function Stop-LaunchedUnityProcess {
    param(
        [Parameter(Mandatory = $true)]
        [System.Diagnostics.Process]$Process
    )

    $Process.Refresh()
    if (-not $Process.HasExited) {
        Stop-Process -Id $Process.Id -Force
    }
}

function Close-LaunchedUnityProcess {
    param(
        [Parameter(Mandatory = $true)]
        [System.Diagnostics.Process]$Process
    )

    $Process.Refresh()
    if ($Process.HasExited) {
        return
    }

    if ($Process.CloseMainWindow()) {
        if ($Process.WaitForExit(10000)) {
            return
        }
    }

    Stop-LaunchedUnityProcess -Process $Process
}

function Copy-UnityGeneratedResultIfReady {
    param(
        [Parameter(Mandatory = $true)]
        [string]$LogPath,

        [Parameter(Mandatory = $true)]
        [string]$ResultsPath,

        [Parameter(Mandatory = $true)]
        [datetime]$StartedAt
    )

    if (Test-Path $ResultsPath) {
        return $true
    }

    if (-not (Test-Path $LogPath)) {
        return $false
    }

    $resultLines = @(Get-Content -LiteralPath $LogPath -ErrorAction SilentlyContinue | Where-Object {
        $_ -match "Saving results to:\s*(.+\.xml)"
    })

    if ($resultLines.Count -eq 0) {
        return $false
    }

    for ($i = $resultLines.Count - 1; $i -ge 0; $i--) {
        if ($resultLines[$i] -notmatch "Saving results to:\s*(.+\.xml)") {
            continue
        }

        $unityResultPath = $Matches[1].Trim().Trim('"')
        if (-not (Test-Path -LiteralPath $unityResultPath)) {
            continue
        }

        $unityResult = Get-Item -LiteralPath $unityResultPath
        if ($unityResult.LastWriteTime -lt $StartedAt.AddSeconds(-5)) {
            continue
        }

        $resultReady = $false
        for ($attempt = 0; $attempt -lt 10; $attempt++) {
            $unityResult.Refresh()
            if ($unityResult.Length -gt 0) {
                try {
                    [xml]$candidateXml = Get-Content -Raw -LiteralPath $unityResultPath
                    if ($candidateXml.DocumentElement) {
                        $resultReady = $true
                        break
                    }
                }
                catch {
                    $resultReady = $false
                }
            }

            Start-Sleep -Milliseconds 500
        }

        if (-not $resultReady) {
            continue
        }

        try {
            Copy-Item -LiteralPath $unityResultPath -Destination $ResultsPath -Force
            Write-Host "Copied Unity-generated results from $unityResultPath to $ResultsPath"
            return $true
        }
        catch {
            return $false
        }
    }

    return $false
}

function Wait-UnityProcess {
    param(
        [Parameter(Mandatory = $true)]
        [System.Diagnostics.Process]$Process,

        [Parameter(Mandatory = $true)]
        [ValidateSet("EditMode", "PlayMode")]
        [string]$TestPlatform,

        [Parameter(Mandatory = $true)]
        [ValidateSet("Batch", "Gui")]
        [string]$LaunchMode,

        [Parameter(Mandatory = $true)]
        [string]$LogPath,

        [Parameter(Mandatory = $true)]
        [string]$ResultsPath,

        [Parameter(Mandatory = $true)]
        [datetime]$StartedAt
    )

    $deadline = (Get-Date).AddSeconds([Math]::Max($TimeoutSeconds, 1))
    while ($true) {
        if ($Process.WaitForExit(2000)) {
            return [pscustomobject]@{ ResultHarvested = $false }
        }

        if ($LaunchMode -eq "Batch") {
            $environmentBlocker = Get-UnityEnvironmentBlocker -LogPath $LogPath
            if ($environmentBlocker) {
                Stop-LaunchedUnityProcess -Process $Process
                $blockerLines = Format-UnityBlockerLines -Blocker $environmentBlocker
                throw "Unity $TestPlatform tests are blocked in batchmode. $($environmentBlocker.Message) $blockerLines Log: $LogPath"
            }
        }

        if (Copy-UnityGeneratedResultIfReady -LogPath $LogPath -ResultsPath $ResultsPath -StartedAt $StartedAt) {
            Close-LaunchedUnityProcess -Process $Process
            return [pscustomobject]@{ ResultHarvested = $true }
        }

        if ((Get-Date) -ge $deadline) {
            Stop-LaunchedUnityProcess -Process $Process
            $blockingReason = Get-UnityBlockingReason -LogPath $LogPath
            throw "Unity $TestPlatform tests timed out after $TimeoutSeconds seconds. The launched Unity process was terminated. $blockingReason Log: $LogPath"
        }
    }
}

function Get-TestResultSummary {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResultsPath
    )

    [xml]$xml = Get-Content -Raw -LiteralPath $ResultsPath
    $root = $xml.DocumentElement
    if (-not $root) {
        throw "XML results at $ResultsPath did not contain a document element."
    }

    return [pscustomobject]@{
        Total = [int]$root.GetAttribute("total")
        Passed = [int]$root.GetAttribute("passed")
        Failed = [int]$root.GetAttribute("failed")
        Skipped = [int]$root.GetAttribute("skipped")
        Inconclusive = [int]$root.GetAttribute("inconclusive")
        Result = $root.GetAttribute("result")
    }
}

function Format-TestResultSummary {
    param(
        [Parameter(Mandatory = $true)]
        $Summary
    )

    return "total=$($Summary.Total) passed=$($Summary.Passed) failed=$($Summary.Failed) skipped=$($Summary.Skipped) inconclusive=$($Summary.Inconclusive)"
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
    if (Test-Path $ResultsPath) {
        Remove-Item -LiteralPath $ResultsPath -Force
    }

    $logPath = Join-Path $ResultsDir ("{0}-{1}-unity.log" -f $TestPlatform.ToLowerInvariant(), $LaunchMode.ToLowerInvariant())
    if (Test-Path $logPath) {
        Remove-Item -LiteralPath $logPath -Force
    }

    Write-Host "Running Unity $TestPlatform tests in $LaunchMode launch mode..."
    Write-Host "Unity: $unity"
    Write-Host "Project: $ProjectRoot"
    Write-Host "Results: $ResultsPath"
    Write-Host "Log: $logPath"

    $arguments = @()
    if ($LaunchMode -eq "Batch") {
        $arguments += "-batchmode"
    }

    $arguments += @(
        "-projectPath", $ProjectRoot,
        "-executeMethod", "UnityCommandLineTestRunner.Run",
        "-chuoTestMode", $TestPlatform,
        "-chuoTestResults", $ResultsPath,
        "-logFile", $logPath
    )

    $windowStyle = "Hidden"
    if ($LaunchMode -eq "Gui") {
        $windowStyle = "Normal"
    }

    $runStartedAt = Get-Date
    $process = Start-Process -FilePath $unity -ArgumentList $arguments -WindowStyle $windowStyle -PassThru
    $runOutcome = Wait-UnityProcess -Process $process -TestPlatform $TestPlatform -LaunchMode $LaunchMode -LogPath $logPath -ResultsPath $ResultsPath -StartedAt $runStartedAt

    if (-not $runOutcome.ResultHarvested -and $process.ExitCode -ne 0) {
        if (Test-Path $ResultsPath) {
            $failedSummary = Get-TestResultSummary -ResultsPath $ResultsPath
            Write-Host "Unity $TestPlatform test counts: $(Format-TestResultSummary -Summary $failedSummary)"
        }

        $blockingReason = Get-UnityBlockingReason -LogPath $logPath
        throw "Unity $TestPlatform tests failed with exit code $($process.ExitCode). $blockingReason Results: $ResultsPath Log: $logPath"
    }

    if (-not (Test-Path $ResultsPath)) {
        $blockingReason = Get-UnityBlockingReason -LogPath $logPath
        throw "Unity $TestPlatform tests finished without producing XML results at $ResultsPath. $blockingReason Log: $logPath"
    }

    $summary = Get-TestResultSummary -ResultsPath $ResultsPath
    if ($summary.Failed -gt 0 -or $summary.Result -eq "Failed") {
        Write-Host "Unity $TestPlatform test counts: $(Format-TestResultSummary -Summary $summary)"
        throw "Unity $TestPlatform tests completed with failures. Results: $ResultsPath Log: $logPath"
    }

    Write-Host "Unity $TestPlatform tests completed. Results: $ResultsPath" -ForegroundColor Green
    Write-Host "Unity $TestPlatform test counts: $(Format-TestResultSummary -Summary $summary)"
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
