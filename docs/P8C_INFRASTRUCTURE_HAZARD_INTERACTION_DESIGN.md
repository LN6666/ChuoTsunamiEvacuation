# P8-C Infrastructure Hazard Interaction Design

Date: 2026-05-24.

## Scope

P8-C connects the P8-B hazard-layer v1 and risk-front timing model to infrastructure hazard states on the P7 high-detail map baseline. It is a smoke/proxy-level runtime adaptation stage, not a full semantic PLATEAU integration stage.

P8-C solves the remaining P2-P6 runtime adaptation concern at smoke/proxy level and connects hazard layer/risk-front data to infrastructure proxies/states.

## Runtime Model

Implemented scripts:

- `P8InfrastructureHazardData.cs`
- `P8InfrastructureHazardState.cs`
- `P8InfrastructureHazardEvaluator.cs`
- `P8InfrastructureHazardTarget.cs`
- `P8InfrastructureHazardProxy.cs`
- `P8InfrastructureHazardMarker.cs`
- `P8InfrastructureHazardDebugSummary.cs`
- `P8P2P6RuntimeCompatibilityInspector.cs`

Supported infrastructure categories:

- road
- building
- bridge
- underground
- entrance
- waterfront
- open_space
- shelter_proxy
- navigation_target_proxy

Supported hazard states:

- safe
- watch
- warning
- inundated_proxy
- restricted_proxy
- avoid_proxy

## Data Driver

The evaluator uses these P8-B hazard-layer v1 fields:

- `arrivalTimeSeconds`
- `inundationDepthMeters`
- `inundationBoundary`
- `hazardIntensity`
- `confidence`
- `sourceMode`
- `evidenceSourceId`

The evaluator records `maxTsunamiHeightMeters` only as reference metadata. It does not use it as inundation depth.

The evaluator records `visualHeightMeters` only as cinematic metadata. It does not use cinematic light-curtain height as physical hazard depth.

## Scene Safety

The components are scene-safe:

- no `Chuo_BaseMap.unity` dependency
- no P9 dependency
- no gameplay result mutation
- no success/failure rule changes
- fail-safe when the hazard layer is missing
- testable with temporary GameObjects

The local `P7_HighDetail_Chuo.unity` scene was already dirty before P8-C work. P8-C does not force scene mutation; anchors/proxies are represented through runtime components, tests, and documented manual setup.

## Explicit Non-Scope

P8-C does not implement building collapse proxy. P8-D owns lightweight damage/collapse proxy.

P8-C does not implement P9 crowd, real spawn, indoor evacuation, congestion, or indoor shelter gameplay.

P8-C does not alter gameplay success/failure rules.
