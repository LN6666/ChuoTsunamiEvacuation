# Codex Prompt Trace: P10-B Runtime Smoke Profiling Optimization

Task: P10-B High-Detail Runtime Smoke + Performance Profiling + Stress Test + Optimization + Manual Playtest Preparation.

Stage correction:

- Do not build the final Windows EXE in P10-B.
- Windows EXE build is deferred to P10-C after user manual playtest and quick fixes.
- Do not create P10-E, P10-F, or P10-G.

Implementation scope:

- Add tsunami-start green rectangular ground frame runtime markers for official evacuation targets and non-official humanitarian high-rise candidates.
- Preserve non-official humanitarian candidate warnings and never mark candidates as official/safe by default.
- Use runtime generation and coordinate/proxy rectangle fallback without mutating PLATEAU assets or high-detail scenes.
- Add lightweight performance metrics and bounded stress scenario configs.
- Prepare manual playtest checklist and P10-C handoff docs.

Protected paths:

- `Assets/Scenes/Chuo_BaseMap.unity`
- `ProjectSettings/`
- `Packages/`
- `Assets/PLATEAU/`
- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

Validation:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p10/run_p10b_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p10b.md
```
