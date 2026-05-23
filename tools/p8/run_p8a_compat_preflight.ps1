[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

function Assert-FileExists {
    param([string]$RelativePath)

    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Assert-FileContains {
    param(
        [string]$RelativePath,
        [string[]]$Fragments
    )

    Assert-FileExists $RelativePath
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    $text = Get-Content -Raw -LiteralPath $path
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
    }
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

function Assert-ProtectedDirtyState {
    $projectSettingsStatus = @(Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", "ProjectSettings", "Packages"))
    if ($projectSettingsStatus.Count -gt 0) {
        throw "ProjectSettings/Packages dirty state detected: $($projectSettingsStatus -join '; ')"
    }

    $baseMapStatus = @(Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", "Assets/Scenes/Chuo_BaseMap.unity"))
    if ($baseMapStatus.Count -gt 0) {
        throw "Chuo_BaseMap dirty state detected: $($baseMapStatus -join '; ')"
    }
}

function Assert-CompatibilityDocs {
    Assert-FileContains "docs/P8A_SCENE_COMPATIBILITY_GATE.md" @(
        "P8 has exactly four stages",
        "P7_HighDetail_Chuo.unity",
        "Chuo_BaseMap.unity",
        "does not change gameplay success/failure rules"
    )

    Assert-FileContains "docs/P8A_P7_HIGHDETAIL_BASELINE_STATUS.md" @(
        "unstaged local dirty state",
        "approximately 22.55 GB",
        "P8-B must anchor risk-front visualization"
    )

    Assert-FileContains "docs/P8A_P2_P6_COMPATIBILITY_SMOKE_REPORT.md" @(
        "SOURCE-COMPATIBLE",
        "runtime validation inside the high-detail scene has not been performed",
        "real_qualified remains opt-in",
        "No unsafe official-route claim"
    )

    Assert-FileContains "docs/P8A_P8B_SCENE_ANCHOR_PLAN.md" @(
        "P8_RiskFrontAnchors",
        "Assets/Data/P8",
        'must not use `Chuo_BaseMap.unity`',
        "P9 crowd simulation"
    )
}

function Assert-PromptTraceability {
    Assert-FileContains "codex_prompts/p8a_scene_compatibility_gate.md" @(
        "P8-A Scene Compatibility Gate for P2-P6 on P7_HighDetail_Chuo",
        "Do not reset, checkout, or overwrite",
        "P8 has exactly four stages"
    )

    Assert-FileContains "deepseek_review_prompt_p8a_compat.md" @(
        "P7_HighDetail",
        "Chuo_BaseMap",
        "P8 still has exactly four stages",
        "A-Level Blockers"
    )
}

Write-Host "P8-A compatibility preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "inspect_p8a_scene_compatibility.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8-A scene compatibility inspection failed."
    }

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p8_hazard_json.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8-A hazard JSON validation failed."
    }

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "inspect_p8a_p2_p6_compatibility.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8-A P2-P6 compatibility inspection failed."
    }

    Assert-ProtectedDirtyState
    Assert-CompatibilityDocs
    Assert-PromptTraceability
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-A compatibility preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-A compatibility preflight: PASS" -ForegroundColor Green
exit 0
