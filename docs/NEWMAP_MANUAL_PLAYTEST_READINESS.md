# NewMap Manual Playtest Readiness

Generated: 2026-05-27T18:21:16+09:00

Decision: `ready_with_documented_non_memory_limitations`

Reason: player/camera/UI/interaction flows are runnable, Player.log is clean, DeepSeek found no A-level blocker, and the remaining limitations are documented non-memory issues.

Active official shelters: 15
Active non-official/training targets: 4
Disabled targets: 156
Coordinate transform: `transform_validated_from_official_anchors`

Route geometry: 60 old route geometries validated as estimated prototype evidence; old route overlays are not spawned at runtime and no official route is claimed.

Performance: max frame reduced from 11273.06 ms to 6375.93 ms, still above the 2000 ms target. Average FPS was 106.26 with 2 frames over 66 ms.

Player.log: 0 errors / 0 warnings.

DeepSeek: no A-level blocker.
