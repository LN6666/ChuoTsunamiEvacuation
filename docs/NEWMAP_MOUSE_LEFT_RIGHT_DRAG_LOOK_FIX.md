# NewMap Left/Right Mouse Drag Look Fix

Generated: 2026-05-29T00:00:00+09:00

Manual feedback: right mouse drag worked, but left mouse drag should also rotate the camera.

Implementation:

- `Assets/Data/P10/newmap_mouse_drag_look_config.json` now includes `allowedButtons: ["LeftMouse", "RightMouse"]`.
- `NewMapPlayerController` checks all configured allowed buttons.
- Mouse movement alone still does not rotate the camera.
- Start menu, pause, rules, and result states disable gameplay control and keep the cursor usable.
- Tourism Mode and Evacuation Mode use the same drag-look gate.

Expected behavior:

- Left mouse button held + drag rotates yaw/pitch.
- Right mouse button held + drag rotates yaw/pitch.
- Releasing the button stops look and unlocks/shows the cursor.
- UI states do not rotate the gameplay camera.

Latest validation:

- EditMode: passed 114/114
- PlayMode: passed 27/27
- Player smoke: `mouse_left_right_drag_look` passed
- Player.log: 0 errors, 0 warnings

Status: validated by automated tests and player smoke; manual physical feel confirmation remains.
