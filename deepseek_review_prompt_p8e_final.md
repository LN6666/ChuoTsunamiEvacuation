# DeepSeek Review Prompt: P8-E Final Closeout

Review the current git diff for P8-E in `D:\UnityProjects\ChuoTsunamiEvacuation-P7`.

Check for A-level blockers:

- P8 has exactly five stages: P8-A, P8-B, P8-C, P8-D, P8-E.
- No P8-F/G or extra P8 stages are introduced.
- P8-A/B/C/D outputs are preserved.
- P8-E does not implement new gameplay systems.
- Humanitarian candidates remain non-official.
- Persistent visibility is handoff/plan/data-only unless already safe and scoped.
- No P9 gameplay is implemented.
- No real indoor scene is implemented.
- No success/failure rule changes are introduced.
- No false official shelter claim is made.
- No false physical tsunami height claim is made from `visualHeightMeters`.
- `maxTsunamiHeightMeters` is not confused with `inundationDepthMeters`.
- No official collapse, damage, or building safety prediction is claimed.
- P9 handoff package is complete.
- `P7_HighDetail_Chuo.unity` baseline is preserved and not staged.
- `Chuo_BaseMap.unity` is untouched.
- ProjectSettings and Packages are clean.
- Preflights and Unity tests pass.

Also report B-level follow-ups for P9/P10:

- P9 scene migration and persistent candidate visibility anchoring;
- P9 entrance / safe-floor / evacuation-complete proxy gameplay;
- P9 crowd/spawn/congestion/failure gameplay;
- P10 release packaging/archive.
