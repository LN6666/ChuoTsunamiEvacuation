# DeepSeek Review Prompt - P7-D Final High-Detail Import / Optimization / Closeout

You are reviewing the P7-D git diff for the Unity + PLATEAU project ChuoTsunamiEvacuation.

Review only the staged diff. Do not modify files.

P7 has exactly five stages:

- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not approve P7-E, P7-F, or P7-G.

Please check:

1. P7 has exactly five stages.
2. No P7-E/F/G expansion is created.
3. The manual PLATEAU import result is honestly validated.
4. Actual LOD mismatch is documented; average LOD3 is not claimed unless verified.
5. Categories imported/missing/partial are clearly listed.
6. `P7_HighDetail_Chuo.unity` is not falsely approved as a full high-detail baseline.
7. P2-P6 compatibility is validated or blockers are listed.
8. Windows EXE profiling is done or blockers/prepared manual steps are honestly documented.
9. New map baseline decision is explicit and evidence-based.
10. P8/P9 handoff is clear.
11. `Chuo_BaseMap.unity` remains untouched.
12. `ProjectSettings` and `Packages` are clean or justified.
13. `Assets/Data` and `Assets/PLATEAU` are unchanged.
14. No P8/P9 systems are implemented.
15. Gameplay success/failure rules are unchanged.
16. Tests/preflight results are documented.
17. Protected paths are clean.
18. Asset persistence/cloud archive strategy exists.
19. No large generated/cache/build/profiler/temp outputs are committed.
20. The 22.55 GB imported scene is not included in the staged diff unless an explicit large-asset strategy is documented.

Classify issues:

- A-level blocker: must fix before commit/push.
- B-level follow-up: acceptable to defer if documented.
- C-level note: minor observation.

Output format:

# DeepSeek P7-D Final Review

## A-Level Blockers

## B-Level Follow-Ups

## C-Level Notes

## Protected Path Review

## Import / LOD Evidence Review

## Compatibility Review

## Profiling Review

## Baseline / Handoff Review

## Test Review

## Overall Verdict

Choose exactly one:

- PASS - no A-level blockers
- CONDITIONAL PASS - no A-level blockers, B-level follow-ups documented
- FAIL - A-level blockers present
