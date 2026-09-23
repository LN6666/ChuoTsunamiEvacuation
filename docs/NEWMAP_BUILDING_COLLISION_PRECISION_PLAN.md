# NewMap Building Collision Precision Plan

Date: 2026-05-30

## Goal

Keep the collision whitelist, but replace broad building obstacle bounds with tighter approximate footprint proxies so roads, sidewalks, target approaches, and open areas are not blocked by inflated building-adjacent invisible walls.

Allowed movement blockers remain:
- ground/support
- tight building footprint proxies
- NPC body/soft-blocking
- 2.27km circular boundary

Everything else remains visual or trigger-only.

## Implementation

1. Add `newmap_building_collision_precision_config.json`.
2. Load a new `NewMapBuildingCollisionPrecisionConfig` in `NewMapRuntimeBootstrap`.
3. Replace the old renderer-bounds building obstacle generation with tight footprint bounds:
   - use mesh projected vertices when available;
   - fall back to renderer bounds only for small building-like renderers;
   - shrink X/Z around the projected center with configurable `shrinkFactorXZ`;
   - cap width/depth at `80m`;
   - cap height to `5m`;
   - use the gameplay ground/support Y where available.
4. Skip or disable inflated cluster/root bounds instead of retaining them as blockers.
5. Add runtime diagnostics for:
   - original building candidates
   - inflated bounds found/skipped
   - tight proxies created
   - max final X/Z size
   - average shrink ratio
   - sampled approach/corridor blocker counts
6. Keep player and NPC building collision enabled and fed by the same tightened proxy list.
7. Add focused EditMode/PlayMode tests and preflight tools.
8. Build only the requested temporary EXE:
   `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapBuildingCollisionPrecisionPre\ChuoTsunamiEvacuation_NewMapBuildingCollisionPrecisionPre.exe`

## Non-Goals

- No GIS-grade footprint claim.
- No map data reimport.
- No lighting, mouse, P11, P10-E/F/G, or final release/archive work.
- No removal of building collision.
