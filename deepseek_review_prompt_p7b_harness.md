# DeepSeek Review Prompt: P7-B Benchmark Harness Wave 1

You are reviewing P7-B Wave 1 for the ChuoTsunamiEvacuation Unity + PLATEAU project.

Review only the current git diff. This pass should be docs/prompts only.

## Review Scope

Confirm:

- changed files are limited to allowed P7-B Wave 1 docs/prompts and explicitly allowed shared docs
- no protected path changes exist
- no Unity scene, script, asset, data, package, or project setting was modified
- no imported PLATEAU asset or local raw PLATEAU data was modified
- P7 has exactly five stages: P7-0, P7-A, P7-B, P7-C, and P7-D
- the future Unity change proposal is explicit, narrow, and gated by human approval
- P7-B Wave 1 does not implement P8/P9 systems
- benchmark scene work is deferred to a future approved Wave 2
- `Chuo_BaseMap.unity` remains protected
- P7 preflight was run and the result is reported

## Files Expected In This Review

Expected new files:

- `docs/P7B_BENCHMARK_HARNESS_DESIGN.md`
- `docs/P7B_UNITY_CHANGE_PROPOSAL.md`
- `docs/P7B_TEST_PLAN.md`
- `docs/P7B_ROLLBACK_PLAN.md`
- `deepseek_review_prompt_p7b_harness.md`

Expected possible updates:

- `docs/TASKS.md`
- `docs/REVIEW_BACKLOG.md`
- `docs/P7_DECISION_LOG.md`
- `docs/P7_BENCHMARK_PROTOCOL.md`
- `docs/P7_BENCHMARK_AUTOMATION_PLAN.md`
- `docs/p7_status/p7_status_latest.md` if P7 preflight updated it

## Reject Conditions

Reject or flag as high severity if:

- any protected Unity or PLATEAU path changed without approval
- the proposal permits changing `Chuo_BaseMap.unity`
- the docs imply broad Chuo import is approved now
- dependencies, packages, project settings, builds, or imported assets were added
- benchmark outputs include large raw/profiler/build artifacts
- P7 stage count is expanded beyond the five approved stages
- Wave 2 Unity implementation is not clearly gated

## Output Requested

Return:

- verdict: PASS / PASS WITH FOLLOW-UPS / FAIL
- protected path assessment
- stage-count assessment
- Unity mutation assessment
- approval-gate assessment
- P8/P9 boundary assessment
- specific file/line issues ranked by severity
- recommended small Codex follow-up tasks, if any
