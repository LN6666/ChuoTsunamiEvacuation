# NewMap Playable Bounds Boundary

Updated: 2026-05-30T00:00:00+09:00

Goal: prevent the player and NPCs from leaving the valid Chuo_BaseMap playable area.

Implementation:
- Runtime resolves a circular boundary from the original map center.
- Radius is `3500m`.
- Player and NPCs are clamped back inside the circle.
- No rectangular `AirWall_*` or `P10_BoundaryAirWall_*` colliders are created for normal gameplay.
- The boundary has no renderer in normal gameplay.
- Spawn validation still rejects candidates outside playable bounds.

Player smoke:
- Boundary source: original map center / runtime map bounds center
- Circular radius: 3500m
- Old air wall colliders: 0
- Visible boundary renderers: 0

Manual checks:
- Walk toward the map edge; player should be clamped at the 3.5km circular limit.
- No wall, blue plane, or rectangular inner air wall should be visible.
- NPCs should remain inside the playable area.
