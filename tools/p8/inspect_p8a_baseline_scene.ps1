[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$baselineRelative = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
$baselinePath = Join-Path $repoRoot ($baselineRelative -replace "/", "\")

function Get-GitLines {
    param([string[]]$GitArgs)

    $previousErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = "Continue"
        $output = & git @GitArgs 2>$null
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($null -eq $output) {
        return @()
    }

    return @($output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

Write-Host "P8-A baseline scene inspection"
Write-Host "Repo root: $repoRoot"

if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
    throw "Missing P7 practical baseline scene: $baselineRelative"
}

$sceneItem = Get-Item -LiteralPath $baselinePath
Write-Host "Baseline scene: $baselineRelative"
Write-Host "Scene bytes: $($sceneItem.Length)"
Write-Host "Scene last write: $($sceneItem.LastWriteTime)"

if ($sceneItem.Length -lt 1000000000) {
    throw "Baseline scene is unexpectedly small; local high-detail state may have been lost."
}

$baselineStatus = Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", $baselineRelative)
if ($baselineStatus.Count -gt 0) {
    Write-Host "Baseline git status: $($baselineStatus -join '; ')"
}
else {
    Write-Host "Baseline git status: clean"
}

$chuoStatus = Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", "Assets/Scenes/Chuo_BaseMap.unity")
if ($chuoStatus.Count -gt 0) {
    throw "Chuo_BaseMap has git status changes: $($chuoStatus -join '; ')"
}

Write-Host "P8-A baseline scene inspection: PASS"
exit 0
