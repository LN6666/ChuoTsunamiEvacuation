# Codex Prompt Trace - P7-C Streaming / Chunk Loading + Visual Quality + Performance

Continue PBL7 using the established PBL4/PBL5/PBL6 workflow.

Project: ChuoTsunamiEvacuation.

Current phase: PBL7 / P7-C.

Task: Streaming / Chunk Loading + Visual Quality + Performance Optimization for the P7Benchmark sandbox.

Requirements:

- Inspect current branch and git status.
- Confirm branch `p7-high-detail-city-foundation`.
- Confirm worktree clean before starting.
- Confirm `Assets/P7Benchmark/Imported/53393690` exists.
- Confirm `Chuo_BaseMap.unity` is untouched.
- Keep P7 exactly five stages: P7-0, P7-A, P7-B, P7-C, P7-D.
- Do not create P7-E, P7-F, or P7-G.
- Do not modify `Chuo_BaseMap.unity`, production scenes, existing gameplay scripts, `Assets/PLATEAU`, `Assets/Data`, `ProjectSettings`, or `Packages`.
- Add strict P7-C scope guard mode.
- Create metadata-driven chunk registry/controller under P7Benchmark paths.
- If CityGML is not directly renderable, use placeholder chunk objects and document conversion pending.
- Add editor tooling to scan `Assets/P7Benchmark/Imported/53393690`.
- Update only `Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity`.
- Extend approximate metrics telemetry with FPS, 1 percent low, active chunk count, imported file/byte summary, and chunk state.
- Add EditMode and PlayMode tests under P7Benchmark test paths.
- Add `tools/p7/run_p7c_preflight.ps1` and `tools/p7/inspect_p7c_benchmark_import.ps1`.
- Create P7-C docs for chunk loading, visual quality, performance optimization, benchmark results, known limitations, and P7-D handoff.
- Update `docs/TASKS.md`, `docs/REVIEW_BACKLOG.md`, and `docs/P7_DECISION_LOG.md`.
- Create `deepseek_review_prompt_p7c.md`.
- Run P7-C preflight, Unity GUI EditMode tests, Unity GUI PlayMode tests, and DeepSeek review.
- Commit and push only if preflight/tests pass, protected paths are clean, and DeepSeek has no A-level blockers.

Performance methodology supplement:

- Measure first, optimize second.
- Use Unity Profiler-style bottleneck categories before claiming optimization success.
- Document draw call and batching risk, SRP Batcher compatibility, static batching tradeoffs, GPU instancing limits, and why dynamic batching is not primary for high-detail city geometry.
- Document texture/material memory risk, mesh memory risk, Memory Profiler workflow, culling strategy, LOD policy, collision strategy, and P7-D Windows EXE handoff.
- Do not add packages.
- Do not modify ProjectSettings.
- Do not claim final optimization success without measured benchmark evidence.

Continuation supplement:

- Continue from local commit `627e131 feat(p7-c): add benchmark chunk loading and performance optimization foundation`.
- Do not push the partial P7-C commit alone.
- Prepare `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` as the P7-C high-detail Chuo scene shell and intended P7-D Windows EXE profiling target.
- Treat PLATEAU SDK screenshots/settings as target import settings, not evidence that assets are loaded.
- Target buildings LOD3, roads LOD3, urban planning decision LOD1, land use import, underground LOD3 if available, city furniture LOD2/LOD3, water LOD1, vegetation LOD3 if available, bridges LOD3, disaster risk import, and relief/terrain import.
- Add LOD coverage, bridge/underground/road feasibility, P2-P6 compatibility, P7-D profiling target, P8/P9/P10 baseline preparation, test tooling, and asset persistence documentation.
- Add read-only validators for high-detail scene readiness, PLATEAU/LOD coverage, and P2-P6 compatibility.
- Keep `Chuo_BaseMap.unity`, production scenes, existing gameplay scripts, `Assets/Data`, `Assets/PLATEAU`, `ProjectSettings`, and `Packages` untouched.
- Do not implement P8, P9, or P10 systems.
