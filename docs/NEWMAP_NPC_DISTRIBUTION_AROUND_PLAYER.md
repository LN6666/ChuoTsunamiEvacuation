# NewMap NPC Distribution Around Player

Generated: 2026-05-29T00:00:00+09:00

Status: `validated_by_editmode_playmode_preflight_and_temp_player_smoke`

NPC distribution now uses `Assets/Data/P10/newmap_npc_distribution_config.json`.

Configured values:

- `npcCountMultiplier`: 20
- previous base count: 8
- requested count: 160
- `maxNpcCount`: 300
- capped count: 160
- distribution radius: 1000m
- minimum distance from player: 20m
- minimum NPC spacing: 6m
- deterministic seed: 20260529
- sectors/rings: 24 sectors, 5 rings

Implementation notes:

- NPCs are lightweight primitive humanoids.
- Layout is deterministic and sector/ring spread avoids one dense blob.
- Ground probing snaps NPCs to low map/support surfaces and rejects extreme Y.
- A fallback grid fills remaining slots if random placement cannot finish.
- Far NPC update throttling avoids per-frame movement work across the full 1000m radius.
- Tourism shows ambient pedestrians with crowd failure disabled.
- Evacuation keeps crowd delay bounded at 8 seconds and NPCs do not directly kill the player.
- Temp player smoke spawned 160/160 NPCs across 24 sectors and 5 rings with 0 Player.log errors/warnings.
- The visual-fix build script copies the NPC config into the standalone player data folder for this temp build.

JSON: `Assets/Data/P10/newmap_npc_distribution_report.json`
