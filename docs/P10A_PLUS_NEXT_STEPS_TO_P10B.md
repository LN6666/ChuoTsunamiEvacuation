# P10-A+ Next Steps To P10-B

P10-B should start from the P10-A+ hardening branch after validation passes.

P10-B focus after the stage correction:

- high-detail scene runtime smoke readiness
- manual playtest preparation
- tsunami-start green ground frame markers for evacuation-related buildings
- CPU usage
- memory usage
- FPS
- 1 percent low/stutter
- GC allocations if available
- loading time
- Player.log warnings/errors
- NPC count
- marker count
- light curtain impact
- UI/ResultPanel impact
- before/after optimization metrics

Windows x64 build is deferred to P10-C so the user can manually playtest after P10-B and request quick fixes before release packaging.

Use P10-A+ reports during P10-B:

- candidate anchor hardening report for marker count and warning checks
- candidate-to-building nearest-match report for visual plausibility checks
- entrance proxy report for entrance marker smoke
- route proxy validation report for route warning/geometry limitation checks
- semantic binding audit for claim boundaries
- high-detail smoke status for manual/runtime checklist

P10-B must still not claim:

- exact PLATEAU Unity object identity
- official route validation
- GIS-grade route geometry proof
- official inundation contour completion
