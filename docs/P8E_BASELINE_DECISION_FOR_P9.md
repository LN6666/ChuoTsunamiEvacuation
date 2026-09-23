# P8-E Baseline Decision For P9

Date: 2026-05-24.

P8 proceeds from the P7 high-detail practical baseline:

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

`Assets/Scenes/Chuo_BaseMap.unity` remains a legacy fallback and is not modified by P8-E.

P8-E confirms the P8 hazard systems are ready for P9 handoff. P9 may use the local high-detail baseline, but it must preserve local dirty scene state and avoid automatic reset/checkout/cleanup operations.

A P9 worktree may not automatically contain the local dirty scene state. P9 must handle baseline migration carefully before scene-dependent gameplay work begins.
