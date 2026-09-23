# P7-D Test Results

Validation date: 2026-05-23.

## Results

| Check | Result | Notes |
|---|---|---|
| P7-D preflight | PASS | Scene/category/LOD/P2-P6/performance-record/P8-P9/archive guards passed. |
| P7-D postcheck preflight | PASS | Practical baseline wording, protected paths, EXE profiling report, closeout docs, and archive plan passed. |
| Unity GUI EditMode | PASS | Initial 900s run timed out before log creation; retry with `-TimeoutSeconds 1800` passed 155/155, 0 failed, 0 skipped, 0 inconclusive. |
| Unity GUI PlayMode | PASS | 31/31 passed, 0 failed, 0 skipped, 0 inconclusive. |
| DeepSeek final review | PASS | `review_reports/deepseek_review_20260523_190600.md`; no A-level or B-level findings. |
| Protected paths | PASS after cleanup | Unity changed `ProjectSettings/ProjectSettings.asset` after tests; it was reverted. No `Packages`, `Assets/Data`, `Assets/PLATEAU`, `Chuo_BaseMap`, build, cache, or profiler outputs are staged. |

## Scene Validation Summary

- Manual import verified: yes.
- Renderable PLATEAU evidence: 117,728 CityObjectGroups with MeshRenderer/MeshFilter/MeshCollider.
- Actual LOD range: LOD0-LOD2.
- Average LOD3 achieved: false.
- Baseline decision: user-approved practical high-detail baseline.

## Remaining Validation Items

- Windows EXE profiling is prepared but not run; this is a P8/P10 follow-up, not a P8 start blocker.
- P2-P6 runtime smoke in the populated scene is pending.
- Missing category follow-up should use data-layer/proxy/rule-based approaches or targeted imports if P8/P9 need water, terrain, disaster risk, land use, urban planning, vegetation, or city furniture.
