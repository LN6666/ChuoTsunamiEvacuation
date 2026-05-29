# NewMap NPC Distribution Around Player

Generated: 2026-05-29T00:00:00+09:00

Status: `pending_100x_player_smoke`

NPC distribution now uses `Assets/Data/P10/newmap_npc_distribution_config.json`.

Configured values:

- `npcCountMultiplier`: 100
- previous base count: 8
- requested count: 800
- `maxNpcCount`: 800
- capped count: 800
- distribution radius: 1000m
- minimum distance from player: 15m
- minimum NPC spacing: 3m
- deterministic seed: 20260529
- sectors/rings: 32 sectors, 6 rings

Implementation notes:

- NPCs are lightweight primitive humanoids.
- Layout is deterministic and sector/ring spread avoids one dense blob.
- Ground probing snaps NPCs to low map/support surfaces and rejects extreme Y.
- A fallback grid fills remaining slots if random placement cannot finish.
- Far NPC update throttling and static proxy mode avoid per-frame movement work across the full 1000m radius.
- Tourism shows ambient pedestrians with crowd failure disabled.
- Evacuation keeps crowd delay bounded at 8 seconds and NPCs do not directly kill the player.
- 100x temp player smoke is pending.

JSON: `Assets/Data/P10/newmap_npc_distribution_report.json`
