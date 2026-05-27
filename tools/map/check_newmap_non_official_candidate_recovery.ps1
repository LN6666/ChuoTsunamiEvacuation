param(
    [string]$ProjectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
)

$ErrorActionPreference = "Stop"

$candidatePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_non_official_candidate_recovery.json"
$activePath = Join-Path $ProjectRoot "Assets\Data\P10\newmap_active_target_final_report.json"
$runtimePath = Join-Path $ProjectRoot "Assets\Resources\NewMap\newmap_runtime_non_official_candidates.json"
$failures = @()

foreach ($path in @($candidatePath, $activePath, $runtimePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures += "Missing candidate recovery input: $path"
    }
}

if ($failures.Count -eq 0) {
    $candidate = Get-Content -LiteralPath $candidatePath -Raw | ConvertFrom-Json
    $active = Get-Content -LiteralPath $activePath -Raw | ConvertFrom-Json
    $runtime = Get-Content -LiteralPath $runtimePath -Raw | ConvertFrom-Json

    if ([int]$candidate.totalCandidateRecordsLoaded -lt 100) {
        $failures += "Candidate recovery did not process the expected P8/P9 handoff scale."
    }

    if ([int]$candidate.finalActiveNonOfficialCandidateCount -le 4 -and [string]::IsNullOrWhiteSpace($candidate.largeInactiveGroupExplanation)) {
        $failures += "Active non-official count remains at/below 4 without detailed evidence."
    }

    if ([int]$candidate.finalActiveNonOfficialCandidateCount -ne [int]$active.activeNonOfficialHumanitarianCandidateCount) {
        $failures += "Candidate recovery and active target report disagree on active recovered candidate count."
    }

    if (@($runtime.records).Count -ne [int]$candidate.finalActiveNonOfficialCandidateCount) {
        $failures += "Runtime Resources candidate cache does not match active recovered candidate count."
    }

    $badActive = @($candidate.records | Where-Object {
        $_.activeInGame -and (
            $_.isOfficialShelter -or
            -not $_.nonOfficialWarningRequired -or
            $_.safeApprovedByDefault -or
            $_.warningBehavior -notmatch "Non-official" -or
            $_.resultPanelBehavior -notmatch "warning"
        )
    })
    if ($badActive.Count -gt 0) {
        $failures += "Active non-official candidates have unsafe semantics: $($badActive.id -join ', ')"
    }

    $badCoordinateProxy = @($candidate.records | Where-Object {
        $_.activeInGame -and
        $_.classification -eq "active_coordinate_proxy_anchor" -and
        ($_.warningBehavior -notmatch "Non-official" -or -not $_.insideChuoBaseMapBounds)
    })
    if ($badCoordinateProxy.Count -gt 0) {
        $failures += "Coordinate-proxy anchors are active without warning or map-bounds evidence: $($badCoordinateProxy.id -join ', ')"
    }

    $badRuntime = @($runtime.records | Where-Object {
        $_.isOfficialShelter -or -not $_.nonOfficialWarningRequired -or $_.safeApprovedByDefault -or -not $_.activeInGame
    })
    if ($badRuntime.Count -gt 0) {
        $failures += "Runtime recovered candidate cache contains bad semantics: $($badRuntime.id -join ', ')"
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "[FAIL] $_" }
    exit 1
}

Write-Host "[PASS] Non-official humanitarian candidate recovery is evidence-backed and warning-preserving."
exit 0
