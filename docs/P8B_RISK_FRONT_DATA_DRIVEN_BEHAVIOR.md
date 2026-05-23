# P8-B Risk Front Data-Driven Behavior

Date: 2026-05-24.

## Runtime Driver

The P8-B risk front is hazard-layer-driven v1. P8-B now uses Tokyo Metropolitan Government tsunami Open Data mesh records clipped to Chuo as an extracted official metropolitan spatial layer.

The runtime driver is `hazard_layer_arrival_depth_boundary_v1`:

- `arrivalTimeSeconds` selects the active feature and controls front progress.
- `inundationBoundary` is converted into the data boundary when available.
- `geometryType=grid` records the extracted mesh driver; point, polyline, polygon, and synthetic records remain supported for fallback/test inputs.
- missing or incomplete boundary data uses a procedural prototype fallback.
- `inundationDepthMeters` and `hazardIntensity` set visual warning intensity from extracted Chuo spatial depth values.
- `maxTsunamiHeightMeters` carries the source tsunami-height field and must not be treated as a spatial inundation-depth grid.
- `confidence`, `sourceMode`, `sourceCategory`, `evidenceSourceId`, and `extractionStatus` are surfaced in status/debug output.

## Visual Behavior

The light curtain is still cinematic:

- `visualHeightMeters` is visual-only.
- large visual height requires `visualHeightIsCinematicOnly=true`.
- mesh alpha/color intensity may increase for higher depth/intensity values.
- the visual boundary may be wavy or smoothed for readability.

## Safety

This is no full fluid simulation. It does not change gameplay success/failure rules. It does not implement P8-C infrastructure interaction. It does not implement P8-D collapse proxy gameplay.

The current layer is an official metropolitan extracted spatial source layer, but not a full real-time fluid simulation. The runtime summary must show official metropolitan source identified, spatial depth/boundary extracted, current risk front driver `official_spatial`, and that the cinematic curtain height is not physical tsunami height.
