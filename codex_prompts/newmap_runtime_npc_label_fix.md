# Codex Prompt: NewMap Runtime NPC Label Fix

Task scope:

- Fix concave MeshCollider trigger runtime error.
- Fix NPC lifecycle rapid refresh/all-stop after player-NPC contact.
- Expand ordinary building/road labels from preprocessing cache.
- Preserve air-wall cleanup, ground cover, mouse drag, lighting, official/non-official semantics, NPC 100x cap/distribution, and P2-P10 smoke.

Validation commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_runtime_npc_label_fix_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_runtime_npc_label_fix_player.ps1
powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_runtime_npc_label_fix_player_log.ps1
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_newmap_runtime_npc_label_fix.md
```
