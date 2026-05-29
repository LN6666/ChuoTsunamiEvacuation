# NewMap Safe Playable Ground Recovery

The recovery prioritizes stable gameplay over terrain accuracy.

- Runtime ground strategy: one invisible collision support surface.
- Support Y: `0.0`.
- Support collider count: `1`.
- Support renderer count: `0`.
- Adaptive relief grid: disabled by default.
- Player spawn: validated against playable bounds and building X/Z overlap.
- Air walls: preserved as invisible boundary colliders.

The support surface is a gameplay safety surface. It is not a GIS-grade road/terrain reconstruction.
