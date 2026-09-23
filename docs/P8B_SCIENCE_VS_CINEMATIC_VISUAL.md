# P8-B Science vs Cinematic Visual

Date: 2026-05-23.

## Science/Data Layer

The P8-B controller consumes P8-A sample/manual hazard data:

- `arrivalTimeSeconds`
- `timeOriginSeconds`
- `inundationDepthMeters`
- `waterLevelMeters`
- `tsunamiHeightMeters`
- `inundationBoundary`
- `confidence`
- `evidenceSourceId`

These values are sample/manual development data in P8-B. They are no official hazard values and no official evacuation guidance.

## Cinematic Visual Layer

The generated light curtain uses:

- `visualHeightMeters`
- `visualHeightIsCinematicOnly`
- `segmentCount`
- `waveAmplitudeMeters`
- `waveFrequency`
- `noiseStrengthMeters`
- `frontTravelMeters`
- `materialAlpha`

These values produce a readable visual front. They are not physical tsunami height, no real-time fluid simulation, and no official hazard claim.

## Boundary Separation

`inundationBoundary` is the data boundary.

The P8-B visual curve is a generated curtain boundary derived from the data boundary for rendering only. It may be wavy or smoothed and must remain separate from the science/data layer.
