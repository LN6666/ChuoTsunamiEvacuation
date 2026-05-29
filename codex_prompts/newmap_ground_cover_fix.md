# Codex Prompt: NewMap Gameplay Ground Cover Fix

Implement the user-approved practical ground fix for `Assets/Scenes/Chuo_BaseMap.unity`.

Use a visible neutral road-like gameplay ground cover over playable blue/fall-through gaps. The cover must be opaque, not blue, not debug-looking, and fully collidable. Player spawn, NPC placement, target anchors, green frames, and fall recovery must use the same cover/support height. Keep the failed adaptive relief/DEM grid disabled by default.

Do not re-import map data, do not rework lighting or mouse controls, do not make runtime web requests, do not create a final release/archive, and do not claim GIS-grade terrain or road accuracy. Building floating may remain documented as a visual limitation.

Required validation:
- `tools/map/run_newmap_ground_cover_preflight.ps1`
- EditMode and PlayMode Unity tests
- temporary player build at `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapGroundCoverPre\ChuoTsunamiEvacuation_NewMapGroundCoverPre.exe`
- Player.log parse with 0 errors and 0 warnings
- DeepSeek review with no A-level blockers
