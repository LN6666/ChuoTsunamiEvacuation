# NewMap Airwall Hard Cleanup Report

Unexpected building-adjacent blockers are handled by tightening the source of player/NPC building obstacle bounds and auditing runtime colliders:

- Outer `PlayableBoundsRoot/AirWall_*` colliders remain active.
- Debug/test colliders inside normal play are disabled.
- Non-trigger interaction blockers inside normal play are converted to triggers.
- Unknown invisible blockers inside normal play are converted to triggers.
- Building collision uses a reduced `0.05m` player margin and a conservative building-obstacle footprint filter.

This cleanup is not a removal of boundary protection. It is intended to remove accidental invisible walls from valid walking corridors while preserving map-edge and fall-prevention blockers.
