# DeepSeek Review Prompt: P8-A Baseline Hazard Data Layer

Review the staged git diff for ChuoTsunamiEvacuation P8-A.

## Required Verdict Format

Return:

- A-Level Blockers
- B-Level Follow-Ups
- C-Level Notes
- Protected Path Review
- Baseline Preservation Review
- P8 Stage Count Review
- Hazard Data Layer Review
- P2-P6 Compatibility Review
- Test Review
- Overall Verdict

## A-Level Blocker Criteria

Flag as A-level if any item is true:

- P8 does not have exactly five stages: P8-A, P8-B, P8-C, P8-D, P8-E.
- Extra P8 stages are introduced.
- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` is reset, deleted, overwritten, staged unsafely, or no longer preserved as local baseline.
- `Assets/Scenes/Chuo_BaseMap.unity` is modified.
- `ProjectSettings` or `Packages` are modified without explicit justification.
- `Assets/PLATEAU` is modified.
- `Assets/Data` is modified outside isolated `Assets/Data/P8`.
- P9 or P10 systems are implemented.
- P8-A implements dynamic light curtain rendering, hazard interaction behavior, or collapse gameplay behavior.
- Gameplay success/failure rules are changed.
- Hazard schema merges scientific tsunami values with cinematic visual height.
- Large `visualHeightMeters` can pass without `visualHeightIsCinematicOnly=true`.
- Collapse proxy fields trigger gameplay behavior in P8-A.
- Loader fails open instead of failing safe for missing or invalid data.
- P2-P6 compatibility gate is missing.
- P8-A preflight, EditMode, or PlayMode tests fail.
- Cache, build, profiler, Library, Temp, Obj, Logs, or review report output is staged.

## Required Checks

Confirm:

- `P7_HighDetail_Chuo` remains the practical baseline.
- `Chuo_BaseMap` remains legacy fallback and untouched.
- P7 LOD/category limitations remain known conditions, not blockers.
- Hazard data uses sample/manual/test placeholders only in P8-A.
- No official tsunami hazard values are claimed.
- `sourceMode` fail-safe behavior exists.
- Runtime loader and validator are isolated under `Assets/Scripts/P8`.
- Tests cover sample loading, validation failure, cinematic height guard, source mode fail-safe, collapse proxy data-only behavior, no gameplay success/failure effects, and compatibility inspector presence.
- `tools/p8/run_p8a_preflight.ps1` checks protected paths, baseline preservation, stage count, JSON validity, science/visual docs, and P2-P6 gate.
