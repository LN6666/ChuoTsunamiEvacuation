# NewMap Spawn-On-Road / Playable-Ground Fix

Generated: 2026-05-29T00:00:00+09:00

Scope: fix random player spawn on `Assets/Scenes/Chuo_BaseMap.unity` so the player is not placed inside building geometry. Lighting and night profiles were not changed.

Implementation:

- Runtime spawn config: `Assets/Data/P10/newmap_spawn_config.json`
- Known fallback points: `Assets/Data/P10/newmap_safe_spawn_points.json`
- Runtime report shell: `Assets/Data/P10/newmap_spawn_validation_report.json`
- Bootstrap code: `NewMapRuntimeBootstrap.ResolveSpawnPosition`

Validation behavior:

- The bootstrap builds a one-time cache of non-runtime renderer bounds that look like building geometry.
- Candidate spawn positions are snapped to the sampled visual/support surface height.
- Ground raycasts are accepted only when the hit is close to the support surface; otherwise the documented runtime support surface is used as the playable-ground proxy.
- Candidates are rejected if they overlap building renderer bounds in X/Z at player height.
- Candidates are rejected if the nearest building bound is closer than `minDistanceFromBuildingMeters`.
- Random retries use `maxSpawnAttempts`; if no random candidate passes, configured safe spawn points are tried as fallback.

Road semantics:

No official road geometry or road metadata is claimed in this fix. When road metadata is unavailable, the spawn validator uses a playable-ground/support proxy plus building-overlap rejection.

Latest temp player smoke:

- Spawn attempts: 3
- Rejected inside building: 2
- Accepted spawn: `(91.02, 2.01, 958.78)`
- Nearest building distance: 4.70m
- Ground/support height: 1.97m
- Fallback safe spawn used: false
- Player.log: 0 errors, 0 warnings

Status: validated by Unity tests and player smoke; manual visual confirmation remains.
