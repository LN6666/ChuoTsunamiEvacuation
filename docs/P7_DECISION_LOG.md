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
| Follow-up | Human review should choose one candidate and approve a Markdown import/benchmark plan before Unity import work starts. |
| Approved by | P7-B area feasibility prompt. |
