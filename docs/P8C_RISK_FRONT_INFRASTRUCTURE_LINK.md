# P8-C Risk Front Infrastructure Link

Date: 2026-05-24.

## Link Model

P8-C links the dynamic risk front to infrastructure through the same hazard-layer v1 timing and hazard fields used by P8-B.

The infrastructure evaluator determines:

- before front arrival
- at risk-front contact
- after front arrival
- depth/intensity affected

This is not geometric fluid simulation. It is a time/depth/intensity evaluation over target proxies or markers.

## Implementation

`P8InfrastructureHazardEvaluator` selects the active hazard feature using `P8RiskFrontCurveGenerator.SelectFeatureForTime`.

It then evaluates:

- `arrivalTimeSeconds`
- nearest available `spatialSamples[].inundationDepthMeters` or feature-level `inundationDepthMeters`
- `inundationBoundary` where comparable
- `hazardIntensity`
- provenance fields

The result includes the selected feature ID, evidence source ID, source mode, contact phase, state, depth, intensity, confidence, and proxy/fallback flags.

## Separation From Visuals

The P8-B light curtain remains cinematic. P8-C does not read mesh height as water depth.

`visualHeightMeters` remains cinematic only.

`maxTsunamiHeightMeters` remains source metadata and does not replace the spatial inundation-depth field.
