# P7-C Known Limitations

## Raw CityGML

Candidate `53393690` is still raw CityGML plus texture source files in the P7Benchmark sandbox.

P7-C does not convert CityGML to Unity meshes and does not verify production renderability.

## No Production Streaming

P7-C implements a metadata-driven chunk registry and placeholder enable/disable controller.

It does not implement full production Chuo streaming, async loading, Addressables, world partitioning, or full-scene paging.

## No Final Performance Claim

The metrics harness records approximate telemetry only.

Unity Profiler, Frame Debugger, Memory Profiler, and Windows EXE profiling remain required before performance claims are made.

## No LOD4 Assumption

LOD3 remains a benchmark candidate only. LOD4 is not assumed available based on current evidence.

## No Collision Integration

P7-C does not add production collision. Raw high-detail geometry must not become complex MeshCollider geometry by default.

## No Project-Wide Visual Setting Changes

P7-C does not change anti-aliasing, shadows, lighting, render pipeline, ProjectSettings, Packages, or texture import policy.

## Editor Builder Rebuild Hygiene

The P7-C editor builder is intended for occasional sandbox regeneration. If it becomes a frequent workflow, generated placeholder materials should be reused or cleaned up to avoid scene clutter across repeated rebuilds.

## No Gameplay Effect

The P7Benchmark registry, controller, scene objects, and metrics recorder do not change:

- success rules
- failure rules
- shelter rules
- tsunami risk behavior
- player spawn
- crowd behavior
- must not implement or affect indoor evacuation

## No P8 Or P9 Work

P7-C must not implement P8 tsunami hazard, inundation, flood, light curtain, or risk-front systems.

P7-C must not implement P9 crowd, real spawn, indoor evacuation, congestion, indoor shelter gameplay, or failure systems.
