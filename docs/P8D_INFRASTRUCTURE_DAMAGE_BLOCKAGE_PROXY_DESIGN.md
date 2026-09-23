# P8-D Infrastructure Damage / Blockage Proxy Design

Date: 2026-05-24.

P8-D adds a lightweight infrastructure status layer on top of the P8-C hazard interaction model. It consumes P8-B/P8-C evidence-backed tsunami fields and emits proxy damage, blockage, low-floor warning, and visual-collapse marker states.

## Inputs

- P8-C infrastructure hazard evaluation.
- `arrivalTimeSeconds`.
- `inundationDepthMeters`.
- derived/prototype `inundationBoundary` state already represented in the P8-C evaluation.
- `hazardIntensity`.
- `confidence`.
- `sourceMode`.
- `evidenceSourceId`.
- `Assets/Data/P8/infrastructure_damage_proxy_config.json`.
- `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`.

`maxTsunamiHeightMeters` remains metadata only. `visualHeightMeters` remains cinematic only. Neither field is used as physical inundation depth.

## Outputs

Supported P8-D proxy states:

- `no_damage`
- `warning`
- `low_floor_inundation_warning`
- `entrance_blocked_proxy`
- `road_restricted_proxy`
- `bridge_restricted_proxy`
- `underground_avoid_proxy`
- `building_damaged_proxy`
- `collapsed_proxy_visual`
- `inaccessible_proxy`
- `manual_review_required`

These states are status and marker outputs only. They do not change player success/failure rules.

## Covered Categories

- road
- building
- bridge
- underground
- entrance
- waterfront
- open_space
- shelter_proxy
- navigation_target_proxy
- humanitarian_candidate_proxy
- highrise_candidate_marker

## Scene Handling

No scene mutation was required for P8-D. Runtime marker scripts can be placed later under an isolated root such as `P8_DamageBlockageCollapseProxy_System`. P8-E should verify final persistent visibility and scene anchoring before P9.

## Non-Goals

- no real structural damage prediction;
- no official building safety/damage claim;
- no physics collapse;
- no debris simulation;
- no rigidbody destruction;
- no P9 selectable vertical evacuation gameplay;
- no real indoor scene gameplay;
- no gameplay success/failure rule changes.
