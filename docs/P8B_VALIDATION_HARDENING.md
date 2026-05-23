# P8-B Validation Hardening

P8-B guard work hardens the risk-front foundation before visual scene implementation. The visual curtain is cinematic only and must not be confused with scientific hazard fields.

## Enforced Field Separation

Science fields remain:

- `arrivalTimeSeconds`
- `inundationDepthMeters`
- `waterLevelMeters`
- `tsunamiHeightMeters`
- `inundationBoundary`
- `hazardIntensity`
- `confidence`
- `evidenceSourceId`

Visual fields remain:

- `visualHeightMeters`
- `visualHeightIsCinematicOnly`

`visualHeightMeters` is not `tsunamiHeightMeters`, not `waterLevelMeters`, and not inundation depth. Large visual values require `visualHeightIsCinematicOnly=true`.

## Config Checks

`tools/p8/validate_p8b_riskfront_config.ps1` checks:

- `visualHeightMeters` guard.
- `boundaryIsEvidenceBasedOrPrototype` is explicit.
- `sourceMode` does not claim official values.
- `geometryType` is one of `grid`, `polygon`, `polyline`, `point`, or `synthetic`.
- Manual sample data is not marked official.
- Collapse proxy stays disabled or data-only until P8-D.
- P8-B does not include P8-C road/building/bridge/underground behavior.
- P8-B does not include P9 systems or P10 systems.
- P8-B keeps an explicit no P9 systems boundary.

## Scope

This branch does not implement a full fluid simulation, road/building/bridge/underground interactions, collapse gameplay, or gameplay success/failure changes.
