# P10-C-Pre Test Results

Validated on 2026-05-25 JST.

## Automated Validation

- P10-C-Pre preflight: PASS
- Unity GUI EditMode: PASS, 279/279
- Unity GUI PlayMode: PASS, 79/79
- Temporary Windows x64 profiling/test build: PASS
- Built-player process profile: PASS
- DeepSeek: PASS, no A-level blockers
- DeepSeek report: `review_reports/deepseek_review_20260525_151744.md`

## Temporary Build

- Output: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\ChuoTsunamiEvacuation_P10CPre.exe`
- Scene: `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- Type: temporary development profiling/test build
- Build warnings/errors: 4 warnings, 0 errors
- Final release package/archive: not created

## Built-Player Profile

- Duration: 30 seconds
- Process samples: 29
- Max working set: 608.05 MB
- Max private memory: 1003.33 MB
- Player.log warnings/errors: 0/0
- FPS/frame-time evidence: not captured by the process-only profile

## Readiness

Decision: `needs_quick_fix_before_p10c`

Required before official P10-C packaging:

- capture or manually record built-player FPS, 1 percent low proxy, and frame-time/stutter around tsunami start, green frames, light curtain, crowd, UI, and ResultPanel
- confirm memory stabilizes during a longer manual run
- verify AA visually in the built player
