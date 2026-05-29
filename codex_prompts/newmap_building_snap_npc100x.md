# Codex Prompt: NewMap Floating Building Snapdown + NPC 100x

Implement runtime visual building snapdown against the accepted gameplay ground cover, then expand NPC density to a 100x request with strong caps and performance safeguards.

Constraints:

- Do not re-import map data.
- Do not rework lighting or mouse drag look.
- Do not alter the accepted blue/ground cover unless a regression is found.
- Do not claim GIS-grade terrain/building elevation accuracy.
- Do not make runtime web requests.
- Do not create a final release/archive or P10-E/F/G.

Validation:

- `powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_building_snap_npc100x_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_building_snap_npc100x_player.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_building_snap_npc100x_player_log.ps1`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_newmap_building_snap_npc100x.md`
