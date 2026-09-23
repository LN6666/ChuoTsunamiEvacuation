# P10-B Performance Profiling

P10-B prepares and tests lightweight runtime metrics. Final Windows EXE profiling is deferred to P10-C.

Metrics to collect in P10-B/P10-C:

- average FPS
- 1 percent low FPS or stutter proxy
- frame time min/avg/max
- CPU/frame-time proxy where available
- memory usage
- GC allocation or collection proxy where available
- loading time
- Player.log warnings/errors
- NPC count
- marker count
- green ground frame count
- light curtain enabled/disabled impact
- ResultPanel/UI impact

`P10BPerformanceMetricsRecorder` records frame-time samples and count metadata without external profiler packages.

## Optimization Rule

P10-B optimization must stay low-risk. Before/after metrics should be captured where possible. If exact before/after evidence is not available in Editor tests, the limitation must be documented and the manual profiling checklist must carry it to P10-C.
