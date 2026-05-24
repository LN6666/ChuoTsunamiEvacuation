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

The risk front is a cinematic visualization driven by hazard layer values. visualHeightMeters is not physical tsunami height.

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
- `docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md`

Candidates are non-official and require explicit warning labels.

## P9 Focus

P9 should focus on:

- crowd interaction;
- real/rule-based spawn points;
- entrance/safe-floor proxy;
- vertical evacuation proxy;
- evacuation failure/congestion gameplay;
- final real gameplay landing.

P9 should not redo P8 tsunami hazard data extraction, dynamic risk-front foundation, infrastructure hazard/damage proxy foundation, or humanitarian candidate audit.
