# P9-B Test Results

Validation status:

- P9-B preflight: PASS
- Unity GUI EditMode: PASS, 223 total / 223 passed / 0 failed
- Unity GUI PlayMode: PASS, 58 total / 58 passed / 0 failed
- DeepSeek: PASS, no A-level blockers

Validation commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p9/run_p9b_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p9b.md
```

DeepSeek review:

- report: `review_reports/deepseek_review_20260524_234426.md` (local review output, not committed)
- verdict: PASS
- A-level blockers: none
- B-level follow-ups recorded in `docs/P9B_REVIEW_BACKLOG.md`

Expected checks:

- weighted spawn deterministic with seed
- high-risk/coastal/low-elevation weighting is higher than low-risk proxy weighting
- spawn cap respected
- exclusion zones and unsafe zero positions rejected
- P8-E humanitarian marker handoff loads 110 records
- humanitarian candidates remain non-official and warning-required
- entrance/safe-floor proxy markers load and generate
- crowd runtime spawner respects cap
- crowd metrics are deterministic/testable
- collapse/debris risk zones do not mutate player outcome
- P5 routes remain estimated prototype guidance
- protected paths remain clean
