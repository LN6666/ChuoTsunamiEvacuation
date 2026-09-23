# P10-B Green Ground Frame Markers

P10-B adds a lightweight runtime visual marker system for evacuation-related buildings.

After tsunami start or hazard/warning activation, the runtime can display green rectangular ground frames for:

- official evacuation building targets where official shelter data exists
- non-official humanitarian high-rise candidates
- life-first vertical evacuation candidate targets

The frame means evacuation-related marker or candidate/target marker. It does not mean official approval or default safety.

## Semantics

For non-official humanitarian candidates:

- `isOfficialShelter=false`
- `nonOfficialWarningRequired=true`
- `safeApprovedByDefault=false`
- warning text remains required in UI/debug/ResultPanel contexts
- the green frame must never display the candidate as an official shelter

## Geometry

The runtime prefers known renderer bounds or footprint-like data when safely available. In current P10-B data, exact building footprints are not proven, so frames are coordinate-derived rectangle proxies from P9-D anchoring.

This is documented as a footprint proxy / coordinate-derived rectangle. Exact PLATEAU Unity object identity and exact building footprint geometry are not claimed.

## Runtime And Performance

- Frames are hidden before tsunami start unless debug preview is enabled.
- Frame objects are pooled and reused.
- No mesh or line recreation is required per frame for stable targets.
- Debug labels are disabled by default.
- Frame count is capped by configuration.
- Ground offset avoids z-fighting.
- The feature is configurable via `Assets/Data/P10/p10b_green_ground_frame_config.json`.
