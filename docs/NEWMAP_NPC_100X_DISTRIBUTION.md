# NewMap NPC 100x Distribution

- Baseline: 8 NPCs.
- Request multiplier: 100x.
- Requested count: 800.
- Cap: 800.
- Distribution: 1000m radius, 32 sectors, 6 rings, max 40 NPCs per sector.
- Placement: inside playable bounds, on gameplay ground cover, away from player, avoiding building bounds.
- Performance guards: pooled runtime objects, far NPC update throttling, far NPC static proxy mode, no full pathfinding per NPC.

If validation shows performance is too heavy, the cap should be lowered and reported as `npc_100x_ready_with_cap` or `npc_100x_too_heavy_capped`.
