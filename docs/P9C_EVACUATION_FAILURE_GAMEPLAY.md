# P9-C Evacuation Failure Gameplay

P9-C converts the P9-B marker/runtime prototype into deterministic gameplay proxy outcomes.

Implemented scope:

- life-first vertical evacuation target selection
- non-official humanitarian candidate warnings
- entrance open, crowded, blocked, hazard-affected, and closed states
- queue and congestion delay with caps
- safe-floor proxy success or failure
- hazard arrival timing comparison through P8 handoff-style inputs
- collapse/debris exposure-event fatality proxy
- result feedback and structured run log records

Boundary:

- no real building interior scene
- no indoor path, stair, or fire-route gameplay
- no heavy crowd package
- no scientific crowd simulation claim
- no official route claim
- no official shelter claim for humanitarian candidates

The P9-C outcome layer is gameplay-facing and deterministic. It can set success, delayed success, or failure reason codes, but all claims remain prototype-level.
