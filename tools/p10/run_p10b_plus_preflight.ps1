[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath
$baselineScene = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity"

function Normalize-RepoPath {
    param([string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path)) { return "" }
    $normalized = $Path -replace "\\", "/"
    while ($normalized.StartsWith("./", [System.StringComparison]::Ordinal)) { $normalized = $normalized.Substring(2) }
    while ($normalized.StartsWith("/", [System.StringComparison]::Ordinal)) { $normalized = $normalized.Substring(1) }
    return $normalized
}

function Get-GitLines {
    param([string[]]$GitArgs)
    $previousErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = "Continue"
        $output = & git -c filter.lfs.clean= -c filter.lfs.smudge= -c filter.lfs.process= -c filter.lfs.required=false @GitArgs 2>$null
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }
    if ($null -eq $output) { return @() }
    return @($output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-ChangedFiles {
    $diffFiles = Get-GitLines @("-C", $repoRoot, "diff", "--name-only", "HEAD", "--")
    $untrackedFiles = Get-GitLines @("-C", $repoRoot, "ls-files", "--others", "--exclude-standard")
    return @(
        @($diffFiles) + @($untrackedFiles) |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
    )
}

function Test-PathStartsWith {
    param([string]$Path, [string]$Prefix)
    return $Path.StartsWith($Prefix, [System.StringComparison]::OrdinalIgnoreCase)
}

function Assert-FileExists {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Assert-Branch {
    $branch = (& git -C $repoRoot branch --show-current).Trim()
    if ($branch -ne "p10b-plus-ui-localization-weather-stamina") {
        throw "Expected branch p10b-plus-ui-localization-weather-stamina, found $branch"
    }
}

function Assert-P10StageCount {
    Assert-FileExists "docs/P10_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath (Join-Path $repoRoot "docs\P10_STAGE_PLAN.md") |
            Where-Object { $_ -match "^##\s+(P10-[A-D])\s*$" } |
            ForEach-Object { $Matches[1] }
    )
    $expected = @("P10-A", "P10-B", "P10-C", "P10-D")
    foreach ($stage in $expected) {
        if ($stages -notcontains $stage) { throw "Missing official P10 stage: $stage" }
    }
    if ($stages.Count -ne 4) {
        throw "P10 must have exactly four official stages. Found: $($stages -join ', ')"
    }
}

function Assert-NoExtraP10StageArtifacts {
    $trackedAndChanged = @(
        Get-GitLines @("-C", $repoRoot, "ls-files") |
            ForEach-Object { Normalize-RepoPath $_ }
    ) + @(Get-ChangedFiles)
    $files = @(
        $trackedAndChanged |
            Where-Object { $_ -match "(^|[\\/_.-])p10[-_]?[efg]($|[\\/_.-])" } |
            Sort-Object -Unique
    )
    if ($files.Count -gt 0) {
        throw "Unexpected P10 stage artifact path detected: $($files -join '; ')"
    }
}

function Assert-ProtectedPathsClean {
    $protectedStatus = @(Get-GitLines @(
        "-C", $repoRoot,
        "status", "--porcelain=v1", "--",
        "ProjectSettings",
        "Packages",
        "Assets/Scenes/Chuo_BaseMap.unity",
        "Assets/PLATEAU",
        $baselineScene
    ))
    if ($protectedStatus.Count -gt 0) {
        throw "Protected path dirty state detected: $($protectedStatus -join '; ')"
    }
}

function Test-AllowedP10BPlusPath {
    param([string]$Path)
    if ($Path -like "Assets/Scripts/P10/P10BPlus*") { return $true }
    if ($Path -like "Assets/Scripts/P10/P10BPlus*.meta") { return $true }
    if ($Path -eq "Assets/Scripts/Player/SimplePlayerController.cs") { return $true }
    if ($Path -like "Assets/Tests/EditMode/P10/P10BPlus*") { return $true }
    if ($Path -like "Assets/Tests/PlayMode/P10/P10BPlus*") { return $true }
    if ($Path -like "Assets/Data/P10/p10b_plus_*.json") { return $true }
    if ($Path -like "Assets/Data/P10/p10b_plus_*.json.meta") { return $true }
    if ($Path -like "docs/P10B_PLUS_*.md") { return $true }
    if ($Path -eq "docs/GAME_RULES_EN.md") { return $true }
    if ($Path -eq "docs/GAME_RULES_JA.md") { return $true }
    if ($Path -eq "docs/P10_STAGE_PLAN.md") { return $true }
    if ($Path -eq "docs/P10B_MANUAL_PLAYTEST_CHECKLIST.md") { return $true }
    if ($Path -eq "docs/REVIEW_BACKLOG.md") { return $true }
    if ($Path -eq "docs/TASKS.md") { return $true }
    if ($Path -eq "tools/p10/run_p10b_plus_preflight.ps1") { return $true }
    if ($Path -eq "tools/p10/validate_p10b_plus_json.ps1") { return $true }
    if ($Path -eq "codex_prompts/p10b_plus_ui_localization_weather_stamina.md") { return $true }
    if ($Path -eq "deepseek_review_prompt_p10b_plus.md") { return $true }
    return $false
}

function Assert-ChangedFilesAllowed {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($file in $changedFiles) {
        if (Test-AllowedP10BPlusPath $file) { continue }
        $violations.Add("$file is outside P10-B+ allowed paths") | Out-Null
    }
    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) { Write-Host "FAIL: $violation" }
        throw "P10-B+ changed-file scope check failed."
    }
    Write-Host "P10-B+ changed-file scope check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "Assets/Data/P10/p10b_plus_localization_en.json",
        "Assets/Data/P10/p10b_plus_localization_ja.json",
        "Assets/Data/P10/p10b_plus_ui_config.json",
        "Assets/Data/P10/p10b_plus_weather_config.json",
        "Assets/Data/P10/p10b_plus_movement_stamina_config.json",
        "Assets/Data/P10/p10b_plus_avatar_mobility_config.json",
        "Assets/Data/P10/p10b_plus_manual_playtest_checklist.json",
        "docs/P10B_PLUS_UI_LOCALIZATION_POLISH.md",
        "docs/P10B_PLUS_START_AND_PAUSE_MENU.md",
        "docs/P10B_PLUS_GAME_RULES_UI.md",
        "docs/P10B_PLUS_WEATHER_AND_STAMINA_RULES.md",
        "docs/P10B_PLUS_PLAYER_PROFILE_POLICY.md",
        "docs/P10B_PLUS_UI_BACKGROUND_ASSET_POLICY.md",
        "docs/P10B_PLUS_TEST_RESULTS.md",
        "docs/P10B_PLUS_MANUAL_PLAYTEST_UPDATE.md",
        "docs/P10B_PLUS_NEXT_STEPS_TO_P10C.md",
        "docs/GAME_RULES_EN.md",
        "docs/GAME_RULES_JA.md",
        "tools/p10/run_p10b_plus_preflight.ps1",
        "tools/p10/validate_p10b_plus_json.ps1",
        "codex_prompts/p10b_plus_ui_localization_weather_stamina.md",
        "deepseek_review_prompt_p10b_plus.md"
    )) {
        Assert-FileExists $file
    }
}

function Assert-NoBuildReleaseOrUnlicensedImages {
    $changedFiles = @(Get-ChangedFiles)
    $forbidden = @(
        $changedFiles |
            Where-Object {
                (Test-PathStartsWith -Path $_ -Prefix "Builds/") -or
                (Test-PathStartsWith -Path $_ -Prefix "Release/") -or
                (Test-PathStartsWith -Path $_ -Prefix "release_package/") -or
                (Test-PathStartsWith -Path $_ -Prefix "archives/") -or
                (Test-PathStartsWith -Path $_ -Prefix "run_logs/") -or
                ($_ -match "\.(exe|zip|7z|msi|png|jpg|jpeg|webp)$")
            } |
            Sort-Object -Unique
    )
    if ($forbidden.Count -gt 0) {
        throw "P10-B+ must not add build/release/archive or unlicensed image artifacts: $($forbidden -join '; ')"
    }
}

function Assert-ClaimBoundaries {
    $text = @(
        (Get-Content -Raw -LiteralPath (Join-Path $repoRoot "docs\P10B_PLUS_PLAYER_PROFILE_POLICY.md")),
        (Get-Content -Raw -LiteralPath (Join-Path $repoRoot "docs\GAME_RULES_EN.md")),
        (Get-Content -Raw -LiteralPath (Join-Path $repoRoot "docs\P10B_PLUS_UI_BACKGROUND_ASSET_POLICY.md"))
    ) -join "`n"
    foreach ($required in @(
        "not a real-world claim",
        "not official evacuation routes",
        "not official evacuation shelters",
        "does not commit internet images"
    )) {
        if ($text.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "P10-B+ docs must clearly document '$required'."
        }
    }

    $avatarJson = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "Assets\Data\P10\p10b_plus_avatar_mobility_config.json")
    if ($avatarJson.Contains('"enableGenderSpeedModifier": true')) {
        throw "Gender speed modifier must not be enabled by default."
    }
}

function Assert-NoP7P8P9RuntimeSourceChanges {
    $changedFiles = @(Get-ChangedFiles)
    $sourceChanges = @(
        $changedFiles |
            Where-Object {
                (Test-PathStartsWith -Path $_ -Prefix "Assets/Scripts/P7") -or
                (Test-PathStartsWith -Path $_ -Prefix "Assets/Scripts/P8") -or
                (Test-PathStartsWith -Path $_ -Prefix "Assets/Scripts/P9")
            }
    )
    if ($sourceChanges.Count -gt 0) {
        throw "P10-B+ must not reimplement P7/P8/P9 runtime systems: $($sourceChanges -join '; ')"
    }
}

Write-Host "P10-B+ preflight: starting"
Write-Host "Repo root: $repoRoot"
Assert-Branch
Assert-P10StageCount
Assert-NoExtraP10StageArtifacts
Assert-RequiredArtifacts
Assert-ChangedFilesAllowed
Assert-NoBuildReleaseOrUnlicensedImages
Assert-ProtectedPathsClean
Assert-ClaimBoundaries
Assert-NoP7P8P9RuntimeSourceChanges
& (Join-Path $scriptRoot "validate_p10b_plus_json.ps1")
Write-Host "P10-B+ preflight: PASS" -ForegroundColor Green
exit 0
