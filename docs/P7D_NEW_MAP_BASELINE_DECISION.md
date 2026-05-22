# P7-D New Map Baseline Decision

Decision: BLOCKED.

## Reason

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` is the intended high-detail baseline candidate, but it is not ready to become the P8/P9/P10 baseline.

The scene is currently a shell/import target. Actual high-detail PLATEAU assets are not loaded, average LOD3 is not achieved, P2-P6 runtime compatibility is not proven, and Windows EXE profiling is not complete.

## Baseline Criteria

| Criterion | Status |
|---|---|
| Actual high-detail asset load | Blocked |
| LOD coverage | Blocked |
| Bridge/underground/road availability | Blocked |
| P2-P6 compatibility | Blocked |
| Windows EXE profiling | Blocked |
| Performance acceptability | Blocked |

## Policy

P8 must not start on this scene until the manual import and validation blockers are resolved.

`Chuo_BaseMap.unity` remains untouched as a legacy fallback.
