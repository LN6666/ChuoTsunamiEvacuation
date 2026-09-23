# P8-B Known Limitations

Date: 2026-05-24.

- Scene anchoring is deferred to avoid mutating the protected local high-detail baseline in this pass.
- The P8-B runtime uses evidence-aware P8-B hazard-layer v1 data; the older manual P8-A sample remains only for compatibility checks.
- Hazard-layer v1 uses Tokyo Metropolitan Government tsunami Open Data mesh records clipped to Chuo, but it is not a full real-time fluid simulation.
- Chuo lacks an apparent standalone tsunami hazard map equivalent to flood hazard maps, but Tokyo metropolitan tsunami evidence is still usable as the primary candidate.
- Maximum tsunami height references around 2.4m to 2.46m are not a full Chuo inundation-depth grid by themselves.
- The current `inundationBoundary` is a derived grid-extent bbox, not a Tokyo-issued official inundation contour.
- Arrival, boundary, depth, intensity, confidence, sourceMode, sourceCategory, extractionStatus, and evidenceSourceId drive the front.
- Missing boundary data can use a procedural fallback visual; fallback boundaries are not evidence.
- Coordinate conversion is a local visualization transform for prototype rendering, not a verified PLATEAU geospatial transform.
- The visual curve is generated for readability and may be wavy or smoothed.
- The visual height may be kilometer-scale but is not physical tsunami height.
- The implementation is no real-time fluid simulation.
- No official evacuation route claim is made.
- P8-C infrastructure hazard interaction is not implemented.
- P8-D collapse proxy behavior is not implemented.
- P9 crowd/spawn/indoor systems are not implemented.
