# Codex Prompt Trace: P10-A+ Final Gap Hardening

Task:

- P10-A+ Final Gap Hardening Before P10-B.

Positioning:

- P10-A+ is a hardening sprint under P10-A, not a new official stage.
- Official P10 stages remain P10-A, P10-B, P10-C, and P10-D.

Scope:

- generate report-backed hardening evidence for candidate anchoring, candidate-to-building nearest-match/proxy binding, entrance proxies, route proxies, PLATEAU semantic binding, light curtain QA readiness, ResultPanel QA readiness, and high-detail smoke readiness
- update P10-B readiness based on hardening findings
- avoid new gameplay systems and avoid scene mutation

Protected paths:

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- `Assets/Scenes/Chuo_BaseMap.unity`
- `ProjectSettings/`
- `Packages/`
- `Assets/PLATEAU/`

Validation:

- `powershell -ExecutionPolicy Bypass -File tools/p10/run_p10a_plus_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p10a_plus.md`
