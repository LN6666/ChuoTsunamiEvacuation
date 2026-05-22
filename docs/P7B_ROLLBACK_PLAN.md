# P7-B Rollback Plan

Date: 2026-05-22

Stage: P7-B Wave 1

Status: Rollback plan only. No rollback action is required for Wave 1 unless preflight exposes an unsafe diff.

## Wave 1 Rollback

Wave 1 is documentation-only. If review rejects this pass, rollback should be limited to the new P7-B Markdown documents, shared-doc entries, and review prompt created by this branch.

Do not touch Unity scenes, imported assets, scripts, data files, packages, or project settings during Wave 1 rollback.

## Wave 2 Rollback Principles

If a later approved Wave 2 creates Unity benchmark content, rollback should remove only the benchmark-specific branch changes:

- benchmark scene
- benchmark-only editor helpers
- benchmark-only tests
- generated benchmark metadata approved for tracking
- shared-doc decision or task entries tied to the rejected change

Rollback must avoid modifying `Assets/Scenes/Chuo_BaseMap.unity`.

## Revert Strategy

Preferred order:

1. Stop new benchmark work immediately after a fail condition.
2. Record the failing condition in the benchmark Markdown record or decision log.
3. Use normal branch revert or a focused commit that removes only benchmark branch files.
4. Re-run P7 preflight.
5. Re-run Unity tests if Unity files were touched before rollback.
6. Confirm `git diff --name-only` no longer includes unapproved protected paths.

Do not use broad destructive Git commands unless the human developer explicitly requests them.

## Generated Benchmark Scene

If Wave 2 creates an isolated scene such as:

```text
Assets/Scenes/P7B_SmallAreaBenchmark.unity
```

and the benchmark is rejected, remove that scene and any approved companion metadata from the branch. Do not edit or regenerate `Chuo_BaseMap.unity` as part of rollback.

## Benchmark Outputs

Keep committed benchmark outputs small and reviewable:

- Markdown summaries are preferred.
- CSV summaries are acceptable when approved and small.
- Large profiler captures, raw screenshots, player builds, cache folders, imported raw data, and generated large Unity artifacts should stay out of Git unless explicitly approved.

If large generated outputs appear in Git status, remove them from the branch before review and record the cleanup.

## Rollback Triggers

Rollback or reduce scope if:

- protected paths changed without approval
- `Chuo_BaseMap.unity` changed
- broad Chuo import appears in the diff
- new dependencies or package/project settings changes appear
- P7 preflight fails
- Unity tests fail after Unity changes
- benchmark record validation fails
- Editor or EXE performance misses the minimum prototype threshold after scope reduction
- generated outputs are too large or include raw PLATEAU data

## Post-Rollback Verification

After rollback:

- run P7 preflight
- inspect `git status --short`
- inspect `git diff --name-only`
- confirm only approved docs/prompts remain changed
- document whether Unity tests were rerun or intentionally skipped
