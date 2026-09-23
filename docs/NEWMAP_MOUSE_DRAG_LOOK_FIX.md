# NewMap Mouse Drag Look Fix

Generated: 2026-05-29T00:00:00+09:00

Status: `validated_by_tests_and_player_smoke_physical_manual_drag_confirmation_remaining`

Round 2 changes the camera contract:

- Mouse movement alone does not rotate the camera.
- Holding the configured mouse button and dragging rotates yaw/pitch.
- Default button: Right Mouse Button.
- Pitch clamp: -60 to 70 degrees.
- Cursor remains visible/unlocked when not dragging, and in menu/rules/pause/result states.
- Tourism Mode and Evacuation Mode both enable drag-look availability.

Config: `Assets/Data/P10/newmap_mouse_drag_look_config.json`

Status JSON: `Assets/Data/P10/newmap_mouse_drag_look_status.json`

Validation:

- EditMode passed 114/114.
- PlayMode passed 26/26.
- Player smoke passed `mouse_drag_look_requires_button`.
