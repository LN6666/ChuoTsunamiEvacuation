# P8-C Problem 2 P2-P6 New-Map Adaptation Status

Date: 2026-05-24.

## Decision

Problem 2 is consolidated at P8-C smoke/proxy level.

P8-C confirms that P2-P6 can adapt to `P7_HighDetail_Chuo.unity` through source compatibility, proxy targets, and fail-safe data paths. This is not final gameplay landing; that remains P9.

No gameplay success/failure rule changes were introduced.

## Phase Status

| Phase | P8-C status | Notes |
|---|---|---|
| P2 movement/camera/result flow | smoke/source compatible | Player movement, camera flow, and ResultPanel success/failure flow remain compatible and unchanged. |
| P2 shelter interaction | proxy-based | `shelter_proxy` targets can receive P8-C hazard state without calling `BuildingShelter`, `ShelterEntranceTrigger`, `EvacuationGameManager`, or `ResultPanelController`. |
| P3 runtime data assumptions | passed | P8-B hazard layer v1 loads from `Assets/Data/P8` and carries provenance fields. |
| P4 real shelter marker/loading | proxy-based | Real shelter loading and marker logic can map to P8-C marker/proxy components. True high-detail entrance semantics remain pending. |
| P5 qualified shelter/route/candidate metadata | proxy-based | Qualified shelter, route, high-rise candidate, and humanitarian candidate metadata can adapt to proxy targets. No official-route or official-candidate overclaim is made. |
| P6 navigation/NPC prototype | proxy-based | Guidance and NPC prototype staging can target P8-C proxy transforms without `Chuo_BaseMap.unity`; no P9 crowd, real spawn, congestion, or indoor gameplay is required. |

## Boundaries

- P8-C does not mutate `Chuo_BaseMap.unity`.
- P8-C does not require real indoor scenes; real indoor scene gameplay is cancelled because there is no LOD4/BIM indoor scene.
- Future vertical evacuation should use entrance / safe floor / shelter completion proxy, not real interior stair/fire-route scenes.
- P9 owns final real gameplay landing.
