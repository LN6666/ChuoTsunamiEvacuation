# P9 Stage Plan

P9 has exactly four stages:

## P9-A

Crowd / Spawn / Entrance-Safe-Floor Proxy Foundation.

Scope:

- Define the P9 scaffold, data schemas, runtime-neutral loaders, tests, and preflight.
- Start while P8-C supplemental work and P8-D/E continue in parallel.
- Treat P8-D/E handoff data as optional and no-effect when missing.
- Avoid scene mutation, final failure gameplay, and release packaging.

## P9-B

New-map Scene Integration + Spawn/Crowd Runtime Prototype.

Scope:

- Integrate P9 scaffold with the new-map baseline after the P8 handoff is stable enough.
- Prototype spawn/crowd runtime behavior without changing final success/failure rules unless explicitly approved.
- Consume P8-E semantic binding, humanitarian candidate marker, P2-P6 adaptation, route/candidate geometry, and risk-front handoff data where available.
- Prepare runtime marker/proxy support for weighted spawn, entrance/safe-floor, humanitarian high-rise candidates, collapse/debris zones, and estimated route guidance without mutating the high-detail scene.
- Keep humanitarian candidates non-official and warning-required.
- Keep P5 routes as estimated prototype guidance, not official evacuation routes.

## P9-C

Evacuation Failure / Congestion / Vertical Evacuation Proxy Gameplay.

Scope:

- Implement final congestion, entrance-blocked, safe-floor, and evacuation-failure proxy gameplay after P8-D/E handoff inputs are available.
- Decide how P9 proxy state affects player outcomes only in this stage.

## P9-D

P9 Final Integration + Handoff to P10.

Scope:

- Stabilize P9 integration, documentation, test coverage, and P10 handoff.
- Do not implement P10 release packaging in P9.

No additional P9 stages are part of the approved P9 plan.

P9 uses the `P7_HighDetail_Chuo` practical baseline after P8 handoff.
Plain wording for automated checks: P9 uses P7_HighDetail_Chuo practical baseline after P8 handoff.
P9 does not solve the P8 hazard/front/infrastructure foundation, and P9-A starts with scaffold work while P8 finalizes.
