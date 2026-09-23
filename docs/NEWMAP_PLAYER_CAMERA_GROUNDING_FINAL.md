# NewMap Player Camera Grounding Final

JSON: Assets/Data/P10/newmap_player_camera_grounding_final.json

Final status: completed_with_documented_runtime_proxy

The runtime creates NewMap_Player, a visible humanoid marker, and a Main Camera on Chuo_BaseMap. Spawn uses map bounds and raycast evidence, then the final manual-test path relies on a documented local runtime collision support proxy. Scene MeshColliders are disabled after spawn raycast to reduce physics load during manual testing.

Required movement and mode rules are covered by NewMapRuntimePlayModeTests, including a 60-second diagnostic movement simulation, camera existence, fall recovery count 0 in the normal scenario, tourism speed 2.0/10.0 with stamina disabled, evacuation speed 1.0/5.0 with stamina enabled, and weather modifiers 1.0/0.75/0.85/0.65.
