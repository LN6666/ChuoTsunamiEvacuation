# Codex Prompt: NewMap Ground/Road Merge, Adaptive Support Grid, Names

Implement and validate the NewMap ground/road merge readiness work:
- Validate `Assets/Scenes/Chuo_GroundRoad_Import_Source.unity`.
- Generate ground/road/relief/bridge/building-base fallback height samples.
- Build an invisible adaptive support grid for runtime collision and local height snapping.
- Hide all blue support/debug surfaces in normal gameplay.
- Align player, NPCs, active targets, green frames, interaction zones, and route target markers to local support height where possible.
- Keep playable air walls active and invisible.
- Reduce floating-building impact without randomly moving PLATEAU buildings unless a consistent verified global offset exists.
- Keep name enrichment preprocessing-only and runtime offline/cache-only.
- Preserve Japanese/Kanji main-name preference, no fabricated names, no ID-only/address-only normal labels, and non-official warnings.
- Preserve P2-P10 systems, Tourism Mode, Evacuation Mode, mouse drag look, spawn building rejection, lighting/night, and official/non-official semantics.

Do not:
- Re-import map data.
- Blindly overwrite `Chuo_BaseMap.unity`.
- Delete imported buildings or core map assets.
- Create release/archive artifacts.
- Create P10-E/F/G.
- Move to PBL11.
- Claim official routes, GIS-grade validation, or terrain/road accuracy from proxy samples.
- Make Unity runtime perform web requests.

Required validation:
- `powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_groundroad_merge_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_groundroad_merge_player.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_groundroad_player_log.ps1`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_newmap_groundroad_merge_support_grid_names.md`
