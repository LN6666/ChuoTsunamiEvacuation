# P8-C Hazard To Infrastructure Mapping

Date: 2026-05-24.

## Mapping Rule

Infrastructure state is derived from hazard-layer v1 values, not from cinematic visuals.

Inputs:

- `arrivalTimeSeconds` determines before-front, contact-window, and after-front phase.
- `inundationDepthMeters` determines watch/warning/restricted/inundated proxy severity.
- `inundationBoundary` determines whether a target is within the hazard boundary when geometry can be compared.
- `hazardIntensity` can raise severity when depth is low or unavailable.
- `confidence`, `sourceMode`, and `evidenceSourceId` are copied into the evaluation result for provenance.

Non-inputs:

- `maxTsunamiHeightMeters` is not substituted for inundation depth.
- `visualHeightMeters` is cinematic only.
- The light curtain mesh height does not drive physical hazard state.

## Category Mapping

| Category | P8-C State Behavior |
|---|---|
| road | watch before arrival, restricted_proxy at moderate/high hazard |
| building | watch/warning/inundated_proxy, no collapse behavior |
| bridge | warning or restricted_proxy |
| underground | avoid_proxy at moderate/high hazard |
| entrance | warning or restricted_proxy |
| waterfront | avoid_proxy at moderate/high hazard |
| open_space | warning or restricted_proxy |
| shelter_proxy | warning/restricted_proxy as proxy-only shelter interaction context |
| navigation_target_proxy | restricted_proxy/avoid_proxy for guidance target smoke tests |

## Missing Data

If a target lacks precise spatial semantics or the hazard feature lacks comparable boundary/depth data, P8-C uses a conservative proxy fallback and marks the result as proxy/prototype/pending.

If the whole hazard layer is missing, P8-C fails safe: state remains `safe`, `failSafe=true`, and no gameplay result is changed.

## Boundary Note

The current P8-B boundary may be a derived grid extent from extracted points. It must not be described as a Tokyo-issued official inundation contour unless future source evidence proves that.
