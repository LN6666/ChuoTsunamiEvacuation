# DeepSeek Review Prompt: P7-B Wave 2-B Main-Branch Integration

You are reviewing the main P7 branch integration of:

`origin/p7b-wave2b-lod3-candidate-dryrun`

into:

`p7-high-detail-city-foundation`

Integrated Wave 2-B commit:

`764c89a docs(p7-b): add LOD3 candidate dry-run package`

## Integration Context

P7-B Wave 2-B is a command-line-only LOD3 candidate dry-run package. It should remain docs/tools/prompts only.

Wave 2-B previously passed:

- Wave 2-B preflight: PASS
- DeepSeek final staged review: PASS, safe to commit/push
- A-level blockers: none
- Protected paths clean
- No Unity files changed
- No PLATEAU assets imported, copied, moved, or written into Unity
- Unity tests skipped correctly because no Unity files changed

## Fixed P7 Stage Count

P7 has exactly five stages:

- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not create P7-E, P7-F, or P7-G.

## Candidate Dry-Run Summary

- `53393690`: found, 5,843 files, 605.38 MB, 70 LOD3 path/name hits, 0 LOD4 hits, estimate bounded but not small.
- `53393672`: fallback, 658 files, 61.22 MB, 0 LOD3 hits, 0 LOD4 hits, estimate bounded-small fallback.
- `53394611`: fallback, 4,104 files, 1.25 GB, 0 LOD3 hits, 0 LOD4 hits, estimate too heavy.

These are path/name/file-metadata findings only. LOD3 is not visually verified. LOD4 is not assumed available.

## Required Review Checks

Confirm the integration:

- cleanly brings Wave 2-B dry-run docs/tools/prompts into the main P7 branch,
- does not modify `Chuo_BaseMap.unity`,
- does not modify `ProjectSettings`,
- does not modify `Packages`,
- does not modify `Assets/Data`,
- does not modify `Assets/PLATEAU`,
- does not modify existing gameplay scripts,
- does not modify production scenes,
- does not import or copy PLATEAU assets,
- does not implement P8 hazard systems,
- does not implement P9 crowd, spawn, congestion, or interior-shelter systems,
- keeps `53393690` as a planning candidate only,
- keeps `53393672` and `53394611` as fallback planning candidates only,
- keeps LOD3 visual verification deferred,
- keeps LOD4 unavailable based on current path/name evidence,
- requires separate explicit human approval before any real LOD3 import,
- preserves P7 as exactly five stages.

## Validation Evidence To Check

The main worktree should have run:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2b_preflight.ps1
```

Expected result:

- preflight PASS,
- candidate inspection PASS,
- P7 scope guard PASS,
- Wave 2-B allowlist PASS,
- protected path check PASS,
- Unity tests skipped because no Unity files changed.

## Classification

Classify findings as:

- A-level: must fix before push.
- B-level: follow-up allowed after push if no safety issue.
- C-level: note or polish only.

Return an overall verdict and explicitly state whether pushing `p7-high-detail-city-foundation` is safe.
