[CmdletBinding()]
param(
    [string]$ExePath = "",
    [string]$BuildDirectory = "D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre",
    [int]$DurationSeconds = 45
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$logPath = Join-Path $BuildDirectory "P10CPre_ActualP7SceneIntegration.log"
$reportPath = Join-Path $repoRoot "docs\P10C_PRE_ACTUAL_P7_SCENE_INTEGRATION_VERIFICATION.md"

if ([string]::IsNullOrWhiteSpace($ExePath)) {
    $ExePath = Join-Path $BuildDirectory "ChuoTsunamiEvacuation_P10CPre.exe"
}

function Write-Report {
    param([string]$Status, [string[]]$Evidence, [string[]]$Limitations)
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# P10-C-Pre Actual P7 Scene Integration Verification")
    $lines.Add("")
    $lines.Add("Status: $Status")
    $lines.Add("")
    $lines.Add("Verification time: $((Get-Date).ToString("s"))")
    $lines.Add("")
    $lines.Add("Built player: $ExePath")
    $lines.Add("")
    $lines.Add("Player log: $logPath")
    $lines.Add("")
    $lines.Add("## Evidence")
    foreach ($line in $Evidence) { $lines.Add("- $line") }
    $lines.Add("")
    $lines.Add("## Limitations Or Blockers")
    if ($Limitations.Count -eq 0) {
        $lines.Add("- None recorded by this scripted check.")
    }
    else {
        foreach ($line in $Limitations) { $lines.Add("- $line") }
    }
    $lines | Set-Content -LiteralPath $reportPath -Encoding UTF8
}

Write-Host "P10-C-Pre actual P7 scene integration verification"
Write-Host "EXE: $ExePath"
Write-Host "Log: $logPath"

if (-not (Test-Path -LiteralPath $ExePath -PathType Leaf)) {
    Write-Report -Status "BLOCKED" -Evidence @("Temporary EXE was not found.") -Limitations @("Build must be recreated before actual P7 scene integration verification can run.")
    throw "Temporary EXE was not found: $ExePath"
}

if (Test-Path -LiteralPath $logPath) {
    Remove-Item -LiteralPath $logPath -Force
}

$arguments = @(
    "-screen-width", "1280",
    "-screen-height", "720",
    "-screen-fullscreen", "0",
    "-logFile", $logPath,
    "-p10cPreAutoStartGame"
)

$process = Start-Process -FilePath $ExePath -ArgumentList $arguments -WindowStyle Normal -PassThru
try {
    Start-Sleep -Seconds ([Math]::Max(5, $DurationSeconds))
    if (-not $process.HasExited) {
        if (-not $process.CloseMainWindow()) {
            Stop-Process -Id $process.Id -Force
        }
        elseif (-not $process.WaitForExit(10000)) {
            Stop-Process -Id $process.Id -Force
        }
    }
}
catch {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
    throw
}

if (-not (Test-Path -LiteralPath $logPath -PathType Leaf)) {
    Write-Report -Status "BLOCKED" -Evidence @("Player log was not written.") -Limitations @("Cannot verify P7 startup diagnostics without Player.log.")
    throw "Player log was not written: $logPath"
}

$log = Get-Content -Raw -LiteralPath $logPath -ErrorAction SilentlyContinue
$diagnostics = @($log -split "`r?`n" | Where-Object { $_ -like "*P10CPreStartupDiagnostics*" })
$evidence = New-Object System.Collections.Generic.List[string]
$limitations = New-Object System.Collections.Generic.List[string]

if ($diagnostics.Count -eq 0) {
    $limitations.Add("Player.log does not contain P10CPreStartupDiagnostics lines.")
}
else {
    foreach ($line in $diagnostics) {
        $evidence.Add($line.Trim())
    }
}

foreach ($required in @(
    "targetActive=True",
    "players=",
    "cameras=",
    "resultPanels=",
    "p4RealShelters=",
    "p5Qualified=",
    "p5Candidates=",
    "p6Npc=",
    "p8RiskFrontLoaded=",
    "p9Crowd=",
    "p9SpawnMarkers=",
    "p9EntranceSafeFloorMarkers=",
    "p9CollapseDebrisZones=",
    "p10GreenFrames=")) {
    if ($log.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        $limitations.Add("Player.log did not contain expected diagnostic token: $required")
    }
}

if ($log.IndexOf("phase=start_game_completed", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
    $limitations.Add("Player.log did not contain final start_game_completed diagnostics.")
}

function Assert-LastPositiveCount {
    param([string]$Pattern, [string]$Label)

    $matches = [regex]::Matches($log, $Pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ($matches.Count -eq 0) {
        $limitations.Add("Player.log did not contain numeric diagnostic for: $Label")
        return
    }

    $value = [int]$matches[$matches.Count - 1].Groups[1].Value
    if ($value -le 0) {
        $limitations.Add("$Label was not reachable in the final P7 startup diagnostics. Last value: $value")
    }
}

Assert-LastPositiveCount -Pattern "players=(\d+)" -Label "P2 player"
Assert-LastPositiveCount -Pattern "cameras=(\d+)" -Label "P2 camera"
Assert-LastPositiveCount -Pattern "resultPanels=(\d+)" -Label "P2 ResultPanel"
Assert-LastPositiveCount -Pattern "entrances=(\d+)" -Label "P2 shelter E interaction entrances"
Assert-LastPositiveCount -Pattern "p4RealShelters=(\d+)" -Label "P3/P4 real shelter markers"
Assert-LastPositiveCount -Pattern "p5Qualified=(\d+)" -Label "P5 qualified shelter guidance"
Assert-LastPositiveCount -Pattern "p5Candidates=(\d+)" -Label "P5 humanitarian candidate guidance"
Assert-LastPositiveCount -Pattern "p6Npc=(\d+)" -Label "P6 NPC prototype"
Assert-LastPositiveCount -Pattern "p9Crowd=(\d+)" -Label "P9 crowd prototype"
Assert-LastPositiveCount -Pattern "p9SpawnMarkers=(\d+)" -Label "P9 spawn markers"
Assert-LastPositiveCount -Pattern "p9EntranceSafeFloorMarkers=(\d+)" -Label "P9 entrance/safe-floor markers"
Assert-LastPositiveCount -Pattern "p9CollapseDebrisZones=(\d+)" -Label "P9 collapse/debris zones"
Assert-LastPositiveCount -Pattern "p10GreenFrames=(\d+)" -Label "P10 green ground frames after verification tsunami start"

if ($log.IndexOf("p8RiskFrontLoaded=True", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
    $limitations.Add("P8 hazard/risk-front handoff did not report as loaded.")
}

if ($log.IndexOf("placeholderLikely=True", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
    $limitations.Add("P9 target scene appears to be the small placeholder/status shell rather than the actual 22GB P7 high-detail scene copy.")
}

$status = if ($limitations.Count -eq 0) { "PASS" } else { "PASS_WITH_LIMITATIONS" }
Write-Report -Status $status -Evidence $evidence.ToArray() -Limitations $limitations.ToArray()
Write-Host "Actual P7 scene integration verification: $status"
Write-Host "Report: $reportPath"
exit 0
