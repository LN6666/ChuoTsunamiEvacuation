# DeepSeek Review Prompt: P8-B Guard

You are reviewing the P8-B Risk Front Validation, Performance Guard, and Review Hardening changes for the Unity + PLATEAU Chuo tsunami evacuation prototype.

Review the git diff on branch `p8b-riskfront-validation-hardening` against `origin/p8-tsunami-hazard-risk-front-foundation`.

## Required Verdict

Classify findings as:

- A-level blocker: must fix before commit/push/integration.
- B-level issue: should fix soon, but may not block this guard branch.
- C-level note: minor cleanup or follow-up.

Give a final integration verdict: safe to commit/push, safe with B/C follow-up, or blocked.

## Must Check

- No scene mutation, especially no `P7_HighDetail_Chuo` or `Chuo_BaseMap` changes.
- No P8-B visual scene implementation here: no light curtain scene objects, runtime renderer wiring, particles, mesh renderer, line renderer, or scene assets.
- No protected paths changed: `ProjectSettings`, `Packages`, `Library`, `Temp`, `Logs`, `Builds`, raw PLATEAU data, generated PLATEAU scene data, review reports, or logs.
- No P9/P10 systems, namespaces, tooling, docs, or gameplay behavior.
- Validation, tests, and performance guard exist:
  - `tools/p8/validate_p8b_riskfront_config.ps1`
  - `tools/p8/run_p8b_guard_preflight.ps1`
  - `tools/p8/inspect_p8b_visual_performance_risk.ps1`
  - P8 EditMode/PlayMode tests
  - `docs/P8B_PERFORMANCE_GUARD.md`
- Science vs visual separation is enforced:
  - `visualHeightMeters` is cinematic only.
  - `visualHeightMeters` is not confused with `tsunamiHeightMeters`, `waterLevelMeters`, or inundation depth.
  - Large visual height requires `visualHeightIsCinematicOnly=true`.
  - `sourceMode` must not claim official values.
  - `boundaryIsEvidenceBasedOrPrototype` remains explicit.
- Collapse proxy remains disabled or data-only until P8-D.
- P8-B does not implement P8-C road/building/bridge/underground interactions.
- P8 has exactly five stages: P8-A, P8-B, P8-C, P8-D, and P8-E.
- Tests/preflight pass:
  - `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8b_guard_preflight.ps1`
  - `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_preflight.ps1`
  - `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
  - `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`

## Scope Reminder

This branch may create/update only:

- `docs/P8B_*.md`
- `tools/p8/*p8b*.ps1`
- `Assets/Scripts/P8/`
- `Assets/Tests/EditMode/P8/`
- `Assets/Tests/PlayMode/P8/`
- `Assets/Data/P8/`
- `codex_prompts/p8b_riskfront_validation_hardening.md`
- `deepseek_review_prompt_p8b_guard.md`

Do not recommend expanding scope into visual scene implementation, full fluid simulation, infrastructure interaction, collapse proxy gameplay, P9, or P10.
