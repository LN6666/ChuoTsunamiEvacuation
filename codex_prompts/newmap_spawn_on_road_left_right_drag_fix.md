# Codex Prompt: NewMap Spawn-On-Road + Left/Right Mouse Drag Fix

Workspace: `D:\UnityProjects\ChuoTsunamiEvacuation`

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

Goal:

- Prevent random player spawn inside buildings.
- Use playable-ground/support probing plus building bounds rejection.
- Add validated fallback safe spawn points without claiming official road geometry.
- Allow both left mouse drag and right mouse drag camera look.
- Keep mouse movement alone from rotating the camera.
- Preserve Tourism/Evacuation mode logic and P2-P10 gameplay.
- Do not change lighting/night.

Validation:

- `powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_spawn_mouse_fix_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_spawn_mouse_fix_player.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_spawn_mouse_fix_player_log.ps1`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_newmap_spawn_mouse_fix.md`
