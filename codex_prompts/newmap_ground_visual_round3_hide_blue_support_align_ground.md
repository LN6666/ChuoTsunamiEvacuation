# Codex Prompt: NewMap Ground Visual Round 3

Implement and validate:
- Hide blue support/collision plane in normal gameplay.
- Keep support collider active.
- Align support/player spawn to sampled visible ground/building base.
- Add invisible playable-boundary air walls.
- Preserve spawn validation and left/right mouse drag look.
- Add offline name labels from real source/project/cache data only.
- Add coordinate-based online enrichment tooling as preprocessing only.
- Do not fabricate road/building names.
- Do not rework lighting/night, reimport map data, create release archives, create P10-E/F/G, or move to P11.

Required validation:
- `tools/map/run_newmap_ground_visual_round3_preflight.ps1`
- Unity EditMode tests
- Unity PlayMode tests
- `tools/map/build_newmap_ground_visual_round3_player.ps1`
- `tools/map/parse_newmap_ground_visual_round3_player_log.ps1`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_newmap_ground_visual_round3.md`
