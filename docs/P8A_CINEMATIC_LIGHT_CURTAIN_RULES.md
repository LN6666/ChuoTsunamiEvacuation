# P8-A Cinematic Light Curtain Rules

## P8-A Status

P8-A does not implement the visual light curtain.

This document defines rules that P8-B must follow when it implements the dynamic tsunami light curtain and risk-front visualization.

## Required Rules

- `visualHeightMeters` may be kilometer-scale for cinematic gameplay readability.
- Large `visualHeightMeters` values must set `visualHeightIsCinematicOnly=true`.
- Visual height is not real tsunami physical height.
- Visual height is not inundation depth.
- Visual height is not water level.
- Visual height must not be used as evidence for hazard claims.
- The risk front visual may be non-linear, wavy, smoothed, or stylized in P8-B.
- The visual curtain boundary must be distinguishable from the data boundary.
- The visual layer must not change gameplay success/failure rules unless a later approved stage explicitly changes those rules.

## Science and Visual Field Split

Science fields:

- `arrivalTimeSeconds`
- `inundationDepthMeters`
- `waterLevelMeters`
- `tsunamiHeightMeters`
- `inundationBoundary`
- `hazardIntensity`
- `confidence`
- `evidenceSourceId`

Visual fields:

- `visualHeightMeters`
- `visualHeightIsCinematicOnly`
- future risk-front animation fields
- future curtain material and wave-shape fields

## P8-B Handoff

P8-B may read P8-A hazard data and visualization config, but it must preserve these labels in code, docs, and tests:

- cinematic visual height,
- non-physical light curtain,
- data boundary versus visual boundary,
- no real-time fluid simulation.
