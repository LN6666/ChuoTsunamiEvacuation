# P6-D Playable Behavior Validation

Date: 2026-05-22

Branch: `p6d-playable-behavior-validation`

## Purpose

P6-D adds a conservative playable integration and behavior validation layer for the P6-A navigation guidance prototype and the P6-B NPC evacuation prototype.

The goal is not to expand route realism, crowd simulation, PLATEAU loading, or gameplay success/failure rules. The goal is to validate that navigation target awareness and ambient NPC evacuation behavior can run together in a generated playable-style test setup while preserving the P5/P6 safety boundaries.

## What P6-D Validates

- Navigation guidance can support a player-facing evacuation target awareness flow.
- Navigation distance changes when the player moves toward the generated target.
- NPC agents can choose the same generated shelter target and move toward it.
- NPC agents can arrive without blocking the player.
- NPC target choice and movement do not affect player success/failure state.
- Navigation remains display-only.
- Required warning text includes `Not official navigation` and `Not official evacuation guidance`.
- Behavior validation is automated-testable in generated EditMode/PlayMode contexts.
- No `Chuo_BaseMap.unity` scene wiring is required for validation.

## Implemented

Runtime validation scripts were added under `Assets/Scripts/Simulation/`:

- `P6DBehaviorValidationResult`
  - Summarizes selected target count, NPC total/arrival/moving/fail-safe counts, guidance distance trend, warning presence, display-only status, NPC non-blocking status, and success/failure untouched status.
- `P6DBehaviorValidationRunner`
  - Evaluates a before/after navigation guidance result plus NPC agents and returns a conservative validation result.
- `P6DPlayableBehaviorScenarioBuilder`
  - Creates a generated test/demo context containing a player transform, generated test shelter, navigation guidance controller/display, NPC spawner, and NPC agents.
  - Uses test source data only and does not modify Unity scenes.

Automated coverage was added:

- `Assets/Tests/EditMode/P6DBehaviorValidationTests.cs`
  - Validates the pure behavior summary.
  - Guards P6-D runtime source against protected result managers, Chuo scene references, ProjectSettings/Packages references, protected data-pipeline paths, and live web request APIs.
  - Confirms inherited source defaults remain protected.
- `Assets/Tests/PlayMode/P6DPlayableBehaviorValidationPlayModeTests.cs`
  - Builds a generated scene at runtime.
  - Confirms player movement changes navigation distance.
  - Confirms generated NPCs select the shelter target and arrive.
  - Confirms NPCs remain non-blocking.
  - Confirms no `EvacuationGameManager` or `ResultPanelController` is needed.
  - Confirms the active test scene is not `Assets/Scenes/Chuo_BaseMap.unity`.

## Generated/Test-Only Scope

P6-D remains generated/test-only:

- No permanent objects were wired into `Chuo_BaseMap.unity`.
- No Unity scene file was edited.
- No PLATEAU imported file or raw PLATEAU data was edited.
- No street-aware pathing, live routing, route rendering, flood simulation, or crowd simulation was added.
- The generated shelter uses `sourceType = test`.
- The generated scenario exists only when created by tests or by explicitly calling the builder.

## Why Chuo_BaseMap Wiring Is Deferred

Permanent scene wiring would require separate decisions about active target selection, UI placement, player start placement, NPC spawn placement, walkable surface validation, map alignment, performance, and full Chuo asset scope.

Those decisions would exceed P6-D's validation purpose and overlap with P7 work. P6-D therefore validates behavior in generated runtime contexts and leaves `Chuo_BaseMap.unity` untouched.

## Behavior Validation Results

Automated GUI validation completed on 2026-05-22:

- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- Result XML: `test-results/editmode-results.xml`
- EditMode result: 142 total / 142 passed / 0 failed / 0 skipped / 0 inconclusive.
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- Result XML: `test-results/playmode-results.xml`
- PlayMode result: 27 total / 27 passed / 0 failed / 0 skipped / 0 inconclusive.

P6-D-specific validation summary:

- Selected target count: generated NPC count.
- NPC arrived count: generated NPC count after deterministic ticks.
- Guidance distance trend: decreases after moving the generated player toward the shelter.
- Warnings present: `Not official navigation` and `Not official evacuation guidance`.
- Success/failure untouched: no gameplay result managers are required or present in the generated validation scene.

## Safety Boundary Confirmation

P6-D preserves inherited P5/P6 boundaries:

- `sourceMode` default remains `test`.
- `real_qualified` remains opt-in.
- `enableHumanitarianCandidates` remains default false.
- `enableLifeFirstCandidateSelection` remains default false.
- No `ProjectSettings` or `Packages` changes.
- No `Chuo_BaseMap.unity` modification.
- No PLATEAU imported file or raw PLATEAU data modification.
- No runtime reads from `data_pipeline/raw`, `data_pipeline/download`, `data_pipeline/cache`, `data_pipeline/tmp`, or `.venv`.
- No live routing, web request, or flood simulation.
- Route, qualification, hazard, and candidate status remain feedback-only.
- Navigation remains display-only.
- NPCs remain non-blocking and cannot affect player success/failure.
- OSM route wording remains estimated prototype route, not official evacuation route.
- OSM/ODbL attribution behavior remains preserved by existing navigation metadata paths.
- Humanitarian candidates remain non-official and are not official shelters.

## P6-E Final Closeout Checklist

- Confirm latest EditMode GUI validation XML counts remain passing.
- Confirm latest PlayMode GUI validation XML counts remain passing.
- Ask DeepSeek to review the P6-D diff, safety boundaries, generated-scene approach, and test coverage.
- Confirm protected files remain unmodified.
- Confirm no P7 work was introduced in P6-D.
- Decide whether P6 closes with generated/test-only integration or whether any tiny scene-demo wiring requires a separate explicit approval.
- Prepare P6 final closeout notes from P6-0 through P6-D.

## P7 Boundary

P7 begins later and is reserved for full Chuo asset loading, underground and bridge assets, LOD upgrade, and game optimization.

P6-D does not start P7 work.
