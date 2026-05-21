# P6 Technical Selection

Date: 2026-05-21

Branch: `p6a-navigation-guidance-prototype`

## Purpose

This document selects conservative technical directions for:

- P6-A Navigation Guidance Prototype
- P6-B NPC Evacuation Prototype
- P6-C Integration

P6-0 is documentation only. It does not implement navigation gameplay, NPC/crowd behavior, Unity package changes, scene edits, ProjectSettings edits, live routing, route line rendering, source-mode changes, or success/failure logic changes.

## Inherited Baseline

Phase 5 is the stable baseline:

- Default `sourceMode` remains `test`.
- `real_qualified` remains opt-in.
- `enableHumanitarianCandidates` remains default `false`.
- `enableLifeFirstCandidateSelection` remains default `false`.
- OSM route records are estimated prototype routes, not official evacuation routes.
- OSM/ODbL attribution must remain visible when OSM route feedback is shown.
- Humanitarian candidates are non-official and must never be presented as official shelters.
- Route geometry parsing exists, but real WGS84 route line rendering remains fail-closed until WGS84 to Unity/PLATEAU transform validation exists.
- Route distance/time, qualification confidence, warnings, and candidate status remain feedback only and must not directly affect gameplay success/failure.

## Technical Selection Verdict

P6-A should start with custom lightweight navigation UI. It may reference Unity AI Navigation/NavMesh ideas, but should avoid new dependency imports, package changes, ProjectSettings changes, NavMesh baking, and route-line rendering for now.

P6-B should start with custom lightweight NPC movement and target selection. It may reference crowd/evacuation projects for behavior presentation, but should avoid heavy dependencies, full crowd simulation, social-force models, congestion physics, ML agents, DOTS/ECS migration, and native navigation libraries for now.

P6-C should integrate only after P6-A and P6-B are separately reviewed and tested. Shared data access should use a small read-only adapter around existing shelter/candidate metadata if needed.

NavMesh or the AI Navigation package can be reconsidered later only with explicit approval and a package/project-settings risk review. The current project already lists `com.unity.ai.navigation` `2.0.12`, but P6-0 does not approve using it as the P6-A/P6-B architecture or changing its package state.

A* Pathfinding Project remains `reference_only` because of license/dependency/package-footprint considerations. Recast remains `reference_only` because it is too heavy for the Unity prototype stage. JR-Morgan and keijiro crowd projects remain `reference_only` for behavior/visual design inspiration, not direct integration.

## Recommended P6-A Architecture

Add new scripts under:

`Assets/Scripts/Navigation/`

P6-A should provide player-facing guidance without changing authoritative gameplay rules.

Recommended features:

- Lightweight target indicator.
- Direction arrow toward selected shelter/candidate target.
- Distance display using existing metadata or simple Unity-space distance where appropriate.
- Estimated time display using existing route feedback when available.
- Optional warning/disclaimer text near route feedback.
- Existing shelter metadata and existing route distance/time feedback.
- No real WGS84 route line rendering until transform validation exists.

Required disclaimers:

- Display `estimated prototype route / not official navigation` whenever route distance/time or route guidance is shown.
- Preserve OSM/ODbL attribution when OSM-derived route feedback is shown.
- Clearly distinguish official shelters from non-official humanitarian candidates.
- Never call humanitarian candidates official shelters.

Implementation boundaries for later P6-A work:

- Do not modify `sourceMode` defaults.
- Do not change success/failure logic.
- Do not add live routing, web requests, OSMnx, NetworkX, or external route calls to Unity runtime.
- Do not enable real route line rendering without verified WGS84 to Unity/PLATEAU transform validation.
- Do not modify `Packages`, `ProjectSettings`, Unity scenes, PLATEAU imported files, or `Chuo_BaseMap.unity` during prototype implementation.

Recommended P6-A design shape:

- `NavigationTargetProvider`: read-only access to currently selected or nearest shelter/candidate metadata.
- `PrototypeNavigationGuidanceController`: computes direction/distance display and disclaimer state.
- `NavigationGuidanceView`: UI-only rendering of arrow, distance, estimated time, and disclaimer.

These names are suggestions for a future implementation plan, not approved implementation work in P6-0.

## Recommended P6-B Architecture

Add new scripts under one or both of:

`Assets/Scripts/NPC/`

`Assets/Scripts/Simulation/`

P6-B should create a small, inspectable NPC evacuation prototype.

Recommended features:

- Generate a small number of NPCs, suggested 10-30.
- NPCs choose a shelter/candidate target using simple prototype scoring.
- NPCs move toward target using simple steering or transform movement first.
- NPC state labels are allowed and recommended for review clarity.
- NPCs may show states such as `Idle`, `ChoosingTarget`, `Moving`, `Arrived`, `Blocked`, or `TimedOut`.
- NPCs must not affect player success/failure.
- NPCs must not claim to model validated human evacuation behavior.

Prototype scoring should stay simple and explainable:

- Prefer nearer targets.
- Prefer usable/official confirmed shelters when using official data.
- Treat humanitarian candidates as non-official and warning-heavy.
- Optionally include simple route estimated time as feedback/scoring input only, not authoritative safety.

Implementation boundaries for later P6-B work:

- No large crowd simulation.
- No complex social-force model.
- No real congestion physics.
- No ML agent training or learned behavior.
- No DOTS/ECS migration.
- No full NavMesh dependency adoption without approval.
- No NPC effect on player result, timer, tsunami risk, shelter availability, or climb success/failure.
- No scene, package, ProjectSettings, PLATEAU import, or base-map changes during prototype stage.

Recommended P6-B design shape:

- `NpcEvacuationAgent`: per-agent state and simple movement.
- `NpcTargetScorer`: small target-choice scorer with explainable outputs.
- `NpcPrototypeSpawner`: creates 10-30 agents in a controlled debug area.
- `NpcStateLabel`: UI/label-only view of agent state.

These names are suggestions for a future implementation plan, not approved implementation work in P6-0.

## Recommended P6-C Integration Plan

P6-C should merge P6-A and P6-B only after both are reviewed and tested independently.

Integration rules:

- Keep P6-A guidance UI and P6-B NPC behavior separate unless a shared read-only data adapter is needed.
- Share shelter/candidate metadata through a safe read-only adapter.
- Do not let NPC state mutate shelter metadata, source config, route records, or player result state.
- Resolve shared file conflicts centrally instead of allowing parallel edits to core managers.
- Reconfirm `sourceMode = test` default and humanitarian flags default false after integration.
- Reconfirm route rendering remains fail-closed.
- Reconfirm estimated route and not-official-navigation disclaimers after integration.
- Re-run appropriate Unity tests only after implementation exists.

Recommended integration sequence:

1. Complete P6-A docs, implementation plan, implementation, and review.
2. Complete P6-B docs, implementation plan, implementation, and review.
3. Add a narrow read-only target metadata adapter if both prototypes need the same shelter/candidate facts.
4. Integrate UI and NPC scripts without changing gameplay success/failure.
5. Run EditMode/PlayMode tests and manual smoke only after implementation files exist.

## Dependency Decisions

| Candidate | P6 decision | Reason |
|---|---|---|
| Unity AI Navigation / NavMesh | `reference_only` now; future `optional_dependency` only with approval | Official and already present in current manifest, but P6-A/P6-B should avoid NavMesh baking and package/settings risk at prototype start. |
| Unity-Technologies/NavMeshComponents | `reference_only` | Useful architecture vocabulary, but legacy/sample import risk is too high. |
| Recast Navigation | `reference_only` | Strong algorithm reference, but native integration is too heavy. |
| A* Pathfinding Project | `reference_only` | License/commercial/package footprint requires separate approval. |
| JR-Morgan/Crowd-Evacuation-Simulation | `reference_only` | Useful evacuation behavior inspiration, but do not import full project or copy code/assets without license clearance. |
| keijiro/unity-crowd-simulation | `reference_only` | Visual/performance inspiration only; P6-B should stay small. |
| DOTS/ECS samples | `reference_only` | Future scale/performance reference only; P6-B does not need ECS. |
| ML-Agents | `not_recommended` for P6 | Training stack and opaque behavior are outside P6-A/P6-B scope. |

## Deferred Decisions

- Whether P6-A ever adopts baked NavMesh guidance.
- Whether the existing AI Navigation package should be used for later NPC movement.
- Whether route line rendering can be enabled after WGS84 to Unity/PLATEAU transform validation.
- Whether P6-B needs obstacle avoidance beyond simple steering.
- Whether future crowd behavior needs a real crowd model after P6-B review.
- Whether any external package/license can be approved after a separate dependency review.

## Acceptance Criteria For Future P6-A Implementation

- Documentation plan is confirmed before implementation.
- Scripts stay under `Assets/Scripts/Navigation/`.
- No package, ProjectSettings, scene, PLATEAU, or base-map modifications.
- Guidance shows estimated prototype route / not official navigation disclaimer.
- OSM/ODbL attribution is preserved when OSM route feedback is displayed.
- Real route line rendering remains fail-closed.
- Existing success/failure logic is unchanged.

## Acceptance Criteria For Future P6-B Implementation

- Documentation plan is confirmed before implementation.
- Scripts stay under `Assets/Scripts/NPC/` and/or `Assets/Scripts/Simulation/`.
- NPC count remains small, suggested 10-30.
- Movement is simple steering or transform movement first.
- NPCs have inspectable state labels or debug state.
- NPCs do not affect player success/failure.
- No full crowd simulation, social-force model, congestion physics, ML training, or DOTS migration.
- No package, ProjectSettings, scene, PLATEAU, or base-map modifications.
