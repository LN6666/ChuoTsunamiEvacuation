# NewMap Walkable Corridor Collision Report

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

## Sample Strategy

Runtime diagnostics sample:

- cardinal points near the player spawn
- approach rings around active official shelters
- approach rings around active non-official/humanitarian/proxy targets

The sampled approach points are checked against the tightened runtime building footprint proxies. A target approach is considered blocked only when no sampled approach point around the active target is clear.

Active target interaction zones are carved from overlapping runtime building footprint proxies before the player and NPC systems receive the bounds. This is gameplay access clearance, not a claim about real entrances or official route validity.

## Pass Criteria

- No unknown blocking collider remains in sampled walkable corridors.
- No route, marker, label, green frame, or hazard visual blocks the player.
- Active target approach rings have at least one clear approach point.
- Actual building footprints still block the player.

Runtime pass/fail fields are populated by `tools/map/parse_newmap_building_collision_precision_player_log.ps1`.
