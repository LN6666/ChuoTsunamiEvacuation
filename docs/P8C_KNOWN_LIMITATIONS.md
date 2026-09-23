# P8-C Known Limitations

Date: 2026-05-24.

## Limitations

- Infrastructure interaction is proxy/data-driven where true PLATEAU semantic geometry is incomplete.
- The derived boundary may come from extracted points or grid extent and must not be claimed as an official inundation contour unless source evidence proves it.
- `maxTsunamiHeightMeters` is not full inundation-depth grid data.
- `visualHeightMeters` is cinematic-only and not physical hazard depth.
- P8-C does not implement building damage, blockage, or collapse proxy.
- P8-C does not implement P9 crowd, real spawn, indoor evacuation, congestion, or failure systems.
- P8-C does not implement P10 release packaging.
- P8-C does not change gameplay success/failure rules.
- Scene anchors were not forced into the high-detail scene because the scene was already local dirty before P8-C.
- Humanitarian/high-rise candidates are represented at proxy/data level only in P8-C. Persistent explicit visibility and handoff verification belong to P8-E.

## Follow-Up

P8-D should implement lightweight infrastructure damage/blockage/collapse proxy while preserving the same evidence and gameplay-rule guardrails. P8-E should close P8 and verify the final handoff to P9.
