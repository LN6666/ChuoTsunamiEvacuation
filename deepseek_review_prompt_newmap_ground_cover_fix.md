# DeepSeek Review Prompt: NewMap Gameplay Ground Cover Fix

Review the current git diff for the Unity project `ChuoTsunamiEvacuation`.

Task scope:
- Implement the user-approved practical gameplay ground cover.
- Use visible road-like opaque ground cover, not blue support/debug material.
- Every ground cover tile must have collision.
- Player, NPCs, active targets, green frames, and interaction zones must share the cover/support height.
- Prevent player fall-through and out-of-bounds escape.
- Keep the failed adaptive relief/DEM support grid disabled.
- Preserve mouse drag, lighting, official/non-official warning semantics, Tourism/Evacuation modes, P2-P10 flow, and offline-only runtime labels.
- Do not claim GIS-grade terrain/road accuracy.
- Do not create a final release/archive or P10-E/F/G milestone.

Please verify:
1. Ground cover is visible, road-like, opaque, and not blue/magenta/debug.
2. Ground cover colliders are enabled on all tiles.
3. Support/debug blue planes remain hidden or covered in normal mode.
4. Player/NPC/targets use the same cover height and cannot fall through it.
5. Air walls and fall recovery still prevent map exit/fall-out.
6. Building floating is documented honestly as a remaining visual limitation if not fully fixed.
7. Runtime name labels read local cache only and make no web requests.
8. Japanese/Kanji main-name and no-address/no-ID behavior is not regressed.
9. No ProjectSettings/Packages/raw PLATEAU/final archive/P10-E/F/G regressions.
10. No A-level blockers remain.

Validation commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_ground_cover_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_ground_cover_player.ps1
powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_ground_cover_player_log.ps1
```

Expected verdict: PASS only if no player-accessible fall-through area remains and the visible cover is collidable.
