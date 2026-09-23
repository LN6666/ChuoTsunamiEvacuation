# NewMap Collision Boundary Leaderboard Plan

This plan is confirmed by the user request for the NewMap collision whitelist, 3.5 km circular boundary, and R leaderboard toggle hardening task. It keeps the work inside the current P10/NewMap runtime and does not create a release archive, P10-E/F/G, P11 work, map reimport, or official route validation.

## Goals

- Player movement should be blocked only by ground/playable support, building obstacle bounds/proxies, NPC body soft-blocking, and the map boundary.
- All route lines, green frames, labels, markers, debug helpers, hazard visuals, old rectangular air walls, and unknown helper colliders must be nonblocking in normal gameplay.
- Replace rectangular boundary air walls with an invisible runtime circular boundary clamp centered on the original map center with radius `2270m`.
- Keep ground cover/support colliders active so the player cannot fall through.
- Preserve building collision, NPC soft-blocking, official/non-official semantics, tsunami/stamina/shelter guidance, mouse drag, spawn validation, and P2-P10 smoke coverage.
- Change `R` from show/refresh-only to a safe show/hide toggle for the mixed shelter ranking UI.

## Implementation Approach

- Add a `newmap_circular_boundary_config.json` config and runtime boundary type.
- Resolve the boundary center from the detected original map bounds center, falling back to documented NewMap bounds.
- Use runtime clamp for player and NPC movement instead of segmented wall colliders.
- Stop creating the four old rectangular `P10_BoundaryAirWall_*` colliders.
- Extend collision cleanup into a whitelist audit:
  - allowed blocking categories: `ground_support`, `building_obstacle`, `npc_body`, `map_boundary`
  - trigger-only categories: `interaction_trigger`, active hazard trigger semantics
  - nonblocking categories: route/line, green frame, label, marker, hazard visual, debug/test, old air wall, invalid zone blocker, unknown helper
- Preserve building collision through existing bounds-based correction and NPC building avoidance.
- Preserve NPC body collision through existing trigger capsules plus soft-blocking resolver.
- Update runtime smoke diagnostics, reports, and tests to expect a circular boundary and zero active rectangular air-wall colliders.
- Add leaderboard toggle config and route R input through UI visibility guards for menu/pause/rules/result states.

## Validation

- Add/update EditMode checks for config/report/tool presence and expected values.
- Add/update PlayMode checks for:
  - circular player clamp
  - NPCs remaining inside the circular boundary
  - no route/greenframe/label/direct-line blocking colliders
  - no old rectangular boundary air-wall colliders
  - building collision still blocks
  - NPC soft-blocking still blocks/slows
  - ground support still collides
  - R ranking show/hide and safe handling with pause/menu/rules
- Run the requested preflight, Unity GUI tests, temporary player build, Player.log parse, and DeepSeek review.

## Outputs

- Runtime/config/report JSON under `Assets/Data/P10/`
- Markdown audit/report/readiness docs under `docs/`
- PowerShell preflight/check/build/parse tools under `tools/map/`
- DeepSeek and Codex prompt files
- Temporary Windows player only:
  `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapCollisionBoundaryLeaderboardPre\ChuoTsunamiEvacuation_NewMapCollisionBoundaryLeaderboardPre.exe`
