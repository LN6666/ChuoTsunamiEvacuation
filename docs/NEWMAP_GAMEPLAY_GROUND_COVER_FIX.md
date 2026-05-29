# NewMap Gameplay Ground Cover Fix

Generated: 2026-05-29T17:00:00+09:00

Approved approach implemented:
- `GameplayGroundCoverRoot` is created by the runtime bootstrap.
- The cover is visible in normal gameplay and uses a neutral road-like material.
- Cover tiles use enabled `BoxCollider` geometry.
- The old relief/DEM adaptive support grid remains disabled.
- This does not claim real terrain or road elevation accuracy.

Expected default cover:
- Tile size: `320m`
- Expected documented-bounds tile count: `135`
- Cover top Y: `0.0`
- Thickness: `0.18m`
- Material: `P10_NewMap_RoadGroundCover`

Remaining limitation:
Building base/facade visual offsets may remain. Player/NPC fall prevention and stable walking are prioritized over PLATEAU visual alignment.
