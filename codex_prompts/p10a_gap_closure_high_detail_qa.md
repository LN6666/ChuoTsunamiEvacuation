# Codex Prompt Trace: P10-A Gap Closure High-Detail QA

Task:

- P10-A Remaining Gap Closure + High-Detail Scene QA.

Scope:

- QA and documentation only
- P10-A data/status JSON
- preflight and JSON validation tools
- high-detail scene smoke readiness
- coordinate anchoring and humanitarian candidate final QA
- P10-B performance/stress/optimization readiness

Boundaries:

- no P10-E/F/G
- no new large gameplay systems
- no P8/P9 reimplementation
- no real indoor scene
- no Windows EXE build in P10-A
- no release package/archive work in P10-A
- protect `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- do not modify `Chuo_BaseMap.unity`, `ProjectSettings`, `Packages`, or `Assets/PLATEAU`

Validation:

- `powershell -ExecutionPolicy Bypass -File tools/p10/run_p10a_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p10a.md`
