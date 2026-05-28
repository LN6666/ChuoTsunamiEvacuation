# Codex Prompt: NewMap Visual Round 2 Mouse/Ground/Material/Night

Implement and validate round-2 fixes for `Assets/Scenes/Chuo_BaseMap.unity`:

- mouse drag-look only while holding the configured mouse button
- ground/support height realignment from visual renderer/building-base samples
- material/photo-texture audit without LOD2 overclaim
- night sky darkening with readable buildings
- clear day preservation
- debug cleanup regression checks
- P2-P10 gameplay regression checks
- temp player build only, no release archive

Required validation:

- `tools/map/run_newmap_visual_round2_preflight.ps1`
- Unity EditMode tests
- Unity PlayMode tests
- `tools/map/build_newmap_visual_round2_player.ps1`
- `tools/map/parse_newmap_visual_round2_player_log.ps1`
- DeepSeek review with `deepseek_review_prompt_newmap_visual_round2.md`
