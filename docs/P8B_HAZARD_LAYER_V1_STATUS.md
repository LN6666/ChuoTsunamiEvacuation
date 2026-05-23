# P8-B Hazard Layer V1 Status

Date: 2026-05-24.

## Current Data Status

`Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json` is the P8-B evidence-aware hazard-layer v1 input. The older `tsunami_hazard_sample_chuo.json` remains only as a legacy P8-A compatibility sample.

Current P8-B status:

- `sourceMode`: `evidence_planned`
- `hazardLayerVersion`: `p8b.1.1`
- `evidenceRegistryFile`: `tsunami_hazard_evidence_registry.json`
- `p8cGateDecision`: `CONDITIONAL PASS`
- `officialMetropolitanEvidenceIdentified`: `true`
- `completeOfficialSpatialLayerExtracted`: `false`
- Spatial extraction status: `manual_extraction_required`

This is no longer treated as a generic manual sample. It is an evidence-aware layer that prioritizes Tokyo Metropolitan Government tsunami damage estimation sources for Chuo. It still must not be described as a complete official Chuo inundation-depth raster, polygon layer, or final tsunami simulation.

## Evidence Position

Chuo City does not appear to provide a standalone tsunami hazard map equivalent to its flood hazard maps. That is not evidence absence.

The primary evidence candidate is Tokyo Metropolitan Government damage estimation material:

- The Tokyo damage estimation digital map is the primary candidate for tsunami layers such as maximum tsunami height and maximum inundation depth.
- The Tokyo 2022 damage estimation report page lists tsunami height and tsunami inundation distribution sections.
- Chuo City references Tokyo's damage estimation report as the source for tsunami numerical simulation results.
- Supplementary public PDF references report Chuo maximum tsunami height around 2.4m to 2.46m, depending on source/scenario.

Maximum tsunami height references around 2.4m to 2.46m are evidence references, but they are not a full spatial inundation-depth grid by themselves.

## What Drives The Risk Front

The P8-B risk front uses hazard-layer feature fields where available:

- `arrivalTimeSeconds` selects and advances the active front record.
- `inundationBoundary` supplies a data boundary or prototype boundary.
- `inundationDepthMeters` contributes to visual warning intensity only when a spatial depth value is actually available; current v1 Chuo values are marked pending/placeholder.
- `hazardIntensity` contributes to visual warning intensity as a prototype normalized signal.
- `maxTsunamiHeightMeters` stores reported municipal maximum-height references and must not be confused with a depth grid.
- `confidence` is surfaced in debug/status output.
- `evidenceSourceId` and `sourceMode` identify data provenance.
- `geometryType` controls whether the boundary is point, polyline, polygon, grid, or synthetic input.
- `boundaryIsEvidenceBasedOrPrototype` prevents prototype geometry from being mistaken for reviewed evidence.

`visualHeightMeters` remains cinematic-only and is not a physical tsunami height, water level, or inundation depth.

## Pending Evidence Review

Before any complete official spatial-layer claim, P8 needs reviewed extraction records for:

- source authority, license, and dataset/API availability,
- scenario selection and source date/version,
- coordinate reference system,
- Chuo municipality filter and waterfront extent,
- arrival time derivation,
- depth/water-level/tsunami-height definitions,
- polygon/polyline/grid extraction method,
- uncertainty and confidence notes.

Until extraction is complete, P8-B may proceed as official metropolitan evidence-planned / manual-extraction-required. The runtime must show that official metropolitan source is known, spatial extraction is pending, and the current boundary/depth is prototype/manual.

## P8-C Use

P8-C may consume this hazard-layer shape under `CONDITIONAL PASS`: Tokyo metropolitan tsunami evidence is identified, but spatial extraction is manual/pending. P8-C can use prototype geometry with explicit labels and must not assume a complete official hazard surface.
