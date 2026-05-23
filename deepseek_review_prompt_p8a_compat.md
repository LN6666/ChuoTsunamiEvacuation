# DeepSeek Review Prompt: P8-A Scene Compatibility Gate

You are reviewing the staged git diff for the Unity + PLATEAU Chuo Tsunami Evacuation project.

Task:
Review P8-A scene compatibility gate changes only.

Project stage rules:

- P8 has exactly four stages: P8-A, P8-B, P8-C, and P8-D.
- Do not allow P8-0, P8-E, P8-F, or P8-G.
- P8-A is a compatibility/documentation/tooling/test gate.
- P8-B light curtain/risk-front visualization must not be implemented yet.
- P8-C hazard interactions must not be implemented yet.
- P8-D collapse proxy behavior must not be implemented yet.
- P9 crowd/spawn/indoor gameplay must not be implemented.
- P10 packaging must not be implemented.

Protected baseline:

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` is the user-approved practical baseline for P8/P9/P10 and must be preserved.
- `Assets/Scenes/Chuo_BaseMap.unity` is legacy fallback and must remain untouched.
- ProjectSettings and Packages must remain untouched.

Review checks:

1. Confirm the diff preserves the P7_HighDetail baseline and does not stage or rewrite the scene.
2. Confirm Chuo_BaseMap remains untouched.
3. Confirm a P2-P6 compatibility smoke gate exists and marks incomplete runtime validation as pending instead of silently passing.
4. Confirm a P8-B scene anchor plan exists and uses `P7_HighDetail_Chuo.unity`, not `Chuo_BaseMap.unity`.
5. Confirm no gameplay success/failure rules changed.
6. Confirm no P8-B, P8-C, or P8-D runtime behavior was implemented.
7. Confirm no P9 or P10 systems were implemented.
8. Confirm tests and preflight scripts cover the compatibility gate and P8 four-stage boundary.
9. Confirm P8 still has exactly four stages.
10. Identify compile risks, Unity lifecycle risks, path handling risks, and test fragility.

Expected validation before commit:

- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_compat_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`

Output format:

# DeepSeek P8-A Compatibility Review

## A-Level Blockers
List only issues that must block commit.

## Warnings
List non-blocking issues and risks.

## Unity-Specific Notes
Mention compile, asmdef, EditMode/PlayMode, scene, lifecycle, and generated-file concerns.

## Protected Path Review
State whether P7_HighDetail, Chuo_BaseMap, ProjectSettings, and Packages are preserved.

## Suggested Codex Tasks
Give small follow-up tasks only if needed.

## Verdict
Choose exactly one:
- Safe to commit
- Commit after minor fixes
- Do not commit yet
