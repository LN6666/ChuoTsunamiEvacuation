# Codex Prompt: NewMap Completion Hardening Until Done

Continue hardening `D:\UnityProjects\ChuoTsunamiEvacuation` on `Assets/Scenes/Chuo_BaseMap.unity`.

Do not create a final release package. Do not move to PBL11. Do not create P10-E/F/G. Do not re-import map data unless explicitly justified.

Required checks:

- Run `tools/map/run_newmap_completion_hardening_preflight.ps1`.
- Run EditMode and PlayMode Unity tests.
- Build `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapHardeningPre\ChuoTsunamiEvacuation_NewMapHardeningPre.exe`.
- Run hardening performance sampling and Player.log parse.
- Run DeepSeek with `deepseek_review_prompt_newmap_completion_hardening.md`.

Do not claim official shelters, official routes, GIS-grade validation, or active old targets unless they are actually verified in the new map.

Final status terms must be limited to:

- `completed_on_new_chuo_basemap`
- `completed_with_documented_runtime_proxy`
- `disabled_missing_from_new_map`
- `blocked_needs_user_map_asset`
- `failed`
