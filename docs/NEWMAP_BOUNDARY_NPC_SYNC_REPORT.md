# NewMap Boundary NPC Sync Report

Boundary/NPC synchronization target:

- Circular boundary radius: `2270m`
- NPC distribution radius: `2270m`
- NPC distribution center: same as circular boundary center.
- Player clamp: `2270m`
- NPC movement clamp: `2270m`
- Spawn validation: inside active playable boundary.
- Active targets/routes: disabled or suppressed outside active playable boundary.

The preflight fails if any active runtime boundary or NPC distribution value reverts to `1500m`, `3500m`, or the old `1000m` NPC distribution radius.
