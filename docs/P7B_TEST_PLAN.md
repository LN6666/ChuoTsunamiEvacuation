# P7-B Test Plan

Date: 2026-05-22

Stage: P7-B Wave 1

Status: Test plan only. No Unity tests are required for Wave 1 because no Unity files are changed.

## Wave 1 Validation

Required for this documentation-only pass:

- run `powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_preflight.ps1`
- inspect `git diff --name-only`
- confirm no protected Unity, PLATEAU, data, package, or project setting paths changed
- confirm P7 still has exactly five stages
- confirm no P8/P9 systems are implemented

## Wave 2 EditMode Tests

If Wave 2 adds editor helpers or scene validation, add focused EditMode coverage for:

- benchmark scene path is isolated from `Chuo_BaseMap.unity`
- selected benchmark area metadata is present
- object/material/texture metric collection handles empty or partial scenes safely
- generated benchmark summaries use approved output paths
- protected paths are rejected by helper code
- missing benchmark scene or missing area data fails clearly

EditMode tests should avoid importing broad data and should not require the full local PLATEAU source folder.

## Wave 2 PlayMode Tests

Run PlayMode tests if Wave 2 adds runtime components, runtime metric collection, scene behavior, or gameplay-facing objects.

PlayMode coverage should verify:

- loading the benchmark scene does not require `Chuo_BaseMap.unity`
- benchmark-only objects do not alter existing evacuation success/failure logic
- gameplay source defaults and humanitarian flags are unchanged
- optional runtime metric components fail closed when references are missing
- benchmark-only cameras or markers do not register as shelters, hazards, NPCs, or player-result systems

If Wave 2 is scene-only with no runtime behavior, document why PlayMode tests are skipped.

## P7 Preflight

Run after implementation:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_preflight.ps1
```

The preflight must pass before review. If it fails due to protected path changes, rollback or obtain explicit approval before proceeding.

## Protected Path Check

After Wave 2 changes, inspect:

```powershell
git diff --name-only
git status --short
```

Expected protected-path result for Wave 1:

- no `Assets/Scenes/`
- no `Assets/Scripts/`
- no `Assets/Data/`
- no `Assets/PLATEAU/`
- no `ProjectSettings/`
- no `Packages/`

For Wave 2, only explicitly approved Unity paths may appear.

## Benchmark Log Validation

If a benchmark Markdown record is created, validate it with:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/validate_p7_performance_log.ps1 -Path docs/p7_benchmark_records/example.md
```

The record should state whether each metric is measured, unavailable, or intentionally deferred. Placeholder values must not be used for pass/fail decisions.

## Chuo Base Map Protection

`Assets/Scenes/Chuo_BaseMap.unity` must not change unless explicitly approved in a later prompt. P7-B should use an isolated scene or documented manual benchmark procedure instead.

## Review Evidence

The P7-B review packet should include:

- preflight result
- Unity EditMode result if Unity files changed
- Unity PlayMode result if runtime/scene behavior changed
- benchmark record validation result if a record exists
- protected path check
- explanation for any intentionally skipped Unity tests
