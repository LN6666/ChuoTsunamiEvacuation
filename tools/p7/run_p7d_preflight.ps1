[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).ProviderPath

$scopeGuardScript = Join-Path $scriptRoot "check_p7_scope.ps1"
$perfValidateScript = Join-Path $scriptRoot "validate_p7d_performance_record.ps1"
. (Join-Path $scriptRoot "get_p7d_scene_evidence.ps1")

function Normalize-RepoPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ""
    }

    $normalized = $Path -replace "\\", "/"
    while ($normalized.StartsWith("./", [System.StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(2)
    }
    while ($normalized.StartsWith("/", [System.StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(1)
    }

    return $normalized
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

function Test-PathStartsWith {
    param(
        [string]$Path,
        [string]$Prefix
    )

    return $Path.StartsWith($Prefix, [System.StringComparison]::OrdinalIgnoreCase)
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

function Invoke-Step {
    param(
        [string]$Name,
        [scriptblock]$ScriptBlock
    )

    Write-Host ""
    Write-Host "P7-D preflight: $Name"
    & $ScriptBlock
    Write-Host "P7-D preflight: $Name PASS"
}

function Assert-ProtectedPathsClean {
    $changedFiles = @(Get-ChangedFiles)
    $violations = New-Object System.Collections.Generic.List[string]

    foreach ($file in $changedFiles) {
        if (Test-PathStartsWith -Path $file -Prefix "ProjectSettings/") {
            $violations.Add("$file matches forbidden ProjectSettings prefix") | Out-Null
        }
        if (Test-PathStartsWith -Path $file -Prefix "Packages/") {
            $violations.Add("$file matches forbidden Packages prefix") | Out-Null
        }
        if (Test-PathStartsWith -Path $file -Prefix "Assets/PLATEAU/") {
            $violations.Add("$file matches forbidden Assets/PLATEAU prefix") | Out-Null
        }
        if (Test-PathStartsWith -Path $file -Prefix "Assets/Data/") {
            $violations.Add("$file matches forbidden Assets/Data prefix") | Out-Null
        }
        if (Test-PathStartsWith -Path $file -Prefix "Assets/Scenes/Chuo_BaseMap.unity") {
            $violations.Add("$file matches forbidden Chuo_BaseMap path") | Out-Null
        }
    }

    if ($violations.Count -gt 0) {
        foreach ($violation in ($violations | Sort-Object -Unique)) {
            Write-Host "FAIL: $violation"
        }

        throw "Protected path check failed."
    }

    Write-Host "Protected path check passed for $($changedFiles.Count) changed/untracked files."
}

function Assert-P7StageCount {
    $taskPath = Join-Path $repoRoot "docs\TASKS.md"
    $taskText = Get-Content -Raw -LiteralPath $taskPath

    foreach ($stage in @("P7-E", "P7-F", "P7-G")) {
        if ($taskText.IndexOf($stage, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -and
            $taskText.IndexOf("Do not create P7-E, P7-F, or P7-G", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "Unexpected $stage reference found outside the approved prohibition context."
        }
    }

    if ($taskText.IndexOf("P7 has exactly five stages", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "docs/TASKS.md must preserve the exact five-stage P7 policy."
    }

    Write-Host "Stage count guard passed: P7-0, P7-A, P7-B, P7-C, P7-D only."
}

function Assert-SceneEvidence {
    param([pscustomobject]$Evidence)

    if (-not $Evidence.SceneExists) {
        throw "Missing P7 high-detail scene."
    }

    Write-Host "sceneBytes=$($Evidence.SceneBytes)"
    Write-Host "meshRendererCount=$($Evidence.MeshRendererCount)"
    Write-Host "meshFilterCount=$($Evidence.MeshFilterCount)"
    Write-Host "plateauCityObjectGroupCount=$($Evidence.PlateauCityObjectGroupCount)"
    Write-Host "averageLod3Achieved=$($Evidence.AverageLod3Achieved)"

    if ($Evidence.MeshRendererCount -eq 0 -or $Evidence.MeshFilterCount -eq 0 -or $Evidence.PlateauCityObjectGroupCount -eq 0) {
        throw "P7 high-detail scene does not contain complete renderable PLATEAU evidence."
    }

    if ($Evidence.ForbiddenHits.Count -gt 0) {
        throw "High-detail scene contains forbidden/protected fragments: $($Evidence.ForbiddenHits -join ', ')"
    }

    Write-P7DCategoryTable -Evidence $Evidence
}

function Assert-CategoryEvidence {
    param([pscustomobject]$Evidence)

    foreach ($required in @("Buildings", "Roads")) {
        $stats = $Evidence.Categories[$required]
        if ($stats.CityObjectGroupHits -eq 0 -or $stats.MeshRendererHits -eq 0 -or $stats.MeshFilterHits -eq 0) {
            throw "$required renderable evidence is missing."
        }
    }

    foreach ($categoryName in $Evidence.Categories.Keys) {
        $stats = $Evidence.Categories[$categoryName]
        Write-Host ("{0,-24} actual={1,-10} status={2}" -f $categoryName, (Get-P7DDetectedLodRange -Stats $stats), (Get-P7DCategoryStatus -Stats $stats))
    }
}

function Assert-LodDocumentation {
    param([pscustomobject]$Evidence)

    $lodReportPath = Join-Path $repoRoot "docs\P7D_LOD_COVERAGE_FINAL_REPORT.md"
    if (-not (Test-Path -LiteralPath $lodReportPath -PathType Leaf)) {
        throw "Missing docs/P7D_LOD_COVERAGE_FINAL_REPORT.md"
    }

    $lodReport = Get-Content -Raw -LiteralPath $lodReportPath
    if (-not $Evidence.AverageLod3Achieved) {
        if ($lodReport.IndexOf("Average LOD3 achieved: FALSE", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "LOD report must explicitly say Average LOD3 achieved: FALSE."
        }
        if ($lodReport.IndexOf("LOD0-LOD2", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "LOD report must document the detected LOD0-LOD2 mismatch."
        }
    }

    Write-Host "LOD mismatch documentation guard passed."
}

function Assert-P2P6CompatibilityDocs {
    $compatPath = Join-Path $repoRoot "docs\P7D_P2_P6_COMPATIBILITY_REPORT.md"
    if (-not (Test-Path -LiteralPath $compatPath -PathType Leaf)) {
        throw "Missing docs/P7D_P2_P6_COMPATIBILITY_REPORT.md"
    }

    $requiredFiles = @(
        "Assets/Scripts/Player/SimplePlayerController.cs",
        "Assets/Scripts/Shelter/ShelterEntranceTrigger.cs",
        "Assets/Scripts/Result/ResultPanelController.cs",
        "Assets/Scripts/Data/ShelterDataSourceResolver.cs",
        "Assets/Scripts/Gameplay/RealShelterMarkerRuntimeGenerator.cs",
        "Assets/Scripts/Gameplay/P5DRealQualifiedShelterRuntimeGenerator.cs",
        "Assets/Scripts/Navigation/NavigationGuidanceController.cs",
        "Assets/Scripts/NPC/NpcEvacuationAgent.cs"
    )

    foreach ($file in $requiredFiles) {
        $path = Join-Path $repoRoot ($file -replace "/", "\")
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "Missing compatibility source evidence: $file"
        }
    }

    $compatText = Get-Content -Raw -LiteralPath $compatPath
    foreach ($phase in @("P2", "P3", "P4", "P5", "P6")) {
        if ($compatText.IndexOf($phase, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "Compatibility report is missing $phase."
        }
    }

    Write-Host "P2-P6 compatibility report/source guard passed."
}

function Assert-P8P9Scope {
    $changedFiles = @(Get-ChangedFiles)
    $forbiddenChangedPrefixes = @(
        "Assets/Scripts/Tsunami/",
        "Assets/Scripts/NPC/",
        "Assets/Scripts/Simulation/"
    )

    foreach ($file in $changedFiles) {
        foreach ($prefix in $forbiddenChangedPrefixes) {
            if (Test-PathStartsWith -Path $file -Prefix $prefix) {
                throw "$file changes a P8/P9 gameplay-adjacent script path during P7-D."
            }
        }
    }

    Write-Host "P8/P9 scope guard passed for changed files."
}

function Assert-AssetPersistenceChecklist {
    $archivePath = Join-Path $repoRoot "docs\P7D_ASSET_ARCHIVE_AND_RECOVERY_PLAN.md"
    if (-not (Test-Path -LiteralPath $archivePath -PathType Leaf)) {
        throw "Missing asset archive/recovery plan."
    }

    $archiveText = Get-Content -Raw -LiteralPath $archivePath
    foreach ($fragment in @("cloud drive", "local only", "P7_HighDetail_Chuo.unity", "Library", "Temp", "Builds", "Logs")) {
        if ($archiveText.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "Archive plan missing required fragment: $fragment"
        }
    }

    Write-Host "Asset persistence checklist passed."
}

function Get-BaselineDecision {
    $baselinePath = Join-Path $repoRoot "docs\P7D_NEW_MAP_BASELINE_DECISION.md"
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw "Missing baseline decision doc."
    }

    $baselineText = Get-Content -Raw -LiteralPath $baselinePath
    if ($baselineText.IndexOf("Decision: PASS", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        return "PASS"
    }
    if ($baselineText.IndexOf("Decision: CONDITIONAL PASS", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        return "CONDITIONAL PASS"
    }
    if ($baselineText.IndexOf("Decision: BLOCKED", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        return "BLOCKED"
    }

    throw "Baseline decision doc must use PASS, CONDITIONAL PASS, or BLOCKED."
}

Write-Host "P7-D preflight: starting"
Write-Host "P7-D preflight: repo root $repoRoot"

$failed = $false
$decision = "BLOCKED"
try {
    Invoke-Step "stage count guard" { Assert-P7StageCount }

    Invoke-Step "P7-D scope guard" {
        & powershell -ExecutionPolicy Bypass -File $scopeGuardScript -Mode P7D
        if ($LASTEXITCODE -ne 0) {
            throw "P7-D scope guard failed."
        }
    }

    Invoke-Step "protected path guard" { Assert-ProtectedPathsClean }

    Write-Host ""
    Write-Host "P7-D preflight: collecting scene evidence once"
    $sceneEvidence = Get-P7DSceneEvidence -RepoRoot $repoRoot

    Invoke-Step "high-detail scene validator" { Assert-SceneEvidence -Evidence $sceneEvidence }
    Invoke-Step "PLATEAU category validator" { Assert-CategoryEvidence -Evidence $sceneEvidence }
    Invoke-Step "LOD coverage validator" { Assert-LodDocumentation -Evidence $sceneEvidence }
    Invoke-Step "P2-P6 compatibility validator" { Assert-P2P6CompatibilityDocs }

    Invoke-Step "performance/profiling record validator" {
        & powershell -ExecutionPolicy Bypass -File $perfValidateScript
        if ($LASTEXITCODE -ne 0) {
            throw "P7-D performance record validator failed."
        }
    }

    Invoke-Step "P8/P9 scope guard" { Assert-P8P9Scope }
    Invoke-Step "asset persistence checklist" { Assert-AssetPersistenceChecklist }

    $decision = Get-BaselineDecision
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P7-D preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P7-D preflight: $decision" -ForegroundColor Yellow
exit 0
