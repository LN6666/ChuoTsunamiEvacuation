# DeepSeek Review Prompt: NewMap Spawn-On-Road + Left/Right Mouse Drag Fix

Review the current git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

Task scope:

- Fix random player spawn so it does not place the player inside buildings.
- Spawn must be on road/playable ground/support surface with ground/support probing and building-overlap rejection.
- Add fallback safe spawn points without claiming official road geometry.
- Update camera drag look so both left mouse drag and right mouse drag rotate view.
- Mouse movement alone must not rotate the camera.
- Do not rework lighting/night, create release/archive, create P10-E/F/G, move to P11, reimport map data, or break P2-P10 gameplay.

Please verify:

- The spawn-inside-building bug is addressed with validation, not just a random offset.
- Spawn selection rejects building renderer/collider bounds overlap and too-close building positions.
- Ground/support alignment is preserved and fallback support is documented.
- Known safe spawn points exist and are used only as fallback/validated proxy points.
- Left and right mouse drag both rotate the camera.
- Mouse movement alone does not rotate.
- UI/menu/pause/result states do not accidentally rotate gameplay camera.
- Tourism and Evacuation modes both retain camera/movement/gameplay flow.
- Lighting/night config was not reworked or regressed.
- No final release/archive was created.
- No P10-E/F/G work was created.
- No A-level blockers remain.

Return a verdict with A/B/C findings. Treat compile errors, missing validation, spawn still able to overlap buildings, broken mouse drag gating, Player.log errors, or false official road/route claims as A-level blockers.
