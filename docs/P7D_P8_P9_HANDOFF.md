# P7-D P8/P9 Handoff

## Status

Handoff status: BLOCKED.

## Intended Scene Paths

- P8 intended scene: `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- P9 intended scene: `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- Legacy fallback: `Assets/Scenes/Chuo_BaseMap.unity`

## Baseline Decision

P7-D decision is BLOCKED. The high-detail scene is not yet approved as the P8/P9/P10 baseline.

## Current Layer Status

| Area | Status |
|---|---|
| LOD coverage | Pending manual import |
| Bridges | Pending manual import |
| Underground | Pending manual import |
| Roads | Pending manual import |
| Water | Pending manual import |
| Terrain/relief | Pending manual import |
| City furniture | Pending manual import |
| Disaster risk | Pending manual import |
| P2-P6 compatibility | Pending runtime smoke |
| Performance | Pending Windows EXE profiling |

## Forbidden Assumptions

- Do not assume average LOD3.
- Do not assume roads, bridges, underground, water, terrain, vegetation, or city furniture are loaded.
- Do not implement P8 hazard systems before baseline import/profiling is resolved.
- Must not implement P9 crowd or indoor evacuation systems before baseline import/profiling is resolved.
- Do not change gameplay success/failure rules as part of baseline validation.
