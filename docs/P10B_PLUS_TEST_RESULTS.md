# P10-B+ Test Results

Status: validation passed.

Planned commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p10/run_p10b_plus_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p10b_plus.md
```

Expected coverage:

- English/Japanese localization lookup, language switch, fallback, missing key safety
- runtime start/pause/rules UI creation
- UI text wrap/overflow guard
- weather movement modifiers
- stamina drain, lockout, and recovery milestones
- avatar presentation/mobility separation and gender modifier disabled by default
- protected paths clean

## Results

| Check | Result |
|---|---|
| P10-B+ preflight | PASS |
| Unity GUI EditMode | PASS, 268/268 |
| Unity GUI PlayMode | PASS, 73/73 |
| DeepSeek | PASS, no A-level blockers |

Unity outputs:

- `test-results/editmode-results.xml`
- `test-results/playmode-results.xml`

Note: the first PlayMode attempt found Unity 6 no longer accepts `Arial.ttf` as a built-in font. The runtime UI now uses `LegacyRuntime.ttf`, and the rerun passed.

DeepSeek review:

- `review_reports/deepseek_review_20260525_042839.md`
- Verdict: no A-level blockers.
