# P9-B Entrance Safe-Floor Runtime Prototype

P9-B adds runtime marker support for the replacement vertical evacuation flow:

Player reaches shelter or high-rise candidate entrance -> entrance interaction -> safe-floor / vertical evacuation status -> evacuation-complete proxy.

Implemented script:

- `P9EntranceSafeFloorMarkerRuntime`

Data file:

- `Assets/Data/P9/p9b_entrance_marker_assignments_sample.json`

Supported marker states:

- entrance marker
- safe-floor label
- vertical evacuation status
- evacuation-complete proxy state placeholder
- official shelter proxy compatibility
- non-official humanitarian candidate warning

P9-B does not implement final outcome mutation. Blocked, warning, and evacuation-complete states are prototype marker/status data only. P9-C owns final success/failure behavior.
