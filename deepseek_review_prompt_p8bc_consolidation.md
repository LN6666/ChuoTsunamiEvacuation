# DeepSeek Review Prompt: P8-B/C Consolidation

You are reviewing the staged git diff for the Unity + PLATEAU Chuo Tsunami Evacuation project.

Task:

Review P8-B/C consolidation gate and humanitarian high-rise candidate audit before P8-D.

Check these A-level blocker areas:

1. P8 stage count is exactly A-E only. P8-0, P8-F, and P8-G must not exist as stages.
2. P8-B first concern is consolidated using the official Tokyo Metropolitan Government tsunami spatial layer v1, not generic flood proxy data.
3. The official tsunami check covers Taisho Kanto, Nankai Trough case 1, and explicitly states case 5/case 8 are not present if not extracted.
4. Chuo samples, max inundation depth, max tsunami height, arrival-time existence, and derived-boundary status are documented.
5. `maxTsunamiHeightMeters` remains separate from `inundationDepthMeters`.
6. `visualHeightMeters` remains cinematic only.
7. `inundationBoundary` is not falsely claimed as a complete official contour when derived from points/grid extent.
8. P8-C second/third concerns are consolidated at smoke/proxy level with clear D/E/P9 handoff.
9. Humanitarian high-rise candidate name list is generated from actual project files, not invented.
10. Non-official candidate disclaimer is explicit and candidates are not claimed as official shelters.
11. D/E/P9 allocation is documented but not implemented here.
12. No P8-D collapse proxy implementation was added.
13. No P9 gameplay implementation was added.
14. `Chuo_BaseMap.unity` is not modified.
15. `P7_HighDetail_Chuo.unity` local baseline is preserved and not staged.
16. ProjectSettings and Packages are clean.
17. P8-B/C consolidation preflight, relevant P8 preflights, EditMode tests, and PlayMode tests pass.

Expected validation before commit:

- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8bc_consolidation_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_compat_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8b_riskfront_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8b_guard_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8b_front_v1_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8b_evidence_spatial_gate.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8c_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`

Output format:

# DeepSeek P8-B/C Consolidation Review

## A-Level Blockers
List only issues that must block commit.

## B-Level Follow-Ups
List non-blocking issues and risks.

## Evidence Review
State whether official Tokyo tsunami evidence, scenario extraction, field separation, and boundary limitations are correct.

## Humanitarian Candidate Review
State whether candidate names come from project files and non-official labeling is explicit.

## Protected Path Review
State whether Chuo_BaseMap, P7_HighDetail, ProjectSettings, Packages, and PLATEAU paths are preserved.

## Verdict
Choose exactly one:
- Safe to commit
- Commit after minor fixes
- Do not commit yet
