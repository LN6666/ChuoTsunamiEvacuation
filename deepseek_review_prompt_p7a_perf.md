# DeepSeek Review Prompt: P7-A Benchmark / Performance Automation Prep

You are reviewing the current git diff for ChuoTsunamiEvacuation P7-A Codex B.

## Scope

This change should be docs/tools/prompts only. It prepares benchmark automation, performance-log validation, status warning traceability, and Windows EXE profiling documentation.

Do not recommend implementation of Unity benchmark scenes, gameplay changes, asset import, package changes, project settings changes, tsunami hazard systems, crowd simulation, spawn systems, or indoor evacuation systems for this P7-A Codex B diff.

## Required Checks

Confirm:

- No protected files changed: `ProjectSettings/`, `Packages/`, `Assets/Scenes/`, `Assets/PLATEAU/`, `Assets/Scripts/`, or `Assets/Data/`.
- No Unity scenes, gameplay scripts, imported assets, PLATEAU files, packages, or project settings changed.
- Guard strictness is preserved: protected paths still fail and large unsafe added files still fail.
- Warning filtering reports expected boundary/prompt/review context without hiding real unsafe keyword additions in ordinary files.
- Benchmark records remain Markdown-first under `docs/p7_benchmark_records/`.
- `tools/p7/new_p7_benchmark_record.ps1` does not run Unity.
- `tools/p7/validate_p7_performance_log.ps1` checks required fields but does not need numeric parsing yet.
- `tools/p7/run_p7_benchmark_preflight.ps1` runs the base P7 preflight and prints clear PASS/FAIL.
- No Unity tests are required for this diff unless Unity files changed.
- P7 still has exactly five stages: P7-0, P7-A, P7-B, P7-C, and P7-D.

## Files To Pay Attention To

- `tools/p7/check_p7_scope.ps1`
- `tools/p7/write_p7_status_report.ps1`
- `tools/p7/run_p7_preflight.ps1`
- `tools/p7/new_p7_benchmark_record.ps1`
- `tools/p7/validate_p7_performance_log.ps1`
- `tools/p7/run_p7_benchmark_preflight.ps1`
- `docs/P7_BENCHMARK_AUTOMATION_PLAN.md`
- `docs/P7_EXE_PROFILING_PREP.md`
- `docs/P7_PERFORMANCE_LOG_SCHEMA.md`
- `docs/P7_AUTOMATION_WORKFLOW.md`
- `docs/P7_BENCHMARK_PROTOCOL.md`
- `docs/P7_PERFORMANCE_METRICS_TEMPLATE.md`
- `docs/P7_TWO_CODEX_WORKFLOW.md`
- `docs/P7_DECISION_LOG.md`
- `docs/TASKS.md`
- `docs/REVIEW_BACKLOG.md`

## Output Requested

Return:

1. Verdict: PASS, PASS WITH B-LEVEL FOLLOW-UPS, or FAIL.
2. A-level blockers, if any.
3. B-level follow-ups, if any.
4. Confirmation that protected paths are untouched.
5. Confirmation that P7 stage count remains exactly five.
6. Confirmation that Unity tests are intentionally not required if no Unity files changed.
