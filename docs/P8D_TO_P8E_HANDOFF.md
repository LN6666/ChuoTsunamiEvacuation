# P8-D To P8-E Handoff

Date: 2026-05-24.

## P8-D Outputs

- `Assets/Scripts/P8/P8InfrastructureDamageEvaluator.cs`
- `Assets/Scripts/P8/P8CollapseProxyRule.cs`
- `Assets/Scripts/P8/P8DamageProxyMarker.cs`
- `Assets/Scripts/P8/P8HumanitarianCandidateHazardStatus.cs`
- `Assets/Data/P8/infrastructure_damage_proxy_config.json`
- `Assets/Data/P8/humanitarian_candidate_hazard_status_config.json`

## P8-E Verification Scope

P8-E should verify:

- final P8 stage closure before P9 starts;
- persistent explicit visibility for humanitarian candidates or a clear P9 handoff if scene placement remains deferred;
- non-official warnings/disclaimers on all humanitarian candidate presentation;
- no official shelter claim;
- no physical collapse, debris, destruction, or real indoor scene behavior;
- P9 receives the entrance / safe-floor / evacuation-complete proxy guidance.

## P9 Boundary

P9 owns any final real gameplay landing. P9 may decide whether reviewed humanitarian candidates become life-first selectable vertical evacuation targets, but must keep them non-official.
