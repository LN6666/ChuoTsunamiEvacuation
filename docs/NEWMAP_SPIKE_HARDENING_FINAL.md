# NewMap Spike Hardening Final

Generated: 2026-05-27T18:21:16+09:00

Focus: startup/frame spike only. Memory values are recorded when measured but are not optimized in this task.

- Before hardening max frame: 20655.05 ms
- Previous after-hardening max frame: 11273.06 ms
- Target: under 2000 ms if possible
- Final max frame: 6375.93 ms
- Final average FPS: 106.26
- Frames over 66 ms: 2
- Player.log: 0 errors / 0 warnings
- Runtime bootstrap timing: `boundsMs=1 spawnSupportMs=16 systemsMs=104 targetsMs=101 configureMs=2 totalMs=224`

Decision: `spike_reduced_but_above_target`

Additional non-memory hardening in this pass defers Stage 2 light curtain/debris creation and keeps route validation/reporting out of player startup. The remaining spike is documented as large Chuo_BaseMap Unity/PLATEAU scene activation/render startup before regular gameplay.
