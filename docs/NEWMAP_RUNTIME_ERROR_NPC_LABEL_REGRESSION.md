# NewMap Runtime Error NPC Label Regression

Preserved areas:

- Air-wall hard cleanup and boundary air walls.
- Ground cover and fall prevention.
- Player spawn building avoidance.
- Player-NPC soft blocking.
- NPC building avoidance.
- Mouse left/right drag.
- Day/night lighting.
- Official and non-official warning semantics.
- P2-P10 gameplay smoke scenarios.
- Offline name cache runtime loading.

Validated in temporary player:

- Player.log contains no concave MeshCollider trigger errors.
- Player.log errors/warnings: `0 / 0`.
- NPC global refresh count is `0`.
- NPC all-stop count is `0`.
- NPC stopped-without-reason count is `0`.
- Ordinary building and road labels are loaded from the expanded cache: `117` / `180`.
