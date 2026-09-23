# P9-B Crowd Runtime Prototype

P9-B upgrades the P9-A scaffold into a lightweight runtime crowd prototype.

Implemented scripts:

- `P9CrowdRuntimeAgent`
- `P9CrowdRuntimeSpawner`
- `P9CrowdRuntimeMetrics`

Data file:

- `Assets/Data/P9/p9b_runtime_crowd_scenario_sample.json`

Supported behavior:

- capped proxy NPC spawning
- deterministic spawn selection
- deterministic target assignment to entrance/safe-floor proxies
- simple movement stepping
- reached count
- average estimated evacuation time
- target selection counts
- congestion hotspot proxy id

Forbidden P9-B behavior is not implemented:

- no thousands-agent simulation
- no heavy crowd package
- no social psychology model
- no NPC-caused player failure
- no direct player success/failure mutation
