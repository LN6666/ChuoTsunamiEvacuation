# P8-B Evidence-Backed Front Model

Date: 2026-05-24.

## Model Position

P8-B answers "what drives the risk front" with an evidence-backed hazard-layer v1 model:

1. Load `tsunami_hazard_layer_v1_chuo.json`.
2. Validate source mode, evidence IDs, extraction status, science/visual separation, boundary kind, geometry type, and collapse-disabled state.
3. Prioritize Tokyo Metropolitan Government tsunami Open Data for Chuo.
4. Select the active hazard feature by `arrivalTimeSeconds`.
5. Convert `inundationBoundary` into a local data boundary.
6. Fall back to a procedural boundary only if a selected feature lacks usable geometry.
7. Use extracted `inundationDepthMeters` and `hazardIntensity` to compute visual warning intensity.
8. Surface `maxTsunamiHeightMeters`, `confidence`, `sourceMode`, `sourceCategory`, `evidenceSourceId`, and `extractionStatus` in status/debug output.
9. Render only the cinematic visual curtain; gameplay success/failure rules are unchanged.

## Evidence Boundaries

Current v1 records are official metropolitan extracted spatial records. The source data comes from Tokyo Open Data tsunami 10m mesh CSVs clipped to the official MLIT N03 Chuo City polygon.

Allowed claim:

- "The P8-B risk front is driven by hazard-layer v1 fields extracted from Tokyo Metropolitan Government tsunami Open Data for Chuo."
- "P8-B is `PASS` for P8-C spatial gate because official spatial mesh samples were extracted."
- "The current risk front driver is `official_spatial`."

Disallowed claim:

- "The P8-B front is a full real-time fluid simulation."
- "The P8-B front is an academic hydrodynamic model."
- "The derived bbox boundary is an official inundation contour."
- "The cinematic curtain height is real tsunami height."
- "`maxTsunamiHeightMeters` is the same as a spatial inundation-depth grid."
- "`visualHeightMeters` is a scientific inundation-depth, water-level, or tsunami-height field."

## Driver Fields

| Field | Runtime Use | Claim Safety |
|---|---|---|
| `arrivalTimeSeconds` | Chooses upcoming/latest feature and computes arrival progress. | Extracted from Tokyo arrival-time CSVs using earliest valid 30cm arrival. |
| `inundationBoundary` | Supplies data geometry for the visual boundary. | Derived grid-extent bbox from extracted Chuo points, not an official inundation contour. |
| `inundationDepthMeters` | Raises visual warning intensity. | Extracted spatial maximum inundation depth from clipped official grid points. |
| `maxInundationDepthMeters` | Reports scenario maximum depth explicitly. | Same extracted depth statistic as `inundationDepthMeters` in v1. |
| `maxTsunamiHeightMeters` | Reports source maximum tsunami-height field. | Separate from inundation depth and not used as the depth grid. |
| `hazardIntensity` | Raises visual warning intensity. | Normalized from extracted maximum depth for v1 visualization. |
| `confidence` | Reported in status for transparency. | Reflects official source plus automated clipping limits. |
| `evidenceSourceId` | Links each feature to evidence metadata. | Required for evidence-backed records. |
| `sourceMode` | Indicates `official_tsunami_metropolitan`. | Valid only with reviewed source metadata and extracted spatial status. |
| `extractionStatus` | Gates P8-C readiness. | `PASS` requires `extracted`. |

## P8-C Relationship

P8-C may use the extracted feature records, arrival times, derived extents, depth/intensity values, affected infrastructure type flags, and provenance fields when adding infrastructure interactions. P8-C must not implement P8-D collapse proxy behavior, P9 systems, or gameplay success/failure changes during P8-C.
