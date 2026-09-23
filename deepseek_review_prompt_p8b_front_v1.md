# DeepSeek Review Prompt: P8-B Hazard-Layer-Driven Risk Front V1

Review the P8-B update on `p8-tsunami-hazard-risk-front-foundation`.

## Context

P8-B already includes:

- Codex A risk-front implementation.
- Codex B guard validation/performance hardening.
- Integrated P8BGuard merge commit on the main P8 branch.

This update finalizes P8-B front v1 so the dynamic risk front is driven by hazard-layer records instead of only sample animation. The current data is still manual/evidence-planned unless reviewed official or academic evidence is added.

## Required Review

Classify findings as:

- A-level blocker: must fix before commit/push.
- B-level issue: should fix soon but does not block.
- C-level note: minor cleanup or follow-up.

Give a final verdict: blocked, safe to commit, or safe to commit with B/C follow-up.

## Must Check

- P8 has exactly five stages: P8-A, P8-B, P8-C, P8-D, and P8-E.
- P8BGuard integration remains complete and guard outputs are preserved.
- The risk front is hazard-layer-driven v1, not purely sample animation.
- `arrivalTimeSeconds`, `inundationBoundary`, `inundationDepthMeters`, and `hazardIntensity` drive behavior where data is available.
- Missing or incomplete boundary data falls back safely without claiming evidence-based geometry.
- `confidence`, `evidenceSourceId`, and `sourceMode` are surfaced for provenance.
- No false official or academic hazard-data claim is introduced.
- Official/academic source modes require reviewed metadata and evidence source notes.
- Cinematic `visualHeightMeters` remains separate from physical `tsunamiHeightMeters` or `waterLevelMeters`.
- Large cinematic visual height still requires `visualHeightIsCinematicOnly=true`.
- The dynamic risk front is manual-sample/config-driven unless evidence data is supplied.
- No real-time/full tsunami fluid simulation is implemented or claimed.
- No P8-C infrastructure hazard interaction is implemented yet.
- No P8-D collapse proxy gameplay is implemented yet.
- No P9/P10 systems are introduced.
- Gameplay success/failure rules are unchanged.
- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` baseline was not reset, lost, staged, or overwritten.
- `Assets/Scenes/Chuo_BaseMap.unity` is untouched.
- `ProjectSettings` and `Packages` are clean.
- Tests and preflights pass:
  - `tools/p8/run_p8b_front_v1_preflight.ps1`
  - `tools/p8/run_p8b_riskfront_preflight.ps1`
  - `tools/p8/run_p8b_guard_preflight.ps1`
  - Unity EditMode GUI tests
  - Unity PlayMode GUI tests

## Protected Paths

Do not recommend committing or modifying:

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- `Assets/Scenes/Chuo_BaseMap.unity`
- `ProjectSettings/`
- `Packages/`
- raw PLATEAU data
- generated logs or review reports

The high-detail scene may remain dirty as local baseline state, but it must not be staged.
