# P7-C P2-P6 Compatibility Plan

## Goal

P7-C prepares validation that P2-P6 core gameplay can move from the old low-detail map to the new high-detail scene without changing gameplay success/failure rules.

## Compatibility Checks

| Area | Required check | Current P7-C status |
|---|---|---|
| Player spawn / movement | A player spawn and movement controller can exist in the new scene. | Prepared, runtime smoke pending |
| Camera | A camera setup can exist in the new scene. | Scene shell includes overview camera; gameplay camera smoke pending |
| GameManager | Runtime managers are not hard-bound to `Chuo_BaseMap.unity`. | Source evidence exists; runtime smoke pending |
| Shelter interaction | Shelter entrance targets can be represented in the new scene. | Placeholder target plan pending actual map objects |
| Result panel | Success/failure flow is not inherently tied to the old scene. | No P7-C rule changes; runtime smoke pending |
| P5 data loaders | P5 loaders can run without requiring the old scene. | Source evidence exists; build-safe data path remains a known follow-up |
| P6 navigation guidance | Guidance can target objects in the new scene. | Target-object staging pending |
| P6 NPC prototype | NPC prototype can be staged for smoke testing. | Prepared, no P9 crowd implementation |

## Rules

P7-C must not implement success/failure rule changes, P8 hazard systems, or P9 crowd/indoor evacuation systems.

If runtime validation is not complete in P7-C, P7-D must run a smoke test on the populated high-detail scene before declaring the scene as the P8/P9/P10 baseline.
