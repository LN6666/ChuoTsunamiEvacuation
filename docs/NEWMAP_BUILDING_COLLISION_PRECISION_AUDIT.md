# NewMap Building Collision Precision Audit

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

## Scope

This audit covers NewMap building-related blockers after the collision whitelist change. The problematic path is the runtime building obstacle bounds list built from building-like renderers and passed to the player/NPC systems. Those bounds can behave like invisible walls when `Renderer.bounds` or a building root bounds covers nearby road or open walking space.

## Diagnosed Risk

- Large renderer/root bounds can include facade, roof, overhang, empty space, or grouped building clusters.
- A single oversized building obstacle bound can cover sidewalks or roads.
- Keeping old oversized bounds together with tighter bounds would preserve the invisible-wall issue.

## Action

Runtime building obstacle bounds now use `tight_footprint_box_proxies`:

- Project mesh vertices into X/Z when available.
- Trim extreme vertices by configurable percentile.
- Shrink X/Z by `shrinkFactorXZ = 0.90`.
- Cap footprint width/depth at `80m`.
- Normalize runtime obstacle height to `5m`.
- Skip oversized cluster/root bounds instead of keeping them active.

This is a tight gameplay approximation, not a GIS-grade footprint validation.

Runtime counts are written back by `tools/map/parse_newmap_building_collision_precision_player_log.ps1`.
