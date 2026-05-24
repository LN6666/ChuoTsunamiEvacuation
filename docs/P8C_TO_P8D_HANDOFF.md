# P8-C To P8-D Handoff

Date: 2026-05-24.

## P8-C Output

P8-C provides:

- infrastructure hazard categories and states
- hazard-layer-driven evaluator
- risk-front arrival/contact/after-arrival phase link
- proxy/marker components
- P2-P6 runtime adaptation inspector
- EditMode and PlayMode smoke tests
- P8-C preflight

## Guardrails Preserved

- no `Chuo_BaseMap.unity` modification
- no ProjectSettings or Packages modification
- no `Assets/PLATEAU` modification
- no gameplay success/failure rule changes
- no P8-D collapse proxy implementation
- no P9 crowd, real spawn, indoor evacuation, congestion, or failure systems
- no P10 packaging

## P8-D Recommended Scope

P8-D should implement lightweight infrastructure damage, blockage, and collapse proxy.

P8-D should continue to use hazard-layer v1 provenance and should not treat cinematic risk-front height as physical water depth.

P8-D should preserve proxy labels where PLATEAU semantic geometry remains incomplete.

## P8-E Closeout Scope

P8-E is now the final P8 closeout stage. It should verify P8-A through P8-D before P9 starts, confirm humanitarian/high-rise candidate persistent visibility or explicit handoff labeling, and make sure P9 focuses on final real gameplay landing instead of repairing missing P8 hazard/front/infrastructure foundations.
