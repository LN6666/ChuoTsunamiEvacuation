# P8-B Evidence-Backed Front Model

Date: 2026-05-24.

## Model Position

P8-B answers "what drives the risk front" with an evidence-aware hazard-layer v1 model:

1. Load `tsunami_hazard_layer_v1_chuo.json`.
2. Validate source mode, evidence IDs, science/visual separation, boundary kind, geometry type, and collapse-disabled state.
3. Prioritize Tokyo Metropolitan Government tsunami damage estimation evidence for Chuo.
4. Select the active hazard feature by `arrivalTimeSeconds`.
5. Convert `inundationBoundary` into a local data boundary when present.
6. Fall back to a procedural prototype boundary only when the selected feature boundary is incomplete.
7. Use `inundationDepthMeters` and `hazardIntensity` to compute a visual warning intensity, while treating pending spatial depth as prototype/manual.
8. Surface `maxTsunamiHeightMeters`, `confidence`, `sourceMode`, and `evidenceSourceId` in status/debug output.
9. Render only the cinematic visual curtain; gameplay success/failure rules are unchanged.

## Evidence Boundaries

Current v1 records are evidence-planned records backed by official metropolitan source candidates. They are not a complete official Chuo spatial inundation layer.

Allowed claim:

- "The P8-B risk front is driven by hazard-layer v1 fields, with Tokyo Metropolitan Government tsunami damage estimation as the primary Chuo evidence candidate."
- "P8-B is `CONDITIONAL PASS` for P8-C: official metropolitan evidence is identified, but spatial extraction remains pending/manual."

Disallowed claim:

- "The P8-B front is a complete official Chuo inundation-depth raster or polygon layer."
- "The P8-B front is an academic tsunami simulation."
- "The cinematic curtain height is real tsunami height."
- "The current prototype boundary/depth values are final official hazard values."
- "`maxTsunamiHeightMeters` is the same as a spatial inundation-depth grid."
- "`visualHeightMeters` is a scientific inundation-depth, water-level, or tsunami-height field."

## Driver Fields

| Field | Runtime Use | Claim Safety |
|---|---|---|
| `arrivalTimeSeconds` | Chooses upcoming/latest feature and computes arrival progress. | May use reviewed report references when present. |
| `inundationBoundary` | Supplies data/prototype geometry for the visual boundary. | Prototype until extracted from Tokyo source layers. |
| `inundationDepthMeters` | Raises visual warning intensity. | Current Chuo v1 depth values are pending/placeholder unless extraction status changes. |
| `maxTsunamiHeightMeters` | Stores reported municipal maximum-height references such as around 2.4m to 2.46m. | Height reference only, not a spatial depth grid. |
| `hazardIntensity` | Raises visual warning intensity. | Prototype normalized signal in current v1. |
| `confidence` | Reported in status for transparency. | Reflects source/extraction maturity. |
| `evidenceSourceId` | Links each feature to evidence metadata. | Required for evidence-aware records. |
| `sourceMode` | Indicates `evidence_planned` or reviewed official metropolitan source state. | Does not imply complete official spatial extraction by itself. |

## P8-C Relationship

P8-C should use the same feature boundaries, arrival times, affected infrastructure type flags, and provenance fields when adding infrastructure interactions. P8-C must keep labels explicit if it uses prototype geometry before spatial extraction is complete.
