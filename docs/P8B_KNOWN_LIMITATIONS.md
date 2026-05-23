# P8-B Known Limitations

Date: 2026-05-23.

- Scene anchoring is deferred to avoid mutating the protected local high-detail baseline in this pass.
- The runtime uses sample/manual P8-A hazard data, not official reviewed hazard values.
- Hazard-layer v1 is manual-sample/evidence-shaped data, not official or academic final inundation data.
- Arrival, boundary, depth, intensity, confidence, sourceMode, and evidenceSourceId drive the front, but current values remain non-authoritative.
- Missing boundary data can use a procedural fallback visual; fallback boundaries are not evidence.
- Coordinate conversion is a local visualization transform for prototype rendering, not a verified PLATEAU geospatial transform.
- The visual curve is generated for readability and may be wavy or smoothed.
- The visual height may be kilometer-scale but is not physical tsunami height.
- The implementation is no real-time fluid simulation.
- No official hazard or evacuation route claim is made.
- P8-C infrastructure hazard interaction is not implemented.
- P8-D collapse proxy behavior is not implemented.
- P9 crowd/spawn/indoor systems are not implemented.
