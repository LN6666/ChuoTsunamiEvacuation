# P7 Decision Log

This file stores P7 decisions that affect scope, dependencies, assets, Unity settings, import size, benchmark acceptance, or phase boundaries.

## Decision Entry Template

### P7-DL-000 - Short Decision Title

| Field | Value |
|---|---|
| Date | YYYY-MM-DD |
| Stage | P7-0 / P7-A / P7-B / P7-C / P7-D |
| Decision |  |
| Options considered |  |
| Reason |  |
| Risk |  |
| Verification |  |
| Follow-up |  |
| Approved by |  |

## Active Decisions

### P7-DL-001 - P7-0 Adopts Documentation And Automation Only

| Field | Value |
|---|---|
| Date | 2026-05-22 |
| Stage | P7-0 |
| Decision | P7-0 creates docs, tools, prompts, and review gates only. It does not modify Unity assets, scenes, scripts, data, packages, project settings, or gameplay. |
| Options considered | Start with asset import; start with dependency/streaming tool adoption; start with docs/tools/reference review. |
| Reason | Full Chuo high-detail import and streaming decisions are high risk without inventory, benchmark protocol, and scope guards. |
| Risk | P7-A may still uncover asset sizes or missing LOD/category data that require scope reduction. |
| Verification | Run `tools/p7/run_p7_preflight.ps1`; verify protected paths are untouched; request DeepSeek review. |
| Follow-up | P7-A asset inventory and area/LOD selection. |
| Approved by | User after P7-0 DeepSeek PASS. |

### P7-DL-002 - P7 Has Exactly Five Stages

| Field | Value |
|---|---|
| Date | 2026-05-22 |
| Stage | P7-0 |
| Decision | P7 stages are P7-0, P7-A, P7-B, P7-C, and P7-D only. |
| Options considered | Continuing to P7-E/P7-F/P7-G; using a five-stage plan. |
| Reason | User instruction fixed P7 to five stages. |
| Risk | Future prompts or generated docs may accidentally create extra stages. |
| Verification | Scope guard warns on P7-E/P7-F/P7-G keywords; DeepSeek review checks stage count. |
| Follow-up | Keep all P7 task records aligned to five stages. |
| Approved by | User after P7-0 DeepSeek PASS. |

### P7-DL-003 - P7-A Uses Command-Line-First Asset Inventory

| Field | Value |
|---|---|
| Date | 2026-05-22 |
| Stage | P7-A |
| Decision | P7-A inventories local assets with PowerShell scripts that run without opening Unity and write Markdown reports for asset, LOD, and benchmark-candidate review. |
| Options considered | Manual Unity inspection; direct asset import; command-line metadata scan first. |
| Reason | P7 needs file counts, extension summaries, large-file risks, PLATEAU category hints, and benchmark candidates before any import or scene work. |
| Risk | Path/name inference can miss actual CityGML LOD details and cannot verify imported geometry quality. |
| Verification | Run `tools/p7/run_p7_asset_inventory.ps1`, then verify reports and P7 preflight result. |
| Follow-up | Use the inventory to narrow P7-B benchmark area planning before any Unity import work. |
| Approved by | P7-A prompt. |

### P7-DL-004 - Prepare Benchmark And Performance Automation Before Asset Decisions

| Field | Value |
|---|---|
| Date | 2026-05-22 |
| Stage | P7-A |
| Decision | Add command-line benchmark record creation, Markdown performance-log validation, benchmark preflight orchestration, warning-summary traceability, and Windows EXE profiling preparation before P7-B/C/D asset-performance decisions. |
| Options considered | Leave benchmark records manual only; create Unity benchmark automation immediately; add docs/tools-only preparation first. |
| Reason | P7-B and later stages need repeatable performance records, but P7-A Codex B must not import assets, change Unity files, create builds, or add dependencies. |
| Risk | Early benchmark records may contain placeholders until Unity benchmark scenes or EXE builds exist. |
| Verification | Run `tools/p7/run_p7_benchmark_preflight.ps1`; validate any created benchmark record with `tools/p7/validate_p7_performance_log.ps1`; confirm protected paths remain untouched. |
| Follow-up | P7-B should use the schema for small-area Editor benchmarks and optional Windows EXE benchmarks when a build exists. P7-D must record Windows EXE profiling before closeout. |
| Approved by | P7-A implementation prompt. |

### P7-DL-005 - P7-B Starts With Feasibility Before Import

| Field | Value |
|---|---|
| Date | 2026-05-22 |
| Stage | P7-B |
| Decision | P7-B starts with command-line feasibility and candidate-area selection before any Unity scene work, asset import, or generated benchmark artifact. LOD4 is not assumed available from the current path/name inventory. |
| Options considered | Import a high-detail area immediately; choose a full Chuo high-detail import; create a conservative feasibility package first. |
| Reason | P7-A found large source folders, mostly unknown LOD tokens, limited LOD3 path/name signals, and no LOD4 path/name evidence. A small candidate set must be reviewed before import risk is accepted. |
| Risk | LOD3 path/name signals may not correspond to usable geometry; underground, bridge, road, and riverfront categories may not import into useful Unity context. |
| Verification | Run `tools/p7/run_p7b_area_feasibility.ps1`; verify protected paths are untouched; request DeepSeek review with `deepseek_review_prompt_p7b_area.md`. |
| Follow-up | Human review should choose one candidate and approve a Markdown import/benchmark plan before Unity import work starts. Current path/name-only planning prefers `53393690`, keeps `53393672` and `53394611` as fallbacks, and records zero LOD4 path/name hits. |
| Approved by | P7-B area feasibility prompt. |

### P7-DL-006 - P7-B Wave 1 Does Not Mutate Unity

| Field | Value |
|---|---|
| Date | 2026-05-22 |
| Stage | P7-B |
| Decision | P7-B Wave 1 prepares benchmark harness design, future Unity change proposal, test plan, rollback plan, task/backlog/protocol updates, and review prompt only. It does not create or modify Unity scenes, scripts, assets, data, packages, project settings, or imported PLATEAU assets. |
| Options considered | Create the benchmark scene immediately; import a small PLATEAU cluster immediately; prepare docs/prompts and approval gate first. |
| Reason | P7-B needs a safe implementation design before any Unity mutation because benchmark scene creation and asset import can affect protected paths and large generated files. |
| Risk | Wave 2 may still require Unity Editor interaction, heavier-than-expected imports, or separate Editor and EXE metric collection. |
| Verification | Run `tools/p7/run_p7_preflight.ps1`; inspect `git diff --name-only`; confirm protected paths remain untouched. |
| Follow-up | P7-B Wave 2 requires explicit approval before creating benchmark scenes, scripts, assets, imports, or generated Unity artifacts. |
| Approved by | P7-B Wave 1 prompt. |

### P7-DL-007 - P7-B Wave 2-A Allows Only Isolated Benchmark Skeleton

| Field | Value |
|---|---|
| Date | 2026-05-22 |
| Stage | P7-B |
| Decision | P7-B Wave 2-A is approved only for an isolated benchmark scene skeleton and prototype metrics harness under `P7Benchmark` paths. No real asset import is approved. |
| Options considered | Import LOD3 candidate assets now; modify `Chuo_BaseMap.unity`; create only a primitive benchmark skeleton and metrics harness. |
| Reason | Wave 2-A needs a Unity-side harness foundation before risking real PLATEAU asset import, scene growth, or project-wide settings changes. |
| Risk | The skeleton scene cannot validate real geometry quality or performance. Unity launch may create unwanted `ProjectSettings` or `Packages` churn that must be reverted. |
| Verification | Run `tools/p7/run_p7b_wave2a_preflight.ps1`; run GUI EditMode and PlayMode tests; inspect `git diff --name-only`; confirm protected paths are untouched. |
| Follow-up | Wave 2-B or later must receive explicit approval before importing LOD3 candidate data, copying real assets, or generating PLATEAU-derived Unity assets. |
| Approved by | P7-B Wave 2-A prompt. |

### P7-DL-008 - P7-B Wave 2-B Allows Only Read-Only LOD3 Candidate Dry Run

| Field | Value |
|---|---|
| Date | 2026-05-23 |
| Stage | P7-B |
| Decision | P7-B Wave 2-B is approved only for read-only LOD3 candidate dry-run metadata feasibility. It does not approve full PLATEAU import, production scene integration, or real asset import. |
| Options considered | Import candidate `53393690`; copy candidate assets into Unity; run a command-line metadata dry run only. |
| Reason | P7-B Wave 1 selected `53393690` as a path/name planning candidate, but LOD3 is not visually verified, LOD4 is not available from current evidence, and candidate footprint may still be too heavy. |
| Risk | Candidate discovery may be incomplete, path/name metadata may not reflect geometry quality, and a future import may still exceed practical benchmark limits. |
| Verification | Run `tools/p7/run_p7b_wave2b_preflight.ps1`; verify `Chuo_BaseMap.unity`, `ProjectSettings`, `Packages`, `Assets/Data`, `Assets/PLATEAU`, Unity scenes, and gameplay scripts are untouched; request DeepSeek review with `deepseek_review_prompt_p7b_wave2b.md`. |
| Follow-up | Wave 2-C or any future real import step needs separate explicit human approval with a confirmed Markdown plan, rollback criteria, protected-path checks, and Unity tests for Unity changes. |
| Approved by | P7-B Wave 2-B prompt. |

### P7-DL-009 - P7-B Wave 2-C Allows Only 53393690 P7Benchmark Sandbox Import

| Field | Value |
|---|---|
| Date | 2026-05-23 |
| Stage | P7-B |
| Decision | P7-B Wave 2-C is approved only for importing the full candidate package `53393690` into `Assets/P7Benchmark/Imported/53393690/`. |
| Options considered | Keep dry-run only; import narrowed subset; import the full approved `53393690` package into a sandbox. |
| Reason | The human developer explicitly approved the full `53393690` candidate footprint of 605.38 MB for sandbox feasibility because the size is acceptable for P7Benchmark isolation. |
| Risk | Raw CityGML and texture source files may not be directly renderable in Unity; Git storage needs narrow LFS tracking; Unity refresh may create unwanted protected-path churn. |
| Verification | Run `tools/p7/run_p7b_wave2c_preflight.ps1`; run GUI EditMode and PlayMode tests; verify `Chuo_BaseMap.unity`, `ProjectSettings`, `Packages`, `Assets/Data`, `Assets/PLATEAU`, production scenes, and existing gameplay scripts are untouched; request DeepSeek review with `deepseek_review_prompt_p7b_wave2c.md`. |
| Follow-up | Any CityGML conversion, visual verification, production scene integration, full Chuo import, or P8/P9 work requires separate explicit approval. |
| Approved by | P7-B Wave 2-C prompt. |

### P7-DL-010 - P7-C Allows Only P7Benchmark Chunk Loading And Performance Foundation

| Field | Value |
|---|---|
| Date | 2026-05-23 |
| Stage | P7-C |
| Decision | P7-C is approved only for P7Benchmark sandbox chunk/loading, visual-quality review, and approximate performance telemetry. It does not approve production scene integration, full Chuo import, `ProjectSettings` changes, `Packages` changes, P8 systems, or P9 systems. |
| Options considered | Integrate `53393690` into production scenes; attempt full Chuo streaming; create a metadata-first benchmark foundation. |
| Reason | The imported candidate is still raw CityGML and texture source data. A safe benchmark registry and placeholder chunk controller can prepare profiling without overclaiming renderability or touching protected production paths. |
| Risk | CityGML conversion, visual verification, authoritative profiling, and full production streaming remain unresolved. |
| Verification | Run `tools/p7/run_p7c_preflight.ps1`; run GUI EditMode and PlayMode tests; verify `Chuo_BaseMap.unity`, `ProjectSettings`, `Packages`, `Assets/Data`, `Assets/PLATEAU`, production scenes, and existing gameplay scripts are untouched; request DeepSeek review with `deepseek_review_prompt_p7c.md`. |
| Follow-up | P7-D must perform Windows EXE profiling and final closeout. CityGML conversion and production streaming require separate approval. |
| Approved by | P7-C implementation prompt. |

### P7-DL-011 - P7-C Continuation Prepares High-Detail Chuo Baseline

| Field | Value |
|---|---|
| Date | 2026-05-23 |
| Stage | P7-C |
| Decision | P7-C continuation is approved for a separate high-detail Chuo scene shell, PLATEAU SDK target checklist, LOD coverage assessment, P2-P6 compatibility validation plan, P7-D profiling target definition, upgraded validators, and asset persistence documentation. |
| Options considered | Continue with benchmark-only P7-C; modify `Chuo_BaseMap.unity`; create a separate high-detail P7 scene target. |
| Reason | P7-D must profile the real high-detail scene target, not only the old benchmark skeleton. The new scene must be prepared without mutating `Chuo_BaseMap.unity`, production gameplay scenes, `Assets/Data`, `Assets/PLATEAU`, `ProjectSettings`, or `Packages`. |
| Risk | The scene shell can still be metadata-only until PLATEAU SDK import succeeds. Average LOD3, road/bridge/underground renderability, and P2-P6 runtime compatibility remain pending until actual assets are loaded and profiled. |
| Verification | Run `tools/p7/run_p7c_preflight.ps1`; run GUI EditMode and PlayMode tests because Unity scripts/tests/scene change; verify protected paths are clean; request DeepSeek review with `deepseek_review_prompt_p7c.md`. |
| Follow-up | P7-D must perform Windows EXE profiling on `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` after actual assets are loaded or explicitly document the import blocker. P8/P9/P10 should use this new scene as baseline only after P7-D confirmation. |
| Approved by | P7-C continuation prompt. |

### P7-DL-012 - P7-D Blocks On Manual PLATEAU SDK Import

| Field | Value |
|---|---|
| Date | 2026-05-23 |
| Stage | P7-D |
| Decision | P7-D cannot safely complete autonomous full high-detail PLATEAU import. It records a manual Unity PLATEAU SDK import checklist and blocks final closeout until import, compatibility smoke checks, and Windows EXE profiling are completed. |
| Options considered | Run SDK code import against raw local source; copy full source into `Assets/P7HighDetail`; stop with manual import checklist. |
| Reason | The requested categories total about 6.49 GB before conversion, the SDK local import path can create side-effect files beside source data, generated scene/assets may be large, and there is no approved Git/LFS/cloud archive decision for those generated outputs. |
| Risk | P8/P9 cannot start on the high-detail scene until manual import and P7-D validation are completed. |
| Verification | Run `tools/p7/run_p7d_preflight.ps1`; run Unity GUI EditMode and PlayMode tests; verify protected paths are clean; request DeepSeek final review with `deepseek_review_prompt_p7d_final.md`. |
| Follow-up | Complete `docs/P7D_MANUAL_PLATEAU_IMPORT_CHECKLIST.md`, then rerun P7-D validation and profiling. |
| Approved by | P7-D execution prompt plus project safety constraints. |
