# NewMap Spawn Final Safety

- Config: `Assets/Data/P10/newmap_spawn_final_safety_config.json`
- Runtime config: `Assets/Data/P10/newmap_spawn_config.json`
- Report: `Assets/Data/P10/newmap_spawn_final_safety_report.json`

Random spawn validation is hardened for the final manual tuning pass:

- Up to `500` random attempts
- Minimum `4m` from building bounds/proxies
- Building proxy cache and renderer bounds cache are checked
- Ground cover/support hit is required
- Player capsule settings: radius `0.35m`, height `1.8m`
- Final capsule-style overlap check is enabled before accepting a spawn
- Spawn is rejected inside the playable boundary margin / circular boundary margin of `3m`
- Fallback safe spawn remains enabled as `newmap_safe_spawn_01`

The player should never start inside a building proxy or renderer footprint. Runtime repeated-spawn results are written by the final tuning player-log parser.
