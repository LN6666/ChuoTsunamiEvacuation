# NewMap Performance Hardening Final

JSON: `Assets/Data/P10/newmap_performance_hardening_final.json`

Implemented hardening:

- Scene MeshCollider shutdown is staged over frames after runtime collision support is active.
- Green frames and route guides remain hidden until Evacuation Stage 2.
- Light curtain remains hidden until Evacuation Stage 2.
- NPC cap remains 8.
- Intentional gameplay failure outcomes log as info instead of warnings.

Hardening performance sample:

- Duration: 180 seconds.
- Average FPS from Player.log probe: 46.2.
- Max frame from Player.log probe: 20,655.05 ms.
- Stutter frames over 66 ms: 2.
- Max private memory: 21,950,038,016 bytes.
- Max working set: 7,523,536,896 bytes.
- Player.log: 0 errors, 0 warnings.

The multi-second startup/frame spike and high private memory remain documented limitations. Ordinary-PC readiness is not claimed.
