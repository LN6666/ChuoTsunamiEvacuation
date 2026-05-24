# P9-C Known Limitations

- P9-C is deterministic gameplay proxy logic, not a validated evacuation behavior simulator.
- Collapse/debris fatality is exposure-event gameplay probability, not structural engineering simulation.
- Crowd and queue delay are bounded scalar rules, not a full pedestrian dynamics model.
- Safe-floor status depends on proxy data. It does not inspect real floors, stairs, corridors, or refuge areas.
- P8 hazard/front data is consumed through handoff-style fields. P9-C does not refine official inundation contours.
- P5 routes remain estimated prototype guidance and are not official routes.
- Humanitarian high-rise candidates remain non-official and warning-required.
- ResultPanel feedback is prototype/debug text, not final UI polish.
- Windows EXE profiling and release packaging remain P10 scope.
