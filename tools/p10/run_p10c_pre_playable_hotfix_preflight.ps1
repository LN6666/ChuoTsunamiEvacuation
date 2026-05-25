[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

function Fail($Message) {
    Write-Host "FAIL: $Message" -ForegroundColor Red
    exit 1
}

function Invoke-Checked($Label, [scriptblock]$Block) {
    Write-Host "Checking: $Label"
    & $Block
    if ($LASTEXITCODE -ne 0) {
        Fail "$Label failed."
    }
}

Write-Host "P10-C-Pre playable startup hotfix preflight"
Write-Host "Repo: $repoRoot"

$branch = (& git -C $repoRoot branch --show-current).Trim()
if ($branch -ne "p10c-pre-playable-startup-hotfix") {
    Fail "Expected branch p10c-pre-playable-startup-hotfix, found $branch"
}

Invoke-Checked "high-detail scene availability" {
    & (Join-Path $scriptRoot "check_p10c_pre_high_detail_scene.ps1")
}

Invoke-Checked "startup config" {
    & (Join-Path $scriptRoot "check_p10c_pre_startup_config.ps1")
}

$forbiddenNames = @(
    "P10E", "P10_E", "P10-F", "P10F", "P10_G", "P10G"
)
$repoFiles = @(& git -C $repoRoot ls-files)
foreach ($file in $repoFiles) {
    foreach ($forbidden in $forbiddenNames) {
        if ($file.IndexOf($forbidden, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Fail "Forbidden P10-E/F/G or final release/archive artifact detected in tracked files: $file"
        }
    }
}

$protectedChanges = @(
    & git -C $repoRoot status --short -- ProjectSettings Packages Assets/PLATEAU Assets/Scenes/Chuo_BaseMap.unity Assets/Scenes/Chuo_BaseMap.unity.meta 2>$null
)
if ($protectedChanges.Count -gt 0) {
    $protectedChanges | ForEach-Object { Write-Host $_ }
    Fail "Protected ProjectSettings/Packages/PLATEAU/Chuo_BaseMap changes detected."
}

$highDetailChanges = @(
    & git -C $repoRoot status --short -- Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity.meta 2>$null
)
if ($highDetailChanges.Count -gt 0) {
    $highDetailChanges | ForEach-Object { Write-Host $_ }
    Fail "P9 P7_HighDetail scene file is modified. Do not commit the large/protected scene in this hotfix."
}

$stagedBuildArtifacts = @(
    & git -C $repoRoot diff --cached --name-only | Where-Object {
        $_ -match "\.(exe|zip|7z|tar|gz|log)$" -or $_ -like "Builds/*" -or $_ -like "builds/*"
    }
)
if ($stagedBuildArtifacts.Count -gt 0) {
    $stagedBuildArtifacts | ForEach-Object { Write-Host $_ }
    Fail "Build artifacts are staged."
}

$p7Source = "D:\UnityProjects\ChuoTsunamiEvacuation-P7"
if (Test-Path -LiteralPath $p7Source -PathType Container) {
    $p7SceneChanges = @(
        & git -C $p7Source status --short -- Assets/Scenes/P7HighDetail 2>$null
    )
    if ($p7SceneChanges.Count -gt 0) {
        $expectedPreExistingStatus = " M Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"
        $expectedPreExistingBytes = 22554882711
        $expectedPreExistingLastWriteUtc = "2026-05-23T07:54:28.3463384Z"
        $p7Scene = Join-Path $p7Source "Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity"
        $p7SceneInfo = if (Test-Path -LiteralPath $p7Scene -PathType Leaf) { Get-Item -LiteralPath $p7Scene } else { $null }
        $matchesDocumentedBaseline =
            $p7SceneChanges.Count -eq 1 -and
            $p7SceneChanges[0] -eq $expectedPreExistingStatus -and
            $p7SceneInfo -ne $null -and
            $p7SceneInfo.Length -eq $expectedPreExistingBytes -and
            $p7SceneInfo.LastWriteTimeUtc.ToString("o") -eq $expectedPreExistingLastWriteUtc

        if (-not $matchesDocumentedBaseline) {
            $p7SceneChanges | ForEach-Object { Write-Host $_ }
            Fail "P7 source high-detail scene directory has unexpected dirty state. Treat P7 as read-only and resolve before committing."
        }

        Write-Host "WARNING: P7 source high-detail scene directory has the documented pre-existing dirty baseline; no new P7 mutation detected by size/timestamp/status." -ForegroundColor Yellow
        $p7SceneChanges | ForEach-Object { Write-Host $_ }
    }
}

Write-Host "P10-C-Pre playable startup hotfix preflight: PASS" -ForegroundColor Green
exit 0
