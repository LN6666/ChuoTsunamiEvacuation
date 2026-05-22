# Codex Prompt: P7-0 Auto-First Reference Review + LOD Strategy Foundation

You are working in the P7 worktree of the Unity project ChuoTsunamiEvacuation.

Phase:
PBL7 / P7-0

P7 total stage count:
P7 must have exactly five stages:
- P7-0: Scope Freeze + Reference Review + Automation Foundation
- P7-A: Chuo Asset Inventory + LOD / Area Selection
- P7-B: Small-Area High-Detail Benchmark + Underground / Bridge / Road Feasibility
- P7-C: Streaming / Chunk Loading + Visual Quality + Performance Optimization
- P7-D: Windows EXE Profiling + P7 Final Closeout

Do not create P7-E, P7-F, or P7-G.

Main objective:
Create the command-line-first P7-0 foundation for:
- official / GitHub reference review
- LOD / optimization technical selection
- automated guard checks
- Markdown status records
- two-Codex parallel workflow planning
- DeepSeek review gate

P7 positioning:
High-detail / Full Chuo Asset Loading + Underground / Bridge + LOD Upgrade + Windows EXE Optimization.

Core operating principle:
If a task can be automated, do not make it manual.
If a result can be recorded in Markdown, record it in Markdown.
If a safety boundary can be checked by script, add a script check.
If Unity tests are not appropriate for a docs-only phase, add command-line preflight checks instead.

Strict prohibitions:
- Do not modify ProjectSettings.
- Do not modify Packages.
- Do not modify Chuo_BaseMap.unity.
- Do not modify Unity scenes.
- Do not modify existing PLATEAU imported files.
- Do not import large assets.
- Do not download large data.
- Do not add external dependencies.
- Do not add or change gameplay scripts.
- Do not add NPC failure mechanics.
- Do not add real crowd simulation.
- Do not add road-blocking gameplay.
- Do not add building-collapse gameplay.
- Do not add real spawn point systems.
- Do not add tsunami height, inundation depth, dynamic light curtain, or P8 hazard systems.
- Do not add P9 indoor shelter evacuation systems.
- Do not modify existing gameplay success/failure rules.

Allowed new files:
- docs/P7_STAGE_PLAN.md
- docs/P7_BOUNDARIES.md
- docs/P7_REFERENCE_REVIEW.md
- docs/P7_LOD_ASSET_STRATEGY.md
- docs/P7_ASSET_INVENTORY_PROTOCOL.md
- docs/P7_BENCHMARK_PROTOCOL.md
- docs/P7_PERFORMANCE_METRICS_TEMPLATE.md
- docs/P7_DECISION_LOG.md
- docs/P7_AUTOMATION_WORKFLOW.md
- docs/P7_TWO_CODEX_WORKFLOW.md
- tools/p7/check_p7_scope.ps1
- tools/p7/write_p7_status_report.ps1
- tools/p7/run_p7_preflight.ps1
- codex_prompts/p7_0_reference_review_lod_strategy_auto.md
- deepseek_review_prompt_p70.md

Allowed updates:
- docs/TASKS.md
- docs/REVIEW_BACKLOG.md
- .gitignore only if needed for P7 generated logs, review outputs, or benchmark outputs.

Documentation requirements:

1. docs/P7_STAGE_PLAN.md
Document P7 purpose, relationship with P6/P8/P9/P10, five-stage plan only, deliverables, checks/tests, DeepSeek review expectation, and commit/push expectation. Explicitly state that P7-E/P7-F/P7-G must not be created.

2. docs/P7_BOUNDARIES.md
Document allowed scope, forbidden scope, protected paths, dependency approval rules, asset import rules, full Chuo import safety rule, Windows x64 EXE optimization boundary, and P7/P8/P9/P10 separation.

3. docs/P7_REFERENCE_REVIEW.md
Create a structured reference review for:
- Project-PLATEAU / PLATEAU-SDK-for-Unity
- Project-PLATEAU / PLATEAU-SDK-Toolkits-for-Unity
- PLATEAU 3D Tiles / streaming references
- Cesium for Unity
- GeoTileLoader or similar tiled city loading references
- Unity Addressables
- AssetBundle / asset streaming
- Unity LODGroup
- Unity Occlusion Culling
- GPU Occlusion Culling
- GPU Instancing
- draw call batching
- mesh slicing / city chunking / culling references
- Unity Profiler
- Unity Frame Debugger
- Unity Memory Profiler

For each reference, classify as:
- reference_only
- optional_dependency_candidate
- not_recommended_for_now

For each reference, include expected P7 use, benefit, risk, P7-0 adoption decision, what must be verified before adoption, and whether it may require Packages or ProjectSettings changes. Do not claim optional dependencies are adopted.

4. docs/P7_LOD_ASSET_STRATEGY.md
Include average LOD3 target, selected LOD4 for key areas, minimum LOD2 baseline, LOD1 fallback-only policy, key-area prioritization, asset quality/performance tradeoff, collision simplification, material count strategy, texture compression/atlas strategy, LOD popping risk, and small-area benchmark requirement before any full Chuo import.

5. docs/P7_ASSET_INVENTORY_PROTOCOL.md
Define command-line-friendly asset inventory:
scan local asset folders, record file counts/extensions/large files/likely PLATEAU category paths/candidate LOD levels, no asset import during inventory, output Markdown and CSV when allowed, support P7-A.

6. docs/P7_BENCHMARK_PROTOCOL.md
Define benchmark area selection, Editor benchmark, Windows x64 EXE benchmark, average FPS, 1% low FPS, RAM, VRAM/texture memory if available, draw calls, triangles, batches, loading time, build size, bottlenecks, rollback criteria, pass/fail threshold, required Markdown record. Use 60 FPS preferred and 30 FPS minimum acceptable.

7. docs/P7_PERFORMANCE_METRICS_TEMPLATE.md
Create reusable Markdown tables for environment, machine specs, scene, asset scope, LOD, graphics settings, Editor metrics, Windows EXE metrics, bottlenecks, and decision.

8. docs/P7_DECISION_LOG.md
Create decision log format: date, decision, options considered, reason, risk, verification, follow-up, approved by.

9. docs/P7_AUTOMATION_WORKFLOW.md
Document command-line-first workflow, preflight checks, scope guard script, Markdown status report script, when to run Unity GUI tests, when not to run Unity tests, DeepSeek review command pattern, commit/push workflow. State P7-0 does not run Unity EditMode/PlayMode because it is docs/tools/prompts only. State P7-A/P7-B/P7-C/P7-D must run automated Unity tests whenever Unity code/assets/scenes are changed.

10. docs/P7_TWO_CODEX_WORKFLOW.md
Define two-Codex workflow:
- Codex A: Reference / Asset / LOD / PLATEAU direction
- Codex B: Automation / Benchmark / Performance / EXE direction
- recommended future worktrees:
  D:\UnityProjects\ChuoTsunamiEvacuation-P7Asset
  D:\UnityProjects\ChuoTsunamiEvacuation-P7Perf
- file ownership rules
- conflict prevention rules
- shared files only integration Codex should update
- protected files both Codex agents must not touch
- no simultaneous edits to ProjectSettings, Packages, Chuo_BaseMap, PLATEAU imported assets, core gameplay files, Assets/Data.

11. tools/p7/check_p7_scope.ps1
Create a PowerShell guard script that:
- checks git diff --name-only against HEAD
- fails if protected paths are modified:
  ProjectSettings/
  Packages/
  Assets/Scenes/
  Assets/PLATEAU/
  Assets/Scripts/
  Assets/Data/
- fails if large files over a conservative threshold are newly added, unless under explicitly allowed docs/log paths
- warns on forbidden scope keywords in changed text files:
  crowd failure
  road block gameplay
  building collapse
  tsunami height
  inundation depth
  light curtain
  real spawn
  indoor evacuation
  P6-F
  P7-E
  P7-F
  P7-G
- prints a clear PASS/FAIL summary
- exits non-zero on failure.

12. tools/p7/write_p7_status_report.ps1
Create a PowerShell script that:
- creates docs/p7_status if needed
- writes a Markdown report containing date/time, branch, git status, changed files, latest commit, scope guard result summary if available, and notes placeholder.

13. tools/p7/run_p7_preflight.ps1
Create a PowerShell script that:
- runs check_p7_scope.ps1
- runs write_p7_status_report.ps1
- prints final PASS/FAIL
- exits non-zero if guard fails.

14. docs/TASKS.md
Add P7 section with P7-0 through P7-D only, current task status, automation requirement, and DeepSeek review requirement.

15. docs/REVIEW_BACKLOG.md
Add P7 risks:
high LOD asset size, full Chuo import risk, dependency/Packages risk, ProjectSettings risk, Windows EXE performance gap, LOD popping, collision overhead, texture/material explosion, underground/bridge data uncertainty, P8/P9 scope creep, manual process risk, two-Codex merge conflict risk.

16. deepseek_review_prompt_p70.md
Create a strict review prompt that checks:
- P7-0 is docs/tools/prompts only
- protected paths untouched
- no Unity scenes changed
- no Packages/ProjectSettings changed
- no large assets added
- no dependencies added
- no gameplay changes
- automation scripts exist
- preflight script works
- Markdown records are produced
- open-source/official references are reviewed as reference_only unless approved
- P7/P8/P9/P10 boundaries are clear
- P7 has only five stages
- two-Codex workflow is documented.

17. codex_prompts/p7_0_reference_review_lod_strategy_auto.md
Keep this prompt for traceability.

After implementation:
- Run:
  powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_preflight.ps1
- Show the output summary.
- Show git diff --name-only.
- Confirm whether protected paths were untouched.
- Confirm whether Unity tests were intentionally not run because this is docs/tools/prompts-only.
- Recommend a commit message.
