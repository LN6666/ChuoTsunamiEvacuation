# DeepSeek Review Prompt: NewMap Building Collision Precision

Review the current git diff for the NewMap precise building collision footprint fix.

Verify:

- The collision whitelist is still enforced.
- Building collision is tightened, not removed.
- Inflated renderer/root building bounds are disabled, skipped, or replaced by tight footprint proxies.
- Tight proxies use X/Z shrink/caps and player-relevant height.
- Walkable corridors near buildings and active targets are tested.
- Player cannot pass through actual sampled building footprints.
- Player can approach official and non-official/candidate markers.
- Route lines, shelter markers, labels, green frames, and hazard visuals remain nonblocking.
- NPC building avoidance and player/NPC soft-blocking are not broken.
- Circular 3.5km boundary and ground/fall prevention are not regressed.
- There is no official route false claim.
- No final release/archive, P10-E/F/G, or P11 artifacts were created.
- Report any A-level blockers first.

Validation commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_building_collision_precision_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_building_collision_precision_player.ps1
powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_building_collision_precision_player_log.ps1
```

Expected result:

- Unexpected building-adjacent air walls are reduced or removed.
- Player is blocked only by tight building footprints, NPCs, ground/support, and the circular boundary.
- Player can walk through normal road/open spaces near buildings.
- Player cannot pass through sampled building footprints.
- NPCs do not regress.
- Player.log remains clean.
- Manual readiness is updated.
