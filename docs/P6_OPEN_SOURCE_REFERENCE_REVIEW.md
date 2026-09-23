# P6 Open-Source Reference Review

Date: 2026-05-21

Branch: `p6a-navigation-guidance-prototype`

## P6-0 Purpose

P6-0 reviews navigation, pathfinding, NPC, crowd, and evacuation simulation references before the P6-A Navigation Guidance Prototype and P6-B NPC Evacuation Prototype.

This is a documentation and technical selection stage only. It does not implement navigation gameplay, NPC behavior, crowd simulation, live routing, route rendering, package changes, scene changes, or source-mode changes.

## P5 Inherited Safety Boundaries

P6 inherits the completed Phase 5 baseline:

- `sourceMode` default remains `test`.
- `real_qualified` remains opt-in.
- `enableHumanitarianCandidates` remains default `false`.
- `enableLifeFirstCandidateSelection` remains default `false`.
- Route geometry parsing exists, but WGS84 to Unity/PLATEAU transform is unverified, so real route line rendering remains fail-closed.
- Route distance/time can be used as feedback only.
- OSM routes must always be labeled `estimated prototype route`, not official evacuation route.
- OSM/ODbL attribution must be preserved.
- Humanitarian candidates are not official shelters and must never be presented as official.
- No live routing or web requests are allowed in Unity runtime.
- Route, qualification, hazard, candidate, and navigation feedback must not change gameplay success/failure rules during P6-A/P6-B unless separately planned and reviewed.

## Prototype-Stage Import Policy

P6-A and P6-B must not modify `ProjectSettings`, `Packages`, Unity scenes, PLATEAU imported files, raw PLATEAU data, or generated base-map scenes during the prototype stage.

The local project currently uses Unity `6000.4.6f1`. `Packages/manifest.json` already contains `com.unity.ai.navigation` `2.0.12`, but P6-0 does not change that package state and does not approve new package imports or NavMesh workflow adoption. Any later package or ProjectSettings change requires explicit approval and a separate risk review.

Navigation and crowd references reviewed here are engineering references only. They are not official evacuation guidance sources, official route sources, or official shelter-designation sources.

## Review Table

Recommended usage categories:

- `reference_only`: study concepts, API shape, architecture, UX, or algorithms without importing code/assets.
- `optional_dependency`: potential future dependency after explicit approval and risk review.
- `direct_integration`: acceptable to import/use directly now.
- `not_recommended`: avoid for P6-A/P6-B.

| Reference | Main purpose | License / usage constraint | Unity 6 compatibility risk | Dependency / settings pollution risk | P6-A fit | P6-B fit | P6-C fit | Category |
|---|---|---|---|---|---|---|---|---|
| Unity-Technologies/NavMeshComponents | Legacy/open NavMesh authoring components and examples for Unity runtime/editor NavMesh workflows. | MIT license in repository. | Medium. Repository describes package replacements by Unity version; older examples may not match Unity 6 package workflow. | Medium to high if imported as sample project; may bring scenes, editor scripts, and NavMesh assumptions. | Useful for concepts only. | Limited; useful only if later NavMesh movement is approved. | Possible future reference if package workflow is reviewed. | `reference_only` |
| Unity AI Navigation / `com.unity.ai.navigation` docs | Official Unity package documentation for NavMesh surfaces, links, obstacles, and agents. | Unity package/documentation terms; package use must follow Unity package license. | Low to medium. Official package is current, and local manifest already lists `2.0.12`, but project-specific Unity 6 baking/runtime behavior still needs testing. | Medium. Even without new dependency install, NavMesh baking can create assets and ProjectSettings/scene workflow changes. | Useful conceptual reference; do not make P6-A depend on baked NavMesh yet. | Possible later option for NPC movement after approval. | Reconsider only after P6-A/P6-B review. | `reference_only` now; future `optional_dependency` only with approval |
| recastnavigation/recastnavigation | C++ Recast/Detour navigation mesh generation, pathfinding, and crowd agent toolkit. | zlib license. | High for this Unity project because it is not a Unity package and would need native build/wrapper work. | High. Native libraries, wrappers, build settings, and custom integration are too large for P6 prototype docs. | Overkill. | Algorithm reference only. | Not suitable until a much later technical spike. | `reference_only` |
| A* Pathfinding Project by Aron Granberg | Unity pathfinding toolkit with grid, point, navmesh/recast-style graphs, movement scripts, and Pro features. | Mixed/free/pro commercial constraints; source is available but not treated as an open license dependency here. Confirm license before any use. | Medium. Current tool likely supports modern Unity, but version/package compatibility must be verified in the target project before adoption. | Medium to high. Importing package assets, editor tooling, examples, and settings can pollute the project. | Concepts for simple target scoring and route feedback only. | Good design reference for movement components, but do not import for P6-B. | Possible future optional dependency only after approval/license review. | `reference_only` |
| JR-Morgan/Crowd-Evacuation-Simulation | Unity evacuation simulation project with pedestrian/crowd behavior ideas. | Public GitHub repo; no clear license file visible from repo listing, so do not copy code/assets. | High. Older standalone Unity project; Unity 6 compatibility unknown. | High. Direct import would bring full project structure, scenes, packages/settings, and assumptions. | Not relevant except UI/visual inspiration. | Useful behavior inspiration for state labels and simple evacuation scenarios. | Reference only, no integration. | `reference_only` |
| keijiro/unity-crowd-simulation | Unity crowd rendering/simulation sample using large-scale visual crowd ideas. | Public GitHub repo; no clear license file visible from repo listing, so do not copy code/assets. | High. Uses old project structure and custom assets/shaders; Unity 6 compatibility unknown. | High. Direct import would bring `ProjectSettings`, sample assets, shaders, and rendering assumptions. | Not relevant for P6-A. | Visual/performance inspiration only; P6-B should stay at 10-30 NPCs. | Reference only, no integration. | `reference_only` |
| Unity-Technologies/EntityComponentSystemSamples | Official Unity DOTS/ECS samples for data-oriented simulation patterns. | Unity Companion License in repository. | Medium. Relevant to modern Unity but tied to Entities/DOTS versions and sample-specific setup. | High. Would add Entities/DOTS workflows and likely package/project configuration changes. | Not needed. | Useful only as future performance reference for large crowds, outside P6-B. | Possible later P7/P8 performance research, not P6. | `reference_only` |
| Unity-Technologies/ml-agents | Unity machine learning agents toolkit for training intelligent agents. | Apache-2.0 license. | Medium. Active Unity package/toolkit, but training/runtime stack is outside current prototype scope. | High. Adds packages, Python training dependencies, examples, and ML workflow complexity. | Not suitable. | Not suitable for simple NPC evacuation behavior in P6-B. | Not recommended unless a future research phase explicitly studies ML behavior. | `not_recommended` for P6 |
| JuPedSim / PedestrianDynamics | Pedestrian dynamics and evacuation simulation toolkit. | Open-source project; license must be confirmed before any code/data use. | High. Not a Unity-native runtime dependency. | High if integrated; would require external tooling, data conversion, and validation. | Not relevant. | Useful as academic/algorithmic reference for what P6-B is intentionally not implementing. | Reference only for future validation vocabulary. | `reference_only` |
| SebLague/Pathfinding | Educational Unity pathfinding examples, including grid/A* visualization patterns. | Public GitHub educational repository; confirm license before any code use. | Medium to high. Educational sample age and Unity version compatibility are uncertain. | Medium if imported; sample scenes/scripts would not match project architecture. | Useful for visual explanation of waypoints/arrows only. | Useful for simple pathfinding concepts, but P6-B should avoid full grid pathfinding initially. | Reference only. | `reference_only` |

## Individual Notes

### Unity-Technologies/NavMeshComponents

Main purpose:

Provides open-source Unity NavMesh components such as surface/link/modifier patterns and sample workflows.

Reasoning:

The repository is valuable for understanding Unity-style NavMesh authoring responsibilities. It is not the recommended P6 starting point because P6-A needs player-facing guidance UI, not baked navigation gameplay, and P6-B needs a small NPC prototype that avoids scene/package churn.

Safe ideas to borrow without import:

- Keep navigation authoring separate from movement behavior.
- Treat walkable-area generation as a reviewed asset/build step, not hidden gameplay logic.
- Use explicit links/areas/obstacle concepts as vocabulary for future planning.

Must not borrow in P6-A/P6-B:

- Do not import sample scenes or editor tooling.
- Do not modify `ProjectSettings` or `Packages`.
- Do not bake project NavMeshes into scenes for P6-A/P6-B without a separate approved plan.
- Do not imply NavMesh paths are official evacuation routes.

### Unity AI Navigation / `com.unity.ai.navigation`

Main purpose:

Official Unity package documentation for AI Navigation, including NavMeshSurface, NavMeshLink, NavMeshModifier, NavMeshModifierVolume, NavMeshAgent, and NavMeshObstacle workflows.

Reasoning:

The package is the official Unity direction for NavMesh workflows. The local project already lists `com.unity.ai.navigation` `2.0.12`, but this review does not approve any new package change or gameplay dependency on baked NavMesh. P6-A should start with a lightweight target indicator and route feedback display. P6-B should start with simple NPC movement so the prototype can be reviewed before any pathfinding dependency is adopted.

Safe ideas to borrow without import:

- Separate target selection from movement execution.
- Keep obstacles/links as future reviewed environment metadata.
- Use a high-level destination concept for NPCs without adopting NavMeshAgent yet.

Must not borrow in P6-A/P6-B:

- Do not change package versions or ProjectSettings.
- Do not create baked NavMesh assets in committed scenes during P6-A/P6-B.
- Do not use NavMesh path success/failure as tsunami evacuation success/failure logic.
- Do not render real route geometry until WGS84 to Unity/PLATEAU transform validation exists.

### recastnavigation/recastnavigation

Main purpose:

Recast builds navigation meshes from geometry, Detour performs pathfinding over those meshes, and DetourCrowd provides local crowd movement behaviors.

Reasoning:

Recast is a strong technical reference but is too heavy for the Unity prototype stage. Native integration, build tooling, and coordinate/mesh conversion would be a large system, and P6 explicitly should not implement full crowd simulation.

Safe ideas to borrow without import:

- Distinguish navigation mesh build data from runtime agent movement.
- Keep crowd/local-avoidance logic separate from route/target selection.
- Treat crowd movement as an approximation, not an official evacuation behavior model.

Must not borrow in P6-A/P6-B:

- Do not add native binaries or wrappers.
- Do not build a custom Recast pipeline.
- Do not implement DetourCrowd-style full local avoidance.
- Do not reinterpret PLATEAU LOD1 building massing as validated walkable street geometry.

### A* Pathfinding Project by Aron Granberg

Main purpose:

Unity pathfinding toolkit with multiple graph types, editor tooling, movement scripts, and Pro features.

Reasoning:

The project is mature and useful to study, but P6-0 should not import it because of license/commercial-feature considerations, dependency footprint, and project pollution risk. The P6 prototypes can achieve their first goals with simple UI and transform movement.

Safe ideas to borrow without import:

- Keep target scoring separate from movement.
- Use readable path status labels.
- Prefer simple prototype scoring before advanced path costs.
- Keep pathfinding visualization distinct from authoritative route claims.

Must not borrow in P6-A/P6-B:

- Do not copy source code or package assets.
- Do not import Free or Pro packages.
- Do not add graph scanning/baking workflows.
- Do not use path results as official evacuation-route validation.

### JR-Morgan/Crowd-Evacuation-Simulation

Main purpose:

Unity project focused on evacuation/crowd simulation behavior.

Reasoning:

This is relevant to P6-B as a behavior and presentation reference, but it should not be integrated. The repo appears to be a full Unity project and no clear license file is visible from the repository listing. Direct import would risk scene/settings/package pollution.

Safe ideas to borrow without import:

- Use simple agent states such as idle, evacuating, arrived, blocked.
- Show state labels for reviewer clarity.
- Keep evacuation scenarios small enough to inspect manually.

Must not borrow in P6-A/P6-B:

- Do not copy scripts/assets without explicit license clearance.
- Do not import the project.
- Do not implement full crowd simulation.
- Do not let NPC arrival/failure affect player success/failure.

### keijiro/unity-crowd-simulation

Main purpose:

Unity visual crowd simulation sample emphasizing crowd rendering/performance ideas.

Reasoning:

Useful as a visual/performance reference, but P6-B should have only a small number of NPCs, suggested 10-30, with simple movement and state labels. Direct use would bring unrelated rendering assets and settings risk.

Safe ideas to borrow without import:

- Keep NPC visualization lightweight.
- Use simple repeated visual agents for readable crowd context.
- Consider instancing/performance only if P6-B later expands beyond the small prototype size.

Must not borrow in P6-A/P6-B:

- Do not copy assets/shaders/code without license clearance.
- Do not import project settings.
- Do not build a large visual crowd for P6-B.
- Do not optimize for scale before behavior correctness is reviewed.

### Unity-Technologies/EntityComponentSystemSamples

Main purpose:

Official Unity DOTS/ECS sample repository demonstrating data-oriented architecture and high-entity-count simulation patterns.

Reasoning:

Relevant as a future performance reference only. P6-B should remain GameObject/MonoBehaviour-friendly unless a later reviewed plan approves Entities/DOTS. Adding DOTS now would be disproportionate and would complicate the stable Phase 5 baseline.

Safe ideas to borrow without import:

- Separate agent data from rendering and UI labels conceptually.
- Keep per-agent update logic simple and data-oriented where possible.
- Avoid heavy per-frame allocations.

Must not borrow in P6-A/P6-B:

- Do not add Entities/DOTS packages or sample projects.
- Do not rewrite existing gameplay systems into ECS.
- Do not optimize for thousands of agents in P6-B.

### Unity-Technologies/ml-agents

Main purpose:

Unity ML-Agents provides reinforcement learning and imitation learning workflows for training agents.

Reasoning:

Not appropriate for P6. It would introduce Python training, package/tooling complexity, and non-deterministic behavior design when the current need is explainable prototype NPC movement.

Safe ideas to borrow without import:

- None needed for P6-A.
- For P6-B, keep behavior explainable and inspectable.

Must not borrow in P6-A/P6-B:

- Do not add ML-Agents packages or Python training dependencies.
- Do not make NPC evacuation decisions learned/opaque.
- Do not make trained behavior part of success/failure logic.

### JuPedSim / PedestrianDynamics

Main purpose:

Pedestrian dynamics and evacuation simulation toolkit for research-style crowd behavior.

Reasoning:

Useful to understand what real pedestrian simulation can involve, but it is not Unity-native and would exceed P6-B scope. It is a boundary-setting reference: P6-B should not claim social-force accuracy, congestion physics, or validated pedestrian dynamics.

Safe ideas to borrow without import:

- Use precise wording: prototype NPC movement, not validated crowd simulation.
- Record future validation vocabulary such as flow, density, and bottleneck only as deferred research concepts.

Must not borrow in P6-A/P6-B:

- Do not add external simulation runtime dependencies.
- Do not implement social-force or congestion physics.
- Do not present P6-B NPCs as realistic evacuation behavior.

### SebLague/Pathfinding

Main purpose:

Educational Unity pathfinding and visualization examples.

Reasoning:

Good for conceptual clarity, but sample code should not be copied. P6-A does not need a grid pathfinder, and P6-B can move agents toward targets with simple steering first.

Safe ideas to borrow without import:

- Visualize paths/targets in a reviewer-readable way.
- Keep path calculation state easy to debug.
- Prefer simple demonstrations before advanced pathfinding.

Must not borrow in P6-A/P6-B:

- Do not copy sample scripts/assets without license clearance.
- Do not add grid pathfinding as a core P6-A/P6-B system.
- Do not use educational path visuals as official route guidance.

## License And Dependency Concerns

- Public GitHub visibility is not enough to copy code. Repositories without a clear license file must be treated as inspiration only.
- MIT, zlib, Apache-2.0, or Unity Companion License references still require compatibility review before copying or importing anything.
- Commercial, Pro, Asset Store, or custom-license tools require explicit approval before dependency use.
- Direct integration of full Unity sample projects can silently introduce scenes, scripts, assets, package versions, ProjectSettings changes, render-pipeline assumptions, or editor workflows.
- P6-A/P6-B should avoid package and ProjectSettings changes even if a dependency is technically compatible with Unity 6.

## P6-0 Decision

P6 starts `reference_only` first.

No package import, package upgrade, sample-project import, ProjectSettings change, scene change, NavMesh bake, route rendering enablement, crowd simulation framework, ML workflow, or dependency adoption is approved by P6-0.

P6-A should implement a custom lightweight navigation UI first. P6-B should implement custom lightweight NPC movement and target selection first. NavMesh, AI Navigation, A* Pathfinding Project, Recast, DOTS/ECS, ML-Agents, and external pedestrian simulation tools can be reconsidered later only after explicit approval and a package/project-settings/license risk review.

## Sources Reviewed

- Unity-Technologies/NavMeshComponents: https://github.com/Unity-Technologies/NavMeshComponents
- Unity AI Navigation package manual: https://docs.unity.cn/Manual/com.unity.ai.navigation.html
- Recast Navigation: https://github.com/recastnavigation/recastnavigation
- A* Pathfinding Project: https://arongranberg.com/astar/
- JR-Morgan/Crowd-Evacuation-Simulation: https://github.com/JR-Morgan/Crowd-Evacuation-Simulation
- keijiro/unity-crowd-simulation: https://github.com/keijiro/unity-crowd-simulation
- Unity-Technologies/EntityComponentSystemSamples: https://github.com/Unity-Technologies/EntityComponentSystemSamples
- Unity ML-Agents: https://github.com/Unity-Technologies/ml-agents
- JuPedSim / PedestrianDynamics: https://github.com/PedestrianDynamics/jupedsim
- SebLague/Pathfinding: https://github.com/SebLague/Pathfinding
