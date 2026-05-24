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
        $output = & git -c filter.lfs.clean= -c filter.lfs.smudge= -c filter.lfs.required=false @GitArgs 2>$null
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
    return @(@($diffFiles) + @($untrackedFiles) | ForEach-Object { Normalize-RepoPath $_ } | Where-Object { $_ } | Sort-Object -Unique)
}

function Assert-FileExists {
    param([string]$RelativePath)
    $path = Join-Path $repoRoot ($RelativePath -replace "/", "\")
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $RelativePath"
    }
}

function Assert-FileContains {
    param([string]$RelativePath, [string[]]$Fragments)
    Assert-FileExists $RelativePath
    $text = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\"))
    foreach ($fragment in $Fragments) {
        if ($text.IndexOf($fragment, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            throw "$RelativePath missing required wording: $fragment"
        }
    }
}

function Read-Json {
    param([string]$RelativePath)
    Assert-FileExists $RelativePath
    return Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repoRoot ($RelativePath -replace "/", "\")) | ConvertFrom-Json
}

function Assert-Branch {
    $branch = (& git -C $repoRoot branch --show-current).Trim()
    if ($branch -ne "p8-tsunami-hazard-risk-front-foundation") {
        throw "Expected branch p8-tsunami-hazard-risk-front-foundation, found $branch"
    }
}

function Assert-P8StageCount {
    Assert-FileExists "docs/P8_STAGE_PLAN.md"
    $stages = @(
        Get-Content -LiteralPath (Join-Path $repoRoot "docs\P8_STAGE_PLAN.md") |
            Where-Object { $_ -match "^##\s+(P8-[A-Z0-9]+)\s*$" } |
            ForEach-Object { $Matches[1] }
    )
    $expected = @("P8-A", "P8-B", "P8-C", "P8-D", "P8-E")
    $unexpected = @($stages | Where-Object { $expected -notcontains $_ })
    if ($stages.Count -ne 5 -or $unexpected.Count -gt 0) {
        throw "P8 stage headings must be exactly P8-A through P8-E. Found: $($stages -join ', ')"
    }

    $forbidden = @(
        Get-GitLines @("-C", $repoRoot, "ls-files") |
            ForEach-Object { Normalize-RepoPath $_ } |
            Where-Object { $_ -match "(^|[\\/_.-])p8[-_]?(0|f|g)($|[\\/_.-])" }
    )
    if ($forbidden.Count -gt 0) {
        throw "Forbidden P8-0/F/G artifact path found: $($forbidden -join '; ')"
    }
}

function Assert-ProtectedPaths {
    $protectedStatus = @(Get-GitLines @("-C", $repoRoot, "status", "--porcelain=v1", "--", "ProjectSettings", "Packages", "Assets/Scenes/Chuo_BaseMap.unity", "Assets/PLATEAU"))
    if ($protectedStatus.Count -gt 0) {
        throw "Protected path dirty state detected: $($protectedStatus -join '; ')"
    }

    $stagedBaseline = @(Get-GitLines @("-C", $repoRoot, "diff", "--cached", "--name-only", "--", $baselineScene))
    if ($stagedBaseline.Count -gt 0) {
        throw "P7_HighDetail baseline scene is staged; do not commit scene mutation."
    }

    $baselinePath = Join-Path $repoRoot ($baselineScene -replace "/", "\")
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw "Missing P7_HighDetail baseline scene: $baselineScene"
    }
    if ((Get-Item -LiteralPath $baselinePath).Length -lt 1000000000) {
        throw "P7_HighDetail baseline scene is unexpectedly small; baseline may be lost."
    }
}

function Assert-RequiredArtifacts {
    foreach ($file in @(
        "Assets/Scripts/P8/P8InfrastructureDamageState.cs",
        "Assets/Scripts/P8/P8InfrastructureDamageConfig.cs",
        "Assets/Scripts/P8/P8InfrastructureDamageEvaluator.cs",
        "Assets/Scripts/P8/P8InfrastructureDamageProxy.cs",
        "Assets/Scripts/P8/P8CollapseProxyRule.cs",
        "Assets/Scripts/P8/P8DamageProxyMarker.cs",
        "Assets/Scripts/P8/P8EntranceBlockedProxy.cs",
        "Assets/Scripts/P8/P8LowFloorInundationWarning.cs",
        "Assets/Scripts/P8/P8HumanitarianCandidateHazardStatus.cs",
        "Assets/Tests/EditMode/P8/P8InfrastructureDamageEvaluatorTests.cs",
        "Assets/Tests/PlayMode/P8/P8DamageProxyMarkerPlayModeTests.cs",
        "Assets/Data/P8/infrastructure_damage_proxy_config.json",
        "Assets/Data/P8/humanitarian_candidate_hazard_status_config.json",
        "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json",
        "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json",
        "Assets/Data/P8/tsunami_hazard_evidence_registry.json",
        "docs/P8D_INFRASTRUCTURE_DAMAGE_BLOCKAGE_PROXY_DESIGN.md",
        "docs/P8D_DAMAGE_PROXY_RULEBOOK.md",
        "docs/P8D_COLLAPSE_PROXY_VISUAL_RULES.md",
        "docs/P8D_ENTRANCE_BLOCKED_AND_LOW_FLOOR_WARNING.md",
        "docs/P8D_ROAD_BRIDGE_UNDERGROUND_RESTRICTED_STATUS.md",
        "docs/P8D_HUMANITARIAN_CANDIDATE_DAMAGE_STATUS.md",
        "docs/P8D_HUMANITARIAN_CANDIDATE_NON_OFFICIAL_LABELING.md",
        "docs/P8D_TEST_RESULTS.md",
        "docs/P8D_KNOWN_LIMITATIONS.md",
        "docs/P8D_TO_P8E_HANDOFF.md",
        "tools/p8/validate_p8d_damage_config.ps1",
        "tools/p8/inspect_p8d_humanitarian_candidate_status.ps1"
    )) {
        Assert-FileExists $file
    }
}

function Assert-CodeRules {
    Assert-FileContains "Assets/Scripts/P8/P8InfrastructureDamageEvaluator.cs" @(
        "arrivalTimeSeconds",
        "inundationDepthMeters",
        "hazardIntensity",
        "confidence",
        "sourceMode",
        "evidenceSourceId",
        "AffectsGameplaySuccessFailure = false",
        "RequiresP9Systems = false",
        "ImplementsTrueStructuralCollapse = false",
        "ImplementsPhysicsCollapse = false",
        "UsesMaxTsunamiHeightAsDepth = false",
        "UsesVisualHeightAsPhysicalDepth = false",
        "maxTsunamiHeightMeters is metadata only",
        "visualHeightMeters is cinematic only"
    )
    Assert-FileContains "Assets/Scripts/P8/P8InfrastructureDamageState.cs" @(
        "NoDamage",
        "LowFloorInundationWarning",
        "EntranceBlockedProxy",
        "RoadRestrictedProxy",
        "BridgeRestrictedProxy",
        "UndergroundAvoidProxy",
        "BuildingDamagedProxy",
        "CollapsedProxyVisual",
        "ManualReviewRequired"
    )
    Assert-FileContains "Assets/Scripts/P8/P8CollapseProxyRule.cs" @(
        "IsStructuralEngineeringAssessment = false",
        "UsesPhysicsCollapse = false",
        "UsesDebrisSimulation = false",
        "collapseProxyRandomSeed",
        "maxCollapseProxySampleCount"
    )
    Assert-FileContains "Assets/Scripts/P8/P8HumanitarianCandidateHazardStatus.cs" @(
        "isOfficialShelter = false",
        "nonOfficialWarningRequired = true",
        "selectableGameplayEnabled = false",
        "Non-official humanitarian high-rise candidate"
    )
}

function Assert-NoOutOfScopeImplementation {
    $changedFiles = @(Get-ChangedFiles)
    foreach ($file in $changedFiles) {
        if ($file -eq $baselineScene) { continue }
        if ($file -match "^(Assets/(Scripts|Tests)/(P9|P10)|tools/(p9|p10)/|docs/P(9|10))") {
            throw "P8-D must not add P9/P10 systems: $file"
        }
    }

    $scriptFiles = @(
        "P8InfrastructureDamageEvaluator.cs",
        "P8InfrastructureDamageProxy.cs",
        "P8CollapseProxyRule.cs",
        "P8DamageProxyMarker.cs",
        "P8HumanitarianCandidateHazardStatus.cs"
    )
    foreach ($script in $scriptFiles) {
        $text = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repoRoot "Assets\Scripts\P8\$script")
        if ($text -match "EvacuationGameManager|ResultPanelController|ShelterEntranceTrigger|BuildingShelter") {
            throw "$script must not couple to gameplay success/failure or shelter controllers."
        }
        if ($text -match "AddComponent\s*<\s*Rigidbody|new\s+Rigidbody|OnCollision|AddForce|FixedUpdate|Joint") {
            throw "$script contains physics-collapse/debris implementation indicators."
        }
    }

    $p8eCode = @(Get-GitLines @("-C", $repoRoot, "ls-files", "--", "Assets/Scripts/P8/P8E_*", "Assets/Scripts/P8/P8E.*", "Assets/Tests/EditMode/P8/P8E_*", "Assets/Tests/PlayMode/P8/P8E_*"))
    if ($p8eCode.Count -gt 0) {
        throw "P8-E must remain handoff/scope only in this goal: $($p8eCode -join '; ')"
    }
}

function Assert-DataRules {
    $config = Read-Json "Assets/Data/P8/infrastructure_damage_proxy_config.json"
    if ([double]$config.collapseProxyProbability -lt 0 -or [double]$config.collapseProxyProbability -gt 0.1) {
        throw "Collapse proxy probability must remain low."
    }
    if ([bool]$config.noPhysicsCollapse -ne $true -or [bool]$config.noDebrisSimulation -ne $true) {
        throw "P8-D config must disable physics collapse and debris simulation."
    }
    if ([bool]$config.noGameplaySuccessFailureChange -ne $true) {
        throw "P8-D config must preserve gameplay success/failure rules."
    }

    $audit = Read-Json "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json"
    foreach ($record in @($audit.records)) {
        if ([bool]$record.isOfficialShelter -ne $false) {
            throw "Candidate $($record.candidateId) claims official shelter status."
        }
        if ([bool]$record.nonOfficialWarningRequired -ne $true) {
            throw "Candidate $($record.candidateId) does not require non-official warning."
        }
        if ([bool]$record.implementsP9SelectableGameplay -ne $false) {
            throw "Candidate $($record.candidateId) enables P9 selectable gameplay."
        }
    }
}

function Assert-Docs {
    Assert-FileContains "docs/P8D_INFRASTRUCTURE_DAMAGE_BLOCKAGE_PROXY_DESIGN.md" @(
        "lightweight",
        "no real structural damage prediction",
        "no physics collapse",
        "no P9 selectable vertical evacuation gameplay",
        "no gameplay success/failure rule changes"
    )
    Assert-FileContains "docs/P8D_HUMANITARIAN_CANDIDATE_NON_OFFICIAL_LABELING.md" @(
        "isOfficialShelter=false",
        "nonOfficialWarningRequired=true",
        "Not an official evacuation shelter"
    )
    Assert-FileContains "docs/P8D_TO_P8E_HANDOFF.md" @(
        "P8-E",
        "P9",
        "non-official",
        "entrance / safe-floor / evacuation-complete proxy"
    )
}

Write-Host "P8-D preflight: starting"
Write-Host "Repo root: $repoRoot"

$failed = $false
try {
    Assert-Branch
    Assert-P8StageCount
    Assert-ProtectedPaths
    Assert-RequiredArtifacts
    Assert-CodeRules
    Assert-NoOutOfScopeImplementation
    Assert-DataRules
    Assert-Docs

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "validate_p8d_damage_config.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8-D damage config validation failed."
    }

    & powershell -ExecutionPolicy Bypass -File (Join-Path $scriptRoot "inspect_p8d_humanitarian_candidate_status.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "P8-D humanitarian candidate status inspection failed."
    }
}
catch {
    $failed = $true
    Write-Host ""
    Write-Host "P8-D preflight: FAIL" -ForegroundColor Red
    Write-Host $_
}

if ($failed) {
    exit 1
}

Write-Host ""
Write-Host "P8-D preflight: PASS" -ForegroundColor Green
exit 0
