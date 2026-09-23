# P8-D Damage Proxy Rulebook

Date: 2026-05-24.

The damage evaluator is implemented in `Assets/Scripts/P8/P8InfrastructureDamageEvaluator.cs`.

## Rule Basis

Rules use P8-C hazard evaluation values:

- `arrivalTimeSeconds`
- `inundationDepthMeters`
- `hazardIntensity`
- `confidence`
- `sourceMode`
- `evidenceSourceId`

The P8-C evaluator already handles spatial/boundary availability, derived/prototype status, and nearest official spatial sample selection.

## Thresholds

Config file: `Assets/Data/P8/infrastructure_damage_proxy_config.json`.

- warning depth: `0.05m`
- low-floor inundation warning depth: `0.3m`
- entrance blocked depth: `0.3m`
- road restricted depth: `0.3m`
- bridge restricted depth: `0.5m`
- underground avoid depth: `0.05m`
- building damage proxy depth: `1.0m`
- collapse proxy visual depth: `1.5m`
- collapse proxy probability: `0.03`
- max collapse proxy sample count: `8`

## State Precedence

1. Severe exposure can produce restricted, avoid, inaccessible, or building damage proxy states.
2. Building and high-rise candidate exposure above the low-floor threshold can produce `low_floor_inundation_warning`.
3. Entrance, road, bridge, and underground categories receive category-specific blockage/restriction states.
4. `collapsed_proxy_visual` can appear only for building-like categories and only through the deterministic low-probability rule.
5. Manual-review candidates retain manual review metadata even when a stronger hazard state is emitted.

## Field Separation

`inundationDepthMeters` drives physical proxy thresholds.

`maxTsunamiHeightMeters` is not treated as local inundation depth.

`visualHeightMeters` is not treated as physical hazard depth.
