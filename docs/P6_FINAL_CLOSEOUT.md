# P6 Final Closeout

Date: 2026-05-22

Branch reviewed: `p6e-final-closeout`

## P6 Purpose

Phase 6 completes the PBL6 navigation guidance and NPC evacuation prototype layer on top of the stable P5 baseline.

P6 is a conservative prototype/validation phase. It adds display-only player navigation guidance, lightweight non-blocking NPC evacuation agents, and generated playable behavior validation. It does not add official navigation, live routing, real route rendering, flood simulation, full crowd simulation, Chuo scene wiring, or P7 asset loading.

## Completed Stages

| Stage | Status | Closeout summary |
|---|---|---|
| P6-0 Open-source Reference Review + Technical Selection | Complete | Reviewed navigation, pathfinding, crowd, and evacuation references. Selected `reference_only` for external tools and custom lightweight P6-A/P6-B prototypes first. |
| P6-A Navigation Guidance Prototype | Complete | Added display-only target direction, distance, estimated time, and warning text under `Assets/Scripts/Navigation/`. |
| P6-B NPC Evacuation Prototype | Complete | Added small deterministic transform-only NPC agents, target scoring, spawning, and state labels under `Assets/Scripts/NPC/`. |
| P6-C Navigation + NPC Integration | Complete | Validated that P6-A and P6-B coexist without shared result-state mutation, scene wiring, or source-mode changes. |
| P6-D Playable Behavior Validation | Complete | Added generated/test-only playable validation under `Assets/Scripts/Simulation/` and confirmed navigation/NPC behavior in automated EditMode and PlayMode tests. |
| P6-E Final Closeout | Complete | Re-ran full GUI automated tests, documented P6 deliverables and boundaries, updated task/review docs, and completed final DeepSeek review with no A-level blockers. |

## Navigation System Summary

P6-A implemented a display-only navigation guidance prototype:

- `NavigationTargetInfo` captures read-only target metadata from a transform or existing shelter metadata.
- `NavigationGuidanceCalculator` computes horizontal direction, Unity world-space distance, estimated time, fallback text, and warning text.
- `NavigationGuidanceResult` carries the display result.
- `NavigationGuidanceController` updates guidance from configured player and target transforms.
- `NavigationGuidanceDisplay` renders target, distance, estimated time, warning/status text, and an arrow.

The navigation layer is not official navigation. It does not perform live routing, does not validate roads or entrances, and does not render real WGS84 route lines. Required warning text includes `Estimated prototype route`, `Not official navigation`, and `Not official evacuation guidance`. Route rendering remains disabled/fail-closed unless a future verified WGS84-to-Unity/PLATEAU coordinate transform exists.

## NPC System Summary

P6-B implemented lightweight prototype NPC evacuation behavior:

- `NpcEvacuationState` defines inspectable deterministic states.
- `NpcEvacuationTargetInfo` snapshots target position and metadata.
- `NpcTargetScorer` applies simple prototype scoring using distance and metadata penalties.
- `NpcEvacuationAgent` moves by transform only toward a chosen target.
- `NpcEvacuationSpawner` creates a bounded small group and clamps the count to the prototype limit.
- `NpcStateLabel` provides optional debug visibility.

NPCs are ambient prototype agents. They are not full crowd simulation, social-force modeling, congestion physics, ML agents, DOTS/ECS, or official evacuation modeling. They have no collider/physics push role in the generated tests and cannot affect player success/failure.

## Playable Behavior Validation Summary

P6-D added generated/test-only validation:

- `P6DBehaviorValidationResult` summarizes target selection, NPC arrival/movement/fail-safe counts, guidance distance trend, required warnings, display-only status, and success/failure isolation.
- `P6DBehaviorValidationRunner` evaluates guidance and NPC behavior together.
- `P6DPlayableBehaviorScenarioBuilder` creates a generated runtime context with a player, test shelter, navigation guidance, and NPCs.

The generated validation scene confirms that navigation distance decreases as the player moves toward the target, NPCs can select and arrive at the same target, required warnings are present, and no `EvacuationGameManager` or `ResultPanelController` is required. `Chuo_BaseMap.unity` remains untouched.

## Automated Test Results

| Run | Command | XML | Total | Passed | Failed | Skipped | Inconclusive |
|---|---|---|---:|---:|---:|---:|---:|
| P6-E EditMode GUI | `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui` | `test-results/editmode-results.xml` | 142 | 142 | 0 | 0 | 0 |
| P6-E PlayMode GUI | `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui` | `test-results/playmode-results.xml` | 27 | 27 | 0 | 0 | 0 |

Earlier stage validation records:

| Stage | EditMode | PlayMode | Notes |
|---|---:|---:|---|
| P6-A | 129 passed / 0 failed | 20 passed / 0 failed | Navigation guidance prototype validation. |
| P6-B | 127 passed / 0 failed | 23 passed / 0 failed | NPC evacuation prototype validation. |
| P6-D | 142 passed / 0 failed | 27 passed / 0 failed | Generated playable behavior validation. |
| P6-E | 142 passed / 0 failed | 27 passed / 0 failed | Final closeout rerun. |

## DeepSeek Review Summary

| Review | Status | Summary |
|---|---|---|
| P6-0 reference/technical selection | Complete by stage closeout | Confirmed reference-only selection and no dependency/package adoption for P6 prototype work. |
| P6-A navigation guidance | Complete by stage closeout | Reviewed as display-only navigation guidance with required warnings and no success/failure coupling. |
| P6-B NPC evacuation | Complete by stage closeout | Reviewed as lightweight, non-blocking prototype NPC behavior, not crowd simulation. |
| P6-C integration | Complete by stage closeout | Confirmed navigation and NPC systems coexist without source-mode, scene, or result-rule changes. |
| P6-D playable behavior validation | No A-level blockers in provided closeout context | Confirmed generated/test-only validation approach and GUI test pass counts. |
| P6-E final closeout | PASS, no A-level blockers | Final review saved locally as `review_reports/deepseek_review_20260522_010241.md`. Reviewer found no protected-file boundary concerns and said P6 is ready for final commit/push after closeout. |

## Safety Boundary Confirmation

- `sourceMode` default remains `test`.
- `real_qualified` remains opt-in.
- `enableHumanitarianCandidates` remains default `false`.
- `enableLifeFirstCandidateSelection` remains default `false`.
- Navigation remains display-only and not official navigation.
- Route rendering remains disabled/fail-closed unless coordinate transform validation is approved later.
- NPCs remain non-blocking and cannot affect player success/failure.
- NPC placement/spawning is generated deterministic prototype placement, not real population modeling.
- NPC target scoring is prototype scoring, not official evacuation modeling.
- Route, qualification, hazard, candidate, navigation, and NPC metadata remain feedback-only.
- No live routing, runtime web request, flood simulation, NavMesh adoption, A* import, Recast integration, DOTS/ECS migration, ML-Agents use, or full crowd simulation was added.
- `Chuo_BaseMap.unity` was not modified.
- PLATEAU imported files and raw PLATEAU data were not modified.
- `ProjectSettings` and `Packages` are not part of the P6-E closeout changes.
- No P7 full asset loading, underground/bridge work, LOD upgrade, or optimization was started.

## Known Limitations

| Severity | Limitation | Status |
|---|---|---|
| High | Navigation is display-only prototype feedback, not official navigation or official evacuation guidance. | Preserved boundary. |
| High | Route rendering remains disabled/fail-closed unless coordinate transform is validated. | Deferred to a future transform-validation milestone. |
| Medium | NPCs are lightweight prototype agents, not full crowd simulation. | Preserved boundary. |
| Medium | NPC spawn/placement is generated deterministic prototype placement, not real population modeling. | Deferred to future reviewed placement work. |
| Medium | NPC target scoring is prototype scoring, not official evacuation modeling. | Preserved boundary. |
| Medium | No live routing, web requests, or runtime OSM/NetworkX/OSMnx calls exist in Unity. | Preserved boundary. |
| Medium | No flood simulation exists. Existing tsunami behavior remains the prototype risk-front/risk-wall gameplay system. | Preserved boundary. |
| Medium | No P7 asset loading was performed. | Deferred to P7. |

## Deferred Items

- Verified WGS84-to-Unity/PLATEAU coordinate transform before any route-line rendering.
- Permanent scene wiring for navigation UI and NPC placement in Chuo scenes.
- Full Chuo asset loading.
- Underground and bridge asset handling.
- LOD upgrade and performance optimization.
- Real population modeling or official evacuation behavior modeling.
- Full crowd simulation, congestion physics, social-force models, ML behavior, or DOTS/ECS scale work.

## Explicit P7 Boundary

P7 begins Full Chuo Asset Loading + Underground/Bridge + LOD Upgrade + Game Optimization.

P6-E does not create P6-F, does not start P7, and does not introduce new gameplay features.
