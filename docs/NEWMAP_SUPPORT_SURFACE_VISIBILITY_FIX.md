# NewMap Support Surface Visibility Fix

Generated: 2026-05-29T00:00:00+09:00

The gameplay support surface remains available for spawn and movement, but it is no longer a rendered primitive.

Implementation:
- Support object: `NewMap_RuntimeGroundSupport_DocumentedProxy`
- Collider: enabled `BoxCollider`
- Renderer: none created for the runtime proxy
- Debug visualization: disabled by default
- Normal mode: no visible blue support plane

Validation gates:
- Support collider must exist.
- Support renderer count may be zero; visible support renderers must be zero.
- Air wall renderers must be zero.

Player smoke:
- Support collider active: true
- Support renderer count: 0
- Visible support renderers: 0
- Air wall visible renderers: 0

Status: `completed_on_new_chuo_basemap_player_smoke`
