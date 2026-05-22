# P7 Two-Codex Workflow

Date: 2026-05-22

## Purpose

P7 work can split into two Codex directions after P7-0, but only with clear file ownership and integration rules. The goal is to reduce conflict between reference/asset decisions and automation/performance work.

## Recommended Future Worktrees

- `D:\UnityProjects\ChuoTsunamiEvacuation-P7Asset`
- `D:\UnityProjects\ChuoTsunamiEvacuation-P7Perf`

These worktrees are recommendations for future parallel work. P7-0 does not create them.

## Codex A: Reference / Asset / LOD / PLATEAU Direction

Primary responsibilities:

- PLATEAU source/category/LOD review.
- Asset inventory protocol execution in P7-A.
- Key-area and LOD selection records.
- Underground/bridge/road feasibility notes.
- Asset import risk documentation.

Preferred file ownership:

- `docs/P7_REFERENCE_REVIEW.md`
- `docs/P7_LOD_ASSET_STRATEGY.md`
- `docs/P7_ASSET_INVENTORY_PROTOCOL.md`
- Future `docs/p7_inventory/` records.
- Future asset/LOD decision logs when approved.

Codex A must not modify protected paths unless a later stage prompt explicitly approves the exact change.

## Codex B: Automation / Benchmark / Performance / EXE Direction

Primary responsibilities:

- Scope guard and preflight tooling.
- Benchmark protocol and metrics templates.
- Windows x64 EXE profiling workflow.
- Performance bottleneck records.
- P7 status report automation.

Preferred file ownership:

- `tools/p7/`
- `docs/P7_AUTOMATION_WORKFLOW.md`
- `docs/P7_BENCHMARK_PROTOCOL.md`
- `docs/P7_PERFORMANCE_METRICS_TEMPLATE.md`
- `docs/P7_BENCHMARK_AUTOMATION_PLAN.md`
- `docs/P7_EXE_PROFILING_PREP.md`
- `docs/P7_PERFORMANCE_LOG_SCHEMA.md`
- Future benchmark/profiling records.

Codex B must not modify protected paths unless a later stage prompt explicitly approves the exact change.

## Shared Files

Only an integration Codex or the human developer should update shared coordination files after parallel work starts:

- `docs/TASKS.md`
- `docs/REVIEW_BACKLOG.md`
- `docs/P7_STAGE_PLAN.md`
- `docs/P7_BOUNDARIES.md`
- `docs/P7_DECISION_LOG.md`
- DeepSeek review prompts.
- Codex prompt records.

Parallel Codex agents should avoid simultaneous edits to shared files.

## Conflict Prevention Rules

- Each Codex agent must state its file ownership before editing.
- Do not edit a file owned by the other Codex agent without coordination.
- Do not rename stage files during parallel work.
- Do not rewrite large Markdown documents for style-only reasons.
- Do not reformat unrelated sections.
- Use small, stage-specific commits.
- Run P7 preflight before handing work to the integration Codex.

## Protected Files For Both Codex Agents

Both Codex agents must not touch:

- `ProjectSettings/`
- `Packages/`
- `Assets/Scenes/`
- `Assets/Scenes/Chuo_BaseMap.unity`
- `Assets/PLATEAU/`
- `Assets/Scripts/`
- `Assets/Data/`
- Local raw PLATEAU data under `D:\PLATEAU_DATA\Chuo_2025_CityGML`

No simultaneous edits are allowed to `ProjectSettings`, `Packages`, `Chuo_BaseMap`, PLATEAU imported assets, core gameplay files, or `Assets/Data`.

## Integration Rule

Before merging parallel outputs:

1. Run `tools/p7/run_p7_preflight.ps1` in each worktree.
2. Compare `git diff --name-only`.
3. Resolve shared-file edits manually.
4. Confirm P7 still has only P7-0, P7-A, P7-B, P7-C, and P7-D.
5. Request DeepSeek review on the integrated diff.
