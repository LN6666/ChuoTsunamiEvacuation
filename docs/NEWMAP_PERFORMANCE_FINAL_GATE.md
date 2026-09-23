# NewMap Performance Final Gate

JSON: `Assets/Data/P10/newmap_performance_final_gate.json`

Decision: `ready_with_memory_limitations`

The rebuilt final temporary player sample reported:

- Average FPS: `125.19`
- Max frame: `6120.26` ms
- Stutter frames over 66 ms: `2`
- Max private memory: `21948108800` bytes
- Max working set: `7527911424` bytes

Final hardening disables scene MeshColliders after spawn raycast, uses a documented runtime collision proxy, keeps NPC count capped, and stages green frames/route guides/light curtain. The map is still high-memory and must not be claimed ordinary-PC ready.
