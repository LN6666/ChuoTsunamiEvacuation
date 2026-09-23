# P10-B Runtime Smoke And Manual Playtest

P10-B is runtime smoke, profiling/stress preparation and execution in scene-safe tests, low-risk optimization, and manual playtest preparation.

Stage correction: P10-B does not build the final Windows EXE. Windows x64 build and release package work are deferred to P10-C so the user can manually playtest after P10-B and request quick fixes before release packaging.

## Runtime Smoke Scope

- Use runtime/bootstrap components and temporary test scenes.
- Do not mutate `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- Do not modify `Chuo_BaseMap.unity`, `ProjectSettings/`, `Packages/`, or `Assets/PLATEAU/`.
- Reuse P9-D coordinate anchoring and P8 candidate handoff data.
- Keep all route guidance as estimated prototype guidance, not official evacuation routes.

## Manual Playtest Preparation

The checklist in `Assets/Data/P10/p10b_manual_playtest_checklist.json` is ready for user playtest after P10-B validation.

Required checks include tsunami start, green ground frame visibility, official/non-official marker semantics, candidate warnings, entrance/safe-floor proxy flow, crowd/congestion, collapse/debris proxy, ResultPanel reason codes, light curtain readability, FPS/stutter feel, memory, loading time, and Player.log warnings/errors.
