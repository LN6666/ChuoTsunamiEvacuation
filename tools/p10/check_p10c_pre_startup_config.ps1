[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$configPath = Join-Path $repoRoot "Assets\Data\P10\p10c_pre_playable_startup_config.json"
$bootstrapPath = Join-Path $repoRoot "Assets\Scripts\P10\P10CPrePlayableStartupBootstrap.cs"
$exporterPath = Join-Path $repoRoot "Assets\Scripts\P10\P10CMMFpsStutterExporter.cs"

function Fail($Message) {
    Write-Host "FAIL: $Message" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path -LiteralPath $configPath -PathType Leaf)) { Fail "Missing playable startup config: $configPath" }
if (-not (Test-Path -LiteralPath $bootstrapPath -PathType Leaf)) { Fail "Missing playable startup bootstrap: $bootstrapPath" }
if (-not (Test-Path -LiteralPath $exporterPath -PathType Leaf)) { Fail "Missing FPS exporter: $exporterPath" }

$config = Get-Content -Raw -Encoding UTF8 -LiteralPath $configPath | ConvertFrom-Json
if (-not $config.playableStartupMode) { Fail "playableStartupMode must be true by default." }
if (-not $config.startMenuVisibleOnLaunch) { Fail "startMenuVisibleOnLaunch must be true." }
if (-not $config.languageSelectorVisible) { Fail "languageSelectorVisible must be true." }
if (-not $config.rulesUiAccessible) { Fail "rulesUiAccessible must be true." }
if (-not $config.generateRuntimeGameplayBootstrap) { Fail "generateRuntimeGameplayBootstrap must be true." }
if ($config.enableProfilingExporterByDefault) { Fail "enableProfilingExporterByDefault must be false." }
if ($config.enableProfilingAutoQuit) { Fail "enableProfilingAutoQuit must be false." }
if ($config.targetHighDetailScenePath -ne "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity") { Fail "Unexpected high-detail target scene path." }

$bootstrapText = Get-Content -Raw -Encoding UTF8 -LiteralPath $bootstrapPath
foreach ($required in @(
    "P10CPreStartupDiagnostics",
    "StartGame",
    "P10BGreenGroundFrameRuntime",
    "P8RiskFrontController",
    "P9DFinalGameplayFlowValidator",
    "RealShelterMarkerRuntimeGenerator",
    "P5DRealQualifiedShelterRuntimeGenerator",
    "P5GHHumanitarianCandidateRuntimeGenerator",
    "NpcEvacuationAgent")) {
    if ($bootstrapText.IndexOf($required, [System.StringComparison]::Ordinal) -lt 0) {
        Fail "Startup bootstrap does not contain required integration marker: $required"
    }
}

$exporterText = Get-Content -Raw -Encoding UTF8 -LiteralPath $exporterPath
if ($exporterText.IndexOf("-p10cMmEnableFpsExporter", [System.StringComparison]::Ordinal) -lt 0) {
    Fail "FPS exporter must be explicit opt-in."
}
if ($exporterText.IndexOf("-p10cMmAutoQuit", [System.StringComparison]::Ordinal) -lt 0) {
    Fail "FPS exporter auto-quit must be explicit command-line mode only."
}

Write-Host "P10-C-Pre playable startup config check: PASS" -ForegroundColor Green
exit 0
