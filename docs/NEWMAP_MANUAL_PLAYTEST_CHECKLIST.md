# NewMap Manual Playtest Checklist

Required final-tuning checks:
- Ground is visibly raised by 30% from the previous effective gameplay-ground baseline.
- Remaining building floating is reduced, with any remaining floating treated as a documented visual limitation.
- No excessive ground/building embedding is visible.
- Random spawn never starts inside or on top of buildings.
- Player can move immediately after spawn.
- Player can approach buildings and targets without a false air wall.
- Player is clamped at the expanded 2.27km circular boundary without a visible wall.
- Player is still blocked by actual sampled building footprints.
- Official shelter approach is passable.
- Non-official candidate approach is passable.
- Route lines, green frames, labels, markers, and hazard visuals remain nonblocking.
- Sprint speed feels about 15% lower in evacuation mode.
- Stamina amount is reduced by 35% in evacuation mode.
- Final P10 tuning: Evacuation max stamina is exactly 3500.
- Final P10 tuning: Evacuation sprint speed is 4.59 m/s, a 20% reduction from the previous P10 build.
- Final P10 tuning: Tourism Mode still has no stamina restriction and keeps 10.0 m/s sprint.
- Tsunami warning lasts 3 minutes before active tsunami/front movement.
- Tourism mode still has no tsunami failure and no stamina limit.
- Evacuation mode still reaches warning, active tsunami, interaction, result, and restart flow.
- NPC count is visibly doubled from the previous 800 baseline to the 1600 requested/capped target.
- NPCs are distributed across the full 2.27km circular boundary, not clustered near the player only.
- NPC lifecycle, grounding, far-proxy throttling, and soft blocking remain stable.
- NPCs do not globally refresh, all stop, or spawn outside the boundary.
- Mouse left/right drag look still works.
- Day/night lighting remains accepted.
- Player.log stays clean.

Current readiness decision: `ready_with_documented_visual_or_npc_limitations`.

Automated validation status:
- `tools/map/run_newmap_ground30_npc2x_preflight.ps1`: passed.
- EditMode tests: passed, 127/127.
- PlayMode tests: passed, 43/43.
- `tools/map/build_newmap_ground30_npc2x_player.ps1`: passed.
- `tools/map/parse_newmap_ground30_npc2x_player_log.ps1`: passed.
- Player.log: 0 errors, 0 warnings, 0 exceptions.
- Active performance sample: average FPS `1767.61`, max frame `1021.96ms`, stutter frames over 66ms `2`.
- Diagnostic warmup sample: max frame `14286.81ms`, documented as self-audit/startup warmup and excluded from active gameplay sample.
- DeepSeek review: `PASS`, no A-level blockers, report `review_reports/deepseek_review_20260531_014419.md`.

Non-official candidate name checks:
- Unnamed non-official candidates now show Japanese/Kanji main names where matched.
- Non-official warning still appears for enriched candidates.
- No full addresses are shown for candidate labels.
- No `bldg`/GML/building IDs are shown in normal gameplay.
- Runtime label system performs no web requests.
- Disabled/out-of-playable-boundary candidates have no active labels.
- Route guidance does not point to disabled targets outside the 2.27km boundary.

Ground30/NPC2x checks:
- Runtime reports old ground Y/offset `3.3` and new ground Y/offset `4.29`.
- Actual raise percent is `30%` of the chosen baseline.
- Player, NPCs, active targets, green frames, E zones, and route markers remain on raised ground.
- No blue ground or fall-through regression appears.
- FPS/stutter sample is acceptable with documented limitations for 1600 NPCs.

Final P10 stamina/sprint checks:
- Runtime config loads max stamina `3500`.
- Evacuation Mode stamina drains and recovers.
- Evacuation sprint speed is `4.59 m/s`.
- Tourism Mode stamina remains disabled.
- Tourism sprint speed remains `10.0 m/s`.
