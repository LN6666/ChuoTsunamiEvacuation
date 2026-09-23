[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$baselineRelative = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
$baselinePath = Join-Path $repoRoot ($baselineRelative -replace "/", "\")
$minimumBaselineBytes = 1000000000

function Get-BaselineCandidates {
    $candidates = New-Object 'System.Collections.Generic.List[string]'
    $candidates.Add($baselinePath) | Out-Null

    if (-not [string]::IsNullOrWhiteSpace($env:P8A_BASELINE_SCENE_PATH)) {
        $candidates.Add($env:P8A_BASELINE_SCENE_PATH) | Out-Null
    }

    $defaultSiblingP7 = "D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity"
    if ($defaultSiblingP7 -ne $baselinePath) {
        $candidates.Add($defaultSiblingP7) | Out-Null
    }

    return @($candidates | Select-Object -Unique)
}

function Select-LargeBaselineScene {
    param([string[]]$CandidatePaths)

    foreach ($candidate in $CandidatePaths) {
        if ([string]::IsNullOrWhiteSpace($candidate)) {
            continue
        }

        if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            continue
        }

        $item = Get-Item -LiteralPath $candidate
        Write-Host "Baseline candidate: $candidate"
        Write-Host "Candidate bytes: $($item.Length)"
        if ($item.Length -ge $minimumBaselineBytes) {
            return $item
        }
    }

    return $null
}

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

$sceneItem = Select-LargeBaselineScene @(Get-BaselineCandidates)
if ($null -eq $sceneItem) {
    throw "No large P7 practical baseline scene found. Set P8A_BASELINE_SCENE_PATH or restore the local P7 high-detail baseline."
}

Write-Host "Selected baseline scene: $($sceneItem.FullName)"
Write-Host "Scene bytes: $($sceneItem.Length)"
Write-Host "Scene last write: $($sceneItem.LastWriteTime)"

if ($sceneItem.FullName -ne $baselinePath) {
    Write-Host "Using external local baseline scene; current worktree scene shell is not modified."
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
