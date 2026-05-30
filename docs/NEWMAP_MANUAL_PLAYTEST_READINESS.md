# NewMap Manual Playtest Readiness

- Decision: ready_with_documented_visual_limitations
- Reason: final tuning preflight, EditMode, PlayMode, temporary player build, player smoke, and Player.log validation passed. Remaining limitations are visual/manual map-tuning limitations, not automated blockers.

Final tuning applied:
- Ground cover/support has an additional `0.3m` micro-raise on top of the previous `3.0m` raise, for expected gameplay ground Y `3.3`.
- Building collision final refinement is enabled with `0.85` default shrink, `0.75` oversized/approach shrink, `60m` max proxy pieces, target clearance, and spawn clearance.
- Spawn safety now uses `500` attempts, `4m` building clearance, proxy + renderer bounds rejection, required support-ground hit, and final player-capsule overlap checks.
- Evacuation stamina max is reduced from `20000` to `13000`.
- Evacuation sprint speed is reduced from `6.75m/s` to `5.7375m/s`.
- Tsunami warning duration is reduced from `300s` to `180s`.

Preserved accepted behavior:
- Tourism mode has no tsunami failure and no stamina restriction.
- Evacuation mode keeps staged tsunami flow, shelter guidance, stamina, NPCs, labels, and result/restart flow.
- Blue ground/fall prevention, NPC grounding, NPC lifecycle, mouse left/right drag look, day/night lighting, official/non-official warnings, route guidance, and P2-P10 gameplay flow remain regression targets.

Validated automated status:
- Final preflight passed.
- EditMode passed: 125/125.
- PlayMode passed: 43/43.
- Temporary player build and 120 second smoke launch passed.
- Player.log has 0 errors, 0 warnings, and 0 exceptions.
- Repeated spawn safety sample passed: 100/100 accepted, 0 inside-building, 0 final-overlap failures, 0 outside-boundary.
- Active official and non-official approach smoke checks remain passable.
- DeepSeek review verdict: `PASS_WITH_NOTES`, no A-level blockers.

This does not claim GIS-grade terrain, road, route, building-footprint, or official route validation.
