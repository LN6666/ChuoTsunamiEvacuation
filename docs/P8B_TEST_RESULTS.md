# P8-B Test Results

Date: 2026-05-23.

| Check | Result | Notes |
|---|---|---|
| P8-B risk-front preflight | PASS | `tools/p8/run_p8b_riskfront_preflight.ps1` passed. |
| P8-A preflight | PASS | `tools/p8/run_p8a_preflight.ps1` passed with the preserved local high-detail baseline warning. |
| P8-A compatibility preflight | PASS | `tools/p8/run_p8a_compat_preflight.ps1` passed. |
| Unity GUI EditMode | PASS | 176/176 passed, 0 failed, 0 skipped, 0 inconclusive. |
| Unity GUI PlayMode | PASS | 38/38 passed, 0 failed, 0 skipped, 0 inconclusive. |
| DeepSeek review | PASS | `review_reports/deepseek_review_20260523_233546.md`; verdict `Safe to commit`, no A-level blockers. |
| Protected path cleanup | PASS | Unity-generated `ProjectSettings/ProjectSettings.asset` churn was reverted and temporary `InitTestScene*.unity` files were removed. |

## Scope Reminder

P8-B is a cinematic risk-front visualization only. It is not physical tsunami height, no real-time fluid simulation, and no official hazard value.
