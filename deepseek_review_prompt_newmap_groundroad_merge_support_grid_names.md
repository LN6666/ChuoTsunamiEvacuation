# DeepSeek Review Prompt: NewMap Ground/Road Merge, Adaptive Support Grid, Names

Review the current git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

Scope:
- Supplemental source scene: `Assets/Scenes/Chuo_GroundRoad_Import_Source.unity`
- Main scene: `Assets/Scenes/Chuo_BaseMap.unity`
- Use the supplemental ground/road source scene as a sampling source.
- Do not re-import PLATEAU data.
- Do not blindly overwrite or save `Chuo_BaseMap.unity`.
- Do not delete imported buildings or core map assets.
- Do not create final release/archive.
- Do not create P10-E/F/G.
- Do not move to PBL11.

Implementation to verify:
- Source validation report exists and hard-fails empty/missing source scenes.
- Height sample cache is generated from supplemental road/terrain/relief/bridge data where present.
- Current imported source is relief/DEM-only, with no road/tran renderers; that limitation must be documented and not misrepresented.
- Adaptive local-height support grid replaces the single flat support plane as the primary runtime collision support.
- Support grid cells have enabled colliders and no visible renderers in normal gameplay.
- Blue support/debug surfaces are hidden by default.
- Player spawn uses local support height, rejects outside bounds, and rejects building overlap.
- NPCs use local support/support colliders and remain inside playable bounds.
- Active official shelters, non-official candidates, green frames, E interaction zones, and route target markers use local support/target height where possible.
- Air walls remain active, invisible, and bounds-integrated.
- Floating-building handling does not randomly move PLATEAU buildings unless a consistent global offset is verified.
- Remaining floating/building-ground mismatch is documented honestly.
- Existing P2-P10 gameplay flow, Tourism Mode, Evacuation Mode, mouse drag look, lighting/night, spawn validation, and official/non-official semantics are not regressed.
- Name enrichment is preprocessing-only.
- Unity runtime performs no web requests and reads only local cache/config.
- Labels prefer Japanese/Kanji main names, hide ID-only/address-only/low-confidence labels in normal mode, preserve non-official warnings, and do not fabricate names.

Validation artifacts to inspect:
- `Assets/Data/P10/newmap_ground_road_source_validation.json`
- `Assets/Data/P10/newmap_ground_road_height_samples.json`
- `Assets/Data/P10/newmap_ground_road_sampling_report.json`
- `Assets/Data/P10/newmap_adaptive_support_grid_config.json`
- `Assets/Data/P10/newmap_adaptive_support_grid_report.json`
- `Assets/Data/P10/newmap_blue_area_final_fix.json`
- `Assets/Data/P10/newmap_ground_road_merge_report.json`
- `Assets/Data/P10/newmap_height_integration_report.json`
- `Assets/Data/P10/newmap_floating_building_round4_report.json`
- `Assets/Data/P10/newmap_air_wall_regression_report.json`
- `Assets/Data/P10/newmap_groundroad_regression_status.json`
- `Assets/Data/P10/newmap_name_cache.json`
- `Assets/Data/P10/newmap_name_enrichment_report.json`
- `Assets/Data/P10/newmap_name_label_runtime_report.json`
- `Assets/Data/P10/newmap_groundroad_merge_player_report.json`
- `Assets/Data/P10/newmap_groundroad_merge_player_log_summary.json`
- `docs/NEWMAP_GROUND_ROAD_SOURCE_VALIDATION.md`
- `docs/NEWMAP_GROUND_ROAD_HEIGHT_SAMPLING.md`
- `docs/NEWMAP_ADAPTIVE_SUPPORT_GRID_FROM_GROUND_ROAD.md`
- `docs/NEWMAP_BLUE_AREA_FINAL_FIX.md`
- `docs/NEWMAP_GROUND_ROAD_MERGE_REPORT.md`
- `docs/NEWMAP_HEIGHT_INTEGRATION_REPORT.md`
- `docs/NEWMAP_FLOATING_BUILDING_ROUND4_REPORT.md`
- `docs/NEWMAP_AIR_WALL_REGRESSION_REPORT.md`
- `docs/NEWMAP_NAME_ENRICHMENT_FROM_COORDINATES.md`
- `docs/NEWMAP_NAME_LABEL_ATTRIBUTION.md`
- `docs/NEWMAP_NAME_LABEL_RUNTIME_REPORT.md`
- `docs/NEWMAP_GROUNDROAD_MERGE_PLAYER_REPORT.md`
- `docs/NEWMAP_MANUAL_PLAYTEST_READINESS.md`
- `docs/NEWMAP_MANUAL_PLAYTEST_CHECKLIST.md`

Validation commands:
- `powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_groundroad_merge_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_groundroad_merge_player.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_groundroad_player_log.ps1`

Return:
- A-level blockers, if any.
- B/C issues and suggested small fixes.
- Whether the manual playtest decision is justified.
- Whether runtime is offline-only for name labels.
- Whether any final release/archive or P10-E/F/G artifact was created.
