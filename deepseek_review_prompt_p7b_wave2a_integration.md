# DeepSeek Review Prompt: P7-B Wave 2-A Integration

Review the P7-B Wave 2-A integration on branch `p7-high-detail-city-foundation`.

Integration source:

- `origin/p7b-wave2a-benchmark-skeleton`

Merge result:

- Fast-forward merge from `ca5ad91` to `01dc3e9`
- No merge conflicts

Important review note:

- The Wave 2-A integration delta is the committed range `HEAD~1..HEAD`.
- The current staged/unstaged diff may only contain this integration review prompt.
- Evaluate the integration using the scope, file list, and validation evidence below.

Integrated files from `git diff --name-only HEAD~1..HEAD`:

```text
Assets/Editor/P7Benchmark.meta
Assets/Editor/P7Benchmark/P7BenchmarkSceneBuilder.cs
Assets/Editor/P7Benchmark/P7BenchmarkSceneBuilder.cs.meta
Assets/Scenes/P7Benchmark.meta
Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity
Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity.meta
Assets/Scripts/P7Benchmark.meta
Assets/Scripts/P7Benchmark/P7BenchmarkMarker.cs
Assets/Scripts/P7Benchmark/P7BenchmarkMarker.cs.meta
Assets/Scripts/P7Benchmark/P7BenchmarkMetricsRecorder.cs
Assets/Scripts/P7Benchmark/P7BenchmarkMetricsRecorder.cs.meta
Assets/Tests/EditMode/P7Benchmark.meta
Assets/Tests/EditMode/P7Benchmark/P7BenchmarkMetricsRecorderTests.cs
Assets/Tests/EditMode/P7Benchmark/P7BenchmarkMetricsRecorderTests.cs.meta
Assets/Tests/PlayMode/P7Benchmark.meta
Assets/Tests/PlayMode/P7Benchmark/P7BenchmarkPlayModeSmokeTests.cs
Assets/Tests/PlayMode/P7Benchmark/P7BenchmarkPlayModeSmokeTests.cs.meta
codex_prompts/p7b_wave2a_benchmark_skeleton.md
deepseek_review_prompt_p7b_wave2a.md
docs/P7B_WAVE2A_BENCHMARK_SCENE_SKELETON.md
docs/P7B_WAVE2A_KNOWN_LIMITATIONS.md
docs/P7B_WAVE2A_METRICS_HARNESS.md
docs/P7B_WAVE2A_TEST_RESULTS.md
docs/P7_DECISION_LOG.md
docs/REVIEW_BACKLOG.md
docs/TASKS.md
tools/p7/check_p7_scope.ps1
tools/p7/create_p7b_benchmark_scene.ps1
tools/p7/run_p7b_wave2a_preflight.ps1
```

Validation already run after integration:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2a_preflight.ps1
```

Result:

- PASS

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
```

Result:

- PASS: total=146 passed=146 failed=0 skipped=0 inconclusive=0

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Result:

- PASS: total=29 passed=29 failed=0 skipped=0 inconclusive=0

Unity GUI generated temporary `ProjectSettings` churn in:

- `ProjectSettings/EditorBuildSettings.asset`
- `ProjectSettings/ProjectSettings.asset`
- `ProjectSettings/ShaderGraphSettings.asset`

That churn was reverted before staging this prompt.

Strict review checklist:

1. Confirm P7 remains exactly five stages:
   - `P7-0`
   - `P7-A`
   - `P7-B`
   - `P7-C`
   - `P7-D`
2. Confirm no `P7-E`, `P7-F`, or `P7-G` stage was created.
3. Confirm only allowed P7Benchmark Unity paths changed:
   - `Assets/Scripts/P7Benchmark/`
   - `Assets/Editor/P7Benchmark/`
   - `Assets/Tests/EditMode/P7Benchmark/`
   - `Assets/Tests/PlayMode/P7Benchmark/`
   - `Assets/Scenes/P7Benchmark/`
4. Confirm allowed non-Unity support paths are limited to:
   - `docs/P7B_WAVE2A_*.md`
   - `docs/TASKS.md`
   - `docs/REVIEW_BACKLOG.md`
   - `docs/P7_DECISION_LOG.md`
   - `tools/p7/check_p7_scope.ps1`
   - `tools/p7/create_p7b_benchmark_scene.ps1`
   - `tools/p7/run_p7b_wave2a_preflight.ps1`
   - `codex_prompts/p7b_wave2a_benchmark_skeleton.md`
   - `deepseek_review_prompt_p7b_wave2a.md`
   - `deepseek_review_prompt_p7b_wave2a_integration.md`
5. Confirm `Assets/Scenes/Chuo_BaseMap.unity` is untouched.
6. Confirm `ProjectSettings/` and `Packages/` are unchanged.
7. Confirm `Assets/PLATEAU/` and `Assets/Data/` are unchanged.
8. Confirm existing gameplay scripts are unchanged.
9. Confirm the `P7Benchmark` scene is isolated.
10. Confirm the metrics harness has no gameplay effect.
11. Confirm no real PLATEAU asset import occurred.
12. Confirm no P8 tsunami hazard, inundation, light curtain, or flood systems were implemented.
13. Confirm no P9 crowd, real spawn, indoor evacuation, congestion, or failure systems were implemented.
14. Confirm Wave 2-B still requires explicit human approval before importing any LOD3 candidate assets.
15. Confirm EditMode and PlayMode tests passed.
16. Confirm Wave 2-A preflight passed.

Return:

- A-level blockers, if any
- B-level risks or follow-ups, if any
- scope/protected-path verdict
- test-result verdict
- recommendation: safe to commit, commit after fixes, or do not commit
