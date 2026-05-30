# NewMap Manual Playtest Readiness

- Decision: ready_with_documented_visual_or_npc_limitations
- Reason: ground30/NPC2x validation passed preflight, Unity EditMode/PlayMode, temporary player build, and Player.log parsing. Remaining limitations are documented: building-floating outliers remain, far NPCs are static proxies for performance, and the 180-second active performance sample has small stutter evidence even though Player.log is clean.

Final tuning applied:
- Ground cover/support has an additional `0.3m` micro-raise on top of the previous `3.0m` raise, for expected gameplay ground Y `3.3`.
- Ground30/NPC2x pass raises the previous effective gameplay ground Y `3.3` by 30% to `4.29` using the existing raise-offset baseline.
- NPC distribution is configured for the full `2270m` circular boundary and doubled from `800` to `1600` requested/capped NPCs.
- Final P10 stamina/sprint tuning sets Evacuation max stamina to `3500` and Evacuation sprint to `4.59 m/s`, a `20%` reduction from the previous P10 build.
- Tourism Mode still has no stamina restriction and keeps `10.0 m/s` sprint.
- Building collision final refinement is enabled with `0.85` default shrink, `0.75` oversized/approach shrink, `60m` max proxy pieces, target clearance, and spawn clearance.
- Spawn safety now uses `500` attempts, `4m` building clearance, proxy + renderer bounds rejection, required support-ground hit, and final player-capsule overlap checks.
- Evacuation stamina max is reduced from `20000` to `13000`.
- Evacuation sprint speed is reduced from `6.75m/s` to `5.7375m/s`.
- Final P10 tuning supersedes those earlier values with max stamina `3500` and Evacuation sprint `4.59m/s`.
- Tsunami warning duration is reduced from `300s` to `180s`.

Preserved accepted behavior:
- Tourism mode has no tsunami failure and no stamina restriction.
- Evacuation mode keeps staged tsunami flow, shelter guidance, stamina, NPCs, labels, and result/restart flow.
- Blue ground/fall prevention, NPC grounding, NPC lifecycle, mouse left/right drag look, day/night lighting, official/non-official warnings, route guidance, and P2-P10 gameplay flow remain regression targets.

Validated automated status:
- Ground30/NPC2x preflight passed.
- EditMode passed: 127/127.
- PlayMode passed: 43/43.
- Temporary player build and 200 second smoke launch passed.
- Player.log has 0 errors, 0 warnings, and 0 exceptions.
- Active 180-second performance sample: average FPS `1767.61`, max frame `1021.96ms`, stutter frames over 66ms `2`.
- Diagnostic warmup before the active sample recorded `14286.81ms`; this came from the self-audit/startup warmup window and is not counted as the active gameplay sample.
- Repeated spawn safety sample passed: 100/100 accepted, 0 inside-building, 0 final-overlap failures, 0 outside-boundary.
- Active official and non-official approach smoke checks remain passable.
- DeepSeek review verdict: `PASS`, no A-level blockers, report `review_reports/deepseek_review_20260531_014419.md`.

This does not claim GIS-grade terrain, road, route, building-footprint, or official route validation.

Non-official candidate name readiness:
- Coordinate-based preprocessing enrichment has been run for missing playable non-official candidate names.
- Runtime labels load local cache only and keep non-official warnings.
- Full addresses, GML/building IDs, coordinate strings, and low-confidence names are hidden.

2.27km circular boundary update:
- Circular air-wall radius is now `2270m` from the original Chuo_BaseMap center.
- Normal gameplay has no visible boundary renderer; debug visualization remains off by default.
- Player, NPC, spawn, active target, and route guidance checks use the expanded playable boundary.

Ground30/NPC2x readiness gates:
- Ground raise reports old/new ground values and `30%` actual raise.
- Player, NPCs, active targets, green frames, E zones, and route markers remain aligned to raised ground.
- NPCs spawn inside the 2.27km boundary with broad sector/ring coverage.
- NPC requested/spawned/cap target is `1600`, with no runtime cap.
- Player.log remains clean.
- Final P10 stamina/sprint pre-P11 build must pass before P11 handoff is marked ready.
