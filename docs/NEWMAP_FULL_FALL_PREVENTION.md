# NewMap Full Fall Prevention

Generated: 2026-05-29T17:00:00+09:00

Fall prevention now has three layers:
- Visible road-like ground cover tiles have enabled colliders.
- Four invisible air walls still bound the playable area.
- Player fall/out-of-bounds recovery returns to the last validated cover/support position.

NPCs use the same cover/support Y for placement and are clamped inside playable bounds. Spawn validation still rejects building overlap and out-of-bounds candidates.

Player smoke must still confirm `Player.log` has 0 errors and 0 warnings before this is treated as manual-test ready.
