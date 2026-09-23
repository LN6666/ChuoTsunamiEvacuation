# Codex Prompt: P7-B Wave 2-A — Isolated Benchmark Scene Skeleton + Metrics Harness

You are working in:
D:\UnityProjects\ChuoTsunamiEvacuation-P7BWave2A

Branch:
p7b-wave2a-benchmark-skeleton

Phase:
PBL7 / P7-B Wave 2-A

Main objective:
Create a minimal, isolated P7 benchmark scene skeleton and metrics harness for future small-area high-detail LOD3 benchmark work.

This is the first P7-B step allowed to touch Unity project files, but only inside explicitly allowed P7Benchmark paths.

P7 stage count:
P7 has exactly five stages:
- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not create P7-E, P7-F, or P7-G.

Strict prohibitions:
- Do not modify Chuo_BaseMap.unity.
- Do not modify existing Unity scenes.
- Do not modify existing gameplay scripts.
- Do not modify Assets/PLATEAU.
- Do not modify Assets/Data.
- Do not modify ProjectSettings.
- Do not modify Packages.
- Do not import real PLATEAU assets.
- Do not download data.
- Do not add external dependencies.
- Do not implement P8 tsunami hazard, inundation, light curtain, or flood systems.
- Do not implement P9 crowd, real spawn, indoor evacuation, congestion, or failure systems.
- Do not change gameplay success/failure rules.

Allowed Unity paths:
- Assets/Scripts/P7Benchmark/
- Assets/Editor/P7Benchmark/
- Assets/Tests/EditMode/P7Benchmark/
- Assets/Tests/PlayMode/P7Benchmark/
- Assets/Scenes/P7Benchmark/

Allowed docs/tools/prompts:
- docs/P7B_WAVE2A_BENCHMARK_SCENE_SKELETON.md
- docs/P7B_WAVE2A_METRICS_HARNESS.md
- docs/P7B_WAVE2A_TEST_RESULTS.md
- docs/P7B_WAVE2A_KNOWN_LIMITATIONS.md
- tools/p7/create_p7b_benchmark_scene.ps1
- tools/p7/run_p7b_wave2a_preflight.ps1
- codex_prompts/p7b_wave2a_benchmark_skeleton.md
- deepseek_review_prompt_p7b_wave2a.md

Allowed updates:
- docs/TASKS.md
- docs/REVIEW_BACKLOG.md
- docs/P7_DECISION_LOG.md
- tools/p7/check_p7_scope.ps1 only to add a strict P7-B Wave 2-A allowlist mode.
- tools/p7/run_p7_preflight.ps1 only if needed, but do not weaken default strictness.

Implementation requirements:

1. Add a strict P7-B Wave 2-A scope mode
Update tools/p7/check_p7_scope.ps1 carefully so default P7 preflight remains strict.
Add a parameter or mode for Wave 2-A that allows only:
- Assets/Scripts/P7Benchmark/
- Assets/Editor/P7Benchmark/
- Assets/Tests/EditMode/P7Benchmark/
- Assets/Tests/PlayMode/P7Benchmark/
- Assets/Scenes/P7Benchmark/

Even in Wave 2-A mode, still fail on:
- ProjectSettings/
- Packages/
- Assets/Scenes/Chuo_BaseMap.unity
- Assets/PLATEAU/
- Assets/Data/
- existing Assets/Scripts outside Assets/Scripts/P7Benchmark/
- existing scenes outside Assets/Scenes/P7Benchmark/

2. Create metrics harness
Add runtime scripts under Assets/Scripts/P7Benchmark/:
- P7BenchmarkMarker.cs
- P7BenchmarkMetricsRecorder.cs

Requirements:
- no external dependencies
- no gameplay rule integration
- no references to P8/P9 systems
- metrics recorder should support frame time collection, average FPS, approximate 1% low FPS calculation, elapsed time, sample count, and exportable summary string
- safe if disabled
- no scene dependency

3. Create editor scene builder
Add editor-only script under Assets/Editor/P7Benchmark/:
- P7BenchmarkSceneBuilder.cs

Requirements:
- creates an isolated benchmark skeleton scene at:
  Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity
- scene contains only simple primitive placeholders, camera, light, and P7 benchmark marker/metrics recorder
- no PLATEAU asset import
- no Chuo_BaseMap reference
- no gameplay manager dependency
- command-line callable static method if feasible

4. Add tests
Add EditMode tests under Assets/Tests/EditMode/P7Benchmark/:
- P7BenchmarkMetricsRecorderTests.cs

Tests should verify:
- metrics summary can be generated
- 1% low calculation handles empty/small samples safely
- no dependency on gameplay managers
- marker constants / labels are stable

Add PlayMode tests only if lightweight and safe:
- P7BenchmarkPlayModeSmokeTests.cs

PlayMode tests should avoid requiring the generated scene if scene creation is not reliable in CI.
Prefer testing components in a temporary runtime GameObject.

5. Add PowerShell automation
Add:
- tools/p7/create_p7b_benchmark_scene.ps1
- tools/p7/run_p7b_wave2a_preflight.ps1

create_p7b_benchmark_scene.ps1 should:
- call Unity Editor command-line method if possible
- otherwise clearly report that scene creation requires Unity invocation
- never modify ProjectSettings or Packages
- never touch Chuo_BaseMap

run_p7b_wave2a_preflight.ps1 should:
- run scope guard in Wave 2-A allowlist mode
- check protected paths
- run or validate benchmark scene creation if feasible
- print PASS/FAIL
- exit non-zero on failure

6. Documentation
Create:
- docs/P7B_WAVE2A_BENCHMARK_SCENE_SKELETON.md
- docs/P7B_WAVE2A_METRICS_HARNESS.md
- docs/P7B_WAVE2A_TEST_RESULTS.md
- docs/P7B_WAVE2A_KNOWN_LIMITATIONS.md

Document:
- Wave 2-A does not import real assets
- scene is isolated
- Chuo_BaseMap is untouched
- metrics are prototype benchmark metrics, not final profiler replacement
- LOD3 candidate import is deferred to later Wave 2-B / future task
- LOD4 is not assumed available
- EXE profiling remains later, not completed here

7. Update docs/TASKS.md
Mark P7-B Wave 2-A as in progress / implemented, but do not create P7-E/F/G.

8. Update docs/REVIEW_BACKLOG.md
Add risks:
- metrics recorder is approximate, not a replacement for Unity Profiler
- benchmark scene skeleton has no real PLATEAU geometry yet
- Wave 2-B needs explicit approval before importing LOD3 candidate data
- Unity scene creation may need GUI/batchmode validation
- ProjectSettings line-ending churn must be reverted if Unity launches modify it

9. Update docs/P7_DECISION_LOG.md
Add a decision:
P7-B Wave 2-A is approved only for isolated benchmark skeleton and metrics harness under P7Benchmark paths.
No real asset import is approved.

10. DeepSeek review prompt
Create deepseek_review_prompt_p7b_wave2a.md checking:
- only allowed P7Benchmark Unity paths changed
- Chuo_BaseMap untouched
- no ProjectSettings/Packages changed
- no Assets/PLATEAU or Assets/Data changed
- no existing gameplay scripts changed
- metrics harness has no gameplay effect
- scene skeleton is isolated
- no real asset import
- P7 remains exactly five stages
- P8/P9 not implemented
- EditMode/PlayMode tests passed or failures are clearly explained
- P7-B Wave 2-B still requires approval before importing LOD3 assets

After implementation:
Run:
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2a_preflight.ps1

Then run Unity tests:
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui

If Unity causes ProjectSettings or Packages churn, revert it unless explicitly required.

Then report:
- Wave 2-A preflight result
- EditMode result
- PlayMode result
- git diff --name-only
- protected path check
- whether Chuo_BaseMap was untouched
- recommended commit message
