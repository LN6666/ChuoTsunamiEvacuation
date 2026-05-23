# DeepSeek Review Prompt: P8-B Dynamic Risk Front

You are reviewing the staged P8-B git diff for the Unity + PLATEAU Chuo Tsunami Evacuation project.

Task:
Review the dynamic tsunami risk front / cinematic light curtain visualization implementation.

Project stage rules:

- P8 has exactly four stages: P8-A, P8-B, P8-C, and P8-D.
- Do not allow P8-0, P8-E, P8-F, or P8-G.
- P8-B may implement dynamic visual risk-front / cinematic light curtain rendering.
- P8-B must not implement P8-C infrastructure hazard interaction.
- P8-B must not implement P8-D collapse proxy behavior.
- P8-B must not implement P9 crowd/spawn/indoor systems.
- P8-B must not implement P10 packaging.

Protected paths:

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` is the user-approved practical baseline and must be preserved.
- `Assets/Scenes/Chuo_BaseMap.unity` must remain untouched.
- `ProjectSettings` and `Packages` must remain clean.
- `Assets/PLATEAU` must remain untouched.

Review checks:

1. Confirm P8 has exactly four stages.
2. Confirm no P8-C/P8-D/P9/P10 implementation was added.
3. Confirm `Chuo_BaseMap.unity` is untouched.
4. Confirm ProjectSettings/Packages are clean.
5. Confirm `P7_HighDetail_Chuo.unity` baseline was not reset/lost/staged.
6. Confirm risk front is visual only and does not alter gameplay success/failure.
7. Confirm cinematic visual height is clearly not physical tsunami height.
8. Confirm science/data and visual layers remain separate.
9. Confirm large visual height requires `visualHeightIsCinematicOnly=true`.
10. Confirm tests/preflight are present and recorded as passing.
11. Confirm there is no full real-time fluid simulation.
12. Identify compile risks, Unity lifecycle risks, mesh/material risks, and performance risks.

Expected validation before commit:

- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8b_riskfront_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_compat_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`

Output format:

# DeepSeek P8-B Risk Front Review

## A-Level Blockers
List only issues that must block commit/push.

## Warnings
List non-blocking issues and risks.

## Unity-Specific Notes
Mention lifecycle, mesh, material, scene, PlayMode/EditMode, compile, and performance concerns.

## Protected Path Review
State whether the high-detail scene, Chuo_BaseMap, ProjectSettings, Packages, and PLATEAU assets are preserved.

## Stage Boundary Review
State whether P8-B is visual only and no P8-C/P8-D/P9/P10 systems were added.

## Suggested Codex Tasks
Give small follow-up tasks only if needed.

## Verdict
Choose exactly one:
- Safe to commit
- Commit after minor fixes
- Do not commit yet
