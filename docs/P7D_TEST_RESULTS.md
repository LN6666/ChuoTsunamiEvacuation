# P7-D Test Results

Validation date: 2026-05-23.

## Results

| Check | Result | Notes |
|---|---|---|
| P7-D preflight | CONDITIONAL PASS | Scene/category/LOD/P2-P6/performance-record/P8-P9/archive guards passed. |
| Unity GUI EditMode | PASS | 155/155 passed, 0 failed, 0 skipped, 0 inconclusive. |
| Unity GUI PlayMode | PASS | 31/31 passed, 0 failed, 0 skipped, 0 inconclusive. |
| DeepSeek final review | CONDITIONAL PASS | No A-level blockers. Latest report is under local-only `review_reports/deepseek_review_*.md`. |
| Protected paths | PASS after cleanup | Unity changed `ProjectSettings/ProjectSettings.asset` (`runInBackground`) and created an `Assets/InitTestScene*.unity`; both were reverted/removed. `Packages`, `Assets/Data`, `Assets/PLATEAU`, and `Chuo_BaseMap` are clean. |

## Scene Validation Summary

- Manual import verified: yes.
- Renderable PLATEAU evidence: 117,728 CityObjectGroups with MeshRenderer/MeshFilter/MeshCollider.
- Actual LOD range: LOD0-LOD2.
- Average LOD3 achieved: false.
- Baseline decision: conditional pass.

## Remaining Validation Items

- Windows EXE profiling is prepared but not run.
- P2-P6 runtime smoke in the populated scene is pending.
- Missing category follow-up is required if P8/P9 need water, terrain, disaster risk, land use, urban planning, vegetation, or city furniture.
