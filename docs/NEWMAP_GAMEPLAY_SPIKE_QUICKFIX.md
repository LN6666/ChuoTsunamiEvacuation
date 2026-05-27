# NewMap Gameplay Spike Quickfix

Generated: 2026-05-27T20:14:37+09:00

Performance decision: `spike_remaining_but_documented`

- Previous remaining-hardening max frame: 6298.42 ms
- Current max frame: 6391.79
- Average FPS: 106.77

Fixes:
- Gameplay self-audit smoke is gated behind `-newmapSelfAuditSmoke`.
- Candidate and route validation stay in precomputed reports.
- Recovered candidate green frames are lazy-created only in Evacuation Stage 2.
- Runtime scene-wide bounds scan and MeshCollider shutdown remain disabled at startup.

Remaining limitation: Startup spike is still above the 2000 ms target unless the latest player retest proves otherwise.
