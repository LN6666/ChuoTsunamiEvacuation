# Codex Prompt: NewMap Ground30 NPC2x

Implement and validate the NewMap 30 percent gameplay-ground raise and NPC 2x boundary-wide distribution pass.

Required validation commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_ground30_npc2x_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_ground30_npc2x_player.ps1
powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_ground30_npc2x_player_log.ps1
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_newmap_ground30_npc2x.md
```

Do not create a final release/archive, P10-E/F/G, P11, map re-import, or GIS-grade route/terrain claims.
