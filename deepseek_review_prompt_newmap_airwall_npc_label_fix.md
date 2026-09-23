# DeepSeek Review Prompt: NewMap Airwall NPC Label Fix

Review the current git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

Focus on `Assets/Scenes/Chuo_BaseMap.unity` runtime behavior and P10 reports/tools. Verify:

- Unexpected building-adjacent air walls are diagnosed and fixed without removing outer boundary air walls.
- Boundary and invalid-zone blockers are preserved.
- Building obstacle bounds are conservative and do not create broad invisible frontage walls.
- Player-NPC collision or soft blocking is implemented so the player cannot simply pass through nearby NPCs.
- NPC collision/lifecycle behavior is not regressed: no rapid refresh, no silent stop, no heavy per-NPC pathfinding.
- Expanded building/road name enrichment was actually run in preprocessing and generated a local cache.
- Unity runtime/player performs no web requests.
- Labels use Japanese/Kanji main names only where reliable.
- Full addresses, coordinate strings, GML IDs, and `bldg_` IDs are hidden in normal mode.
- Official shelter labels and non-official warning semantics are preserved.
- Lighting/night, mouse drag, ground cover, spawn, NPC 100x, and P2-P10 flow are not regressed.
- No final release/archive artifacts were created.
- No P10-E/F/G artifacts were created.
- No A-level blockers remain.

Validation commands expected for this pass:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_airwall_npc_label_fix_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_airwall_npc_label_player.ps1
powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_airwall_npc_label_player_log.ps1
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_newmap_airwall_npc_label_fix.md
```

Return a verdict with blockers first. Do not rewrite code.
