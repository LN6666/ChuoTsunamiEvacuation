# DeepSeek Review Prompt: P7-0 Reference Review + LOD Strategy Foundation

You are DeepSeek V4 Pro reviewing the current git diff for the Unity project ChuoTsunamiEvacuation.

Phase:
PBL7 / P7-0

P7-0 scope:
Docs, tools, Codex prompt record, and DeepSeek review prompt only.

Critical rule:
P7 has exactly five stages:

- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not accept any plan that creates P7-E, P7-F, or P7-G.

## Review Goals

Check whether the diff safely creates the P7-0 foundation for:

- Official/GitHub reference review.
- LOD / optimization strategy.
- Asset inventory protocol.
- Benchmark protocol.
- Performance metrics template.
- Decision log.
- Automation workflow.
- Two-Codex workflow.
- Scope guard / status report / preflight scripts.

## Required Checks

Verify all of the following:

1. P7-0 is docs/tools/prompts only.
2. Protected paths are untouched:
   - `ProjectSettings/`
   - `Packages/`
   - `Assets/Scenes/`
   - `Assets/PLATEAU/`
   - `Assets/Scripts/`
   - `Assets/Data/`
3. No Unity scenes are changed, including `Assets/Scenes/Chuo_BaseMap.unity`.
4. No `Packages/` or `ProjectSettings/` changes exist.
5. No large assets are added.
6. No dependencies are added.
7. No gameplay changes exist.
8. Automation scripts exist:
   - `tools/p7/check_p7_scope.ps1`
   - `tools/p7/write_p7_status_report.ps1`
   - `tools/p7/run_p7_preflight.ps1`
9. The preflight script works and exits non-zero if the scope guard fails.
10. Markdown records are produced, including a P7 status report.
11. Open-source and official references are reviewed as `reference_only` unless explicitly approved; optional dependencies must not be claimed as adopted.
12. P7/P8/P9/P10 boundaries are clear.
13. P7 has only five stages.
14. Two-Codex workflow is documented with clear file ownership and protected paths.

## Scope Risks To Look For

Flag any accidental addition of:

- Tsunami height.
- Inundation depth.
- Dynamic light curtain.
- P8 hazard systems.
- P9 indoor shelter or indoor evacuation systems.
- Real crowd simulation.
- NPC failure mechanics.
- Road-blocking gameplay.
- Building-collapse gameplay.
- Real spawn point systems.
- New gameplay success/failure rules.
- Asset import or large data download steps.

## Expected Files

P7-0 should create or update:

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
- `docs/TASKS.md`
- `docs/REVIEW_BACKLOG.md`
- `tools/p7/check_p7_scope.ps1`
- `tools/p7/write_p7_status_report.ps1`
- `tools/p7/run_p7_preflight.ps1`
- `codex_prompts/p7_0_reference_review_lod_strategy_auto.md`
- `deepseek_review_prompt_p70.md`
- Generated `docs/p7_status/` Markdown status report if preflight was run.

## Output Format

Return:

1. Verdict: PASS / PASS WITH B-LEVEL FOLLOW-UPS / FAIL.
2. A-level blockers, if any.
3. B-level follow-ups, if any.
4. Protected-path confirmation.
5. Stage-count confirmation.
6. Automation/preflight confirmation.
7. Reference/adoption confirmation.
8. Final recommendation before commit.
