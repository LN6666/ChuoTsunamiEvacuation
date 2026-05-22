# Codex Prompt: P7-B Codex A — Small-Area Feasibility + LOD/Bridge/Underground/Road Selection

You are working in:
D:\UnityProjects\ChuoTsunamiEvacuation-P7BArea

Branch:
p7b-area-feasibility

Phase:
PBL7 / P7-B Wave 1

Role:
Codex A — area feasibility, LOD availability interpretation, underground/bridge/road/riverfront candidate selection.

P7 stage count:
P7 has exactly five stages:
- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not create P7-E, P7-F, or P7-G.

Main objective:
Use the P7-A asset inventory outputs to produce a conservative, command-line-first P7-B small-area benchmark feasibility package.

Strict scope:
- Do not import assets.
- Do not modify Unity scenes.
- Do not modify ProjectSettings.
- Do not modify Packages.
- Do not modify existing PLATEAU imported files.
- Do not modify gameplay scripts.
- Do not modify Assets/Data.
- Do not implement P8 tsunami hazard systems.
- Do not implement P9 crowd/spawn/indoor evacuation systems.
- Do not claim LOD quality is verified.
- Do not claim LOD4 exists unless supported by actual local inventory evidence.

Allowed new files:
- docs/P7B_SMALL_AREA_FEASIBILITY.md
- docs/P7B_LOD3_CANDIDATE_SELECTION.md
- docs/P7B_UNDERGROUND_BRIDGE_ROAD_FEASIBILITY.md
- docs/P7B_BENCHMARK_AREA_DECISION.md
- tools/p7/select_p7b_candidate_area.ps1
- tools/p7/run_p7b_area_feasibility.ps1
- codex_prompts/p7b_area_feasibility.md
- deepseek_review_prompt_p7b_area.md

Allowed updates:
- docs/TASKS.md
- docs/REVIEW_BACKLOG.md
- docs/P7_DECISION_LOG.md
- docs/P7_BENCHMARK_AREA_CANDIDATES.md
- docs/P7_LOD_AVAILABILITY_REPORT.md only if needed to add P7-B interpretation notes.

Do not edit:
- Unity scene files.
- Assets/Scripts.
- Assets/Data.
- ProjectSettings.
- Packages.
- P7 benchmark performance scripts owned by Codex B unless absolutely necessary.

Inputs to inspect:
- docs/P7_ASSET_INVENTORY_REPORT.md
- docs/P7_LOD_AVAILABILITY_REPORT.md
- docs/P7_BENCHMARK_AREA_CANDIDATES.md
- tools/p7/scan_p7_assets.ps1 output assumptions
- local file tree only through read-only commands

Implementation requirements:

1. docs/P7B_SMALL_AREA_FEASIBILITY.md
Create a feasibility report that summarizes:
- what P7-A found
- what can be inferred safely
- what cannot be inferred
- why P7-B should start with a small area
- why no full Chuo high-detail import is allowed yet
- whether LOD4 appears available from current path/name inventory
- recommended next action

2. docs/P7B_LOD3_CANDIDATE_SELECTION.md
Create a report identifying candidate LOD3 folders/path clusters based on P7-A reports and local path/name signals.
Make clear this is path/name-based inference only.

3. docs/P7B_UNDERGROUND_BRIDGE_ROAD_FEASIBILITY.md
Analyze available path/name hints for:
- underground
- bridge
- road
- riverfront/waterfront
- elevated structures if detectable
Do not claim geometry or visual correctness.

4. docs/P7B_BENCHMARK_AREA_DECISION.md
Recommend:
- one preferred P7-B small benchmark area or path cluster if evidence is strong enough
- otherwise recommend 2-3 candidates and mark final selection as pending
Include scoring criteria:
- LOD3 availability
- asset size controllability
- relation to shelter/high-rise/bridge/riverfront/underground goals
- risk
- expected Unity import difficulty
- expected Windows EXE benchmark usefulness

5. tools/p7/select_p7b_candidate_area.ps1
Create a read-only helper that summarizes candidate folders using P7-A reports and local filesystem metadata.
It should not modify files.
It should output Markdown or JSON summary to stdout.

6. tools/p7/run_p7b_area_feasibility.ps1
Run:
- select_p7b_candidate_area.ps1
- tools/p7/run_p7_preflight.ps1
Exit non-zero on failure.

7. docs/P7_DECISION_LOG.md
Add P7-B decision entry:
- P7-B starts with feasibility/area selection before Unity scene or asset import.
- LOD4 is not assumed available from current path/name inventory.

8. docs/TASKS.md
Update P7-B subsection only.

9. docs/REVIEW_BACKLOG.md
Add risks:
- LOD3 path/name signals may not equal usable geometry.
- LOD4 not found by path/name scan.
- underground/bridge/road feasibility remains unverified until Unity/geometry inspection.
- external PLATEAU source is only enumerated.

10. deepseek_review_prompt_p7b_area.md
Create a review prompt that checks:
- no protected paths changed
- no Unity scene/script/asset/settings/package changes
- helper scripts are read-only
- P7 has exactly five stages
- P7-B does not implement P8/P9
- LOD findings are clearly marked as unverified path/name inference
- small-area benchmark decision is conservative

After implementation:
Run:
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_area_feasibility.ps1

Then report:
- area feasibility result
- P7 preflight result
- git diff --name-only
- protected path check
- whether Unity tests were intentionally not run
- recommended commit message
