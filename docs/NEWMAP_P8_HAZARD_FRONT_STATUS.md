# NewMap P8 Hazard Front Status

P8 runtime behavior:

- Stage 1 `Warning`: light curtain hidden and risk contact ignored.
- Stage 2 `FrontApproaching`: light curtain visible and advancing.
- Hazard front does not immediately kill the player at game start.
- `visualHeight` behavior is cinematic only.

The old sample hazard grid is synthetic and has no verified transform into the reset map, so the runtime uses a local test hazard front/band near the spawn area.

Final status: `completed_with_documented_runtime_proxy`.
