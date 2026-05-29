# NewMap Circular Boundary 3.5km

Config: `Assets/Data/P10/newmap_circular_boundary_config.json`

- Center source: `original_map_center`
- Radius: `3500` meters
- Boundary method: `runtime_circular_clamp`
- Player clamp: enabled
- NPC clamp: enabled
- Visible in normal mode: false
- Debug visible: false

The runtime clamp projects player and NPC positions back inside the circle instead of creating segmented wall colliders. This removes the old random rectangular air-wall behavior while still preventing exits from the playable map.

The invisible support surface is expanded to cover the circular boundary so fall prevention remains intact at the edge.

Player smoke result:
- Runtime center: `-2.14, 474.58`
- Radius: `3500m`
- Player clamp: passed
- NPC clamp: passed
- Diagnostic colliders: 0
