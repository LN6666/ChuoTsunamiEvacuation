# P10-B Optimization Pass

Implemented low-risk optimization:

- green ground frames use pooled GameObjects
- frame line positions are configured only when targets are generated/refreshed
- target lists are supplied or built from P9-D reports instead of searched every frame
- debug labels are disabled by default
- frame count is capped
- performance metrics expose green frame count for stress comparisons

Deferred optimization candidates for P10-C/manual profiling:

- compare light curtain on/off cost
- measure UI/ResultPanel layout cost during long warning display
- measure NPC and marker caps against high-detail scene
- throttle noisy logs if Player.log warning count is high
- identify Update-loop hot spots from Profiler evidence
