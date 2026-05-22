# DeepSeek Review Prompt - P7-C Streaming / Chunk Loading + Visual Quality + Performance

You are reviewing the P7-C git diff for the Unity + PLATEAU project ChuoTsunamiEvacuation.

Review only the diff. Do not modify files.

P7 has exactly five stages:

- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not approve any P7-E, P7-F, or P7-G expansion.

P7-C scope:

- benchmark sandbox chunk/loading/visual/performance foundation only
- candidate `53393690` already imported under `Assets/P7Benchmark/Imported/53393690/`
- no full Chuo production streaming
- no CityGML conversion unless explicitly present in diff
- no P8 tsunami hazard, inundation, flood, light curtain, or risk-front systems
- no P9 crowd, real spawn, indoor evacuation, congestion, indoor shelter gameplay, or failure systems

Please check:

1. P7 remains exactly five stages.
2. P7-C stays within P7Benchmark sandbox paths and allowed docs/tools/prompts.
3. `Chuo_BaseMap.unity` is untouched.
4. `ProjectSettings` and `Packages` are clean.
5. `Assets/PLATEAU` is unchanged.
6. `Assets/Data` is unchanged.
7. Production scenes are unchanged.
8. Existing gameplay scripts outside P7Benchmark are unchanged.
9. No full Chuo import is introduced.
10. No additional external assets are imported.
11. No P8/P9 systems are implemented.
12. Chunk/loading system has no gameplay success/failure effect.
13. Chunk registry handles raw/unconverted CityGML safely.
14. Placeholder chunk groups are clearly documented as metadata-only if no renderable mesh exists.
15. Metrics are documented as approximate and not a Unity Profiler replacement.
16. Performance methodology covers measure-first workflow, batching/draw-call risk, texture/material risk, mesh memory risk, LOD, culling, collision, and P7-D Windows EXE handoff.
17. EditMode and PlayMode tests are appropriate and passed according to docs or final report.
18. P7-D remains the Windows EXE profiling and final closeout stage.

Classify issues:

- A-level blocker: must fix before commit/push.
- B-level follow-up: acceptable to defer if documented.
- C-level note: minor observation.

Output format:

# DeepSeek P7-C Review

## A-Level Blockers

## B-Level Follow-Ups

## C-Level Notes

## Protected Path Review

## Unity Compile / Lifecycle Review

## Performance Methodology Review

## Test Review

## Overall Verdict

Choose exactly one:

- PASS - no A-level blockers
- CONDITIONAL PASS - no A-level blockers, B-level follow-ups documented
- FAIL - A-level blockers present
