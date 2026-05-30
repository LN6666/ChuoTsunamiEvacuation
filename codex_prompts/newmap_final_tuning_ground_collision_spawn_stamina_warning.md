# Codex Prompt: NewMap Final Tuning

Implement and validate the final NewMap manual tuning pass for `Assets/Scenes/Chuo_BaseMap.unity`.

Scope:
- Add small configurable final ground-cover/support micro-raise.
- Refine building collision overflow using tight runtime proxies only.
- Harden random spawn validation against building proxy and renderer bounds.
- Reduce effective evacuation stamina by 35%.
- Reduce evacuation sprint speed by 15%.
- Set tsunami warning duration to 180 seconds.
- Preserve P2-P10 gameplay flow and accepted fixes.

Required files:
- `Assets/Data/P10/newmap_final_ground_micro_raise_config.json`
- `Assets/Data/P10/newmap_final_ground_micro_raise_report.json`
- `Assets/Data/P10/newmap_building_collision_final_refinement.json`
- `Assets/Data/P10/newmap_building_collision_final_refinement_report.json`
- `Assets/Data/P10/newmap_spawn_final_safety_config.json`
- `Assets/Data/P10/newmap_spawn_final_safety_report.json`
- `Assets/Data/P10/newmap_stamina_final_tuning_report.json`
- `Assets/Data/P10/newmap_sprint_speed_final_tuning_report.json`
- `Assets/Data/P10/newmap_tsunami_warning_final_tuning_report.json`
- `Assets/Data/P10/newmap_final_tuning_regression_status.json`
- final tuning docs and validation scripts under `tools/map/`

Validation:
- `powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_final_tuning_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_final_tuning_player.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_final_tuning_player_log.ps1`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_newmap_final_tuning.md`

Do not:
- create a final release/archive,
- create P10-E/F/G,
- move to P11,
- re-import map data,
- rework lighting/night,
- rework mouse drag,
- rework NPC distribution unless needed for regression,
- claim GIS-grade validation.
