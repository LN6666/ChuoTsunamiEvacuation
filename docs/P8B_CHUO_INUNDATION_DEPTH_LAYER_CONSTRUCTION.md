# P8-B Chuo Inundation Depth Layer Construction

Date: 2026-05-24.

## Construction Summary

The P8-B Chuo inundation-depth layer is built from Tokyo Open Data tsunami 10m mesh CSVs clipped to the MLIT N03 Chuo City polygon. The official mesh points are clipped to Chuo before scenario statistics are written.

The layer is not generated from a generic flood proxy and does not treat Chuo City's lack of a standalone tsunami map as evidence absence.

## Processing Steps

1. Load MLIT N03 Tokyo administrative boundary GeoJSON from the official ZIP.
2. Select the Chuo City polygon by administrative attributes.
3. Load Tokyo official tsunami depth, height, and arrival-time CSVs for each scenario.
4. Clip mesh points by longitude/latitude point-in-polygon against Chuo.
5. Join records by source mesh coordinates.
6. Compute scenario summary fields:
   - `spatialSampleCount`
   - `inundationDepthMeters`
   - `maxInundationDepthMeters`
   - `averageInundationDepthMeters`
   - `maxTsunamiHeightMeters`
   - `arrivalTimeSeconds`
7. Store clipped `spatialSamples` in `Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json`.

## Boundary Handling

The current `inundationBoundary` is a derived bbox around the extracted Chuo mesh samples. It is evidence-based as an extracted grid extent, but it is not an official inundation contour.

P8-C can use the derived boundary as a conservative data extent for infrastructure interaction design. It must not label the bbox as a Tokyo-issued inundation polygon.

## Field Rules

- `maxTsunamiHeightMeters` is a tsunami-height field and is not used as the inundation-depth grid.
- `inundationDepthMeters` comes from the maximum inundation-depth CSV and represents the extracted scenario maximum over Chuo samples.
- `arrivalTimeSeconds` uses earliest valid 30cm arrival in the clipped grid.
- `hazardIntensity` is normalized from extracted maximum depth for P8-B warning visualization.
- `visualHeightMeters` remains cinematic only.

## Output

The output layer is `official_tsunami_metropolitan`, `extractionStatus=extracted`, and `p8cGateDecision=PASS`.

This is sufficient for P8-C to start infrastructure hazard interaction, while keeping the model provenance explicit and avoiding full fluid simulation claims.
