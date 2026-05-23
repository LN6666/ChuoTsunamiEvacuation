# P8-B Hazard Layer V1 Status

Date: 2026-05-24.

## Current Data Status

`Assets/Data/P8/tsunami_hazard_sample_chuo.json` is now the P8-B hazard-layer v1 manual sample.

Current status:

- `sourceMode`: `manual_sample`
- `hazardLayerVersion`: `p8b.1.0`
- `evidenceSourceId`: `p8b_manual_hazard_layer_v1_placeholder`
- `reviewedStatus`: `reviewed_for_planning`
- Official or academic inundation values: not present

This layer is evidence-shaped and validator-backed, but it is not an official inundation map, not an academic simulation output, and not a final Chuo tsunami model.

## What Drives The Risk Front

The P8-B risk front uses hazard-layer feature fields where available:

- `arrivalTimeSeconds` selects and advances the active front record.
- `inundationBoundary` supplies the data boundary or prototype boundary.
- `inundationDepthMeters` contributes to visual warning intensity.
- `hazardIntensity` contributes to visual warning intensity.
- `confidence` is surfaced in debug/status output.
- `evidenceSourceId` and `sourceMode` identify data provenance.
- `geometryType` controls whether the boundary is treated as point, polyline, polygon, grid, or synthetic input.
- `boundaryIsEvidenceBasedOrPrototype` prevents prototype geometry from being mistaken for reviewed evidence.

`visualHeightMeters` remains cinematic-only and is not a physical tsunami height, water level, or inundation depth.

## Pending Evidence Review

Before any official/academic claim, P8 needs reviewed source records for:

- source authority and license,
- hazard map or simulation date/version,
- coordinate reference system,
- arrival time derivation,
- depth/water-level/tsunami-height definitions,
- polygon/polyline/grid extraction method,
- confidence and uncertainty notes.

Until then, the P8-B front is hazard-layer-driven v1 using manual-sample records.

## P8-C Use

P8-C may consume this hazard-layer shape to decide where road/building/bridge/underground interaction rules should attach. P8-C must still treat the current values as manual sample/prototype values unless reviewed evidence replaces them.
