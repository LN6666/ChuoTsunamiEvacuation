# P8-A P8-B Scene Anchor Plan

Date: 2026-05-23.

## Purpose

This plan defines how P8-B should anchor the tsunami risk-front visualization to the accepted high-detail scene without depending on the legacy base map.

## Baseline Scene

P8-B must use:

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

P8-B must not use `Chuo_BaseMap.unity` as the baseline and must not require that scene to exist.

## Anchor Placement

Risk-front origin and target placeholders can live under a dedicated root in the high-detail scene, for example:

`P8_RiskFrontAnchors`

Recommended child placeholders:

- `P8_RiskFront_Origin`
- `P8_RiskFront_Target`
- `P8_RiskFront_BoundaryPreview`
- `P8_RiskFront_DebugMarkers`

Before creating visual risk-front objects, P8-B must verify:

- The high-detail scene opens without losing the accepted baseline state.
- The anchor root can be added without moving or rewriting imported PLATEAU objects.
- Origin and target placeholders are visible and framed in the scene.
- Placeholder positions are documented as prototype/gameplay visualization anchors, not official tsunami model outputs.
- Existing P2-P6 success/failure behavior still works in a staged smoke scene or temporary test harness.

## Hazard Data

P8-B should read P8-A hazard JSON/config from:

`Assets/Data/P8`

Required P8-A inputs:

- `tsunami_hazard_sample_chuo.json`
- `tsunami_hazard_layer_schema.json`
- `risk_front_visualization_config.json`
- `infrastructure_hazard_interaction_config.json`

P8-B can use hazard arrival/depth/boundary fields to drive visualization timing and placement, but the visualization must remain marked as cinematic/prototype when it exceeds science-layer values.

## Avoiding Chuo_BaseMap Dependency

P8-B code and scene objects must reference the high-detail baseline path or scene-local anchors. They must not hard-code `Assets/Scenes/Chuo_BaseMap.unity`.

If a fallback is needed for tests, use temporary generated objects in EditMode/PlayMode tests instead of loading `Chuo_BaseMap.unity`.

## P9/P10 Boundary

P8-B must not implement:

- P9 crowd simulation.
- P9 real spawn systems.
- P9 indoor shelter gameplay.
- P9 congestion or indoor evacuation failure mechanics.
- P10 packaging/release automation.

P8-B may create visual risk-front objects only after anchor verification. It must not add crowd, indoor, or packaging systems.

## Verification Before Visual Objects

Before P8-B creates the light curtain/risk-front visualization:

- Run the P8-A compatibility preflight.
- Confirm `P7_HighDetail_Chuo.unity` exists and remains protected.
- Confirm `Chuo_BaseMap.unity` is untouched.
- Confirm P8-A hazard JSON validates.
- Confirm P2-P6 compatibility report has no silent pass for pending runtime checks.
- Confirm no ProjectSettings or Packages churn is required.
