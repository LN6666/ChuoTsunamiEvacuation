# DeepSeek Review Prompt - P7-C Streaming / Chunk Loading + High-Detail Scene Readiness

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

- benchmark sandbox chunk/loading/visual/performance foundation
- high-detail Chuo scene readiness for P7-D profiling
- candidate `53393690` already imported under `Assets/P7Benchmark/Imported/53393690/`
- target high-detail scene: `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- no full Chuo production streaming unless explicitly and safely documented
- no CityGML conversion unless explicitly present in diff
- no P8 tsunami hazard, inundation, flood, light curtain, or risk-front systems
- no P9 crowd, real spawn, indoor evacuation, congestion, indoor shelter gameplay, or failure systems

Please check:

1. P7 remains exactly five stages.
2. P7-C stays within P7Benchmark/P7HighDetail sandbox paths and allowed docs/tools/prompts.
3. `Chuo_BaseMap.unity` is untouched.
4. `ProjectSettings` and `Packages` are clean.
5. `Assets/PLATEAU` is unchanged.
6. `Assets/Data` is unchanged.
7. Production scenes are unchanged.
8. Existing gameplay scripts outside P7Benchmark are unchanged.
9. P7-C is not just a skeleton optimization task; it addresses high-detail Chuo scene readiness.
10. P7-D profiling target is not the old skeleton/test scene.
11. If `P7_HighDetail_Chuo.unity` contains only placeholders, docs clearly state that final profiling needs actual PLATEAU assets.
12. No full Chuo import is introduced unless it is isolated under approved P7 paths and documented.
13. No additional external assets are imported outside approved paths.
14. No P8/P9 systems are implemented.
15. Chunk/loading system has no gameplay success/failure effect.
16. Chunk registry handles raw/unconverted CityGML safely.
17. Placeholder chunk groups are clearly documented as metadata-only if no renderable mesh exists.
18. P2-P6 compatibility validation or smoke-test plan is present.
19. New high-detail scene is prepared as the intended P8/P9/P10 baseline, with final confirmation deferred to P7-D.
20. Testing tools were upgraded for high-detail scene readiness, LOD coverage, P2-P6 compatibility, and P7-D profiling target validation.
21. Asset persistence strategy is documented, including GitHub versus cloud-drive/release/LFS responsibilities.
22. LOD coverage claims are evidence-based.
23. Average LOD3 is not claimed unless verified by actual scene/import evidence.
24. Bridge, underground, and road loading status clearly distinguishes target settings from actual loaded/renderable evidence.
25. Metrics are documented as approximate and not a Unity Profiler replacement.
26. Performance methodology covers measure-first workflow, Unity Profiler, Memory Profiler, Frame Debugger, batching/draw-call risk, texture/material risk, mesh memory risk, LOD, culling, collision, and P7-D Windows EXE handoff.
27. EditMode and PlayMode tests are appropriate and passed according to docs or final report if Unity files changed.
28. Large generated/cache/build/profiler/temp outputs are not committed.
29. P7-D remains the Windows EXE profiling and final closeout stage.

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
