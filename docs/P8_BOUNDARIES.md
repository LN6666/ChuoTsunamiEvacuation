# P8 Boundaries

## P8 Positioning

P8 is evidence-based tsunami hazard interaction and dynamic risk-front visualization on the completed P7 practical high-detail baseline.

P8 now has exactly five stages: P8-A, P8-B, P8-C, P8-D, and P8-E. P8-E is allowed and reserved for final P8 closeout, humanitarian candidate persistent visibility/handoff verification, and final pre-P9 checks. Do not create P8-0, P8-F, P8-G, or any other extra P8 stage.

## In Scope

- Tsunami arrival time.
- Inundation depth.
- Water level and tsunami height where evidence supports them.
- Inundation boundary.
- Hazard intensity.
- Flood progression and arrival front.
- Cinematic risk front or light curtain visualization.
- Road, building, bridge, underground, entrance, waterfront, open_space, shelter_proxy, navigation_target_proxy, humanitarian_candidate_proxy, and highrise_candidate_marker hazard interaction at proxy/data level.
- Lightweight infrastructure damage, blockage, and collapse proxy in P8-D only.
- P8-E final closeout and P9 readiness verification.

## P8-A Scope

P8-A is foundation only. It adds schemas, sample data, configuration, loader/validator code, compatibility gates, tests, preflight, and review prompts.

P8-A does not render the dynamic light curtain, apply hazard interactions, or trigger collapse behavior.

## P8-B Scope

P8-B establishes the official/evidence tsunami hazard layer v1 and dynamic cinematic risk front. It must use Tokyo Metropolitan Government tsunami damage-estimation datasets where extracted and must not downgrade back to generic flood proxy data when the official spatial layer exists.

P8-B keeps `maxTsunamiHeightMeters` separate from `inundationDepthMeters`, and keeps `visualHeightMeters` cinematic only.

## P8-C Scope

P8-C solves P2-P6 on the P7 high-detail baseline at smoke/proxy level and links the risk-front/hazard-layer fields to infrastructure hazard states.

P8-C does not change gameplay success/failure rules and does not implement P8-D collapse, P9 final gameplay, or real indoor scene behavior.

## P8-D Scope

P8-D remains infrastructure damage, blockage, and lightweight collapse proxy. It is not implemented by the P8-B/C consolidation gate.

P8-D must not implement real structural collapse, real tsunami fluid simulation, P9 crowd/spawn/congestion/indoor gameplay, or P10 packaging.

## P8-E Scope

P8-E verifies final P8 closure before P9 starts. It should ensure humanitarian/high-rise candidates are persistently visible or explicitly handed off with non-official labeling, and that P9 has clear instructions for future life-first selectable vertical evacuation targets.

P8-E must not be expanded into P8-F/G.

## Out Of Scope

- Full real-time fluid simulation.
- P9 crowd, real spawn, indoor shelter evacuation, congestion, final failure gameplay, or final success/failure redesign.
- P10 release packaging.
- Gameplay success/failure rule changes.
- New imports or PLATEAU raw data modifications.
- Claiming non-official humanitarian candidates as official shelters.
- Claiming a derived grid boundary as an official inundation contour.

## Stage Count Boundary

P8 remains limited to the five stages defined in `docs/P8_STAGE_PLAN.md`: P8-A through P8-E only.

P9 should focus on final real gameplay landing, not fixing missing P8 hazard/front/infrastructure foundation work.
