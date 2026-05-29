# NewMap Player Building Collision

The player now receives the runtime building bounds cache from `NewMapRuntimeBootstrap`.

Collision approach:
- use lightweight renderer-bounds obstacle correction instead of full MeshColliders for every building
- preserve spawn rejection inside or too close to building bounds
- correct a proposed player position to the nearest outside point when it enters a building bound
- keep target interaction zones reachable

This is a gameplay collision proxy. It is not a GIS/building-solid simulation.

Validated player-smoke summary:
- building bounds cached: 12,742
- player building collision enabled: true
- spawn rejected inside building attempts: 2
- Player.log: 0 errors / 0 warnings
