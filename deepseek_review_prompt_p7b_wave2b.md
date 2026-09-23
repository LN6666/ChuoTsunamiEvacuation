# DeepSeek Review Prompt: P7-B Wave 2-B LOD3 Candidate Dry Run

You are reviewing the current git diff for:

PBL7 / P7-B Wave 2-B

Branch:

`p7b-wave2b-lod3-candidate-dryrun`

Main objective:

Review a conservative command-line-only LOD3 candidate dry-run package for candidate `53393690`, with fallback candidates `53393672` and `53394611`.

## Required Review Position

This Wave 2-B package must be dry-run-only.

It must not approve or perform real asset import.

Candidate `53393690` must remain a planning candidate only.

LOD3 must remain unverified by geometry or visuals.

LOD4 must not be assumed available.

Future real import must require explicit human approval.

## Files Expected In Diff

Allowed new files:

- `docs/P7B_WAVE2B_LOD3_CANDIDATE_DRYRUN.md`
- `docs/P7B_WAVE2B_IMPORT_DECISION.md`
- `docs/P7B_WAVE2B_VISUAL_FEASIBILITY_PLAN.md`
- `docs/P7B_WAVE2B_TEST_RESULTS.md`
- `docs/P7B_WAVE2B_KNOWN_LIMITATIONS.md`
- `tools/p7/inspect_p7b_lod3_candidate.ps1`
- `tools/p7/run_p7b_wave2b_preflight.ps1`
- `codex_prompts/p7b_wave2b_lod3_candidate_dryrun.md`
- `deepseek_review_prompt_p7b_wave2b.md`

Allowed shared updates:

- `docs/TASKS.md`
- `docs/REVIEW_BACKLOG.md`
- `docs/P7_DECISION_LOG.md`

`tools/p7/check_p7_scope.ps1` may change only if a strict dry-run-only Wave 2-B mode is required. If it changed, confirm default strictness was not weakened.

## Required Checks

Confirm:

- Wave 2-B is dry-run-only.
- No real asset import happened.
- No external PLATEAU data was copied into `Assets`.
- `Chuo_BaseMap.unity` is untouched.
- `ProjectSettings` is clean.
- `Packages` is clean.
- `Assets/Data` is unchanged.
- `Assets/PLATEAU` is unchanged.
- Existing gameplay scripts are unchanged.
- No production scene integration was performed.
- No Unity scene was created or modified.
- No P8 tsunami hazard, inundation, flood, risk-front, or light curtain system was implemented.
- No P9 crowd, real spawn, indoor evacuation, congestion, or failure system was implemented.
- P7 remains exactly five stages: P7-0, P7-A, P7-B, P7-C, and P7-D.
- The docs do not create P7-E, P7-F, or P7-G.
- Candidate `53393690` remains a dry-run planning candidate.
- Fallback candidates `53393672` and `53394611` remain fallback planning candidates.
- LOD3 is not visually verified.
- LOD3 geometry quality is not claimed.
- LOD4 is not assumed available.
- Future real import requires explicit approval.

## Tooling Review

Review `tools/p7/inspect_p7b_lod3_candidate.ps1`:

- It should use file/path metadata only.
- It should use `Get-ChildItem` / `FileInfo` style metadata enumeration.
- It should not parse CityGML content.
- It should not copy, move, delete, import, or mutate files.
- It should not write into `Assets/Data`, `Assets/PLATEAU`, or `D:\PLATEAU_DATA\Chuo_2025_CityGML`.
- It should summarize file paths, sizes, extensions, category hints, LOD path/name indicators, and import-footprint estimates.

Review `tools/p7/run_p7b_wave2b_preflight.ps1`:

- It should run the dry-run inspection.
- It should run the P7 scope guard.
- It should fail non-zero if protected paths are changed.
- It should fail if files outside the Wave 2-B allowlist are changed.
- It should verify no Unity production files changed.

## Validation Evidence

Expected command:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2b_preflight.ps1
```

Unity tests should be skipped only because this task is docs/tools/prompts-only and no Unity files changed.

Please classify findings:

- A-level: must fix before commit/push.
- B-level: follow-up allowed after commit if no safety issue.
- C-level: notes or polish.

Return an overall verdict and explicitly state whether commit/push is safe.
