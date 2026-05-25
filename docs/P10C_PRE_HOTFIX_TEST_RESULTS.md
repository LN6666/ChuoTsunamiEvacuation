# P10-C-Pre Hotfix Test Results

Status: validated with one documented scene-source limitation.

Latest validation commands:

- `powershell -ExecutionPolicy Bypass -File tools/p10/run_p10c_pre_playable_hotfix_preflight.ps1`
  - PASS: target scene availability/config/protected paths/build artifact checks passed.
  - Documented limitation: P9 target scene is the small placeholder/status shell.
  - Documented P7 source baseline: pre-existing dirty state matches expected status/size/timestamp; no new P7 mutation detected.
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
  - PASS: total=286 passed=286 failed=0 skipped=0 inconclusive=0.
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
  - PASS: total=82 passed=82 failed=0 skipped=0 inconclusive=0.
- `powershell -ExecutionPolicy Bypass -File tools/p10/build_p10c_pre_test_build.ps1`
  - PASS: rebuilt `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\ChuoTsunamiEvacuation_P10CPre.exe`.
- `powershell -ExecutionPolicy Bypass -File tools/p10/run_p10c_pre_actual_p7_scene_integration_verification.ps1 -DurationSeconds 45`
  - PASS_WITH_LIMITATIONS: built player loaded `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` and reached the playable startup flow.

Actual P7 startup evidence from Player.log:

- Start menu diagnostics were written before gameplay bootstrap.
- Final startup diagnostics after Start Game: player=1, camera=1, canvases=2, EventSystem=1, shelters=34, entrances=34, ResultPanel=1.
- P3/P4 real shelter markers: 5.
- P5 real-qualified guidance markers: 27.
- P5 humanitarian candidate guidance markers: 5.
- P6 NPC prototype: 1.
- P8 risk-front handoff loaded: True.
- P9 lightweight crowd prototype agents: 16.
- P9 spawn markers: 12.
- P9 entrance/safe-floor markers: 4.
- P9 collapse/debris zones: 3.
- P9 final gameplay flow validation: passed=7/7.
- P10 green ground frames after verification tsunami start: 2.

DeepSeek review:

- PASS: no A-level blockers.
- Report: `review_reports/deepseek_review_20260525_214635.md` (not committed).

Remaining limitation:

- The P9/P10 worktree target scene is still the tracked placeholder/status shell, while the actual 22.5 GB high-detail P7 scene remains in the protected P7 source worktree. This is reported as `placeholderLikely=True` instead of silently blanking.
