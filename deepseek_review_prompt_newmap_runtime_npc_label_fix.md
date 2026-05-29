# DeepSeek Review Prompt: NewMap Runtime NPC Label Fix

Review the current git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

User goal:

- Eliminate runtime red error: `Triggers on concave MeshColliders are not supported`.
- Fix NPC rapid refresh and all-stop after player-NPC contact.
- Preserve player-NPC soft blocking and NPC building anti-clipping.
- Expand ordinary building/road labels from local cache only.
- Runtime Unity player must not make web requests.
- Do not create final release/archive, P10-E/F/G, P11, or GIS/official route claims.

Verify:

- Concave MeshCollider trigger errors are fixed by avoiding concave MeshCollider triggers, not by hiding Player.log.
- Runtime blocker cleanup does not convert imported concave MeshColliders into triggers.
- NPC lifecycle has explicit no-global-refresh config and counters.
- Player-NPC contact is local and does not globally stop/pause/refresh all NPCs.
- 100x NPC cap/distribution and building avoidance remain.
- Ordinary building/road label coverage is expanded and loaded from `Assets/Data/P10/newmap_name_cache.json`.
- Runtime web requests are disabled.
- Japanese/Kanji main-name rule remains enforced; no full addresses, IDs, or fabricated names are shown.
- Player.log summary shows 0 errors/0 warnings, 0 concave trigger errors, 0 global respawns, 0 all-stop events.
- EditMode and PlayMode tests pass.
- No final release/archive, no P10-E/F/G, no A-level blockers.

Key validation artifacts:

- `Assets/Data/P10/newmap_runtime_npc_label_player_log_summary.json`
- `Assets/Data/P10/newmap_concave_mesh_trigger_fix.json`
- `Assets/Data/P10/newmap_npc_180s_lifecycle_smoke.json`
- `Assets/Data/P10/newmap_building_road_label_runtime_report.json`
- `Assets/Data/P10/newmap_manual_playtest_readiness.json`

Return a concise verdict with blockers first.
