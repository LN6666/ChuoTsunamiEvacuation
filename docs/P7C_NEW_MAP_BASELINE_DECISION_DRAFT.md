# P7-C New Map Baseline Decision Draft

## Draft Decision

P7-C prepares `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` as the intended high-detail Chuo baseline for P8/P9/P10.

Final confirmation is deferred to P7-D after Windows EXE profiling and P2-P6 compatibility smoke checks.

## Consequence

If P7-D passes:

- `P7_HighDetail_Chuo.unity` becomes the default baseline for P8/P9/P10.
- `Chuo_BaseMap.unity` becomes a legacy fallback.
- Future hazard, crowd, and release work should be staged on the new high-detail baseline.

If P7-D blocks:

- P8/P9 should not silently continue on the old low-detail map.
- The blocker must be recorded with the required import, performance, or compatibility fix.

## Current Status

Prepared, not confirmed. Actual PLATEAU SDK import and final profiling remain pending.
