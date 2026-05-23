# P8-B Next To P8-C Handoff

Date: 2026-05-23.

## P8-B Output

P8-B provides a data-driven cinematic risk-front visualization layer:

- hazard/config loading through P8-A data files
- hazard-layer v1 front selection by `arrivalTimeSeconds`
- boundary/prototype geometry from `inundationBoundary`
- visual warning intensity from `inundationDepthMeters` and `hazardIntensity`
- provenance reporting through `confidence`, `sourceMode`, and `evidenceSourceId`
- time-driven risk-front progression
- non-linear visual curve generation
- lightweight mesh light curtain rendering
- cinematic disclaimer and fail-safe hidden behavior

## Required Before P8-C

- Confirm safe scene anchoring inside `P7_HighDetail_Chuo.unity`.
- Verify representative visual root placement and camera visibility.
- Keep the visual boundary separate from the data boundary.
- Preserve existing gameplay success/failure rules unless a later approved task changes them.

## P8-C Boundary

P8-C may connect hazard data to road/building/bridge/underground hazard interaction where P7 scene evidence exists.

P8-B does not implement P8-C. P8-B remains not physical tsunami height, no real-time fluid simulation, and no official hazard value. Current v1 data is manual sample/prototype data until reviewed official or academic evidence is supplied.
