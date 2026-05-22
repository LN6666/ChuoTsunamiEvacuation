# DeepSeek Review Prompt: P7-B Wave 2-A Benchmark Skeleton

Review the current git diff for P7-B Wave 2-A.

Scope approved for this wave:

- isolated benchmark scene skeleton
- benchmark marker and metrics recorder under `P7Benchmark` paths
- focused EditMode/PlayMode tests
- Wave 2-A scope/preflight automation
- docs, tasks, review backlog, P7 decision log, and this review prompt

Strict review checklist:

1. Confirm only allowed P7Benchmark Unity paths changed:
   - `Assets/Scripts/P7Benchmark/`
   - `Assets/Editor/P7Benchmark/`
   - `Assets/Tests/EditMode/P7Benchmark/`
   - `Assets/Tests/PlayMode/P7Benchmark/`
   - `Assets/Scenes/P7Benchmark/`
2. Confirm `Assets/Scenes/Chuo_BaseMap.unity` is untouched.
3. Confirm `ProjectSettings/` and `Packages/` are untouched.
4. Confirm `Assets/PLATEAU/` and `Assets/Data/` are untouched.
5. Confirm no existing gameplay scripts changed.
6. Confirm the metrics harness has no gameplay effect and no dependency on `EvacuationGameManager`, shelter, tsunami, route, NPC, hazard, or result systems.
7. Confirm the scene skeleton is isolated and contains only primitives, camera, light, marker, and metrics recorder.
8. Confirm no real asset import, data download, or external dependency was added.
9. Confirm P7 remains exactly five stages: P7-0, P7-A, P7-B, P7-C, and P7-D.
10. Confirm P8 tsunami hazard, inundation, light curtain, and flood systems are not implemented.
11. Confirm P9 crowd, real spawn, indoor evacuation, congestion, and failure systems are not implemented.
12. Confirm EditMode and PlayMode tests passed, or that any failure is clearly explained with logs and next action.
13. Confirm P7-B Wave 2-B still requires explicit approval before importing LOD3 assets or copying real candidate data.

Expected validation commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2a_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Return:

- A-level blockers, if any
- B-level risks, if any
- scope/protected-path verdict
- test-result verdict
- recommendation: safe to commit, commit after fixes, or do not commit

