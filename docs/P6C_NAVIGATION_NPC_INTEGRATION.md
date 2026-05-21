# P6-C Navigation + NPC Integration Validation

Date: 2026-05-21

Branch: `p6c-navigation-npc-integration`

## Purpose

P6-C performs conservative integration validation after merging the independently reviewed P6-A Navigation Guidance Prototype and P6-B NPC Evacuation Prototype.

This stage is not a feature expansion phase. It confirms that the two prototypes compile and run together while preserving the existing gameplay, data-source, scene, PLATEAU, and safety boundaries.

## What Was Merged

P6-A contributes display-only navigation guidance under `Assets/Scripts/Navigation/`:

- target metadata snapshots from transforms or existing shelter metadata
- direction, distance, and estimated time calculations
- required warnings for estimated prototype route, not official navigation, and not official evacuation guidance
- route metadata display without live routing or route-line rendering
- UI display wrapper for target, distance, estimated time, status, and arrow feedback

P6-B contributes lightweight non-blocking NPC evacuation behavior under `Assets/Scripts/NPC/`:

- NPC target snapshots from existing `BuildingShelter` metadata
- simple prototype target scoring
- transform-only NPC movement
- small bounded NPC spawning
- optional debug state labels
- explicit non-coupling flags for player success/failure

## What Remains Separate

Navigation and NPC behavior remain independent prototypes:

- no shared live target registry was added
- no scene-level placement was added
- no gameplay manager or result-panel integration was added
- no shelter source config, shelter availability, route status, qualification status, hazard status, or candidate status logic was changed
- no player success/failure rules were changed

NPCs may read shelter metadata into local target snapshots. Navigation may read shelter metadata into local guidance snapshots. Neither system owns shelter data or mutates gameplay result state.

## Safety Boundary Confirmation

P6-C preserves the inherited boundaries:

- `sourceMode` default remains `test`
- `real_qualified` remains opt-in
- `enableHumanitarianCandidates` remains `false`
- `enableLifeFirstCandidateSelection` remains `false`
- `Chuo_BaseMap.unity` was not modified
- PLATEAU imported files and raw PLATEAU data were not modified
- `ProjectSettings` and `Packages` were not modified
- runtime navigation/NPC code does not read `data_pipeline/raw`, `data_pipeline/downloads`, `data_pipeline/cache`, `data_pipeline/tmp`, or `.venv`
- no live routing, web request, or flood simulation was added
- route, qualification, hazard, and candidate status remain feedback-only
- navigation remains display-only
- NPCs remain non-blocking and cannot affect player success/failure
- OSM routes remain labeled as estimated prototype routes, not official evacuation routes
- OSM/ODbL attribution remains preserved by existing metadata feedback
- humanitarian candidates remain non-official and are not official shelters

## Why Scene-Level Integration Is Deferred

Scene-level wiring is deferred because the current milestone is only integration validation. Wiring P6-A guidance and P6-B NPCs into `Chuo_BaseMap.unity` would require decisions about active target selection, player UI placement, NPC spawn locations, map alignment, street/walkable constraints, and demo performance.

Those decisions belong in a later reviewed milestone. P6-C therefore validates coexistence in automated test scenes only and avoids modifying generated or large Unity scenes.

## Testing Summary

Added focused P6-C coverage:

- EditMode source/boundary guard verifying P6-A/P6-B sources do not reference player result APIs and inherited source defaults remain protected.
- PlayMode coexistence test verifying navigation and NPC components can share a simple test scene, NPC movement does not change guidance state, guidance refresh does not change NPC state, and no `EvacuationGameManager` or `ResultPanelController` is required.

Current GUI validation will be recorded after the P6-C test run.

## Test Files Added

- `Assets/Tests/EditMode/P6CNavigationNpcIntegrationTests.cs`
  - Verifies source-level safety boundaries and confirms P6-A/P6-B code does not reference player result APIs.
  - Confirms navigation guidance remains display-only and NPC behavior remains isolated from player success/failure logic.

- `Assets/Tests/PlayMode/P6CNavigationNpcIntegrationPlayModeTests.cs`
  - Verifies Navigation and NPC components can coexist in a simple runtime scene.
  - Confirms NPC movement does not change navigation guidance state.
  - Confirms navigation guidance does not change NPC state or player success/failure behavior.

## Next Steps For P6-D Final Review / Closeout

- Run final EditMode and PlayMode GUI validation after P6-C review comments are addressed.
- Ask DeepSeek to review P6-A/P6-B presence, protected-file boundaries, source/result decoupling, and test results.
- Keep scene-level `Chuo_BaseMap.unity` wiring deferred unless P6-D explicitly approves a small final demo setup.
- If a future milestone needs shared active-target selection, design it as a narrow read-only adapter with a confirmed Markdown plan.
- Preserve display-only navigation wording and non-blocking NPC semantics in all future P6 changes.
