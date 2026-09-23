# P8 Stage Plan

P8 has exactly five stages.

Allowed P8 stages:

- P8-A
- P8-B
- P8-C
- P8-D
- P8-E

Do not create P8-0, P8-F, P8-G, or any other extra P8 stage.

## P8-A

Baseline handoff from P7, P2-P6 compatibility gate, and tsunami hazard data layer foundation.

Deliverables:

- Preserve `P7_HighDetail_Chuo.unity` as the practical baseline.
- Confirm P2-P6 compatibility assumptions against the new baseline.
- Add hazard data schema, sample data, visualization config, infrastructure interaction config, loader, validator, tests, and preflight.
- Keep scientific hazard data separate from cinematic visualization parameters.

## P8-B

Official/evidence tsunami hazard layer v1 plus dynamic risk-front / cinematic light curtain.

Deliverables:

- Use or reference Tokyo Metropolitan Government tsunami damage-estimation datasets where available, not generic flood proxy data.
- Keep official spatial tsunami samples separate from derived/prototype boundaries.
- Implement visual risk-front progression without claiming physical fluid simulation.
- Keep cinematic height marked as visual/gameplay representation only.

## P8-C

P2-P6 new-map smoke/proxy adaptation plus hazard/risk-front-driven infrastructure hazard states.

Deliverables:

- Connect hazard layers to infrastructure categories where P7 scene evidence exists.
- Use proxy, marker, or rule-based fallbacks where detailed geometry is missing.
- Support road, building, bridge, underground, entrance, waterfront, open_space, shelter_proxy, navigation_target_proxy, humanitarian_candidate_proxy, and highrise_candidate_marker data-only hazard state evaluation.
- Keep gameplay success/failure rules unchanged.

## P8-D

Infrastructure damage, blockage, and lightweight collapse proxy.

Deliverables:

- Add data-driven damage/blockage/collapse proxy behavior only after P8-B/C consolidation is accepted.
- Preserve official/non-official candidate separation.
- Do not implement real structural collapse, real fluid simulation, P9 gameplay, or indoor scenes.

## P8-E

Final P8 closeout, humanitarian candidate persistent visibility/handoff, and final verification before P9.

Deliverables:

- Verify P8-A through P8-D are complete before P9 starts.
- Verify humanitarian/high-rise candidate persistent visibility expectations and non-official labeling are handed off clearly.
- Confirm P9 can focus on final real gameplay landing instead of repairing missing P8 hazard/front/infrastructure foundations.
- Close P8 without implementing P9 crowd, real spawn, indoor shelter, congestion, final failure gameplay, or P10 packaging systems.
