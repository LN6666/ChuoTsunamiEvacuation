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

## P8-B Front V1 Validation

Date: 2026-05-24.

| Check | Result | Notes |
|---|---|---|
| P8-B front-v1 preflight | PASS | `tools/p8/run_p8b_front_v1_preflight.ps1` passed. |
| P8-B risk-front preflight | PASS | `tools/p8/run_p8b_riskfront_preflight.ps1` passed. |
| P8-B guard preflight | PASS | `tools/p8/run_p8b_guard_preflight.ps1` passed with the intentionally preserved local high-detail baseline warning. |
| P8-A preflight | PASS | `tools/p8/run_p8a_preflight.ps1` passed. |
| P8-A compatibility preflight | PASS | `tools/p8/run_p8a_compat_preflight.ps1` passed. |
| Unity GUI EditMode | PASS | 189/189 passed, 0 failed, 0 skipped, 0 inconclusive. |
| Unity GUI PlayMode | PASS | 42/42 passed, 0 failed, 0 skipped, 0 inconclusive. |
| DeepSeek front-v1 review | PASS | `review_reports/deepseek_review_20260524_003442.md`; verdict `Safe to commit with C-level follow-up notes`, no A-level or B-level blockers. |

The new expected behavior is hazard-layer-driven v1: arrival time selects the active record, boundary/prototype geometry drives the front shape, depth/intensity drive visual warning level, and source/evidence metadata is surfaced without making official hazard claims.

## Scope Reminder

P8-B is a cinematic risk-front visualization only. It is not physical tsunami height, no real-time fluid simulation, and no official hazard value.

## P8-B Tokyo Evidence Hazard Layer

Date: 2026-05-24.

| Check | Result | Notes |
|---|---|---|
| P8-B evidence preflight | PASS | `tools/p8/run_p8b_evidence_preflight.ps1` passed. |
| P8-B front-v1 preflight | PASS | `tools/p8/run_p8b_front_v1_preflight.ps1` passed after switching the P8-B loader to `tsunami_hazard_layer_v1_chuo.json`. |
| P8-B risk-front preflight | PASS | `tools/p8/run_p8b_riskfront_preflight.ps1` passed. |
| P8-B guard preflight | PASS | `tools/p8/run_p8b_guard_preflight.ps1` passed with the intentionally preserved local high-detail baseline warning. |
| P8-A preflight | PASS | `tools/p8/run_p8a_preflight.ps1` passed. |
| Unity GUI EditMode | PASS | 191/191 passed, 0 failed, 0 skipped, 0 inconclusive. Initial run timed out during Unity import; rerun after clearing stale Bee backend completed. |
| Unity GUI PlayMode | PASS | 42/42 passed, 0 failed, 0 skipped, 0 inconclusive. |
| DeepSeek evidence review | PASS | `review_reports/deepseek_review_20260524_013940.md`; verdict `Safe to commit`, no A-level or B-level blockers. |
| Protected path cleanup | PASS | Unity-generated `ProjectSettings/ProjectSettings.asset` churn was reverted and temporary `InitTestScene*.unity` files were removed after approval. |

The earlier evidence-layer update prioritized Tokyo Metropolitan Government tsunami damage estimation as the primary Chuo evidence candidate.

## P8-B Spatial Extraction Gate

Date: 2026-05-24.

| Check | Result | Notes |
|---|---|---|
| P8-B evidence spatial gate | PASS | `tools/p8/run_p8b_evidence_spatial_gate.ps1` verified extracted official Chuo spatial mesh data and P8-C gate `PASS`. |
| P8-B evidence preflight | PASS | `tools/p8/run_p8b_evidence_preflight.ps1` passed after updating evidence docs/data from conditional to extracted spatial. |
| P8-B front-v1 preflight | PASS | `tools/p8/run_p8b_front_v1_preflight.ps1` passed with `official_tsunami_metropolitan` source mode. |
| P8-B risk-front preflight | PASS | `tools/p8/run_p8b_riskfront_preflight.ps1` passed. |
| P8-B guard preflight | PASS | `tools/p8/run_p8b_guard_preflight.ps1` passed; preserved the local high-detail scene baseline. |
| P8-A preflight | PASS | `tools/p8/run_p8a_preflight.ps1` passed. |
| P8-A compatibility preflight | PASS | `tools/p8/run_p8a_compat_preflight.ps1` passed. |
| Unity GUI EditMode | PASS | 193/193 passed, 0 failed, 0 skipped, 0 inconclusive. |
| Unity GUI PlayMode | PASS | 42/42 passed, 0 failed, 0 skipped, 0 inconclusive. |
| DeepSeek spatial-gate review | PASS | `review_reports/deepseek_review_20260524_023231.md`; verdict `PASS`, no A-level blockers. B/C follow-up: consider compressed/lazy spatial samples if runtime memory becomes a concern. |
| Protected path cleanup | PASS | Unity-generated `ProjectSettings/ProjectSettings.asset` churn was reverted and temporary `InitTestScene*.unity` files were removed after approval. |

Current P8-C gate is `PASS`: official Tokyo tsunami mesh records were extracted and clipped to Chuo. The maximum inundation depth is spatial; `maxTsunamiHeightMeters` remains a separate field and is not the depth grid. The current boundary is a derived grid-extent bbox, not a Tokyo-issued official inundation contour.
