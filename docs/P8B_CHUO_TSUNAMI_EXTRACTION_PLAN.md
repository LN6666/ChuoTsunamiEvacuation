# P8-B Chuo Tsunami Extraction Plan

Date: 2026-05-24.

## Goal

Convert Tokyo Metropolitan Government tsunami evidence into a project-ready Chuo hazard layer without overclaiming.

## Completed Extraction

The extraction path did not require API tokens, user registration, login, G-Spatial tokens, real-estate-library API keys, or browser-only manual downloads.

Used sources:

- Tokyo Open Data ward-area tsunami distribution CSVs for maximum inundation depth, maximum tsunami height, and arrival time.
- MLIT National Land Numerical Information N03 administrative boundary ZIP for the Chuo City polygon.

The extractor is `tools/p8/extract_p8b_chuo_tsunami_layer.py`.

Construction method:

1. Download Tokyo official CP932/Shift-JIS CSVs in memory.
2. Download official MLIT N03 Tokyo boundary ZIP in memory.
3. Read the N03 GeoJSON and select `N03_004=中央区`.
4. Clip Tokyo tsunami mesh points to the Chuo polygon.
5. Join depth, height, and arrival-time records by source mesh coordinates.
6. Write `Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json`.

## Field Separation

- `inundationDepthMeters` stores extracted spatial maximum inundation depth from the clipped grid.
- `maxInundationDepthMeters` stores the same scenario maximum for explicit status/reporting.
- `maxTsunamiHeightMeters` stores the extracted maximum tsunami-height field and is not used as the inundation-depth grid.
- `arrivalTimeSeconds` uses earliest valid 30cm arrival time for the scenario feature.
- `inundationBoundary` is a derived grid-extent bbox from clipped points; it is evidence-based as an extracted extent, but it is not an official inundation contour.
- `boundaryIsEvidenceBasedOrPrototype=evidence_based` is set only because extraction status is `extracted`.
- `visualHeightMeters` remains cinematic-only and separate from physical tsunami fields.

## Current V1 State

`Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json` is `official_tsunami_metropolitan` with `spatialExtractionStatus=extracted` and `p8cGateDecision=PASS`.

Extracted scenarios:

- Taisho Kanto earthquake: 1123 Chuo grid samples; maximum spatial inundation depth 2.0436m; maximum tsunami height 2.1287m.
- Nankai Trough megathrust earthquake case 1: 1256 Chuo grid samples; maximum spatial inundation depth 2.2629m; maximum tsunami height 2.4223m.

## Remaining Caveats

This is not a full fluid simulation. It is an official spatial source extraction into P8-B's runtime schema. The derived bbox boundary supports a data-driven cinematic risk front but is not an official inundation contour polygon.
