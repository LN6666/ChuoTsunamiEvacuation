# DeepSeek Review Prompt - P7-D Final Postcheck Practical Baseline

You are reviewing the P7-D final postcheck git diff for the Unity + PLATEAU project ChuoTsunamiEvacuation.

Review only the staged diff. Do not modify files.

P7 has exactly five stages:

- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not approve P7-E, P7-F, or P7-G.

Please check:

1. P7-D practical baseline wording is correct.
2. Lower-than-original-target LOD is documented as a limitation, not as full LOD3.
3. `P7_HighDetail_Chuo.unity` is documented as the user-approved practical baseline for P8/P9/P10.
4. `Chuo_BaseMap.unity` remains a legacy fallback and is untouched.
5. Limitations remain clearly documented, including missing/partial layers.
6. No false average LOD3 claim appears.
7. No false full-category high-detail import claim appears.
8. P8/P9 handoff is clear: P8 can start from `P7_HighDetail_Chuo`; P9 can use markers, rule-based nodes, proxy colliders, and representative templates where detailed geometry is missing.
9. P7 has exactly five stages and no P7-E/F/G expansion.
10. No P8 tsunami hazard, inundation, light curtain, flood, or risk-front system was implemented.
11. No P9 crowd, real spawn, indoor evacuation, congestion, indoor shelter gameplay, or failure system was implemented.
12. Gameplay success/failure rules are unchanged.
13. `ProjectSettings` and `Packages` are clean or reverted.
14. `Assets/Data` and `Assets/PLATEAU` are not unsafely modified.
15. P2-P6 compatibility is honestly documented.
16. Windows EXE profiling result or blocker is honestly documented and no runtime metrics are fabricated.
17. Asset archive/recovery plan exists and covers cloud-drive preservation before VM deletion.
18. Unity tests passed or any retry/blocker is honestly documented.
19. No cache/build/profiler/temp outputs are committed.
20. The 22.55 GB imported scene is not included in the staged diff unless an explicit large-asset strategy is documented.

Classify issues:

- A-level blocker: must fix before commit/push.
- B-level follow-up: acceptable to defer if documented.
- C-level note: minor observation.

Output format:

# DeepSeek P7-D Final Postcheck Review

## A-Level Blockers

## B-Level Follow-Ups

## C-Level Notes

## Protected Path Review

## Practical Baseline Review

## Import / LOD Evidence Review

## Compatibility Review

## Profiling Review

## Asset Archive Review

## Test Review

## Overall Verdict

Choose exactly one:

- PASS - no A-level blockers
- CONDITIONAL PASS - no A-level blockers, B-level follow-ups documented
- FAIL - A-level blockers present
