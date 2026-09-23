# Codex Prompt Trace: P10-B+ UI Localization Weather Stamina

Task: P10-B+ UI / Localization / Weather / Stamina / Manual Playtest Polish.

P10-B+ is not a new official stage. It is a polish/fix sprint after P10-B and before P10-C.

Required boundaries:

- Do not create P10-E, P10-F, or P10-G.
- Do not build the final Windows EXE.
- Keep Windows EXE build deferred to P10-C.
- Do not add real indoor scenes, new tsunami simulation, new crowd engine, package dependencies, or release/archive artifacts.
- Protect `Chuo_BaseMap.unity`, `ProjectSettings/`, `Packages/`, `Assets/PLATEAU/`, and `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.

Implementation scope:

- English/Japanese localization with English fallback.
- Runtime-ready start, pause, options, and rules UI.
- Exported English/Japanese rules docs.
- Safe background asset policy without unlicensed images.
- Weather/night movement modifiers.
- Deterministic stamina/sprint rules.
- Avatar presentation separated from mobility profile; optional gender speed modifier disabled by default and documented as scenario-only assumption.

Validation:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p10/run_p10b_plus_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p10b_plus.md
```
