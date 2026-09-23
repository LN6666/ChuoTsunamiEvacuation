# P10-A P10-B Readiness

P10-B becomes: Windows EXE Build + Performance Profiling + Stress Test + Optimization Pass.

P10-A does not build the final Windows EXE and does not create release package artifacts.

Build target:

- Windows x64

Quality preset plan:

- Low
- Medium
- High

Benchmark scenarios:

- startup to first playable
- normal success
- congestion delay success
- entrance blocked failure
- safe-floor failure
- crowd-delay hazard failure
- collapse/debris fatality
- collapse disabled success
- high-detail scene marker generation
- light curtain visual sweep

Stress-test scenarios:

- 116 anchor targets visible or queryable
- capped NPC runtime spawn
- dense candidate marker visibility
- long ResultPanel warning text
- risk front/light curtain visible
- repeated runs for log growth

Metrics to collect:

- FPS
- 1 percent low or stutter
- CPU usage
- memory usage
- GC allocations if available
- loading time
- Player.log errors/warnings
- NPC count
- marker count
- light curtain impact
- UI/ResultPanel impact

Optimization candidate checklist:

- marker density cap
- NPC cap
- debug visual toggles
- log throttling
- object pooling opportunities
- Update-loop hot spots
- UI text/layout risks
- light curtain visual cost

Optimization rule:

- P10-B performance optimization must use before/after metrics where possible.
- Low-risk fixes are allowed when they reduce measurable stutter, memory, logging, or frame-time cost.
- P10-B must not change route claims: routes remain estimated prototype guidance, not official evacuation routes, and not fully road-geometry validated.
- Coordinate anchoring remains proxy/nearest-match unless future exact proof exists.

Machine-readable checklist:

- `Assets/Data/P10/p10a_p10b_readiness_checklist.json`

P10-A+ readiness update:

- P10-A+ adds full candidate anchor hardening, nearest-match proxy, entrance proxy, route proxy validation, semantic binding audit, and high-detail smoke status reports.
- P10-B should use those reports as benchmark and smoke-test inputs.
- P10-B still owns Windows EXE build, high-detail runtime stress, CPU/memory/FPS/stutter/loading/log monitoring, and before/after optimization measurements.
