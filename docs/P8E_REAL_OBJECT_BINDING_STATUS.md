# P8-E Real Object Binding Status

P8-E finds local PLATEAU metadata for roads/transport, buildings, bridges, and waterfront-adjacent data under `Assets/P7Benchmark/Imported/53393690/udx`.

What is complete before P9:

- P8 hazard categories have a documented binding mode: `plateau_metadata`, `proxy_marker`, or `data_only`.
- P8-C/P8-D can apply hazard/damage/blockage status to the categories without depending on `Chuo_BaseMap.unity`.
- Humanitarian candidates can carry marker and hazard-status readiness data.

What is not complete:

- Per-object Unity scene binding to exact rendered PLATEAU GameObjects is not proven.
- Underground and entrance semantics are not available as reliable real-object metadata.
- Route geometry is not road-geometry-validated against the new high-detail baseline.

P9 may assume P8 provides a usable hazard/front/infrastructure foundation. P9 must not assume official routes, official building safety, real entrance placement, or complete scene-semantic binding.
