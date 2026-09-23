# P9-A Foundation Summary

P9-A creates the scaffold for crowd/spawn/entrance-safe-floor proxy work.

Implemented foundation:

- P9 four-stage plan and boundaries.
- P8 handoff contract with missing-data fallback.
- Spawn point schema and sample data.
- Crowd/NPC agent profile schema and sample data.
- Entrance/safe-floor vertical evacuation proxy schema and sample data.
- Runtime-neutral P9 loaders and proxy components.
- Congestion and evacuation proxy state model.
- Metrics/logging plan for future stages.
- EditMode and PlayMode tests for sample loading and temporary-scene components.
- P9-A preflight and JSON validation tools.

P9-A is scaffold only. It does not implement final NPC-caused failure, final congestion failure, final vertical evacuation failure, real indoor scene gameplay, final P8-D/E damage/collapse integration, or P10 release packaging.

P9-B will integrate with the new map after P8 handoff is stable enough. P9-C will implement final failure/congestion/vertical evacuation proxy gameplay.
