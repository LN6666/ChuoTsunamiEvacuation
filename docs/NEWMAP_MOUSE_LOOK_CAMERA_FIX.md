# NewMap Mouse Look Camera Fix

Generated: 2026-05-29T00:00:00+09:00

Status: `implemented_pending_manual_mouse_test`

Fixes:

- Gameplay now reads mouse X/Y continuously; RMB is no longer required.
- Gameplay mode locks and hides the cursor.
- Start menu, pause, result, and safe-floor sequence disable look and unlock the cursor.
- Resume, Tourism Mode, and Evacuation Mode restore camera control.
- Sensitivity remains serialized on `NewMapPlayerController`.
- Pitch remains clamped from `-15` to `60` degrees.

JSON: `Assets/Data/P10/newmap_mouse_look_camera_status.json`
