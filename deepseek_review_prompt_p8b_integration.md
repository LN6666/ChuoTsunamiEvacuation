# DeepSeek Review Prompt: P8-B Integration

Review the merge of `origin/p8b-riskfront-validation-hardening` into `p8-tsunami-hazard-risk-front-foundation`.

## Context

The main P8 branch already contains Codex A / P8RiskFront:

- Dynamic cinematic tsunami risk front implementation.
- Scene mutation deferred.
- Manual scene anchor guidance in `docs/P8B_SCENE_ANCHOR_REPORT.md`.
- P7 high-detail baseline scene preserved as local state.

The integrated branch adds Codex B / P8BGuard:

- Risk-front config validation hardening.
- Performance guard and inspector.
- Guard tests.
- P8-C handoff checklist.

## Required Review

Classify findings as:

- A-level blocker: must fix before push/integration.
- B-level issue: should fix soon but does not block.
- C-level note: minor cleanup or follow-up.

Give a final verdict: blocked, safe to push, or safe to push with B/C follow-up.

## Must Check

- P8 has exactly four stages: P8-A, P8-B, P8-C, and P8-D.
- P8RiskFront outputs are preserved:
  - `P8RiskFrontController`
  - `P8RiskFrontVisualConfig`
  - curve generation, time driver, light curtain renderer, debug status
  - P8-B risk-front tests and docs
- P8BGuard outputs are preserved:
  - `tools/p8/validate_p8b_riskfront_config.ps1`
  - `tools/p8/run_p8b_guard_preflight.ps1`
  - `tools/p8/inspect_p8b_visual_performance_risk.ps1`
  - `P8RiskFrontPerformanceGuard`
  - guard tests and P8-C handoff docs
- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` baseline was not reset, lost, staged, or overwritten.
- `Assets/Scenes/Chuo_BaseMap.unity` is untouched.
- `ProjectSettings` and `Packages` are clean.
- No P8-C infrastructure hazard interaction is implemented yet.
- No P8-D collapse proxy gameplay is implemented yet.
- No P9/P10 systems are introduced.
- Science/data layer and cinematic visual layer remain separated.
- Large cinematic visual height requires `visualHeightIsCinematicOnly=true`.
- No official hazard value claim is introduced.
- Dynamic risk front remains sample/config-driven unless reviewed evidence data is supplied.
- Risk-front scene anchor remains manual/deferred.
- Tests and preflights pass:
  - `tools/p8/run_p8b_riskfront_preflight.ps1`
  - `tools/p8/run_p8b_guard_preflight.ps1`
  - `tools/p8/run_p8a_preflight.ps1`
  - `tools/p8/run_p8a_compat_preflight.ps1`
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
