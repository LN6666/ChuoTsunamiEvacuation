# NewMap Building Collision Precision Fix

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

## Implementation

`NewMapRuntimeBootstrap` now loads `Assets/Data/P10/newmap_building_collision_precision_config.json` and builds tighter player/NPC building obstacle bounds:

- `collisionMode`: `tight_footprint_box_proxies`
- `shrinkFactorXZ`: `0.90`
- `maxColliderWidthMeters`: `80.0`
- `maxColliderDepthMeters`: `80.0`
- `colliderHeightMeters`: `5.0`
- `disableClusterRootColliders`: `true`
- `targetInteractionClearanceMeters`: `14.0`

The old broad runtime building obstacle bounds are not kept when the precision path accepts or rejects a building renderer. Oversized cluster/root bounds are skipped and counted as disabled/skipped inflated bounds.

Overlapping tight proxies are split around active shelter/candidate interaction zones so markers remain approachable without removing building collision across the whole footprint.

## Preserved Behavior

- Buildings still block player movement through footprint proxies.
- NPC building avoidance still uses the same tightened bounds.
- Ground/support, NPC body or soft-blocking, and the 3.5km circular boundary remain valid blockers.
- Route lines, green frames, labels, markers, and hazard visuals remain nonblocking.

## Validation

The temporary player smoke logs:

- inflated bounds found/skipped
- tight proxies created
- mesh-footprint vs renderer-fallback counts
- maximum final proxy width/depth
- target clearance zones and split/removed bounds
- sampled corridor count
- active target approach blocked count

Status is pending until the player log parser runs after the temporary build.
