# P8-B Chuo Tsunami Extraction Plan

Date: 2026-05-24.

## Goal

Convert the identified Tokyo Metropolitan Government tsunami evidence into a project-ready Chuo hazard layer without overclaiming.

## Extraction Steps

1. Confirm the Tokyo damage estimation map layer names for maximum tsunami height and maximum inundation depth.
2. Confirm whether a downloadable dataset/API exists for Chuo municipality selection and the relevant scenarios.
3. If no direct dataset/API is available, perform manual GIS/web-map extraction with a recorded method:
   - selected municipality: Chuo City,
   - scenario name,
   - layer name,
   - date/version,
   - coordinate reference system,
   - screenshots or exported geometries,
   - reviewer and review date.
4. Separate extracted fields:
   - `maxTsunamiHeightMeters` for municipal height reference,
   - `inundationDepthMeters` only for spatial depth values,
   - `inundationBoundary` only for extracted or explicitly prototype boundaries.

Rule: inundationDepthMeters only for spatial depth values; maximum tsunami-height references stay in `maxTsunamiHeightMeters`.
5. Mark `boundaryIsEvidenceBasedOrPrototype=evidence_based` only after extraction is reviewed.
6. Keep `visualHeightMeters` cinematic-only and separate from physical tsunami fields.

## Current V1 State

`Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json` is `evidence_planned` with `spatialExtractionStatus=manual_extraction_required`.

Current boundaries are prototype/manual. Current depth values are placeholder/pending unless the feature states otherwise. The layer carries maximum tsunami-height references around 2.4m to 2.46m, but those references are not a full spatial inundation-depth grid.

## Upgrade Criteria

The layer can move from `CONDITIONAL PASS` to `PASS` only when an actual spatial hazard layer is extracted and reviewed:

- valid source and scenario are recorded,
- Chuo extent is documented,
- geometry is extracted into project coordinates,
- depth values are spatial, not copied from municipal maximum-height references,
- evidence notes state what was extracted and what remains uncertain.

Until then, P8-C may use prototype geometry only with explicit labels.
