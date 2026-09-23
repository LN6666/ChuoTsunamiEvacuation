# NewMap Unexpected Airwall Final Cleanup

Unexpected air walls are handled by the collision whitelist.

Removed or disabled:
- Old rectangular `P10_BoundaryAirWall_*` blockers.
- Debug/test blockers.
- Invalid-zone blockers inside normal walking areas.
- Blocking colliders on route lines, green frames, labels, shelter markers, and hazard visuals.
- Unknown blockers inside the playable circle.

Preserved:
- Ground/support colliders.
- Building obstacle collision.
- NPC soft-blocking.
- 2.27km circular map boundary clamp.

Remaining limitation: the full local PLATEAU scene is too large for a static source-controlled per-instance collider dump, so runtime Player.log counts are the authoritative full scan.
