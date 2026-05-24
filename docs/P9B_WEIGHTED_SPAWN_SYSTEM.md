# P9-B Weighted Spawn System

P9-B implements deterministic weighted spawn selection in `P9WeightedSpawnSelector`.

Data files:

- `Assets/Data/P9/p9b_weighted_spawn_config.json`
- `Assets/Data/P9/p9b_spawn_zones_sample.json`

The selector biases spawn probability toward:

- coastal and waterfront zones
- low-elevation zones
- river and canal-adjacent zones
- high inundation exposure zones
- underground or metro entrance proxy zones
- office/commercial dense activity zones

The selector excludes or downweights:

- explicitly blocked zones
- already flooded zones when configured
- invalid positions
- unsafe zero positions unless debug allowance is explicitly enabled

Supported runtime fields include deterministic seed, scenario preset id, spawn count cap, spawn category, candidate weight explanation, exclusion handling, and debug/export-friendly summaries.

This is estimated prototype guidance. It is not official pedestrian origin data and it does not mutate player success/failure.
