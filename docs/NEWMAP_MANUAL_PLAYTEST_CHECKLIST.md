# NewMap Manual Playtest Checklist

Required final-tuning checks:
- Ground is slightly raised and remaining building floating is reduced.
- No excessive ground/building embedding is visible.
- Random spawn never starts inside or on top of buildings.
- Player can move immediately after spawn.
- Player can approach buildings and targets without a false air wall.
- Player is still blocked by actual sampled building footprints.
- Official shelter approach is passable.
- Non-official candidate approach is passable.
- Route lines, green frames, labels, markers, and hazard visuals remain nonblocking.
- Sprint speed feels about 15% lower in evacuation mode.
- Stamina amount is reduced by 35% in evacuation mode.
- Tsunami warning lasts 3 minutes before active tsunami/front movement.
- Tourism mode still has no tsunami failure and no stamina limit.
- Evacuation mode still reaches warning, active tsunami, interaction, result, and restart flow.
- NPC amount/distribution, lifecycle, grounding, and soft blocking remain stable.
- Mouse left/right drag look still works.
- Day/night lighting remains accepted.
- Player.log stays clean.

Current readiness decision: `ready_with_documented_visual_limitations`.

Automated validation status:
- `tools/map/run_newmap_final_tuning_preflight.ps1`: passed.
- EditMode tests: passed, 125/125.
- PlayMode tests: passed, 43/43.
- `tools/map/build_newmap_final_tuning_player.ps1`: passed.
- `tools/map/parse_newmap_final_tuning_player_log.ps1`: passed.
- DeepSeek review: `PASS_WITH_NOTES`, no A-level blockers.
