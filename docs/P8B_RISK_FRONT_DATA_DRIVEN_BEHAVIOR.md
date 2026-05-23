# P8-B Risk Front Data-Driven Behavior

Date: 2026-05-24.

## Runtime Driver

The P8-B risk front is hazard-layer-driven v1. P8-B now prioritizes Tokyo Metropolitan Government tsunami damage estimation sources for Chuo, while still marking spatial extraction as manual/pending.

The runtime driver is `hazard_layer_arrival_depth_boundary_v1`:

- `arrivalTimeSeconds` selects the active feature and controls front progress.
- `inundationBoundary` is converted into the data boundary when available.
- `geometryType` supports point, polyline, polygon, grid, and synthetic records.
- missing or incomplete boundary data uses a procedural prototype fallback.
- `inundationDepthMeters` and `hazardIntensity` set visual warning intensity. Current Chuo v1 depth values remain pending/placeholder unless marked otherwise.
- `maxTsunamiHeightMeters` carries Chuo maximum tsunami-height references around 2.4m to 2.46m and must not be treated as a spatial inundation-depth grid.
- `confidence`, `sourceMode`, and `evidenceSourceId` are surfaced in status/debug output.

## Visual Behavior

The light curtain is still cinematic:

- `visualHeightMeters` is visual-only.
- large visual height requires `visualHeightIsCinematicOnly=true`.
- mesh alpha/color intensity may increase for higher depth/intensity values.
- the visual boundary may be wavy or smoothed for readability.

## Safety

This is no full fluid simulation. It does not change gameplay success/failure rules. It does not implement P8-C infrastructure interaction. It does not implement P8-D collapse proxy gameplay.

The current layer is not a complete official Chuo spatial inundation surface. The runtime summary must show official metropolitan source known, spatial extraction pending, and current boundary/depth prototype/manual where applicable. Official or academic calculation remains source-dependent and must be reviewed before claims change.
