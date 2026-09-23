# P8-B Hazard Layer V1 Status

Date: 2026-05-24.

## Current Data Status

`Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json` is now the P8-B official metropolitan spatial hazard-layer v1 input. The older `tsunami_hazard_sample_chuo.json` remains only as a legacy P8-A compatibility sample.

Current P8-B status:

- `sourceMode`: `official_tsunami_metropolitan`
- `sourceCategory`: `official_tsunami_metropolitan`
- `hazardLayerVersion`: `p8b.1.2`
- `evidenceRegistryFile`: `tsunami_hazard_evidence_registry.json`
- `p8cGateDecision`: `PASS`
- `officialMetropolitanEvidenceIdentified`: `true`
- `completeOfficialSpatialLayerExtracted`: `true`
- Spatial extraction status: `extracted`

Tokyo Metropolitan Government Open Data tsunami 10m mesh CSVs were accessible through direct HTTPS download with no API token, login, or manual browser-only download. The mesh points were clipped to the official MLIT N03 Chuo City administrative polygon.

## Evidence Position

Chuo City does not appear to provide a standalone tsunami hazard map equivalent to its flood hazard maps. That is not evidence absence.

The primary P8-B source is Tokyo Metropolitan Government tsunami damage estimation data:

- Maximum inundation depth CSVs provide spatial `inundationDepthMeters` values.
- Maximum tsunami height CSVs provide spatial `maxTsunamiHeightMeters` / `tsunamiHeightMeters` values.
- Arrival-time CSVs provide arrival time values; P8-B uses earliest valid 30cm arrival as `arrivalTimeSeconds`.
- Chuo City references Tokyo's 2022 damage estimation report for tsunami numerical simulation results.
- Supplementary public PDF values around 2.4m to 2.46m remain references, not the source used as the depth grid.

The current extracted layer includes two Tokyo scenarios:

- Taisho Kanto earthquake: 1123 clipped Chuo mesh samples, maximum spatial inundation depth 2.0436m, maximum tsunami height 2.1287m.
- Nankai Trough megathrust earthquake case 1: 1256 clipped Chuo mesh samples, maximum spatial inundation depth 2.2629m, maximum tsunami height 2.4223m.

Nankai Trough case 5 and case 8 are not present in the current generated Chuo layer or extractor scenario list. They must not be claimed as extracted until official Tokyo CSV inputs are added and clipped.

`maxTsunamiHeightMeters` is not the same as a full `inundationDepthMeters` grid. The layer stores both fields separately.

## What Drives The Risk Front

The P8-B risk front uses hazard-layer feature fields:

- `arrivalTimeSeconds` selects and advances the active front record.
- `inundationBoundary` supplies a data boundary derived from the clipped grid extent.
- `inundationDepthMeters` and `maxInundationDepthMeters` drive warning intensity.
- `hazardIntensity` contributes to warning intensity as a normalized signal.
- `maxTsunamiHeightMeters` is reported for provenance/status and must not be used as a substitute for inundation depth.
- `confidence`, `evidenceSourceId`, `sourceMode`, `sourceCategory`, and `extractionStatus` are surfaced in status output.
- `geometryType=grid` records that the driver comes from extracted mesh samples.
- `boundaryIsEvidenceBasedOrPrototype=evidence_based` is allowed because the feature records are extracted, but the stored boundary is a derived grid extent bbox, not an official inundation contour.

`visualHeightMeters` remains cinematic-only and is not a physical tsunami height, water level, or inundation depth.

## Remaining Limits

This is not a full real-time fluid simulation, not an academic hydrodynamic model, and not a gameplay success/failure rule change. The derived boundary is not an official inundation contour; it is the bounding extent of clipped official grid points. P8-C can use the extracted layer for infrastructure interaction design, but it must preserve provenance labels and avoid treating visual curtain height as science.
