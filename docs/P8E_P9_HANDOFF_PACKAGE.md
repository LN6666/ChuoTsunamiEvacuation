# P8-E P9 Handoff Package

Date: 2026-05-24.

## Baseline

- Practical baseline: `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- Legacy fallback: `Assets/Scenes/Chuo_BaseMap.unity`

The practical baseline may be local dirty/local-only. P9 must preserve it carefully and must not assume a fresh worktree automatically contains the local scene state.

## Hazard Layer

- `Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json`
- `Assets/Data/P8/tsunami_hazard_evidence_registry.json`
- `Assets/Data/P8/tsunami_hazard_layer_schema.json`

P8-B extracted Chuo tsunami samples for Taisho Kanto and Nankai Trough case 1 under the broader Tokyo metropolitan damage-estimation source family. Derived boundaries are not official contours unless official contour evidence is separately proven.

## Risk Front

- `Assets/Scripts/P8/P8RiskFrontController.cs`
- `Assets/Scripts/P8/P8RiskFrontCurveGenerator.cs`
- `Assets/Scripts/P8/P8RiskFrontLightCurtainRenderer.cs`
- `Assets/Data/P8/risk_front_visualization_config.json`
- `Assets/Scripts/P8/P8RiskFrontProgressionModel.cs`
- `Assets/Scripts/P8/P8RiskFrontSpeedModel.cs`
- `Assets/Data/P8/risk_front_progression_model_config.json`

The risk front is a cinematic visualization driven by hazard layer values. `arrivalTimeSeconds` drives front progression when present; fallback onshore speed is used only for interpolation gaps and includes shallow-depth slowdown. `inundationDepthMeters` and `hazardIntensity` control visual/risk strength. visualHeightMeters is not physical tsunami height and maxTsunamiHeightMeters is not inundationDepthMeters.

## Infrastructure Hazard Status

- `Assets/Scripts/P8/P8InfrastructureHazardEvaluator.cs`
- `Assets/Data/P8/infrastructure_hazard_interaction_config.json`

P8-C maps hazard/front values to data-only infrastructure states for roads, buildings, bridges, underground, entrances, waterfront, open space, shelter proxies, navigation target proxies, humanitarian candidate proxies, and high-rise candidate markers.

## Damage / Blockage / Collapse Proxy

- `Assets/Scripts/P8/P8InfrastructureDamageEvaluator.cs`
- `Assets/Scripts/P8/P8CollapseProxyRule.cs`
- `Assets/Scripts/P8/P8DamageProxyMarker.cs`
- `Assets/Data/P8/infrastructure_damage_proxy_config.json`

P8-D provides visual/status proxy only. It is not real structural collapse, physics collapse, debris, or official damage prediction.

## Humanitarian Candidates

- `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`
- `Assets/Data/P8/humanitarian_candidate_hazard_status_config.json`
- `Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json`
- `Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json`
- `docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md`
- `docs/P8E_HUMANITARIAN_CANDIDATE_PERSISTENT_MARKER_READINESS.md`

Candidates are non-official and require explicit warning labels.

## Semantic Binding And Geometry

- `Assets/Data/P8/p8e_semantic_binding_v1.json`
- `Assets/Data/P8/p8e_p2_p6_new_map_adaptation_matrix.json`
- `Assets/Data/P8/p8e_route_candidate_geometry_handoff.json`
- `docs/P8E_PLATEAU_SEMANTIC_BINDING_V1.md`
- `docs/P8E_P2_P6_PASSED_PROXY_BLOCKED_MATRIX.md`
- `docs/P8E_P5_ROUTE_CANDIDATE_GEOMETRY_HANDOFF.md`

P8-E hardening provides semantic binding v1 as metadata/proxy/data-only where full scene object binding is not proven. Routes remain estimated prototype guidance, not official routes and not road-geometry-validated.

## P9 Focus

P9 should focus on:

- crowd interaction;
- real/rule-based spawn points;
- entrance/safe-floor proxy;
- vertical evacuation proxy;
- evacuation failure/congestion gameplay;
- final real gameplay landing.

P9 should not redo P8 tsunami hazard data extraction, dynamic risk-front foundation, infrastructure hazard/damage proxy foundation, or humanitarian candidate audit.

P9 should also not redo semantic binding v1, humanitarian persistent marker readiness, light-curtain progression assumptions, P2-P6 adaptation matrix, or route/candidate geometry handoff unless new evidence is added.
