# NewMap Manual Playtest Checklist

Required checks:
- Player is blocked by sampled buildings.
- Player is blocked or soft-blocked by NPC bodies.
- Player cannot leave the 3.5km circular boundary.
- Player is not blocked by random invisible air walls inside the map.
- Route lines, green frames, labels, markers, and hazard visuals do not block movement.
- R shows the leaderboard/ranking panel.
- R hides the leaderboard/ranking panel.
- Ranking preserves official/non-official warning semantics.
- Direct-line/ranking wording does not claim official evacuation routes.
- Ground/support prevents falling.
- Failure opens result/restart UI immediately.

Current readiness decision: `ready_with_documented_boundary_limitations`.

Automated validation passed:
- Preflight
- EditMode 123/123
- PlayMode 42/42
- Temporary player build and smoke
- Player.log parse with 0 errors, 0 warnings, 0 exceptions
