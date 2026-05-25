# DeepSeek Review Prompt: P10-C-- Extended Performance Sampling

Review the current git diff for ChuoTsunamiEvacuation P10-C--.

P10-C-- is a second pre-release performance gate before official P10-C. It is not official P10-C, not the final Windows EXE release build, not release packaging, not archive work, and not P10-E/P10-F/P10-G.

Please verify:

1. P10-C-- is not treated as a new official P10-E/F/G stage.
2. No final release package/archive was created.
3. No build artifacts, raw huge profiling logs, or Unity temporary scene outputs are committed.
4. 3/5/10 minute process sampling is implemented or honestly documented.
5. FPS/frame-time/1 percent low/stutter evidence gap from P10-C-Pre is addressed.
6. High-detail scene full-load validation is attempted and documented without mutating `P7_HighDetail_Chuo.unity`.
7. Player.log, memory, CPU/proxy, paging, and loading evidence are meaningful.
8. Scenario coverage is honest for tsunami start, green frames, light curtain, crowd, UI/ResultPanel, and night/rain.
9. Any quick fixes are low-risk and do not touch ProjectSettings, Packages, PLATEAU assets, or protected scenes.
10. Protected paths are clean.
11. P10-C readiness decision is honest.
12. Unity lifecycle and compile risks are acceptable.
13. There are no A-level blockers.

Focus especially on whether the runtime FPS exporter can run in a built player without scene mutation, whether PowerShell scripts can accidentally commit release artifacts, and whether docs overclaim scenario validation that was not actually measured.
