# DeepSeek Review Prompt: P6-B NPC Evacuation Prototype

Please review the current git diff for the P6-B NPC Evacuation Prototype in `ChuoTsunamiEvacuation`.

## Review Scope

Focus on whether this is a conservative, lightweight NPC evacuation prototype and whether it preserves all P5/P6 safety boundaries.

## Required Checks

- Confirm there is no P5/P6 safety boundary violation.
- Confirm no `ProjectSettings`, `Packages`, Unity scene, `Chuo_BaseMap.unity`, PLATEAU import, or raw PLATEAU data was modified.
- Confirm no live routing, web request, flood simulation, NavMesh, A*, Recast, ML-Agents, ECS, external crowd package, social-force model, or congestion physics was introduced.
- Confirm NPC behavior cannot influence player success/failure.
- Confirm NPC logic does not call `EvacuationGameManager`, `ResultPanelController`, `ResultMetrics`, shelter result APIs, or player outcome APIs.
- Confirm implementation is isolated under `Assets/Scripts/NPC/` and tests are under allowed test folders.
- Confirm NPCs are small-count, collider-free or non-blocking, and use simple transform movement only.
- Confirm target scoring remains prototype-only and does not present humanitarian candidates as official shelters.
- Confirm route, qualification, hazard, candidate, and warning metadata remains feedback/scoring input only and does not directly change gameplay success/failure.
- Confirm runtime code does not read `data_pipeline/raw`, `data_pipeline/download`, `data_pipeline/cache`, `data_pipeline/tmp`, or `.venv`.

## Test Coverage To Check

- Target scoring prefers closer/suitable targets.
- Blocked or unavailable targets are skipped or penalized.
- Humanitarian candidate, manual review, and warning penalties are applied when represented.
- No-target behavior fails safely.
- NPC state transitions are deterministic where possible.
- NPC implementation has no player success/failure API dependency.
- Small NPC groups can spawn in a test scene.
- NPCs move toward targets and reach `Arrived`.
- Spawned NPCs do not have blocking colliders.
- Zero-target PlayMode behavior does not throw.

## P6-C Integration Notes

Please verify `docs/P6B_NPC_EVACUATION_PROTOTYPE.md` clearly defers any scene-level placement, P6-A/P6-B coordination, GameManager/ResultPanel/source-config changes, and street-aware pathing to P6-C or later.
