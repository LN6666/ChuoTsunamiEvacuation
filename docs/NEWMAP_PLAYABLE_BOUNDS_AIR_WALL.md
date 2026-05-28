# NewMap Playable Bounds Air Wall

Generated: 2026-05-29T00:00:00+09:00

Goal: prevent the player and NPCs from leaving the valid Chuo_BaseMap playable area.

Implementation:
- Runtime resolves playable bounds from map renderer bounds, with documented Chuo fallback bounds if renderer bounds are unavailable.
- Four invisible `BoxCollider` air walls are created under `PlayableBoundsRoot`:
  - `AirWall_North`
  - `AirWall_South`
  - `AirWall_East`
  - `AirWall_West`
- Air walls have no renderers in normal gameplay.
- Spawn validation rejects candidates outside playable bounds or too close to the air wall.
- NPC distribution rejects or clamps positions outside playable bounds.

Player smoke:
- Bounds source: auto-detected map renderer bounds
- X: -1361.55 to 1357.26
- Z: -1900.08 to 2849.24
- Air wall colliders: 4
- Visible air wall renderers: 0

Manual checks:
- Walk toward the map edge; player should be blocked before leaving the map.
- No wall or blue plane should be visible.
- NPCs should remain inside the playable area.
