# P7 Stage Plan

Date: 2026-05-22

Phase: PBL7 / P7

Status: P7-0 foundation in progress.

## Purpose

P7 upgrades the Chuo city environment foundation for high-detail / full Chuo asset loading, underground and bridge feasibility, LOD strategy, chunk loading, visual quality, and Windows x64 EXE performance optimization.

P7 is not a gameplay-rule phase. It must preserve the existing P6 behavior baseline and must not add tsunami hazard systems, indoor evacuation systems, new success/failure rules, real crowd simulation, or road-blocking gameplay.

## Relationship With Other Phases

| Phase | Relationship |
|---|---|
| P6 | P6 completed navigation guidance and lightweight NPC evacuation behavior. P7 must not change P6 gameplay behavior unless a later P7 stage explicitly authorizes a small integration fix. |
| P8 | P8 is reserved for hazard systems such as tsunami height, inundation depth, and dynamic light curtain work. P7 must not implement these systems. |
| P9 | P9 is reserved for indoor shelter / indoor evacuation work. P7 must not implement indoor navigation or indoor shelter gameplay. |
| P10 | P10 is reserved for final integration, presentation, and closeout work after earlier phases are stable. P7 should produce measurable asset/performance evidence that P10 can cite. |

## Fixed P7 Stage Count

P7 has exactly five stages:

| Stage | Name | Scope | Primary Deliverables | Required Checks |
|---|---|---|---|---|
| P7-0 | Scope Freeze + Reference Review + Automation Foundation | Docs, tools, prompts only. No Unity assets, scenes, scripts, packages, settings, or gameplay changes. | P7 stage docs, scope boundaries, reference review, LOD strategy, inventory protocol, benchmark protocol, metrics template, decision log, automation workflow, two-Codex workflow, preflight scripts, DeepSeek prompt. | `tools/p7/run_p7_preflight.ps1`; DeepSeek review after implementation. |
| P7-A | Chuo Asset Inventory + LOD / Area Selection | Command-line-friendly local inventory and area selection planning. No full import until inventory and benchmark plan are accepted. | Inventory Markdown/CSV records, candidate LOD map, area priority list, import risk notes. | P7 scope guard; Unity tests only if Unity code/assets/scenes are changed. |
| P7-B | Small-Area High-Detail Benchmark + Underground / Bridge / Road Feasibility | Small selected area benchmark before full Chuo import. | Benchmark scene/procedure records, underground/bridge/road feasibility notes, rollback criteria. | Editor benchmark, Windows x64 benchmark when build exists, Unity tests for any code/assets/scenes changed. |
| P7-C | Streaming / Chunk Loading + Visual Quality + Performance Optimization | Implement approved chunk loading or LOD optimization only after P7-B evidence. | Chunk/LOD workflow, visual quality settings, performance optimization records. | Unity automated tests for Unity changes; performance metrics; scope guard. |
| P7-D | Windows EXE Profiling + P7 Final Closeout | Final profiling, documentation, review, and handoff. | Windows EXE profiling report, final P7 closeout, DeepSeek review prompt/report summary. | Windows x64 profiling; Unity tests for changed Unity content; scope guard; DeepSeek final review. |

Do not create P7-E, P7-F, or P7-G. Any prompt, branch, document, task entry, or script output that implies P7-E/P7-F/P7-G is a scope error unless it is explicitly warning that those stages must not exist.

## Stage Deliverables

P7-0 deliverables:

- `docs/P7_STAGE_PLAN.md`
- `docs/P7_BOUNDARIES.md`
- `docs/P7_REFERENCE_REVIEW.md`
- `docs/P7_LOD_ASSET_STRATEGY.md`
- `docs/P7_ASSET_INVENTORY_PROTOCOL.md`
- `docs/P7_BENCHMARK_PROTOCOL.md`
- `docs/P7_PERFORMANCE_METRICS_TEMPLATE.md`
- `docs/P7_DECISION_LOG.md`
- `docs/P7_AUTOMATION_WORKFLOW.md`
- `docs/P7_TWO_CODEX_WORKFLOW.md`
- `tools/p7/check_p7_scope.ps1`
- `tools/p7/write_p7_status_report.ps1`
- `tools/p7/run_p7_preflight.ps1`
- `codex_prompts/p7_0_reference_review_lod_strategy_auto.md`
- `deepseek_review_prompt_p70.md`

P7-A through P7-D must add only the files explicitly approved by their stage prompts and Markdown plans.

## Checks And Tests

P7-0 uses command-line preflight checks instead of Unity tests because it is docs/tools/prompts only. The required P7-0 command is:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_preflight.ps1
```

P7-A, P7-B, P7-C, and P7-D must run automated Unity EditMode and/or PlayMode tests whenever Unity code, Unity assets, Unity scenes, or gameplay-facing configuration are changed. When a stage is documentation-only, the reason Unity tests were skipped must be recorded.

## DeepSeek Review Expectation

Each stable P7 milestone should receive a DeepSeek V4 Pro review of the current git diff. P7-0 review must verify:

- P7-0 is docs/tools/prompts only.
- Protected Unity and PLATEAU paths are untouched.
- The P7 stage count is exactly five.
- Automation scripts exist and run.
- References are reviewed conservatively and no optional dependency is adopted.
- P7/P8/P9/P10 boundaries are clear.

## Commit And Push Expectation

Commit after each stable milestone. Keep commits small and meaningful. P7-0 should be committed only after preflight passes and the human developer accepts the P7-0 DeepSeek review result.
