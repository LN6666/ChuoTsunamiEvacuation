# DeepSeek Review Prompt: P7-A Integration

You are reviewing the integrated P7-A work for ChuoTsunamiEvacuation.

Current branch:
p7-high-detail-city-foundation

Integrated branches:
- p7a-asset-inventory-lod-area
- p7a-benchmark-performance-prep

P7 positioning:
High-detail / Full Chuo Asset Loading + Underground / Bridge + LOD Upgrade + Windows EXE Optimization.

P7 has exactly five stages:
- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not allow P7-E, P7-F, or P7-G.

Review checks:

1. Confirm P7 has exactly five stages: P7-0, P7-A, P7-B, P7-C, P7-D.
2. Confirm no P7-E/P7-F/P7-G stage is introduced.
3. Confirm no protected paths were changed:
   - ProjectSettings/
   - Packages/
   - Assets/Scenes/
   - Assets/PLATEAU/
   - Assets/Scripts/
   - Assets/Data/
4. Confirm no Unity scene, gameplay script, imported asset, package, project setting, or runtime data was modified.
5. Confirm P7Asset outputs are preserved:
   - docs/P7_ASSET_INVENTORY_REPORT.md
   - docs/P7_LOD_AVAILABILITY_REPORT.md
   - docs/P7_BENCHMARK_AREA_CANDIDATES.md
   - tools/p7/scan_p7_assets.ps1
   - tools/p7/write_p7_asset_inventory_report.ps1
   - tools/p7/run_p7_asset_inventory.ps1
6. Confirm P7Perf outputs are preserved:
   - docs/P7_BENCHMARK_AUTOMATION_PLAN.md
   - docs/P7_EXE_PROFILING_PREP.md
   - docs/P7_PERFORMANCE_LOG_SCHEMA.md
   - tools/p7/new_p7_benchmark_record.ps1
   - tools/p7/validate_p7_performance_log.ps1
   - tools/p7/run_p7_benchmark_preflight.ps1
   - improved tools/p7/check_p7_scope.ps1
   - improved tools/p7/write_p7_status_report.ps1
7. Confirm shared documents combine both branches rather than losing either side:
   - docs/TASKS.md
   - docs/REVIEW_BACKLOG.md
   - docs/P7_DECISION_LOG.md
   - docs/P7_TWO_CODEX_WORKFLOW.md
   - docs/P7_AUTOMATION_WORKFLOW.md
8. Confirm asset inventory reports clearly state path/name-based inference only and no geometry quality verification.
9. Confirm scan_p7_assets.ps1 remains read-only:
   - no copy
   - no delete
   - no move
   - no import
   - no file mutation
   - external PLATEAU source is only enumerated
10. Confirm benchmark/performance scripts do not run Unity or modify Unity assets.
11. Confirm no P8 tsunami hazard/light curtain/inundation system is implemented.
12. Confirm no P9 crowd/spawn/indoor evacuation/failure system is implemented.
13. Confirm P7 preflight passed.
14. Confirm Unity tests are not required because this integration is docs/tools/prompts/report-only.
15. Identify B-level follow-ups that should remain in REVIEW_BACKLOG:
   - warning filtering heuristic hardening
   - Codex file ownership clarification
   - benchmark record Stage parameter should possibly become mandatory
   - asset scanner live-run confirmation / clean-checkout spot check
   - LOD findings are path/name-based, not geometry-quality verified

Output format:
1. Overall verdict:
   PASS / PASS WITH B-LEVEL FOLLOW-UPS / BLOCKED

2. A-level blockers:
   Must-fix before commit/push.

3. B-level follow-ups:
   Non-blocking items.

4. Protected path confirmation.

5. P7 stage-count confirmation.

6. Integration preservation confirmation:
   State whether both P7Asset and P7Perf outputs are preserved.

7. Automation/preflight confirmation.

8. Unity test decision:
   State whether Unity tests are intentionally not required.

9. Final recommendation:
   State whether this integrated branch can be pushed.
