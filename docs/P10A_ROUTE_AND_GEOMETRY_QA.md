# P10-A Route And Geometry QA

P10-A preserves the P5/P9 route boundary.

Required wording:

- estimated prototype guidance
- not official evacuation route
- not fully road-geometry validated
- coordinate-projected gameplay proxy where applicable

QA status:

- P5 route handoff remains a gameplay guidance input.
- P9-D route proxy anchors remain `routeIsOfficial = false`.
- P9-D route proxy anchors remain `routeEstimatedPrototypeGuidance = true`.
- Coordinate projection is usable for gameplay but is not GIS-grade validation.
- Exact road geometry validation remains a known limitation.

P10-A does not attempt to solve:

- official route validation
- GIS-grade snapping to road centerlines
- official inundation contour refinement
- exact PLATEAU object/road semantic coverage
