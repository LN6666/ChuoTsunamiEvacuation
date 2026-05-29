# NewMap GroundRoadMerge Failure Analysis

GroundRoadMergePre is treated as a failed gameplay fix.

- Failed build: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapGroundRoadMergePre\ChuoTsunamiEvacuation_NewMapGroundRoadMergePre.exe`
- Faulty runtime path: relief/DEM-derived adaptive support grid.
- Source validation problem: supplemental source had `0` road/transport renderers and `20` relief renderers.
- Result: relief samples were not a safe road surface source and caused unsafe player/building/support height behavior.

Actions:
- Adaptive support grid is disabled by default.
- Relief/DEM source samples are not used for runtime player/NPC/target support heights.
- Source-sample building alignment fallback is disabled.
- A conservative invisible gameplay support surface is restored.

This rollback does not claim road/terrain accuracy or a full floating-building visual fix.
