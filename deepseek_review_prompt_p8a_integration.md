# DeepSeek Review Prompt: P8-A Evidence Hardening Integration

You are reviewing the staged merge/integration diff for the Unity + PLATEAU Chuo Tsunami Evacuation project.

Task:
Review the integration of `origin/p8a-hazard-evidence-hardening` into `p8-tsunami-hazard-risk-front-foundation`.

Project stage rules:

- P8 has exactly four stages: P8-A, P8-B, P8-C, and P8-D.
- Do not allow P8-0, P8-E, P8-F, or P8-G.
- P8-A is a data, evidence, compatibility, tooling, and test foundation.
- P8-B dynamic risk front / cinematic light curtain visualization must not be implemented yet.
- P8-C infrastructure hazard interaction must not be implemented yet.
- P8-D collapse proxy behavior must not be implemented yet.
- P9 crowd, spawn, indoor shelter, indoor evacuation, congestion, or failure systems must not be implemented.
- P10 packaging must not be implemented.

Protected baseline:

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` is the user-approved practical baseline for P8/P9/P10.
- The high-detail scene may be locally dirty and must not be reset, overwritten, deleted, cleaned, or staged.
- `Assets/Scenes/Chuo_BaseMap.unity` is legacy fallback and must remain untouched.
- `ProjectSettings` and `Packages` must remain clean.

Review checks:

1. Confirm P8 still has exactly four stages.
2. Confirm P8Compat outputs are preserved:
   - `docs/P8A_SCENE_COMPATIBILITY_GATE.md`
   - `docs/P8A_P7_HIGHDETAIL_BASELINE_STATUS.md`
   - `docs/P8A_P2_P6_COMPATIBILITY_SMOKE_REPORT.md`
   - `docs/P8A_P8B_SCENE_ANCHOR_PLAN.md`
   - `tools/p8/run_p8a_compat_preflight.ps1`
   - P8 scene compatibility EditMode/PlayMode tests.
3. Confirm P8Evidence outputs are preserved:
   - evidence registry
   - official source review protocol
   - hazard variable definitions
   - arrival/depth/boundary model
   - cinematic light curtain rules
   - hardened schema/configs/tests.
4. Confirm `P7_HighDetail_Chuo.unity` baseline was not reset, lost, or staged.
5. Confirm `Chuo_BaseMap.unity` remains untouched.
6. Confirm `ProjectSettings` and `Packages` are clean.
7. Confirm no P8-B/P8-C/P8-D implementation was introduced.
8. Confirm no P9/P10 systems were introduced.
9. Confirm the science/data layer and cinematic visual layer remain separated.
10. Confirm large cinematic visual height requires `visualHeightIsCinematicOnly=true`.
11. Confirm the P2-P6 compatibility gate exists and marks pending runtime checks clearly.
12. Confirm tests/preflights are recorded as passing.

Expected validation:

- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_preflight.ps1` PASS
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_compat_preflight.ps1` PASS
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui` PASS
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui` PASS

Output format:

# DeepSeek P8-A Integration Review

## A-Level Blockers
List only issues that must block commit/push.

## Warnings
List non-blocking issues and risks.

## Protected Path Review
State whether `P7_HighDetail_Chuo`, `Chuo_BaseMap`, `ProjectSettings`, and `Packages` are preserved.

## P8Compat Preservation
State whether the scene compatibility gate, P2-P6 smoke report, P8-B anchor plan, and compatibility tooling/tests remain intact.

## P8Evidence Preservation
State whether evidence registry/protocol/docs/schema/config/test hardening remains intact.

## Stage Boundary Review
State whether no P8-B/P8-C/P8-D, P9, or P10 implementation was added.

## Suggested Codex Tasks
List small follow-up tasks only if needed.

## Verdict
Choose exactly one:
- Safe to commit
- Commit after minor fixes
- Do not commit yet
