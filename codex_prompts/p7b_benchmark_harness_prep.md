# Codex Prompt: P7-B Codex B — Benchmark Harness Prep Without Unity Mutation

You are working in:
D:\UnityProjects\ChuoTsunamiEvacuation-P7BHarness

Branch:
p7b-benchmark-harness-prep

Phase:
PBL7 / P7-B Wave 1

Role:
Codex B — benchmark harness planning and safe implementation design.

P7 stage count:
P7 has exactly five stages:
- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not create P7-E, P7-F, or P7-G.

Main objective:
Prepare a safe implementation design for P7-B small-area high-detail benchmark, without modifying Unity assets/scenes/scripts yet.

Strict scope:
- Do not create Unity scenes.
- Do not modify Assets/Scripts.
- Do not modify Assets/Scenes.
- Do not modify Assets/PLATEAU.
- Do not modify Assets/Data.
- Do not modify ProjectSettings.
- Do not modify Packages.
- Do not import assets.
- Do not download data.
- Do not add dependencies.
- Do not implement P8/P9 systems.

Allowed new files:
- docs/P7B_BENCHMARK_HARNESS_DESIGN.md
- docs/P7B_UNITY_CHANGE_PROPOSAL.md
- docs/P7B_TEST_PLAN.md
- docs/P7B_ROLLBACK_PLAN.md
- codex_prompts/p7b_benchmark_harness_prep.md
- deepseek_review_prompt_p7b_harness.md

Allowed updates:
- docs/TASKS.md
- docs/REVIEW_BACKLOG.md
- docs/P7_DECISION_LOG.md
- docs/P7_BENCHMARK_PROTOCOL.md
- docs/P7_BENCHMARK_AUTOMATION_PLAN.md

Do not edit files owned by Codex A:
- docs/P7B_SMALL_AREA_FEASIBILITY.md
- docs/P7B_LOD3_CANDIDATE_SELECTION.md
- docs/P7B_UNDERGROUND_BRIDGE_ROAD_FEASIBILITY.md
- docs/P7B_BENCHMARK_AREA_DECISION.md
- tools/p7/select_p7b_candidate_area.ps1
- tools/p7/run_p7b_area_feasibility.ps1

Implementation requirements:

1. docs/P7B_BENCHMARK_HARNESS_DESIGN.md
Design the future small-area benchmark harness:
- isolated benchmark scene, not Chuo_BaseMap
- no ProjectSettings/Packages modification
- no full Chuo import
- small path cluster only
- Editor metrics
- optional Windows EXE metrics
- rollback criteria
- performance thresholds

2. docs/P7B_UNITY_CHANGE_PROPOSAL.md
List proposed future Unity changes for P7-B Wave 2:
- exact allowed paths
- possible benchmark scene name
- possible scripts or editor tools
- tests required
- what must not be touched
- approval gate before implementation

3. docs/P7B_TEST_PLAN.md
Define tests required when P7-B Wave 2 touches Unity:
- EditMode
- PlayMode if scene/runtime behavior changes
- P7 preflight
- protected path check
- benchmark log validation
- no Chuo_BaseMap change unless explicitly approved

4. docs/P7B_ROLLBACK_PLAN.md
Define rollback strategy:
- revert benchmark branch
- remove generated benchmark scene
- avoid touching main scene
- keep benchmark outputs out of Git unless Markdown/CSV summaries

5. docs/P7_DECISION_LOG.md
Add P7-B decision entry:
- P7-B Wave 1 does not mutate Unity.
- P7-B Wave 2 requires explicit approval before creating scene/scripts/assets.

6. docs/TASKS.md
Update P7-B benchmark harness subsection only.

7. docs/REVIEW_BACKLOG.md
Add risks:
- benchmark scene may require Unity Editor interaction
- asset import may be heavier than expected
- LOD3/LOD4 visual quality not yet verified
- EXE benchmark may differ from Editor benchmark

8. deepseek_review_prompt_p7b_harness.md
Create a review prompt that checks:
- docs/prompts only
- no protected path changes
- no Unity mutation
- P7 has exactly five stages
- future Unity change proposal is explicit and gated
- P8/P9 not implemented

After implementation:
Run:
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_preflight.ps1

Then report:
- preflight result
- git diff --name-only
- protected path check
- whether Unity tests were intentionally not run
- recommended commit message
