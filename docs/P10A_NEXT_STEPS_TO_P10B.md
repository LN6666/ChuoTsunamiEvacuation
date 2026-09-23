# P10-A Next Steps To P10-B

P10-B should start only after P10-A preflight, Unity EditMode, Unity PlayMode, and DeepSeek pass.

Recommended P10-B sequence:

1. Confirm branch starts from P10-A.
2. Verify protected paths are clean.
3. Prepare Windows x64 build scene list.
4. Build Windows EXE outside normal Git output paths.
5. Run high-detail scene smoke.
6. Run benchmark scenarios on Low, Medium, and High quality presets.
7. Collect FPS, 1 percent low/stutter, CPU, memory, GC allocation if available, loading time, Player.log errors/warnings, NPC count, marker count, light curtain impact, and UI/ResultPanel impact.
8. Apply low-risk optimization only with before/after metrics where possible.
9. Export benchmark results and document remaining performance limitations.

P10-B should not:

- claim official route validation
- claim GIS-grade coordinate anchoring
- claim exact PLATEAU Unity object identity
- add real indoor scenes
- commit build outputs unless explicitly approved
