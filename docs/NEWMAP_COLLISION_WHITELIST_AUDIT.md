# NewMap Collision Whitelist Audit

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

The runtime policy is a whitelist: only ground/support, building obstacles, NPC body soft-blocking, and the circular map boundary may block movement. Runtime bootstrap scans loaded colliders through `AuditAndCleanupUnexpectedAirwallColliders`; the JSON audit records the category policy and representative paths.

Allowed blockers:
- `ground_support`: gameplay ground cover and invisible support surface.
- `building_obstacle`: conservative building obstacle bounds.
- `npc_body`: NPC capsule triggers used by player soft-blocking.
- `map_boundary`: 3.5km circular runtime clamp.

Nonblocking categories:
- `interaction_trigger`, `shelter_marker_visual`, `green_frame_visual`, `route_line_visual`, `label_visual`, `hazard_visual`, `debug_test`, `old_air_wall`, `invalid_zone_blocker`, `unknown`.

Hard fail conditions are unknown blocking colliders inside the playable circle, blocking route/label/frame/hazard visuals, active old rectangular air walls, missing ground support, or missing building/NPC/boundary blocking.
