# P10-C-Pre Built Player Profiling Report

P10-C-Pre is a performance gate, not the final release. The temporary player profile is intended to answer whether ordinary-PC playability is realistic before P10-C release packaging begins.

## Metrics To Collect

- startup time and first playable/load time
- average FPS and 1 percent low FPS or best available stutter proxy
- min/avg/max frame time and frame spike count
- CPU usage proxy from frame time and Windows process samples
- working set and private memory
- managed heap, allocated memory, and GC deltas where Unity runtime metrics are available
- disk paging symptoms from Task Manager, Resource Monitor, or PowerShell observation
- Player.log warning/error counts
- NPC, marker, green frame, light curtain, ResultPanel, weather, and quality-profile state

## Current Evidence

Validated on 2026-05-25 JST:

- Temporary build: succeeded
- Profile script: succeeded for a 30-second process-level run
- Process samples: 29
- Max working set: 608.05 MB
- Max private memory: 1003.33 MB
- Player.log warnings/errors: 0 warnings, 0 errors
- Build Player.log data warning from the first attempt was fixed by copying `Assets/Data` into the temporary player data folder.

Limit: FPS, 1 percent low, min/avg/max frame time, Unity managed heap, GC deltas, NPC/marker/green-frame counts, light curtain state, and ResultPanel state were not captured from inside the built player. They still need runtime instrumentation or manual observation.

## Readiness Classifications

- `pass_for_p10c`: Low profile is playable near or above 30 FPS at 1080p, no fatal freeze, no error spam, no unbounded memory growth, and no paging-heavy stalls.
- `pass_with_limitations`: playable enough for P10-C with clear documented Low/Medium limits.
- `needs_quick_fix`: a bounded fix is likely sufficient before P10-C.
- `needs_major_optimization_before_release`: high-detail scene, memory, stutter, or paging behavior blocks release without a larger optimization plan.
- `blocked`: build/profile evidence is missing or the temporary player cannot run.

Routes remain prototype guidance and are not official route claims. Humanitarian candidates remain non-official unless separately verified.
