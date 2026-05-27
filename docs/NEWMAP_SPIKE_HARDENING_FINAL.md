# NewMap Spike Hardening Final

Generated: 2026-05-27T19:21:33+09:00

Focus: startup/frame spike only. Memory values are recorded when measured but are not optimized in this task.

- Previous remaining-hardening input max frame: 6375.93 ms
- Target: under 2000 ms if possible
- Current max frame: 6298.42 ms
- Average FPS: 106.85
- Frames over 66 ms: 4
- Current decision: `spike_reduced_but_above_target`

Candidate recovery is precomputed into a small runtime Resources JSON, and recovered candidate green frames are lazy-built only when Evacuation Stage 2 starts. Route validation and candidate route proximity checks remain report/preflight work, not player startup work.
