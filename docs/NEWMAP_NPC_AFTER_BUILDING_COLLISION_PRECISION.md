# NewMap NPC After Building Collision Precision

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

## Preserved NPC Behavior

NPCs still receive building avoidance bounds, but those bounds are now the same tightened footprint proxies used by player building blocking. This avoids broad inflated root bounds making NPCs stop on roads near buildings.

Checks:

- NPC building avoidance remains enabled.
- NPCs remain inside the 3.5km circular boundary.
- NPC body/soft-blocking remains enabled.
- NPC lifecycle does not regress into global refresh or all-stop behavior.

Runtime status is populated from the temporary player smoke.
