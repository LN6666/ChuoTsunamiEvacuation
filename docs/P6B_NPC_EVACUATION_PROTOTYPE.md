# P6-B NPC Evacuation Prototype

## Purpose

P6-B adds a conservative, lightweight NPC evacuation behavior prototype for PBL6.

The goal is to make a small number of visible NPCs choose evacuation targets and move toward them with simple transform movement. NPCs are only ambient decision-behavior feedback. They do not affect player success, failure, shelter availability, route validity, tsunami timing, or result metrics.

## Confirmed Implementation Plan

- Implement isolated NPC scripts under `Assets/Scripts/NPC/`.
- Read existing `BuildingShelter` components and optional metadata only through public read-only properties.
- Convert readable shelter data into `NpcEvacuationTargetInfo` records.
- Score targets with simple prototype penalties for distance, blocked/unavailable state, humanitarian/non-official distinction, crowding delay, manual review, and warnings.
- Move NPCs by transform position only, with no collider and no physics push behavior.
- Generate a default small group count and clamp it to a conservative limit.
- Expose deterministic states: `Idle`, `SelectingTarget`, `MovingToTarget`, `Arrived`, and `FailedNoTarget`.
- Add focused EditMode and PlayMode tests for scoring, fail-safe behavior, deterministic transitions, simple movement, spawn behavior, and no player success/failure dependency.

## Implemented Components

- `NpcEvacuationState`: deterministic prototype states: `Idle`, `SelectingTarget`, `MovingToTarget`, `Arrived`, and `FailedNoTarget`.
- `NpcEvacuationTargetInfo`: read-only target snapshot that can be built from existing `BuildingShelter` components and optional metadata.
- `NpcTargetScorer`: simple prototype scoring based on distance, availability, official/non-official status, humanitarian candidate flag, crowding delay, manual review, and warning count.
- `NpcEvacuationAgent`: transform-only NPC movement toward one selected target; no player result or game-state API calls.
- `NpcEvacuationSpawner`: small deterministic group spawner with default count clamped to 30 and collider-free default capsules.
- `NpcStateLabel`: optional `TextMesh` state/target label for debug visibility.

## Why This Is Not Full Crowd Simulation

P6-B deliberately avoids crowd simulation. It does not use social-force models, congestion physics, NavMesh, A*, Recast, ML-Agents, ECS, live routing, lane logic, group behavior, or capacity feedback loops.

Each NPC independently scores known target positions and moves directly toward one chosen target. This is suitable for visual prototype feedback only.

## Why This Is Not Official Evacuation Modeling

NPC decisions are prototype heuristics, not official evacuation behavior models. Scores are based on simplified distance and metadata penalties. Official shelter, route, qualification, humanitarian candidate, hazard, and warning metadata remain feedback-only.

Humanitarian candidates are not official shelters. If NPCs read humanitarian candidate metadata, they only apply warning/review penalties and must not present those candidates as official facilities.

OSM route information, when available through existing metadata, remains an estimated prototype route and not official evacuation guidance.

## P5/P6 Safety Boundary Preservation

- `sourceMode` default remains `test`.
- `real_qualified` remains opt-in.
- `enableHumanitarianCandidates` remains default false.
- `enableLifeFirstCandidateSelection` remains default false.
- No scene, `ProjectSettings`, `Packages`, PLATEAU import, or raw PLATEAU data changes are in scope.
- Runtime NPC code does not read `data_pipeline/raw`, `data_pipeline/download`, `data_pipeline/cache`, `data_pipeline/tmp`, or `.venv`.
- NPC behavior does not call player success/failure APIs and does not alter `EvacuationGameManager`, result metrics, route status, qualification status, hazard status, candidate status, or shelter source configuration.

## Testing Summary

Automated GUI validation completed on 2026-05-21:

- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- Result XML: `test-results/editmode-results.xml`
- EditMode result: 127 total / 127 passed / 0 failed / 0 skipped / 0 inconclusive.
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- Result XML: `test-results/playmode-results.xml`
- PlayMode result: 23 total / 23 passed / 0 failed / 0 skipped / 0 inconclusive.

P6-B-specific coverage:

- Target scoring prefers closer suitable targets.
- Blocked/unavailable targets fail closed when an available target exists.
- Humanitarian candidate, manual review, and warning penalties are applied when represented.
- No-target behavior enters `FailedNoTarget`.
- Manual ticks produce deterministic movement and arrival transitions.
- NPC source files do not reference player success/failure APIs.
- Small NPC groups spawn in PlayMode without scene editing.
- NPCs move toward targets, reach `Arrived`, and have no enabled colliders.
- Zero-target PlayMode spawning does not throw and agents fail safely.

## P6-C Integration Notes

- Scene-level placement in `Chuo_BaseMap.unity` is deferred to P6-C or later.
- Coordination with P6-A navigation guidance is deferred until both prototypes are reviewed independently.
- If future integration needs GameManager, ResultPanel, `BuildingShelter`, or source-config changes, that work should be planned as P6-C and reviewed separately.
- Future street-aware placement or pathing must remain clearly labeled as prototype guidance unless separately validated.
