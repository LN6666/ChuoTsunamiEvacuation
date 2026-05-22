# DeepSeek Review Prompt - P7-D Final High-Detail Import / Optimization / Closeout

You are reviewing the P7-D git diff for the Unity + PLATEAU project ChuoTsunamiEvacuation.

Review only the diff. Do not modify files.

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
3. High-detail PLATEAU import is actually complete, or honestly blocked with a manual checklist.
4. If import is blocked, P7 final closeout is not falsely marked complete.
5. `P7_HighDetail_Chuo.unity` is not described as a complete high-detail baseline unless actual renderable assets exist.
6. Average LOD3 is not claimed unless verified by evidence.
7. Bridge, underground, road, water, terrain, city furniture, disaster risk, land use, and urban planning status is evidence-based.
8. P2-P6 compatibility is validated or blockers are listed.
9. Windows EXE profiling is done or blockers are honestly documented.
10. New map baseline decision is explicit.
11. P8/P9 handoff is clear.
12. `Chuo_BaseMap.unity` remains untouched as legacy fallback.
13. `ProjectSettings` and `Packages` are clean or justified.
14. `Assets/Data` and `Assets/PLATEAU` are unchanged.
15. No P8/P9 systems are implemented.
16. Gameplay success/failure rules are unchanged.
17. Tests/preflight results are documented.
18. Protected paths are clean.
19. Asset persistence/cloud archive strategy exists.
20. No large generated/cache/build/profiler/temp outputs are committed.

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
