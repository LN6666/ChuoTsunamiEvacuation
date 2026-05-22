# Codex Prompt: P7-A Codex A — Chuo Asset Inventory + LOD / Area Selection

You are working in:
D:\UnityProjects\ChuoTsunamiEvacuation-P7Asset

Branch:
p7a-asset-inventory-lod-area

Phase:
PBL7 / P7-A

Role:
Codex A — Reference / Asset / LOD / PLATEAU direction.

Main objective:
Create command-line-first asset inventory tooling and reports for P7-A.

P7 total stage count:
P7 has exactly five stages:
- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not create P7-E, P7-F, or P7-G.

Strict scope:
P7-A must inventory existing local assets only.
Do not import assets.
Do not download data.
Do not modify Unity scenes.
Do not modify ProjectSettings.
Do not modify Packages.
Do not modify existing PLATEAU imported files.
Do not modify gameplay scripts.
Do not modify Assets/Data.
Do not create P8 hazard systems.
Do not create P9 crowd, spawn, or indoor evacuation systems.

Allowed new files:
- tools/p7/scan_p7_assets.ps1
- tools/p7/write_p7_asset_inventory_report.ps1
- tools/p7/run_p7_asset_inventory.ps1
- docs/P7_ASSET_INVENTORY_REPORT.md
- docs/P7_LOD_AVAILABILITY_REPORT.md
- docs/P7_BENCHMARK_AREA_CANDIDATES.md
- codex_prompts/p7a_asset_inventory_lod_area.md
- deepseek_review_prompt_p7a_asset.md

Allowed updates:
- docs/P7_ASSET_INVENTORY_PROTOCOL.md
- docs/P7_DECISION_LOG.md
- docs/TASKS.md
- docs/REVIEW_BACKLOG.md
- .gitignore only if needed for generated CSV/temporary logs.

Do not edit:
- docs/P7_AUTOMATION_WORKFLOW.md
- docs/P7_BENCHMARK_PROTOCOL.md
- docs/P7_PERFORMANCE_METRICS_TEMPLATE.md
- tools/p7/check_p7_scope.ps1
- tools/p7/write_p7_status_report.ps1
- tools/p7/run_p7_preflight.ps1

These are owned by Codex B or P7-0 integration.

Implementation requirements:

1. tools/p7/scan_p7_assets.ps1
Create a PowerShell scanner that runs without Unity Editor.
It should scan local project folders and produce inventory data.

It should inspect:
- Assets/
- Assets/PLATEAU/ if present
- Assets/Scenes/ only for listing, not modification
- Assets/Data/ only for listing existing data, not modification
- other likely asset folders if present

It should record:
- total file count
- extension summary
- large file list
- likely mesh/model files
- likely texture files
- likely material/prefab files
- likely PLATEAU-related paths
- likely LOD indicators in path/file names:
  LOD1, LOD2, LOD3, LOD4, lod1, lod2, lod3, lod4
- likely bridge-related paths/keywords
- likely underground-related paths/keywords
- likely road-related paths/keywords
- likely riverfront/waterfront-related paths/keywords
- candidate folders for P7 benchmark

It must not modify assets.

2. tools/p7/write_p7_asset_inventory_report.ps1
Create a script that writes Markdown report:
docs/P7_ASSET_INVENTORY_REPORT.md

Report should include:
- scan timestamp
- branch
- project path
- extension summary
- large file summary
- candidate LOD path summary
- PLATEAU-related path summary
- underground/bridge/road/riverfront keyword hits
- risks
- recommended next scan improvements

3. tools/p7/run_p7_asset_inventory.ps1
Create orchestration script:
- runs scan_p7_assets.ps1
- runs write_p7_asset_inventory_report.ps1
- runs tools/p7/run_p7_preflight.ps1
- exits non-zero on failure

4. docs/P7_LOD_AVAILABILITY_REPORT.md
Create a report skeleton and populate it with whatever the scanner can infer from filenames/paths.
Make clear this is path/name-based inference only, not verified geometry quality.

5. docs/P7_BENCHMARK_AREA_CANDIDATES.md
Create candidate selection framework.
Do not choose a final benchmark area unless there is strong evidence from local inventory.
Recommend 2-5 candidate areas or categories if discoverable.
Include criteria:
- LOD3/LOD4 availability
- shelter/high-rise relevance
- bridge/riverfront relevance
- underground/road relevance
- asset size controllability
- risk of scene/import instability
- suitability for Windows EXE benchmark

6. docs/P7_ASSET_INVENTORY_PROTOCOL.md
Update only if needed to align with implemented scripts.

7. docs/P7_DECISION_LOG.md
Mark P7-DL-001 and P7-DL-002 as approved by user after P7-0 DeepSeek PASS.
Add a P7-A decision entry for command-line-first asset inventory.

8. docs/TASKS.md
Update only the P7-A asset inventory subsection.

9. docs/REVIEW_BACKLOG.md
Add or refine P7-A asset inventory risks.

10. deepseek_review_prompt_p7a_asset.md
Create a review prompt that checks:
- no protected files changed
- no assets modified
- no scenes modified
- no Packages/ProjectSettings changed
- scanner is read-only
- reports are clearly marked as path/name-based inference
- P7 still has exactly five stages
- P8/P9 scope is not implemented

After implementation:
Run:
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_asset_inventory.ps1

Then show:
- preflight result
- asset inventory result
- git diff --name-only
- protected path check
- whether Unity tests were intentionally not run
- recommended commit message
