# NewMap Circular Boundary Superseded

Config: `Assets/Data/P10/newmap_circular_boundary_config.json`

The earlier larger circular boundary has been superseded by the active 2.27km runtime clamp.

- Center source: `original_map_center`
- Active radius: `2270` meters
- Boundary method: `runtime_circular_clamp`
- Player clamp: enabled
- NPC clamp: enabled
- Visible in normal mode: false
- Debug visible: false

The runtime clamp projects player and NPC positions back inside the circle instead of creating segmented wall colliders. This removes the old random rectangular air-wall behavior while still preventing exits from the playable map.

Player smoke expectations:
- Runtime center: `-2.14, 474.58`
- Active radius: `2270m`
- Player clamp: passed
- NPC clamp: passed
- Diagnostic colliders: 0
