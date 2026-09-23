# P8-C Problem 3 Risk-Front Infrastructure Closure Status

Date: 2026-05-24.

## Decision

Problem 3 is consolidated for P8-C.

The P8-B risk front and official/evidence hazard layer v1 now drive P8-C infrastructure hazard states at proxy/data level.

## Supported Categories

P8-C supports:

- `road`
- `building`
- `bridge`
- `underground`
- `entrance`
- `waterfront`
- `open_space`
- `shelter_proxy`
- `navigation_target_proxy`
- `humanitarian_candidate_proxy`
- `highrise_candidate_marker`

## State Inputs

Infrastructure hazard state is based on:

- `arrivalTimeSeconds`
- `inundationDepthMeters`
- `inundationBoundary`
- `hazardIntensity`
- `confidence`
- `sourceMode`
- `sourceCategory`
- `evidenceSourceId`

`maxTsunamiHeightMeters` is reference metadata only and is not used as physical depth.

`visualHeightMeters` is cinematic only and is not used as physical hazard depth.

## Closure Boundary

P8-C closes the risk-front-to-infrastructure link, but not damage/collapse behavior.

P8-D will close infrastructure damage, blockage, and lightweight collapse proxy.

P8-E will verify final P8 closure, humanitarian candidate persistent visibility/handoff, and pre-P9 readiness.
