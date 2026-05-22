# DeepSeek Review Prompt: P7-B Wave 1 Integration

You are reviewing the integrated P7-B Wave 1 work for ChuoTsunamiEvacuation.

Current branch:
p7-high-detail-city-foundation

Integrated branches:
- p7b-area-feasibility
- p7b-benchmark-harness-prep

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
5. Confirm P7BArea outputs are preserved:
   - docs/P7B_SMALL_AREA_FEASIBILITY.md
   - docs/P7B_LOD3_CANDIDATE_SELECTION.md
   - docs/P7B_UNDERGROUND_BRIDGE_ROAD_FEASIBILITY.md
   - docs/P7B_BENCHMARK_AREA_DECISION.md
   - tools/p7/select_p7b_candidate_area.ps1
   - tools/p7/run_p7b_area_feasibility.ps1
6. Confirm P7BHarness outputs are preserved:
   - docs/P7B_BENCHMARK_HARNESS_DESIGN.md
   - docs/P7B_UNITY_CHANGE_PROPOSAL.md
   - docs/P7B_TEST_PLAN.md
   - docs/P7B_ROLLBACK_PLAN.md
7. Confirm shared documents combine both branches rather than losing either side:
   - docs/TASKS.md
   - docs/REVIEW_BACKLOG.md
   - docs/P7_DECISION_LOG.md
   - docs/p7_status/p7_status_latest.md
8. Confirm P7-B Wave 1 remains docs/tools/prompts-only.
9. Confirm P7-B Wave 2 requires explicit human approval before any Unity scene, script, asset, import, package, or ProjectSettings change.
10. Confirm LOD3 candidate 53393690 and fallback candidates 53393672 / 53394611 are treated only as path/name-based planning candidates.
11. Confirm LOD4 is not assumed available.
12. Confirm no P8 tsunami hazard/light curtain/inundation system is implemented.
13. Confirm no P9 crowd/spawn/indoor evacuation/failure system is implemented.
14. Confirm P7 preflight passed.
15. Confirm Unity tests are intentionally not required because this integration is docs/tools/prompts/report-only.

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
   State whether both P7BArea and P7BHarness outputs are preserved.

7. Automation/preflight confirmation.

8. Unity test decision:
   State whether Unity tests are intentionally not required.

9. Final recommendation:
   State whether this integrated branch can be pushed.
