# NewMap NPC Building Collision

NPCs use the same building bounds cache as the player for lightweight avoidance and recovery.

Rules:
- spawn candidates reject building overlaps
- movement targets are retargeted when inside building bounds
- invalid positions recover to a nearby outside point
- far NPC throttling remains lightweight and must not make NPCs visibly pass through major nearby buildings
- all NPCs stay on the raised gameplay ground cover

No heavy per-NPC pathfinding is introduced.

Performance fix:
- NPC building avoidance now uses a spatial grid over building bounds instead of scanning every building bound for every NPC movement update.
- 180s player-smoke sample: 800 NPCs, 213.19 average FPS, 0 stopped-without-reason NPCs.
