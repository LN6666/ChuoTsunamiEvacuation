# NewMap Ground Raise Alignment Report

Generated: 2026-05-29T17:00:00+09:00

The practical fix makes the visible road-like gameplay cover and collision surface share the same Y reference.

- Ground cover top Y: `0.0`
- Player spawn Y: `0.04`
- NPC root Y: `0.04`
- Targets and green frames are aligned from the same cover/support Y.
- Runtime fall recovery returns to the validated cover/support height.

This is a gameplay alignment fix. It is not a claim that imported roads, DEM, terrain, or building bases are accurately aligned.
