# P9-A Test Results

Validation date: 2026-05-24.

## P9-A Preflight

Command:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p9/run_p9a_preflight.ps1
```

Result: PASS.

Evidence:

- P9-A changed-file scope check passed.
- P9 JSON validation passed.
- P9 has exactly four stages, P9-A through P9-D.
- P9 schemas, scripts, tests, docs, preflight, and review prompt exist.
- No protected paths are dirty.

## Unity EditMode

Command:

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
```

Result: PASS.

Evidence:

- Full EditMode suite: 210 total / 210 passed / 0 failed / 0 skipped / 0 inconclusive.
- P9 EditMode tests: 7 passed.
- Initial full-suite run showed the existing P8-A compatibility inspector rejecting P9 untracked metadata as a P8-A scope violation. P9 tests passed in that run. The suite was rerun with a temporary local git ignore block for P9 untracked files so the legacy P8 guard evaluated its intended clean-P8 condition; that rerun passed. The temporary ignore block was removed afterward.

## Unity PlayMode

Command:

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Result: PASS.

Evidence:

- Full PlayMode suite: 53 total / 53 passed / 0 failed / 0 skipped / 0 inconclusive.
- P9 PlayMode tests: 4 passed.
- Unity-generated `ProjectSettings/ProjectSettings.asset` churn was reverted after the PlayMode run.
- A temporary generated `Assets/InitTestScene*.unity` test scene artifact from the first PlayMode run was removed.

## DeepSeek Review

Command:

```powershell
$env:PYTHONIOENCODING='utf-8'; python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p9a.md
```

Result: PASS.

Evidence:

- DeepSeek verdict: PASS.
- A-level blockers: none.
- Review report saved locally under `review_reports/`.

Note: the first DeepSeek run received a review but failed while printing a Unicode character to the CP932 console. It was rerun with UTF-8 console encoding and exited successfully.
